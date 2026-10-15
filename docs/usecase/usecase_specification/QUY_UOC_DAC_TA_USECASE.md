# Quy ước viết đặc tả use case cho báo cáo

Ngày lập: 15/10/2026.

Trạng thái: Đề xuất để chủ dự án xem xét; chưa thay thế baseline nghiệp vụ hoặc chốt danh sách và mã use case.

## Cập nhật phạm vi biên soạn do chủ dự án chọn ngày 15/10/2026

Chủ dự án chọn bám 37 chức năng nghiệp vụ: lập 37 bảng chính theo NV01–NV39, loại NV10/NV11; dùng `DANH_SACH_37_DAC_TA_USECASE.md` trong cùng thư mục. Lựa chọn này thay thế đề xuất phân rã 83 mục và hướng dẫn một mục tiêu độc lập cho mọi bảng ở các phần dưới, trong phạm vi tổ chức báo cáo. Các hướng dẫn hình thức còn là đề xuất.

Mỗi nhóm NV phải bao phủ đủ các thao tác bằng tiểu luồng/nhánh có tên, giữ đúng tác nhân, guard và kết quả. NV36–NV39 là bốn xử lý tự động hỗ trợ, không là vai trò/menu độc lập. Ba chức năng chung Đăng xuất, Tự đổi mật khẩu và Sửa hồ sơ cá nhân vẫn phải mô tả ở phụ mục theo E01 mục 8, không tự thêm NV mới. Bản Đăng nhập đã viết thử mới bao phủ phần đăng nhập của NV01; bảng NV01 chính thức cần thêm luồng tự đăng ký Student. Mã UC-001–UC-037 liên tục là đề xuất, mã NV gốc giữ nguyên.

## 1. Cấu trúc quan sát được trong mẫu

Nguồn: `mau_dac_ta_usecase.pdf` trong cùng thư mục. PDF có 88 trang, tương ứng số trang in 124–211. Có 53 bảng, từ Bảng 2.4 đến Bảng 2.56, được trình bày trong 15 nhóm nghiệp vụ.

Cả 53 bảng có cùng chín mục và cùng thứ tự:

| STT | Nhãn trong mẫu | Nội dung quan sát được |
| --- | --- | --- |
| 1 | Use Case ID | Mã duy nhất, dạng UC-12, UC-13…; không trùng số thứ tự bảng. |
| 2 | Tên chức năng | Tên hành động hoặc nhóm chức năng. |
| 3 | Mô tả | Mục đích, phạm vi, kết quả khái quát và quy tắc quan trọng. |
| 4 | Tác nhân | Một hoặc nhiều tác nhân, liệt kê từng dòng. |
| 5 | Điều kiện trước | Quyền truy cập, dữ liệu và trạng thái cần có khi bắt đầu. |
| 6 | Điều kiện tiên quyết | Nội dung thực tế mô tả kết quả sau thực hiện, chẳng hạn dữ liệu được lưu hoặc danh sách được hiển thị. |
| 7 | Luồng chính | Các bước đánh số 1, 2, 3…; mô tả hành động tác nhân và phản hồi hệ thống. |
| 8 | Luồng thay thế | Nhánh hợp lệ hoặc kết quả khác của nghiệp vụ; thường có điểm quay lại hoặc kết thúc. |
| 9 | Luồng ngoại lệ | Dữ liệu sai, thiếu quyền, xung đột, sai trạng thái hoặc trường hợp ngăn hoàn thành luồng chính. |

Mỗi bảng có hai cột: cột nhãn hẹp, cột nội dung rộng. Phía trên có tiêu đề mã/tên use case và chú thích bảng. Các bảng dài được tiếp tục qua trang; số bước không bắt đầu lại theo trang.

Một số kiểu đặc tả xuất hiện trong mẫu:

- Nhóm chức năng: UC-12 quản lý đặt phòng và lưu trú, gọi các chức năng chi tiết.
- Tra cứu: UC-13 tra cứu phòng, không có kết quả là luồng thay thế.
- Xử lý dùng chung: UC-15 kiểm tra khả dụng, được nghiệp vụ khác gọi.
- Thay đổi nghiệp vụ: UC-16 ghi nhận cọc và xác nhận đặt phòng.
- Kiểm soát kỹ thuật: UC-80 xử lý yêu cầu trùng lặp.

