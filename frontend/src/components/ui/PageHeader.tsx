import type { ReactNode } from 'react';

export function PageHeader({ title, action }: { title: string; action?: ReactNode }) {
  return (
    <div className="ec-page-header">
      <h1>{title}</h1>
      {action}
    </div>
  );
}
