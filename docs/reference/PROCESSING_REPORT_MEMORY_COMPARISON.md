# Đối chiếu Nhom18_ThietKeXuLy.docx với Project Memory

Ngày đối chiếu: 06/10/2026. Mục đích: xác định nội dung cần đồng bộ trước khi cập nhật báo cáo. Đây là bản đối chiếu, chưa sửa tài liệu Word.

## 1. Nguồn, phạm vi và kết luận

**Tài liệu được đọc trực tiếp:** [Nhom18_ThietKeXuLy.docx](../Nhom18_ThietKeXuLy.docx).

SHA-256: `C7DE4DE6338670A94C99280DF89BDDC8E06FAE53A63E3B0405BBFE9DA0149F6C`.

Đã đọc văn bản/bảng toàn tài liệu, kiểm tra trực quan các sơ đồ use case, hai sơ đồ dữ liệu, lược đồ chuyển màn hình, các hình giao diện đại diện và toàn bộ 10 hình sequence ở cuối tài liệu. Các ảnh giao diện trùng lặp không được tính thành chức năng khác nhau. Không dùng nội dung tài liệu cũ làm chỉ dẫn thực thi hoặc tự thêm yêu cầu.

`Bxxx` trong bản này là thứ tự phần tử trực tiếp trong `word/document.xml/w:body`, tính cả đoạn và bảng, kể cả phần tử trống. Đây **không phải số trang**. Tên mục và tên bảng đi kèm giúp tìm bằng Word; ảnh được nhận diện bằng tiêu đề hiển thị.

Chuẩn đối chiếu:

- [requirements.md](../requirements.md): đặc tả sản phẩm chuẩn, gồm các quyết định nghiệp vụ đã chấp nhận.
- [FUNCTION_SPEC.md — E01](../design/FUNCTION_SPEC.md), [WORKFLOW_SPEC.md — E02](../design/WORKFLOW_SPEC.md): chức năng, quyền, trạng thái và điều kiện xử lý.
- [DATABASE_SPEC.md — E03](../design/DATABASE_SPEC.md): 24 bảng, dictionary T01–T24, quan hệ và ràng buộc.
- [E04_FINAL_CONTRACT.md](../design/E04_FINAL_CONTRACT.md), [E04_DTO_SPEC.md](../design/E04_DTO_SPEC.md), [E04_REPLAY_SPEC.md](../design/E04_REPLAY_SPEC.md): hợp đồng kỹ thuật đã chấp nhận.
- [DECISIONS.md](../DECISIONS.md), [CURRENT_STATE.md](../CURRENT_STATE.md): quyết định và checkpoint triển khai hiện hành.
- [SEQUENCE_PLAN.md](../design/SEQUENCE_PLAN.md) và các hướng dẫn SQ: tài liệu hỗ trợ vẽ theo nghiệp vụ đã duyệt. Danh mục 25 SQ là quy hoạch đề xuất, **không phải yêu cầu đã duyệt về số hình bắt buộc**.

Không lấy nguyên bản [BUSINESS_FUNCTION_COMPARISON.md](BUSINESS_FUNCTION_COMPARISON.md) làm kết quả cho file này: bản đó đối chiếu một tài liệu khác, `Nhom18_ThietKeGiaoDien.docx`, tại đường dẫn và hash khác.

**Kết luận:** tài liệu Word đang trộn các phiên bản. Văn bản dữ liệu và một số hình vẫn cũ; 10 sequence cuối đã cập nhật đáng kể theo quy tắc mới. Cần sửa theo từng phần, không thay toàn bộ vì cho rằng mọi nội dung đều lỗi thời.

| Nội dung | Trong Word hiện tại | Chuẩn cần đưa vào báo cáo |
| --- | --- | --- |
| Danh sách nghiệp vụ | Mục III có 39 NV, còn nghiệp vụ làm thay của Tuyển sinh | 37 NV còn hiệu lực; bỏ NV10/NV11, giữ mã để truy vết. E01 có 32 F còn hiệu lực, F15 đã loại; E02 có 21 W. |
| Vai trò | Bảy vai trò người dùng và tác nhân “Hệ thống” | Giữ bảy vai trò; Hệ thống là xử lý tự động, không role thứ tám. |
| Dictionary VI.2 | 20 bảng, 123 trường được liệt kê | 24 bảng, 230 cột; thêm AuthSession, EntryTestBooking, ReplayOperation, InvoiceNumberCounter. |
| Hình dữ liệu | Còn các bảng học bổng và cấu trúc khác dictionary | Vẽ lại từ E03 T01–T24; loại học bổng/phê duyệt chương trình-lớp/max_score. |
| Quan hệ và concurrency | Danh sách FK cũ, thiếu lịch sử booking và các actor mới | 50 FK NO ACTION, trong đó hai FK ghép; rowversion ở 20 bảng mutable, không thêm vào mọi bảng. |
| Trạng thái thực thi | Hình minh họa có thể tạo cảm giác hệ thống hoàn chỉnh | Thiết kế E01–E05 Accepted; triển khai mới hoàn tất từng stage đến R03. Host mặc định chưa gắn các adapter; chưa chứng minh vận hành sản phẩm đầy đủ. |

## 2. Sai lệch nghiệp vụ trong các mục I–III và V

“Sửa” là khác chuẩn hiện hành; “Bổ sung” là quy tắc còn thiếu; “Giữ và làm rõ” là chức năng cốt lõi vẫn đúng.

