# Design system — Trung tâm tiếng Anh

Giai đoạn2, định hướng owner duyệt A/teal-vàng nắng. Một nền dùng cho Student
và staff, không thêm quyền hoặc data. Font/icon local, React/Tailwind hiện có.

## Token

Nguồn duy nhất: frontend/src/styles/tokens.css; theme và semantic CSS ở
styles/components.css; index.css import font/theme. Component không hardcode hex,
font hoặc px. SVG geometry chỉ là tọa độ vector, fills dùng token.

| Nhóm | Contract |
|---|---|
| Brand | Primary0F766E, hover115E59, softE5F3EF; accentFBBF24; dark123F3A |
| Canvas/text |fafaf6, white surface, text1E293B, muted526168 |
| Semantic | Success166534/EDF7EE; warning854D0E/FFF5D6; dangerB42318/FFF0ED; info155E75/EDF7FA |
| Typography | Be Vietnam Pro400/500/600/700, Vietnamese+Latin subsets, sizes14/16/20/24/32/40; body1.6 |
| Space |4px steps (rem tokens); no unique page values |
| Shape |8pxcontrols,12pxcards/dialogs,pillbadge/avatar; two light shadowlevels |
| Motion |180mscolors; existing prefers-reduced-motion respected |
| Controls |44pxminimum touch, labels/descriptions/errors, native DOM, visible focus |

Contrast tests use WCAG sRGB luminance on exact source tokens; body/muted/brand/
all semantic text pairs >=4.5:1. Disabled text retains muted/canvas contrast;
color is never the only state indication. Focus/input borders use dark teal.
These checks are not a whole-product accessibility certification.

## Component mapping

| Component | Source/contract |
|---|---|
| Button | ExistingButton: primary/secondary/danger +ghost/large; native type/disabled/pending unchanged |
| Input/Select | ExistingTextField/SelectField names retained; same labels/IDs/names/error aria links |
| Textarea/FormField | Surfaces.tsx; exact raw text, linked hint/error, native textarea |
| Card/Badge/Avatar | Surfaces.tsx; semantic surfaces/state text, display initials only |
| Dialog | ExistingModal; native showModal/Tab trap/Escape/cancel/backdrop and opener restoration retained |
| Alert/Toast | Feedback +aliases; errors persistent, status not auto-dismiss |
| Table/Pagination | Surfaces.tsx; caption/keyboard-scroll region, bounds/disabled, explicit onChange |
| Tabs | Controlled nativebuttons, tablist/tabpanel IDs and arrow/Home/End keyboard |
| EmptyState/Skeleton | Clear empty/loading, decorative BookOpen, no data or fake progress |
| PageHeader/Brand | Reused semantic heading; original geometric temporary wordmark |

Keep feature schemas/validation/save units in existing pages, not design system.
Shared primitives do not fetch, renew sessions or calculate money. Continue exact
money digit strings; localized display never Number-converts financial amounts.

## Shell và account controls

WorkspaceShell: Student top navigation >=768/mobile drawer; staff sidebar >=768,
optional compact icon mode. Existing routes and page callbacks are used. Native
Modal handles drawer focus, Escape and keyboard wrap; navigation has selected
aria-current and explicit names in compact mode. No bottom tabs or new router.

AccountActions portals existing page controls into the shared account panel,
retaining original handlers/disabled conditions/secret cleanup. Parent profile nav
is Student-only. Account Escape/click-away closes, selection returns focus; real
business dialog Escape keeps its own cancel handling. Panels fit within viewport.
NORMAL current verified context alone renders the shell. Checking/unknown use
IdentityShell; local Student view is scoped to client/auth revision, never durable.

Student local-view requests reuse the previous view-selection cleanup. No initial
data-fetch behavior change yet: that implementation belongs to phase3 pilot.
One six-item navigation replaces the duplicate rows; profile/password/logout live
in account area. Preserve existing labels for compatibility with user workflows.

## Auth / recovery

IdentityShell: desktop brand panel with one decorative original book illustration
and form; mobile/tablet concise wordmark+form. Login/signup/password fields, raw
values, validation/controller/CSRF callbacks unchanged. Login's explicit checkSession
is in a recovery disclosure; unavailable CSRF prepare controls remain visible.
NORMAL password page uses workspace, PASSWORD_CHANGE uses restricted auth shell;
no business menus or scope upgrade for temporary-password sessions.

Styleguide at tests/ui-refresh-styleguide.html is a separate dev-only entry with
sample text and no API calls; default build contains no styleguide route. No UI
kit/Recharts/Playwright dependency was added. Stage2 live preview: Vite loopback
port5178 while reviewing; product runtime profile/pins remain untouched.

## Asset provenance

- @fontsource/be-vietnam-pro5.3.0, OFL-1.1; npm registry metadata checked13/10/2026.
  Full package LICENSE copied to public/licenses/BeVietnamPro-OFL.html.
- lucide-react1.55.0, ISC; metadata checked same date; license copied to
  public/licenses/lucide-ISC.html. All UI icons from this set, standardized size/stroke.
- Brand.tsx wordmark and LearningIllustration SVG authored for this UI task,
  CC0-1.0 dedication documented in source/public license note. No third-party art,
  external CDN/image request, mascot or AI bitmap. Font files are bundled locally.

New libs pinned exactly in package.json/lock; ignore-scripts install. No claim of
latest/security servicing. Existing .NET/SQL/runtime/crypto material unchanged.

## Verification / review

Frontend174tests includes textarea linkage/raw value, empty pagination bounds,
state labels/caption/avatar and14contrast pairs. Real Edge styleguide375/768/1280
checks fonts, no pageoverflow, Tabs keyboard and ModalEscape/opener restoration.
Business/auth browser evidence and final review recorded separately in stage2
review/evidence. Earlier exploratory screenshots/failures are not final PASS.
Cổng2 owner reviews actual styleguide/auth/shell before Student pilot phase3.
