import { useState, type FormEvent } from 'react';
import { TextField } from '../../../components/ui/TextField';
import { Button } from '../../../components/ui/Button';

export interface CourseFormProps {
  initial?: {
    code: string;
    name: string;
    basePrice: string;
    status: 'ACTIVE' | 'INACTIVE';
  };
  editing: boolean;
  pending: boolean;
  onSubmit: (value: {
    code: string;
    name: string;
    basePrice: string;
    status: 'ACTIVE' | 'INACTIVE';
  }) => void;
  onCancel: () => void;
}

export function CourseForm({
  initial,
  editing,
  pending,
  onSubmit,
  onCancel,
}: CourseFormProps) {
  const [code, setCode] = useState(initial?.code ?? '');
  const [name, setName] = useState(initial?.name ?? '');
  const [basePrice, setBasePrice] = useState(initial?.basePrice ?? '');
  const [status, setStatus] = useState<'ACTIVE' | 'INACTIVE'>(initial?.status ?? 'ACTIVE');
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (pending) return;
    setError(null);

    const trimmedCode = code.trim().toUpperCase();
    if (!/^[A-Z0-9_-]{1,30}$/.test(trimmedCode)) {
      setError('Mã khóa học phải từ 1 đến 30 ký tự (chữ hoa, số, dấu gạch dưới hoặc gạch ngang).');
      return;
    }

    const trimmedName = name.trim();
    const nameLength = Array.from(trimmedName).length;
    if (nameLength === 0 || nameLength > 150) {
      setError('Tên khóa học không được để trống và tối đa 150 ký tự.');
      return;
    }

    const trimmedPrice = basePrice.trim();
    if (!/^[1-9]\d{0,17}$/.test(trimmedPrice)) {
      setError('Học phí cơ bản phải là số nguyên dương tối đa 18 chữ số và không có số 0 ở đầu.');
      return;
    }

    onSubmit({
      code: trimmedCode,
      name: trimmedName,
      basePrice: trimmedPrice,
      status,
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4 rounded-[var(--ec-radius)] border border-[var(--ec-border)] bg-[var(--ec-surface)] p-4">
      {error && (
        <div role="alert" className="rounded-[var(--ec-radius)] border border-[var(--ec-danger)] p-3 text-sm text-[var(--ec-danger)]">
          {error}
        </div>
      )}

      <TextField
        id="course-code"
        label="Mã khóa học"
        value={code}
        onChange={(e) => setCode(e.target.value)}
        readOnly={editing}
        disabled={pending}
      />

      <TextField
        id="course-name"
        label="Tên khóa học"
        value={name}
        onChange={(e) => setName(e.target.value)}
        disabled={pending}
      />

      <TextField
        id="course-base-price"
        label="Học phí cơ bản (VND)"
        value={basePrice}
        onChange={(e) => setBasePrice(e.target.value)}
        disabled={pending}
      />

      {!editing && (
        <div>
          <label htmlFor="course-status" className="mb-1 block text-sm font-medium text-[var(--ec-text)]">
            Trạng thái
          </label>
          <select
            id="course-status"
            value={status}
            onChange={(e) => setStatus(e.target.value as 'ACTIVE' | 'INACTIVE')}
            disabled={pending}
            className="min-h-[44px] w-full rounded-[var(--ec-radius)] border border-[var(--ec-border)] bg-[var(--ec-surface)] px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-[var(--ec-focus)]"
          >
            <option value="ACTIVE">Hoạt động</option>
            <option value="INACTIVE">Không hoạt động</option>
          </select>
        </div>
      )}

      <div className="flex justify-end gap-3 pt-2">
        <Button type="button" onClick={onCancel} disabled={pending}>
          Hủy
        </Button>
        <Button type="submit" disabled={pending}>
          {editing ? 'Lưu' : 'Tạo mới'}
        </Button>
      </div>
    </form>
  );
}
