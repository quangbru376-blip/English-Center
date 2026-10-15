# Student registration feedback — 2026-10-15

Owner screenshot after real course registration shows six duplicate generic reasons
and an empty attempt grid consuming space. Follow-up to UI_SPACING_TASK_CARD;
implementation_skills frontend-ui; review_skills code-review, same already-read
skill versions/overrides. Scope: StudentEnrollmentPage, new pure
StudentActionReasons component, student.css, relevant regression/browser fixtures.
No API/backend/session/command/rights/data reset or NV04 change.

Show reasons only for the actions actually rendered in each group and currently
unavailable. Group identical server text once, prefix with the applicable action
labels; place booking-change reasons next to an existing booking only. Preserve
server text/allowedActions; no inferred reason or new authority. Omit empty attempt
grid. Verify fresh-registration UI at375/768/1280, existing frontend checks and
manual disabled/enabled action presentations, then publish only demo UI with
original custody/data/owner sessions retained.

Verification:186frontend tests PASS (183existing+3reason regressions), types/lint/
isolatedbuild PASS; real DEV main with mocked fresh Registration at375/768/1280
PASS: exactly1labelled reason instead of6, no empty attempt grid, book/cancel enabled
and confirm disabled, no overflow/page errors/API mutations. Screenshot is a mock
of the reported state; no access or mutation of owner's real registration186.
Reviewed helper filtering/deduplication/plain-text escaping and unchanged button
handlers/capabilities/API/session contracts: Codex review **PASS**. Backend/SQL,
NV04 and broad UIgate4/recovery acceptance are outside this follow-up.

Published32-file UI to demo; previous build retained in
.local-demo/registration-feedback-previous-dist. Anonymous trusted Edge verifies
new CSS hash at3widths/API mutations0 and fresh ready200; launcher session15684
retained. No reset/password operation/session revocation. Evidence:
student-registration-feedback-evidence/{browser.json,live-ui.json,tests.log,
lint.log,build.log,publish.json,release-files.json}. No commit/push/public deploy.
