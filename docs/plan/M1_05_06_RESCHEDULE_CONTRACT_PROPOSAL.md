# API-029 reschedule options — proposed, awaiting owner selection

Current accepted DTO requires `attemptContext.currentBookingId` to be null and
returns no options while a SCHEDULED booking exists. API-032 requires that booking
and the selected target Session rowversion. Thus the accepted Student contracts
do not currently supply a usable target picker for the required reschedule page.
This is a newly identified contract gap, not a new permission gate.

Proposed bounded refinement, not implemented or accepted yet:
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

If the owner selects postponement instead, the full package-5 completion/apply
claim must remain blocked by its missing required reschedule UI.
