import type React from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';

export type ScoreFields = {
  score: string;
  reason: string;
};

export function EntryScoreForm({
  mode,
  value,
  disabled,
  onChange,
  onSubmit,
  onCancel,
}: {
  mode: 'GRADE' | 'CORRECT_SCORE' | 'MARK_ABSENT';
  value: ScoreFields;
  disabled: boolean;
  onChange: (value: ScoreFields) => void;
  onSubmit: () => void;
  onCancel: () => void;
}) {
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!disabled) {
      onSubmit();
    }
  };

  const submitLabel =
    mode === 'GRADE'
      ? 'Lưu điểm'
      : mode === 'CORRECT_SCORE'
        ? 'Sửa điểm'
        : 'Xác nhận vắng';

  return (
    <form onSubmit={handleSubmit} className="ec-work-form space-y-4">
      {mode === 'MARK_ABSENT' ? (
        <p className="text-sm text-[var(--ec-text)]">
          Xác nhận học viên không dự thi. Vắng không phải điểm 0.
        </p>
      ) : (
        <>
          <TextField
            label="Điểm (0–100)"
            type="text"
            inputMode="decimal"
            value={value.score}
            onChange={(e) => onChange({ ...value, score: e.target.value })}
            disabled={disabled}
            required
            className="min-h-[44px]"
          />
          {mode === 'CORRECT_SCORE' ? (
            <TextField
              label="Lý do sửa điểm"
              type="text"
              value={value.reason}
              onChange={(e) => onChange({ ...value, reason: e.target.value })}
              disabled={disabled}
              required
              maxLength={2000}
              className="min-h-[44px]"
            />
          ) : null}
        </>
      )}

      <div className="flex items-center gap-3 pt-2">
        <Button type="submit" disabled={disabled} className="min-h-[44px]">
          {submitLabel}
        </Button>
        <Button
          type="button"
          onClick={onCancel}
          disabled={disabled}
          className="min-h-[44px]"
        >
          Hủy
        </Button>
      </div>
    </form>
  );
}
