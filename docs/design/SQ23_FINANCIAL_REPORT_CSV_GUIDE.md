# SQ23 — Hướng dẫn vẽ sequence báo cáo tài chính và xuất CSV

Ngày: 02/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA),
mức nghiệp vụ Actor → Boundary → Control. Bố cục/mã thông điệp là gợi ý,
không thêm metric hoặc quyền sản phẩm mới.

Nguồn: [SEQUENCE_PLAN](SEQUENCE_PLAN.md) SQ23;
[FUNCTION_SPEC](FUNCTION_SPEC.md) NV29;
[WORKFLOW_SPEC](WORKFLOW_SPEC.md) W20;
[requirements](../requirements.md) ADR-013/018/063–064/070/072/091/094/101/126.

## 1. Mục tiêu và cách chia hình

Kế toán có phiên thường hợp lệ, dùng F30 để xem báo cáo học phí và xuất
CSV. Chỉ báo cáo tài chính; Giám đốc xem tổng hợp qua SQ24 và không có CSV.

Nên vẽ hai hình:

- **SQ23-A — Xem báo cáo:** chọn/áp dụng bộ lọc, truy vấn dữ liệu nhất quán,
  trả thẻ/bảng nhóm/tổng và thời điểm truy vấn.
- **SQ23-B — Xuất CSV:** dùng bộ lọc đã áp dụng, query mới toàn bộ kết quả,
  kiểm tra file, trả file hoặc từ chối toàn bộ.

Xuất CSV là thao tác tùy chọn sau khi có bộ lọc đã áp dụng, không là bước
luôn chạy sau xem. Không vẽ query xuất dùng lại snapshot của trang đang mở.
Tra cứu hóa đơn cá nhân thuộc SQ13; xác nhận thu tiền thuộc SQ14.

## 2. Thành phần cần đặt trong EA

| Trái → phải | Tên lifeline | Loại/vai trò |
| --- | --- | --- |
| 1 | Kế toán | Actor Accountant, đọc báo cáo và chủ động yêu cầu CSV. |
| 2 | Giao diện báo cáo học phí | Boundary F30, bộ lọc/thẻ/bảng/nút xuất. |
| 3 | Hệ thống quản lý trung tâm | Control, kiểm quyền, truy vấn chỉ số, tổng hợp và tạo CSV. |

Dùng message, phản hồi nét đứt, activation, self-message, `alt` và note.
Không cần lifeline Payment/Invoice/Database hoặc phần mềm Excel. Không thêm
Kế toán thứ hai, Giám đốc duyệt file hoặc dịch vụ gửi email.

## 3. Bộ lọc và định nghĩa chỉ số

| Nội dung | Định nghĩa bắt buộc |
| --- | --- |
| Kỳ mặc định | Tháng Việt Nam hiện tại, dùng cho doanh thu và tổng phải thu; ngày biên theo Asia/Ho_Chi_Minh. |
| Doanh thu | Tổng Payment theo ngày xác nhận trong kỳ; không lấy ngày phát hành Invoice. |
| Tổng phải thu trong kỳ | Tổng amount Invoice phát hành trong kỳ, UNPAID + PAID theo trạng thái hiện tại; loại CANCELLED. |
| Công nợ hiện tại | Tổng Invoice UNPAID hiện tại, không áp kỳ ngày xác nhận/phát hành của hai chỉ số trên. |
| Quá hạn | Phần công nợ UNPAID có due_date trước ngày Việt Nam hiện tại. “Chỉ quá hạn” áp phần công nợ, không làm lọc doanh thu/tổng phải thu thành tập khác. |
| Lọc khóa | Áp nhất quán các chỉ số; giữ khóa lịch sử phù hợp, không chỉ khóa ACTIVE. |
| Tổng | Tổng chính xác trên toàn tập lọc, không chỉ trang hiện tại; không lấy tổng phải thu trừ doanh thu trong kỳ để tính nợ. |
| Thời điểm truy vấn | `queried_at` của kết quả mới; không là báo cáo công nợ tại ngày quá khứ/as-of. |
| Bộ lọc đang sửa | Chưa đổi kết quả/nhãn đang áp dụng; chỉ khi bấm Áp dụng mới chạy query mới. |
| Kết quả rỗng | Hiển thị đúng không có dữ liệu phù hợp; không tự tạo dữ liệu. Tổng tiền 0 hợp lệ nếu tập tương ứng rỗng; không thêm tỷ lệ mới hoặc suy 0% khi không có mẫu số. |

