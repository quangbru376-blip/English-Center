import { useId } from 'react';
import type { InputHTMLAttributes } from 'react';

export interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  hint?: string;
  error?: string;
}

const baseInputClasses =
  'block w-full min-h-[44px] px-3 py-2 text-sm bg-[var(--ec-surface)] text-[var(--ec-text)] border rounded-[var(--ec-radius)] transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1 disabled:bg-[var(--ec-background)] disabled:text-[var(--ec-muted)] disabled:cursor-not-allowed';

export function TextField({
  label,
  hint,
  error,
  id,
  className = '',
  ...rest
}: TextFieldProps) {
  const generatedId = useId();
  const inputId = id || generatedId;
  const hintId = hint ? `${inputId}-hint` : undefined;
  const errorId = error ? `${inputId}-error` : undefined;

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
  const inputClassName = className
    ? `${baseInputClasses} ${borderClasses} ${className}`
    : `${baseInputClasses} ${borderClasses}`;

  return (
    <div className="w-full">
      <label
        htmlFor={inputId}
        className="block text-sm font-medium text-[var(--ec-text)] mb-1"
      >
        {label}
      </label>
      <input
        {...rest}
        id={inputId}
        aria-describedby={effectiveAriaDescribedBy}
        aria-invalid={effectiveAriaInvalid}
        className={inputClassName}
      />
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