| Mã | Vị trí/nội dung trong Word | Đánh giá | Nội dung cần dùng khi sửa báo cáo | Nguồn chuẩn |
| --- | --- | --- | --- | --- |
| NV-01 | I B26; III NV10/NV11; V B213/B217/B220: hỗ trợ ghi danh, đăng ký/đặt ca thay | Sửa | Student tự tạo Registration và đặt ca. Tuyển sinh tra cứu, duyệt/từ chối/hủy theo quyền; bỏ hai chức năng làm thay. | requirements; ADR-012; E01 F15 |
| NV-02 | II B52: học bổng | Sửa | Loại học bổng và xử lý giảm học phí liên quan khỏi phạm vi hệ thống. | requirements, phần ngoài phạm vi |
| NV-03 | II B59: tự động chấm thi đầu vào | Sửa | Teacher được phân công nhập điểm tổng 0–100; hệ thống chọn lượt tốt nhất và suy Band, không tự chấm bài thi. | E02 W08 |
| NV-04 | II B48/B62: nợ quá 15 ngày, tự thay trạng thái học viên | Sửa | Quá hạn chỉ là cảnh báo Invoice UNPAID sau due_date; không đổi trạng thái Student, không phạt/tăng tiền. Hạn thanh toán là ngày phát hành VN +7 ngày lịch. | requirements, tài chính |
| NV-05 | II B61: công nợ = phải thu − đã thu | Sửa | Doanh thu theo Payment.paid_at trong kỳ; phải thu theo Invoice phát hành trong kỳ, loại CANCELLED; công nợ là tổng Invoice UNPAID hiện tại, không trừ hai tổng khác kỳ. | E02 W20 |
| NV-06 | III NV38/B83; V mô tả cấu hình: số buổi và trọng số thuộc Course | Sửa | Course giữ mã/tên/học phí/trạng thái. TrainingProgram theo Course/Band giữ expected_lesson_count và các ScoreComponentConfig. | E03 T05–T08 |
| NV-07 | III/V: hai lượt thi nhưng chưa phân biệt lượt với lịch đặt ca | Bổ sung | Tối đa hai logical attempts mỗi Registration. Vắng/hủy/đổi ca tạo lịch sử booking, không tiêu thêm lượt. Lượt hai sau điểm lượt một và trước CONFIRMED; vắng không là điểm 0. | E02 W07–W09; E03 T11–T12 |
| NV-08 | Xác định Band chưa nói cách xử lý hòa điểm | Bổ sung | Lấy điểm cao nhất; hòa thì chọn lượt một. Chọn Band có min_score lớn nhất ≤ điểm; seed ngưỡng 0/25/45/65/85. | requirements; E03 T06/T13 |
| NV-09 | V B220: duyệt khi đủ thông tin/không trùng | Bổ sung | Chỉ Tuyển sinh duyệt Registration CONFIRMED có Band hợp lệ. ADMITTED + một Invoice UNPAID + audit cùng giao dịch. Xác nhận của Student chưa tạo hóa đơn. | E02 W09–W11 |
| NV-10 | V B220: sĩ số tối đa 15 “hoặc cấu hình” | Sửa | max_size được cấu hình trong 1–15, không cho vượt 15. capacity ca thi trong 1–20. | E03 T10/T14 |
| NV-11 | VI B478: xếp lớp OPEN hoặc ONGOING | Sửa | Chỉ xếp Class OPEN, Registration ADMITTED, Invoice PAID, đúng Course/Band, còn chỗ, không trùng lịch Student. Không chuyển/gỡ assignment. | E02 W15; E03 T15 |
| NV-12 | Xếp lớp chưa tạo bảng điểm nháp cùng thao tác | Bổ sung | Tạo StudentClassAssignment + GradeReport DRAFT + audit cùng transaction; không tạo điểm/điểm danh mặc định. | ADR-134; E03 T15/T19 |
| NV-13 | Chống trùng lịch mới nói phòng/lớp/giáo viên | Bổ sung | Kiểm thêm Student giữa ca thi và lịch lớp; kiểm lại tại commit. Một xung đột chặn toàn batch lịch tuần; preview không giữ chỗ. | E02 W07/W15/W16 |
| NV-14 | Lịch chưa có nghĩa vụ, dời/thay, xác nhận đã tổ chức | Bổ sung | Schedule có PLANNED/HELD/NOT_HELD và chuỗi replacement cùng obligation_key. HELD do Giáo vụ xác nhận sau end, không tự suy từ đồng hồ/Attendance. | E02 W16; E03 T16 |
| NV-15 | III NV26/B75: chỉ sửa khi RETURNED | Sửa | Teacher lưu nháp điểm/nhận xét khi DRAFT hoặc RETURNED theo cửa sổ hợp lệ; SUBMITTED/FINALIZED bị khóa. | E02 W18 |
| NV-16 | Gửi/trả/chốt chưa xác định đơn vị thao tác | Bổ sung | Lưu nháp theo một học viên; gửi/gửi lại/trả/chốt cả lớp hoặc không ai. Gửi khi Class COMPLETED và đủ điểm toàn lớp; FINALIZED không mở lại. | E02 W18/W19 |
| NV-17 | Tổng điểm/xếp loại chưa tách nháp và chốt | Bổ sung | Thang 0–100, tổng Σ(score×weight/100); decimal, half-up một lần cuối đến hai chữ số. Nháp tính khi đọc; final_score chỉ lưu khi FINALIZED. Đạt nếu tổng đã chốt ≥50. | E03 T19; requirements |
| NV-18 | III NV29/B77; V báo cáo: Excel/CSV | Sửa | Kế toán xuất CSV tài chính theo bộ lọc/cột chuẩn. Director chỉ xem tổng hợp không định danh, không xuất file/phê duyệt. | E01 F30/F31; E04 DTO D10 |
| NV-19 | II B64; V B213: xem/thanh toán hóa đơn, biên lai | Giữ và làm rõ | Student xem Invoice/hướng dẫn và trả tiền bên ngoài hệ thống. Accountant xác nhận thu đủ một lần; không payment gateway, trả góp hoặc biên lai riêng. | E02 W11/W12 |
| NV-20 | V quản lý nhân sự: ACTIVE↔LOCKED khi nghỉ việc | Sửa | ACTIVE/LOCKED/DISABLED tách biệt; nghỉ/ngừng dùng là DISABLED, có khôi phục theo guard. Khóa/ngừng/reset/đổi mật khẩu thu hồi phiên; mở lại không phục hồi phiên cũ. | E02 W03; requirements |
| NV-21 | I B44; V B217: audit mọi cập nhật/tạo-sửa-xóa | Sửa | Audit mutation bắt buộc theo danh mục cùng transaction; security-only theo cơ chế riêng. Không suy quyền xóa dữ liệu hoặc audit mọi lần đọc. Admin chỉ tra cứu, không sửa/xóa/export. | E02 W21; E03 T22 |
| NV-22 | Thiếu đóng băng cấu hình sau sử dụng | Bổ sung | Program khóa Course/Band/list/kind/weights/số buổi sau Class đầu tiên, kể cả lớp CANCELLED. Toàn bộ set Band khóa sau completed entry result đầu tiên. | E03 T06–T08 |
| NV-23 | V đăng ký/đăng nhập nói Email/SĐT | Sửa | Đăng nhập bằng email; phone là thông tin liên hệ. Student tự đăng ký Account+Profile+audit nguyên tử, không tự đăng nhập hoặc tạo Registration khi signup thành công. | E02 W01/W02 |
| NV-24 | Thiếu điều kiện đăng ký học lại | Bổ sung | Một hồ sơ chưa kết thúc/Student/Course. Kết thúc khi CANCELLED/REJECTED hoặc Class COMPLETED và report FINALIZED; không phụ thuộc đạt. Đăng ký mới không sao chép điểm/Band cũ. | E03 T09; requirements |

