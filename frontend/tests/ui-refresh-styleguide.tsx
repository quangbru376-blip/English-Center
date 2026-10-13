import { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { Check, BookOpen } from 'lucide-react';
import { Brand } from '../src/components/ui/Brand';
import { Button } from '../src/components/ui/Button';
import { TextField } from '../src/components/ui/TextField';
import { SelectField } from '../src/components/ui/SelectField';
import { Modal } from '../src/components/ui/Modal';
import { PageHeader } from '../src/components/ui/PageHeader';
import { Card, Badge, Avatar, EmptyState, Skeleton, Table, Textarea, Pagination, Tabs, Alert, Toast } from '../src/components/ui/Surfaces';
import '../src/styles/index.css';

function Styleguide() {
  const [dialog, setDialog] = useState(false), [tab, setTab] = useState('overview'), [page, setPage] = useState(1);
  return <main className="ec-styleguide"><Brand/><div className="mt-8"><PageHeader title="Một nền giao diện. Mọi màn hình."/><p className="text-muted mb-8">Styleguide dành cho phát triển — toàn bộ nội dung dưới đây là dữ liệu minh họa.</p></div><div className="ec-styleguide-grid">
    <Card><h2 className="text-xl font-semibold mb-4">Màu và trạng thái</h2><div className="ec-styleguide-row"><Badge>Thương hiệu teal</Badge><Badge tone="success">Đã hoàn tất</Badge><Badge tone="warning">Đang chờ</Badge><Badge tone="danger">Có lỗi</Badge><Badge tone="info">Chỉ xem</Badge></div><p className="mt-4 text-muted">Màu vàng nắng tạo điểm nhấn. Trạng thái luôn có nhãn chữ.</p></Card>
    <Card><h2 className="text-xl font-semibold mb-4">Nút và icon</h2><div className="ec-styleguide-row"><Button><Check className="ec-icon" aria-hidden="true"/>Lưu thay đổi</Button><Button variant="secondary">Quay lại</Button><Button variant="ghost">Xem thêm</Button><Button variant="danger">Hủy hồ sơ</Button><Button pending>Đang gửi</Button><Button disabled>Chưa đủ điều kiện</Button><Button size="large"><BookOpen className="ec-icon" aria-hidden="true"/>Chọn khóa học</Button></div></Card>
    <Card><h2 className="text-xl font-semibold mb-4">Form có nhãn và phản hồi</h2><div className="grid gap-4"><TextField label="Họ và tên" defaultValue="Nguyễn Minh Anh" hint="Tên có dấu tiếng Việt, tối đa 100 ký tự."/><TextField label="Email" type="email" defaultValue="" error="Vui lòng nhập email."/><TextField label="Mật khẩu" type="password" hint="Giữ nguyên ký tự và khoảng trắng đã nhập."/><SelectField label="Khóa học"><option>Tiếng Anh giao tiếp</option><option>IELTS</option></SelectField><Textarea label="Nhận xét" rows={3} hint="Văn bản thuần, tối đa 1.000 ký tự."/></div></Card>
    <Card><h2 className="text-xl font-semibold mb-4">Loading / lỗi / thành công</h2><Skeleton/><Alert kind="loading" message="Đang đọc dữ liệu…"/><Alert kind="error" message="Dữ liệu đã thay đổi. Hãy tải lại và kiểm tra." traceId="minh-hoa"/><Toast>Đã lưu thay đổi.</Toast><p className="text-muted mt-4">Lỗi quan trọng giữ trên màn hình; thao tác unknown phải có đối chiếu thủ công.</p></Card>
    <Card><EmptyState title="Chưa có hồ sơ" action={<Button>Chọn khóa học</Button>}>Bạn có thể chọn một khóa học để bắt đầu.</EmptyState></Card>
    <Card><h2 className="text-xl font-semibold mb-4">Tabs, avatar và dialog</h2><Tabs label="Các phần minh họa" items={[{id:'overview',label:'Tổng quan',panelId:'overview-panel'},{id:'details',label:'Chi tiết',panelId:'details-panel'}]} selected={tab} onChange={setTab}/><div role="tabpanel" id={`${tab}-panel`} aria-labelledby={`${tab}-panel-tab`} tabIndex={0}><p>{tab === 'overview' ? 'Tổng quan dữ liệu đã lưu.' : 'Chi tiết đúng phạm vi được xem.'}</p></div><div className="ec-styleguide-row mt-6"><Avatar name="Nguyễn Minh Anh"/><span>Nguyễn Minh Anh</span><Button variant="secondary" onClick={() => setDialog(true)}>Mở hộp thoại mẫu</Button></div><Modal open={dialog} title="Xác nhận thay đổi" onRequestClose={() => setDialog(false)}><p>Đối tượng và ảnh hưởng cần hiển thị rõ trước thao tác.</p><div className="ec-styleguide-row mt-6"><Button onClick={() => setDialog(false)}>Xác nhận</Button><Button variant="secondary" onClick={() => setDialog(false)}>Quay lại</Button></div></Modal></Card>
    <Card className="md:col-span-2"><h2 className="text-xl font-semibold mb-4">Bảng và phân trang</h2><Table caption="Các hồ sơ minh họa"><thead><tr><th>Khóa học</th><th>Trạng thái</th><th className="text-right">Học phí</th></tr></thead><tbody><tr><td>Tiếng Anh giao tiếp</td><td><Badge tone="success">Đã thanh toán</Badge></td><td className="text-right">3.500.000 ₫</td></tr><tr><td>IELTS nền tảng</td><td><Badge tone="warning">Chưa thanh toán</Badge></td><td className="text-right">4.800.000 ₫</td></tr></tbody></Table><Pagination page={page} totalPages={3} onChange={setPage}/></Card>
  </div></main>;
}
// Separate test HTML entry, excluded from normal builds and refused outside dev.
if (import.meta.env.DEV) createRoot(document.getElementById('root')!).render(<Styleguide/>);
