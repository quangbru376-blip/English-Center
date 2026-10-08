using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace EnglishCenter.Api.Infrastructure.Readiness;

public sealed class ProtectedCsrfReadinessProbe : IRuntimeReadinessProbe
{
    private readonly string reference;
    private readonly string directory;
    private readonly int maximumFiles;
    private readonly int maximumFileBytes;
    private readonly TimeProvider clock;
    public ProtectedCsrfReadinessProbe(string reference, string directory, int maximumFiles,
        int maximumFileBytes, TimeProvider clock)
    {
        this.reference = reference; this.directory = directory; this.maximumFiles = maximumFiles;
        this.maximumFileBytes = maximumFileBytes; this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }
    public ReadinessDependency Dependency => ReadinessDependency.ProtectedCsrfKeys;
    public async Task<ReadinessProbeStatus> CheckAsync(RuntimeProfile profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (RuntimeProfileValidator.Validate(profile) != ProfileIssue.None || string.IsNullOrWhiteSpace(reference) ||
            profile.Auth!.ProtectedPersistedCsrfStoreReference != reference ||
            maximumFiles <= 0 || maximumFileBytes <= 0) return ReadinessProbeStatus.NotReady;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(profile.Execution!.ReadinessMs);
        try
        {
            var pending = Task.Run(() => CheckStore(deadline.Token), deadline.Token);
            ProbeWait.Observe(pending);
            var result = await pending.WaitAsync(deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return result ? ReadinessProbeStatus.Pass : ReadinessProbeStatus.NotReady;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception)
        { cancellationToken.ThrowIfCancellationRequested(); return ReadinessProbeStatus.NotReady; }
    }
    private bool CheckStore(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (LocalReadinessFiles.Resolve(directory, ".") is null) return false;
        var elements = new List<XElement>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.xml", SearchOption.TopDirectoryOnly))
        {
            token.ThrowIfCancellationRequested();
            if (elements.Count >= maximumFiles || LocalReadinessFiles.Resolve(directory, Path.GetFileName(path)) is null) return false;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length is < 1 || stream.Length > maximumFileBytes) return false;
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = maximumFileBytes });
            elements.Add(XElement.Load(reader));
        }
        if (elements.Count == 0 || elements.Any(element => element.Name != "key" && element.Name != "revocation")) return false;
        var keyElements = elements.Where(element => element.Name == "key").ToArray();
        if (keyElements.Length == 0 || keyElements.Any(element =>
            element.Descendants(XName.Get("encryptedSecret", "http://schemas.asp.net/2015/03/dataProtection")).Count() != 1 || element.Descendants("masterKey").Any() ||
            (string?)element.Descendants(XName.Get("encryptedSecret", "http://schemas.asp.net/2015/03/dataProtection")).Single().Attribute("decryptorType") != typeof(DpapiXmlDecryptor).AssemblyQualifiedName)) return false;
        var ids = keyElements.Select(element => Guid.Parse((string?)element.Attribute("id") ?? "")).ToArray();
        if (ids.Distinct().Count() != ids.Length) return false;
        // Snapshot is read-only. Key management cannot generate/store a key through this repository.
        var services = new ServiceCollection();
        services.AddDataProtection().DisableAutomaticKeyGeneration();
        services.Configure<KeyManagementOptions>(options => options.XmlRepository = new ReadOnlyKeys(elements));
        using var provider = services.BuildServiceProvider();
        var keys = provider.GetRequiredService<IKeyManager>().GetAllKeys();
        var now = clock.GetUtcNow();
        var active = keys.Where(key => !key.IsRevoked && key.ActivationDate <= now && now < key.ExpirationDate).ToArray();
        if (active.Length == 0 || keys.Count != ids.Length) return false;
        var challenge = RandomNumberGenerator.GetBytes(32);
        var purpose = "EnglishCenter.Readiness.CsrfKeyCheck.v1"u8.ToArray();
        try
        {
            foreach (var key in keys.Where(key => !key.IsRevoked))
            {
                token.ThrowIfCancellationRequested();
                var encryptor = key.CreateEncryptor();
                if (encryptor is null) return false;
                var encrypted = encryptor.Encrypt(new ArraySegment<byte>(challenge), new ArraySegment<byte>(purpose));
                var decrypted = encryptor.Decrypt(new ArraySegment<byte>(encrypted), new ArraySegment<byte>(purpose));
                try { if (!CryptographicOperations.FixedTimeEquals(challenge, decrypted)) return false; }
                finally { CryptographicOperations.ZeroMemory(decrypted); }
            }
            token.ThrowIfCancellationRequested();
            return true;
        }
        finally { CryptographicOperations.ZeroMemory(challenge); }
    }
    private sealed class ReadOnlyKeys(List<XElement> elements) : IXmlRepository
    {
        public IReadOnlyCollection<XElement> GetAllElements() => elements.Select(element => new XElement(element)).ToArray();
        public void StoreElement(XElement element, string? friendlyName) => throw new InvalidOperationException("Read-only readiness store");
    }
}
