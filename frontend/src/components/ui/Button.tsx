import type { ButtonHTMLAttributes } from 'react';

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  pending?: boolean;
  size?: 'normal' | 'large';
}

const variantClasses: Record<ButtonVariant, string> = {
  primary:
    'ec-button--primary',
  secondary:
    'ec-button--secondary',
  danger:
    'ec-button--danger',
  ghost: 'ec-button--ghost',
};

const baseClasses =
  'ec-button';

export function Button({
  type = 'button',
  variant = 'primary',
  pending = false,
  size = 'normal',
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
      className={`${composedClassName}${size === 'large' ? ' ec-button--large' : ''}`}
    >
      {pending && <span className="text-xs font-normal">Đang xử lý...</span>}
      {children}
    </button>
  );
}
