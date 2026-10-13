import type { FormEvent } from 'react';
import { Button } from '../../../components/ui/Button';

export interface AdmissionReasonFormProps {
  value: string;
  disabled: boolean;
  actionLabel: string;
  onChange: (value: string) => void;
  onSubmit: () => void;
  onCancel: () => void;
}

export function AdmissionReasonForm({
  value,
  disabled,
  actionLabel,
  onChange,
  onSubmit,
  onCancel,
}: AdmissionReasonFormProps) {
  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!disabled) {
      onSubmit();
    }
  };

  return (
    <form onSubmit={handleSubmit} className="ec-work-form space-y-4">
      <div>
        <label
          htmlFor="admission-reason"
          className="mb-1 block text-sm font-medium text-[var(--ec-text)]"
        >
          Lý do
        </label>
        <textarea
          id="admission-reason"
          name="admission-reason"
          rows={4}
          maxLength={2000}
          required
          value={value}
          onChange={(e) => onChange(e.target.value)}
          disabled={disabled}
          aria-describedby="admission-reason-hint"
          className="w-full rounded-md border border-[var(--ec-border)] p-3 text-sm text-[var(--ec-text)] placeholder:text-[var(--ec-muted)] focus:border-[var(--ec-primary)] focus:outline-none focus:ring-1 focus:ring-[var(--ec-primary)] disabled:cursor-not-allowed disabled:bg-[var(--ec-surface)] disabled:text-[var(--ec-muted)]"
        />
        <p id="admission-reason-hint" className="mt-1 text-xs text-[var(--ec-muted)]">
          Lý do bắt buộc, tối đa 1.000 ký tự.
        </p>
      </div>

      <div className="flex items-center justify-end gap-3">
        <Button
          type="button"
          onClick={onCancel}
          disabled={disabled}
          className="min-h-11"
        >
          Hủy
        </Button>
        <Button
          type="submit"
          disabled={disabled}
          className="min-h-11"
        >
          {actionLabel}
        </Button>
      </div>
    </form>
  );
}
