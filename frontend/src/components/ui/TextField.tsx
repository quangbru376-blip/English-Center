import { useId } from 'react';
import type { InputHTMLAttributes } from 'react';

export interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  hint?: string;
  error?: string;
}

const baseInputClasses =
  'ec-input';

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
        className="ec-label"
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
