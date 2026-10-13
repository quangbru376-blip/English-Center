import { useEffect, useId, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import { ChevronDown, Menu, PanelLeftClose, PanelLeftOpen } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import type { AuthContext } from '../lib/auth';
import { Brand } from '../components/ui/Brand';
import { Button } from '../components/ui/Button';
import { Modal } from '../components/ui/Modal';
import { Avatar } from '../components/ui/Surfaces';
import { ShellSlots } from './shellSlots';

export type NavigationItem = { label: string; icon: LucideIcon; selected: boolean; onClick: () => void; disabled?: boolean };
const roleNames: Record<AuthContext['role'], string> = { Student: 'Học viên', Admissions: 'Tuyển sinh', AcademicAffairs: 'Giáo vụ', Teacher: 'Giáo viên', Accountant: 'Kế toán', Director: 'Giám đốc', Admin: 'Quản trị viên' };
export function WorkspaceShell({ context, navigation, children }: {
  context: AuthContext; navigation: readonly NavigationItem[]; children: ReactNode;
}) {
  const [drawer, setDrawer] = useState(false), [compact, setCompact] = useState(false), [accountOpen, setAccountOpen] = useState(false);
  const [account, setAccount] = useState<HTMLDivElement | null>(null);
  const accountRef = useRef<HTMLDivElement>(null), toggle = useRef<HTMLButtonElement>(null), panelId = useId();
  useEffect(() => {
    if (!accountOpen) return;
    const outside = (event: PointerEvent) => { if (event.target instanceof Node && !accountRef.current?.contains(event.target)) setAccountOpen(false); };
    document.addEventListener('pointerdown', outside);
    const escape = (event: KeyboardEvent) => { if (event.key === 'Escape' && !document.querySelector('dialog[open]')) { event.preventDefault(); setAccountOpen(false); toggle.current?.focus(); } };
    document.addEventListener('keydown', escape);
    return () => { document.removeEventListener('pointerdown', outside); document.removeEventListener('keydown', escape); };
  }, [accountOpen]);
  const nav = <nav aria-label={roleNames[context.role]}>{navigation.map(item => <Button key={item.label} variant="ghost" aria-label={item.label} title={item.label} aria-current={item.selected ? 'page' : undefined} disabled={item.disabled} onClick={() => { item.onClick(); setDrawer(false); setAccountOpen(false); }}><item.icon className="ec-icon" aria-hidden="true"/><span className="nav-label">{item.label}</span></Button>)}</nav>;
  return <ShellSlots.Provider value={{ account, closeAccount: () => { setAccountOpen(false); toggle.current?.focus(); } }}><div>
    <a className="skip-link" href="#main-content">Đến nội dung chính</a>
    <header className="ec-shell-header"><div className="ec-shell-header-inner"><div className="flex items-center gap-3"><Button className="ec-menu-toggle" variant="ghost" aria-label="Mở menu" aria-expanded={drawer} onClick={() => setDrawer(true)}><Menu className="ec-icon" aria-hidden="true"/></Button><Brand /></div>
      <div ref={accountRef} className="ec-shell-account" onKeyDown={event => { if (event.key === 'Escape' && accountOpen) { event.preventDefault(); setAccountOpen(false); toggle.current?.focus(); } }}>
        <button ref={toggle} type="button" className="ec-button ec-button--ghost ec-account-toggle" aria-label="Menu tài khoản" aria-expanded={accountOpen} aria-controls={panelId} onClick={() => setAccountOpen(open => !open)}><Avatar name={context.fullName}/><span className="ec-account-name">{context.fullName}<small>{roleNames[context.role]}</small></span><ChevronDown className="ec-icon" aria-hidden="true"/></button>
        <div id={panelId} className="ec-account-panel" hidden={!accountOpen}><p className="text-sm font-semibold break-words">{context.fullName}</p><p className="text-sm text-muted mb-3">{roleNames[context.role]}</p><div ref={setAccount}/></div>
      </div>
    </div></header>
    <div className={`ec-shell-layout ec-shell-layout--${context.role === 'Student' ? 'student' : 'staff'}${compact ? ' ec-shell-layout--compact' : ''}`}><aside className="ec-navigation">{context.role !== 'Student' && <Button className="ec-sidebar-collapse mb-4" variant="ghost" aria-label={compact ? 'Mở rộng thanh điều hướng' : 'Thu gọn thanh điều hướng'} onClick={() => setCompact(value => !value)}>{compact ? <PanelLeftOpen className="ec-icon" aria-hidden="true"/> : <PanelLeftClose className="ec-icon" aria-hidden="true"/>}</Button>}{nav}</aside><main id="main-content" tabIndex={-1} className="ec-shell-main">{children}</main></div>
    <Modal open={drawer} title="Điều hướng" className="ec-mobile-nav" onRequestClose={() => setDrawer(false)}><div className="ec-navigation">{nav}</div></Modal>
  </div></ShellSlots.Provider>;
}