Đây là các kiểu trình bày của mẫu, không phải danh sách chức năng được đưa vào dự án trung tâm Anh ngữ.

## 2. Cấu trúc thống nhất đề xuất

Giữ hai cột và chín mục. Đổi mục thứ sáu thành **Hậu điều kiện**: “điều kiện trước” và “điều kiện tiên quyết” đều chỉ điều kiện cần có từ trước, không nên dùng hai nhãn này cho trước/sau xử lý.

| Mục | Quy ước nội dung |
| --- | --- |
| Use Case ID | Mã ổn định và duy nhất cho từng use case chi tiết. Đề xuất dạng UC-001, UC-002…; chỉ gán sau khi lập danh sách và đối chiếu sơ đồ. |
| Tên chức năng | Động từ + đối tượng + phạm vi cần thiết, ví dụ “Xác nhận thanh toán học phí”. Dùng cùng một tên trong bảng, sơ đồ và phần mô tả liên quan. |
| Mô tả | Khoảng 2–4 câu: tác nhân muốn đạt mục tiêu gì, thao tác bắt đầu từ đâu và xử lý trong phạm vi nào. Có thể ghi sự kiện kích hoạt tại đây nếu cần. Không chép lại toàn bộ các bước. |
| Tác nhân | Ghi “Chính: …”; thêm “Tham gia: …” khi có tác nhân thực sự tương tác trong cùng use case. Không liệt kê mọi vai trò chỉ vì đều dùng chung hệ thống. |
| Điều kiện trước | Liệt kê điều kiện phiên/quyền, phạm vi sở hữu, dữ liệu và trạng thái ban đầu quan trọng. Không đưa kết quả cần tạo vào mục này. |
| Hậu điều kiện | Ghi “Thành công: …” và “Không thành công/hủy: …” khi phù hợp. Nêu kết quả dữ liệu, trạng thái và tính nhất quán; tra cứu chỉ tạo kết quả hiển thị, không tự đổi trạng thái. |
| Luồng chính | Một đường đi thành công điển hình, từ khởi tạo đến kết quả; đánh số liên tục. Mỗi bước nêu rõ ai làm gì. |
| Luồng thay thế | Các lựa chọn hợp lệ, không có dữ liệu hoặc người dùng chủ động dừng. Mỗi nhánh có điều kiện, cách xử lý và điểm quay lại/kết thúc. Không tự tạo nhánh để đủ mục. |
| Luồng ngoại lệ | Trường hợp không thể tiếp tục bình thường: dữ liệu sai, mất quyền/phiên, sai trạng thái, dữ liệu thay đổi đồng thời, lỗi lưu hoặc mất phản hồi. Ghi phản ứng và ảnh hưởng dữ liệu cụ thể. |

Nếu không có luồng thay thế riêng, ghi “Không có luồng thay thế riêng.” Chỉ đưa ngoại lệ thực sự áp dụng, không sao chép một bộ lỗi giống nhau vào mọi use case.

## 3. Phạm vi của một bảng

Một bảng chi tiết mô tả một mục tiêu nghiệp vụ có kết quả xác định. Ví dụ: “Xác nhận nhập học”, “Duyệt hồ sơ nhập học”, “Xác nhận thanh toán” là ba use case riêng vì khác tác nhân, điều kiện và kết quả.

Không gộp nhiều mục tiêu thành “Quản lý…” rồi chỉ mô tả mở trang/chọn chức năng/lưu dữ liệu. Nếu cần bảng tổng quan nhóm, phải ghi rõ là tổng quan và dẫn tới các bảng chi tiết, không tính đó là đặc tả đầy đủ cho mọi hành động bên trong.

Không tách mỗi nút giao diện, endpoint hoặc kiểm tra kỹ thuật thành một use case. Kiểm tra quyền, ghi nhật ký, kiểm soát yêu cầu lặp và kiểm tra xung đột thường được mô tả trong nghiệp vụ liên quan. Chỉ tách khi có mục đích tài liệu rõ và truy vết được với sơ đồ đã thống nhất.