Kết quả là thẻ chỉ số/bảng nhóm theo phạm vi báo cáo, không danh mục hồ sơ/
Invoice/PII dump hoặc dữ liệu điểm/điểm danh. Không chi phí/lợi nhuận,
PDF/XLSX/chart mới hoặc quyền in báo cáo nhân sự.

## 4. SQ23-A — Thông điệp xem báo cáo

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| A01 | Kế toán → Giao diện | Mở báo cáo học phí. |
| A02 | Giao diện → Kế toán | Hiển thị bộ lọc mặc định. |
| A03 | Kế toán → Giao diện | Chọn/sửa bộ lọc và bấm Áp dụng; có thể áp luôn mặc định. |
| A04 | Giao diện → Hệ thống | Yêu cầu báo cáo theo bộ lọc đã áp dụng. |
| A05 | Hệ thống → Hệ thống | Kiểm quyền và bộ lọc theo định nghĩa từng chỉ số. |
| A06 | Hệ thống → Giao diện | Không thể truy vấn; `alt [Không đủ quyền hoặc bộ lọc sai]`. |
| A07 | Giao diện → Kế toán | Hiển thị lỗi, không gắn bộ lọc mới lên kết quả cũ như đã áp dụng thành công. |
| A08 | Hệ thống → Hệ thống | Query snapshot nhất quán và tổng hợp toàn tập lọc; `else [Đủ quyền và bộ lọc hợp lệ]`. |
| A09 | Hệ thống → Giao diện | Trả kết quả/bộ lọc áp dụng/tổng/queried_at sau query thành công. |
| A10 | Giao diện → Kế toán | Hiển thị báo cáo và đúng bộ lọc/thời điểm của kết quả. |

Hình chính mô tả lần Áp dụng chủ động. Nếu UI tải báo cáo mặc định khi mở,
có thể nối A01/A02 tới A04 bằng bộ lọc mặc định; không thêm thao tác người
dùng bấm Áp dụng giả. Khi query lỗi xử lý, không đi A09/A10 thành công:
hiển thị lỗi/giữ nhãn kết quả cũ nếu có, không trộn dữ liệu hai lần query.
Đây có thể là note hoặc nhánh lỗi phụ.

## 5. SQ23-B — Thông điệp xuất CSV

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| B01 | Kế toán → Giao diện | Chọn Xuất CSV. |
| B02 | Giao diện → Hệ thống | Yêu cầu xuất theo bộ lọc đã áp dụng, không lấy ô lọc đang sửa. |
| B03 | Hệ thống → Hệ thống | Kiểm lại quyền Accountant và bộ lọc. |
| B04 | Hệ thống → Giao diện | Không thể xuất; `alt [Không đủ quyền hoặc bộ lọc sai]`. |
| B05 | Giao diện → Kế toán | Thông báo phù hợp, không trả file. |
| B06 | Hệ thống → Hệ thống | Query snapshot mới toàn bộ kết quả theo định nghĩa/cột báo cáo; `else [Đủ quyền và bộ lọc hợp lệ]`. |
| B07 | Hệ thống → Hệ thống | Chuẩn bị CSV an toàn và kiểm giới hạn dòng/kích thước. |
| B08 | Hệ thống → Giao diện | Từ chối toàn file; `[Vượt 10.000 dòng dữ liệu hoặc 5 MB]`. |
| B09 | Giao diện → Kế toán | Thông báo vượt giới hạn, đề nghị thu hẹp lọc; không tự cắt/lọc rồi xuất lại. |
| B10 | Hệ thống → Giao diện | Trả CSV hoàn chỉnh/thời điểm query riêng; `[Trong giới hạn và tạo file hoàn tất]`. |
| B11 | Giao diện → Kế toán | Cho tải file CSV. |
| B12 | Hệ thống → Giao diện | Không thể hoàn tất; `[Lỗi truy vấn hoặc tạo file]`. |
| B13 | Giao diện → Kế toán | Báo lỗi, không trả file một phần. |

