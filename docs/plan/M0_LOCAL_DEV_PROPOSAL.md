# M0 LOCAL_DEV — selected environment

Owner selected LOCAL_DEV according to this proposal2026-10-09. Table values below
are selected environment values, not proof of an active operational profile.
ADR177 already authorizes implementation/checks; this is selection of previously
deferred E04 D12/D13 values, not renewed task/delegation permission. Do not re-ask
LOCAL_DEV versus LAN_DEMO or these accepted table values.

| Slot | Selected value |
| --- | --- |
| Kind / exposure | LOCAL_DEV, this laptop only |
| Same UI/API origin | https://localhost:8443 |
| Listen endpoint | 127.0.0.1:8443; no forwarding; empty trusted proxy/network sets |
| Lock / SQL / request | 3000 ms / 10 seconds / 30000 ms |
| CSV / readiness | 25000 ms / 5000 ms |
| Request body / response buffer | 262144 bytes / 5000000 bytes |
| WeeklyPreviewBudget | 10000 work units per weekly preview; owner selected2026-10-09 |
| Replay / audit job intervals | 60 seconds / 3600 seconds, separate maintenance identity |
| JWT issuer / audience | english-center-m0-local / english-center-m0-local-ui |
| Timezone / database-provider compatibility | Asia/Ho_Chi_Minh / explicit160, fixed accepted values |

Port8443 was not listening at the read-only observation; not reserved or promised
available at startup. No SQL login, OS identity, certificate trust, secret directory,
listener, grants or active configuration was created by preparing this proposal.

WeeklyPreviewBudget is now selected:10000 work units, resource exhaustion503, not
a business lesson/date cap. M0 does not mount a weekly generator. The later weekly
implementation must explicitly define/count work units and verify the normal-use
envelope before mounting; selecting a positive descriptor does not implement it.

Required operational closure after selection:
1. Materialize dedicated runtime/schema/maintenance identities and protected stores
   in a separately reviewed concrete operator contract; keys never enter Git/chat.
2. Bind the declared R/S metadata candidate against exact D12 role grants without owner
   fallback, undeclared permissions or weakening expected-manifest verification. R/S verified VIEW DEFINITION plus two history-column reads; no permanent grant applied.
3. Validate web and SQL certificate trust independently; TrustServerCertificate=False
   remains mandatory for the transport/maintenance probes.
4. Register all9 essential probes, mount explicit M0 adapters only after startup
   gates, then rerun fresh runtime/browser/owned-commit evidence and restart checks.

E04_RUNTIME_PROFILE missing slots still fail closed. Values here were selected by
the owner; operational references and actual dependency proof remain required.
