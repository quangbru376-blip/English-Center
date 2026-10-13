import { useId } from 'react';
import type { HTMLAttributes, ReactNode, TextareaHTMLAttributes } from 'react';
import { BookOpen, ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from './Button';
import { Feedback } from './Feedback';

export function Card({ children, className = '', ...props }: HTMLAttributes<HTMLElement>) {
  return <section {...props} className={`ec-card ${className}`}>{children}</section>;
}
export function Badge({ children, tone = 'neutral' }: { children: ReactNode; tone?: 'neutral'|'success'|'warning'|'danger'|'info' }) {
  return <span className={`ec-badge ec-badge--${tone}`}>{children}</span>;
}
export function Avatar({ name }: { name: string }) {
  const initials = name.trim().split(/\s+/).filter(Boolean).slice(-2).map(word => Array.from(word)[0]).join('').toLocaleUpperCase('vi');
  return <span className="ec-avatar" aria-hidden="true">{initials}</span>;
}
export function EmptyState({ title, children, action }: { title: string; children: ReactNode; action?: ReactNode }) {
  return <div className="ec-empty"><BookOpen className="ec-icon mx-auto" aria-hidden="true"/><h2>{title}</h2><p>{children}</p>{action && <div className="mt-4">{action}</div>}</div>;
}
export function Skeleton({ label = 'Đang tải nội dung…' }: { label?: string }) {
  return <div role="status" aria-label={label} className="grid gap-3"><span className="ec-skeleton" aria-hidden="true"/><span className="ec-skeleton" aria-hidden="true"/><span className="sr-only">{label}</span></div>;
}
export function Table({ caption, children }: { caption: string; children: ReactNode }) {
  return <div className="ec-table-wrap" role="region" aria-label={caption} tabIndex={0}><table className="ec-table"><caption className="sr-only">{caption}</caption>{children}</table></div>;
}
export function FormField({ id, label, hint, error, children }: { id: string; label: string; hint?: string; error?: string; children: ReactNode }) {
  return <div className="ec-field"><label htmlFor={id} className="ec-label">{label}</label>{children}{hint && <p id={`${id}-hint`} className="ec-field-hint">{hint}</p>}{error && <p id={`${id}-error`} className="ec-field-error">{error}</p>}</div>;
}
export function Textarea({ label, hint, error, id, className = '', ...props }: TextareaHTMLAttributes<HTMLTextAreaElement> & { label: string; hint?: string; error?: string }) {
  const generated = useId(), fieldId = id || generated;
  const description = [props['aria-describedby'], hint ? `${fieldId}-hint` : null, error ? `${fieldId}-error` : null].filter(Boolean).join(' ') || undefined;
  return <FormField id={fieldId} label={label} hint={hint} error={error}><textarea {...props} id={fieldId} className={`ec-input ${className}`} aria-describedby={description} aria-invalid={error ? true : props['aria-invalid']}/></FormField>;
}
export function Pagination({ page, totalPages, disabled = false, onChange }: { page: number; totalPages: number; disabled?: boolean; onChange: (page: number) => void }) {
  return <nav aria-label="Phân trang" className="flex flex-wrap items-center gap-3 mt-4"><Button variant="secondary" disabled={disabled || page <= 1} onClick={() => onChange(page - 1)}><ChevronLeft className="ec-icon" aria-hidden="true"/>Trang trước</Button><span aria-live="polite" className="text-sm text-muted">Trang {page} / {Math.max(1, totalPages)}</span><Button variant="secondary" disabled={disabled || page >= totalPages} onClick={() => onChange(page + 1)}>Trang sau<ChevronRight className="ec-icon" aria-hidden="true"/></Button></nav>;
}
export function Tabs({ label, items, selected, onChange }: { label: string; items: readonly { id: string; label: string; panelId: string }[]; selected: string; onChange: (id: string) => void }) {
  return <div className="ec-tabs" role="tablist" aria-label={label}>{items.map((item, index) => <Button key={item.id} role="tab" id={`${item.panelId}-tab`} aria-controls={item.panelId} aria-selected={selected === item.id} tabIndex={selected === item.id ? 0 : -1} variant={selected === item.id ? 'primary' : 'ghost'} onClick={() => onChange(item.id)} onKeyDown={event => {
    let next: number | null = null;
    if (event.key === 'ArrowRight') next = (index + 1) % items.length;
    if (event.key === 'ArrowLeft') next = (index + items.length - 1) % items.length;
    if (event.key === 'Home') next = 0;
    if (event.key === 'End') next = items.length - 1;
    if (next !== null) { event.preventDefault(); onChange(items[next].id); document.getElementById(`${items[next].panelId}-tab`)?.focus(); }
  }}>{item.label}</Button>)}</div>;
}
export function Alert(props: Parameters<typeof Feedback>[0]) { return <Feedback {...props}/>; }
// Inline status with no auto-dismiss; errors use persistent Alert, not a toast-only path.
export function Toast({ children }: { children: string }) { return <Feedback kind="success" message={children}/>; }