B06 lỗi thì đi B12/B13, không bắt buộc chạy B07 sau query thất bại. Hình
chữ đặt `alt` kết quả sau nhóm xử lý để gọn; không hiểu mọi bước đều chạy
trong mọi nhánh. Trong EA có thể tách `alt` query thành công trước B07.

**Quy tắc file:**

- CSV toàn kết quả lọc, cùng định nghĩa/cột báo cáo, không chỉ trang hiện tại
  hoặc cột ẩn/định danh mới.
- UTF-8 BOM, dấu phẩy, CRLF, quote/escape đúng; xử lý ô văn bản có nguy cơ
  công thức kể cả full-width theo quy tắc đã duyệt, không sửa dữ liệu DB.
- Không vượt 10.000 dòng dữ liệu hoặc 5 MB; vượt bất kỳ giới hạn nào từ chối
  toàn file, không tải trước một phần rồi coi đó là kết quả hoàn chỉnh.
- Snapshot xuất mới có thể khác trang đang mở do dữ liệu thay đổi; giữ
  thời điểm riêng, không báo sai là file cố định từ query trước.
- Cảnh báo giới hạn khi mở/chỉnh/lưu/mở lại bằng phần mềm bảng tính;
  không khẳng định mọi phần mềm đều an toàn sau thao tác bên ngoài.
- Không tự gửi CSV qua email, lưu báo cáo lịch sử/as-of hoặc cấp CSV cho
  Director/Admin/Giáo vụ/Tuyển sinh.

## 6. Phác thảo bố cục bằng đường gạch

```text
SQ23-A — Kế toán áp dụng bộ lọc và xem báo cáo

        Kế toán                             Giao diện báo cáo học phí               Hệ thống
        |                                   |                                       |
        |- A01: Mở báo cáo ----------------->                                       |
        <. A02: Bộ lọc mặc định ............|                                       |
        |- A03: Sửa lọc/Áp dụng ------------>                                       |
        |                                   |- A04: Yêu cầu báo cáo ---------------->
        |                                   |                                       |----+  A05: Kiểm tra quyền
        |                                   |                                       |<---+  và bộ lọc theo chỉ số
+- alt [Không đủ quyền hoặc bộ lọc sai] -------------------------------------------------------------------------------------+
|       |                                   <. A06: Không thể truy vấn .............|                                        |
|       <. A07: Hiển thị lỗi ...............|                                       |                                        |
+- [Đủ quyền và bộ lọc hợp lệ] ----------------------------------------------------------------------------------------------+
|       |                                   |                                       |----+  A08: Query snapshot              |
|       |                                   |                                       |<---+  tính tổng toàn tập lọc           |
|       |                                   <. A09: Kết quả/queried_at .............|                                        |
|       <. A10: Báo cáo/lọc áp dụng ........|                                       |                                        |
|       |   Sửa ô lọc chưa đổi kết quả hiện tại; Áp dụng tạo query mới.             |                                        |
+----------------------------------------------------------------------------------------------------------------------------+

SQ23-B — Xuất CSV theo bộ lọc đã áp dụng (hình riêng)

        Kế toán                             Giao diện báo cáo học phí               Hệ thống
        |                                   |                                       |
        |- B01: Chọn Xuất CSV -------------->                                       |
        |                                   |- B02: Xuất theo applied filters ------>
        |                                   |                                       |----+  B03: Kiểm tra lại
        |                                   |                                       |<---+  quyền và bộ lọc
+- alt [Không đủ quyền hoặc bộ lọc sai] -------------------------------------------------------------------------------------+
|       |                                   <. B04: Không thể xuất .................|                                        |
|       <. B05: Thông báo phù hợp ..........|                                       |                                        |
+- [Đủ quyền và bộ lọc hợp lệ] ----------------------------------------------------------------------------------------------+
|       |                                   |                                       |----+  B06: Query snapshot mới          |
|       |                                   |                                       |<---+  toàn bộ kết quả lọc              |
|       |                                   |                                       |----+  B07: Chuẩn bị CSV an toàn        |
|       |                                   |                                       |<---+  kiểm số dòng/kích thước          |
| +- alt [Vượt 10.000 dòng dữ liệu hoặc 5 MB] -----------------------------------------------------------------------------+ |
| |     |                                   <. B08: Từ chối toàn file ..............|                                      | |
| |     <. B09: Đề nghị thu hẹp lọc ........|                                       |                                      | |
| +- [Trong giới hạn và tạo file hoàn tất] --------------------------------------------------------------------------------+ |
| |     |                                   <. B10: CSV/queried_at mới .............|                                      | |
| |     <. B11: Tải file CSV ...............|                                       |                                      | |
| +- [Lỗi truy vấn hoặc tạo file] -----------------------------------------------------------------------------------------+ |
| |     |                                   <. B12: Không thể hoàn tất .............|                                      | |
| |     <. B13: Báo lỗi/không file .........|                                       |                                      | |
| +------------------------------------------------------------------------------------------------------------------------+ |
+----------------------------------------------------------------------------------------------------------------------------+
```