Mục V chỉ có 30 dòng chức năng, không tương đương 30 NV hay 30 F. Khi sửa, dùng ma trận NV→F→W trong E01/E02 để tránh mất chức năng do gộp dòng hoặc đánh số lại.

## 3. Biểu mẫu minh họa ở mục IV

Biểu mẫu nguồn là minh họa, không phải căn cứ tự mở rộng yêu cầu. Tuy nhiên, giữ nguyên các trường/giá trị sau trong báo cáo sẽ làm người đọc hiểu sai sản phẩm.

| Vị trí | Nội dung cũ | Nội dung cần đồng bộ |
| --- | --- | --- |
| Phiếu đăng ký B91–B107 | Giới tính, nơi sinh, CCCD/hộ chiếu, phụ huynh, nơi làm việc; giảm giá/trả góp/cà thẻ | Form Student chỉ các trường hồ sơ đã duyệt. DOB bắt buộc, không hạn tuổi; địa chỉ tùy chọn. Không thêm module phụ huynh/giảm giá/trả góp. |
| B110 | Hoàn tiền/bảo lưu | Loại khỏi phạm vi chức năng hiện hành. |
| Danh sách lớp B121 | Giới tính/DOB/phụ huynh/chuyển lớp | Teacher chỉ đọc dữ liệu roster đúng phạm vi: mã Student, họ tên, email, phone hiện tại; không DOB/address/chuyển lớp. |
| B132 | Thông báo Email/Zalo | Không mô tả gửi thông báo tự động khi chưa có chức năng được duyệt. |
| Bảng điểm B139 | Tổng /10, hệ số giữa kỳ/cuối kỳ cố định | Thay thành /100, thành phần và weight lấy từ program; không cố định hệ số 1/2. |
| Điểm danh B153 | x/p/k/m, bốn trạng thái | Có mặt/Vắng; chưa nhập là thiếu row, không trạng thái vắng có phép/đi trễ. |
| Biên lai B163 | Chứng từ thu tiền riêng | Không coi là màn hình/chức năng phát hành biên lai. Student in Invoice bằng trình duyệt theo quyền hiện hành. |
| Báo cáo thu chi B178–B187 | Lương, marketing, chi phí, lợi nhuận, phê duyệt Director | Phạm vi chỉ báo cáo học phí/doanh thu/phải thu/công nợ đã duyệt. |
| Hóa đơn B194/B200–B208 | INV…, nhiều dòng hàng, tài liệu, discount, VAT, ký/đóng dấu | HD-YYYY-NNNNNNNN; một Invoice/Registration, tuition snapshot nguyên VND. Bản in không chữ ký/đóng dấu giả, không thêm phí/discount/VAT module. |

## 4. Đối chiếu dictionary từng bảng — VI.2

Số trường được kiểm bằng đọc XML bảng Word và các dictionary T01–T24 của E03. Bảng dưới liệt kê thay đổi chính để sửa; **kiểu/NULL/default/khóa đầy đủ phải lấy nguyên chuẩn tại T tương ứng**, không chỉ thêm vài cột rồi giữ constraints cũ.

Quy ước chung: PK nghiệp vụ `int IDENTITY(1,1)`; enum/mã/email/phòng dùng `varchar` theo kích thước dictionary; sự kiện UTC `datetime2(3)`; ngày nghiệp vụ VN `date`; giờ VN `time(0)` có giây=0; tiền `decimal(18,0)>0`; điểm/weight `decimal(5,2)`. Unicode giới hạn theo code point nên dung lượng `nvarchar` có thể lớn gấp đôi giới hạn ký tự nghiệp vụ. FK đều NO ACTION.

