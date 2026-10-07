# U01 request/session scope worker — exact external payload

Prepared 2026-10-07 by Codex. Implementation authorized by owner; exact new
external sharing authorized by owner continuation. Codex is manager and reviewer.

Read ONLY this complete file, `.agents/skills/frontend-ui/SKILL.md`, and
`docs/plan/SKILL_ROUTING.md`, in that order, fully before writing. No links,
other files/AGENTS, listing/search, commands/Git/validation/network/SQL/secrets.
The sole output may be read after creation. Do not change context files.

Create ONLY `frontend/src/lib/requestScope.ts`, at most 1 file / 180 diff lines.
TypeScript6 strict ES2022/DOM, no dependencies or React imports, no `any`.
Do not implement HTTP/auth/login/credential storage, UUIDs, retries, timers,
cross-tab signaling, routes, business pages or tests. Codex integrates these
primitives into its separately reviewed HTTP/auth implementation.

## Accepted requirements and effective skill policy

E04 final section6: old-session or superseded-request responses must be discarded.
Logout/revoke/identity changes clear RAM caches/drafts/previews/secrets. Requests
are same-origin/JWT-cookie; CSRF and context RAM only. No durable storage/logs,
PII URLs, JWT parsing, optimistic business success, automatic credential/write
retry, heartbeat/idle renewal. No package additions despite skill suggestions.
Discarding a response does NOT prove a write rolled back; caller reports unknown.

## Exact API

Export `type RequestTicket = Readonly<{ signal: AbortSignal; isCurrent: () => boolean;
finish: () => void }>` and `class RequestScope` with these public methods:

- `begin(slot: string): RequestTicket`: one latest request per nonsecret logical
  slot; abort and supersede the previous ticket in that slot. Never infer slot
  from payload/URL/PII. Reject empty slot. Capture current session epoch; ticket
  is current only while same epoch, latest in slot, not finished and not aborted.
- `invalidate(): void`: advance epoch, abort all outstanding tickets and clear
  slot map, so even fetch implementations ignoring AbortSignal cannot publish.
  Registered RAM cleanup callbacks all run once per invalidation, including when
  another throws; failures cannot prevent other cleanup or epoch invalidation.
  Do not expose/log caught errors. Invalidation has no HTTP/server effect.
- `registerCleanup(cleanup: () => void): () => void`: register explicit cleanup;
  return idempotent unregister function. No ambient global caches/storage.

`finish()` is idempotent, removes only its own slot entry and never removes a
newer ticket. Keep completed result delivery check before finish in callers.
Slots retain no payloads/results/credentials. Use a simple counter/Map/Set,
snapshot cleanup callbacks before invoking them; no elaborate framework.
No mutable internal map/controller exposed. AbortController is local to tickets.
Internal epoch/sequence are opaque implementation details, not Account/session IDs.

Codex tests reverse completion order, epoch invalidation, ignored abort,
finished tickets, safe cleanup after throwing callbacks and unregister behavior.
Return concise output/limitations, never run those checks yourself.
