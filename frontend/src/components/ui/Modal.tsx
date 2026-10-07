import { useEffect, useId, useRef } from 'react';
import type { ReactNode } from 'react';
import { Button } from './Button';

type ModalProps = {
  open: boolean;
  title: string;
  children: ReactNode;
  onRequestClose: () => void;
};

export function Modal({ open, title, children, onRequestClose }: ModalProps) {
  const dialogRef = useRef<HTMLDialogElement>(null);
  const headingId = useId();

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!open || !dialog) return;
    const opener = document.activeElement;
    dialog.showModal();
    return () => {
      dialog.close();
      if (opener instanceof HTMLElement && opener.isConnected) opener.focus();
    };
  }, [open]);

  return (
    <dialog
      ref={dialogRef}
      aria-labelledby={headingId}
      className="w-[calc(100%-2rem)] max-w-lg rounded-[var(--ec-radius)] border border-[var(--ec-border)] bg-[var(--ec-surface)] p-5 text-[var(--ec-text)] backdrop:bg-black/40"
      onCancel={(event) => { event.preventDefault(); onRequestClose(); }}
      onKeyDown={(event) => {
        if (event.key !== 'Tab') return;
        const items = Array.from(event.currentTarget.querySelectorAll<HTMLElement>(
          'button:not(:disabled), input:not(:disabled), select:not(:disabled), textarea:not(:disabled), a[href], [tabindex="0"]',
        )).filter((item) => item.getClientRects().length > 0);
        const first = items[0];
        const last = items.at(-1);
        if (event.shiftKey && document.activeElement === first && last) {
          event.preventDefault(); last.focus();
        } else if (!event.shiftKey && document.activeElement === last && first) {
          event.preventDefault(); first.focus();
        }
      }}
      onClick={(event) => {
        if (event.target !== event.currentTarget) return;
        const rect = event.currentTarget.getBoundingClientRect();
        if (event.clientX < rect.left || event.clientX > rect.right ||
            event.clientY < rect.top || event.clientY > rect.bottom) onRequestClose();
      }}
    >
      <div className="mb-4 flex items-center justify-between gap-4">
        <h2 id={headingId} className="text-lg font-semibold">{title}</h2>
        <Button variant="secondary" onClick={onRequestClose} aria-label="Đóng hộp thoại">Đóng</Button>
      </div>
      {children}
    </dialog>
  );
}