| Bảng nguồn/vị trí | Trường cũ → mới | Thay đổi cần đưa vào dictionary |
| --- | --- | --- |
| Role — B256, T01 | 3 → 3 | role_code `varchar(32)`, role_name `nvarchar(200)`; seed đúng bảy mã Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin. Không dùng nguyên enum uppercase/snake-case cũ như giá trị lưu. Không CRUD role động. |
| Account — B259, T02 | 10 → 16 | Bỏ locked_until. Thêm security_version, failure_window_started_at, login_throttled_until, must_change_password, temp_issued_at, temp_expires_at, row_version. email varchar254 bất biến; phone varchar10; hash varchar512; full_name nvarchar200. Throttle riêng manual lock. |
| StudentProfile — B262, T03 | 4 → 5 | DOB NOT NULL, không sau ngày VN hiện tại; address nvarchar510 tùy chọn; thêm row_version. Chỉ Account Student có Profile. |
| Course — B265, T05 | 5 → 6 | code varchar30; name nvarchar300; base_price decimal(18,0)>0; thêm row_version. Giá mới không cập nhật Invoice cũ. |
| Band — B268, T06 | 3 → 4 | code varchar30; thêm row_version; min_score unique, không max_score; freeze toàn set sau kết quả đầu tiên. |
| TrainingProgram — B271, T07 | 5 → 7 | name nvarchar300; thêm expected_lesson_count>0 và row_version. Unique Course/Band cả khi INACTIVE; ACTIVE cần đủ config, tổng100, đúng một FINAL_EXAM. |
| ScoreComponentConfig — B274, T08 | 4 → 6 | component_name nvarchar200; thêm kind GENERAL/LESSON/FINAL_EXAM và row_version; unique tên/program, filtered unique FINAL_EXAM/program. Chỉnh child phải chạm Program version. |
| Registration — B277, T09 | 8 → 20 | Thêm is_unfinished, row_version; rejection bundle: rejected_by/at, rejection_reason, rejected_from_status, rejection_operation_id; cancellation bundle: cancelled_by/at, cancellation_reason, cancelled_from_status, cancellation_operation_id. Filtered unique Student/Course khi unfinished. Giữ metadata approval sau hủy hợp lệ. |
| EntryTestSession — B280, T10 | 7 → 12 | room varchar30, thời gian UTCms cùng ngày VN, default CLOSED; thêm cancelled_by/at, cancellation_reason/operation_id, row_version. Sức chứa theo SCHEDULED Booking. |
| EntryTestAttempt — B283, T11 | 9 → 8 | Bỏ session_id, status, registered_at; thêm completed_booking_id và row_version. attempt_number int1–2. Bộ score/graded_by/graded_at/completed_booking_id cùng NULL hoặc cùng có; original grader/time không đổi khi correction. |
| BandAssignment — B286, T13 | 5 → 6 | Thêm row_version. best_attempt dùng FK ghép cùng registration_id, không độc lập UNIQUE best_attempt_id như nguồn. Hòa điểm chọn lượt một. |
| Class — B289, T14 | 6 → 12 | code varchar30 bất biến, max_size int1–15; thêm created_at, cancelled_by/at, cancellation_reason/operation_id, row_version. Không trường phê duyệt mở/hủy lớp. |
| StudentClassAssignment — B292, T15 | 4 → 5 | Thêm assigned_by→Account; giữ unique registration_id. Assignment bất biến, không gỡ/chuyển, không row_version; không đổi registration_id thành student_id như hình cũ. |
| Schedule — B295, T16 | 7 → 18 | Thêm obligation_key, replaces_schedule_id, is_current, occurrence_state, held_by/at, not_held_by/at/reason, replacement_operation_id, row_version. Unique có điều kiện cho current obligation/current FINAL_EXAM; room varchar30, time theo phút. |
| Attendance — B298, T17 | 4 → 7 | Thêm entered_by, entered_at, row_version. present bit không default; chỉ current LESSON cùng Class. Không row khác Vắng. |
| Score — B301, T18 | 6 → 8 | Thêm schedule_id và row_version. GENERAL schedule=NULL; LESSON/FINAL_EXAM bắt buộc đúng current Schedule/type/Class. Score save chạm GradeReport version, blank không thành0/xóa. |
| GradeReport — B304, T19 | 11 → 14 | Thêm submitted_operation_id, review_operation_id, row_version; comment/reason nvarchar2000. final_score/finalized_at chỉ khi FINALIZED. Latest metadata không thay audit từng lần gửi/trả/chốt. |
| Invoice — B307, T20 | 7 → 13 | number varchar16, tiền decimal(18,0)>0; thêm cancelled_by/at, cancellation_reason, cancelled_from_status, cancellation_operation_id, row_version. due_date=ngày VN issued+7; snapshot tiền và số/link/issued/due bất biến. |
| Payment — B310, T21 | 7 → 11 | Tiền decimal(18,0)>0; method CASH/TRANSFER, bỏ BANK_TRANSFER. Reference nvarchar200, tối đa100 code point, không unique. Thêm last_corrected_by/at/reason, row_version; chỉ sửa method/reference có lý do, không sửa tiền/link/confirmed_by/paid_at. |
| AuditLog — B313, T22 | 8 → 13 | audit_id int IDENTITY, không BIGINT; action varchar80/entity_type varchar64/entity_id varchar100/IPvarchar45. Thêm result, operation_id, audit_category, security_expires_at, ip_expires_at. details JSON allowlist; actor NULL chỉ khi chưa xác thực. |

### 4.1. Bốn bảng thiếu trong Word

| Bảng chuẩn | Số cột | Trường phải bổ sung và mục đích |
| --- | --- | --- |
| AuthSession — T04 | 9 | sid binary32 PK, account_id FK, issued_at, expires_at, last_activity_at, revoked_at, security_version, session_scope, row_version. Quản lý phiên hiện hành/thu hồi, không lưu JWT/password/CSRF. |
| EntryTestBooking — T12 | 14 | booking_id, attempt_id, session_id, status, booked_at, absent_by/at/operation_id, cancelled_by/at, cancellation_reason, cancelled_from_status, cancellation_operation_id, row_version. Giữ lịch sử đặt/đổi/vắng/hủy cùng logical attempt. |
| ReplayOperation — T23 | 10 | replay_id, actor_account_id, action_code, request_id, input_digest, operation_id, result_mode, result_reference, completed_at, expires_at. Completion nguyên tử, chống lặp effects; không snapshot PII/secret, append-only không row_version. E04/R02 chốt cửa sổ đúng24h. |
| InvoiceNumberCounter — T24 | 3 | issue_year smallint PK không IDENTITY, last_value int, row_version. Cấp HD-YYYY-NNNNNNNN trong transaction theo năm VN, không tái dùng số đã phát hành, chặn overflow. |

