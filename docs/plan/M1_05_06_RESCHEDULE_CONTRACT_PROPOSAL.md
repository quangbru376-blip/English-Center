# API-029 reschedule options — technical correction implemented 2026-10-10

The earlier pending-selection framing below is superseded. Existing accepted W07
requires reschedule and API032 already defines its complete mutation. Codex resolves
the missing target-options input as a technical contract correction under standing
ADR177 and the owner's completion/apply/continue instruction; no new business
policy, role, action, endpoint, schema or runtime setting is introduced. This does
not claim that the owner explicitly selected the parameter. Exact E04/API docs and
SQL/HTTP/frontend/Edge verification now reflect the correction.

Historical proposal rationale follows; its permission gate is withdrawn.

Current accepted DTO requires `attemptContext.currentBookingId` to be null and
returns no options while a SCHEDULED booking exists. API-032 requires that booking
and the selected target Session rowversion. Thus the accepted Student contracts
do not currently supply a usable target picker for the required reschedule page.
This is a newly identified contract gap, not a new permission gate.

Implemented bounded refinement:
- Keep the existing API-029 endpoint, nine outer fields and 154 endpoint count.
- Add optional positive `bookingId` query, scoped to the URL's own Registration.
- Without that query, preserve existing book/rebook behavior and literal-null
  currentBookingId. With it, require the own current SCHEDULED booking and its
  source Session OPEN and strictly future, with the existing pre-confirmation guards.
- Return its logical Attempt ID/number/RV and currentBookingId in the existing
  four-field attemptContext; return the Registration RV as before.
- List only OPEN strictly future targets with active Teacher, fresh capacity and
  no own Class/other-booking half-open overlap. Exclude the source Session, and
  exclude the replaced source booking from the overlap test only.
- Keep same SNAPSHOT/final authority, paging/date semantics and read-only behavior.
- API-032 body/C1/replay/action registry/atomic effects remain unchanged: full
  Registration/Attempt/Booking/source/target RVs, cancel old + new same Attempt.
- UI requires deliberate target selection and one confirmation; unknown result
  uses existing manual lookup, never resend. Add scoped options SQL/HTTP tests,
  stale/full-seat/overlap rollback and three-width Edge reschedule verification.

Without this correction the required W07 UI could not select a target. The earlier
proposal to defer it did not satisfy completion; that proposal was not applied.
