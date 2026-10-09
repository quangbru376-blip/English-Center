# V01 / M0 LOCAL_DEV — acceptance closed2026-10-09

Full assigned M0 COMPLETE, integrated Codex review PASS/apply ADR163. Main source
b43ed99/managerb501cca identical;1276defaultPASS78intentionalSKIP/buildclean each.
Owner LOCAL_DEV/preview10000 fixed ADR186. ADR177 authorizes assigned operational
aspects; ADR191–192 records closure. Historical pending gates below are superseded
by exact actual evidence here, not silently dropped. M1–M4 remain unstarted.

| Obligation | Final evidence / scope |
| --- | --- |
| Signup201/no auto-login | Historical B/D/E/F real HTTP/SQL/browser source-equivalent adapters; owned Account/Profile/audit and no auto session PASS |
| Seven roles/login/me | Historical B/D/F all7 browser identities/owned sessions/security LOGIN PASS; actual E uses operational Admin with dedicated runtime signer/custody/profile |
| Tabs/profiles/logout | F owned revoke plus W5E actual shared/separate contexts,4sessions all revoked/4LOGIN/4LOGOUT fresh SQL PASS |
| Normal/restricted password | F real browser204/owned hash/version/temp/allSID effects/deliberate login; unchanged adapters/services PASS |
| Admin lock/manual lost ack | F actual response drop after owned lock/T23/audit COMMIT/manual lookup; unchanged production pilot; W5E real self-detail/self-lock-disabled/manual unknown404 PASS |
| Mandatory/independent audit | A–F atomic caller audit, E persisted fallback/timeout/race proof; W5D actual bootstrap mandatory1 and W5E independent4LOGIN/4LOGOUT/no incidents PASS |
| Actual SNAPSHOT/current authority/races | E distinct contexts/isolation/final live revocation and4two-client races PASS; no50user claim |
| Exact schema/grants | W2 owned M0 existing6migrations/24tables/exact4roles102permissions; genuine runtime880facts/maintenance denial PASS, no diagnostic owner fallback |
| Runtime identity | W1 nonadmin dedicated Windows runtime/maintenance/schema; W2–W5 actual SSPI safe principal/metadata/effective privilege guards PASS |
| Active typed profile | W5 exact protected selected LOCAL_DEV refs/finite values/5pins and all9 fresh Q before identity/audit/post-listener startup PASS |
| HTTPS/SQL trust/IP | W3 actual strict TLS/no TrustServerCertificate; W4 runtime own key/public leaf trust; W5C/E actual HTTPS+installed Edge platform trust/no bypass/loopback/no forwarding PASS |
| JWT/CSRF/private stores | W4 actual DPAPI/persisted framework key/nonexportable HTTPS/private-access denial/restart; W5 custody drift refusal/current issuer/audience/actual cookie/CSRF PASS |
| Patch/runtime/offline corpus | W5 five exact main package/artifact pins and installed NET/ASP/EF10.0.4/SQL17.0.1000.7, H real timezone/offline reviewed licensed corpus PASS; no latest/security release claim |
| Deadlines/body/buffer/preview | G/N typed/actual boundary tests; W5 bound3000/10/30000/25000/5000/262144/5000000/10000 PASS; no later weekly generator implementation claim |
| Maintenance separation | W5B genuine nonexportable proof key, public-only runtime read, local remote-reject/exact SID/fresh signed nonce/profile/SQL proof; stopped worker refuses PASS |
| Cleanup/scheduler/COMMIT | T real retention effects/narrow SQL rights; U independent periods/coalescing/cancellation; W5E actual system-clock60s replay COMMIT1 + explicit actual audit COMMIT1/failures0. Audit3600 unchanged; no elapsed-hour claim |
| Restart / usable owner entry | W5E both processes reuse same protected keys/profile/all9ready, stop maintenance503, graceful stop/forced0/remaining0; interactive Start-M0Local and owner runbook PASS |

W01 Student signup/W02 login+password/W03 lock pilot integrated caller/audit/replay
coverage is complete for implemented M0. Staff provisioning/remaining F32 commands,
full55 replay callers/audit UI and other product modules remain later milestones.
All fixture functional evidence stays explicitly synthetic where its context was
synthetic; actual W1–W5 closes environment assumptions without promoting fake
accounts/keys. Operational catalog has7roles/oneAdmin/Profile0,4revoked sessions,
bootstrap mandatory audit1 and persisted independent security history. No fake
staff/student rows, expired cleanup fixtures or demo passwords were promoted.

D07 retained unchanged, W4/W5 no SQL rerun/mutation; last W3 strict baseline byteSHA
10CD3407CE5986394703A9BEC2EE3E42314080809DDABE26906B1F14B593FC8E.
Owner4 unchanged/unshared/untracked/unstaged. No install/packages/push/deploy.
No LAN/Internet exposure, full load/SLO, backup/restore, elapsed90d retention or
production key servicing assertion. Key/manifest expiry still refuses; explicit
future reviewed servicing required, no automatic rotation/repair.

Source/actual receipts outrank older pending labels. Detailed history retained in
V01 A–V/E/F/T/U cards, M0 W1–W5D/E cards and ADRs186–192. Final summary:
V01-evidence/M0-final-summary.json. Owner guide: M0_LOCAL_DEV_RUNBOOK.md.