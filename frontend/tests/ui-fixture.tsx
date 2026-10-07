import { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { IdentityShell } from '../src/app/IdentityShell';
import { PageHeader } from '../src/components/ui/PageHeader';
import { Button } from '../src/components/ui/Button';
import { TextField } from '../src/components/ui/TextField';
import { SelectField } from '../src/components/ui/SelectField';
import { Feedback } from '../src/components/ui/Feedback';
import { Modal } from '../src/components/ui/Modal';
import '../src/styles/index.css';

function Fixture() {
  const [open, setOpen] = useState(false);
  return (
    <IdentityShell>
      <PageHeader title="Kiểm chứng component U02" />
      <div className="grid gap-5">
        <TextField label="Tên hiển thị" hint="Dữ liệu kiểm chứng không được gửi." error="Ví dụ lỗi trường." defaultValue="Tên mẫu" />
        <TextField label="Chuỗi nguyên văn" type="password" defaultValue="  Á thử chuỗi  " />
        <SelectField label="Lựa chọn mẫu" hint="Chọn bằng bàn phím."><option>Mẫu một</option><option>Mẫu hai</option></SelectField>
        <Feedback kind="loading" message="Đang tải dữ liệu mẫu…" />
        <Feedback kind="empty" message="Chưa có dữ liệu mẫu." />
        <Feedback kind="no-matches" message="Không có kết quả mẫu phù hợp." />
        <Feedback kind="error" message="Lỗi minh họa để kiểm chứng." traceId="fixture-u02" />
        <Feedback kind="success" message="Đã kiểm chứng thông báo." />
        <div className="flex flex-wrap gap-3">
          <Button pending>Đang xử lý</Button>
          <Button disabled variant="secondary">Chưa khả dụng</Button>
          <Button onClick={() => setOpen(true)}>Mở hộp thoại</Button>
        </div>
      </div>
      <Modal open={open} title="Hộp thoại kiểm chứng" onRequestClose={() => setOpen(false)}>
        <TextField label="Trường trong hộp thoại" />
        <p>Chỉ kiểm chứng focus và bàn phím.</p>
      </Modal>
    </IdentityShell>
  );
}

createRoot(document.getElementById('root')!).render(<Fixture />);
