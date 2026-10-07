export type FeedbackKind =
  | 'loading'
  | 'empty'
  | 'no-matches'
  | 'error'
  | 'success';

export interface FeedbackProps {
  kind: FeedbackKind;
  message: string;
  traceId?: string;
  className?: string;
}

const defaultHeadings: Partial<Record<FeedbackKind, string>> = {
  empty: 'Chưa có dữ liệu',
  'no-matches': 'Không tìm thấy kết quả phù hợp',
};

const kindContainerClasses: Record<FeedbackKind, string> = {
  loading: 'border-[var(--ec-border)] bg-[var(--ec-surface)]',
  empty: 'border-[var(--ec-border)] bg-[var(--ec-surface)]',
  'no-matches': 'border-[var(--ec-border)] bg-[var(--ec-surface)]',
  error: 'border-[var(--ec-danger)] bg-[var(--ec-surface)]',
  success: 'border-[var(--ec-success)] bg-[var(--ec-surface)]',
};

const messageTextClasses: Record<FeedbackKind, string> = {
  loading: 'text-[var(--ec-muted)]',
  empty: 'text-[var(--ec-muted)]',
  'no-matches': 'text-[var(--ec-muted)]',
  error: 'text-[var(--ec-danger)]',
  success: 'text-[var(--ec-success)]',
};

export function Feedback({
  kind,
  message,
  traceId,
  className = '',
}: FeedbackProps) {
  const role =
    kind === 'error'
      ? 'alert'
      : kind === 'loading' || kind === 'success'
        ? 'status'
        : undefined;

  const heading = defaultHeadings[kind];
  const containerClass = `rounded-[var(--ec-radius)] p-4 border ${kindContainerClasses[kind]} ${className}`.trim();

  return (
    <div role={role} className={containerClass}>
      {heading && (
        <h3 className="text-sm font-semibold text-[var(--ec-text)] mb-1">
          {heading}
        </h3>
      )}
      <p className={`text-sm ${messageTextClasses[kind]}`}>{message}</p>
      {traceId && (
        <p className="mt-2 text-xs font-mono text-[var(--ec-muted)]">
          Mã theo dõi: {traceId}
        </p>
      )}
    </div>
  );
}
