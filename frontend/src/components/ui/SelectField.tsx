import { useId } from 'react';
import type { SelectHTMLAttributes } from 'react';

export interface SelectFieldProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  hint?: string;
  error?: string;
}

const baseSelectClasses =
  'block w-full min-h-[44px] px-3 py-2 text-sm bg-[var(--ec-surface)] text-[var(--ec-text)] border rounded-[var(--ec-radius)] transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1 disabled:bg-[var(--ec-background)] disabled:text-[var(--ec-muted)] disabled:cursor-not-allowed';

export function SelectField({
  label,
  hint,
  error,
  id,
  className = '',
  children,
  ...rest
}: SelectFieldProps) {
  const generatedId = useId();
  const selectId = id || generatedId;
  const hintId = hint ? `${selectId}-hint` : undefined;
  const errorId = error ? `${selectId}-error` : undefined;

  const describedByParts = [
    rest['aria-describedby'],
    hintId,
    errorId,
  ].filter(Boolean);
  const effectiveAriaDescribedBy =
    describedByParts.length > 0 ? describedByParts.join(' ') : undefined;
  const effectiveAriaInvalid = error ? true : (rest['aria-invalid'] ?? undefined);

  const borderClasses = error
    ? 'border-[var(--ec-danger)] focus:ring-[var(--ec-danger)]'
    : 'border-[var(--ec-border)] focus:ring-[var(--ec-focus)]';
  const selectClassName = className
    ? `${baseSelectClasses} ${borderClasses} ${className}`
    : `${baseSelectClasses} ${borderClasses}`;

  return (
    <div className="w-full">
      <label
        htmlFor={selectId}
        className="block text-sm font-medium text-[var(--ec-text)] mb-1"
      >
        {label}
      </label>
      <select
        {...rest}
        id={selectId}
        aria-describedby={effectiveAriaDescribedBy}
        aria-invalid={effectiveAriaInvalid}
        className={selectClassName}
      >
        {children}
      </select>
      {hint && (
        <p id={hintId} className="mt-1 text-xs text-[var(--ec-muted)]">
          {hint}
        </p>
      )}
      {error && (
        <p id={errorId} className="mt-1 text-xs text-[var(--ec-danger)]">
          {error}
        </p>
      )}
    </div>
  );
}
