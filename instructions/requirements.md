# Yêu cầu hệ thống — Quản lý trung tâm Anh ngữ

Nguồn sự thật cho mọi kế hoạch và code. Chỉ chủ dự án sửa file này. Agent gặp chỗ thiếu hoặc mâu thuẫn thì ghi vào `docs/plan/questions.md`, không tự quyết.

Nhãn: **[CHỐT]** chủ dự án đã quyết. **[ĐỀ XUẤT]** bản nháp đang chờ xác nhận. **[MỞ]** chưa có quyết định, xem mục 9.
Chi tiết thao tác từng màn hình: `docs/reference/screens.md`. Thiết kế database: `docs/reference/` (script T-SQL cũ) và entity/migration trong code.

## 1. Phạm vi
Trong phạm vi: 33 màn hình F01–F33 (web responsive, giao diện tiếng Việt).
Ngoài phạm vi (giai đoạn hiện tại):
- Form giao diện cho học bổng và phê duyệt chương trình/lớp/học bổng (bảng dữ liệu học bổng vẫn có, tách hẳn tài chính).
- Thi trực tuyến (thi trực tiếp, giáo viên nhập điểm).
- Cổng thanh toán trực tuyến.
- Hóa đơn, phiếu thu, bảng điểm, báo cáo xuất ra là đầu ra của các màn hình trên, không tính là màn hình riêng.

## 2. Vai trò (7, mỗi tài khoản có đúng một vai trò cố định)
| Vai trò | Mã trong code | Màn hình chính |
|---|---|---|
| Học viên | Student | F02–F12 |
| Tuyển sinh | Admissions | F13–F15 |
| Giáo vụ | AcademicAffairs | F16–F23 |
| Giáo viên | Teacher | F24–F27 |
| Kế toán | Accountant | F28–F30 |
| Ban Giám đốc | Director | F31 |
| Quản trị viên | Admin | F32–F33 |
Đăng nhập (F01) dùng chung. Học viên tự đăng ký tài khoản (F02); tài khoản nhân sự do Admin tạo (F32).

## 3. Ma trận quyền
X = xem, T = tạo, S = sửa, D = duyệt/xác nhận. (own) = chỉ dữ liệu của chính mình. (pc) = chỉ lớp/ca thi được phân công. Ô trống = không có quyền. [ĐỀ XUẤT: suy ra từ danh sách màn hình]

| Chức năng (màn hình) | Học viên | Tuyển sinh | Giáo vụ | Giáo viên | Kế toán | Giám đốc | Admin |
|---|---|---|---|---|---|---|---|
| Khóa học đang mở (F04) | X | X | X | | | | |
| Cấu hình khóa học, chương trình theo Band, trọng số (F18) | | | T S X | | | | |
| Hồ sơ đăng ký (F05, F13, F14, F15) | T X(own) | T(hộ) X D | | | | | |
| Xác nhận nhập học (F09) | D(own) | | | | | | |
| Ca thi (F06, F07, F16, F17) | đăng ký, X(own) | đăng ký hộ, X | T S X, xếp học viên, phân công chấm | X(pc) | | | |
| Điểm thi đầu vào và Band (F08, F25) | X(own) | X | X (tiến độ) | T S(pc) | | | |
| Hóa đơn (F10, F28) | X(own) | | | | X | | |
| Ghi nhận thanh toán (F29) | | | | | D | | |
| Lớp và xếp lớp (F19, F20, F22) | | | T S X | | | | |
| Lịch học, lịch thi (F11, F21, F24) | X(own) | | T S X | X(pc) | | | |
| Điểm danh (F26) | | | | T S(pc) | | | |
| Sổ điểm và bảng điểm (F12, F23, F27) | X(own, đã chốt) | | X D(chốt/trả lại) | T S gửi(pc) | | | |
| Báo cáo tài chính (F30) | | | | | X, xuất | | |
| Tổng quan báo cáo (F31) | | | | | | X | |
| Tài khoản nhân sự, vai trò (F32) | | | | | | | T S khóa/mở |
| Nhật ký hoạt động (F33) | | | | | | | X |

## 4. Luồng nghiệp vụ chính
1. Học viên đăng ký tài khoản (F02), đăng nhập (F01).
2. Xem khóa học (F04), tạo hồ sơ đăng ký (F05). Tuyển sinh có thể tạo hộ (F15).
3. Giáo vụ tạo ca thi, phòng và phân công giáo viên chấm (F17). Học viên đăng ký ca thi còn chỗ (F06).
4. Học viên thi trực tiếp. Giáo viên nhập điểm (F25). Hệ thống tính Band từ điểm cao nhất trong các lượt (BR-05, BR-06). Học viên xem kết quả (F08).
5. Học viên xác nhận nhập học (F09).
6. Tuyển sinh kiểm tra và duyệt hồ sơ (F14). Khi duyệt, hệ thống tạo hóa đơn.
7. Học viên xem hóa đơn (F10) và trả tiền ngoài hệ thống. Kế toán đối chiếu và ghi nhận thanh toán (F29).
8. Giáo vụ tạo lớp (F20), lập lịch (F21), xếp học viên vào lớp (F22). Học viên xem lớp và lịch (F11).
9. Giáo viên điểm danh (F26), nhập điểm thành phần và gửi bảng điểm (F27).
10. Giáo vụ chốt bảng điểm hoặc trả lại (F23). Học viên xem bảng điểm đã chốt (F12).
11. Giám đốc xem báo cáo (F31). Admin quản lý tài khoản (F32) và xem nhật ký (F33).

