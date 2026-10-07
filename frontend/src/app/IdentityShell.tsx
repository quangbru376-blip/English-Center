import type { ReactNode } from 'react';

export function IdentityShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-[var(--ec-background)] text-[var(--ec-text)]">
      <a className="skip-link" href="#main-content">Đến nội dung chính</a>
      <header className="border-b border-[var(--ec-border)] bg-[var(--ec-surface)] px-4 py-4 sm:px-6">
        <div className="mx-auto max-w-4xl font-semibold">Trung tâm tiếng Anh</div>
      </header>
      <main id="main-content" tabIndex={-1} className="mx-auto w-full max-w-4xl px-4 py-8 sm:px-6 lg:py-12">
        {children}
      </main>
    </div>
  );
}
