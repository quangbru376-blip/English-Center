import type { ReactElement } from 'react';
import {Button} from '../../../components/ui/Button';

export type GradeDraftField = {
  componentId: number;
  name: string;
  kind: 'GENERAL' | 'LESSON' | 'FINAL_EXAM';
  value: string;
  scheduleId: string;
  options: ReadonlyArray<{ id: number; label: string; disabled: boolean }>;
  error?: string;
};

export type GradeDraftFieldsProps = {
  fields: ReadonlyArray<GradeDraftField>;
  comment: string;
  disabled: boolean;
  dirty: boolean;
  onScore: (componentId: number, value: string) => void;
  onSchedule: (componentId: number, value: string) => void;
  onComment: (value: string) => void;
  onSave: () => void;
  onDiscard: () => void;
};

const KIND_LABEL: Record<GradeDraftField['kind'], string> = {
  GENERAL: 'Chung',
  LESSON: 'Buổi học',
  FINAL_EXAM: 'Thi cuối khóa',
};

export function GradeDraftFields(props: GradeDraftFieldsProps): ReactElement {
  const isActionDisabled = props.disabled || !props.dirty;

  return (
    <fieldset className="flex w-full max-w-full flex-col gap-4" aria-label="Điểm học viên">
      <legend className="text-base font-semibold text-[var(--ec-text)]">Nhập điểm đánh giá</legend>

      {props.fields.map((f) => (
        <div key={f.componentId} className="flex flex-col gap-1.5 border-b border-[var(--ec-border)] pb-3">
          <label htmlFor={`score-${f.componentId}`} className="text-sm font-medium text-[var(--ec-text)]">
            {f.name} ({KIND_LABEL[f.kind]})
          </label>
          <span id={`hint-${f.componentId}`} className="text-xs text-[var(--ec-text)] opacity-75">
            Để trống nghĩa là chưa có điểm và không xóa điểm đã lưu.
          </span>
          <input
            id={`score-${f.componentId}`}
            type="text"
            inputMode="decimal"
            value={f.value}
            disabled={props.disabled}
            aria-describedby={`hint-${f.componentId}${f.error ? ` err-${f.componentId}` : ''}`}
            aria-invalid={f.error ? 'true' : undefined}
            onChange={(e) => props.onScore(f.componentId, e.target.value)}
            className="min-h-[44px] w-full rounded border border-[var(--ec-border)] bg-[var(--ec-surface)] px-3 py-2 text-sm text-[var(--ec-text)] focus:outline-none focus:ring-2 focus:ring-[var(--ec-primary)] disabled:opacity-50"
          />

          {f.kind !== 'GENERAL' && (
            <div className="flex flex-col gap-1 pt-1">
              <label htmlFor={`schedule-${f.componentId}`} className="text-xs font-medium text-[var(--ec-text)]">
                Buổi học áp dụng
              </label>
              <select
                id={`schedule-${f.componentId}`}
                value={f.scheduleId}
                disabled={props.disabled}
                onChange={(e) => props.onSchedule(f.componentId, e.target.value)}
                className="min-h-[44px] w-full rounded border border-[var(--ec-border)] bg-[var(--ec-surface)] px-3 py-2 text-sm text-[var(--ec-text)] focus:outline-none focus:ring-2 focus:ring-[var(--ec-primary)] disabled:opacity-50"
              >
                <option value="">-- Chọn buổi học --</option>
                {f.options.map((opt) => (
                  <option key={opt.id} value={String(opt.id)} disabled={opt.disabled}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>
          )}

          {f.error && (
            <span id={`err-${f.componentId}`} role="alert" className="text-xs text-red-600">
              {f.error}
            </span>
          )}
        </div>
      ))}

      <div className="flex flex-col gap-1.5">
        <label htmlFor="grade-comment" className="text-sm font-medium text-[var(--ec-text)]">
          Ghi chú
        </label>
        <textarea
          id="grade-comment"
          rows={3}
          value={props.comment}
          disabled={props.disabled}
          onChange={(e) => props.onComment(e.target.value)}
          className="min-h-[44px] w-full rounded border border-[var(--ec-border)] bg-[var(--ec-surface)] px-3 py-2 text-sm text-[var(--ec-text)] focus:outline-none focus:ring-2 focus:ring-[var(--ec-primary)] disabled:opacity-50"
        />
      </div>

      <div className="flex flex-col gap-2 pt-2 sm:flex-row">
        <Button
          type="button"
          disabled={isActionDisabled}
          onClick={props.onSave}
          className="min-h-[44px] rounded bg-[var(--ec-primary)] px-4 py-2 text-sm font-medium text-white focus:outline-none focus:ring-2 focus:ring-[var(--ec-primary)] disabled:cursor-not-allowed disabled:opacity-50"
        >
          Lưu học viên
        </Button>
        <Button
          type="button"
          disabled={isActionDisabled}
          onClick={props.onDiscard}
          className="min-h-[44px] rounded border border-[var(--ec-border)] bg-[var(--ec-surface)] px-4 py-2 text-sm font-medium text-[var(--ec-text)] focus:outline-none focus:ring-2 focus:ring-[var(--ec-primary)] disabled:cursor-not-allowed disabled:opacity-50"
        >
          Bỏ thay đổi
        </Button>
      </div>
    </fieldset>
  );
}
