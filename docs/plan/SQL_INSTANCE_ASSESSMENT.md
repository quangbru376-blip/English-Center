# SQL hiện có — đánh giá giữ lại và hợp đồng môi trường

Ngày kiểm tra: 04/10/2026. Quyết định: **giữ SQL Server2025 Express đang có**
cho local dev/test/demo (ADR-162 thay riêng SQL choice ADR-119). Đây là đánh giá
capability và inventory, không nghiệm thu ứng dụng, hiệu năng hoặc phục hồi.
Owner cho phép dùng bản hiện có nếu phù hợp; không cần cài Developer2022.

## 1. Bằng chứng trên máy

Repository được kiểm tra trước: main clean, ahead8; chưa source ứng dụng.
Kiểm tra bằng sqlcmd với Windows authentication, metadata và SELECT chỉ đọc;
giao dịch SNAPSHOT chỉ đọc trong master kết thúc bằng ROLLBACK. Không DDL/DML,
không tạo/sửa/xóa DB, không ALTER config hoặc truy cập dữ liệu nghiệp vụ của
17 user databases có sẵn. Không lưu tên DB khác hoặc thông tin đăng nhập.

| Kiểm tra | Kết quả quan sát | Giới hạn kết luận |
| --- | --- | --- |
| Engine/version/edition | ProductVersion17.0.1000.7; ProductLevelRTM; ProductUpdateLevelNULL; Express Edition64-bit; EngineEdition4 | SQL2025 major17.x; chưa cập nhật CU/GDR |
| Instance/service | Registry lists SQLEXPRESS only, instance ID MSSQL17.SQLEXPRESS; service Running/Auto | Chưa cấu hình instance mới hoặc app DB |
| Authentication | Windows-only; Windows login kết nối được; diagnostic login là sysadmin | Không dùng quyền này cho app; ec_app_runtime/grants chưa tạo/kiểm |
| Diagnostic transport | Shared memory, NTLM, encrypt_optionTRUE; sqlcmd dùng -C | Không chứng minh TCP/certificate trust; -C không thành runtime TLS policy |
| Collation | SQL_Latin1_General_CP1_CI_AS | Không tự inherit vào E03; exact column/collation/normalization cần migration checks |
| System DB settings | master/model/tempdb/msdb compatibility170; master/msdb snapshotON, model/tempdbOFF; RCSI0 cả4 | Chỉ system DB, không phải readiness DB dự án |
| Snapshot read | SNAPSHOT transaction SELECT metadata trong master thành công, rollback | Chưa compound application/race tests; app DB phải ALLOW_SNAPSHOT_ISOLATION ON |
| SQL types/catalog | uniqueidentifier16bytes; timestamp/rowversion8bytes; datetime2; decimal precision38; sp_getapplock/sp_releaseapplock hiện diện | Chưa kiểm concurrency, RV updates hoặc quyền caller |
| SELECT probes | decimal38 sum999999999999999999+1 đúng1000000000000000000; datetime2(3)/date/time conversions; OPENJSON([1,2]); SHA2_256 trả32bytes | Không thay codec C1 bằng HASHBYTES; không chứng minh EF materialization/digest corpus |
| Machine resources | 8logical CPUs, physical memory15610MB; C free129.9GB tại lần kiểm | Không phải tài nguyên Express được phép dùng hoặc cam kết performance/storage |
| OS/client | Windows11 Home Single Language64-bit, version10.0.26300/build26300; sqlcmd ProductVersion17.0.1000.7 | Windows11 Home family supported by SQL2025; specific OS build servicing/certification not reviewed |
| External scheduler/tool | Windows Schedule service Running; sqlcmd ODBC180 có sẵn | Chưa backup job, encryption tool, destination hoặc restore test |

## 2. Đối chiếu với E03/E04

| Yêu cầu | Đánh giá chọn edition | Bằng chứng thực thi còn phải có |
| --- | --- | --- |
| 24tables230fields; PK/FK/composite FK/filtered UNIQUE/CHECK/index | Core SQL Server relational features phù hợp Express; không thấy Enterprise-only dependency trong baseline | D01–D07 schema/negative constraints/migration inspection |
| 5triggers6views6UDF7SP/13TX | Programmability phù hợp engine17.x; không cần SQL Agent để gọi object | D08–D11 exact signatures/multirow/rollback; caller evidence theo slice |
| RV8bytes, short EF/SQL transaction, sp_getapplock | Types/system procedures hiện có; edition không cản contract | Hai connection, lock ordering/timeouts, rollback/replay/current rights |
| Compound SNAPSHOT/report consistency | SNAPSHOT đọc thật đã chạy trong master; app DB option là prerequisite riêng | Explicit DB option và compound snapshots/race/CSV tests |
| 4roles/runtime least privilege | Express hỗ trợ user-defined roles; diagnostic sysadmin không là runtime identity | D12 positive/negative grants; operational/runtime separation |
| .NET10/EF10 provider; 5trigger/OUTPUT tables | Official EF10 hỗ trợ SQL2025; engine phù hợp target | P01 package pins; D13 EF writes/OUTPUT strategy/type mappings/readiness |
| exact decimal18/38, dates/time/UTC, Unicode | Read-only type/conversion probes phù hợp; E03 representation giữ nguyên | EF/report exact-money boundaries, collation/normalization và timezone tests |
| 10,000 students/50 concurrent reference scope | Chưa có căn cứ phải thay edition để bắt đầu local; capacity cần đo | Không claim load pass, không thêm business caps; owner D12 vẫn hoãn |
| encrypted daily backup/recovery ADR-118 | Làm được qua external scheduler và encrypted archive; không có native encryption/Agent | Key/tool/media/access/missed runs và decrypt/restore còn DF03 |

