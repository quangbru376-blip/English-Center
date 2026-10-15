# SQ24 — Hướng dẫn vẽ sequence Giám đốc xem ba báo cáo tổng hợp

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV30–32; W20; F31; UC08.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: ReportEndpoints.cs](../../../backend/src/EnglishCenter.Api/Api/Reports/ReportEndpoints.cs).
- [Code: SqlReportsExecutor.cs](../../../backend/src/EnglishCenter.Api/Infrastructure/Reports/SqlReportsExecutor.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Director áp dụng bộ lọc để đọc tổng hợp tuyển sinh, đào tạo hoặc tài chính. Ba hình A/B/C dùng cùng kiểu query nhưng cohort và định nghĩa chỉ số khác nhau; không gộp quyền Accountant vào Director.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ24-A — Báo cáo tuyển sinh**; actor Giám đốc.
- **SQ24-B — Báo cáo đào tạo và kết quả**; actor Giám đốc.
- **SQ24-C — Báo cáo tài chính quản trị**; actor Giám đốc.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không danh sách Student/Invoice, phê duyệt, báo cáo lợi nhuận/chi phí hoặc tự thêm biểu đồ.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giám đốc | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện báo cáo tuyển sinh tổng hợp; Giao diện báo cáo đào tạo tổng hợp; Giao diện báo cáo tài chính tổng hợp | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Scope | Director NORMAL; cards/group tables không định danh Student, không hidden individual payload hoặc link qua Invoice/Payment/Profile. |
| Tuyển sinh | Cohort theo ngày tạo Registration, current state ở queried_at; people distinct Student trên toàn tập. Một Student nhiều Course: nhóm mỗi nơi 1, overall people vẫn 1. |
| Đào tạo | Current Class inventory không lọc kỳ ngày tạo lớp; tiến độ HELD obligations/expected count, FINAL_EXAM riêng. NOT_HELD + replacement chỉ một nghĩa vụ. |
| Kết quả | FINALIZED theo ngày finalize, kỳ mặc định tháng VN; grain enrollment-result. Không chốt chưa phải không đạt. Denominator rỗng: Chưa có dữ liệu, nonempty không ai đạt mới 0%. |
| Tài chính | Revenue theo Payment confirmation date; billed theo Invoice issue date loại current CANCELLED; debt/overdue UNPAID hiện tại không áp kỳ. Không billed−period revenue để tính nợ. |
| Query | Giữ bộ lọc/result/queried_at một snapshot nhất quán, totals toàn filtered set. Course lịch sử INACTIVE vẫn chọn theo phạm vi; edit filter chưa thay result trước Áp dụng. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Báo cáo tuyển sinh

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giám đốc → Giao diện báo cáo tuyển sinh tổng hợp | Áp dụng bộ lọc tuyển sinh | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| A02 | Giao diện báo cáo tuyển sinh tổng hợp → Hệ thống | Yêu cầu báo cáo tuyển sinh theo kỳ tạo Registration/Course/state | Request đọc; bắt đầu activation. |
| A03 | Hệ thống → Hệ thống | Kiểm Director, bộ lọc/cohort và projection chỉ tổng hợp | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| A04 | Hệ thống → Giao diện báo cáo tuyển sinh tổng hợp | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Sai quyền Director hoặc bộ lọc không hợp lệ]`; không đi nhánh query thành công. |
| A05 | Giao diện báo cáo tuyển sinh tổng hợp → Giám đốc | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| A06 | Hệ thống → Hệ thống | Query snapshot toàn tập; đếm hồ sơ và distinct Student đúng cohort, nhóm/current states tại queried_at | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| A07 | Hệ thống → Giao diện báo cáo tuyển sinh tổng hợp | Cards/bảng nhóm/tổng/queried_at không định danh | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| A08 | Giao diện báo cáo tuyển sinh tổng hợp → Giám đốc | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| A09 | Hệ thống → Giao diện báo cáo tuyển sinh tổng hợp | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| A10 | Giao diện báo cáo tuyển sinh tổng hợp → Giám đốc | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Giám đốc; G = Giao diện báo cáo tuyển sinh tổng hợp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Áp dụng bộ lọc tuyển sinh
                            |-- A02 --> H : Yêu cầu báo cáo tuyển sinh theo kỳ tạo Registration/Course/state
                                                       H --+ A03: Kiểm Director, bộ lọc/cohort và projection chỉ tổng hợp
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. A05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A06: Query snapshot toàn tập; đếm hồ sơ và distinct Student đúng cohort,…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A07 ..| : Cards/bảng nhóm/tổng/queried_at không định danh
    N <.. A08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. A09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. A10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### B. Báo cáo đào tạo và kết quả

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Giám đốc → Giao diện báo cáo đào tạo tổng hợp | Áp dụng bộ lọc đào tạo/kết quả | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| B02 | Giao diện báo cáo đào tạo tổng hợp → Hệ thống | Yêu cầu báo cáo inventory hiện tại và kết quả theo kỳ FINALIZED riêng | Request đọc; bắt đầu activation. |
| B03 | Hệ thống → Hệ thống | Kiểm Director và định nghĩa hai tập current inventory/finalized cohort | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| B04 | Hệ thống → Giao diện báo cáo đào tạo tổng hợp | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Sai quyền Director hoặc bộ lọc không hợp lệ]`; không đi nhánh query thành công. |
| B05 | Giao diện báo cáo đào tạo tổng hợp → Giám đốc | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| B06 | Hệ thống → Hệ thống | Query inventory/lớp/HELD obligations và cohort FINALIZED; tính tỷ lệ đạt với mẫu số đúng | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| B07 | Hệ thống → Giao diện báo cáo đào tạo tổng hợp | Tổng hợp lớp/progress/kết quả và Chưa có dữ liệu khi denominator rỗng | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| B08 | Giao diện báo cáo đào tạo tổng hợp → Giám đốc | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| B09 | Hệ thống → Giao diện báo cáo đào tạo tổng hợp | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| B10 | Giao diện báo cáo đào tạo tổng hợp → Giám đốc | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Giám đốc; G = Giao diện báo cáo đào tạo tổng hợp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Áp dụng bộ lọc đào tạo/kết quả
                            |-- B02 --> H : Yêu cầu báo cáo inventory hiện tại và kết quả theo kỳ FINALIZED riêng
                                                       H --+ B03: Kiểm Director và định nghĩa hai tập current inventory/finalized cohort
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. B05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B06: Query inventory/lớp/HELD obligations và cohort FINALIZED; tính tỷ lệ…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B07 ..| : Tổng hợp lớp/progress/kết quả và Chưa có dữ liệu khi denominator rỗng
    N <.. B08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. B09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. B10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### C. Báo cáo tài chính quản trị

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Giám đốc → Giao diện báo cáo tài chính tổng hợp | Áp dụng bộ lọc tài chính | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| C02 | Giao diện báo cáo tài chính tổng hợp → Hệ thống | Yêu cầu báo cáo tài chính tổng hợp theo kỳ revenue/billed và lọc current debt | Request đọc; bắt đầu activation. |
| C03 | Hệ thống → Hệ thống | Kiểm Director, bộ lọc và projection aggregate-only | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| C04 | Hệ thống → Giao diện báo cáo tài chính tổng hợp | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Sai quyền Director hoặc bộ lọc không hợp lệ]`; không đi nhánh query thành công. |
| C05 | Giao diện báo cáo tài chính tổng hợp → Giám đốc | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| C06 | Hệ thống → Hệ thống | Query snapshot; tính revenue/billed/current debt/overdue theo đúng ngày và tập dữ liệu | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| C07 | Hệ thống → Giao diện báo cáo tài chính tổng hợp | Tổng hợp tài chính và queried_at, không danh sách Invoice/Payment cá nhân | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| C08 | Giao diện báo cáo tài chính tổng hợp → Giám đốc | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| C09 | Hệ thống → Giao diện báo cáo tài chính tổng hợp | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| C10 | Giao diện báo cáo tài chính tổng hợp → Giám đốc | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Giám đốc; G = Giao diện báo cáo tài chính tổng hợp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Áp dụng bộ lọc tài chính
                            |-- C02 --> H : Yêu cầu báo cáo tài chính tổng hợp theo kỳ revenue/billed và lọc…
                                                       H --+ C03: Kiểm Director, bộ lọc và projection aggregate-only
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. C05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C06: Query snapshot; tính revenue/billed/current debt/overdue theo đúng ngày…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C07 ..| : Tổng hợp tài chính và queried_at, không danh sách Invoice/Payment cá…
    N <.. C08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. C09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. C10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Ba query không sửa dữ liệu, không export. Director không tham gia approval Course/Class/Invoice/grade.”**
2. **“Overall distinct Student phải tính trực tiếp toàn tập; không cộng distinct của từng nhóm.”**
3. **“UI hiện tại có thể tải bộ lọc mặc định khi mở. Không vẽ actor bấm Áp dụng giả cho request khởi tạo; query chủ động sau đó là lần riêng.”**

Đọc lỗi không làm dữ liệu nghiệp vụ bị hoàn tác vì đây không phải mutation. Không tự audit mọi lần đọc; khi không còn quyền, không trình bày dữ liệu cũ như kết quả mới đã xác minh.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Ba loại báo cáo giữ cohort khác nhau.
- Không dùng ngày tạo Class làm inventory kỳ.
- Không coi unpublished grade là fail.
- Không CSV/PDF/XLSX hoặc individual drill-down.
- Totals từ server, không cộng current page.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Một Student đăng ký 2 Course trong cohort | Distinct mỗi nhóm Course là 1, overall people là 1; không cộng thành 2. |
| Không có kết quả FINALIZED trong kỳ | Pass rate Chưa có dữ liệu, không giả 0%. |
| Director yêu cầu CSV hoặc Invoice detail | Ngoài scope; không cấp quyền vì dùng cùng metric tài chính. |
