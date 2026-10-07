# U06 pure Admin detail panel — exact external payload

Read ONLY this complete card, .agents/skills/frontend-ui/SKILL.md and
docs/plan/SKILL_ROUTING.md, in that order, fully before writing. No links/source/
AGENTS/search/list/commands/Git/tests/network/SQL/secrets. Own output read after
creation only. Codex owns authorization/dialog/requests/tests/review.
Create ONLY frontend/src/features/administration/components/AdminAccountPanel.tsx,
one file <=150 added/deleted lines. React19.3/TS6/Tailwind4, no any/dependencies.
Canonical overrides: native shared controls/system font/tokens/44px, Vietnamese,
safe error envelope not ProblemDetails, cookie JWT/CSRF RAM only. Pure view,
no hooks/state/auth/fetch/storage/logs/UUID/timers/retry/navigation/confirmation.
Export AdminAccountPanelProps={account:{id:number;fullName:string;email:string;
phone:string|null;role:string;status:string;mustChangePassword:boolean};
createdLabel:string;temporaryExpiryLabel:string;canLock:boolean;
lockDisabledReason:string|null;pending:boolean;onRequestLock:()=>void;
onReload:()=>void}. Named AdminAccountPanel export.
Import Button from ../../../components/ui/Button. Native props plus pending,
variant primary/secondary/danger; do not read source. All props trusted manager
projection; escaped text only, no dangerouslySetInnerHTML. account role/status
already Vietnamese labels; no role selector or edit inputs. Render semantic dl
with id/name/email/phone (null => Không có)/role/status/createdLabel/
mustChangePassword (Có/Không)/temporaryExpiryLabel. Simple responsive grid,
min-w-0 and break-words to avoid overflow. No password/hash/SID/counters/history.
Only two actions: Button danger text Khóa tài khoản disabled !canLock or pending,
onClick onRequestLock; Button secondary text Tải lại disabled pending,onReload.
If !canLock render lockDisabledReason or Không thể khóa trong trạng thái hiện tại.
Show short read-only impact: Khóa sẽ hủy mọi phiên của tài khoản; không xóa dữ liệu.
Never render reset/unlock/disable/restore/create/search/audit/other business controls,
even if backend permits them. No fake success, modal or extra authorization logic.
