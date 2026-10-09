using System.ComponentModel;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace EnglishCenter.Api.Infrastructure.Maintenance;

public static class LocalMaintenancePipe
{
    private const uint Overlapped = 0x40000000, FirstInstance = 0x00080000, RejectRemote = 8;
    private const uint ClientAccess = 0x0012009B; // Specific read/write rights exclude FILE_CREATE_PIPE_INSTANCE (4).
    private const uint IdentificationOnly = 0x00110000;
    public static NamedPipeServerStream CreateServer(string name, string maintenanceSid, string runtimeSid)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        ValidateName(name);
        var maint = new SecurityIdentifier(maintenanceSid).Value;
        var runtime = new SecurityIdentifier(runtimeSid).Value;
        var descriptor = $"O:{maint}G:{maint}D:P(A;;FA;;;{maint})(A;;FA;;;SY)(A;;FA;;;BA)(A;;0x0012019B;;;{runtime})";
        if (!ConvertSecurityDescriptor(descriptor, 1, out var security, out _)) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            var attributes = new SecurityAttributes { Length = Marshal.SizeOf<SecurityAttributes>(), Descriptor = security };
            var handle = CreateNamedPipe(@"\\.\pipe\" + name, 3 | Overlapped | FirstInstance, RejectRemote, 1, 325, 68, 5000, ref attributes);
            if (handle.IsInvalid) { var error = Marshal.GetLastWin32Error(); handle.Dispose(); throw new Win32Exception(error); }
            try { return new NamedPipeServerStream(PipeDirection.InOut, true, false, handle); }
            catch { handle.Dispose(); throw; }
        }
        finally { _ = LocalFree(security); }
    }
    public static async Task<NamedPipeClientStream> ConnectAsync(string name, CancellationToken token)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        ValidateName(name);
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var handle = CreateFile(@"\\.\pipe\" + name, ClientAccess, 0, IntPtr.Zero, 3, Overlapped | IdentificationOnly, IntPtr.Zero);
            if (!handle.IsInvalid)
            {
                try { return new NamedPipeClientStream(PipeDirection.InOut, true, true, handle); }
                catch { handle.Dispose(); throw; }
            }
            var error = Marshal.GetLastWin32Error(); handle.Dispose();
            if (error is not (2 or 231)) throw new Win32Exception(error);
            await Task.Delay(25, token);
        }
    }
    public static string? ReadClientSid(NamedPipeServerStream pipe)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        string? sid = null;
        pipe.RunAsClient(() =>
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            using var peer = WindowsIdentity.GetCurrent(ifImpersonating: true);
            if (peer?.ImpersonationLevel == TokenImpersonationLevel.Identification) sid = peer.User?.Value;
        });
        return sid;
    }
    private static void ValidateName(string name)
    {
        const string prefix = "EnglishCenter.M0.Maint.";
        if (!name.StartsWith(prefix, StringComparison.Ordinal) || !Guid.TryParseExact(name[prefix.Length..], "N", out _))
            throw new ArgumentException("Invalid maintenance pipe reference");
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityAttributes { public int Length; public IntPtr Descriptor; public int InheritHandle; }
    [DllImport("advapi32.dll", EntryPoint = "ConvertStringSecurityDescriptorToSecurityDescriptorW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ConvertSecurityDescriptor(string descriptor, uint revision, out IntPtr security, out uint length);
    [DllImport("kernel32.dll", EntryPoint = "CreateNamedPipeW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafePipeHandle CreateNamedPipe(string name, uint openMode, uint pipeMode, uint maxInstances,
        uint outBuffer, uint inBuffer, uint timeout, ref SecurityAttributes attributes);
    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafePipeHandle CreateFile(string name, uint access, uint sharing, IntPtr security, uint creation, uint flags, IntPtr template);
    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);
}