Bốn bảng không có rowversion: Role, StudentClassAssignment, AuditLog, ReplayOperation. Không áp một cột RV đồng loạt cho 24 bảng.

## 5. Các ràng buộc VI.3 cần thay hoặc bổ sung

| Vị trí nguồn | Sai lệch/thiếu | Cách sửa theo chuẩn |
| --- | --- | --- |
| B319: role_code uppercase | Khác giá trị canonical T01 | Dùng bảy mã đúng chữ hoa/thường chuẩn của E03; nhãn tiếng Việt ở role_name. |
| B328/B384/B388: tiền ≥0 | Cho học phí/thu tiền bằng0 và số lẻ VND | Tiền >0, nguyên VND; Payment bằng đúng Invoice.amount. |
| B355/B400: Attempt.status và bundle kết quả | Trạng thái thuộc Booking, result bundle chưa đầy đủ | Chuyển SCHEDULED/COMPLETED/ABSENT/CANCELLED sang Booking; Attempt result bundle bốn trường. |
| B389/B404: BANK_TRANSFER | Enum cũ | TRANSFER bắt buộc reference không trống; CASH có thể NULL. |
| B393: actor NULL cho mọi hành động tự động | Có thể làm mất actor của thao tác đã xác thực | NULL chỉ khi chưa xác thực; xử lý tự động trong một nghiệp vụ đã xác thực vẫn giữ actor theo contract. |
| B397: locked_until khi LOCKED | Đồng nhất throttle với khóa trạng thái | Bỏ locked_until; failure window/throttle ở cột riêng, không tự đổi status/unlock. |
| B399: approved bundle NULL nếu chưa ADMITTED | Không bảo toàn approval của hồ sơ đã duyệt rồi hủy | Giữ approval actor/time sau hủy; kiểm toàn bộ rejection/cancellation/prior-state/operation bundles theo T09. |
| B402: GradeReport bundles | Chưa có operation IDs, điều kiện final_score chưa chặt | Bổ sung submit/review operation, FINALIZED-only total/time, cùng operation cho cohort; dùng CHECK/bundle T19 và E04. |
| B409: phone unique khi có | Thiếu điều kiện vật lý | Filtered unique WHERE phone IS NOT NULL; Student phone bắt buộc tại service. |
| B416: một program “hiện hành”/Course/Band | Có thể hiểu cho nhiều version khi INACTIVE | Unique cặp Course/Band cho mọi trạng thái, không version configuration. |
| B417: chỉ tổng weight100 | Thiếu kind/FINAL_EXAM/freeze | ACTIVE cần tổng100 và đúng một FINAL_EXAM; trước first-use ACTIVE muốn sửa config phải về DRAFT. |
| B420: UNIQUE best_attempt_id | Không đúng tập key vật lý hiện hành | Unique registration_id; FK ghép `(best_attempt_id,registration_id)` sang Attempt, index best_attempt_id. |
| B427/B428/B481/B482: overlap | Thiếu Student và current/history/Cancelled semantics | Kiểm tài nguyên current hữu hiệu, Student, giờ VN, cả ca thi và lịch lớp; lịch Class CANCELLED giữ history nhưng không chiếm chỗ. |
| B464: best Attempt COMPLETED | Không còn cột Attempt.status | Best có result và completed_booking đúng lượt; điểm cao nhất, hòa chọn lượt một. |
| B466–B468: chuyển trạng thái | Thiếu điểm đầu tiên/booking lượt hai/pending confirmation guard | PENDING→TESTING ở booking đầu; kết quả hợp lệ→BAND_ASSIGNED; lượt hai không kéo lùi. CONFIRMED cần không còn lượt đặt/chờ chấm. |
| B471: đếm Attempt trong ca | Sai đối tượng occupancy | Đếm SCHEDULED Booking dưới session lock; kết quả/vắng giữ history, không cache booked_count. |
| B478 | Cho xếp ONGOING | Chỉ OPEN và đầy đủ guard W15. |
| B486: ngoại lệ phân quyền sửa điểm | Mở quyền override chưa được duyệt | Không Admin/Giáo vụ override. Teacher đúng phân công/cửa sổ; correction điểm đầu vào còn đúng original grader và trước CONFIRMED. |
| B487–B492: tổng/gửi/chốt | Thiếu đơn vị cả lớp và khóa nháp | Tổng decimal/làm tròn cuối; lưu nháp một Student; gửi/trả/chốt all-or-none toàn Class; Student-own chỉ FINALIZED. |
| B469/B496/B497: transaction/audit | Cốt lõi đúng nhưng chưa đủ đồng thời/lặp request | Giữ atomic approval/payment; bổ sung request_id/replay ở các action được chọn, gates/rowversion/mandatory audit rollback. Không replay credential inputs. |

### 5.1. Danh sách FK để sửa VI.3.4 và vẽ quan hệ

Mỗi mũi tên dưới đây là **một constraint**; danh sách có50 constraint, gồm hai FK ghép. Tất cả dùng NO ACTION. NULL và quyền actor theo dictionary/contract, không suy chỉ từ việc tồn tại bản ghi.

