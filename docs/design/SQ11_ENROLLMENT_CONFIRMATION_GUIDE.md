# SQ11 — Hướng dẫn vẽ sequence học viên xác nhận nhập học

Ngày: 02/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA),
mức nghiệp vụ Actor → Boundary → Control. Bố cục/mã thông điệp là gợi ý,
không thay đổi quyết định đã duyệt.

Nguồn: [SEQUENCE_PLAN](SEQUENCE_PLAN.md) SQ11;
[FUNCTION_SPEC](FUNCTION_SPEC.md) NV05;
[WORKFLOW_SPEC](WORKFLOW_SPEC.md) W09;
[requirements](../requirements.md) ADR-016/021/029–031/038/090/093/133.

## 1. Mục tiêu và ranh giới

**Mục tiêu:** Học viên xác nhận nguyện vọng nhập học sau khi có kết quả đầu
vào/Band, để hồ sơ chuyển BAND_ASSIGNED → CONFIRMED và chờ Tuyển sinh xử lý.

**Điều kiện trước:** Học viên đăng nhập bằng phiên thường hợp lệ, thao tác
trên hồ sơ của mình. Được xem hồ sơ không đồng nghĩa đủ điều kiện xác nhận.

**Thành công:** Registration CONFIRMED, ghi người xác nhận/thời gian Hệ thống
và audit bắt buộc. Khóa thi lại/sửa điểm đầu vào/Band theo quy tắc đã chốt.

**Thất bại:** không chuyển trạng thái hoặc ghi xác nhận một phần; hiển thị
lý do phù hợp trong scope và cho tải lại thông tin hiện tại.

SQ11 không duyệt tuyển sinh, không tạo hóa đơn/thanh toán/lớp. Những bước
đó thuộc SQ12/SQ14/SQ17. Học viên không phải chờ Invoice để xác nhận SQ11.

## 2. Thành phần cần đặt trong EA

| Trái → phải | Tên lifeline | Loại/vai trò |
| --- | --- | --- |
| 1 | Học viên | Actor Student, xác nhận cho hồ sơ của mình. |
| 2 | Giao diện xác nhận nhập học | Boundary, màn hình F09. |
| 3 | Hệ thống quản lý trung tâm | Control, đọc/kiểm tra hồ sơ, Band, lượt thi và ghi xác nhận. |

Dùng message/return message, self-message, activation, `alt`, note.
`opt [Học viên chọn xác nhận nhập học]` đặt sau hiển thị: chỉ xem không
làm hồ sơ chuyển trạng thái.

Không thêm lifeline Tuyển sinh/Teacher/Kế toán; dữ liệu kết quả đã được tạo
ở SQ10. Bàn giao hồ sơ cho Tuyển sinh là bước nghiệp vụ kế tiếp, không là
message tự động gọi người đó hoặc gửi thông báo chưa được yêu cầu.

## 3. Dữ liệu và guard

| Nội dung | Quy tắc đã chốt |
| --- | --- |
| Đầu vào | Hồ sơ của mình và lệnh xác nhận; không gửi Band/giá/thời điểm do Học viên chọn. |
| Trạng thái | Registration BAND_ASSIGNED, có Band hợp lệ và chưa kết thúc. |
| Lượt thi đang chờ | Chặn nếu lượt hai đang SCHEDULED hoặc đã thi nhưng chưa chấm, kể cả ca CLOSED. |
| Giải quyết chờ | Chờ giáo viên xử lý kết quả hoặc hủy booking hợp lệ trước start ở ca OPEN qua SQ09; không tự hủy booking bằng thao tác xác nhận. |
| Chỉ có lượt một | Không bắt buộc thi hai lượt; có Band hợp lệ và không còn lượt hai chờ thì được xác nhận. |
| Kiểm tra cuối | Kiểm lại trạng thái, kết quả/best/Band/booking khi ghi, chống race đặt ca/chấm sửa/hủy hồ sơ. |
| Dữ liệu khóa | Không thi lại hoặc sửa đầu vào/Band sau CONFIRMED. Course của hồ sơ đã bất biến từ trước, không phải mới cho sửa rồi khóa ở bước này. |
| Hồ sơ cá nhân | Không khóa toàn bộ StudentProfile; vẫn sửa các trường được phép qua SQ06. |
| Nguyên tử | CONFIRMED + actor/thời gian + audit bắt buộc cùng thành công hoặc cùng rollback. |

Band hiện có không đủ để suy nút xác nhận luôn khả dụng: đặt lượt hai vẫn
giữ BAND_ASSIGNED, nên phải kiểm tra trạng thái lượt thi riêng.

