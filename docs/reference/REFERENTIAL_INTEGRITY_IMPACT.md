# Bảng tầm ảnh hưởng — Ràng buộc tham chiếu cơ sở dữ liệu trung tâm Anh ngữ

Ngày lập: 06/10/2026. Cập nhật: 07/10/2026.

## 1. Phạm vi và cơ sở đối chiếu

Tài liệu trình bày đầy đủ **50 ràng buộc khóa ngoại của 24 bảng hiện hành**, theo mẫu **Mô tả → Bảng liên quan → Bảng tầm ảnh hưởng → Ghi chú**. Nội dung theo [yêu cầu nghiệp vụ](../requirements.md), [từ điển cơ sở dữ liệu E03](../design/DATABASE_SPEC.md) và cấu hình quan hệ hiện hành. Dùng phần này tại mục VI.3.4 của báo cáo; các ràng buộc miền giá trị, duy nhất, trạng thái và nghiệp vụ phải trình bày riêng.

So với danh sách ban đầu và bản 34 ràng buộc:

- Quan hệ ca thi là `EntryTestBooking.session_id → EntryTestSession.session_id`; `EntryTestAttempt` không có `session_id`.
- Người gửi/người xử lý bảng điểm dùng `GradeReport.submitted_by` và `GradeReport.reviewed_by`.
- Hai FK ghép kiểm tra đúng cặp: kết quả lượt thi với lịch đặt của chính lượt đó; lượt thi tốt nhất với chính hồ sơ đăng ký.
- Bổ sung 16 ràng buộc về phiên đăng nhập, người xử lý, lịch thay thế, điểm gắn buổi và kết quả yêu cầu lặp. Mỗi FK có một bảng tầm ảnh hưởng riêng.

### Danh mục theo bảng chứa khóa ngoại

| Bảng | Số FK | Mục chi tiết |
| --- | --- | --- |
| Account | 1 | 1 |
| StudentProfile | 1 | 2 |
| AuthSession | 1 | 3 |
| TrainingProgram | 2 | 4–5 |
| ScoreComponentConfig | 1 | 6 |
| Registration | 5 | 7–11 |
| EntryTestSession | 2 | 12–13 |
| EntryTestAttempt | 3 | 14–16 |
| EntryTestBooking | 4 | 17–20 |
| BandAssignment | 3 | 21–23 |
| Class | 3 | 24–26 |
| StudentClassAssignment | 3 | 27–29 |
| Schedule | 4 | 30–33 |
| Attendance | 3 | 34–36 |
| Score | 4 | 37–40 |
| GradeReport | 3 | 41–43 |
| Invoice | 2 | 44–45 |
| Payment | 3 | 46–48 |
| AuditLog | 1 | 49 |
| ReplayOperation | 1 | 50 |
| **Tổng** | **50** | **1–50** |

Role, Course, Band và InvoiceNumberCounter không chứa FK. Ba bảng đầu vẫn xuất hiện với vai trò cha; InvoiceNumberCounter không có ràng buộc tham chiếu để lập bảng riêng.

## 2. Quy ước đọc bảng

- **`+`**: thao tác có thể làm vi phạm ràng buộc đang xét, cần kiểm tra.
- **`−`**: thao tác không làm vi phạm riêng ràng buộc đang xét; có thể vẫn bị ràng buộc khác hoặc nghiệp vụ chặn.
- **`+ (thuộc tính)`**: chỉ sửa thuộc tính được chỉ ra mới ảnh hưởng trực tiếp FK này. Sửa thuộc tính khác không ảnh hưởng riêng FK, nhưng vẫn phải kiểm tra nghiệp vụ.
- **Thêm** là thêm bản ghi; **Xóa** là xóa vật lý bản ghi. Đổi status sang CANCELLED/INACTIVE/LOCKED/DISABLED không phải xóa vật lý.
- Bảng cha là bảng được tham chiếu; bảng con chứa FK. Với FK thông thường: thêm con, sửa FK ở con, xóa cha hoặc sửa khóa được tham chiếu ở cha có ảnh hưởng; thêm cha và xóa con không ảnh hưởng riêng FK.
- Với FK nullable, giá trị khác NULL phải tồn tại ở bảng cha. NULL chỉ hợp lệ khi nullability, CHECK bundle và nghiệp vụ đều cho phép; không tự xóa liên kết để né lưu lịch sử.
- Với FK ghép, kiểm tra **cả cặp giá trị**, không kiểm từng ID độc lập. FK ghép nullable ở mục 16 có thể chưa áp dụng kiểm tra tồn tại khi `completed_booking_id` NULL, nhưng CHECK result bundle và FK khác vẫn áp dụng.

### Chính sách dùng chung

Mọi FK trong thiết kế dùng **NO ACTION**, không ON DELETE CASCADE hoặc ON UPDATE CASCADE:

1. Xóa bản ghi cha bị từ chối nếu còn bản ghi con tham chiếu.
2. Sửa khóa được tham chiếu ở cha mà để lại liên kết không hợp lệ bị từ chối; hệ thống **không tự cập nhật khóa ở con**.
3. Các PK nghiệp vụ kiểu int IDENTITY là định danh nội bộ, không có nghiệp vụ đổi ID; SQL Server không cho UPDATE giá trị cột IDENTITY thông thường. Dấu `+` ở cột Sửa cha diễn tả tầm ảnh hưởng lý thuyết, không cấp quyền sửa PK.
4. Dữ liệu lịch sử giữ nguyên liên kết; thao tác ngừng dùng/hủy thực hiện bằng trạng thái trong workflow được phép.
5. FK chỉ chứng minh **tồn tại** (và cùng cặp đối với FK ghép); ACTIVE, đúng vai trò/chủ sở hữu/lớp/chương trình, sức chứa, giờ thao tác và quyền phải kiểm tra thêm bằng nghiệp vụ.
6. Các bảng dưới xét từng FK độc lập. Ví dụ xóa con có `−` với FK trỏ cha, nhưng vẫn có thể bị FK từ bảng khác trỏ vào nó hoặc quy tắc giữ lịch sử chặn.

Với FK tự tham chiếu Schedule, hai dòng biểu diễn vai trò cha/con trong cùng bảng; khi xóa hoặc sửa phải xét cả liên kết đi vào và đi ra.

## 3. Bảng tầm ảnh hưởng từng ràng buộc

### 1. Ràng buộc: Account(role_id) → Role(role_id)

**Mô tả:** Mỗi tài khoản phải thuộc một vai trò đang tồn tại.