## 5. Vòng đời trạng thái [ĐỀ XUẤT]
**Hồ sơ đăng ký:** Đã nộp → Đã có Band → Đã xác nhận → Đã duyệt → Đã xếp lớp. Nhánh phụ: Từ chối/Hủy.
| Từ | Sang | Ai | Điều kiện |
|---|---|---|---|
| Đã nộp | Đã có Band | Hệ thống | Có điểm thi ít nhất một lượt (tính lại khi có lượt hai) |
| Đã có Band | Đã xác nhận | Học viên (F09) | |
| Đã xác nhận | Đã duyệt | Tuyển sinh (F14) | Tạo hóa đơn (BR-09) |
| Đã duyệt | Đã xếp lớp | Giáo vụ (F22) | Hóa đơn đã thanh toán, đúng khóa và Band, lớp còn chỗ (BR-10) |
| Trước Đã xếp lớp | Từ chối/Hủy | Tuyển sinh | Có lý do |

**Hóa đơn:** Chưa thanh toán → Đã thanh toán (Kế toán, F29). "Quá hạn" không lưu thành trạng thái: tính từ ngày đến hạn khi hóa đơn chưa thanh toán.

**Bảng điểm:** Nháp → Đã gửi (Giáo viên, F27) → Đã chốt (Giáo vụ, công bố ngay) hoặc Bị trả lại (Giáo vụ, bắt buộc kèm lý do bằng chữ, hiển thị cho giáo viên ở F27) → Giáo viên sửa → Đã gửi lại (trạng thái quay về Đã gửi, tính là một lần gửi mới). Đã chốt thì không sửa được nữa; chỉ Bị trả lại mới sửa lại được.

**Tài khoản:** Hoạt động ↔ Bị khóa (Admin, F32).

## 6. Quy tắc nghiệp vụ
| Mã | Quy tắc | Trạng thái |
|---|---|---|
| BR-01 | Mỗi lớp có đúng một giáo viên; không lưu lịch sử đổi giáo viên. | CHỐT |
| BR-02 | Lớp tối đa 15 học viên. | CHỐT |
| BR-03 | Mỗi ca/phòng thi tối đa 20 học viên. | CHỐT |
| BR-04 | Mỗi học viên tối đa 2 lượt thi đầu vào (1 lượt thi + 1 lượt thi lại). | CHỐT (điều kiện thi lại: MỞ, Q4) |
| BR-05 | Band lấy từ điểm cao nhất trong các lượt thi. | CHỐT |
| BR-06 | Điểm thi đầu vào là một điểm tổng thang 100, đổi ra Band theo bảng bên dưới. Bảng nằm trong bảng dữ liệu `BandThreshold`, nạp bằng seed, không viết cứng trong code. Điểm tối thiểu của Band tính vào Band đó; điểm tối đa là điểm tối thiểu của Band kế tiếp trừ đi. | CHỐT |
| BR-07 | Mỗi hóa đơn thanh toán một lần đủ, không trả góp. | CHỐT |
| BR-08 | Thanh toán diễn ra ngoài hệ thống (tiền mặt hoặc chuyển khoản). Kế toán đối chiếu thực tế rồi ghi nhận thủ công. Không tích hợp cổng thanh toán. | CHỐT |
| BR-09 | Hóa đơn chỉ được tạo khi Tuyển sinh duyệt hồ sơ. | ĐỀ XUẤT (suy ra từ F14) |
| BR-10 | Chỉ xếp lớp học viên đã thanh toán, đúng khóa và Band, lớp còn chỗ. Band lớp và Band học viên phải trùng nhau. | ĐỀ XUẤT |
| BR-11 | Không được trùng lịch giáo viên hoặc phòng khi tạo lớp và lịch (F20, F21). | ĐỀ XUẤT (suy ra từ F20, F21) |
| BR-12 | Học viên chỉ thấy bảng điểm đã được Giáo vụ chốt; chốt là công bố ngay. | ĐỀ XUẤT (suy ra từ F12, F23) |
| BR-13 | Dữ liệu học bổng tách hẳn khỏi bảng tài chính, không liên kết Invoice hay Payment. | CHỐT |
| BR-14 | Band được lưu tại thời điểm chấm; đổi ngưỡng sau này không đổi Band cũ. | ĐỀ XUẤT |
| BR-15 | Đăng ký tài khoản học viên kiểm tra trùng email và số điện thoại (F02). | ĐỀ XUẤT (suy ra từ F02) |
| BR-17 | Trường dữ liệu hồ sơ đăng ký (F02, F05, F15): họ tên, ngày sinh, số điện thoại, email, khóa học chọn. | CHỐT |
| BR-18 | Học viên chỉ được đăng ký thi lại (lượt 2) sau khi đã có điểm của lượt 1. | CHỐT |
| BR-19 | Không làm tự phục vụ quên mật khẩu. Đăng xuất là nút trong khung giao diện dùng chung. Admin đặt lại mật khẩu cho tài khoản nhân sự lẫn học viên (F32); Admin cũng là người khóa/mở tài khoản học viên. | CHỐT |
| BR-20 | Học viên đạt Band mà chưa có khóa/lớp thuộc Band đó: hồ sơ giữ nguyên trạng thái Đã duyệt, chờ có lớp phù hợp. | CHỐT |
| BR-16 | Ghi nhật ký hoạt động: tầng ứng dụng ghi vào bảng `AuditLog` (không dùng trigger database). | CHỐT |