## 4. Thứ tự thông điệp

| Mã | Từ → Đến | Nhãn thông điệp | Vị trí/ý nghĩa |
| --- | --- | --- | --- |
| M01 | Học viên → Giao diện | Mở chức năng xác nhận nhập học | Chọn hồ sơ của mình. |
| M02 | Giao diện → Hệ thống | Yêu cầu thông tin hồ sơ và điều kiện xác nhận | Chưa ghi xác nhận. |
| M03 | Hệ thống → Hệ thống | Kiểm tra quyền và đọc hồ sơ, Band, tình trạng lượt thi | Chỉ dữ liệu của chủ hồ sơ. |
| M04 | Hệ thống → Giao diện | Trả kết quả không thể truy cập | `alt [Không được phép xem hồ sơ]`; không tiết lộ dữ liệu người khác. |
| M05 | Giao diện → Học viên | Hiển thị thông báo phù hợp | Nếu hết phiên, về đăng nhập; nhánh không có/ngoài scope không lộ chi tiết. |
| M06 | Hệ thống → Giao diện | Trả thông tin hồ sơ, Band và điều kiện xác nhận | `else [Được phép xem hồ sơ]`. |
| M07 | Giao diện → Học viên | Hiển thị thông tin và khả năng xác nhận | Chưa CONFIRMED; có thể chỉ xem. |
| M08 | Học viên → Giao diện | Chọn xác nhận nhập học | `opt [Học viên chọn xác nhận nhập học]`. |
| M09 | Giao diện → Hệ thống | Gửi yêu cầu xác nhận hồ sơ | Không phải lệnh duyệt hồ sơ. |
| M10 | Hệ thống → Hệ thống | Kiểm tra lại quyền, trạng thái, kết quả/Band và lượt thi | Không tin điều kiện đã hiển thị ở M07. |
| M11 | Hệ thống → Giao diện | Trả kết quả không đủ điều kiện xác nhận | `alt [Không đủ điều kiện xác nhận]`. |
| M12 | Giao diện → Học viên | Hiển thị lý do và tải lại hồ sơ | Ví dụ còn lượt hai chưa chấm; không tự hủy ca/sửa Band. |
| M13 | Hệ thống → Hệ thống | Chuyển CONFIRMED, ghi actor/thời gian và audit bắt buộc | `else [Đủ điều kiện xác nhận]`; nguyên tử. |
| M14 | Hệ thống → Giao diện | Trả hồ sơ CONFIRMED | `alt` kết quả: `[Hoàn tất toàn bộ thay đổi]`. |
| M15 | Giao diện → Học viên | Thông báo đã xác nhận và hiển thị trạng thái mới | Chờ Tuyển sinh duyệt, chưa Invoice/PAID/lớp. |
| M16 | Hệ thống → Hệ thống | Hoàn tác thay đổi của yêu cầu | `else [Điều kiện đổi đồng thời hoặc lỗi xử lý]`. |
| M17 | Hệ thống → Giao diện | Trả kết quả không thể hoàn tất | Không để lại xác nhận/audit một phần. |
| M18 | Giao diện → Học viên | Hiển thị lỗi và tải lại hồ sơ | Không ghi đè trạng thái bằng form cũ. |

Các mã chỉ vị trí trong nhánh; không phải M04 đến M18 đều chạy nối tiếp.
M08 có thể không xuất hiện khi nút không khả dụng/không được chọn. Dù UI
đã kiểm tra, Hệ thống vẫn phải từ chối yêu cầu không đủ điều kiện.

## 5. Phác thảo bố cục bằng đường gạch

