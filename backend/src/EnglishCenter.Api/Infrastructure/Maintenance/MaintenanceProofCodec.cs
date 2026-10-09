using System;
using System.Security.Cryptography;
using System.Text;

namespace EnglishCenter.Api.Infrastructure.Maintenance;

public static class MaintenanceProofCodec
{
    private const int RequestLength = 68, NonceLength = 32, ProfileHashLength = 32, MagicLength = 4;
    private const int FlagLength = 1, HeaderLength = 69, SignatureLength = 256, ResponseLength = 325;
    private const int ExpectedRsaKeySize = 2048;

    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("ECM1");
    private static readonly byte[] DomainPrefix = Encoding.UTF8.GetBytes("EnglishCenter.M0.Maintenance.Readiness.v1\0");

    public static byte[] CreateRequest(ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> profileHash)
    {
        if (nonce.Length != NonceLength || profileHash.Length != ProfileHashLength)
        {
            throw new ArgumentException("Invalid request input parameters.");
        }

        byte[] request = new byte[RequestLength];
        Magic.AsSpan().CopyTo(request.AsSpan(0, MagicLength));
        nonce.CopyTo(request.AsSpan(MagicLength, NonceLength));
        profileHash.CopyTo(request.AsSpan(MagicLength + NonceLength, ProfileHashLength));
        return request;
    }

    public static bool TryReadRequest(ReadOnlySpan<byte> request, out byte[] nonce, out byte[] profileHash)
    {
        nonce = Array.Empty<byte>();
        profileHash = Array.Empty<byte>();
        if (request.Length != RequestLength || !request.Slice(0, MagicLength).SequenceEqual(Magic))
        {
            return false;
        }

        nonce = request.Slice(MagicLength, NonceLength).ToArray();
        profileHash = request.Slice(MagicLength + NonceLength, ProfileHashLength).ToArray();
        return true;
    }

    public static byte[] CreateResponse(ReadOnlySpan<byte> request, bool ready, RSA signer)
    {
        try
        {
            if (signer == null || signer.KeySize != ExpectedRsaKeySize || !TryReadRequest(request, out _, out _))
            {
                throw new ArgumentException("Invalid response generation argument.");
            }

            byte[] response = new byte[ResponseLength];
            request.CopyTo(response.AsSpan(0, RequestLength));
            response[RequestLength] = ready ? (byte)1 : (byte)0;

            byte[] payload = new byte[DomainPrefix.Length + HeaderLength];
            DomainPrefix.AsSpan().CopyTo(payload.AsSpan(0, DomainPrefix.Length));
            response.AsSpan(0, HeaderLength).CopyTo(payload.AsSpan(DomainPrefix.Length, HeaderLength));

            byte[] signature = signer.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            if (signature.Length != SignatureLength)
            {
                throw new ArgumentException("Invalid signature length.");
            }

            signature.CopyTo(response.AsSpan(HeaderLength, SignatureLength));
            return response;
        }
        catch (Exception)
        {
            throw new ArgumentException("Failed to create maintenance response.");
        }
    }

    public static bool VerifyResponse(ReadOnlySpan<byte> request, ReadOnlySpan<byte> response, RSA verifier, out bool ready)
    {
        ready = false;
        try
        {
            if (verifier == null || verifier.KeySize != ExpectedRsaKeySize || !TryReadRequest(request, out _, out _))
            {
                return false;
            }
            if (response.Length != ResponseLength || !CryptographicOperations.FixedTimeEquals(request, response.Slice(0, RequestLength)))
            {
                return false;
            }

            byte flag = response[RequestLength];
            if (flag != 0 && flag != 1)
            {
                return false;
            }

            byte[] payload = new byte[DomainPrefix.Length + HeaderLength];
            DomainPrefix.AsSpan().CopyTo(payload.AsSpan(0, DomainPrefix.Length));
            response.Slice(0, HeaderLength).CopyTo(payload.AsSpan(DomainPrefix.Length, HeaderLength));

            ReadOnlySpan<byte> signature = response.Slice(HeaderLength, SignatureLength);
            if (!verifier.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss))
            {
                return false;
            }

            ready = flag == 1;
            return true;
        }
        catch (Exception)
        {
            ready = false;
            return false;
        }
    }
}