Không ép số bảng bằng 37 NV, 32 F, 21 W hoặc số hình sơ đồ. Một chức năng NV có thể cần nhiều use case; một use case có thể liên quan nhiều màn hình.

Các mã UC00–UC09 của bộ sơ đồ hiện có là mã hình/tài liệu nhóm; không tự coi đó là chín mã use case chi tiết. Lập bảng ánh xạ trước khi gán mã chính thức hoặc sửa sơ đồ.

## 4. Cách viết bước và đánh số nhánh

### Luồng chính

- Dùng `1.`, `2.`, `3.`…; không đánh lại theo trang.
- Viết rõ chủ thể: “Học viên chọn…”, “Kế toán xác nhận…”, “Hệ thống kiểm tra…”.
- Một bước thể hiện một hành động hoặc một nhóm xử lý gắn chặt cùng mục đích. Không bắt buộc tác nhân/hệ thống luân phiên máy móc.
- Nêu điều kiện quyết định bằng nghiệp vụ: kiểm tra hóa đơn chưa thanh toán, số tiền bằng số tiền hóa đơn…; tránh “Hệ thống kiểm tra hợp lệ” mà không giải thích kiểm tra gì.
- Thường khoảng 5–9 bước; đây là định hướng, không phải hạn mức bắt buộc.
- Nếu cần xác nhận nhạy cảm, đặt bước xác nhận trước bước ghi nhận. Không thêm xác nhận thứ hai cho mọi thao tác thông thường.
- Kết thúc bằng kết quả cụ thể, không dừng ở “nhấn Lưu”.

### Luồng thay thế và ngoại lệ

Mẫu dùng `3a`, `4a`, `5a` cho cùng một nhánh. Để người đọc luôn biết nhánh rẽ từ bước nào, đề xuất giữ số bước gốc cho toàn nhánh:

```text
4a. Không có kết quả phù hợp với bộ lọc.
4a.1. Hệ thống hiển thị danh sách trống và thông báo không có kết quả.
4a.2. Tác nhân sửa bộ lọc và quay lại bước 3, hoặc kết thúc tra cứu.

4b. Bộ lọc có khoảng ngày không hợp lệ.
4b.1. Hệ thống chỉ rõ trường cần sửa và chưa thực hiện tra cứu.
4b.2. Tác nhân sửa dữ liệu và quay lại bước 3.
```

`4a` và `4b` cùng rẽ tại bước 4. Chữ cái phân biệt các nhánh, tiếp tục a/b/c… trong phạm vi các nhánh của cùng bước; không dùng trùng một mã cho hai nhánh dù nằm ở hai mục khác nhau. Số thập phân chỉ thứ tự xử lý trong nhánh. Các bước được dẫn chiếu phải có thật.

Phân loại theo ý nghĩa, không theo chữ cái:

- Thay thế: không có kết quả, chọn bộ lọc khác, chủ động hủy trước khi ghi nhận hoặc một phương án nghiệp vụ được cho phép.
- Ngoại lệ: dữ liệu không hợp lệ, thiếu quyền, phiên hết hạn, hồ sơ không còn đúng trạng thái, hết chỗ, trùng lịch hoặc lỗi ghi nhận.
- Yêu cầu gửi lại không tự là lỗi. Chỉ mô tả kết quả không tạo hiệu ứng lần hai khi nghiệp vụ có hợp đồng hỗ trợ; không suy rộng cơ chế này sang đăng ký tài khoản, sửa hồ sơ hoặc hiển thị lại mật khẩu tạm.

Kết thúc nhánh bằng “Quay lại bước …”, “Tiếp tục tại bước …” hoặc “Kết thúc use case”. Với mất phản hồi, phải tra cứu trạng thái/kết quả có quyền trước khi quyết định bước tiếp theo; không khẳng định chưa lưu và không tự gửi lại lệnh.

## 5. Ngôn ngữ và nội dung nghiệp vụ

