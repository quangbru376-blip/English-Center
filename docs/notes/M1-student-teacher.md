Student Profile reads and updates only the authenticated Student's Account/Profile.
Both RVs guard the full editable input; audit stores field names, no credential/PII copy.
Registration creation checks ACTIVE Course and one unfinished own registration.
Commands lock request, sorted business gates, Accounts/SIDs and domain rows.
Reads use SNAPSHOT plus final live authority; Teacher reads recheck current assignment.
Booking reuses the ungraded logical attempt; attempt2 needs actual completed attempt1.
Reschedule options exclude only its own source booking; command cancels old and creates new.
Capacity and own half-open calendar conflicts are checked again inside the transaction.
Cancellation copies the complete future booking/session/invoice manifest and preserves history.
Confirmation copies all attempts/Band/current bookings, recomputes best/FN02, creates no Invoice.
Teacher initial score/absence/correction requires current assignment and the exact manifest.
Correction also requires original grader and reason; initial provenance remains unchanged.
Business effects, mandatory audit and replay completion commit or roll back together.
Role pages use Vietnamese native controls, RAM only and explicit confirmation.
Lost ACK uses manual safe lookup/fresh read without resend; default host remains unchanged.
