# Full Design Agreement

Status: done
Owner direction: 2026-09-27
Updated: 2026-10-03

## Objective and boundary

2026-10-02 owner-requested side review: textual report business/function
table rewritten in [BUSINESS_FUNCTION_TABLE](../design/BUSINESS_FUNCTION_TABLE.md)
with37 retained functions and the original five-column structure/source STT;
no new policy decisions. The textual
comparison is in [BUSINESS_FUNCTION_COMPARISON](../reference/BUSINESS_FUNCTION_COMPARISON.md).
It covers39 source NV/37 retained/32 F/21 W and21 report-alignment items;
source Word remains unchanged. This does not complete or accept E05, and
did not advance E04 D5 at its date or authorize implementation/report-scope
additions. Current contracts checkpoint below advances independently.

Agree on business functions → workflows → database → system architecture →
consistency/owner acceptance before implementation planning (ADR-011).
Source rules: `../requirements.md`, decisions: `../DECISIONS.md`, provenance:
`../reference/REPORT_BASELINE.md`. Leave Word unchanged; illustrative forms/
older kits do not supersede accepted memory. No Git/scaffold/Slice 0/migration/
application code, external coding connections or new packages in this stage.

## Stages and acceptance outputs

These statuses describe completion of reviewable stage outputs, not how many
individual choices are accepted. A–C approval does not by itself finish E.

| Stage | Status | Output to agree with the owner |
|---|---|---|
| 1. Business functions | done | Owner accepted E01-v1 on 2026-09-30: [FUNCTION_SPEC](../design/FUNCTION_SPEC.md) + [FUNCTION_CATALOG](../design/FUNCTION_CATALOG.md); actor/module/input-output/rights/validation/effects and39 NV→37 retained/32 F trace. |
| 2. Workflows | done | Owner accepted [WORKFLOW_SPEC E02-v1](../design/WORKFLOW_SPEC.md) on 2026-09-30: 21 lifecycle/cross-cutting flows, state transitions, actor/guards/effects/audits/transactions, concurrency/unknown-response and37 NV/32 F trace. |
| 3. Database | done | Owner accepted actual [DATABASE_SPEC E03-v1](../design/DATABASE_SPEC.md) on2026-10-01:24 tables/230 fields, ERD/keys/constraints/history/projections,5 TG/6 VW/6 FN/7 SP/13 TX/4 SQL roles/index candidates/rubric/18 DC/21 W trace and E04 handoff. E03 checked; no executed SQL/tests or implementation approval. |
| 4. System architecture | done | Owner accepted actual [E04-v1](../design/SYSTEM_ARCHITECTURE_SPEC.md) +[final contract](../design/E04_FINAL_CONTRACT.md)/catalog/DTO/SC01–04 on2026-10-03,ADR-160. Full presented technical contracts accepted,DF01–05/D12/D13 environment/evidence deferred;no implementation/tests. |
| 5. Consistency and acceptance | done | Owner accepted [E05-v1](../design/E05_BASELINE_REVIEW.md) after E04 on2026-10-03,ADR-161:37NV/32F/21W trace/state parity/24cases/findings/excluded/deferred/rubric and complete E01–E04 design baseline. No implementation authority or runtime proof. |

## Approved current choices

Owner approved all 33 A01–A10/B01–B07/C01–C16 [x], with no alternatives, and
confirmed D/E remain open. Recorded ADR-083–ADR-094; synced requirements,
FUNCTION_CATALOG, target model, current state/TODO/handoff/provenance.

- A: operational viewing/account menu/Student history and catalogue/class
  lifecycle/initial entry-score timing.
- B: explicit actual occurrence, final-exam replacement, time/Student conflicts,
  effective closure, weekly all-or-none, blocked assignments/action previews.
- C: logical-attempt/booking history, Schedule occurrence/obligations and Score
  dependencies, unfinished flag/relations/freeze/IDs/invoice number, types/
  normalization/time, auth state, atomic/replay/audit contracts and reporting.
- Target model now includes 22 entities (EntryTestBooking/AuthSession added),
  not deployed schema or accepted complete dictionary/ERD. C13 replay-result
  physical store mapping still belongs in the full dictionary.
- Prior accepted policies remain; no scholarships,/10, post-confirmation retake,
  on-behalf booking, extra LESSONs, transfers/refunds or automated debt penalties.

## Complete design baseline Accepted — 03/10/2026

Owner confirmed actual E04-v1 first (ADR-160),then actual E05-v1/full baseline
(ADR-161),without amendments. Stages1–5 done;OWNER_REVIEW73/75accepted,
only D12/D13 deferred with original notes. RF156–159/prior Accepted policies
preserved. SC01–04/exact technical refinements are accepted design contracts,
not runtime evidence;DF01–05 operational/evidence prerequisites remain.

