# 6.2 — Đối chiếu chi tiết các bảng dữ liệu

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

## 1. Kết luận

**Không cần thay toàn bộ từ điển 6.2.** Đối chiếu từng cột cho thấy 22 bảng/217 cột đã có đều khớp **tên cột, kiểu SQL và NULL** với migration/catalog hiện tại. Vấn đề chính là thiếu hai bảng kỹ thuật và mô tả chưa đầy đủ PK/AK/index/CHECK/default hoặc còn chữ “đề xuất”. Điều này khác hai hình 6.1/6.4 và các đoạn ràng buộc cũ.

Cần bổ sung ReplayOperation (10 cột), InvoiceNumberCounter (3 cột), đưa tổng thành **24 bảng/230 cột**. Không sửa dữ liệu vì thấy từ điển thiếu bảng.

## 2. Đối chiếu từng bảng

| Mã | Bảng | Cột báo cáo | Cột hiện tại | Đánh giá cấu trúc | File chi tiết |
| --- | --- | --- | --- | --- | --- |
| T01 | Role | 3 | 3 | Khớp tên/kiểu/NULL | [Role](tables/T01_Role.md) |
| T02 | Account | 16 | 16 | Khớp tên/kiểu/NULL | [Account](tables/T02_Account.md) |
| T03 | StudentProfile | 5 | 5 | Khớp tên/kiểu/NULL | [StudentProfile](tables/T03_StudentProfile.md) |
| T04 | AuthSession | 9 | 9 | Khớp tên/kiểu/NULL | [AuthSession](tables/T04_AuthSession.md) |
| T05 | Course | 6 | 6 | Khớp tên/kiểu/NULL | [Course](tables/T05_Course.md) |
| T06 | Band | 4 | 4 | Khớp tên/kiểu/NULL | [Band](tables/T06_Band.md) |
| T07 | TrainingProgram | 7 | 7 | Khớp tên/kiểu/NULL | [TrainingProgram](tables/T07_TrainingProgram.md) |
| T08 | ScoreComponentConfig | 6 | 6 | Khớp tên/kiểu/NULL | [ScoreComponentConfig](tables/T08_ScoreComponentConfig.md) |
| T09 | Registration | 20 | 20 | Khớp tên/kiểu/NULL | [Registration](tables/T09_Registration.md) |
| T10 | EntryTestSession | 12 | 12 | Khớp tên/kiểu/NULL | [EntryTestSession](tables/T10_EntryTestSession.md) |
| T11 | EntryTestAttempt | 8 | 8 | Khớp tên/kiểu/NULL | [EntryTestAttempt](tables/T11_EntryTestAttempt.md) |
| T12 | EntryTestBooking | 14 | 14 | Khớp tên/kiểu/NULL | [EntryTestBooking](tables/T12_EntryTestBooking.md) |
| T13 | BandAssignment | 6 | 6 | Khớp tên/kiểu/NULL | [BandAssignment](tables/T13_BandAssignment.md) |
| T14 | Class | 12 | 12 | Khớp tên/kiểu/NULL | [Class](tables/T14_Class.md) |
| T15 | StudentClassAssignment | 5 | 5 | Khớp tên/kiểu/NULL | [StudentClassAssignment](tables/T15_StudentClassAssignment.md) |
| T16 | Schedule | 18 | 18 | Khớp tên/kiểu/NULL | [Schedule](tables/T16_Schedule.md) |
| T17 | Attendance | 7 | 7 | Khớp tên/kiểu/NULL | [Attendance](tables/T17_Attendance.md) |
| T18 | Score | 8 | 8 | Khớp tên/kiểu/NULL | [Score](tables/T18_Score.md) |
| T19 | GradeReport | 14 | 14 | Khớp tên/kiểu/NULL | [GradeReport](tables/T19_GradeReport.md) |
| T20 | Invoice | 13 | 13 | Khớp tên/kiểu/NULL | [Invoice](tables/T20_Invoice.md) |
| T21 | Payment | 11 | 11 | Khớp tên/kiểu/NULL | [Payment](tables/T21_Payment.md) |
| T22 | AuditLog | 13 | 13 | Khớp tên/kiểu/NULL | [AuditLog](tables/T22_AuditLog.md) |
| T23 | ReplayOperation | 0 | 10 | Thiếu bảng | [ReplayOperation](tables/T23_ReplayOperation.md) |
| T24 | InvoiceNumberCounter | 0 | 3 | Thiếu bảng | [InvoiceNumberCounter](tables/T24_InvoiceNumberCounter.md) |

Mỗi file chi tiết ghi đủ cột, PK/AK, identity, default, indexes/filter, CHECK và các mô tả cần sửa/giữ. Không nhầm timestamp trong SQL catalog với cột thời gian: timestamp ở đây là rowversion.

## 3. Những bổ sung chung

| Nội dung | Cách cập nhật |
| --- | --- |
| Trạng thái “đề xuất” | Các kiểu/length/default dưới đây đã có trong source migration/configuration; gọi là thiết kế triển khai hiện hành, không các đề xuất chưa lựa chọn. |
| UNIQUE/index | Ghi tên/cột/filter thật, nhất là unfinished Registration, Booking current/completed, Schedule current obligation/exam và Program FINAL_EXAM. |
| Collation | Model dùng Latin1_General_100_CI_AS_SC; các CHECK enum/mã có BIN2 riêng. Đừng suy enum được nhập mọi kiểu hoa-thường vì collation tên mặc định CI. |
| Chuỗi Unicode | SQL nvarchar capacity và giới hạn code point của validation là hai tầng; không đổi nvarchar(200) thành 100 chỉ vì họ tên hạn100 code point. |
| Thời gian | Sự kiện UTC datetime2(3); ngày/giờ lịch VN date/time(0) theo phút. Hạn nhập ca/root schedule future, HELD từ end là service guard, không cứ đọc là ghi. |
| Dữ liệu thiếu | Score/Attendance chưa nhập là thiếu row hoặc NULL result; không default0/Vắng. |
| FK actor | Chỉ chứng minh tài khoản tồn tại; quyền/role/ownership/current task do transaction/service kiểm. |
| Bảng kỹ thuật | ReplayOperation không cache credential/PII; InvoiceNumberCounter không endpoint reset, không FK sang Invoice. |

## 4. Bộ số đếm dùng trong báo cáo

- 24 bảng/230 cột do ứng dụng quản lý; 24 PK, 2 AK, 50 FK, 70 CHECK, 20 rowversion.
- 78 indexes ngoài indexes của PK/AK; trong 78 có 28 unique indexes và 50 non-unique indexes. Tổng indexes vật lý của 24 bảng là 104 = 78 + 24 PK + 2 AK.
- 21 default constraints được ghi trong catalog nguồn.
- Nếu tính cả bảng framework __EFMigrationsHistory: thêm một bảng, hai cột và một PK/index. Không ghi 25/232/105 như số bảng/cột/index nghiệp vụ.

Số đếm này kiểm từ source/canonical catalog và đối chiếu migration; không là kết quả đo live SQL mới.


Nguồn dùng chung và phương pháp: [README](README.md).
