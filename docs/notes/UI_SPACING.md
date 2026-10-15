# UI spacing — 15/10/2026
Shared CSS supplies token-based label, section, detail and action gaps.
`ec-stack` lays out vertical detail content; `ec-actions` wraps related buttons.
Modal keeps its native dialog handlers and adds a spaced body wrapper.
Calendar cards separate text and actions; narrow weekly cells keep controls inside.
Native nested labels receive the same8px control gap as shared fields.
Mobile outer card padding leaves more room for forms and wrapped actions.
All handlers, text, payloads and non-class JSX attributes retain AST parity.
183existing tests, types, lint/build and bounded Edge checks passed.
Visual mocks do not prove SQL/authentication or every populated business state.
Evidence/review: docs/plan/UI_SPACING_REVIEW.md and ui-spacing-evidence/.
