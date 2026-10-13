import { CalendarDays, Mail, MapPin, Phone, UserRound } from 'lucide-react';
import { Badge, Card, EmptyState } from '../../../components/ui/Surfaces';
import { Button } from '../../../components/ui/Button';
import { bookingLabel, vietnamDateTime } from '../../../lib/enrollmentDisplay';
import type { Page } from '../../../lib/m1Wire';
import type { Booking, Profile } from '../enrollmentApi';

export function StudentProfileSummary({ profile, busy, editing, edit }: { profile: Profile; busy: boolean; editing: boolean; edit: () => void }) {
  return <Card className="student-detail"><div className="student-card-heading"><div><span className="student-eyebrow">Thông tin tài khoản học viên</span><h2>{profile.fullName}</h2></div><UserRound className="ec-icon" aria-hidden="true"/></div>
    <dl className="student-summary"><div><dt><Mail className="ec-icon" aria-hidden="true"/>Email đăng nhập</dt><dd>{profile.email}</dd></div><div><dt><CalendarDays className="ec-icon" aria-hidden="true"/>Ngày sinh</dt><dd>{profile.dateOfBirth.split('-').reverse().join('/')}</dd></div><div><dt><Phone className="ec-icon" aria-hidden="true"/>Số điện thoại</dt><dd>{profile.phone}</dd></div><div><dt><MapPin className="ec-icon" aria-hidden="true"/>Địa chỉ</dt><dd>{profile.address ?? 'Chưa có địa chỉ'}</dd></div></dl>
    <p className="text-muted text-sm">Bạn có thể sửa họ tên, ngày sinh, số điện thoại và địa chỉ. Email đăng nhập giữ nguyên.</p>
    {!editing && <div><Button variant="secondary" disabled={busy} onClick={edit}>Sửa thông tin cá nhân</Button></div>}
  </Card>;
}

export function StudentBookings({ data, busy, read, changePage }: { data: Page<Booking>; busy: boolean; read: (id: number) => void; changePage: (page: number) => void }) {
  return <section aria-label="Các lượt đặt thi"><div className="student-list-heading"><h2>Các lượt đặt của bạn</h2><span className="text-muted text-sm">{data.totalCount} lượt đặt — trang {data.page}</span></div>
    {!data.items.length ? <EmptyState title="Chưa có lịch thi phù hợp">Không có lượt đặt trong khoảng đã chọn. Bạn có thể đổi bộ lọc hoặc mở hồ sơ để chọn ca.</EmptyState> : <ul className="student-grid">{data.items.map(booking => <li key={booking.id}><Card className="student-registration-card">
      <div className="student-card-heading"><span className="student-eyebrow">Hồ sơ {booking.registrationId} · Lượt đặt {booking.id}</span><Badge tone={booking.status==='SCHEDULED'?'info':booking.status==='COMPLETED'?'success':booking.status==='ABSENT'?'warning':'neutral'}>{bookingLabel(booking.status)}</Badge></div>
      <h3>Lượt thi {booking.attemptNumber}</h3><dl className="student-facts"><div><dt><CalendarDays className="ec-icon" aria-hidden="true"/>Giờ kiểm tra đầu vào</dt><dd>{vietnamDateTime(booking.session.startAt)}<span className="student-fact-note">Đến {vietnamDateTime(booking.session.endAt)} · Phòng {booking.session.room}</span></dd></div></dl>
      <Button variant="secondary" disabled={busy} onClick={()=>read(booking.registrationId)}>Xem hồ sơ {booking.registrationId}</Button>
    </Card></li>)}</ul>}
    <div className="student-pagination" aria-label="Phân trang lịch thi"><Button variant="secondary" disabled={busy||data.page===1} onClick={()=>changePage(data.page-1)}>Lịch trang trước</Button><span>Trang {data.page} / {Math.max(1,Math.ceil(data.totalCount/data.pageSize))}</span><Button variant="secondary" disabled={busy||data.page*data.pageSize>=data.totalCount} onClick={()=>changePage(data.page+1)}>Lịch trang sau</Button></div>
  </section>;
}
