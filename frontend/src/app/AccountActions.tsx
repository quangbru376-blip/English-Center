import { useContext } from 'react';
import type { ReactNode } from 'react';
import { createPortal } from 'react-dom';
import { ShellSlots } from './shellSlots';
// Move the existing controls with their original callbacks/disabled conditions.
export function AccountActions({ children }: { children: ReactNode }) {
  const slots = useContext(ShellSlots);
  if (!slots) return <>{children}</>;
  return slots.account ? createPortal(<div onClick={event => {
    const button = event.target instanceof Element ? event.target.closest('button') : null;
    if (button instanceof HTMLButtonElement && !button.disabled) slots.closeAccount();
  }}>{children}</div>, slots.account) : null;
}
