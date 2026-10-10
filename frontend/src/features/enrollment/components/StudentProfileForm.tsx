import type { FormEvent } from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';

export type ProfileFields = {
  fullName: string;
  dateOfBirth: string;
  phone: string;
  address: string;
};

export type StudentProfileFormProps = {
  value: ProfileFields;
  disabled: boolean;
  onChange: (value: ProfileFields) => void;
  onSubmit: () => void;
  onCancel: () => void;
};

export function StudentProfileForm({
  value,
  disabled,
  onChange,
  onSubmit,
  onCancel,
}: StudentProfileFormProps) {
  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!disabled) {
      onSubmit();
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <TextField
        id="fullName"
        name="fullName"
        label="Họ và tên"
        value={value.fullName}
        onChange={(e) => onChange({ ...value, fullName: e.target.value })}
        required
        maxLength={200}
        disabled={disabled}
      />

      <TextField
        id="dateOfBirth"
        name="dateOfBirth"
        type="date"
        label="Ngày sinh"
        value={value.dateOfBirth}
        onChange={(e) => onChange({ ...value, dateOfBirth: e.target.value })}
        required
        disabled={disabled}
      />

      <TextField
        id="phone"
        name="phone"
        type="tel"
        label="Số điện thoại"
        value={value.phone}
        onChange={(e) => onChange({ ...value, phone: e.target.value })}
        required
        maxLength={30}
        disabled={disabled}
      />

      <TextField
        id="address"
        name="address"
        label="Địa chỉ"
        value={value.address}
        onChange={(e) => onChange({ ...value, address: e.target.value })}
        maxLength={510}
        disabled={disabled}
      />

      <div className="flex items-center gap-3 pt-2">
        <Button type="submit" disabled={disabled} className="min-h-[44px]">
          Lưu
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
