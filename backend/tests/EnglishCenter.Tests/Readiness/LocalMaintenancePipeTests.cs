using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Principal;
using EnglishCenter.Api.Infrastructure.Maintenance;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class LocalMaintenancePipeTests
{
    [Fact]
    public async Task NativePipeRoundTripReusesOwnedInstanceAndRejectsSecondInstance()
    {
        if (!OperatingSystem.IsWindows()) return;
        using var identity = WindowsIdentity.GetCurrent(); var sid = identity.User!.Value;
        var name = "EnglishCenter.M0.Maint." + Guid.NewGuid().ToString("N");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken); deadline.CancelAfter(3000);
        using var server = LocalMaintenancePipe.CreateServer(name, sid, sid);
        var collision = Assert.Throws<Win32Exception>(() => LocalMaintenancePipe.CreateServer(name, sid, sid));
        Assert.Contains(collision.NativeErrorCode, new[] { 5, 231 }); // Access denied or exhausted one-instance quota.
        using var key = RSA.Create(2048);
        var requests = new[] { MaintenanceProofCodec.CreateRequest(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(32)),
            MaintenanceProofCodec.CreateRequest(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(32)) };
        var serving = Task.Run(async () =>
        {
            foreach (var request in requests)
            {
                await server.WaitForConnectionAsync(deadline.Token);
                var input = new byte[68]; await server.ReadExactlyAsync(input, deadline.Token);
                Assert.Equal(sid, LocalMaintenancePipe.ReadClientSid(server)); Assert.Equal(request, input);
                await server.WriteAsync(MaintenanceProofCodec.CreateResponse(input, true, key), deadline.Token);
                var acknowledgment = new byte[1]; await server.ReadExactlyAsync(acknowledgment, deadline.Token);
                Assert.Equal(0xAC, acknowledgment[0]); server.Disconnect();
            }
        }, deadline.Token);
        foreach (var request in requests)
        {
            using var client = await LocalMaintenancePipe.ConnectAsync(name, deadline.Token);
            await client.WriteAsync(request, deadline.Token);
            var response = new byte[325]; var reading = client.ReadExactlyAsync(response, deadline.Token).AsTask();
            await Task.WhenAny(serving, reading); if (serving.IsFaulted) await serving; await reading;
            Assert.True(MaintenanceProofCodec.VerifyResponse(request, response, key, out var ready)); Assert.True(ready);
            await client.WriteAsync(new byte[] { 0xAC }, deadline.Token);
            Assert.Equal(0, await client.ReadAsync(new byte[1], deadline.Token));
        }
        await serving;
    }
    [Fact]
    public async Task MissingServerHonorsFiniteCancellation()
    {
        if (!OperatingSystem.IsWindows()) return;
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken); deadline.CancelAfter(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => LocalMaintenancePipe.ConnectAsync("EnglishCenter.M0.Maint." + Guid.NewGuid().ToString("N"), deadline.Token));
    }
    [Theory]
    [InlineData(@"\\remote\pipe\other")] [InlineData("EnglishCenter.M0.Maint.invalid")]
    public void NonOwnedOrRemoteNamesCannotBeOpened(string name)
    {
        if (!OperatingSystem.IsWindows()) return;
        using var identity = WindowsIdentity.GetCurrent();
        var sid = identity.User!.Value;
        Assert.Throws<ArgumentException>(() => LocalMaintenancePipe.CreateServer(name, sid, sid));
    }
}
