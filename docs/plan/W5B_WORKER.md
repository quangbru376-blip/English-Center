# W5B worker — pure maintenance readiness proof framing

Implement exactly one new C# file:
backend/src/EnglishCenter.Api/Infrastructure/Maintenance/MaintenanceProofCodec.cs
namespace EnglishCenter.Api.Infrastructure.Maintenance, public static class MaintenanceProofCodec.
At most130source lines; no tests/other writes/commands/network/process/Windows/SQL/files.
Before writing read this entire brief, .agents/skills/backend-clean-code/SKILL.md,
and docs/plan/SKILL_ROUTING.md. These three files + sole own output are the ONLY read
allowlist. Do not follow links/read AGENTS/source/README/search/list directories.
No actual key/profile/identity/config exists in provided context. Caller owns keys.
No permission to generate/store/export keys or sign application JWTs.
Canonical overrides JWT cookie/real SQL/no packages apply; this codec is pure.
Codex manager validates; never run validation commands or Git.

Exact public methods:
byte[] CreateRequest(ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> profileHash)
bool TryReadRequest(ReadOnlySpan<byte> request, out byte[] nonce, out byte[] profileHash)
byte[] CreateResponse(ReadOnlySpan<byte> request, bool ready, RSA signer)
bool VerifyResponse(ReadOnlySpan<byte> request, ReadOnlySpan<byte> response, RSA verifier, out bool ready)
Use System.Security.Cryptography and System.Text (only standard framework).

Request is EXACT68bytes: ASCII ECM1 prefix4, nonce32, profileHash32. CreateRequest
requires exact32/32 inputs, throws generic ArgumentException for malformed input.
TryReadRequest requires exact68/magic; false gives empty outputs, never partial.
Successful parse returns copied32byte nonce/hash arrays. No key material in frame.

Response EXACT325bytes: complete request68 echo, ready flag one byte0or1,256byte
RSA signature. Keys must be exactly2048bits. Signed input: exact UTF8 bytes
"EnglishCenter.M0.Maintenance.Readiness.v1\0" (terminal NUL, not literal backslash0)
followed by response first69bytes. Sign/verify RSA-PSS SHA256. No PKCS1/other algorithms.
CreateResponse rejects malformed request/wrong key size with generic argument error;
never include key.ToString/data/paths/exception text in error/output.
VerifyResponse checks expected request valid, exact response325, response echo all68
bytes matches request via CryptographicOperations.FixedTimeEquals, flag0or1, key2048,
signature valid over domain+first69. Initialize out ready=false; return false and
keep false on malformed/wrong key/signature or cryptographic exceptions. True
returns authenticated flag (false is a valid signed not-ready response).
Pure deterministic framing except signature implementation randomness. No logs,
no caching/retry, no certificate parsing/lifetime/policy. Those belong to manager.
No broad refactor/commentary/report files. Worker declares only bounded result.
