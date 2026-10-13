import { ArrowRight, BookOpen, CalendarDays, ClipboardCheck, Receipt } from 'lucide-react';
import { Button } from '../../../components/ui/Button';
import { Badge, Card, EmptyState } from '../../../components/ui/Surfaces';
import { registrationLabel, vietnamDateTime } from '../../../lib/enrollmentDisplay';
import type { Page } from '../../../lib/m1Wire';
import type { CourseChoice, Home } from '../enrollmentApi';

const money = (value: string) => value.replace(/\B(?=(\d{3})+(?!\d))/g, '.') + ' ₫';
const nextLabels: Record<string, string> = {
  BOOK_ENTRY_TEST: 'Xem bước đặt lịch', VIEW_ENTRY_RESULTS: 'Xem kết quả đầu vào',
  CONFIRM_REGISTRATION: 'Xem bước xác nhận nhập học', VIEW_INVOICE: 'Xem hóa đơn',
  VIEW_CLASS: 'Xem lớp của tôi', VIEW_FINALIZED_REPORT: 'Xem kết quả đã công bố',
  CANCEL_ENTRY_BOOKING: 'Xem lượt đặt', RESCHEDULE_ENTRY_BOOKING: 'Xem lịch đã đặt',
  CANCEL_REGISTRATION: 'Xem hồ sơ đăng ký',
};

export function StudentHome({ data, busy, read, changePage, chooseCourses, openNext, openReport }: {
  data: Page<Home>; busy: boolean; read: (id: number) => void; changePage: (page: number) => void;
  chooseCourses: () => void; openNext: (item: Home) => void; openReport?: (id: number) => void;
}) {
  return <section aria-label="Hồ sơ đăng ký của tôi">
    <div className="student-list-heading"><div><h2>Hành trình học tập của bạn</h2><p className="text-muted">{data.totalCount} hồ sơ — trang {data.page}</p></div>
      <Button variant="secondary" disabled={busy} onClick={chooseCourses}><BookOpen className="ec-icon" aria-hidden="true"/>Chọn khóa học mới</Button></div>
    {!data.items.length ? <EmptyState title="Bắt đầu với một khóa học" action={<Button disabled={busy} onClick={chooseCourses}>Khám phá khóa học</Button>}>Chưa có hồ sơ. Chọn khóa học để đăng ký.</EmptyState> :
      <ul className="student-grid">{data.items.map(item => <li key={item.registrationId}>
        <Card className="student-registration-card">
          <div className="student-card-heading"><span className="student-eyebrow">Hồ sơ {item.registrationId} · {item.course.code}</span><Badge tone={item.status === 'CANCELLED' || item.status === 'REJECTED' ? 'neutral' : 'info'}>{registrationLabel(item.status)}</Badge></div>
          <h3>{item.course.name}</h3><p className="student-step">{item.currentStep}</p>
          {item.waitingReason && <p className="student-waiting">{item.waitingReason}</p>}
          <dl className="student-facts">
            {item.nearestEvent && <div><dt><CalendarDays className="ec-icon" aria-hidden="true"/>{item.nearestEvent.kind === 'ENTRY_TEST' ? 'Ca kiểm tra đầu vào' : 'Buổi học gần nhất'}</dt><dd>{vietnamDateTime(item.nearestEvent.startAt)}<span className="student-fact-note">Phòng {item.nearestEvent.room}</span></dd></div>}
            {item.unpaidInvoice && <div><dt><Receipt className="ec-icon" aria-hidden="true"/>Học phí chưa thanh toán</dt><dd>{money(item.unpaidInvoice.amount)}<span className="student-fact-note">{item.unpaidInvoice.number} · Hạn {item.unpaidInvoice.dueDate.split('-').reverse().join('/')}</span>{item.unpaidInvoice.isOverdue && <Badge tone="warning">Quá hạn thanh toán</Badge>}</dd></div>}
            {item.finalizedReport && <div><dt><ClipboardCheck className="ec-icon" aria-hidden="true"/>Kết quả đã công bố</dt><dd>Báo cáo {item.finalizedReport.gradeReportId} · Lớp {item.finalizedReport.classId}</dd></div>}
          </dl>
          <div className="student-card-actions">
            {item.nextAction && <Button disabled={busy} onClick={() => openNext(item)}>{nextLabels[item.nextAction.action] ?? 'Xem bước tiếp theo'}<ArrowRight className="ec-icon" aria-hidden="true"/></Button>}
            <Button variant="secondary" disabled={busy} onClick={() => read(item.registrationId)}>Xem hồ sơ {item.registrationId}</Button>
            {item.finalizedReport && openReport && item.nextAction?.action !== 'VIEW_FINALIZED_REPORT' && <Button variant="ghost" disabled={busy} onClick={() => openReport(item.finalizedReport!.gradeReportId)}>Xem kết quả {item.finalizedReport.gradeReportId}</Button>}
          </div>
        </Card>
      </li>)}</ul>}
    <div className="student-pagination" aria-label="Phân trang hồ sơ"><Button variant="secondary" disabled={busy || data.page === 1} onClick={() => changePage(data.page - 1)}>Hồ sơ trang trước</Button><span>Trang {data.page} / {Math.max(1, Math.ceil(data.totalCount / data.pageSize))}</span><Button variant="secondary" disabled={busy || data.page * data.pageSize >= data.totalCount} onClick={() => changePage(data.page + 1)}>Hồ sơ trang sau</Button></div>
  </section>;
}

export function StudentCourses({ data, busy, blocked, register, changePage }: {
  data: Page<CourseChoice>; busy: boolean; blocked: boolean; register: (course: CourseChoice) => void; changePage: (page: number) => void;
}) {
  return <section aria-label="Khóa học đang mở"><div className="student-list-heading"><div><h2>Chọn hướng học phù hợp</h2><p className="text-muted">{data.totalCount} khóa học — trang {data.page}</p></div></div>
    {!data.items.length ? <EmptyState title="Chưa có khóa học đang mở">Bạn có thể quay lại sau hoặc liên hệ trung tâm để được hướng dẫn.</EmptyState> :
      <ul className="student-grid">{data.items.map(course => <li key={course.id}><Card className="student-course-card">
        <div className="student-card-heading"><span className="student-course-icon"><BookOpen className="ec-icon" aria-hidden="true"/></span><Badge tone="success">Đang mở đăng ký</Badge></div>
        <span className="student-eyebrow">{course.code}</span><h3>{course.name}</h3>
        <dl className="student-facts"><div><dt>Học phí hiện tại</dt><dd className="student-price">{money(course.tuition)}</dd></div></dl>
        <p className="text-muted text-sm">Đăng ký để tạo hồ sơ và chọn lịch kiểm tra đầu vào. Hóa đơn được lập khi hồ sơ được duyệt.</p>
        <Button disabled={blocked} onClick={() => register(course)}>Đăng ký khóa {course.code}<ArrowRight className="ec-icon" aria-hidden="true"/></Button>
      </Card></li>)}</ul>}
    <div className="student-pagination" aria-label="Phân trang khóa học"><Button variant="secondary" disabled={busy || data.page === 1} onClick={() => changePage(data.page - 1)}>Khóa trang trước</Button><span>Trang {data.page} / {Math.max(1, Math.ceil(data.totalCount / data.pageSize))}</span><Button variant="secondary" disabled={busy || data.page * data.pageSize >= data.totalCount} onClick={() => changePage(data.page + 1)}>Khóa trang sau</Button></div>
  </section>;
}
