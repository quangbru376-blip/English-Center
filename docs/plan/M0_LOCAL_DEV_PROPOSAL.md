# M0 LOCAL_DEV — concrete environment proposal

2026-10-08. Proposal awaiting owner's environment decision, not an active profile.
ADR177 already authorizes implementation/checks; this is selection of previously
deferred E04 D12/D13 values, not renewed task/delegation permission.

| Slot | Proposed value |
| --- | --- |
| Kind / exposure | LOCAL_DEV, this laptop only |
| Same UI/API origin | https://localhost:8443 |
| Listen endpoint | 127.0.0.1:8443; no forwarding; empty trusted proxy/network sets |
| Lock / SQL / request | 3000 ms / 10 seconds / 30000 ms |
| CSV / readiness | 25000 ms / 5000 ms |
| Request body / response buffer | 262144 bytes / 5000000 bytes |
| Replay / audit job intervals | 60 seconds / 3600 seconds, separate maintenance identity |
| JWT issuer / audience | english-center-m0-local / english-center-m0-local-ui |
| Timezone / database-provider compatibility | Asia/Ho_Chi_Minh / explicit160, fixed accepted values |

Port8443 was not listening at the read-only observation; not reserved or promised
available at startup. No SQL login, OS identity, certificate trust, secret directory,
listener, grants or active configuration was created by preparing this proposal.

WeeklyPreviewBudget remains unresolved: it is a resource budget, not a hidden lesson
or date cap. M0 has no weekly generator; its normal-use envelope/unit belongs to the
accepted runtime profile review before that caller is mounted. Do not invent a
business limit to make a descriptor validate.

Required operational closure after selection:
1. Materialize dedicated runtime/schema/maintenance identities and protected stores
   in a separately reviewed concrete operator contract; keys never enter Git/chat.
2. Resolve runtime-only catalog visibility against exact D12 grants without owner
   fallback, hidden VIEW DEFINITION or weakening expected-manifest verification.
3. Validate web and SQL certificate trust independently; TrustServerCertificate=False
   remains mandatory for the transport/maintenance probes.
4. Register all9 essential probes, mount explicit M0 adapters only after startup
   gates, then rerun fresh runtime/browser/owned-commit evidence and restart checks.

E04_RUNTIME_PROFILE explicitly says missing slots fail closed and Codex does not
choose timeout/body/weekly caps in place of the owner. Thus existing fixture numbers
are not promoted to approved operational values by passing tests.