```text
SQ11 — Học viên xác nhận nhập học

        Học viên                            Giao diện xác nhận                    Hệ thống
        |                                   |                                     |
        |- M01: Mở xác nhận ---------------->                                     |
        |                                   |- M02: Yêu cầu thông tin hồ sơ ------>
        |                                   |                                     |----+  M03: Kiểm tra quyền
        |                                   |                                     |<---+  và đọc hồ sơ/Band
+- alt [Không được phép xem hồ sơ] --------------------------------------------------------------------------------------+
|       |                                   <. M04: Không thể truy cập ...........|                                      |
|       <. M05: Thông báo phù hợp ..........|                                     |                                      |
+- [Được phép xem hồ sơ] ------------------------------------------------------------------------------------------------+
|       |                                   <. M06: Hồ sơ/Band/điều kiện .........|                                      |
|       <. M07: Hiển thị thông tin .........|                                     |                                      |
| +- opt [Học viên chọn xác nhận nhập học] ----------------------------------------------------------------------------+ |
| |     |- M08: Chọn xác nhận -------------->                                     |                                    | |
| |     |                                   |- M09: Yêu cầu xác nhận ------------->                                    | |
| |     |                                   |                                     |----+  M10: Kiểm tra lại            | |
| |     |                                   |                                     |<---+  state/Band/lượt thi          | |
| | +- alt [Không đủ điều kiện xác nhận] ----------------------------------------------------------------------------+ | |
| | |   |                                   <. M11: Không thể xác nhận ...........|                                  | | |
| | |   <. M12: Lý do/tải lại hồ sơ ........|                                     |                                  | | |
| | +- [Đủ điều kiện xác nhận] --------------------------------------------------------------------------------------+ | |
| | |   |                                   |                                     |----+  M13: CONFIRMED,            | | |
| | |   |                                   |                                     |<---+  actor/time và audit        | | |
| | | +- alt [Hoàn tất toàn bộ thay đổi] --------------------------------------------------------------------------+ | | |
| | | | |                                   <. M14: Hồ sơ CONFIRMED ..............|                                | | | |
| | | | <. M15: Đã xác nhận ................|                                     |                                | | | |
| | | +- [Điều kiện đổi đồng thời hoặc lỗi xử lý] -----------------------------------------------------------------+ | | |
| | | | |                                   |                                     |----+  M16: Hoàn tác            | | | |
| | | | |                                   |                                     |<---+  thay đổi của yêu cầu     | | | |
| | | | |                                   <. M17: Không thể hoàn tất ...........|                                | | | |
| | | | <. M18: Lỗi/tải lại hồ sơ ..........|                                     |                                | | | |
| | | +------------------------------------------------------------------------------------------------------------+ | | |
| | +----------------------------------------------------------------------------------------------------------------+ | |
| +--------------------------------------------------------------------------------------------------------------------+ |
+------------------------------------------------------------------------------------------------------------------------+
```

M13 trong hình chữ rút gọn; tên đầy đủ và nhóm cập nhật ở bảng phía trên.
Đặt self-message/activation và fragment lồng nhau theo đúng nhánh. Nếu hình
quá sâu, có thể tách phần đọc thông tin M01–M07 ra hình phụ, giữ phần gửi
xác nhận M08–M18 làm hình chính; ghi tiền điều kiện đã mở hồ sơ đúng scope.

## 6. Note và cách dựng

Note có thể dùng nguyên văn:

1. M07/M10: **“BAND_ASSIGNED có Band hợp lệ; không còn lượt hai đang đặt
   hoặc đã thi chưa chấm. Ca CLOSED không có nghĩa kết quả đã xử lý xong.”**
2. M13: **“CONFIRMED, người/thời gian xác nhận và audit bắt buộc cùng
   thao tác. Kiểm tra lại kết quả/Band/booking khi hoàn tất.”**
3. M15: **“Đã xác nhận chưa là được Tuyển sinh duyệt. Không tạo Invoice,
   Payment hoặc xếp lớp tại đây; bước duyệt tiếp theo thuộc SQ12.”**
4. M15: **“Không thi lại/sửa điểm đầu vào/Band sau CONFIRMED; hồ sơ cá
   nhân vẫn sửa được các trường đã cho phép.”**

Tạo hình SQ11, đặt ba lifeline, vẽ M01–M03 rồi `alt` quyền đọc. Trong
nhánh xem được, vẽ M06/M07 và `opt` gửi xác nhận, sau đó các `alt`
kiểm tra/kết quả. Gắn note, activation; không thêm `loop` thi lại ở đây.

## 7. Checklist

- Actor là Student tự xác nhận, không phải Tuyển sinh duyệt.
- Chỉ đọc không đổi state; nút xác nhận không thay kiểm tra tại Hệ thống.
- Có Band nhưng đang chờ lượt hai vẫn bị chặn; không bắt tất cả thi hai lần.
- Không tự hủy ca CLOSED, tự ghi điểm 0 hoặc bỏ lượt chưa chấm để xác nhận.
- Chuyển BAND_ASSIGNED → CONFIRMED, không nhảy ADMITTED/PAID.
- Không tạo hóa đơn, thu tiền, xếp lớp, gửi email hay thông báo tự động mới.
- Hệ thống quyết định thời gian/actor/Band, người dùng không nhập chúng.
- Audit lỗi hoặc race không tạo xác nhận một phần; tải lại thông tin hiện tại.
- Mất phản hồi: đọc lại Registration/operation có quyền, không đoán đã/chưa
  thành công; gửi lặp cùng định danh/nội dung không tạo hiệu ứng thứ hai.
