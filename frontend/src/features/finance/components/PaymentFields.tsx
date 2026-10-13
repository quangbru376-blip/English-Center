import type { FormEvent } from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';

export interface PaymentFieldsProps {
  method: 'CASH' | 'TRANSFER';
  reference: string;
  reason: string;
  correction: boolean;
  disabled: boolean;
  onMethod: (value: 'CASH' | 'TRANSFER') => void;
  onReference: (value: string) => void;
  onReason: (value: string) => void;
  onSubmit: () => void;
  onCancel: () => void;
}

export function PaymentFields({
  method, reference, reason, correction, disabled,
  onMethod, onReference, onReason, onSubmit, onCancel,
}: PaymentFieldsProps) {
  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!disabled) onSubmit();
  };

  return (
    <form onSubmit={handleSubmit} className="ec-work-form space-y-4">
      <div>
        <label htmlFor="payment-method" className="block text-sm font-medium mb-1" style={{ color: 'var(--ec-text)' }}>
          Phương thức
        </label>
        <select
          id="payment-method"
          value={method}
          onChange={(e) => onMethod(e.target.value as 'CASH' | 'TRANSFER')}
          disabled={disabled}
          className="w-full min-h-11 px-3 py-2 rounded border text-sm"
          style={{ borderColor: 'var(--ec-border)', backgroundColor: 'var(--ec-surface)', color: 'var(--ec-text)' }}
        >
          <option value="CASH">Tiền mặt</option>
          <option value="TRANSFER">Chuyển khoản</option>
        </select>
      </div>

      <div>
        <TextField
          id="payment-reference"
          label="Mã giao dịch"
          value={reference}
          onChange={(e) => onReference(e.target.value)}
          disabled={disabled}
          required={method === 'TRANSFER'}
          className="min-h-11"
        />
        <p className="text-xs mt-1" style={{ color: 'var(--ec-text)' }}>≤ 100 ký tự</p>
      </div>

      {correction && (
        <div>
          <label htmlFor="payment-reason" className="block text-sm font-medium mb-1" style={{ color: 'var(--ec-text)' }}>
            Lý do sửa
          </label>
          <textarea
            id="payment-reason"
            rows={3}
            value={reason}
            onChange={(e) => onReason(e.target.value)}
            disabled={disabled}
            required
            className="w-full min-h-11 px-3 py-2 rounded border text-sm"
            style={{ borderColor: 'var(--ec-border)', backgroundColor: 'var(--ec-surface)', color: 'var(--ec-text)' }}
          />
          <p className="text-xs mt-1" style={{ color: 'var(--ec-text)' }}>≤ 1.000 ký tự</p>
        </div>
      )}

      <div className="flex items-center gap-3 pt-2">
        <Button type="submit" disabled={disabled} className="min-h-11">
          Chuẩn bị xác nhận
        </Button>
        <Button type="button" disabled={disabled} onClick={onCancel} className="min-h-11">
          Hủy
        </Button>
      </div>
    </form>
  );
}
