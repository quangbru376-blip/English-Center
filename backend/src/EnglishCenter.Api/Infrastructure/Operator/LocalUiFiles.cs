using System.Security.Cryptography;
using EnglishCenter.Api.Common.Errors;
using EnglishCenter.Api.Infrastructure.Readiness;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace EnglishCenter.Api.Infrastructure.Operator;

public sealed class LocalUiFiles : IFileProvider
{
    private readonly string root;
    private readonly Dictionary<string, Pin> pins;
    private sealed record Pin(long Length, byte[] Hash);
    public LocalUiFiles(string root)
    {
        this.root = root;
        pins = Walk().ToDictionary(name => name, name =>
        {
            using var stream = OpenSource(name); return new Pin(stream.Length, SHA256.HashData(stream));
        }, StringComparer.Ordinal);
        if (!pins.ContainsKey("index.html")) throw new InvalidOperationException("Missing UI entry");
    }
    public bool IsCurrent()
    {
        try
        {
            if (!Walk().Order(StringComparer.Ordinal).SequenceEqual(pins.Keys.Order(StringComparer.Ordinal))) return false;
            foreach (var name in pins.Keys) using (OpenPinned(name)) { }
            return true;
        }
        catch (Exception) { return false; }
    }
    public IFileInfo GetFileInfo(string subpath)
    {
        var name = subpath.TrimStart('/');
        return pins.TryGetValue(name, out var pin) ? new PinnedFile(this, name, pin.Length) : new NotFoundFileInfo(subpath);
    }
    public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;
    public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    private IEnumerable<string> Walk()
    {
        var result = new List<string>(); var directories = 0; Visit(root, 0); return result;
        void Visit(string directory, int depth)
        {
            if (++directories > 128 || depth > 4 || (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("Invalid UI source");
            foreach (var path in Directory.EnumerateFileSystemEntries(directory))
            {
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("Invalid UI entry");
                if ((attributes & FileAttributes.Directory) != 0) Visit(path, depth + 1);
                else
                {
                    var extension = Path.GetExtension(path);
                    if (extension is not (".html" or ".js" or ".css" or ".svg" or ".ico" or ".png" or ".jpg" or ".jpeg" or ".webp" or ".woff" or ".woff2"))
                        throw new InvalidOperationException("Unsupported UI artifact");
                    if (result.Count >= 128) throw new InvalidOperationException("Invalid UI file count");
                    result.Add(Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'));
                }
            }
        }
    }
    private FileStream OpenSource(string name)
    {
        var path = LocalReadinessFiles.Resolve(root, name) ?? throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length is < 1 or > 5000000) { stream.Dispose(); throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE); }
        return stream;
    }
    private FileStream OpenPinned(string name)
    {
        var stream = OpenSource(name);
        try
        {
            var pin = pins[name];
            if (stream.Length != pin.Length || !CryptographicOperations.FixedTimeEquals(SHA256.HashData(stream), pin.Hash))
                throw new ApiErrorException(ApiErrorCode.SERVICE_UNAVAILABLE);
            stream.Position = 0; return stream;
        }
        catch { stream.Dispose(); throw; }
    }
    private sealed class PinnedFile(LocalUiFiles files, string name, long length) : IFileInfo
    {
        public bool Exists => true;
        public long Length => length;
        public string? PhysicalPath => null;
        public string Name => Path.GetFileName(name);
        public DateTimeOffset LastModified => DateTimeOffset.UnixEpoch;
        public bool IsDirectory => false;
        public Stream CreateReadStream() => files.OpenPinned(name);
    }
}