| Bảng con | Khóa ngoại → bảng/cột được tham chiếu |
| --- | --- |
| Account | role_id → Role.role_id |
| StudentProfile | account_id → Account.account_id |
| AuthSession | account_id → Account.account_id |
| Course; Band; Role; InvoiceNumberCounter | Không có FK |
| TrainingProgram | course_id → Course.course_id; band_id → Band.band_id |
| ScoreComponentConfig | program_id → TrainingProgram.program_id |
| Registration | student_id → StudentProfile.student_id; course_id → Course.course_id; admission_approved_by → Account.account_id; rejected_by → Account.account_id; cancelled_by → Account.account_id |
| EntryTestSession | grader_id → Account.account_id; cancelled_by → Account.account_id |
| EntryTestAttempt | registration_id → Registration.registration_id; graded_by → Account.account_id; **(completed_booking_id, attempt_id) → EntryTestBooking(booking_id, attempt_id)** |
| EntryTestBooking | attempt_id → EntryTestAttempt.attempt_id; session_id → EntryTestSession.session_id; absent_by → Account.account_id; cancelled_by → Account.account_id |
| BandAssignment | registration_id → Registration.registration_id; **(best_attempt_id, registration_id) → EntryTestAttempt(attempt_id, registration_id)**; band_id → Band.band_id |
| Class | program_id → TrainingProgram.program_id; teacher_id → Account.account_id; cancelled_by → Account.account_id |
| StudentClassAssignment | class_id → Class.class_id; registration_id → Registration.registration_id; assigned_by → Account.account_id |
| Schedule | class_id → Class.class_id; replaces_schedule_id → Schedule.schedule_id; held_by → Account.account_id; not_held_by → Account.account_id |
| Attendance | schedule_id → Schedule.schedule_id; class_assignment_id → StudentClassAssignment.class_assignment_id; entered_by → Account.account_id |
| Score | class_assignment_id → StudentClassAssignment.class_assignment_id; component_id → ScoreComponentConfig.component_id; entered_by → Account.account_id; schedule_id → Schedule.schedule_id |
| GradeReport | class_assignment_id → StudentClassAssignment.class_assignment_id; submitted_by → Account.account_id; reviewed_by → Account.account_id |
| Invoice | registration_id → Registration.registration_id; cancelled_by → Account.account_id |
| Payment | invoice_id → Invoice.invoice_id; confirmed_by → Account.account_id; last_corrected_by → Account.account_id |
| AuditLog | actor_account_id → Account.account_id |
| ReplayOperation | actor_account_id → Account.account_id |

`EntryTestAttempt.session_id → EntryTestSession` trong B441 phải bỏ; quan hệ với ca nằm ở EntryTestBooking. Không tạo thêm scalar FK best_attempt/completed_booking trùng với hai FK ghép. operation_id, request_id, obligation_key và AuditLog.entity_id là correlation/technical identifiers, không tự biến thành FK nghiệp vụ.

Cập nhật ngày 07/10/2026: [REFERENTIAL_INTEGRITY_IMPACT.md](REFERENTIAL_INTEGRITY_IMPACT.md) đã mở rộng từ bản 34 ràng buộc thành **đầy đủ 50 FK hiện hành**, gồm hai FK ghép và FK tự tham chiếu Schedule. Dùng phần quy ước và 50 bảng chi tiết để bổ sung mục VI.3.4 của báo cáo.

## 6. Sai lệch trong hình vẽ và giao diện

| Hình/vị trí | Kết quả đọc trực quan | Hành động khi sửa báo cáo |
| --- | --- | --- |
| Use case tổng thể B224 | Bảy actor người dùng; nhóm chức năng tổng thể còn phù hợp | Giữ cấu trúc tổng quát, đồng bộ chi tiết quyền từ E01. |
| Use case tài khoản B227 | Chỉ nối Student với đăng nhập, Admin với cấp phát | Bổ sung đăng nhập/đăng xuất/đổi mật khẩu chung các vai trò; profile edit Student; reset/lock/disable/restore Admin theo scope. |
| Use case tuyển sinh B230 | Còn “Hỗ trợ ghi danh” | Bỏ chức năng làm thay; giữ tra cứu/duyệt, bổ sung từ chối/hủy theo guard. |
| Use case học phí B235 | “Xác nhận thanh toán” include “Tự động tạo hóa đơn” | Sai thời điểm: tạo Invoice thuộc duyệt tuyển sinh, không payment. Student xem/in/hướng dẫn; Accountant xác nhận thu đủ và sửa metadata cho phép. |
| Use case đào tạo B238 | Thiếu ca thi/Band/program và xử lý lịch hiện hành; Student/Teacher đứng nhưng không có liên kết | Đồng bộ đầy đủ chức năng đúng actor; tránh để actor không tham gia use case trong hình. |
| Use case giảng dạy B242 | Có điểm danh/nhập điểm, thiếu nhập đầu vào và gửi/gửi lại report | Bổ sung đúng chức năng Teacher; phân biệt tính nháp với chốt của Giáo vụ. |
| Use case báo cáo B248 | Director xem ba nhóm tổng hợp | Giữ view-only, thêm scope không định danh; báo cáo/CSV Accountant thể hiện ở nhóm riêng. |
| Sơ đồ logic VI.1 B253 | Có ScholarshipGrant/Candidate/Criteria; Attempt→Session; có liên kết trực tiếp StudentProfile→Course | Vẽ lại theo ERD E03; bỏ học bổng và FK trực tiếp Profile→Course; dùng Registration, Booking; bổ sung AuditLog và các bảng mới. |
| Sơ đồ quan hệ VI.4 B501 | Còn học bổng, Band.max_score, approval ở Program/Class, StudentClassAssignment.student_id, GradeReport.approved_by/at; khác cả dictionary Word | Thay bằng mô hình T01–T24; max_score không dùng, lớp/program không phê duyệt Director, assignment dùng registration_id, report dùng reviewed_by/at. |
| Lược đồ chuyển màn hình VII.1 B506 | Còn hỗ trợ ghi danh; lặp xếp lớp; nhánh nhân sự/audit ghi “Màn hình giám đốc” | Bỏ F15, sửa nhánh cuối thành Admin, xóa dòng lặp; thêm luồng tài khoản chung/phiên hạn chế theo thiết kế. |
| Đăng nhập VII.2.1 B514 | TextBox email “kiểm tra có đăng ký trong DB” | UI validate hình thức; kết quả trước xác thực không tiết lộ email tồn tại/status. Thành công nhận role/session do server xác định. |
| Đăng ký VII.2.2 B517/B523 | Mô tả do Admin cấp; thành công chuyển trang chủ | Student tự đăng ký; thành công hướng dẫn đăng nhập, không auto-login. Hình fields cơ bản có thể giữ sau cập nhật validation/ngày VN. |
| Trang chủ Student VII.2.3 B527/B530 | Hình tổng8.2/10; thành phần9.0/7.8/8.1; nút tải phiếu điểm | Thay dữ liệu sang /100 và cấu hình program; bỏ tải phiếu điểm ngoài phạm vi. Không nhân10 máy móc nếu chưa có dữ liệu chuẩn để minh họa. |
| “Trang chủ Giáo vụ” VII.2.4 B533/B538 | Hình thực tế ghi Tuyển sinh và chức năng duyệt hồ sơ; có ghi chú nội bộ | Đổi tên/mô tả thành Tuyển sinh. Không thêm note field/CRUD nội bộ ngoài DTO; dùng lý do từ chối/hủy đã duyệt khi cần. |
| Các hình preview vai trò | Có dropdown “Vai trò xem trước”, dữ liệu mẫu, HS-… | Ghi rõ prototype. Hệ thống thật có role server, không chọn quyền qua dropdown; mã định danh theo DTO chuẩn, không suy prefix trang trí thành field lưu. |
| Trang chủ Teacher VII.2.5 | Lớp/lịch/nhập đầu vào/điểm danh/sổ điểm cơ bản đúng | Giữ và bổ sung cửa sổ thao tác, current Schedule/HELD, gửi cả lớp; không chứng minh UI đã triển khai. |

