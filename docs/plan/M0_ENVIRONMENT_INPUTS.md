# M0 — required actual environment bindings

2026-10-09. Owner assigned completing M0; ADR177/163 covers implementation and
reviewed application. This is missing configuration, not a request to repeat
permission for accepted work. No secret belongs in this document or worker context.
Current code supplies all nine dependency probes and a strict reference-profile reader;
none are registered in an active default host. Tests do not choose operator values.

| Required binding | Verified state / next action |
| --- | --- |
| Profile kind, origin, listen address/port | Owner selected LOCAL_DEV proposal2026-10-09, loopback127.0.0.1:8443/https://localhost:8443/no forwarding. Do not ask again. Operational listener/custody still unbound. |
| Lock/SQL/request/readiness/CSV deadlines, body/response/preview bounds | Owner selected proposal table values and10000preview work units; selected descriptor template exists, unbound refs/no active config. Not an SLO proof or weekly route activation. |
| Actual least-privileged SQL authentication | Preflight Windows-only/sysadmin diagnostic, manager Windows token not elevated. Dedicated runtime/schema/maintenance Windows identities require elevated provisioning; no runtime owner fallback. See M0_OPERATOR_CLOSURE. |
| Exact runtime-readable catalog | R proves880/880facts with VIEW DEFINITION plus two migration-history column reads; S implements this exact state-G-only guard allowlist. Actual declared operator grants remain unbound; do not switch to owner-mode checks or silently repair grants. Current 102 grants/retained D07 unchanged. |
| HTTPS certificate/client trust and SQL TLS trust | One existing current-user localhost server-auth key candidate, not custody/trust proof. Strict encrypted SQL connection fails untrusted certificate chain; certificate/service configuration needs elevated manager. No bypass or store/config mutation made. |
| JWT issuer/audience/key references/rotation | K proves actual supplied HMAC/RSA/ECDSA material, not deployed keys. Generate/provision only after actual custody binding; never write keys into tracked files or external prompts. |
| CSRF store and identity | L can read existing protected current-user DPAPI XML on Windows and prove restart decryption without key writes. Actual durable store, ACL/custody, application binding remain required. |
| Maintenance jobs/principal/interval | P jobs/guard, T actual SQL effects/rollback/narrow-grant tests, U synthetic timer lifecycle/independence/non-overlap/coalescing/shutdown/refusal PASS. Actual maintenance login/TLS/scheduled SQL binding and service-owned COMMIT remain; no runtime purge permission. |
| Approved patch/lock review | J compares explicitly supplied reviewed versions and five artifact pins with fresh runtime/hash evidence; SQL version resolver must use the selected DB reference. Fixture metadata is not actual approval. |

After these bindings: use Q explicit HTTP readiness and existing transport/execution/maintenance
boundaries; wire M0 only; verify full readiness/startup refusal/least privilege and
real browser→owned SQL, then owner walkthrough and close M0. Do not open M1–M4.
Backup destination/recovery operations and LAN/Internet exposure remain governed
by their accepted timing prerequisites, not fabricated as passing M0 evidence.
