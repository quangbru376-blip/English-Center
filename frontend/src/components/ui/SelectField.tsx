import { useId } from 'react';
import type { SelectHTMLAttributes } from 'react';

export interface SelectFieldProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  hint?: string;
  error?: string;
}

const baseSelectClasses =
  'ec-input';

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
        className="ec-label"
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
        <p id={hintId} className="ec-field-hint">
          {hint}
        </p>
      )}
      {error && (
        <p id={errorId} className="ec-field-error">
          {error}
        </p>
      )}
    </div>
  );
}