## 7. Mười sequence cuối tài liệu: phần đã cập nhật

Các hình cuối **không cùng mức độ cũ với dictionary/ERD**. Những điểm đúng sau cần được giữ khi sửa văn bản xung quanh.

| Tiêu đề sequence / vị trí | Nội dung đã phù hợp Project Memory | Việc còn cần đồng bộ |
| --- | --- | --- |
| SQ01 Tự đăng ký — B549 | Student role, Account+Profile+mandatory audit, rollback/trùng đồng thời; không tự login/tạo Registration | Sửa mô tả signup VII đang nói do Admin cấp/chuyển trang chủ. |
| SQ03-A Đăng nhập — B552 | Lỗi không tiết lộ Account; normal2h/idle30min; restricted10min/capped temp expiry; server role | Dictionary phải thêm AuthSession/security fields; mô tả chính sách throttle/tạm password ở phần xử lý chi tiết. |
| SQ03-B Đăng xuất — B555 | Thu hồi phiên hiện tại, không các phiên khác; xóa trạng thái local | Đồng bộ khái niệm session ở dữ liệu; không mô tả logout chỉ xóa UI/JWT. |
| SQ04 Tự đổi mật khẩu — B560 | Kiểm mật khẩu cũ, normal/restricted; update+all-session revoke+mandatory audit nguyên tử; login mới; không replay mật khẩu | Giữ guards này; bổ sung chính sách password ở đặc tả chung thay vì nhồi toàn bộ vào hình. |
| SQ07-A Đăng ký khóa — B564 | Course ACTIVE, một hồ sơ chưa kết thúc, PENDING+audit; không copy điểm/Band | Bổ sung is_unfinished và filtered unique trong dictionary. |
| SQ07-B Hủy hồ sơ — B567 | Student trước CONFIRMED; preview không giữ state; recheck, hủy booking tương lai, giữ history, rollback | Bổ sung Booking và cancellation bundles; không mở quyền hủy sau CONFIRMED cho Student. |
| SQ11 Xác nhận nhập học — B570 | BAND_ASSIGNED, không lượt chờ; CONFIRMED nguyên tử/audit; chưa Invoice/lớp; khóa thi lại/sửa kết quả | Đồng bộ mục V/constraints thiếu pending-attempt guard. |
| SQ13-A Student xem/in Invoice — B573 | Own/fresh authority; lấy mới trước in; browser A4; UNPAID không xác nhận thu tiền | Bỏ biên lai/PDF và biểu mẫu ký/đóng dấu giả ở phần minh họa. |
| SQ13-B Accountant tra cứu — B576 | Scope kế toán, mọi trạng thái, không sửa PAID khi xem; không cấp quyền in/export danh mục Invoice | Giữ; tách CSV báo cáo tài chính khỏi màn hình Invoice. |
| SQ15-A Quản lý Course — B579 | Kiểm quyền/state/stale, mutation+audit all-or-none; code bất biến, giá mới không đổi Invoice cũ | Đây mới nhánh Course; không dùng làm hình đại diện đủ cho Band/Program/component/freeze. |

Không thấy sai lệch nghiệp vụ trọng yếu trong10 hình này ở mức đọc trực quan. Tuy nhiên, đây không phải nghiệm thu UML hoặc bằng chứng API/runtime. Một số guard chi tiết nằm ở ghi chú/tài liệu SQ, không nhất thiết phải vẽ thêm lifeline kỹ thuật.

Phần sequence hiện có chưa thể hiện riêng các luồng quan trọng như: quản trị nhân sự/reset/lock; đặt/đổi/vắng/thi lại; nhập/sửa điểm đầu vào và Band; Tuyển sinh duyệt/từ chối/hủy; payment/correction; program/freeze; lifecycle lớp/xếp lớp; lịch replacement/HELD; điểm danh; lưu nháp/gửi/trả/chốt toàn lớp; báo cáo tài chính/CSV/Director; audit lookup. Chọn hình cần bổ sung theo yêu cầu báo cáo; dùng W tương ứng và danh mục SQ để bao phủ, không mặc định phải vẽ đủ25 hình.

