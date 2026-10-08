# M0 — required actual environment bindings

2026-10-08. Owner assigned completing M0; ADR177/163 covers implementation and
reviewed application. This is missing configuration, not a request to repeat
permission for accepted work. No secret belongs in this document or worker context.
Current code supplies all nine dependency probes and a strict reference-profile reader;
none are registered in an active default host. Tests do not choose operator values.

| Required binding | Verified state / next action |
| --- | --- |
| Profile kind, origin, listen address/port | D13/DF02 deferred; concrete M0_LOCAL_DEV_PROPOSAL value-selection question pending. LOCAL_DEV loopback and LAN_DEMO are different exposure profiles. HTTPS/same-origin/Secure/CSRF stay mandatory. |
| Lock/SQL/request/readiness/CSV deadlines, body/response/preview bounds | Typed finite ordering already enforced; actual values still DF01 pending. No fixture budget promoted to an SLO or default. |
| Actual least-privileged SQL authentication | Existing Windows diagnostic is privileged and denied by runtime probe. No new login/auth-mode change inferred. Must bind a genuine non-dbo ec_app_runtime identity, separate schema/maintenance contexts. |
| Exact runtime-readable catalog | R proves880/880facts with VIEW DEFINITION plus two migration-history column reads; S implements this exact state-G-only guard allowlist. Actual declared operator grants remain unbound; do not switch to owner-mode checks or silently repair grants. Current 102 grants/retained D07 unchanged. |
| HTTPS certificate/client trust and SQL TLS trust | Browser fixtures use transient exact-pinned certificates. Actual origin/listen and selected trust/custody needed before concrete transport proof. No global trust bypass. |
| JWT issuer/audience/key references/rotation | K proves actual supplied HMAC/RSA/ECDSA material, not deployed keys. Generate/provision only after actual custody binding; never write keys into tracked files or external prompts. |
| CSRF store and identity | L can read existing protected current-user DPAPI XML on Windows and prove restart decryption without key writes. Actual durable store, ACL/custody, application binding remain required. |
| Maintenance jobs/principal/interval | Accepted replay24h/security-IP90d/mandatory-retained policy is unchanged. P supplies configuration/probe/scheduler code; actual scheduler binding and cleanup effects remain; no runtime purge permission. |
| Approved patch/lock review | J compares explicitly supplied reviewed versions and five artifact pins with fresh runtime/hash evidence; SQL version resolver must use the selected DB reference. Fixture metadata is not actual approval. |

After these bindings: use Q explicit HTTP readiness and existing transport/execution/maintenance
boundaries; wire M0 only; verify full readiness/startup refusal/least privilege and
real browser→owned SQL, then owner walkthrough and close M0. Do not open M1–M4.
Backup destination/recovery operations and LAN/Internet exposure remain governed
by their accepted timing prerequisites, not fabricated as passing M0 evidence.