Bảng Band (BR-06):
| Band | Điểm | Mức tương đương |
|---|---|---|
| 1 | 0–24 | Mới bắt đầu (A1) |
| 2 | 25–44 | Sơ cấp (A2) |
| 3 | 45–64 | Trung cấp (B1) |
| 4 | 65–84 | Trên trung cấp (B2) |
| 5 | 85–100 | Nâng cao (C1) |
Test bắt buộc ở biên: 24/25, 44/45, 64/65, 84/85.

Trọng số điểm thành phần của khóa học (F18, F27): tổng trọng số các thành phần trong một khóa phải bằng 100%. Giá trị cụ thể do Giáo vụ cấu hình.

## 7. Danh sách màn hình và ưu tiên [ĐỀ XUẤT]
Bắt buộc = cần để chạy trọn luồng ở mục 4. Nên có = làm sau khi luồng chạy. Nếu còn thời gian = có thể cắt.
| Ưu tiên | Màn hình |
|---|---|
| Bắt buộc (25) | F01 Đăng nhập, F02 Đăng ký tài khoản, F04 Khóa học, F05 Đăng ký khóa học, F06 Đăng ký thi, F08 Kết quả thi và Band, F09 Xác nhận nhập học, F10 Hóa đơn học viên, F11 Lớp và TKB, F12 Bảng điểm học viên, F13 Danh sách hồ sơ, F14 Duyệt hồ sơ, F17 Quản lý ca thi, F19 Danh sách lớp, F20 Tạo/cập nhật lớp, F21 Lịch học, F22 Xếp lớp, F23 Chốt bảng điểm, F24 Lớp của giáo viên, F25 Nhập điểm thi, F26 Điểm danh, F27 Sổ điểm, F28 Danh sách hóa đơn, F29 Ghi nhận thanh toán, F32 Tài khoản nhân sự |
| Nên có (5) | F03 Trang chủ học viên, F07 Chi tiết ca thi, F15 Ghi danh trực tiếp, F16 Danh sách ca thi, F18 Khóa học và trọng số (trước đó dùng dữ liệu seed) |
| Nếu còn thời gian (3) | F30 Báo cáo tài chính, F31 Tổng quan báo cáo, F33 Nhật ký hoạt động |
Lưu ý: nếu F31 bị cắt thì vai trò Giám đốc chưa có màn hình nào.

## 8. Dữ liệu mẫu (seed) và yêu cầu phi chức năng
Seed (chỉ môi trường Development):
- 7 tài khoản demo, mỗi vai trò một tài khoản. Mật khẩu lấy từ cấu hình, không ghi trong repo.
- Bảng `BandThreshold` theo BR-06.
- Vài khóa học và chương trình theo Band (IELTS, TOEIC...), vài ca thi, một lớp, một hóa đơn mẫu, dữ liệu tiếng Việt thực tế.
Phi chức năng: toàn bộ chữ trên giao diện bằng tiếng Việt; responsive ở 375, 768, 1280 px; ngày dd/MM/yyyy; tiền VND; đăng nhập bằng JWT; kiểm tra quyền ở backend.

## 9. Câu hỏi mở (agent không được tự quyết)
Tất cả câu hỏi mở tại thời điểm 24/09/2026 đã được chủ dự án trả lời và chuyển thành quy tắc CHỐT ở mục 6 (BR-17 đến BR-20) và mục 5 (vòng đời bảng điểm). Mục này để trống, dùng khi phát sinh câu hỏi mới trong lúc code.

| Mã | Câu hỏi | Quyết định |
|---|---|---|
| (chưa có) | | |
