import type { ReactNode } from 'react';

export function PageHeader({ title, action }: { title: string; action?: ReactNode }) {
  return (
    <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
      <h1 className="min-w-0 break-words text-2xl font-bold">{title}</h1>
      {action}
    </div>
  );
}
