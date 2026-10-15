# Đối chiếu database với báo cáo — 14/10/2026

## Kết luận chính

- Hai hình 6.1/6.4 là mô hình cũ, còn học bổng và các field/quan hệ không tồn tại trong dự án; cần vẽ lại.
- 22 bảng và 217 cột trong từ điển 6.2 **khớp tên, kiểu SQL và NULL**. Thiếu ReplayOperation10cột + InvoiceNumberCounter3cột để đủ 24 bảng/230 cột.
- 49 FK viết bằng chữ trong 6.3.4 **khớp endpoints**. Bổ sung một FK ReplayOperation→Account; schema có50FK.
- Ràng buộc còn sai literal vai trò, locked_until, status trên Attempt, tiền≥0, BANK_TRANSFER, placement ONGOING, đặc quyền override điểm và reason sau finalize; từng lỗi có nguồn/cách sửa trong file tương ứng.

Không vì hình cũ sai mà thay code hoặc xóa phần từ điển đang đúng. Bộ này chỉ tạo tài liệu riêng, không sửa Word nguồn/migration/schema.

## Mỗi phần báo cáo một file

| Phần báo cáo | File đối chiếu |
| --- | --- |
| 6.1 — Sơ đồ logic | [Đối chiếu sơ đồ logic](6_1_SO_DO_LOGIC.md) |
| 6.2 — Chi tiết các bảng | [Tổng hợp và liên kết từng bảng](6_2_CHI_TIET_BANG.md) |
| 6.3.1 — Miền giá trị | [Đối chiếu miền](6_3_1_RANG_BUOC_MIEN_GIA_TRI.md) |
| 6.3.2 — Liên thuộc tính | [Đối chiếu các bundle/cặp thuộc tính](6_3_2_RANG_BUOC_LIEN_THUOC_TINH.md) |
| 6.3.3 — Liên bộ cùng quan hệ | [Đối chiếu uniqueness/indexes](6_3_3_RANG_BUOC_LIEN_BO.md) |
| 6.3.4 — Tham chiếu | [Đối chiếu từng FK/bảng tầm ảnh hưởng](6_3_4_RANG_BUOC_THAM_CHIEU.md) |
| 6.3.5 — Liên quan hệ | [Đối chiếu workflow và transaction](6_3_5_RANG_BUOC_LIEN_QUAN_HE.md) |
| 6.4 — Sơ đồ quan hệ | [Đối chiếu hình schema cũ](6_4_SO_DO_QUAN_HE.md) |
| Phụ lục bổ sung | [SQL objects/database roles](PHU_LUC_SQL_OBJECTS_VA_PHAN_QUYEN.md) |

Mục6.2 có **24 file riêng theo bảng** trong `tables/`; mỗi file ghi từng cột, khóa/default/index/filter/CHECK và ý nghĩa cần cập nhật. Hai bảng chưa có trong báo cáo được đánh dấu bổ sung, không giả ghi “khớp”.

## Cách dùng

1. Sửa hình theo6.1/6.4 và quan hệ50FK.
2. Giữ cấu trúc217cột đúng của6.2, thêm hai bảng kỹ thuật và cập nhật metadata được chỉ rõ.
3. Sửa các đoạn ràng buộc theo6.3.1–6.3.5. Phân biệt SQL constraint với validation và transactional service/trigger.
4. Nếu cần chứng minh các cơ chế bảo vệ, dẫn phụ lục SQL objects/source; không nói CHECK/FK tự chứng minh current role, trạng thái liên bảng, capacity hoặc clock.

## Phương pháp và nguồn

- Báo cáo Word được đọc qua OOXML, lấy đủ22 bảng/cột,49 dòng FK và các đoạn ràng buộc. Hai ảnh được trích và xem trực tiếp. Vị trí chỉ số XML trong bằng chứng là block index, **không phải số trang Word**.
- [InitialSchema](../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs): parse 24 CreateTable/230 cột để đối chiếu tên/kiểu/NULL/IDENTITY.
- [ExpectedCatalog](../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json): structural baseline FK/keys/indexes/CHECK/defaults/module/permissions; không dùng bản trong bin như source.
- [EF snapshot](../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/EnglishCenterDbContextModelSnapshot.cs) và [configurations](../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/AccountConfiguration.cs): đối chiếu model/source; không suy bằng source sample fixtures.
- [DATABASE_SPEC](../../design/DATABASE_SPEC.md), [requirements](../../requirements.md), [WORKFLOW_SPEC](../../design/WORKFLOW_SPEC.md), [CURRENT_STATE](../../CURRENT_STATE.md) và code service/SQL modules hiện hành: đối chiếu ý nghĩa và quyền.
- [Requirements](../../requirements.md) xác định học bổng ngoài phạm vi cả chức năng lẫn tables; không hồi sinh ba bảng học bổng từ hình cũ.

Số đếm nghiệp vụ/source:24 bảng/230 cột/24 PK/2 AK/50 FK/70 CHECK/20 RV/78 indexes ngoài PK/AK (tổng104 indexes vớiPK/AK),6 migrations. Bảng framework __EFMigrationsHistory không tính trong24bảng.

## Giới hạn kiểm chứng

Lượt này không kết nối SQL live, không chạy migration hoặc bộ test runtime mới; không thay grants/login/key/custody/data. “Hiện tại” là source/model/catalog chuẩn đang trong dự án; không tuyên bố kiểm tra live DB mới. Các linked sources ngoài thư mục vẫn nằm trong workspace; ZIP chỉ đóng gói các file đối chiếu, ảnh nguồn và bằng chứng trích/phân tích.

Word nguồn giữ SHA256: `40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B`.
