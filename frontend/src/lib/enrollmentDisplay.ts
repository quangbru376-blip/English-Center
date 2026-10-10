const registrationLabels: Record<string, string> = {
    PENDING: 'Chờ đăng ký thi', TESTING: 'Đang kiểm tra đầu vào',
    BAND_ASSIGNED: 'Đã có kết quả xếp Band', CONFIRMED: 'Đã xác nhận nhập học',
    ADMITTED: 'Đã duyệt tuyển sinh', REJECTED: 'Đã từ chối', CANCELLED: 'Đã hủy',
};
const bookingLabels: Record<string, string> = {
    SCHEDULED: 'Đang đặt', COMPLETED: 'Đã chấm điểm', ABSENT: 'Vắng thi', CANCELLED: 'Đã hủy',
};
const sessionLabels: Record<string, string> = {
    OPEN: 'Đang mở đăng ký', CLOSED: 'Đã đóng đăng ký', CANCELLED: 'Đã hủy',
};
const invoiceLabels: Record<string, string> = {
    UNPAID: 'Chưa thanh toán', PAID: 'Đã thanh toán', CANCELLED: 'Đã hủy',
};
const classLabels: Record<string, string> = {
    DRAFT: 'Bản nháp', OPEN: 'Đang nhận học viên', ONGOING: 'Đang học',
    COMPLETED: 'Đã hoàn thành', CANCELLED: 'Đã hủy',
};
export const registrationLabel = (value: string) => registrationLabels[value] ?? 'Chưa xác định';
export const bookingLabel = (value: string) => bookingLabels[value] ?? 'Chưa xác định';
export const sessionLabel = (value: string) => sessionLabels[value] ?? 'Chưa xác định';
export const invoiceLabel = (value: string) => invoiceLabels[value] ?? 'Chưa xác định';
export const classLabel = (value: string) => classLabels[value] ?? 'Chưa xác định';
export const scoreLabel = (value: string | null) => value === null ? 'Chưa có kết quả' : value.replace('.', ',') + '/100';
const vietnamDateTimeFormat = new Intl.DateTimeFormat('vi-VN', {
    timeZone: 'Asia/Ho_Chi_Minh', year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit', hourCycle: 'h23',
});
export const vietnamDateTime = (value: string) => vietnamDateTimeFormat.format(new Date(value));