Không dùng database snapshot ở bảng edition làm bằng chứng transaction SNAPSHOT;
đây là hai tính năng khác nhau. Catalog presence không thay concurrency tests.

## 3. Target và phần P01 còn thiếu

- Engine: SQL Server2025 Express64-bit trên instance hiện có; observed build
  17.0.1000.7 được ghi làm inventory, không xem là latest/supported patch pin.
- Future local project/test databases và EF provider dùng **compatibility160**
  tường minh: giữ SQL semantics/mapping E03 từ baseline, không tự chuyển các
  text JSON columns sang native JSON khi EF10 được cấu hình170. Engine vẫn là
  SQL2025. Không ALTER system/user DB hiện có; không mặc định app DB đã160.
- Preserve E03 exact types/collations/constraints, ALLOW_SNAPSHOT_ISOLATION ON,
  runtime principal ít quyền và full schema/object signatures. RCSI alone không
  đủ; missing config/objects fail closed, không startup auto-migration.
- Express limits:50GB/database, buffer pool1410MB, lesser1socket/4cores.
  Không suy dm_os_sys_info8CPUs là quyền dùng8cores;17 DB khác có thể chia tài
  nguyên instance. Data/index/version-store/log/backup footprint cần đo riêng.
- Microsoft build list tại04/10/2026 có CU9 17.0.5005.3 (15/09/2026) và RTM-
  branch GDR17.0.1135.8 (08/09/2026). Build hiện có chưa gồm những updates này.
  Giữ engine/edition; review compatible17.x patch trước dữ liệu thật/public
  exposure theo ADR-119. Không auto pin/install latest hoặc chọn servicing
  branch thay owner; chưa cập nhật máy. Local skeleton planning không đòi
  reinstall major/edition; ứng dụng chưa có nên chưa có EF runtime evidence.

## 4. Backup: giữ yêu cầu, đổi cơ chế theo Express

ADR-118 yêu cầu backup mã hóa, không bắt buộc SQL Agent hoặc native BACKUP WITH
ENCRYPTION. Express không có cả hai. Hướng tương thích: Windows Task Scheduler
điều phối SQL backup, sau đó mã hóa archive bằng công cụ được rà và đưa sang
media riêng khỏi ổ primary; key tách khỏi backup/repo. Chưa chọn tool/package,
key, drive, identity hoặc lịch cụ thể; không suy disk C trống là backup media.

Phải bảo vệ temporary plaintext bằng quyền truy cập, có cleanup và xử lý crash;
không giữ/copy .bak plaintext như một bản đã đạt policy. Chỉ đánh dấu backup
thành công sau khi SQL backup, encryption và bản copy kiểm được. Lỗi không được
bị scheduler che khuất. Monthly restore cần thử archive decrypt và restore trên
DB test độc lập; giữ7daily/4weekly, missed-run handling và RPO24h/RTO2h targets.
Chưa chạy backup/restore hoặc chứng minh mục tiêu. DF03 còn nguyên; nếu không
hoàn thiện được cơ chế này thì recovery readiness chưa đạt, không hạ encryption.

## 5. Handoff và nguồn

Chat code tiếp nhận ADR-162 và assessment này sau memory theo AGENTS/HANDOFF.
Bắt đầu **P01**: hoàn thiện package provenance/pins, concrete LOCAL_DEV profile
và test isolation; SQL edition mismatch đã giải quyết. Sau P01 đạt các gate
liên quan mới **P02 backend skeleton**, rồi P03 và thứ tự tại M0 section11.
Không bắt đầu D01/migration như việc đầu tiên. Không hỏi lại SQL2022 Developer.
Các approval trước apply worker changes, skill gates và owner D12/D13 không đổi.

Phạm vi lượt này chỉ kiểm tra và tài liệu; không code/SQL application/scaffold/
install/config/migration/deploy/AGY dispatch, không chạm dữ liệu DB có sẵn.

Nguồn Microsoft đọc04/10/2026:

- [SQL2025 editions/limits](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-2025?view=sql-server-ver17).
- [EF SQL provider/compatibility configuration](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/),
  [EF10 SQL2025 support và JSON defaults](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew).
- [SQL2025 build list](https://support.microsoft.com/en-us/servicing/sql/sql-server-2025/kb5005684-build-versions).
- [Express backup scheduling](https://learn.microsoft.com/en-us/troubleshoot/sql/database-engine/backup-restore/schedule-automate-backup-database).
- [ALTER DATABASE snapshot/compatibility options](https://learn.microsoft.com/en-us/sql/t-sql/statements/alter-database-transact-sql-set-options?view=sql-server-ver17),
  [sp_getapplock](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver17).

Verification tài liệu sau cập nhật: git diff --check đạt;19file thay đổi đều
Markdown trong docs. Giữ nguyên75approval/note cells (73accepted/2deferred),
ADR001–161 historical bodies ngoài status SQL supersession của119, và toàn bộ
M0 section11/thứ tự39families/diagram. New/changed local links resolve; remaining
SQL2022 references chỉ là lịch sử có supersession hoặc explicit removal notes.
Không chạy app build/test/lint vì chưa có source ứng dụng.

Nguồn OS family: [SQL2025 hardware/software requirements](https://learn.microsoft.com/en-us/sql/sql-server/install/hardware-and-software-requirements-for-installing-sql-server-2025?view=sql-server-ver17).