No further design-approval question pending in this baseline. Await separate
owner direction for next work;do not start implementationplanning/code/SQL/
Git/scaffold/deployment/packages/integration or operational execution merely
because baseline accepted. Historical acceptance checkpoints below retained.

## E01-v1 accepted — 2026-09-30

FUNCTION_SPEC details37 retained functions with7 consistent components each,
shared actions/validation matrix, source ADRs and32 individual F mappings.
Catalog retains39 NV source rows; NV10/NV11/F15 excluded explicitly. Document
structure/coverage verified, not application behavior or tests. Accepted requirement
rules/ADRs are unchanged; requirements section7 records E01 status. Owner
explicitly accepted E01-v1 in chat on 2026-09-30; this is stage-output
acceptance, not implementation evidence. E02–E05 and D12/D13 are not accepted
by E01.

## E02-v1 accepted — 2026-09-30

WORKFLOW_SPEC was consolidated from the owner-confirmed E02 sections and
accepted E01/ADR contracts. It contains 21 flow IDs W01–W21, a state-transition
matrix, shared error/replay/audit rules, design verification cases and trace to
all37 retained NV and32 in-scope F codes; NV10/NV11/F15 remain excluded.
Structural document coverage was checked, not application behavior, SQL,
browser, CSV or automated tests. Owner explicitly accepted the full E02-v1
artifact in chat on 2026-09-30, after section-level review; E02 is checked
and stage 2 done. E03–E05 and D12/D13 remain open.

## E03-v1 accepted — 2026-10-01

Owner explicitly accepted the actual consolidated DATABASE_SPEC E03-v1,
including section14 physical details/handoff, after A–G2 review (ADR-131–138).
Checked only E03; stage3 done and stage4 doing. Document coverage is24 tables/
230 fields/21 W/37 NV/32 F, with SQL/rubric/permission/transaction verification
objectives, not executable evidence. No new product ADR or implementation approval.
At that E03 acceptance checkpoint E04-v0.1 was incomplete; presented package1 accepted ADR-139,
presented Auth/session2a through Attendance/per-Student draft4e accepted ADR-140–149;
owner-requested E04_REMAINING_REVIEW E04-R1's all23 displayed rows are now
accepted as ADR-150–155; this still does not accept unseen refinements/E04/E05.
E04/E05 and D12/D13 remain open.

## Working method and durable updates

Owner-requested side work 2026-10-02: business sequence planning is in
`docs/design/SEQUENCE_PLAN.md` (25 proposed SQ items traced to accepted
E01/E02's 21 W and 37 retained NV). Owner draws in Enterprise Architect;
agent supplies information/answers only, no generated diagrams. The division
is proposed, not a new product decision or E04/E05 acceptance. That side work
did not advance D5 at its date; current E04 checkpoint and review status above
take precedence. Owner resumed remaining E04 here on2026-10-03.

SQ01, SQ03, SQ04, SQ05, SQ07, SQ11, SQ13, SQ15 and SQ23 now have
owner-requested Markdown drawing guides linked
from SEQUENCE_PLAN; actor/boundary/control, messages/guards/notes and text
sketches only. SQ03 distinguishes login/logout from SQ04's mandatory-audit
password change and revocation of all sessions; no product policy changes.

Requirements own accepted product rules; ARCHITECTURE owns target data/system
contracts; ADRs own significant decision context/source/consequences. Focused
documents may hold full workflows/dictionary/API design with links from memory.
Separate accepted choices, unapproved proposals and implemented evidence.

Cross-check every retained NV/F function against actors/data/guards/architectural
ownership. Stage 1 still has 39 source NV references (NV10/NV11 removed →37
retained) and 32 F screens (F15 removed, no renumbering). Expected verification
cases are design statements now, not executable test results.

After full baseline E05 acceptance, obtain owner direction before moving to
implementation planning/update plan.md. No stage is complete merely because a
discussion estimate has elapsed; the older 8–12-round estimate is superseded by
the consolidated review workflow, not a completion guarantee.

## Handoff

Resume from `HANDOFF.md`, current OWNER_REVIEW, owner-requested
E04_REMAINING_REVIEW and Project Memory. The new file reviews remaining
packages only; OWNER_REVIEW keeps sole E04/E05 gates. Do not ask
the owner to retell accepted choices or ADR-001–ADR-149. Do not re-ask the
D17 operator unless owner requests change. E01-v1/E02-v1/E03-v1 are accepted;
E04/E05 are not.
