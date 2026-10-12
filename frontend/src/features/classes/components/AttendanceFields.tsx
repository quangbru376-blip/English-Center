import { Button } from '../../../components/ui/Button';

export type AttendanceFieldsProps = {
  rows: readonly {
    classAssignmentId: number;
    fullName: string;
    present: 0 | 1 | null;
  }[];
  disabled: boolean;
  onChange: (id: number, value: 0 | 1 | null) => void;
  onSave: () => void;
  onCancel: () => void;
};

export function AttendanceFields({
  rows,
  disabled,
  onChange,
  onSave,
  onCancel,
}: AttendanceFieldsProps) {
  return (
    <div
      className="flex w-full flex-col gap-4"
      style={{ color: 'var(--ec-text)' }}
    >
      {rows.length === 0 ? (
        <p
          className="rounded border p-4 text-center text-sm"
          style={{
            borderColor: 'var(--ec-border)',
            backgroundColor: 'var(--ec-surface)',
          }}
        >
          Chưa có học viên trong lớp.
        </p>
      ) : (
        <div className="flex flex-col gap-3">
          {rows.map((row) => (
            <div
              key={row.classAssignmentId}
              className="flex flex-col gap-1.5 rounded border p-3"
              style={{
                borderColor: 'var(--ec-border)',
                backgroundColor: 'var(--ec-surface)',
              }}
            >
              <label
                htmlFor={`attendance-${row.classAssignmentId}`}
                className="text-sm font-medium"
              >
                <span>Điểm danh </span>
                <span className="font-semibold">{row.fullName}</span>
              </label>
              <select
                id={`attendance-${row.classAssignmentId}`}
                aria-label={`Điểm danh ${row.fullName}`}
                value={row.present === 1 ? '1' : row.present === 0 ? '0' : ''}
                disabled={disabled}
                onChange={(e) => {
                  const val = e.target.value;
                  const parsed = val === '1' ? 1 : val === '0' ? 0 : null;
                  onChange(row.classAssignmentId, parsed);
                }}
                className="min-h-11 w-full rounded border px-3 py-2 text-sm focus:outline-none focus:ring-2 disabled:cursor-not-allowed disabled:opacity-50"
                style={{
                  borderColor: 'var(--ec-border)',
                  backgroundColor: 'var(--ec-surface)',
                  color: 'var(--ec-text)',
                }}
              >
                <option value="">Chưa ghi</option>
                <option value="1">Có mặt</option>
                <option value="0">Vắng</option>
              </select>
            </div>
          ))}
        </div>
      )}

      <div className="flex flex-wrap items-center gap-3 pt-2">
        <Button disabled={disabled} onClick={onSave}>Lưu điểm danh</Button>
        <Button variant="secondary" disabled={disabled} onClick={onCancel}>Bỏ thay đổi</Button>
      </div>
    </div>
  );
}
