import type { ButtonHTMLAttributes } from 'react';

export type ButtonVariant = 'primary' | 'secondary' | 'danger';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  pending?: boolean;
}

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    'bg-[var(--ec-primary)] text-white hover:bg-[var(--ec-primary-hover)] active:bg-[var(--ec-primary-hover)]',
  secondary:
    'bg-[var(--ec-surface)] text-[var(--ec-text)] border border-[var(--ec-border)] hover:bg-[var(--ec-background)] active:bg-[var(--ec-background)]',
  danger:
    'bg-[var(--ec-danger)] text-white hover:opacity-90 active:opacity-90',
};

const baseClasses =
  'inline-flex items-center justify-center gap-2 min-h-[44px] px-4 py-2 text-sm font-medium rounded-[var(--ec-radius)] transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-[var(--ec-focus)] focus-visible:ring-offset-2 disabled:opacity-60 disabled:cursor-not-allowed';

export function Button({
  type = 'button',
  variant = 'primary',
  pending = false,
  disabled = false,
  className = '',
  children,
  ...rest
}: ButtonProps) {
  const isEffectiveDisabled = Boolean(disabled || pending);
  const selectedVariantClass = variantClasses[variant] ?? variantClasses.primary;
  const composedClassName = className
    ? `${baseClasses} ${selectedVariantClass} ${className}`
    : `${baseClasses} ${selectedVariantClass}`;

  return (
    <button
      {...rest}
      type={type}
      disabled={isEffectiveDisabled}
      aria-busy={pending ? true : undefined}
      className={composedClassName}
    >
      {pending && <span className="text-xs font-normal">Đang xử lý...</span>}
      {children}
    </button>
  );
}