Nhãn trong hình chữ rút gọn; bảng thông điệp là nội dung đầy đủ. Các bước
query/tổng hợp/chuẩn bị file là self-message trên Hệ thống. Các phản hồi đi
qua Giao diện, không trực tiếp từ Hệ thống tới actor.

## 7. Note và cách dựng

Note nên gắn:

1. A08: **“Doanh thu theo ngày xác nhận Payment; tổng phải thu theo ngày phát
   hành Invoice không CANCELLED. Công nợ/quá hạn là UNPAID hiện tại, không kỳ ngày.”**
2. A10: **“Kết quả gắn bộ lọc đã áp dụng và queried_at; sửa ô lọc chưa đổi kết
   quả. Tổng trên toàn tập lọc, không chỉ trang.”**
3. B06: **“CSV query mới theo applied filters/cột báo cáo, có snapshot/thời
   điểm riêng; không cam kết bằng snapshot trang trước.”**
4. B07/B08: **“Giới hạn 10.000 dòng dữ liệu hoặc 5 MB; vượt thì từ chối
   toàn file. CSV escape/an toàn công thức, không sửa dữ liệu gốc.”**

Dựng SQ23-A/B riêng cùng ba lifeline. A đặt `alt` quyền/validation rồi
query/kết quả; B đặt `alt` quyền và `alt` các kết quả tạo file. Gắn
activation/note. Không cần `loop` đọc từng Invoice/Payment hoặc từng dòng file.

## 8. Checklist

- Chỉ Accountant xuất CSV; không nhập quyền Director của SQ24.
- Kỳ mặc định tháng VN; công nợ không bị lọc theo kỳ thu/phát hành.
- Không tính nợ bằng billed minus revenue, không chi phí/lợi nhuận/as-of.
- Giữ tiền tổng chính xác; tổng toàn tập khác tổng current page.
- Chưa Áp dụng thì giữ đúng bộ lọc/queried_at của kết quả đang xem.
- Query xuất mới, không tái dùng snapshot UI; dữ liệu mới có thể khác.
- Đúng cột/phạm vi báo cáo, không profile/Invoice dump hoặc export riêng F28.
- Vượt dòng/kích thước hoặc lỗi file không tải một phần.
- Không PDF/XLSX/email/chart/quyền in mới.
- Đọc/xuất không đổi Invoice/Payment và không tự audit mọi lần xem.