- Viết tiếng Việt, thì hiện tại, câu ngắn và rõ chủ thể. Không đưa lời hướng dẫn người biên soạn vào nội dung bảng báo cáo.
- Dùng tên tác nhân thống nhất: Học viên, Tuyển sinh, Giáo vụ, Giáo viên, Kế toán, Giám đốc, Quản trị viên. Người chưa đăng nhập có thể được mô tả là người đăng ký tài khoản; không tạo thêm vai trò tài khoản.
- “Hệ thống” là bên xử lý trong luồng, không phải tác nhân bên ngoài hoặc vai trò thứ tám.
- Dùng “xác nhận nhập học” cho thao tác Học viên; “duyệt hồ sơ nhập học” cho thao tác Tuyển sinh. Không gọi cả hai là “duyệt”.
- Phân biệt hóa đơn với thanh toán; nháp điểm với gửi điểm; trả lại với chốt công bố; hủy hồ sơ với hủy đặt ca.
- Trạng thái viết bằng tiếng Việt; thêm mã kỹ thuật trong ngoặc tại vị trí cần phân biệt chính xác, chẳng hạn “Đã xác nhận nhập học (CONFIRMED)”. Có bảng thuật ngữ chung thì không cần lặp mã ở mọi bước.
- Dùng điểm thang 100, tiền VND nguyên đồng, ngày giờ theo Việt Nam. Chỉ ghi các giới hạn liên quan trực tiếp tới mục tiêu.
- Không đưa URL API, tên lớp C#, câu SQL, JWT/CSRF, mã HTTP, RowVersion hoặc chi tiết khóa dữ liệu vào luồng nghiệp vụ khi không cần thiết. Các hợp đồng đó nằm ở thiết kế kỹ thuật và phần truy vết.
- Khi cần tính nguyên tử, viết rõ các kết quả phải cùng thành công: chẳng hạn “Hệ thống ghi nhận duyệt hồ sơ, phát hành một hóa đơn và lưu nhật ký trong cùng giao dịch; nếu một phần thất bại thì không ghi nhận phần nào.”
- Không viết chung “mọi lỗi đều giữ nguyên toàn bộ dữ liệu”. Hậu quả phụ thuộc nghiệp vụ; ví dụ đăng nhập sai có thể cập nhật bộ đếm, lỗi ghi nhật ký bảo mật có chính sách riêng.
- Chỉ ghi nhật ký/audit ở các thao tác đã có yêu cầu; không thêm audit cho mọi lần tra cứu hoặc biến điểm danh thành lịch sử audit mới.
- Không thêm nghiệp vụ từ mẫu khách sạn: thanh toán trực tuyến, hoàn tiền, phê duyệt của Giám đốc hoặc quyền quản trị vượt baseline.

## 6. Quan hệ và truy vết

Tạo chỉ mục biên soạn riêng: mã use case chi tiết → hình sơ đồ liên quan → NV → W → F → nguồn yêu cầu/quyết định → mã triển khai cần đối chiếu.

Chỉ mục này phục vụ kiểm tra tính đầy đủ; không cần thêm tất cả các mã vào chín mục của bảng báo cáo.

Nếu có quan hệ include/extend, nội dung phải phù hợp với sơ đồ. Include là xử lý được bao gồm bắt buộc trong phạm vi gọi; extend là hành vi mở rộng có điều kiện. Không dùng chúng chỉ để nối thứ tự màn hình, và không coi mọi luồng thay thế là một extend riêng.

Thứ tự nguồn:

1. Yêu cầu của chủ dự án trong công việc hiện tại và các quyết định đã được chấp thuận.
2. `docs/requirements.md`, ADR liên quan và baseline E01–E05.
3. Mã thực tế, checkpoint mới nhất và evidence tương ứng để xác minh hành vi đã triển khai.
4. Báo cáo và sơ đồ cũ để truy vết; hình minh họa không mở thêm yêu cầu.

Khi code và tài liệu lệch nhau, ghi nhận và đối chiếu điểm lệch; không tự sửa quy tắc đã chấp thuận hoặc mô tả tính năng còn thiếu như đã có. Đặc biệt NV04 còn thiếu hiển thị tường minh điểm tốt nhất/lượt được chọn trên UI; không suy từ hợp đồng tính Band rằng mọi chi tiết hiển thị đã hoàn thành.

