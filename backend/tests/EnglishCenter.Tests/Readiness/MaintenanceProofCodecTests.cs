using System.Security.Cryptography;
using System.Text;
using EnglishCenter.Api.Infrastructure.Maintenance;
using Xunit;

namespace EnglishCenter.Tests.Readiness;

public sealed class MaintenanceProofCodecTests
{
    private static byte[] Request() => MaintenanceProofCodec.CreateRequest(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(32));
    private static byte[] DomainPayload(byte[] request, byte flag) => Encoding.UTF8.GetBytes("EnglishCenter.M0.Maintenance.Readiness.v1\0")
        .Concat(request).Append(flag).ToArray();

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void IndependentRsaVerifierAcceptsDomainBoundReadyAndRefusal(bool ready)
    {
        using var privateKey = RSA.Create(2048);
        using var publicKey = RSA.Create(); publicKey.ImportParameters(privateKey.ExportParameters(false));
        var request = Request(); var response = MaintenanceProofCodec.CreateResponse(request, ready, privateKey);
        Assert.Equal(325, response.Length);
        Assert.True(publicKey.VerifyData(DomainPayload(request, ready ? (byte)1 : (byte)0), response.AsSpan(69),
            HashAlgorithmName.SHA256, RSASignaturePadding.Pss));
        Assert.True(MaintenanceProofCodec.VerifyResponse(request, response, publicKey, out var actual));
        Assert.Equal(ready, actual);
    }

    [Fact]
    public void IndependentSignedResponseRequiresCorrectDomainAndPss()
    {
        using var key = RSA.Create(2048); var request = Request();
        byte[] Make(byte[] payload, RSASignaturePadding padding) => request.Append((byte)1)
            .Concat(key.SignData(payload, HashAlgorithmName.SHA256, padding)).ToArray();
        Assert.True(MaintenanceProofCodec.VerifyResponse(request, Make(DomainPayload(request, 1), RSASignaturePadding.Pss), key, out var ready));
        Assert.True(ready);
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, Make(request.Append((byte)1).ToArray(), RSASignaturePadding.Pss), key, out ready));
        Assert.False(ready);
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, Make(DomainPayload(request, 1), RSASignaturePadding.Pkcs1), key, out ready));
        Assert.False(ready);
    }

    [Theory]
    [InlineData(0)] [InlineData(4)] [InlineData(35)] [InlineData(36)]
    [InlineData(67)] [InlineData(68)] [InlineData(69)] [InlineData(324)]
    public void EveryEnvelopeFieldTamperFailsClosed(int index)
    {
        using var key = RSA.Create(2048); var request = Request(); var response = MaintenanceProofCodec.CreateResponse(request, true, key);
        response[index] ^= 1;
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, response, key, out var ready)); Assert.False(ready);
    }

    [Fact]
    public void AnotherNonceProfileSignerOrSizeCannotReplayProof()
    {
        using var key = RSA.Create(2048); using var wrong = RSA.Create(2048); using var large = RSA.Create(3072);
        var request = Request(); var response = MaintenanceProofCodec.CreateResponse(request, true, key);
        Assert.False(MaintenanceProofCodec.VerifyResponse(Request(), response, key, out var ready)); Assert.False(ready);
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, response, wrong, out ready)); Assert.False(ready);
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, response, large, out ready)); Assert.False(ready);
        Assert.Throws<ArgumentException>(() => MaintenanceProofCodec.CreateResponse(request, true, large));
    }

    [Fact]
    public void MalformedFramesRejectPartialOutputsAndUnsupportedFlag()
    {
        using var key = RSA.Create(2048); var request = Request();
        foreach(var length in new[] {0, 4, 67, 69})
        {
            Assert.False(MaintenanceProofCodec.TryReadRequest(new byte[length], out var nonce, out var hash));
            Assert.Empty(nonce); Assert.Empty(hash);
        }
        Assert.Throws<ArgumentException>(() => MaintenanceProofCodec.CreateRequest(new byte[31], new byte[32]));
        var invalid = MaintenanceProofCodec.CreateResponse(request, true, key); invalid[68] = 2;
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, invalid, key, out var ready)); Assert.False(ready);
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, invalid[..324], key, out ready)); Assert.False(ready);
        Assert.False(MaintenanceProofCodec.VerifyResponse(request, invalid.Concat(new byte[1]).ToArray(), key, out ready)); Assert.False(ready);
    }

    [Fact]
    public void ProviderArgumentFailureCannotLeakDetails()
    {
        using var key = new FailingSigner();
        var exception = Assert.Throws<ArgumentException>(() => MaintenanceProofCodec.CreateResponse(Request(), true, key));
        Assert.DoesNotContain("PRIVATE_SIGNER_DETAIL", exception.Message, StringComparison.Ordinal);
        Assert.Null(exception.InnerException);
    }
    private sealed class FailingSigner : RSA
    {
        public FailingSigner() { KeySizeValue = 2048; LegalKeySizesValue = [new KeySizes(2048, 2048, 0)]; }
        public override byte[] SignHash(byte[] hash, HashAlgorithmName algorithm, RSASignaturePadding padding) => throw new ArgumentException("PRIVATE_SIGNER_DETAIL");
        public override RSAParameters ExportParameters(bool includePrivateParameters) => throw new NotSupportedException();
        public override void ImportParameters(RSAParameters parameters) => throw new NotSupportedException();
    }
}