**Bảng liên quan:** Account (role_id, bảng con), Role (role_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Role | − | + (`role_id`) | + (`role_id`) |
| Account | + (`role_id`) | − | + (`role_id`) |

**Ghi chú:**

- `role_id` không được NULL; giá trị tham chiếu phải tồn tại trong Role.
- Không xóa Role khi còn Account tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Mỗi Account có đúng một vai trò cố định trong bảy vai trò của hệ thống; tồn tại Role là điều kiện FK, còn mã vai trò hợp lệ được kiểm tra riêng.
- Không có nghiệp vụ đổi vai trò của tài khoản đã tạo.

### 2. Ràng buộc: StudentProfile(account_id) → Account(account_id)

**Mô tả:** Mỗi hồ sơ học viên phải thuộc một tài khoản đang tồn tại.

**Bảng liên quan:** StudentProfile (account_id, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| StudentProfile | + (`account_id`) | − | + (`account_id`) |

**Ghi chú:**

- `account_id` không được NULL; giá trị tham chiếu phải tồn tại trong Account.
- Không xóa Account khi còn StudentProfile tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Account được liên kết phải có vai trò Student; FK không tự kiểm tra vai trò.
- Một Account có tối đa một StudentProfile; đăng ký học viên tạo Account và StudentProfile trong cùng transaction.
- Không chuyển hồ sơ học viên sang tài khoản khác.

### 3. Ràng buộc: AuthSession(account_id) → Account(account_id)

**Mô tả:** Mỗi phiên đăng nhập phải thuộc một tài khoản đang tồn tại.

**Bảng liên quan:** AuthSession (account_id, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| AuthSession | + (`account_id`) | − | + (`account_id`) |

**Ghi chú:**

- `account_id` không được NULL; giá trị phải tồn tại trong Account.
- Không xóa Account khi còn AuthSession tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Chủ sở hữu phiên được xác định khi cấp phiên và không được chuyển sang tài khoản khác.
- Đăng xuất, đổi mật khẩu hoặc khóa tài khoản thu hồi phiên theo quy trình; thu hồi không phải xóa tài khoản hoặc xóa dây chuyền phiên.
- Xác thực phải kiểm tra tài khoản, phiên, thời hạn và phiên bản bảo mật hiện hành; FK không chứng minh phiên còn hiệu lực.

### 4. Ràng buộc: TrainingProgram(course_id) → Course(course_id)

**Mô tả:** Mỗi chương trình đào tạo phải thuộc một khóa học đang tồn tại.

**Bảng liên quan:** TrainingProgram (course_id, bảng con), Course (course_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Course | − | + (`course_id`) | + (`course_id`) |
| TrainingProgram | + (`course_id`) | − | + (`course_id`) |

**Ghi chú:**

- `course_id` không được NULL; giá trị tham chiếu phải tồn tại trong Course.
- Không xóa Course khi còn TrainingProgram tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Cặp Course/Band của chương trình là duy nhất.
- Chỉ thay cấu hình trước lần sử dụng Class đầu tiên và theo trạng thái cấu hình được phép; đã dùng thì course_id bị khóa, kể cả Class đã CANCELLED.
- Course INACTIVE vẫn tồn tại và được giữ cho lịch sử; FK không kiểm tra ACTIVE.

### 5. Ràng buộc: TrainingProgram(band_id) → Band(band_id)

**Mô tả:** Mỗi chương trình đào tạo phải gắn với một Band đang tồn tại.

**Bảng liên quan:** TrainingProgram (band_id, bảng con), Band (band_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Band | − | + (`band_id`) | + (`band_id`) |
| TrainingProgram | + (`band_id`) | − | + (`band_id`) |

**Ghi chú:**

- `band_id` không được NULL; giá trị tham chiếu phải tồn tại trong Band.
- Không xóa Band khi còn TrainingProgram tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Cặp Course/Band là duy nhất; không tạo chương trình trùng để né khóa cấu hình.
- band_id bị khóa sau khi bất kỳ Class nào sử dụng chương trình; trước đó chỉ sửa trong cấu hình hợp lệ.
- Sửa min_score của Band không phải sửa khóa band_id; vẫn chịu quy tắc đóng băng ngưỡng riêng.

### 6. Ràng buộc: ScoreComponentConfig(program_id) → TrainingProgram(program_id)

**Mô tả:** Mỗi thành phần điểm phải thuộc một chương trình đào tạo đang tồn tại.

**Bảng liên quan:** ScoreComponentConfig (program_id, bảng con), TrainingProgram (program_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| TrainingProgram | − | + (`program_id`) | + (`program_id`) |
| ScoreComponentConfig | + (`program_id`) | − | + (`program_id`) |

**Ghi chú:**

- `program_id` không được NULL; giá trị tham chiếu phải tồn tại trong TrainingProgram.
- Không xóa TrainingProgram khi còn ScoreComponentConfig tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Danh sách thành phần, loại và trọng số bị khóa sau lần sử dụng Class đầu tiên.
- Chỉ chỉnh cấu hình nháp chưa dùng theo transaction cấu hình toàn chương trình; không chuyển thành phần đã dùng sang chương trình khác.
- Tổng trọng số 100 và đúng một thành phần FINAL_EXAM khi kích hoạt là các ràng buộc nghiệp vụ riêng.

### 7. Ràng buộc: Registration(student_id) → StudentProfile(student_id)

**Mô tả:** Mỗi hồ sơ đăng ký khóa học phải thuộc một học viên đang tồn tại.

**Bảng liên quan:** Registration (student_id, bảng con), StudentProfile (student_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| StudentProfile | − | + (`student_id`) | + (`student_id`) |
| Registration | + (`student_id`) | − | + (`student_id`) |

**Ghi chú:**

- `student_id` không được NULL; giá trị tham chiếu phải tồn tại trong StudentProfile.
- Không xóa StudentProfile khi còn Registration tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Chủ sở hữu lấy từ tài khoản Student đã xác thực; không cho nhân sự đăng ký thay hoặc client chọn học viên khác.
- Không chuyển Registration sang học viên khác; mỗi học viên chỉ có một hồ sơ chưa kết thúc cho cùng Course.
- Hủy hồ sơ là đổi trạng thái, không xóa StudentProfile hoặc Registration.

### 8. Ràng buộc: Registration(course_id) → Course(course_id)

**Mô tả:** Mỗi hồ sơ đăng ký phải chọn một khóa học đang tồn tại.

**Bảng liên quan:** Registration (course_id, bảng con), Course (course_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Course | − | + (`course_id`) | + (`course_id`) |
| Registration | + (`course_id`) | − | + (`course_id`) |

**Ghi chú:**

- `course_id` không được NULL; giá trị tham chiếu phải tồn tại trong Course.
- Không xóa Course khi còn Registration tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Đăng ký mới chỉ chọn Course ACTIVE; FK chỉ kiểm tra tồn tại.
- course_id bất biến từ lúc tạo; đổi lựa chọn phải hủy hợp lệ và tạo hồ sơ mới, thi đầu vào lại.
- Course INACTIVE không tự hủy hồ sơ đã tồn tại.

### 9. Ràng buộc: Registration(admission_approved_by) → Account(account_id)

**Mô tả:** Người duyệt nhập học được ghi nhận phải là tài khoản đang tồn tại.

**Bảng liên quan:** Registration (admission_approved_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Registration | + (`admission_approved_by`) | − | + (`admission_approved_by`) |

**Ghi chú:**

- FK cho phép NULL theo điều kiện nêu dưới đây; khi có giá trị tham chiếu, bản ghi tương ứng phải tồn tại.
- Không xóa Account khi còn Registration tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Cho phép NULL khi chưa duyệt; admission_approved_by và admission_approved_at phải cùng NULL hoặc cùng có giá trị.
- Khi duyệt, người thao tác phải có quyền Admissions; chuyển ADMITTED, tạo Invoice và ghi audit trong cùng transaction.
- Giữ người/thời điểm duyệt đã có khi hồ sơ bị hủy hợp lệ; không đổi người duyệt bằng sửa hồ sơ thông thường.

### 10. Ràng buộc: Registration(rejected_by) → Account(account_id)

**Mô tả:** Người từ chối hồ sơ đăng ký phải là tài khoản đang tồn tại.

**Bảng liên quan:** Registration (rejected_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Registration | + (`rejected_by`) | − | + (`rejected_by`) |

**Ghi chú:**

- `rejected_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn Registration tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Nhân viên tuyển sinh (Admissions) từ chối hồ sơ CONFIRMED theo điều kiện nghiệp vụ.
- Người, thời điểm và lý do từ chối đi cùng trạng thái REJECTED và nhật ký trong cùng giao dịch.
- Giữ thông tin người từ chối; không sửa tùy ý hoặc đặt NULL để bỏ lịch sử xử lý.

### 11. Ràng buộc: Registration(cancelled_by) → Account(account_id)

**Mô tả:** Người hủy hồ sơ đăng ký phải là tài khoản đang tồn tại.

**Bảng liên quan:** Registration (cancelled_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Registration | + (`cancelled_by`) | − | + (`cancelled_by`) |

**Ghi chú:**

- `cancelled_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn Registration tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Học viên chỉ hủy hồ sơ của mình trước xác nhận nhập học; nhân viên tuyển sinh hủy hồ sơ CONFIRMED hoặc ADMITTED theo điều kiện đã chốt. Hóa đơn đã có phải UNPAID.
- Nhân viên tuyển sinh phải cung cấp lý do; không áp thêm yêu cầu này cho đường học viên tự hủy.
- Hủy hồ sơ, xử lý lịch đặt thi liên quan, hủy hóa đơn chưa thanh toán và ghi nhật ký trong cùng giao dịch; giữ người xử lý và lịch sử.

### 12. Ràng buộc: EntryTestSession(grader_id) → Account(account_id)

**Mô tả:** Mỗi ca thi đầu vào phải phân công một tài khoản giáo viên đang tồn tại.

**Bảng liên quan:** EntryTestSession (grader_id, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| EntryTestSession | + (`grader_id`) | − | + (`grader_id`) |

**Ghi chú:**

- `grader_id` không được NULL; giá trị tham chiếu phải tồn tại trong Account.
- Không xóa Account khi còn EntryTestSession tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Tạo/phân công cần Teacher ACTIVE và kiểm tra xung đột lịch; FK không bảo đảm các điều kiện này.
- Chỉ AcademicAffairs đổi giáo viên chấm trước ca bắt đầu và trước mọi điểm hoặc ABSENT, có lý do và audit.
- Account LOCKED/DISABLED không làm mất liên kết hoặc tự đổi giáo viên chấm.

### 13. Ràng buộc: EntryTestSession(cancelled_by) → Account(account_id)

**Mô tả:** Người hủy ca thi đầu vào phải là tài khoản đang tồn tại.

**Bảng liên quan:** EntryTestSession (cancelled_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| EntryTestSession | + (`cancelled_by`) | − | + (`cancelled_by`) |

**Ghi chú:**

- `cancelled_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn EntryTestSession tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Nhân viên giáo vụ (AcademicAffairs) hủy ca trước khi ca bắt đầu theo điều kiện nghiệp vụ, có lý do.
- Ghi người hủy, thời điểm, lý do và xử lý các lịch đặt bị ảnh hưởng trong cùng giao dịch với nhật ký.
- Giữ ca thi và lịch đặt; không xóa hoặc đổi người hủy để bỏ lịch sử.

### 14. Ràng buộc: EntryTestAttempt(registration_id) → Registration(registration_id)

**Mô tả:** Mỗi lượt thi đầu vào phải thuộc một hồ sơ đăng ký đang tồn tại.

**Bảng liên quan:** EntryTestAttempt (registration_id, bảng con), Registration (registration_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Registration | − | + (`registration_id`) | + (`registration_id`) |
| EntryTestAttempt | + (`registration_id`) | − | + (`registration_id`) |

**Ghi chú:**

- `registration_id` không được NULL; giá trị tham chiếu phải tồn tại trong Registration.
- Không xóa Registration khi còn EntryTestAttempt tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Tối đa hai lượt logic cho mỗi Registration; lượt hai cần kết quả lượt một và phải trước xác nhận nhập học.
- registration_id của lượt thi được giữ cố định; hủy/đổi lịch hoặc vắng không chuyển lượt sang hồ sơ khác.
- Hồ sơ CANCELLED vẫn tồn tại để giữ lịch sử, nhưng chặn chấm/sửa điểm và ghi vắng theo nghiệp vụ.

### 15. Ràng buộc: EntryTestAttempt(graded_by) → Account(account_id)

**Mô tả:** Người nhập điểm đầu vào ban đầu phải là tài khoản đang tồn tại.

**Bảng liên quan:** EntryTestAttempt (graded_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| EntryTestAttempt | + (`graded_by`) | − | + (`graded_by`) |

**Ghi chú:**

- FK cho phép NULL theo điều kiện nêu dưới đây; khi có giá trị tham chiếu, bản ghi tương ứng phải tồn tại.
- Không xóa Account khi còn EntryTestAttempt tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- NULL khi chưa có kết quả; score, graded_by, graded_at và completed_booking_id phải cùng NULL hoặc cùng có.
- Khi nhập điểm, actor phải là Teacher đang được phân công ca và chỉ nhập từ khi ca kết thúc.
- graded_by lưu người chấm ban đầu, không đổi khi sửa điểm; chỉ người này còn được phân công mới được sửa trong cửa sổ đã chốt, có lý do/audit.

### 16. Ràng buộc: EntryTestAttempt(completed_booking_id, attempt_id) → EntryTestBooking(booking_id, attempt_id)

**Mô tả:** Kết quả lượt thi phải tham chiếu Booking đã hoàn thành của chính lượt đó.

**Bảng liên quan:** EntryTestAttempt (completed_booking_id, attempt_id, bảng con), EntryTestBooking (booking_id, attempt_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| EntryTestBooking | − | + (`booking_id`, `attempt_id`) | + (`booking_id`, `attempt_id`) |
| EntryTestAttempt | + (`completed_booking_id`, `attempt_id`) | − | + (`completed_booking_id`, `attempt_id`) |

**Ghi chú:**

- FK cho phép NULL theo điều kiện nêu dưới đây; khi có giá trị tham chiếu, bản ghi tương ứng phải tồn tại.
- Không xóa EntryTestBooking khi còn EntryTestAttempt tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Bổ sung quan hệ ghép trong thiết kế hiện hành; không trỏ Booking của lượt khác.
- completed_booking_id NULL khi chưa có kết quả; attempt_id luôn bắt buộc. Khi có kết quả, cặp tham chiếu phải tồn tại; CHECK result bundle buộc các trường kết quả cùng có.
- Booking được tham chiếu phải COMPLETED theo nghiệp vụ; FK ghép chỉ bảo đảm tồn tại/cùng lượt.
- Khởi tạo Attempt chưa kết quả, tạo Booking rồi ghi kết quả để xử lý vòng tham chiếu; chấm điểm và cập nhật Booking/Band/audit cùng transaction.

### 17. Ràng buộc: EntryTestBooking(attempt_id) → EntryTestAttempt(attempt_id)

**Mô tả:** Mỗi lần đặt lịch phải thuộc một lượt thi logic đang tồn tại.

**Bảng liên quan:** EntryTestBooking (attempt_id, bảng con), EntryTestAttempt (attempt_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| EntryTestAttempt | − | + (`attempt_id`) | + (`attempt_id`) |
| EntryTestBooking | + (`attempt_id`) | − | + (`attempt_id`) |

**Ghi chú:**

- `attempt_id` không được NULL; giá trị tham chiếu phải tồn tại trong EntryTestAttempt.
- Không xóa EntryTestAttempt khi còn EntryTestBooking tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Bổ sung để mô tả đầy đủ đường nối Registration → Attempt → Booking → Session sau khi sửa quan hệ ca thi.
- attempt_id của Booking bất biến; đặt lại sau ABSENT/CANCELLED vẫn cùng lượt chưa hoàn thành, không tăng số lượt.
- Tối đa một Booking SCHEDULED và một Booking COMPLETED cho một Attempt; kiểm tra trạng thái/result trong transaction.

### 18. Ràng buộc: EntryTestBooking(session_id) → EntryTestSession(session_id)

**Mô tả:** Mỗi lần đặt lịch thi phải tham chiếu một ca thi đầu vào đang tồn tại.

**Bảng liên quan:** EntryTestBooking (session_id, bảng con), EntryTestSession (session_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| EntryTestSession | − | + (`session_id`) | + (`session_id`) |
| EntryTestBooking | + (`session_id`) | − | + (`session_id`) |

**Ghi chú:**

- `session_id` không được NULL; giá trị tham chiếu phải tồn tại trong EntryTestSession.
- Không xóa EntryTestSession khi còn EntryTestBooking tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Đây là quan hệ hiện hành thay cho EntryTestAttempt.session_id trong danh sách ban đầu; EntryTestAttempt không có cột session_id.
- Đặt mới cần ca OPEN, chưa bắt đầu, còn chỗ và không trùng lịch học viên; FK chỉ bảo đảm ca tồn tại.
- session_id của Booking bất biến; đổi ca hủy Booking cũ và tạo Booking mới trong cùng transaction, giữ lịch sử.
- Hủy/đóng ca không xóa ca hoặc Booking; CLOSED không đồng nghĩa CANCELLED.

### 19. Ràng buộc: EntryTestBooking(absent_by) → Account(account_id)

**Mô tả:** Người xác nhận học viên vắng thi phải là tài khoản đang tồn tại.

**Bảng liên quan:** EntryTestBooking (absent_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| EntryTestBooking | + (`absent_by`) | − | + (`absent_by`) |

**Ghi chú:**

- `absent_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn EntryTestBooking tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Giáo viên được phân công ca xác nhận vắng sau khi ca kết thúc, có kiểm tra quyền và trạng thái hồ sơ/lịch đặt.
- Ghi ABSENT cùng người, thời điểm và mã thao tác theo nhóm trường ghi vắng; không suy ra vắng từ việc chưa có điểm.
- Vắng không tăng số lượt thi logic; có thể đặt lại cùng lượt chưa hoàn thành theo điều kiện nghiệp vụ và giữ lịch đặt cũ.

### 20. Ràng buộc: EntryTestBooking(cancelled_by) → Account(account_id)

**Mô tả:** Người hủy hoặc đổi lịch thi phải là tài khoản đang tồn tại.

**Bảng liên quan:** EntryTestBooking (cancelled_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| EntryTestBooking | + (`cancelled_by`) | − | + (`cancelled_by`) |

**Ghi chú:**

- `cancelled_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn EntryTestBooking tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Người hủy có thể là học viên tự hủy/đổi lịch hợp lệ, nhân viên giáo vụ hủy ca, hoặc người hủy hồ sơ đăng ký theo đúng quyền và điều kiện thao tác gốc.
- Đường học viên hủy/đổi lịch cần ca OPEN và chưa bắt đầu; đường hủy từ hồ sơ hoặc ca tuân theo điều kiện riêng.
- Ghi đồng bộ nhóm thông tin hủy và giữ lý do từ thao tác cha khi có. Đổi lịch tạo bản ghi mới, không ghi đè lịch cũ.

### 21. Ràng buộc: BandAssignment(registration_id) → Registration(registration_id)

**Mô tả:** Mỗi kết quả xếp Band phải thuộc một hồ sơ đăng ký đang tồn tại.

**Bảng liên quan:** BandAssignment (registration_id, bảng con), Registration (registration_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Registration | − | + (`registration_id`) | + (`registration_id`) |
| BandAssignment | + (`registration_id`) | − | + (`registration_id`) |

**Ghi chú:**

- `registration_id` không được NULL; giá trị tham chiếu phải tồn tại trong Registration.
- Không xóa Registration khi còn BandAssignment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Mỗi Registration có tối đa một BandAssignment; chưa có điểm hoàn thành thì chưa tạo bản ghi.
- Không chuyển kết quả Band sang Registration khác; chấm/sửa điểm và cập nhật Band phải nguyên tử.
- best_attempt_id còn phải thuộc chính Registration này, được bảo vệ bởi FK ghép ở mục kế tiếp.

### 22. Ràng buộc: BandAssignment(best_attempt_id, registration_id) → EntryTestAttempt(attempt_id, registration_id)

**Mô tả:** Lượt thi tốt nhất phải tồn tại và thuộc đúng hồ sơ đăng ký được xếp Band.

**Bảng liên quan:** BandAssignment (best_attempt_id, registration_id, bảng con), EntryTestAttempt (attempt_id, registration_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| EntryTestAttempt | − | + (`attempt_id`, `registration_id`) | + (`attempt_id`, `registration_id`) |
| BandAssignment | + (`best_attempt_id`, `registration_id`) | − | + (`best_attempt_id`, `registration_id`) |

**Ghi chú:**

- `best_attempt_id, registration_id` không được NULL; giá trị tham chiếu phải tồn tại trong EntryTestAttempt.
- Không xóa EntryTestAttempt khi còn BandAssignment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Quan hệ đơn best_attempt_id → attempt_id trong danh sách được triển khai bằng FK ghép (best_attempt_id, registration_id) → (attempt_id, registration_id).
- Lượt được chọn phải có kết quả hoàn thành và điểm cao nhất; nếu bằng điểm thì chọn lượt một. FK ghép chỉ bảo đảm tồn tại/cùng hồ sơ.
- Khi sửa điểm hợp lệ, chọn lại lượt tốt nhất và Band cùng transaction; từ CONFIRMED hoặc hồ sơ đã kết thúc thì không sửa kết quả.
- Cặp ở EntryTestAttempt là khóa duy nhất thay thế (alternate key); sửa bất kỳ thành phần nào cũng có thể ảnh hưởng FK.

### 23. Ràng buộc: BandAssignment(band_id) → Band(band_id)

**Mô tả:** Kết quả xếp Band phải tham chiếu một Band đang tồn tại.

**Bảng liên quan:** BandAssignment (band_id, bảng con), Band (band_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Band | − | + (`band_id`) | + (`band_id`) |
| BandAssignment | + (`band_id`) | − | + (`band_id`) |

**Ghi chú:**

- `band_id` không được NULL; giá trị tham chiếu phải tồn tại trong Band.
- Không xóa Band khi còn BandAssignment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Band được chọn bằng ngưỡng min_score lớn nhất không vượt điểm tốt nhất; không cho chọn tùy ý chỉ vì Band tồn tại.
- Ngưỡng seed 0/25/45/65/85 và khóa ngưỡng sau kết quả đầu tiên là nghiệp vụ riêng.
- Cập nhật band_id chỉ qua chấm/sửa điểm hợp lệ và tính lại kết quả trước xác nhận; không sửa tay kết quả đã khóa.

### 24. Ràng buộc: Class(program_id) → TrainingProgram(program_id)

**Mô tả:** Mỗi lớp phải sử dụng một chương trình đào tạo đang tồn tại.

**Bảng liên quan:** Class (program_id, bảng con), TrainingProgram (program_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| TrainingProgram | − | + (`program_id`) | + (`program_id`) |
| Class | + (`program_id`) | − | + (`program_id`) |

**Ghi chú:**

- `program_id` không được NULL; giá trị tham chiếu phải tồn tại trong TrainingProgram.
- Không xóa TrainingProgram khi còn Class tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- program_id bất biến sau khi tạo Class; chọn sai thì xử lý hủy lớp hợp lệ và tạo lớp mới.
- Tạo/mở lớp phải kiểm tra trạng thái chương trình theo nghiệp vụ; FK không tự bảo đảm ACTIVE.
- Bất kỳ Class nào từng tham chiếu chương trình, kể cả CANCELLED, đều làm khóa cấu hình đã dùng.

### 25. Ràng buộc: Class(teacher_id) → Account(account_id)

**Mô tả:** Mỗi lớp phải có một tài khoản giáo viên phụ trách đang tồn tại.

**Bảng liên quan:** Class (teacher_id, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Class | + (`teacher_id`) | − | + (`teacher_id`) |

**Ghi chú:**

- `teacher_id` không được NULL; giá trị tham chiếu phải tồn tại trong Account.
- Không xóa Account khi còn Class tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Phân công cần Teacher ACTIVE và kiểm tra toàn bộ xung đột lịch lớp/ca thi.
- Chỉ AcademicAffairs đổi giáo viên trước ONGOING và trước khi có Attendance/Score; không đổi khi đã bắt đầu dạy.
- Tài khoản bị khóa/ngừng dùng vẫn giữ liên kết; không tự chuyển giáo viên hoặc hủy lớp.

### 26. Ràng buộc: Class(cancelled_by) → Account(account_id)

**Mô tả:** Người hủy lớp phải là tài khoản đang tồn tại.

**Bảng liên quan:** Class (cancelled_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Class | + (`cancelled_by`) | − | + (`cancelled_by`) |

**Ghi chú:**

- `cancelled_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn Class tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Nhân viên giáo vụ hủy lớp PLANNED/OPEN khi lớp chưa có học viên và đáp ứng điều kiện chưa tổ chức giảng dạy.
- Người hủy, thời điểm, lý do, trạng thái CANCELLED và nhật ký được ghi đồng bộ.
- Giữ lớp và lịch sử; hủy lớp không mở lại cấu hình chương trình đã khóa bởi lần sử dụng lớp đầu tiên.

### 27. Ràng buộc: StudentClassAssignment(class_id) → Class(class_id)

**Mô tả:** Mỗi lần xếp lớp phải tham chiếu một lớp đang tồn tại.

**Bảng liên quan:** StudentClassAssignment (class_id, bảng con), Class (class_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Class | − | + (`class_id`) | + (`class_id`) |
| StudentClassAssignment | + (`class_id`) | − | + (`class_id`) |

**Ghi chú:**

- `class_id` không được NULL; giá trị tham chiếu phải tồn tại trong Class.
- Không xóa Class khi còn StudentClassAssignment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Xếp vào Class OPEN, đúng Course/Band, còn sức chứa và không trùng lịch học viên.
- Liên kết class_id bất biến; không gỡ/chuyển học viên sang lớp khác kể cả trước khi dạy.
- Tạo Assignment, GradeReport DRAFT và audit cùng transaction; FK không thay kiểm tra điều kiện xếp lớp.

### 28. Ràng buộc: StudentClassAssignment(registration_id) → Registration(registration_id)

**Mô tả:** Mỗi lần xếp lớp phải gắn với một hồ sơ đăng ký đang tồn tại.

**Bảng liên quan:** StudentClassAssignment (registration_id, bảng con), Registration (registration_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Registration | − | + (`registration_id`) | + (`registration_id`) |
| StudentClassAssignment | + (`registration_id`) | − | + (`registration_id`) |

**Ghi chú:**

- `registration_id` không được NULL; giá trị tham chiếu phải tồn tại trong Registration.
- Không xóa Registration khi còn StudentClassAssignment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Một Registration có tối đa một Assignment; hồ sơ phải ADMITTED và Invoice PAID.
- Không đổi registration_id để chuyển lượt học hoặc chủ sở hữu; giữ lịch sử xếp lớp.
- Class COMPLETED và GradeReport FINALIZED mới kết thúc lượt học cho quyền đăng ký lại, không chỉ dựa vào Assignment tồn tại.

### 29. Ràng buộc: StudentClassAssignment(assigned_by) → Account(account_id)

**Mô tả:** Người thực hiện xếp học viên vào lớp phải là tài khoản đang tồn tại.

**Bảng liên quan:** StudentClassAssignment (assigned_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| StudentClassAssignment | + (`assigned_by`) | − | + (`assigned_by`) |

**Ghi chú:**

- `assigned_by` không được NULL; giá trị phải tồn tại trong Account.
- Không xóa Account khi còn StudentClassAssignment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Nhân viên giáo vụ thực hiện xếp lớp; FK không tự chứng minh đúng vai trò.
- Xếp lớp, tạo bảng điểm DRAFT và nhật ký trong cùng giao dịch; kiểm tra hồ sơ ADMITTED, hóa đơn PAID, lớp OPEN, khóa học/trình độ phù hợp, sức chứa và xung đột lịch.
- Người xếp lớp là thông tin nguồn gốc bất biến; không chuyển lớp, đổi hồ sơ hoặc xóa liên kết đã tạo theo quy trình hiện hành.

### 30. Ràng buộc: Schedule(class_id) → Class(class_id)

**Mô tả:** Mỗi buổi học hoặc thi cuối khóa phải thuộc một lớp đang tồn tại.

**Bảng liên quan:** Schedule (class_id, bảng con), Class (class_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Class | − | + (`class_id`) | + (`class_id`) |
| Schedule | + (`class_id`) | − | + (`class_id`) |

**Ghi chú:**

- `class_id` không được NULL; giá trị tham chiếu phải tồn tại trong Class.
- Không xóa Class khi còn Schedule tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Không chuyển buổi lịch sang lớp khác; chuỗi thay thế giữ cùng Class, loại buổi và nghĩa vụ.
- Tạo/dời/thay buổi phải kiểm tra lifecycle, thời gian, số nghĩa vụ và xung đột phòng/giáo viên/học viên.
- Class CANCELLED vẫn giữ Schedule làm lịch sử nhưng không giữ chỗ tài nguyên; không xóa dây chuyền lịch.

### 31. Ràng buộc: Schedule(replaces_schedule_id) → Schedule(schedule_id)

**Mô tả:** Một buổi thay thế phải tham chiếu buổi lịch tiền nhiệm đang tồn tại.

**Bảng liên quan:** Schedule (replaces_schedule_id, bảng con), Schedule (schedule_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Schedule — buổi được tham chiếu (vai trò cha) | − | + (`schedule_id`) | + (`schedule_id`) |
| Schedule — buổi tham chiếu tiền nhiệm (vai trò con) | + (`replaces_schedule_id`) | − | + (`replaces_schedule_id`) |

**Ghi chú:**

- `replaces_schedule_id` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Schedule. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Schedule khi còn Schedule tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Buổi gốc có liên kết NULL; buổi thay thế trỏ đến tiền nhiệm. Mỗi tiền nhiệm có tối đa một buổi kế tiếp; không tự trỏ, tạo nhánh hoặc vòng.
- Chuỗi giữ cùng lớp, loại buổi và nghĩa vụ tổ chức. Buổi cũ NOT_HELD và không còn hiện hành; buổi mới ở tương lai, PLANNED và hiện hành, được cập nhật cùng giao dịch.
- Dời buổi trước giờ bắt đầu sửa thời gian/phòng trên cùng bản ghi; thay buổi không tổ chức tạo bản ghi mới và giữ lịch sử.
- Hai dòng dưới là hai vai trò của cùng bảng. Xóa một Schedule có ảnh hưởng nếu buổi khác đang tham chiếu nó; dấu − ở vai trò con chỉ xét việc bỏ liên kết đi ra.

### 32. Ràng buộc: Schedule(held_by) → Account(account_id)

**Mô tả:** Người xác nhận buổi học hoặc thi đã tổ chức phải là tài khoản đang tồn tại.

**Bảng liên quan:** Schedule (held_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Schedule | + (`held_by`) | − | + (`held_by`) |

**Ghi chú:**

- `held_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn Schedule tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Nhân viên giáo vụ xác nhận HELD sau giờ kết thúc dự kiến, khi lớp ONGOING; người và thời điểm xác nhận đi cùng trạng thái buổi.
- Không suy ra đã tổ chức từ đồng hồ, điểm danh hoặc điểm; FK chỉ bảo đảm người xác nhận tồn tại.
- Giữ thông tin xác nhận; không thay người xác nhận bằng thao tác sửa lịch thông thường.

### 33. Ràng buộc: Schedule(not_held_by) → Account(account_id)

**Mô tả:** Người xác nhận buổi không tổ chức phải là tài khoản đang tồn tại.

**Bảng liên quan:** Schedule (not_held_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Schedule | + (`not_held_by`) | − | + (`not_held_by`) |

**Ghi chú:**

- `not_held_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn Schedule tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Nhân viên giáo vụ ghi NOT_HELD tại hoặc sau giờ bắt đầu, có lý do, theo trạng thái lớp cho phép; buổi không được có điểm danh hoặc điểm liên kết.
- Ghi đồng bộ người, thời điểm, lý do và buổi không còn hiện hành; tạo buổi thay thế và nhật ký cùng giao dịch theo quy trình thay buổi.
- Giữ buổi cũ và người xử lý; không xóa bản ghi để tránh lịch sử chuỗi thay thế.

### 34. Ràng buộc: Attendance(schedule_id) → Schedule(schedule_id)

**Mô tả:** Mỗi bản ghi điểm danh phải tham chiếu một buổi lịch đang tồn tại.

**Bảng liên quan:** Attendance (schedule_id, bảng con), Schedule (schedule_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Schedule | − | + (`schedule_id`) | + (`schedule_id`) |
| Attendance | + (`schedule_id`) | − | + (`schedule_id`) |

**Ghi chú:**

- `schedule_id` không được NULL; giá trị tham chiếu phải tồn tại trong Schedule.
- Không xóa Schedule khi còn Attendance tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Schedule phải là LESSON hiện hành, thuộc cùng Class với Assignment; FK không tự bảo đảm các điều kiện này.
- Teacher phụ trách nhập/sửa từ lúc LESSON bắt đầu đến trước Class COMPLETED; chưa nhập là không có row.
- Không chuyển Attendance sang buổi khác để né cutoff hoặc tạo điểm danh giả cho buổi NOT_HELD.

### 35. Ràng buộc: Attendance(class_assignment_id) → StudentClassAssignment(class_assignment_id)

**Mô tả:** Mỗi bản ghi điểm danh phải thuộc một học viên đã được xếp lớp và có Assignment đang tồn tại.

**Bảng liên quan:** Attendance (class_assignment_id, bảng con), StudentClassAssignment (class_assignment_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| StudentClassAssignment | − | + (`class_assignment_id`) | + (`class_assignment_id`) |
| Attendance | + (`class_assignment_id`) | − | + (`class_assignment_id`) |

**Ghi chú:**

- `class_assignment_id` không được NULL; giá trị tham chiếu phải tồn tại trong StudentClassAssignment.
- Không xóa StudentClassAssignment khi còn Attendance tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Assignment phải thuộc cùng lớp với Schedule; một cặp Schedule/Assignment có tối đa một Attendance.
- Lưu các giá trị chủ động chọn/thay đổi của một LESSON theo batch toàn bộ hoặc không phần nào.
- Không chuyển điểm danh sang Assignment của người khác; Class COMPLETED thì chỉ đọc.

### 36. Ràng buộc: Attendance(entered_by) → Account(account_id)

**Mô tả:** Người lưu điểm danh phải là tài khoản đang tồn tại.

**Bảng liên quan:** Attendance (entered_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Attendance | + (`entered_by`) | − | + (`entered_by`) |

**Ghi chú:**

- `entered_by` không được NULL; giá trị phải tồn tại trong Account.
- Không xóa Account khi còn Attendance tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Giáo viên hiện phụ trách nhập/sửa điểm danh cho LESSON hiện hành của đúng lớp, từ giờ bắt đầu đến trước khi lớp COMPLETED.
- entered_by và entered_at ghi người/thời điểm lần lưu thay đổi thực tế gần nhất; nhật ký giữ lịch sử cũ/mới.
- Lưu các giá trị chủ động chọn hoặc thay đổi của một buổi theo lô trong cùng giao dịch; chưa nhập không tạo bản ghi giả hoặc tự coi là vắng.

### 37. Ràng buộc: Score(class_assignment_id) → StudentClassAssignment(class_assignment_id)

**Mô tả:** Mỗi điểm thành phần phải thuộc một lượt học đã được xếp lớp và đang tồn tại.

**Bảng liên quan:** Score (class_assignment_id, bảng con), StudentClassAssignment (class_assignment_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| StudentClassAssignment | − | + (`class_assignment_id`) | + (`class_assignment_id`) |
| Score | + (`class_assignment_id`) | − | + (`class_assignment_id`) |

**Ghi chú:**

- `class_assignment_id` không được NULL; giá trị tham chiếu phải tồn tại trong StudentClassAssignment.
- Không xóa StudentClassAssignment khi còn Score tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Teacher hiện phụ trách lớp mới được lưu điểm; report phải DRAFT/RETURNED và đáp ứng cửa sổ nhập điểm.
- Một Assignment/component có tối đa một Score; không chuyển điểm sang Assignment khác.
- Lưu điểm/nhận xét theo một học viên, chạm version GradeReport khi có thay đổi thực tế và ghi audit bắt buộc cùng transaction.

### 38. Ràng buộc: Score(component_id) → ScoreComponentConfig(component_id)

**Mô tả:** Mỗi điểm phải thuộc một thành phần điểm đang tồn tại.

**Bảng liên quan:** Score (component_id, bảng con), ScoreComponentConfig (component_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| ScoreComponentConfig | − | + (`component_id`) | + (`component_id`) |
| Score | + (`component_id`) | − | + (`component_id`) |

**Ghi chú:**

- `component_id` không được NULL; giá trị tham chiếu phải tồn tại trong ScoreComponentConfig.
- Không xóa ScoreComponentConfig khi còn Score tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Component phải thuộc TrainingProgram của Class chứa Assignment; FK chỉ kiểm tra component tồn tại.
- GENERAL không liên kết Schedule; LESSON/FINAL_EXAM phải gắn đúng loại lịch hiện hành và đã đến giờ nhập.
- Không đổi component_id để chuyển điểm; danh sách thành phần đã dùng bị khóa, SUBMITTED/FINALIZED khóa sửa điểm.

### 39. Ràng buộc: Score(entered_by) → Account(account_id)

**Mô tả:** Người lưu điểm thành phần phải là tài khoản đang tồn tại.

**Bảng liên quan:** Score (entered_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Score | + (`entered_by`) | − | + (`entered_by`) |

**Ghi chú:**

- `entered_by` không được NULL; giá trị tham chiếu phải tồn tại trong Account.
- Không xóa Account khi còn Score tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Actor phải là Teacher hiện phụ trách Class và có quyền lưu điểm tại thời điểm thao tác.
- entered_by/entered_at ghi lần lưu thay đổi thực tế gần nhất; không sửa tùy ý để đổi người chịu trách nhiệm.
- Row không đổi giá trị giữ provenance; lịch sử cũ/mới được giữ trong audit theo nghiệp vụ.

### 40. Ràng buộc: Score(schedule_id) → Schedule(schedule_id)

**Mô tả:** Điểm gắn với buổi học hoặc thi cuối khóa phải tham chiếu buổi lịch đang tồn tại.

**Bảng liên quan:** Score (schedule_id, bảng con), Schedule (schedule_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Schedule | − | + (`schedule_id`) | + (`schedule_id`) |
| Score | + (`schedule_id`) | − | + (`schedule_id`) |

**Ghi chú:**

- `schedule_id` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Schedule. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Schedule khi còn Score tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- GENERAL bắt buộc schedule_id NULL; LESSON/FINAL_EXAM bắt buộc liên kết buổi phù hợp. FK nullable không tự kiểm tra điều kiện theo loại thành phần.
- Buổi phải hiện hành, cùng lớp của lượt học, đúng loại và không NOT_HELD; cửa sổ lưu điểm mở từ giờ bắt đầu.
- Kiểm tra chương trình/thành phần, quyền giáo viên và bảng điểm DRAFT/RETURNED; không xóa liên kết hoặc đổi buổi để né điều kiện nghiệp vụ.

### 41. Ràng buộc: GradeReport(class_assignment_id) → StudentClassAssignment(class_assignment_id)

**Mô tả:** Mỗi bảng điểm phải thuộc một lượt học đã được xếp lớp đang tồn tại.

**Bảng liên quan:** GradeReport (class_assignment_id, bảng con), StudentClassAssignment (class_assignment_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| StudentClassAssignment | − | + (`class_assignment_id`) | + (`class_assignment_id`) |
| GradeReport | + (`class_assignment_id`) | − | + (`class_assignment_id`) |

**Ghi chú:**

- `class_assignment_id` không được NULL; giá trị tham chiếu phải tồn tại trong StudentClassAssignment.
- Không xóa StudentClassAssignment khi còn GradeReport tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Một Assignment có tối đa một GradeReport; report DRAFT tạo cùng transaction xếp lớp.
- Không chuyển GradeReport sang lượt học hoặc học viên khác.
- Gửi/gửi lại/trả/chốt toàn lớp; chỉ FINALIZED công bố cho Student và lưu final_score, kết quả đã chốt bất biến.

### 42. Ràng buộc: GradeReport(submitted_by) → Account(account_id)

**Mô tả:** Người gửi hoặc gửi lại bảng điểm phải là tài khoản đang tồn tại.

**Bảng liên quan:** GradeReport (submitted_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| GradeReport | + (`submitted_by`) | − | + (`submitted_by`) |

**Ghi chú:**

- FK cho phép NULL theo điều kiện nêu dưới đây; khi có giá trị tham chiếu, bản ghi tương ứng phải tồn tại.
- Không xóa Account khi còn GradeReport tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Đây là tên cụ thể của trường người gửi trong danh sách ban đầu.
- NULL khi DRAFT chưa từng gửi; SUBMITTED/RETURNED/FINALIZED phải có thông tin lần gửi theo bundle trạng thái.
- Người gửi là Teacher hiện phụ trách, Class COMPLETED và toàn bộ học viên đủ điểm; thao tác toàn lớp all-or-none.
- submitted_by lưu lần gửi gần nhất; từng lần gửi/gửi lại vẫn có audit, không sửa tay provenance.

### 43. Ràng buộc: GradeReport(reviewed_by) → Account(account_id)

**Mô tả:** Người trả lại hoặc chốt bảng điểm phải là tài khoản đang tồn tại.

**Bảng liên quan:** GradeReport (reviewed_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| GradeReport | + (`reviewed_by`) | − | + (`reviewed_by`) |

**Ghi chú:**

- FK cho phép NULL theo điều kiện nêu dưới đây; khi có giá trị tham chiếu, bản ghi tương ứng phải tồn tại.
- Không xóa Account khi còn GradeReport tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Đây là tên cụ thể của trường người xử lý trong danh sách ban đầu.
- Cho phép NULL khi chưa review; RETURNED/FINALIZED bắt buộc bundle review tương ứng. Thông tin review gần nhất có thể được giữ sau gửi lại.
- Actor phải có quyền AcademicAffairs; trả/chốt toàn lớp SUBMITTED của Class COMPLETED, RETURNED cần lý do.
- reviewed_by giữ lần xử lý gần nhất; lịch sử các lần xử lý được lưu trong audit, FINALIZED không chỉnh lại.

### 44. Ràng buộc: Invoice(registration_id) → Registration(registration_id)

**Mô tả:** Mỗi hóa đơn phải thuộc một hồ sơ đăng ký đang tồn tại.

**Bảng liên quan:** Invoice (registration_id, bảng con), Registration (registration_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Registration | − | + (`registration_id`) | + (`registration_id`) |
| Invoice | + (`registration_id`) | − | + (`registration_id`) |

**Ghi chú:**

- `registration_id` không được NULL; giá trị tham chiếu phải tồn tại trong Registration.
- Không xóa Registration khi còn Invoice tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Một Registration có tối đa một Invoice; duyệt ADMITTED và tạo Invoice UNPAID cùng transaction.
- registration_id và số tiền snapshot của Invoice bất biến; không chuyển hóa đơn sang hồ sơ khác.
- Hủy hồ sơ hợp lệ đồng thời hủy Invoice UNPAID, giữ bản ghi; không hủy hồ sơ đã thanh toán hoặc xóa lịch sử.

### 45. Ràng buộc: Invoice(cancelled_by) → Account(account_id)

**Mô tả:** Người hủy hóa đơn theo việc hủy hồ sơ đăng ký phải là tài khoản đang tồn tại.

**Bảng liên quan:** Invoice (cancelled_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Invoice | + (`cancelled_by`) | − | + (`cancelled_by`) |

**Ghi chú:**

- `cancelled_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn Invoice tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Người hủy phản ánh người thực hiện hủy hồ sơ theo quy trình hợp lệ; hóa đơn đã tạo chỉ hủy khi UNPAID.
- Hủy hóa đơn đồng bộ với hủy hồ sơ, ghi người, thời điểm, lý do và nhật ký; không hủy độc lập hóa đơn PAID hoặc hoàn tiền.
- Giữ liên kết hồ sơ, số tiền đã ghi nhận và người xử lý; không xóa hóa đơn để bỏ lịch sử.

### 46. Ràng buộc: Payment(invoice_id) → Invoice(invoice_id)

**Mô tả:** Mỗi thanh toán đã xác nhận phải tham chiếu một hóa đơn đang tồn tại.

**Bảng liên quan:** Payment (invoice_id, bảng con), Invoice (invoice_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Invoice | − | + (`invoice_id`) | + (`invoice_id`) |
| Payment | + (`invoice_id`) | − | + (`invoice_id`) |

**Ghi chú:**

- `invoice_id` không được NULL; giá trị tham chiếu phải tồn tại trong Invoice.
- Không xóa Invoice khi còn Payment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Một Invoice có tối đa một Payment; số tiền phải bằng Invoice.amount và Invoice phải UNPAID trước thu.
- Tạo Payment, đổi Invoice sang PAID và audit cùng transaction.
- invoice_id bất biến; không xóa/hủy/hoàn tiền, chỉ được sửa phương thức/mã giao dịch có lý do/audit.

### 47. Ràng buộc: Payment(confirmed_by) → Account(account_id)

**Mô tả:** Người xác nhận thanh toán phải là tài khoản đang tồn tại.

**Bảng liên quan:** Payment (confirmed_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Payment | + (`confirmed_by`) | − | + (`confirmed_by`) |

**Ghi chú:**

- `confirmed_by` không được NULL; giá trị tham chiếu phải tồn tại trong Account.
- Không xóa Account khi còn Payment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Actor phải có quyền Accountant tại thời điểm xác nhận; FK không tự kiểm tra vai trò.
- confirmed_by và paid_at bất biến sau xác nhận; sửa metadata không đổi người hoặc thời điểm xác nhận ban đầu.
- Không xóa Account để làm mất liên kết người xác nhận; dùng trạng thái khóa/ngừng dùng theo nghiệp vụ.

### 48. Ràng buộc: Payment(last_corrected_by) → Account(account_id)

**Mô tả:** Người sửa thông tin thanh toán gần nhất phải là tài khoản đang tồn tại.

**Bảng liên quan:** Payment (last_corrected_by, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| Payment | + (`last_corrected_by`) | − | + (`last_corrected_by`) |

**Ghi chú:**

- `last_corrected_by` cho phép NULL theo điều kiện nghiệp vụ; khi khác NULL phải tồn tại trong Account. Nhóm trường sự kiện/trạng thái đi kèm vẫn phải được kiểm tra.
- Không xóa Account khi còn Payment tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Chưa sửa thì nhóm thông tin sửa để NULL; khi sửa phải ghi đồng bộ người, thời điểm và lý do.
- Kế toán (Accountant) chỉ sửa phương thức hoặc mã giao dịch; không đổi hóa đơn, số tiền, người hoặc thời điểm xác nhận ban đầu.
- Các trường này ghi lần sửa gần nhất; nhật ký lưu từng lần sửa với giá trị cũ/mới. Không xóa, hủy hay hoàn tiền qua thao tác sửa thông tin.

### 49. Ràng buộc: AuditLog(actor_account_id) → Account(account_id)

**Mô tả:** Người thao tác đã xác thực trong nhật ký phải là tài khoản đang tồn tại.

**Bảng liên quan:** AuditLog (actor_account_id, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| AuditLog | + (`actor_account_id`) | − | + (`actor_account_id`) |

**Ghi chú:**

- FK cho phép NULL theo điều kiện nêu dưới đây; khi có giá trị tham chiếu, bản ghi tương ứng phải tồn tại.
- Không xóa Account khi còn AuditLog tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Cho phép NULL chỉ khi người thao tác chưa được xác thực; không gán email người dùng nhập thành actor đã xác minh.
- Audit append-only; không có nghiệp vụ sửa actor_account_id để chuyển người chịu trách nhiệm, không xóa audit bắt buộc.
- Khóa/ngừng dùng Account không làm mất lịch sử actor. Vận hành dọn log SECURITY/IP hết hạn theo chính sách riêng, không cấp quyền xóa audit trong ứng dụng.

### 50. Ràng buộc: ReplayOperation(actor_account_id) → Account(account_id)

**Mô tả:** Kết quả thao tác để nhận diện yêu cầu lặp phải thuộc một tài khoản đã xác thực đang tồn tại.

**Bảng liên quan:** ReplayOperation (actor_account_id, bảng con), Account (account_id, bảng cha).

**Bảng tầm ảnh hưởng:**

| Bảng | Thêm | Xóa | Sửa |
| --- | --- | --- | --- |
| Account | − | + (`account_id`) | + (`account_id`) |
| ReplayOperation | + (`actor_account_id`) | − | + (`actor_account_id`) |

**Ghi chú:**

- `actor_account_id` không được NULL; giá trị phải tồn tại trong Account.
- Không xóa Account khi còn ReplayOperation tham chiếu theo ràng buộc này; không cập nhật khóa dây chuyền, theo chính sách NO ACTION.
- Người thao tác lấy từ ngữ cảnh xác thực, không dùng chung theo trình duyệt và không chuyển sang tài khoản khác.
- Bản ghi hoàn tất được ghi cùng giao dịch với thay đổi nghiệp vụ và nhật ký bắt buộc; không sửa người thao tác hoặc kết quả đã ghi.
- Nhận diện yêu cầu lặp trong cửa sổ 24 giờ vẫn phải kiểm tra quyền hiện hành trên đối tượng trước khi khớp yêu cầu; bản ghi không tự cấp quyền nghiệp vụ.

## 4. Nguồn và giới hạn

- [DATABASE_SPEC](../design/DATABASE_SPEC.md): T01–T24, nullability, khóa ghép, NO ACTION và các bundle trạng thái.
- [requirements](../requirements.md): vai trò, giữ lịch sử, lifecycle và transaction nghiệp vụ.
- [BandAssignmentConfiguration](../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/BandAssignmentConfiguration.cs), [EntryTestAttemptConfiguration](../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/EntryTestAttemptConfiguration.cs), [EntryTestBookingConfiguration](../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/EntryTestBookingConfiguration.cs): đối chiếu trực tiếp quan hệ ca thi và hai FK ghép.
- Ký hiệu tầm ảnh hưởng không đồng nghĩa hệ thống có API cho phép thao tác đó. Ghi chú mô tả các quy tắc đã chấp nhận; không tuyên bố mọi workflow đã triển khai hoặc kiểm thử.