## 7. Trình bày trong Word

- Tiêu đề: `UC-xxx. Tên chức năng`, in đậm; bên dưới là chú thích `Bảng x.y. Đặc tả use case Tên chức năng`. Số chương/bảng theo vị trí thật trong báo cáo, không sao chép chương 2 của mẫu.
- Bảng hai cột; đề xuất cột nhãn khoảng 22%, nội dung khoảng 78%. Dùng cùng độ rộng cho tất cả bảng.
- Cột nhãn căn trái, in đậm, căn trên; nội dung căn trái, căn trên. Giữ đường viền đơn màu đen. Đây là điều chỉnh đề xuất để nhãn dễ đọc hơn mẫu căn giữa theo chiều dọc.
- Dùng font/cỡ chữ theo quy cách chung của báo cáo; nếu chưa có, đề xuất Times New Roman 13 pt. Không thu nhỏ font để ép mỗi use case vào một trang.
- Mỗi bước, điều kiện hoặc tác nhân là một đoạn riêng trong ô. Giữ khoảng cách đoạn và lề ô nhất quán.
- Cho phép bảng dài qua trang; giữ tiêu đề/chú thích cùng phần đầu bảng, tránh để nhãn hoặc một dòng bước đơn lẻ ở cuối trang. Không cấm tách một hàng dài hơn một trang vì có thể gây khoảng trắng lớn hoặc tràn trang.
- Không bắt đầu lại số bước khi bảng sang trang. Không lặp caption như thể đó là một bảng mới.

## 8. Khung trống để biên soạn

**UC-xxx. [Động từ + đối tượng]**

*Bảng x.y. Đặc tả use case [Tên chức năng]*

| Mục | Nội dung |
| --- | --- |
| Use Case ID | UC-xxx |
| Tên chức năng | [Tên thống nhất với danh sách và sơ đồ] |
| Mô tả | [Mục tiêu, sự kiện bắt đầu nếu cần và phạm vi] |
| Tác nhân | Chính: [Tác nhân]. Tham gia: [Chỉ ghi khi có]. |
| Điều kiện trước | [Phiên/quyền, sở hữu, dữ liệu và trạng thái ban đầu liên quan] |
| Hậu điều kiện | Thành công: [Kết quả cụ thể]. Không thành công/hủy: [Bảo đảm và ảnh hưởng thực sự áp dụng]. |
| Luồng chính | 1. [Tác nhân khởi tạo]. 2. [Hệ thống phản hồi]. … n. [Kết quả cuối]. Khi đưa vào Word, mỗi bước là một đoạn riêng. |
| Luồng thay thế | [Mã nhánh gắn bước gốc; điều kiện; xử lý; điểm quay lại/kết thúc] hoặc “Không có luồng thay thế riêng.” |
| Luồng ngoại lệ | [Mã nhánh gắn bước gốc; điều kiện; phản ứng; ảnh hưởng dữ liệu; điểm quay lại/kết thúc] |

## 9. Kiểm tra trước khi đưa vào báo cáo

1. Mã/tên duy nhất, truy vết được với sơ đồ; không nhầm mã hình, NV, F và mã use case.
2. Một mục tiêu rõ; tác nhân đúng quyền; điều kiện trước khác hậu điều kiện.
3. Luồng chính có kết quả nghiệp vụ cụ thể; mỗi nhánh có điểm rẽ và kết thúc/quay lại hợp lệ.
4. Đúng trạng thái, thời điểm, giới hạn và tác động phụ thuộc; không tạo quyền mới hoặc trạng thái suy diễn mới.
5. Phân biệt kết quả không có dữ liệu, lỗi bị từ chối và mất phản hồi; không yêu cầu gửi lại mù.
6. Nội dung phù hợp baseline và hành vi thực tế đã đối chiếu; mục còn thiếu được ghi nhận trong tài liệu làm việc.
7. Hình thức thống nhất; không cắt chữ, không bước mồ côi hoặc tiêu đề tách khỏi bảng.

Tài liệu này chỉ đề xuất quy ước. Chưa soạn các đặc tả cụ thể, chưa đổi mã sơ đồ và không sửa bản Word nguồn của chủ dự án.