## 8. Nội dung kỹ thuật còn thiếu nếu báo cáo trình bày thiết kế hiện hành

| Nhóm | Nội dung nên bổ sung | Nguồn |
| --- | --- | --- |
| Xác thực | JWT trong cookie HttpOnly, server kiểm Account/AuthSession hiện tại; CSRF boundary; không localStorage token/refresh. Throttle5 sai/cửa sổ15min/chặn15min khác manual lock; normal2h/idle30min; temp24h/restricted≤10min. | requirements; E04 runtime/DTO |
| Mật khẩu | 15–128 Unicode code point, giữ raw spaces/không normalize, offline blocklist; hash chuẩn. Nhân sự/reset nhận mật khẩu tạm một lần; không ghi/lưu/log/replay secret. | requirements; E04 |
| Đồng thời/giao dịch | rowversion/parent aggregate version, khóa tài nguyên theo contract, all-or-none; mandatory audit lỗi thì rollback. Preview không giữ chỗ, recheck tại commit. | E03 mục9; E04 SQL integration |
| Chống lặp | Registry55 canonical actions,6lookup groups; completion và business/audit cùng transaction; fresh authority trước replay;24h; reference an toàn, không response snapshot hoặc phát lại mật khẩu. | E04 replay; R01/R02 cards |
| Phân tầng enforcement | PK/FK/unique/CHECK bảo vệ cấu trúc; service bảo vệ role/ownership/state/window/liên bảng;5trigger chặn đúng các invariant chọn. FK tồn tại không đủ bảo đảm nghiệp vụ. | E03; E04 SQL integration |
| SQL objects/quyền | Danh mục5trigger/6view/6UDF/7SP và đường gọi;4SQL roles/exact102permissions. Không gán một SQL role cho mỗi role sản phẩm hoặc hứa app là dbo. | E03/E04; D08–D12 cards |
| DTO và định dạng | Money strings nguyên VND; decimal score/weight≤2lẻ; thời gian UTC/giờ VN; DTO allowlist, scope dữ liệu theo role. CSV UTF-8 BOM/comma/CRLF, đúng cột/caps đã chốt. | E04 DTO/final contract |
| Retention audit | Mandatory history giữ lại; security-only/IP90ngày; operator cleanup riêng, không app purge/export. | E03 T22; E04 |

Không chép các câu “chưa tạo SQL/code” trong E03 nguyên bản thành trạng thái triển khai hiện nay: đó là ranh giới tại thời điểm thiết kế. Trạng thái mới phải lấy CURRENT_STATE và task card có bằng chứng.

### 8.1. Cách ghi trạng thái triển khai vào báo cáo

- **Thiết kế:** E01–E05 đã Accepted, ADR-160/161.
- **Dữ liệu/SQL:** checkpoint ghi nhận24tables/230columns/50NO ACTION FK/78indexes/70CHECK/20RV;5trigger/6view/6UDF/7SP;4roles/102permissions/6migrations.
- **Ứng dụng:** các stage được duyệt đến R02/R03 đã hoàn tất/review/applied; checkpoint `main fcdd21e`,891unit/HTTP/C1+495SQL=1386PASS, build0warnings0errors. Đây là kết quả **đã lưu**, lượt đối chiếu này không chạy lại test.
- **Vận hành:** default Program chưa mount adapters; production keys/profile/resolver/login readiness và vận hành sản phẩm đầy đủ chưa được xác minh. SQL fixture rollback/source review không tương đương chứng minh runtime triển khai hoặc committed stress/load test.

Không viết “mọi nghiệp vụ trong báo cáo đã triển khai xong” hoặc “hệ thống hoàn chỉnh đang vận hành” chỉ từ thiết kế Accepted hay tổng test của stage nền tảng.

## 9. Thứ tự sửa báo cáo đề nghị

1. Đồng bộ phạm vi, vai trò,37NV/32F và luồng chính ở I–III/V; bỏ nghiệp vụ ngoài phạm vi.
2. Thay VI.2 theo T01–T24 và VI.3 theo constraints hiện hành; sửa danh sách50 FK.
3. Vẽ lại hai hình dữ liệu theo E03, bảo đảm tên trường/quan hệ khớp dictionary mới.
4. Đồng bộ use case và quyền, sửa hình học phí tạo Invoice sai thời điểm.
5. Sửa biểu mẫu/hình UI: /100, không học bổng/biên lai/phiếu điểm download, đúng Tuyển sinh/Admin.
6. Giữ10sequence đã phù hợp; bổ sung các luồng quan trọng còn thiếu theo phạm vi báo cáo.
7. Tách mục thiết kế, mục triển khai đã kiểm chứng và mục vận hành còn chờ; ghi checkpoint/ngày chứng cứ.

Ví dụ diễn đạt luồng chính thống nhất:

> Học viên tự đăng ký tài khoản, tạo hồ sơ đăng ký khóa và đặt ca thi đầu vào. Giáo viên được phân công nhập điểm; hệ thống chọn kết quả tốt nhất trong tối đa hai lượt và xác định Band. Khi không còn lượt chờ xử lý, học viên xác nhận nhập học. Tuyển sinh duyệt hồ sơ đã xác nhận và phát hành một hóa đơn trong cùng giao dịch. Kế toán xác nhận thu đủ tiền một lần; Giáo vụ xếp học viên vào lớp OPEN phù hợp và tạo bảng điểm nháp. Giáo viên điểm danh, lưu điểm/nhận xét, gửi bảng điểm cả lớp sau khi lớp hoàn tất. Giáo vụ trả lại hoặc chốt toàn lớp; học viên chỉ xem kết quả đã chốt.
