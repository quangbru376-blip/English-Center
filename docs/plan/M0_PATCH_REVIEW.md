# M0 LOCAL_DEV installed artifact review

2026-10-09, W5A / ADR177 / ADR186. Pin the existing installed compatible SDK10.0.200,
.NET/ASP10.0.4, EF10.0.4 and SQL Express17.0.1000.7 for this local M0 profile.
This records reproducibility/compatibility, not a latest-release or production-security claim.
No installation/update/package modification. Exact five main-workspace artifact byte pins
follow; fresh probes refuse version, date, length or SHA drift. Quarterly review expires
2027-01-09; changes require operator review, never automatic patch acceptance.

```json
{
  "dotNet": "10.0.4",
  "aspNet": "10.0.4",
  "ef": "10.0.4",
  "sql": "17.0.1000.7",
  "reviewed": "2026-10-09",
  "nextReview": "2027-01-09",
  "artifacts": [
    {
      "relativePath": "backend/global.json",
      "length": 124,
      "sha256": "58BFCF8AF1D45A49149987BB2FB630BAA3C7423ED0358AD393A3988FE3B89ADB"
    },
    {
      "relativePath": "backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj",
      "length": 1181,
      "sha256": "450A7938ED9EAE2C5A8B5037A8348D36FFDA6AC1D976BB69920AF03972AB9E27"
    },
    {
      "relativePath": "backend/src/EnglishCenter.Api/packages.lock.json",
      "length": 15618,
      "sha256": "C289DDDDAEB680F306C57CE87AFA15D01A464048BE3D16D2B1D1C3161C6DAFDD"
    },
    {
      "relativePath": "frontend/package.json",
      "length": 1067,
      "sha256": "70B9F0EE8669E156FDF1C3F60E10ACDE9A54AE23E25577EE20CF64B77784F55F"
    },
    {
      "relativePath": "frontend/package-lock.json",
      "length": 122609,
      "sha256": "F25552576DAF532AED94114363A59861CE8FEB21E90042E8F0579218ACBD2637"
    }
  ]
}
```


UI handoff2026-10-13: exact frontend/package.json and package-lock.json pins
refreshed for owner-approved Be Vietnam Pro5.3.0/lucide-react1.55.0 only.
Codex integrated review PASS/183frontend/21Edge/1438default backend, source receipt
stage6/reviewed-source.json. Three backend pins/runtime versions/2026-10-09 review/
2027-01-09expiry preserved; no quarterly/security/latest/update claim. Installed
manifest refresh is guarded by Refresh-UIArtifactPins; before/after receipts are in
UI-refresh-evidence/stage6. Prior manifest retained runtime-pins-before.json.
