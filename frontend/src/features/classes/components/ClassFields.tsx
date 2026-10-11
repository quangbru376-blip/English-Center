import type { FormEvent } from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';

export interface ClassFieldsProps {
  code: string;
  programId: string;
  teacherId: string;
  maxSize: string;
  programs: ReadonlyArray<{ id: number; label: string }>;
  teachers: ReadonlyArray<{ id: number; label: string; disabled: boolean }>;
  disabled: boolean;
  onCode: (value: string) => void;
  onProgram: (value: string) => void;
  onTeacher: (value: string) => void;
  onMaxSize: (value: string) => void;
  onSubmit: () => void;
  onCancel: () => void;
}

export function ClassFields({
  code, programId, teacherId, maxSize,
  programs, teachers, disabled,
  onCode, onProgram, onTeacher, onMaxSize,
  onSubmit, onCancel,
}: ClassFieldsProps) {
  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!disabled) onSubmit();
  };

  return (
    <form onSubmit={handleSubmit} className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <TextField
        id="code"
        name="code"
        label="Mã lớp"
        value={code}
        onChange={(e) => onCode(e.target.value)}
        required
        disabled={disabled}
      />

      <div className="flex flex-col gap-1">
        <label htmlFor="programId" className="text-sm font-medium text-[var(--ec-text)]">
          Chương trình
        </label>
        <select
          id="programId"
          name="programId"
          value={programId}
          onChange={(e) => onProgram(e.target.value)}
          required
          disabled={disabled}
          className="min-h-11 w-full rounded border border-[var(--ec-border)] bg-[var(--ec-surface)] px-3 py-2 text-sm text-[var(--ec-text)] focus:outline-none focus:ring-2 focus:ring-[var(--ec-focus)] disabled:opacity-60"
        >
          <option value="">Chọn chương trình</option>
          {programs.map((p) => (
            <option key={p.id} value={p.id}>{p.label}</option>
          ))}
        </select>
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="teacherId" className="text-sm font-medium text-[var(--ec-text)]">
          Giáo viên
        </label>
        <select
          id="teacherId"
          name="teacherId"
          value={teacherId}
          onChange={(e) => onTeacher(e.target.value)}
          required
          disabled={disabled}
          className="min-h-11 w-full rounded border border-[var(--ec-border)] bg-[var(--ec-surface)] px-3 py-2 text-sm text-[var(--ec-text)] focus:outline-none focus:ring-2 focus:ring-[var(--ec-focus)] disabled:opacity-60"
        >
          <option value="">Chọn giáo viên</option>
          {teachers.map((t) => (
            <option key={t.id} value={t.id} disabled={t.disabled}>{t.label}</option>
          ))}
        </select>
      </div>

      <TextField
        id="maxSize"
        name="maxSize"
        label="Sĩ số tối đa"
        type="text"
        inputMode="numeric"
        value={maxSize}
        onChange={(e) => onMaxSize(e.target.value)}
        required
        disabled={disabled}
      />

      <div className="flex items-center gap-3 sm:col-span-2">
        <Button type="submit" disabled={disabled}>
          Chuẩn bị tạo lớp
        </Button>
        <Button type="button" disabled={disabled} onClick={() => !disabled && onCancel()}>
          Hủy
        </Button>
      </div>
    </form>
  );
}
