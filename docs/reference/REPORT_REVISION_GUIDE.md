# Hướng dẫn thêm và chỉnh sửa báo cáo Nhom18_ThietKeXuLy.docx

Ngày lập: **07/10/2026**. Phần I–V và phụ lục A dùng từ ngữ nghiệp vụ tiếng Việt; tên bảng, trường và mã trạng thái kỹ thuật chỉ trình bày ở phần thiết kế dữ liệu. File này hướng dẫn sửa lần lượt theo cấu trúc Word; không sửa trực tiếp Word và không bổ sung quyết định nghiệp vụ mới.

## Cách sử dụng

Tài liệu gốc: [Nhom18_ThietKeXuLy.docx](../Nhom18_ThietKeXuLy.docx), SHA-256 `C7DE4DE6338670A94C99280DF89BDDC8E06FAE53A63E3B0405BBFE9DA0149F6C`.

Mở Navigation Pane của Word và đi theo các mục I–VII dưới đây. Với đoạn không có heading, dùng Ctrl+F tìm **cụm chữ nhận diện** được ghi trong bảng. `Bxxx` là vị trí phần tử XML dùng trong [bản đối chiếu](PROCESSING_REPORT_MEMORY_COMPARISON.md), không phải số trang. Không ghi Bxxx vào báo cáo nộp.

Mỗi hướng dẫn xác định **sửa ở đâu → làm gì → nội dung mới**. Các đoạn mang nhãn “Nội dung thay thế” có thể chép vào Word. Phụ lục A chứa bảng nghiệp vụ thay cho mục III; phụ lục B chứa dictionary thay cho VI.2; phụ lục C chứa danh sách FK thay cho VI.3.4. Các phụ lục này là nội dung để đưa vào vị trí tương ứng, không nhất thiết trở thành phụ lục của bản Word.

Chuẩn nghiệp vụ là [requirements](../requirements.md), [E01](../design/FUNCTION_SPEC.md), [E02](../design/WORKFLOW_SPEC.md); chuẩn dữ liệu là [E03](../design/DATABASE_SPEC.md); hợp đồng kỹ thuật là [E04](../design/E04_FINAL_CONTRACT.md). Trạng thái triển khai lấy [CURRENT_STATE](../CURRENT_STATE.md). Không lấy biểu mẫu cũ hoặc hình xem trước làm yêu cầu mới.

| Vị trí trong Word | Việc thực hiện |
| --- | --- |
| Bìa | Giữ thông tin trường, môn, nhóm và giáo viên; sửa mục lục/số hình sau cùng nếu có. |
| I. Hiện trạng | Sửa mô tả vai trò và luồng; bỏ ghi danh thay, thu hẹp mô tả nhật ký. |
| II. Yêu cầu | Thay các nhóm lưu trữ/tra cứu/tính toán/kết xuất bằng nội dung tại mục II dưới đây. |
| III. Bảng chức năng nghiệp vụ | Thay bằng 37 dòng còn hiệu lực ở phụ lục A, giữ mã nguồn; bỏ NV10/NV11. |
| IV. Biểu mẫu | Sửa từng biểu mẫu; thay phiếu thu riêng bằng mẫu thông tin xác nhận thanh toán; loại trường/phép xuất ngoài phạm vi được phép. |
| V. Mô tả chức năng | Sửa hai bảng tác nhân, bảng 30 chức năng, các trường hợp sử dụng; thêm phần quy tắc xử lý chung. |
| VI. Thiết kế dữ liệu | Thay hai hình dữ liệu; thay dictionary; sửa năm nhóm constraints; thêm xử lý giao dịch/concurrency nếu trình bày thiết kế kỹ thuật. |
| VII. Thiết kế giao diện | Sửa nhánh vai trò, tên Tuyển sinh, dữ liệu /100, chức năng tải; bổ sung màn hình và sequence chưa mô tả. |

## I. Hiện trạng

### I.1. Vị trí cần chỉnh

Giữ tên mục “I. Hiện trạng” và các nhóm quản lý hiện có.

| Tìm trong Word | Thao tác | Nội dung thay thế |
| --- | --- | --- |
| “Tra cứu hồ sơ (kiểm tra trùng lặp) và hỗ trợ học viên ghi danh trực tiếp” — B26 | Thay cả câu | “Tra cứu hồ sơ, theo dõi tiến độ tuyển sinh; hướng dẫn học viên tự đăng ký khóa và đặt ca thi. Nhân viên Tuyển sinh không tạo hồ sơ hoặc đặt ca thay học viên.” |
| “Duyệt hồ sơ hợp lệ để làm cơ sở kích hoạt tiến trình thu học phí” — B27 | Thay | “Duyệt hồ sơ đã được học viên xác nhận và có mức trình độ hợp lệ; đồng thời chuyển hồ sơ sang đã được duyệt nhập học và phát hành một hóa đơn chưa thanh toán.” |
| “Quản lý danh mục khóa học, tạo và thiết lập sĩ số” — B30 | Mở rộng | “Quản lý khóa học, mức trình độ, chương trình đào tạo và cấu hình thành phần điểm; tạo lớp theo chương trình với sĩ số tối đa được cấu hình trong khoảng 1–15.” |
| “Xếp học viên (đã hoàn tất thanh toán)” — B32 | Thay | “Xếp hồ sơ đã được duyệt nhập học có hóa đơn đã thanh toán vào lớp đang mở đúng khóa học/mức trình độ, còn chỗ và không trùng lịch học viên; tạo bảng điểm nháp cùng thao tác.” |
| “Nhập điểm đánh giá … và khóa/chốt bảng điểm” — B34 | Tách trách nhiệm | “Giáo viên nhập điểm, lưu nháp và gửi bảng điểm cả lớp. Giáo vụ trả lại hoặc chốt bảng điểm cả lớp sau khi lớp đã hoàn tất; học viên chỉ xem kết quả đã chốt.” |
| “Tra cứu và quản lý danh sách hóa đơn theo trạng thái” — B36 | Bổ sung trạng thái | Ghi đủ chưa thanh toán, đã thanh toán, đã hủy; quá hạn là cảnh báo, không trạng thái riêng. |
| “Xuất báo cáo tài chính” — B38 | Làm rõ | “Kế toán xem và xuất CSV báo cáo tài chính theo bộ lọc được phép; không xuất Excel.” |
| “Quản lý, cấp phát, khóa/mở tài khoản nhân sự” — B42 | Mở rộng | “Quản trị viên cấp tài khoản nhân sự, đặt lại mật khẩu, khóa/mở/ngừng/khôi phục tài khoản theo quyền; mỗi tài khoản có một trong bảy vai trò cố định.” |
| “mọi hoạt động cập nhật dữ liệu” — B44 | Thay cả câu B44 | “Hệ thống tính tổng điểm theo cấu hình chương trình đào tạo; chỉ lưu tổng chính thức khi bảng điểm đã chốt. Các thao tác thay đổi thuộc danh mục bắt buộc được ghi nhật ký cùng giao dịch; sự kiện bảo mật được xử lý theo chính sách riêng.” |

### I.2. Đoạn cần thêm

**Chèn sau câu cuối của mục I, ngay trước “II. Yêu cầu”.**

> Hành trình chính của hệ thống gồm: học viên tự đăng ký tài khoản → đăng ký khóa học → đặt ca thi đầu vào → giáo viên nhập điểm và hệ thống xác định mức trình độ → học viên xác nhận nhập học → Tuyển sinh duyệt và phát hành hóa đơn → Kế toán xác nhận đã thu đủ tiền → Giáo vụ xếp lớp và lập lịch → Giáo viên điểm danh, nhập và gửi bảng điểm → Giáo vụ chốt → học viên xem kết quả đã chốt. Thanh toán diễn ra bên ngoài phần mềm. Ban Giám đốc chỉ xem báo cáo tổng hợp; không tham gia phê duyệt hồ sơ, chương trình hoặc lớp.

Nếu mục “Hiện trạng” được dùng để kể cả quy trình thủ công thực tế, có thể giữ mô tả thực tế đã xác minh nhưng phải phân biệt với **phạm vi phần mềm đề xuất**. Không trình bày tính năng bị loại như chức năng hệ thống sẽ triển khai.

## II. Yêu cầu

**Thay phần từ “• Lưu trữ” đến hết “• Kết xuất” (B46–B66).** Giữ bốn nhóm yêu cầu hiện có; dùng nội dung sau.

### II.1. Nội dung thay thế — Lưu trữ

- **Tài khoản và hồ sơ:** thông tin đăng nhập, vai trò, liên hệ, mật khẩu đã băm và trạng thái tài khoản; hồ sơ học viên có ngày sinh bắt buộc, địa chỉ tùy chọn. Lưu thông tin phiên đăng nhập để kiểm soát thời hạn và thu hồi quyền truy cập.
- **Tuyển sinh và thi đầu vào:** hồ sơ đăng ký khóa học, ca thi, tối đa hai lượt thi mỗi hồ sơ, lịch sử đặt/đổi/hủy ca, điểm từng lượt và mức trình độ được xác định. Ghi nhận người thực hiện, thời điểm và lý do từ chối, hủy hoặc sửa kết quả theo quyền.
- **Cấu hình đào tạo:** thông tin khóa học và học phí; các mức trình độ và ngưỡng điểm; chương trình đào tạo theo từng khóa học/mức trình độ, số buổi học bắt buộc, thành phần điểm và trọng số. Thành phần điểm thuộc chương trình đào tạo.
- **Lớp và lịch:** lớp học có sĩ số tối đa trong khoảng 1–15, giáo viên phụ trách và trạng thái hoạt động; danh sách học viên đã được xếp lớp; ngày, giờ, phòng học và lịch sử buổi thay thế. Ca thi có sức chứa trong khoảng 1–20.
- **Điểm danh và điểm:** Có mặt/Vắng cho từng buổi học; điểm thành phần theo thang 0–100; bảng điểm nháp/đã gửi/đã trả lại/đã chốt, nhận xét và thông tin gửi/trả/chốt. Chưa nhập không được coi là điểm 0 hoặc Vắng.
- **Tài chính:** một hóa đơn cho mỗi hồ sơ được duyệt; xác nhận thu đủ tiền một lần cho mỗi hóa đơn bằng tiền mặt hoặc chuyển khoản. Sửa phương thức/mã tham chiếu cần lý do. Tiền dương, nguyên đồng Việt Nam; không học bổng, giảm giá, trả góp hoặc hoàn tiền.
- **Nhật ký và kiểm soát thao tác:** giữ lịch sử nghiệp vụ bắt buộc và sự kiện bảo mật; ghi nhận kết quả xử lý để tránh lặp tác động khi gửi lại cùng yêu cầu; cấp số hóa đơn theo năm Việt Nam. Không ghi mật khẩu dạng rõ hoặc thông tin xác thực bí mật vào nhật ký.

### II.2. Nội dung thay thế — Tra cứu

- Học viên tra cứu hồ sơ, lượt thi/mức trình độ, hóa đơn, lớp/lịch và kết quả đã chốt của mình.
- Tuyển sinh tra cứu hồ sơ trong phạm vi nhiệm vụ; Giáo vụ tra cứu cấu hình, ca thi, lớp, lịch và bảng điểm đã gửi theo quyền.
- Giáo viên chỉ tra cứu ca/lớp hiện được phân công và dữ liệu học viên được phép trong đó.
- Kế toán tra cứu hóa đơn chưa thanh toán/đã thanh toán/đã hủy, xác nhận thanh toán và báo cáo tài chính.
- Giám đốc xem tổng hợp tuyển sinh/đào tạo/tài chính không định danh học viên. Quản trị viên tra cứu nhật ký theo bộ lọc và chi tiết thao tác; quyền xem nhật ký không mở thêm quyền dữ liệu nghiệp vụ.

### II.3. Nội dung thay thế — Tính toán

- **Mức trình độ:** chọn điểm cao nhất trong các lượt đã có kết quả; bằng điểm chọn lượt một. Chọn mức có ngưỡng điểm tối thiểu lớn nhất không vượt quá điểm thi; các ngưỡng là 0/25/45/65/85. Giáo viên nhập điểm, hệ thống không tự chấm bài thi.
- **Tổng kết:** tổng = Σ(điểm thành phần × trọng số /100). Tính chính xác và làm tròn một lần cuối đến hai chữ số thập phân, từ 5 trở lên làm tròn lên. Tổng nháp được tính khi xem; tổng chính thức chỉ ghi nhận khi bảng điểm đã chốt. Tổng đã chốt từ 50 trở lên là đạt.
- **Hóa đơn:** lấy học phí khóa học tại thời điểm duyệt và giữ nguyên trên hóa đơn đã phát hành. Hạn thanh toán là ngày phát hành tại Việt Nam cộng 7 ngày lịch. Số HD-YYYY-NNNNNNNN do hệ thống cấp.
- **Doanh thu trong kỳ:** tổng số tiền đã xác nhận thu theo thời điểm xác nhận trong kỳ.
- **Phải thu trong kỳ:** tổng tiền các hóa đơn phát hành trong kỳ, loại hóa đơn đã hủy.
- **Công nợ hiện tại:** tổng tiền các hóa đơn chưa thanh toán; quá hạn là phần có hạn thanh toán trước ngày hiện tại tại Việt Nam. Không lấy phải thu trong kỳ trừ doanh thu trong kỳ làm công nợ; không phạt hoặc tự đổi trạng thái học viên.
- **Tiến độ và tỷ lệ đạt:** tiến độ theo số buổi học đã được xác nhận tổ chức so với số buổi bắt buộc của chương trình; thi cuối khóa theo dõi riêng. Tỷ lệ đạt chỉ tính kết quả đã chốt trong kỳ ngày chốt; không có kết quả để tính thì hiển thị “Chưa có dữ liệu”.

### II.4. Nội dung thay thế — Kết xuất

- Học viên in hóa đơn của mình qua trình duyệt từ dữ liệu mới hợp lệ; chưa thanh toán không là xác nhận đã thu tiền. Không chức năng xuất PDF riêng, biên lai hoặc tải phiếu điểm riêng.
- Kế toán xuất CSV báo cáo tài chính theo toàn bộ kết quả/bộ lọc/cột được phép, đúng giới hạn và định dạng E04.
- Giám đốc xem báo cáo tổng hợp trong ứng dụng; không xuất file. Nhật ký chỉ tra cứu trong ứng dụng, không xuất CSV/PDF.

### II.5. Nội dung bổ sung — Quy tắc chung

**Chèn sau Kết xuất, trước mục III**, với tiêu đề “Quy tắc nghiệp vụ và an toàn dữ liệu chung”.

> Mỗi thao tác phải kiểm tra vai trò, phạm vi sở hữu/phân công, trạng thái, thời gian và phiên đăng nhập hiện tại. Email đăng nhập không được thay đổi; thông tin cá nhân được lấy từ hồ sơ hiện tại, không sao chép cố định vào hồ sơ đăng ký khóa học. Ngày nghiệp vụ và lịch sử dụng giờ Việt Nam. Khi nhiều người cùng chỉnh sửa, hệ thống phát hiện dữ liệu đã thay đổi để tránh ghi đè hoặc vượt sức chứa. Thao tác nghiệp vụ, các cập nhật liên quan và nhật ký bắt buộc phải thành công cùng nhau; nếu có lỗi thì hoàn tác toàn bộ. Gửi lại cùng yêu cầu hợp lệ không tạo tác động lần hai; không ghi mật khẩu vào nhật ký hoặc phát lại mật khẩu từ yêu cầu cũ. Không xóa lịch sử, chuyển lớp, thi lại sau xác nhận nhập học, mở lại bảng điểm đã chốt, tự phạt nợ hoặc bổ sung học bổng.

## III. Bảng chức năng nghiệp vụ

### III.1. Cách thay bảng

**Thay toàn bộ tám bảng vai trò trong mục III (B69–B83) bằng phụ lục A của file này.** Giữ năm cột STT/Tên chức năng/Quy định–Điều kiện/Biểu mẫu/Ghi chú. Giữ STT nguồn để đối chiếu; bỏ dòng 10 và 11, không đánh lại 12 thành 10. Trước bảng Học viên, chèn:

> Hệ thống có 37 nghiệp vụ còn hiệu lực; NV10 và NV11 đã loại bỏ vì học viên tự đăng ký khóa và đặt ca. Bảy vai trò tài khoản là học viên, Tuyển sinh, Giáo vụ, Giáo viên, Kế toán, Giám đốc và Quản trị viên. Nhóm “Hệ thống” mô tả xử lý tự động, không phải vai trò thứ tám. Tên biểu mẫu chỉ nội dung hiển thị liên quan, không cấp thêm chức năng ký duyệt hoặc xuất chứng từ.

### III.2. Những dòng thay đổi nhiều nhất

| Bảng/dòng cũ | Nội dung phải kiểm sau khi thay |
| --- | --- |
| Học viên 1–8 | Đăng ký tài khoản tách đăng nhập; đổi/hủy đặt ca; lượt thi chờ xử lý chặn xác nhận; chỉ xem/in hóa đơn và kết quả đã chốt của mình. |
| Tuyển sinh 9–12 | Bỏ 10/11; dòng 12 thêm từ chối/hủy theo điều kiện. Duyệt hồ sơ và phát hành hóa đơn phải thành công cùng nhau. |
| Giáo vụ 13–20 | Cấu hình và khóa sửa theo lần sử dụng đầu; ca thi khởi tạo đóng; vòng đời lớp; chỉ xếp vào lớp đang mở; buổi thay thế/xác nhận đã tổ chức; kiểm tra bảng điểm cả lớp. |
| Giáo viên 21–26 | Người chấm ban đầu sửa điểm đầu vào theo điều kiện; ghi vắng; thời hạn điểm danh; lưu nháp từng học viên, gửi/gửi lại cả lớp. |
| Kế toán 27–29 | Có hóa đơn đã hủy; sửa phương thức/mã tham chiếu thanh toán có lý do; bỏ phiếu thu riêng, Excel và công thức công nợ sai kỳ. |
| Giám đốc 30–32 | Chỉ xem tổng hợp không định danh học viên, không xuất file. |
| Quản trị viên 33–35 | Mật khẩu tạm/đặt lại; ngừng/khôi phục tài khoản; không khóa/ngừng chính mình hoặc quản trị viên hoạt động cuối cùng; không xuất nhật ký. |
| Hệ thống 36–39 | Bằng điểm chọn lượt một; phát hành hóa đơn cùng duyệt; tổng chính thức chỉ ghi khi chốt; phân biệt nhật ký nghiệp vụ bắt buộc và sự kiện bảo mật. |

## IV. Biểu mẫu

**Giữ mục IV, nhưng đổi phần mở đầu thành:**

> Các mẫu dưới đây minh họa nội dung hiển thị trong phần mềm theo quyền của từng vai trò. Chúng không bổ sung nghiệp vụ ngoài đặc tả, không thay thế chứng từ tài chính có chữ ký/đóng dấu và không mặc định cấp chức năng xuất file.

### IV.1. Hồ sơ đăng ký / Biểu mẫu Đăng ký khóa học — B86–B112

| Vị trí trong biểu mẫu | Thao tác cụ thể |
| --- | --- |
| Thông tin học viên B91–B99 | Bỏ giới tính, nơi sinh, CCCD/Passport/ngày cấp/nơi cấp, nơi làm việc/trường, toàn phần phụ huynh bắt buộc. Không bắt họ tên viết HOA. Hiển thị mã học viên, họ tên, ngày sinh, email, số điện thoại, địa chỉ hiện tại theo phạm vi được phép. Các trường thuộc hồ sơ cá nhân được sửa tại biểu mẫu hồ sơ cá nhân, không sửa qua biểu mẫu hồ sơ đăng ký. |
| Bảng khóa B101 | Một hồ sơ đăng ký chọn một khóa học đang hoạt động; đổi thành cột “Mã khóa/Tên khóa/Học phí hiện tại”. Bỏ nhiều dòng khóa và ngày khai giảng/lịch lớp như thông tin đã cam kết tại lúc đăng ký. Học phí hiện tại là tham khảo, số tiền chỉ được ghi nhận cố định khi phát hành hóa đơn sau này. |
| B102–B107 | Bỏ ưu đãi/tổng phải đóng/trả góp/cà thẻ/chia đợt. Phương thức thanh toán thuộc bước Kế toán xác nhận thanh toán, không thuộc tạo hồ sơ đăng ký. |
| Cam kết B110 và bảng chữ ký B112 | Bỏ chính sách hoàn tiền/bảo lưu và chữ ký Tuyển sinh/Kế toán như bước xử lý hệ thống. Thay bằng hướng dẫn đặt ca sau đăng ký. |

**Mẫu nội dung thay thế:**

| Nội dung | Hiển thị/nhập |
| --- | --- |
| Học viên | Mã học viên, họ tên, email, số điện thoại hiện tại theo quyền |
| Khóa chọn | Một khóa học đang hoạt động, mã/tên/học phí tham khảo |
| Thao tác | Đăng ký khóa học |
| Kết quả | hồ sơ đăng ký chờ xử lý, mã hồ sơ, thời điểm tạo; chưa ca thi/mức trình độ/hóa đơn/lớp |
| Bước tiếp theo | Học viên tự chọn ca thi đầu vào còn phù hợp |

### IV.2. Danh sách lớp — B115–B124

Thay bảng B121 bằng **STT | Mã học viên | Họ tên | Email | Số điện thoại** cho bản Giáo viên. Bỏ giới tính/ngày sinh/phụ huynh/ghi chú bảo lưu-chuyển lớp. Phần đầu giữ mã/tên lớp, khóa học/mức trình độ/chương trình đào tạo, giáo viên, sĩ số; lịch ngày/giờ/phòng xem ở bảng lịch, không cố định một phòng/ca học cho toàn lớp học. Bỏ khối chữ ký nếu mô tả màn hình; không thêm quyền xuất file danh sách học viên.

### IV.3. Thời khóa biểu — B126–B132

Giữ hình tuần để minh họa, nhưng mỗi ô phải là buổi cụ thể có ngày, giờ bắt đầu/kết thúc, phòng học, lớp, loại buổi học/thi cuối khóa và trạng thái hiển thị hợp lệ. Không biến quy tắc tuần thành lịch lặp vô hạn trong cơ sở dữ liệu. Thay ghi chú Email/Zalo B132 bằng:

> Lịch hiển thị theo các buổi hiện hành trong hệ thống. Giáo vụ dời hoặc tạo buổi thay thế theo điều kiện nghiệp vụ; học viên/giáo viên đọc lại lịch để nhận dữ liệu mới. Không có chức năng tự gửi thông báo Email/Zalo trong phạm vi hiện tại.

### IV.4. Bảng điểm cá nhân — B134–B145

Thay bảng B139 bằng **Thành phần điểm | Loại | Trọng số (%) | Điểm /100**. Các dòng lấy từ cấu hình thành phần điểm của chương trình đào tạo, không cố định giữa kỳ hệ số 1/cuối kỳ hệ số 2. Cuối bảng ghi tổng /100, trạng thái và nhận xét; “Đạt/Không đạt” chỉ khi đã chốt, tổng nháp không gắn nhãn kết quả chính thức. Bỏ xếp loại thêm như Giỏi/Khá nếu chưa có quy tắc. Học viên chỉ xem bản đã chốt; Giáo viên có bản nháp theo quyền.

### IV.5. Bảng điểm danh — B148–B157

Thay ký hiệu x/p/k/m tại B153 bằng **Có mặt/Vắng/Chưa nhập**. Hai giá trị lưu là Có mặt hoặc Vắng; Chưa nhập không có bản ghi. Nên minh họa một buổi học với **Mã học viên | Họ tên | Có mặt/Vắng/Chưa nhập**; nếu giữ bảng tháng, mỗi cột là lịch học cụ thể, không thêm phân hệ tổng vắng/cảnh báo tự động. Bỏ “Cảnh báo” nếu hàm ý xử phạt/chặn đạt theo chuyên cần. Điểm danh không tự xác nhận buổi đã tổ chức.

### IV.6. Biên lai / Phiếu thu điện tử — B159–B173

**Thay toàn bộ tiểu mục 6 bằng “Thông tin xác nhận thanh toán học phí”.** Giữ số thứ tự 6 để các tiểu mục sau không phải đổi. Bỏ PT-…, mã biểu mẫu phiếu thu, ngày chứng từ riêng và chữ ký. Dùng mẫu:

| Nội dung | Quy tắc |
| --- | --- |
| Hóa đơn | Chọn hóa đơn chưa thanh toán có quyền; số hóa đơn và số tiền cố định lúc phát hành |
| Số tiền xác nhận | Bằng toàn bộ số tiền hóa đơn; không thu từng phần |
| Phương thức | Tiền mặt hoặc chuyển khoản |
| Mã tham chiếu | Bắt buộc khi chuyển khoản, không duy nhất toàn hệ thống |
| Xác nhận | Kế toán ghi nhận đã thu đủ tiền, cập nhật hóa đơn thành đã thanh toán và ghi nhật ký; các thay đổi phải thành công cùng nhau |
| Sau xác nhận | Hiển thị người và thời điểm xác nhận thu; chỉ sửa phương thức/mã tham chiếu có lý do và nhật ký. Không đổi tiền, hóa đơn liên quan hoặc thông tin xác nhận ban đầu |

Đây là nội dung hiển thị/xử lý thanh toán, không chứng từ xuất riêng.

### IV.7. Báo cáo tổng hợp — B176–B188

Đổi tên B178 thành **“Báo cáo tổng hợp học phí”**. Bỏ dòng thu giáo trình, toàn mục chi phí B183/B184, lợi nhuận B185/B186 và “Giám đốc phê duyệt” B187. Giữ bộ lọc kỳ/khóa học và các chỉ số doanh thu trong kỳ, phải thu trong kỳ, công nợ hiện tại, quá hạn hiện tại; giải thích hai chỉ số sau không theo kỳ ngày. Không tự thêm đơn giá trung bình, tỷ trọng hoặc chỉ số ngoài cấu trúc dữ liệu chuẩn. Bảng báo cáo và CSV phải dùng đúng cột trong đặc tả báo cáo đã duyệt (E04, nhóm D10); Kế toán có CSV, Giám đốc chỉ tổng hợp không định danh.

### IV.8. Hóa đơn học phí — B189–B208

Thay INV tại B194 bằng **HD-YYYY-NNNNNNNN**; bỏ địa chỉ học viên/MST cá nhân khỏi mẫu nếu không thuộc nội dung hiển thị được phép in đã duyệt. Bỏ bảng nhiều dòng hàng/phí tài liệu B200, giảm giá B202, VAT B203 và chữ ký/đóng dấu B208. Dùng bản in A4 dọc với số hóa đơn, tên hiện tại của chủ hồ sơ, khóa, số tiền cố định lúc phát hành, thời điểm phát hành, hạn thanh toán, trạng thái/cảnh báo quá hạn. Ghi:

> Hiển thị đúng trạng thái Chưa thanh toán/Đã thanh toán/Đã hủy. Hóa đơn chưa thanh toán không phải xác nhận đã thu tiền. In không đổi trạng thái hóa đơn; học viên lấy lại dữ liệu có quyền trước khi mở chức năng in trình duyệt.

## V. Mô tả chức năng

### V.1. Danh sách tác nhân và mô tả tác nhân — B213/B217

**Dùng bảng sau thay cho cả hai bảng tác nhân.** Nếu giữ hai bảng, diễn giải từ cùng nội dung để tránh mô tả khác nhau.

| Tác nhân | Phạm vi và trách nhiệm |
| --- | --- |
| Học viên | Tự đăng ký tài khoản, quản lý hồ sơ cá nhân; chọn khóa, đặt/đổi/hủy ca, xem điểm và mức trình độ, xác nhận nhập học; xem/in hóa đơn, lớp/lịch và kết quả đã chốt của mình. |
| Tuyển sinh | Tra cứu, duyệt/từ chối/hủy hồ sơ theo điều kiện; không ghi danh, đặt ca hoặc sửa hồ sơ cá nhân thay học viên. |
| Giáo vụ | Quản lý khóa học, mức trình độ, chương trình, thành phần điểm, ca thi, lớp, phân công, xếp lớp và lịch; trả/chốt bảng điểm cả lớp; không sửa điểm thay giáo viên. |
| Giáo viên | Chỉ xử lý ca/lớp được phân công; nhập/sửa điểm đầu vào, ghi vắng theo điều kiện; điểm danh, lưu nháp điểm/nhận xét từng học viên và gửi/gửi lại cả lớp. |
| Kế toán | Tra cứu hóa đơn, xác nhận thu đủ tiền, sửa phương thức/mã tham chiếu có lý do; xem/xuất CSV báo cáo tài chính theo quyền. |
| Giám đốc | Chỉ xem tổng hợp tuyển sinh/đào tạo/tài chính; không xem thông tin định danh học viên, chỉnh sửa, phê duyệt hoặc xuất file. |
| Quản trị viên | Cấp tài khoản nhân sự, đặt lại mật khẩu, quản lý trạng thái theo điều kiện và tra cứu nhật ký; không mặc định có quyền toàn bộ nghiệp vụ. |
| Hệ thống | Tự xác định mức trình độ, phát hành hóa đơn, tính tổng điểm và ghi nhật ký khi thực hiện nghiệp vụ; không phải vai trò tài khoản thứ tám. |

### V.2. Mô tả chi tiết từng chức năng — bảng B220

Giữ ba cột **STT | Chức năng | Mô tả**. STT dưới đây là **STT riêng của mục V cũ**, không phải mã nghiệp vụ ở mục III.

| STT cũ | Tên chức năng mới | Mô tả thay thế |
| --- | --- | --- |
| 1 | Đăng ký và đăng nhập | Học viên tự tạo tài khoản và hồ sơ cá nhân, đồng thời ghi nhật ký; thành công chưa đăng nhập. Mọi vai trò đăng nhập bằng email/mật khẩu; hệ thống xác định vai trò và phiên. Lỗi trước xác thực không tiết lộ tài khoản tồn tại. |
| 2 | Đăng ký và hủy hồ sơ khóa học | Khóa đang hoạt động; mỗi học viên chỉ có một hồ sơ chưa kết thúc cho cùng khóa. Tạo hồ sơ chờ xử lý; học viên hủy trước xác nhận nhập học. Đổi khóa cần hồ sơ mới và thi đầu vào lại. |
| 3 | Xem/in hóa đơn và hướng dẫn thanh toán | Chỉ hóa đơn của mình; trả tiền ngoài hệ thống, không tự xác nhận đã thanh toán. In trình duyệt từ dữ liệu mới, không PDF/biên lai riêng. |
| 4 | Xem thời khóa biểu | Chỉ lớp đã được xếp và lịch hiện hành của mình; không tự sửa/xếp/chuyển lớp. |
| 5 | Xem kết quả học tập | Chỉ kết quả đã chốt của mình sau khi lớp hoàn tất; thang 0–100, tổng từ 50 đạt; không tải phiếu điểm. |
| 6 | Tra cứu hồ sơ | Tuyển sinh tra cứu đúng phạm vi và theo dõi trạng thái; không sửa hồ sơ cá nhân hoặc tạo hồ sơ thay học viên. |
| 7 | Loại khỏi bảng | Bỏ Hỗ trợ ghi danh; không dùng số trống để thêm nghiệp vụ mới. |
| 8 | Duyệt/từ chối/hủy hồ sơ | Duyệt/từ chối hồ sơ đã xác nhận và có mức trình độ hợp lệ. Duyệt, ghi nhận nhập học, phát hành hóa đơn và nhật ký thành công cùng nhau. Hủy/từ chối theo điều kiện và lý do; không hủy hồ sơ đã thanh toán. |
| 9 | Quản lý cấu hình đào tạo | Chương trình theo khóa học/mức trình độ, số buổi học dương, tổng trọng số 100 và đúng một thành phần thi cuối khóa. Khóa sửa cấu hình theo lần sử dụng đầu. |
| 10 | Quản lý lớp | Lớp thuộc chương trình, sĩ số tối đa 1–15; đang chuẩn bị→đang mở→đang học→đã hoàn tất. Hủy lớp còn trống/chưa dạy theo điều kiện; không phê duyệt của Giám đốc. |
| 11 | Phân công/đổi giáo viên | Giáo viên hợp lệ, không trùng lịch; đổi trước khi dạy và chưa có điểm danh/điểm. Khóa tài khoản không tự phân công lại. |
| 12 | Xếp học viên vào lớp | Hồ sơ đã duyệt, hóa đơn đã thanh toán, đúng khóa/trình độ, lớp đang mở và còn chỗ, không trùng lịch học viên. Xếp lớp, tạo bảng điểm nháp và nhật ký thành công cùng nhau. |
| 13 | Lập/dời/thay lịch, xác nhận buổi | Lịch từng buổi cụ thể; lưu lịch tuần toàn bộ hoặc không buổi nào. Buổi thay thế giữ cùng buổi học bắt buộc; Giáo vụ xác nhận đã tổ chức theo điều kiện. |
| 14 | Kiểm tra/chốt bảng điểm | Giáo vụ xử lý toàn bộ bảng điểm đã gửi của lớp hoàn tất; trả lại có lý do hoặc chốt cả lớp. Không sửa điểm thay giáo viên. |
| 15 | Xem lịch giảng dạy | Giáo viên chỉ xem lịch ca/lớp được phân công theo quyền. |
| 16 | Xem danh sách lớp | Giáo viên chỉ danh sách được phân công: mã học viên/họ tên/email/số điện thoại; không ngày sinh/địa chỉ/phụ huynh. |
| 17 | Điểm danh | Buổi học hiện hành thuộc lớp phụ trách, từ giờ bắt đầu đến trước lớp hoàn tất. Có mặt/Vắng; chưa nhập khác Vắng. Lưu nhóm thao tác thành công toàn bộ hoặc hoàn tác. |
| 18 | Lưu nháp/gửi/gửi lại điểm | Điểm và nhận xét một học viên khi nháp/đã trả lại; chỉ gửi cả lớp khi lớp hoàn tất và đủ điểm, không gửi riêng một người. |
| 19 | Tra cứu hóa đơn | Kế toán xem hóa đơn chưa thanh toán/đã thanh toán/đã hủy và cảnh báo quá hạn; đọc không đổi trạng thái. |
| 20 | Xác nhận/sửa thông tin thanh toán | Thu đủ một lần bằng tiền mặt/chuyển khoản; xác nhận và nhật ký cùng thành công. Chỉ sửa phương thức/mã tham chiếu có lý do; giữ nguyên tiền, hóa đơn và thời điểm xác nhận. |
| 21 | Báo cáo tài chính/CSV | Doanh thu theo thời điểm xác nhận thu; phải thu theo thời điểm phát hành; công nợ là hóa đơn chưa thanh toán hiện tại. CSV truy vấn mới theo đúng bộ lọc, cột và giới hạn. |
| 22 | Báo cáo tuyển sinh | Giám đốc xem tổng hợp không định danh; trạng thái hiện tại, đếm học viên không trùng đúng phạm vi. |
| 23 | Báo cáo đào tạo | Tiến độ theo buổi học đã tổ chức/số buổi chương trình; thi cuối khóa riêng. Tỷ lệ đạt theo kết quả đã chốt trong kỳ; chưa có kết quả không hiển thị 0%. |
| 24 | Báo cáo tài chính Giám đốc | Tổng hợp theo định nghĩa chung, không chi tiết hóa đơn/thanh toán cá nhân hoặc xuất file. |
| 25 | Cấp nhân sự/đặt lại mật khẩu | Quản trị viên cấp vai trò cố định; mật khẩu tạm hiện một lần, hạn 24 giờ, bắt đổi. Đặt lại không tự mở khóa, thu hồi mọi phiên. |
| 26 | Khóa/mở/ngừng/khôi phục tài khoản | Hoạt động/bị khóa/ngừng sử dụng; bảo vệ chính mình và quản trị viên hoạt động cuối cùng. Khóa/ngừng thu hồi phiên; khôi phục không phục hồi phiên cũ. Tạm chặn sai mật khẩu là cơ chế riêng. |
| 27 | Tra cứu nhật ký | Quản trị viên lọc/xem chi tiết/nhóm thao tác; không sửa/xóa/xuất/hoàn tác hoặc dùng nhật ký để mở thêm quyền nghiệp vụ. |
| 28 | Phát hành hóa đơn | Cùng thao tác duyệt; một hóa đơn có tiền dương nguyên đồng, giữ học phí lúc phát hành. Hệ thống cấp số/năm/hạn thanh toán. |
| 29 | Tính tổng kết | Trọng số chương trình /100; nháp tính khi xem, chốt mới ghi tổng chính thức; làm tròn cuối đến hai chữ số thập phân, từ 50 đạt. |
| 30 | Ghi nhật ký | Thay đổi nghiệp vụ và nhật ký bắt buộc thành công cùng nhau; sự kiện bảo mật xử lý riêng. Ghi người thực hiện, thời gian, kết quả và thao tác liên quan; không thông tin xác thực bí mật/nội dung yêu cầu đầy đủ. |

**Chèn ngay sau bảng B220, trước “Các lược đồ usecase” trong Word, bảng “Các luồng chưa được liệt kê riêng trong bảng cũ”:**

| Luồng bổ sung | Nội dung tối thiểu | Nguồn |
| --- | --- | --- |
| Hồ sơ cá nhân và menu tài khoản | Học viên sửa họ tên/ngày sinh/số điện thoại/địa chỉ; email bất biến. Mọi vai trò đổi mật khẩu/đăng xuất. | W02/W04 |
| Ca thi đầu vào | Giáo vụ tạo ca đóng; sửa/mở/đóng/hủy/đổi giáo viên theo thời hạn và điều kiện. Giáo viên chỉ xem ca được phân công. | W06 |
| Đặt ca/thi lại | Đặt/đổi/hủy/đặt lại cùng lượt; tối đa hai lượt, lượt hai sau kết quả lượt một và trước xác nhận nhập học; giữ lịch sử. | W07 |
| Chấm đầu vào/ghi vắng/sửa điểm | Giáo viên được phân công xử lý sau ca kết thúc. Người chấm ban đầu còn phân công mới được sửa trước xác nhận, có lý do; tính lại điểm tốt nhất/trình độ cùng nhật ký. Không tự ghi vắng từ điểm trống. | W08 |
| Xác nhận nhập học | Hồ sơ của mình đã xác định trình độ, không lượt chờ; xác nhận và nhật ký cùng thành công, chưa hóa đơn/lớp. | W09 |
| Nháp và vòng gửi lại | Nháp một học viên; gửi/trả/chốt cả lớp; giữ lịch sử từng vòng, bảng đã chốt không mở lại. | W18/W19 |
| Xử lý chung | Kiểm lại phiên/quyền/trạng thái/thời gian và dữ liệu thay đổi. Nếu mất phản hồi, tra cứu kết quả có quyền, không đoán thành công/thất bại hoặc gửi lại thông tin bí mật tự động. | E02 mục 3; E04 |

### V.3. Sơ đồ trường hợp sử dụng — sửa từng hình B224–B248

| Hình | Xóa/sửa/thêm trong hình |
| --- | --- |
| Chung | Giữ bảy tác nhân; không thêm “Hệ thống” thành vai trò. Đăng nhập/đổi mật khẩu/đăng xuất dùng chung, quyền nghiệp vụ vẫn tách vai trò. |
| Tài khoản | Mọi vai trò có đăng nhập/đổi mật khẩu/đăng xuất; học viên tự đăng ký/sửa hồ sơ; Quản trị viên cấp nhân sự/đặt lại/khóa/ngừng/khôi phục. Không nối học viên vào cấp tài khoản nhân sự. |
| Tuyển sinh | Xóa “Hỗ trợ ghi danh”. Thêm học viên đặt ca/xem kết quả/xác nhận ở hình phù hợp. Tuyển sinh duyệt/từ chối/hủy; duyệt bao gồm phát hành hóa đơn. |
| Học phí | Bỏ quan hệ “Xác nhận thanh toán→Tự động tạo hóa đơn”. Hóa đơn tạo khi duyệt; xác nhận thanh toán cập nhật đã thanh toán. Học viên chỉ xem/in/hướng dẫn; không cổng thanh toán. Kế toán sửa thông tin theo phạm vi. |
| Đào tạo | Thêm chương trình/thành phần điểm/khóa sửa cấu hình, ca thi, vòng đời lớp, xếp lớp, buổi thay thế/xác nhận đã tổ chức và trả/chốt cả lớp. Nối đúng tác nhân, bỏ tác nhân đứng rời. |
| Giảng dạy | Thêm nhập/sửa điểm đầu vào, ghi vắng, nháp/gửi/gửi lại bảng điểm. Tổng nháp không là kết quả chốt; Giáo viên không chốt bảng điểm. |
| Báo cáo | Giám đốc chỉ xem ba nhóm tổng hợp; Kế toán có báo cáo tài chính/CSV riêng; không xuất file hoặc phê duyệt của Giám đốc. |

Nếu hình quá nhiều trường hợp sử dụng, chia thành hình nhỏ cùng nhóm; đây là lựa chọn trình bày, không thêm nghiệp vụ.

## VI. Thiết kế dữ liệu

### VI.1. 6.1. Sơ đồ logic của phần mềm — B253

**Thay hình cũ hoàn toàn**, vẽ từ 24 bảng ở phụ lục B và50 FK ở phụ lục C. Xóa ScholarshipGrant/Candidate/Criteria; xóa liên kết Profile→Course trực tiếp; thay Attempt→Session bằng Attempt→Booking→Session; thêm AuthSession, AuditLog, ReplayOperation, InvoiceNumberCounter. Counter không FK Invoice; không giả FK operation_id/entity_id.

Chèn dưới hình:

> Mô hình gồm22 thực thể và2 bảng kỹ thuật ReplayOperation/InvoiceNumberCounter, tổng24 bảng. Hình logic có thể lược bớt actor FK để dễ đọc; dictionary và danh sách ràng buộc tham chiếu liệt kê đầy đủ 50 FK, gồm2 FK ghép. Các liên kết đều NO ACTION, không cascade xóa/cập nhật lịch sử.

### VI.2. 6.2. Chi tiết các bảng dữ liệu — B256–B314

**Thay dictionary20 bảng bằng24 bảng tại phụ lục B**, theo thứ tự T01–T24. Thêm lời dẫn:

> Từ điển dữ liệu dùng int IDENTITY cho PK nghiệp vụ; riêng AuthSession.sid dùng binary(32), InvoiceNumberCounter.issue_year không IDENTITY. Tiền decimal(18,0)>0, điểm/trọng số decimal(5,2), thời gian sự kiện UTC datetime2(3), ngày/giờ lịch VN date/time(0) theo phút. NULL có chủ đích cho trường chưa có sự kiện. rowversion là token đồng thời, không thời gian; có trên20 bảng mutable. Role, StudentClassAssignment, AuditLog và ReplayOperation không rowversion.

| Tiểu mục cũ | Tiểu mục mới | Sửa/thêm ở đâu |
| --- | --- | --- |
| 1.Role;2.Account;3.StudentProfile | 1–3 giữ tên | Thay toàn bộ bảng, không chỉ bổ sung tên cột; Account bỏ locked_until, Student DOB NOT NULL. |
| Chưa có | 4.AuthSession | Chèn sau StudentProfile, trước Course. |
| 4.Course đến7.ScoreComponentConfig | 5–8 | Thay types/RV; Program thêm expected_lesson_count; component thêm kind. |
| 8.Registration;9.EntryTestSession;10.EntryTestAttempt | 9–11 | Thay bundles/security of history; Attempt bỏ session_id/status/registered_at, thêm completed_booking_id. |
| Chưa có | 12.EntryTestBooking | Chèn sau Attempt, trước BandAssignment. |
| 11.BandAssignment đến17.GradeReport | 13–19 | Thay theo phụ lục: composite ownership, Class metadata, assignment actor, Schedule chain, Score link, aggregate RV/operation IDs. |
| 18.Invoice;19.Payment;20.AuditLog | 20–22 | Thay tiền/types/bundles; method TRANSFER; audit allowlist/category/retention. |
| Chưa có | 23.ReplayOperation | Chèn sau AuditLog. |
| Chưa có | 24.InvoiceNumberCounter | Chèn cuối dictionary, trước6.3. |

Sửa cả danh sách thuộc tính ở dòng tiêu đề mỗi bảng cho khớp bảng trường; nếu giữ dòng cũ “Account(...locked_until...)” thì dictionary vẫn tự mâu thuẫn. Kiểm cả type, NULL, default, PK/UQ/FK, không chép “đề xuất” như một giá trị field. Annotation E03 về giới hạn service phải trình bày đúng, không biến thành CHECK chưa tồn tại.

### VI.3. 6.3. Các ràng buộc toàn vẹn — B316–B497

Giữ năm nhóm hiện tại, thay/supplement đúng nhóm sau.

#### VI.3.1. Ràng buộc miền giá trị — thay từ Role đến AuditLog B318–B395

| Bảng | Nội dung miền giá trị thay thế/bổ sung |
| --- | --- |
| Role | Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin; mã đúng canonical, không enum uppercase snake-case cũ. |
| Account | ACTIVE/LOCKED/DISABLED; security_version>0; failed_login_count≥0; phone canonical10ASCII bắt đầu0 nếu có; temp/normal policy riêng. |
| AuthSession — thêm sau Account/Profile | NORMAL/PASSWORD_CHANGE; sid binary32; UTC issued/expires/activity/revoked theo bundle và deadline. |
| Course | base_price>0 nguyên VND; ACTIVE/INACTIVE. |
| Band | min_score0–100, không max_score; ngưỡng0/25/45/65/85. |
| TrainingProgram | DRAFT/ACTIVE/INACTIVE; expected_lesson_count nguyên>0. |
| ScoreComponentConfig | weight>0≤100,≤2lẻ; kind GENERAL/LESSON/FINAL_EXAM. |
| Registration | PENDING/TESTING/BAND_ASSIGNED/CONFIRMED/ADMITTED/REJECTED/CANCELLED; không thêm COMPLETED. is_unfinished là predicate server duy trì. |
| EntryTestSession | capacity1–20; OPEN/CLOSED/CANCELLED, initial CLOSED. |
| EntryTestAttempt | attempt_number1–2; score NULL hoặc0–100≤2lẻ. **Bỏ status của Attempt.** |
| EntryTestBooking — thêm ngay sau Attempt | SCHEDULED/COMPLETED/ABSENT/CANCELLED, initial SCHEDULED; vắng không0. |
| Class | max_size1–15; PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED. |
| Schedule | LESSON/FINAL_EXAM; occurrence_state PLANNED/HELD/NOT_HELD; giờ VN theo phút; current/history rõ. |
| Attendance/Score | present0/1 không default; score_value0–100≤2lẻ không default; thiếu là thiếu row. |
| GradeReport | DRAFT/SUBMITTED/RETURNED/FINALIZED; final_score0–100 chỉ có khi FINALIZED. |
| Invoice/Payment | Tiền>0 nguyên VND. Invoice UNPAID/PAID/CANCELLED; method CASH/TRANSFER, **bỏ BANK_TRANSFER**. |
| AuditLog | result SUCCESS/FAILURE, category MANDATORY/SECURITY; detailsJSON nếu có; action/entity allowlist, secret-free. |
| ReplayOperation/Counter — thêm cuối | SAFE_REFERENCE/SECRET_RESULT_UNAVAILABLE; JSON references, digest binary32; Counter year1–9999/value0–99.999.999. |

#### VI.3.2. Ràng buộc liên thuộc tính cùng một bộ — thay B397–B404 và thêm bundles

- **Account:** bỏ locked_until/status=LOCKED coupling. Failure window/throttle riêng manual lock. must_change_password đi cùng issued/expires bundle24h; normal credential bỏ temp bundle.
- **AuthSession:** issued≤activity≤expires, expires>issued; revoked≥issued nếu có; giới hạn normal/restricted và expiry kiểm theo raw clock tại service.
- **Registration:** approval actor/time cùng có hoặc cùngNULL và giữ sau hủy; rejection/cancellation cần actor/time/prior-state/operation và lý do theo guard. Không viết “mọi status khác ADMITTED phải xóa approval metadata”.
- **Session:** end>start, cùng ngày VN, phút hợp lệ; CANCELLED bundle đủ actor/time/reason/operation.
- **Attempt:** score/graded_by/graded_at/completed_booking_id cùngNULL hoặc cùng có; không dùng Attempt.status kiểm bộ kết quả.
- **Booking:** absence/cancellation bundles đầy đủ theo state, theo E03 T12; không overwrite booking cũ khi đổi ca.
- **Class:** cancellation bundle khi CANCELLED; max_size không thấp hơn số đã xếp.
- **Schedule:** start<end, seconds=0; PLANNED/HELD current=1, NOT_HELD current=0; held/non-held bundles theo state, predecessor không tự trỏ.
- **GradeReport:** submit/review actor/time/operation theo trạng thái; final_score/finalized_at có khi và chỉ khi FINALIZED; RETURNED bắt buộc lý do. Metadata lần gần nhất không thay audit toàn lịch sử.
- **Invoice:** số/amount/link/issued/due bất biến; date calculation VN+7 tại allocator/service; cancellation bundle theo parent.
- **Payment:** TRANSFER bắt buộc reference; correction actor/time/reason cùng có; không sửa confirmed_by/paid_at/tiền/link.
- **Audit/Replay:** JSON/category/retention/mode/expiry bundle đúng dictionary; mandatory history không hết hạn90ngày, security-only/IP theo policy90ngày; replay completion đúng24h theo E04.

#### VI.3.3. Ràng buộc liên bộ cùng quan hệ — sửa B405–B428

Giữ các unique role_code, email, phone-khi-có, Profile.account, Course.code, Band.code/min_score, Class.code, assignment.registration, Attendance(schedule,assignment), Score(assignment,component), report.assignment, Invoice.registration/number và Payment.invoice. Bổ sung/làm rõ:

- Program(Course,Band) unique **mọi trạng thái**, không chỉ program “hiện hành”.
- Component(program,name) unique và filtered unique program khi FINAL_EXAM; ACTIVE phải có đúng một FINAL_EXAM và tổng100 (service).
- Registration(Student,Course) filtered unique khi is_unfinished=1; không permanent unique cặp này.
- Attempt(registration,number) unique; AK(attempt_id,registration_id); completed_booking unique khi có.
- Booking AK(booking_id,attempt_id); tối đa một SCHEDULED và một COMPLETED booking/lượt bằng filtered unique tương ứng.
- BandAssignment unique registration; **bỏ UNIQUE riêng best_attempt_id**, dùng FK ghép cùng Registration.
- Schedule tối đa một current occurrence/(Class,obligation), một current FINAL_EXAM/Class, một successor/predecessor. History cùng obligation được phép nhiều row.
- Replay unique(actor,action,request_id). Payment.transaction_reference **không unique**.
- Overlap/capacity/đúng một current là invariant giao dịch/service, không mô tả unique index như bảo đảm đầy đủ mọi điều kiện.

#### VI.3.4. Ràng buộc tham chiếu — thay B430–B462

**Chép toàn bộ danh sách50 FK ở phụ lục C vào đây.** Bỏ scalar Attempt.session_id; quan hệ session nằm ở Booking. Dùng hai FK ghép cho completed_booking và best_attempt. Mỗi actor mới có FK Account nhưng tồn tại Account không tự chứng minh đúng role.

Sau danh sách, thêm:

> Khóa ngoại dùng NO ACTION; PK IDENTITY không được chỉnh qua nghiệp vụ. Không cascade xóa hoặc cập nhật lịch sử. Bản ghi có phụ thuộc được giữ lại và quản lý bằng trạng thái theo quyền. Chỉ cấu hình nháp chưa dùng, không phụ thuộc mới được xóa nếu chức năng hiện hành cho phép. operation_id/request_id/obligation_key và AuditLog.entity_id không tự tạo FK sang một bảng nghiệp vụ giả định.

Nếu đưa **bảng tầm ảnh hưởng** vào ngay sau danh sáchFK, thêm đề mục “Bảng tầm ảnh hưởng của ràng buộc tham chiếu”; mỗi constraint có bảng cha/con và Thêm/Xóa/Sửa. Dấu+ là thao tác cần kiểm, không tự có nghĩa CASCADE. [Bảng tầm ảnh hưởng đầy đủ](REFERENTIAL_INTEGRITY_IMPACT.md) đã cập nhật ngày 07/10/2026, gồm 50 ràng buộc riêng theo mô hình hiện hành. Chép phần quy ước và các mục 1–50 của file này vào đây; giữ ghi chú về NO ACTION, hai FK ghép và FK tự tham chiếu Schedule.

#### VI.3.5. Ràng buộc liên quan hệ — thay/bổ sung B464–B497

- Lượt tốt nhất có kết quả và Booking COMPLETED đúng lượt, cùng Registration; chọn điểm cao nhất, hòa chọn lượt một. Band là ngưỡng min_score lớn nhất không vượt quá điểm.
- Booking đầu chuyển PENDING→TESTING; điểm đầu hợp lệ chuyển BAND_ASSIGNED. Lượt hai giữ BAND_ASSIGNED. Chặn CONFIRMED khi lượt hai còn đặt/chờ chấm; sau xác nhận không thi lại hoặc sửa kết quả.
- Teacher được phân công nhập điểm/ghi ABSENT sau giờ kết thúc ca. Correction cần người chấm ban đầu vẫn được phân công, lý do và hồ sơ còn trong cửa sổ hợp lệ; không quyền override đặc biệt.
- Capacity đếm Booking SCHEDULED; khóa kiểm sức chứa và xung đột Student tại commit.
- Duyệt CONFIRMED có Band, chuyển ADMITTED, cấp số và tạo Invoice UNPAID, ghi audit trong cùng giao dịch. Hủy hợp lệ xử lý booking tương lai/Invoice UNPAID nguyên tử, giữ lịch sử đã bắt đầu và kết quả.
- Class đầu tiên khóa cấu hình program, kể cả Class về sau CANCELLED; kết quả đầu vào đầu tiên khóa bộ Band. Giá Course mới không đổi Invoice cũ.
- Placement cần OPEN, ADMITTED, PAID, đúng Course/Band, còn chỗ và không trùng lịch Student; assignment + DRAFT report + audit cùng giao dịch; không chuyển/gỡ assignment.
- Lịch hiện hành kiểm Class/Teacher/room/Student; batch tuần thành công toàn bộ hoặc rollback, preview không giữ chỗ. Replacement giữ Class/type/obligation; HELD do Giáo vụ xác nhận.
- Attendance thuộc current LESSON cùng Class, Teacher được phân công, từ start đến trước Class COMPLETED; không default Vắng hoặc tự HELD.
- Score component thuộc Class.program; GENERAL có schedule NULL; LESSON/FINAL_EXAM đúng current Schedule/type/Class. Teacher, cửa sổ và report state phải hợp lệ; lưu Score/comment chạm GradeReport version.
- Gửi/gửi lại/trả/chốt cả lớp hoặc không ai; đúng Class COMPLETED, đủ điểm và trạng thái cohort. Chốt mới lưu tổng/kết quả và cập nhật unfinished nhất quán. FINALIZED không sửa; Student chỉ đọc bản của mình.
- Accountant xác nhận Payment đúng toàn bộ tiền Invoice UNPAID; Payment + PAID + audit cùng giao dịch. Chỉ sửa method/reference có lý do; không refund/hủy/xóa/đổi tiền.
- Báo cáo dùng đúng đơn vị dữ liệu, kỳ ngày, trạng thái hiện tại, distinct Student, HELD và cohort FINALIZED; không dùng phép join/cộng nhóm làm nhân số tiền hoặc số người.
- Mandatory audit rollback cùng mutation; security độc lập. Replay không lặp effects/audit/activity và không cấp thêm quyền. FK/unique/CHECK không thay kiểm role/ownership/state/time tại service.

### VI.4. 6.4. Sơ đồ quan hệ — B501

**Thay toàn bộ hình quan hệ**, không sửa vài nhãn trên hình cũ. Vẽ từ 24 dictionary/50 FK ở phụ lục; nếu quá rộng chia theo miền, giữ một hình tổng quan. Xóa học bổng, Band.max_score, Program/Class.approved_by/at, GradeReport.approved_by/at; Assignment dùng registration_id chứ không student_id. Thể hiện hai FK ghép, self FK Schedule và liên kết result Booking; cardinality/NULL đúng dictionary. Hình logic VI.1 và hình quan hệ VI.4 phải cùng mô hình.

### VI.5. Đề mục mới đề nghị — 6.5. Xử lý giao dịch và đồng thời

**Chèn sau hình 6.4, trước “VII. Thiết kế giao diện”.** Đây là đề mục trình bày mới để phản ánh thiết kế đã có, không nghiệp vụ mới. Nội dung:

| Thao tác | Nhóm thay đổi phải nguyên tử |
| --- | --- |
| Signup | Account + StudentProfile + mandatory audit |
| Đặt/đổi/hủy booking | Booking/history + Registration liên quan + capacity/resource kiểm lại + audit |
| Chấm/correction | Booking/result + best/Band + Registration state liên quan + audit |
| Approval/cancel | Registration + Invoice/counter hoặc booking/Invoice cancellation liên quan + audit |
| Payment | Payment + Invoice.PAID + audit |
| Placement | Assignment + GradeReport.DRAFT + audit |
| Weekly/move/replacement/HELD | Schedule/current chain và dependent facts được phép + audit |
| Nháp/review | Score/comment + report version; hoặc whole-Class report/state/unfinished + audit |
| Account security | Status/hash/security version + session revocation + audit theo action |
| Replay action được chọn | Business effects + mandatory audit + completion cùng TX; retry khớp không chạy lại effects |

> Dùng rowversion phát hiện stale edits, chạm parent version khi child thay đổi; khóa tài nguyên theo protocol E04 để tránh vượt chỗ/xung đột. Reads báo cáo dùng snapshot nhất quán, mutation recheck authority/state/time tại commit. Preview không giữ chỗ. Lỗi mandatory audit rollback toàn thao tác. Khi mất phản hồi, đọc lại trạng thái/lookup có quyền; không tự gửi lại credentials hoặc tạo tác động lần hai.

Nếu báo cáo cần phần SQL objects/quyền, chèn **6.6. Đối tượng SQL và phân quyền** sau 6.5: trình bày 5 trigger/6 view/6 UDF/7 SP và 4 SQL roles theo E03/E04; bảy role sản phẩm không ánh xạ 1–1 thành bảy SQL roles. Không cần chép toàn bộ implementation/test log vào báo cáo thiết kế.

## VII. Thiết kế giao diện và sequence

### VII.1. Danh sách giao diện và lược đồ chuyển đổi — B505–B507

Sửa hình B506: xóa “Hỗ trợ ghi danh trực tiếp”; bỏ một dòng “Xếp học viên vào lớp” bị lặp; nhánh nhân sự/audit cuối đổi “Màn hình giám đốc” thành **Admin**. Director chỉ có báo cáo. Thêm menu tài khoản chung cho đổi mật khẩu/logout, Profile chỉ dành cho Student. Phiên hạn chế dẫn đến đổi mật khẩu/logout, không trang nghiệp vụ. Server xác định vai trò, người dùng không chọn quyền qua dropdown.

Chèn dưới hình:

> Catalog gồm F01–F33, trong đó F15 đã loại, còn 32 mã màn hình trong phạm vi. Hình mô tả điều hướng theo vai trò, không chứng minh mọi màn hình đã triển khai. Signup thành công hướng dẫn đăng nhập; đổi mật khẩu thành công cần đăng nhập mới. Phiên không hợp lệ được xử lý theo chính sách đăng nhập hiện hành.

### VII.2.1. Giao diện đăng nhập — B509–B514

- Sửa mô tả textbox email: kiểm tra hình thức, không tiết lộ email tồn tại trong DB trước xác thực.
- Sửa mô tả button: server kiểm credential/status/throttle, trả thông báo chung; normal session vào trang theo role, restricted session vào đổi mật khẩu.
- Thêm trạng thái đang gửi, chặn bấm trùng, lỗi và yêu cầu thử lại khi bị throttle; không tự gửi lại mật khẩu. Signup chỉ tạo Student; quên mật khẩu qua Admin, không email recovery.
- Có thể giữ hình hai input; bổ sung giải thích phiên và điều hướng dưới hình. UI không trực tiếp kiểm DB.

### VII.2.2. Giao diện đăng ký tài khoản cho sinh viên — B516–B523

Đổi tên thành **“Giao diện học viên tự đăng ký tài khoản”**. Thay câu “được người quản trị cung cấp tài khoản” bằng “Học viên tự tạo tài khoản Student và hồ sơ cá nhân”. Sửa button 7: thành công thông báo và hướng dẫn đăng nhập, không chuyển trang chủ.

DOB và phone bắt buộc; address tùy chọn; không giới hạn tuổi. Password có 15–128 code point Unicode, giữ khoảng trắng, kiểm offline blocklist, không bắt tổ hợp ký tự. Phone chấp nhận và chuẩn hóa dạng 0/+84 theo quy tắc; không chỉ chặn phím không phải số khiến người dùng không nhập được +84. Email unique toàn Account, phone unique khi có. Mô tả lỗi input/server/trùng đồng thời; không tự tạo Registration.

### VII.2.3. Trang chủ Học viên — B525–B530

Thay hình có 8.2/10 và các điểm 9.0/7.8/8.1 bằng dữ liệu minh họa /100 đúng program; không đơn thuần nhân 10 khi chưa chọn dữ liệu hợp lệ. Bỏ button 14 “Tải phiếu điểm”. Chỉ FINALIZED hiển thị kết quả; chưa FINALIZED ghi chưa công bố. Học viên chỉ đọc dữ liệu của mình. Xóa dropdown “Vai trò xem trước” khỏi mô tả chức năng thật; ưu tiên thay hình theo thiết kế hiện hành.

### VII.2.4. “Trang chủ Giáo vụ” — B533–B538

Đây là hình **Tuyển sinh**. Đổi heading/chú thích/ý nghĩa thành “Trang chủ Tuyển sinh”. Giữ list/search/detail nhưng approve/reject/cancel theo guard. Bỏ textbox “Ghi chú nội bộ” nếu hàm ý CRUD field chưa có; lý do từ chối/hủy nằm ở action tương ứng. Không thêm cột note vào Registration chỉ để khớp mockup. Giáo vụ cần phần riêng minh họa ca/lớp/lịch/review theo quyền.

### VII.2.5. Trang chủ Giáo viên — B540–B544

Giữ lớp/lịch/đầu vào/Attendance/gradebook trong phạm vi phân công. Sửa caption/state theo current Schedule và lifecycle đúng nguồn. Tách nút “Lưu nháp học viên” với “Gửi/gửi lại cả lớp”; Teacher không finalize. Không suy ngày quá khứ thành HELD hoặc điểm trống thành ABSENT.

### VII.3. Các giao diện còn thiếu — chèn sau 2.5, trước sequence

Các mục 2.6–2.11 sau là **cách chia đề nghị**, không thêm mã F. Có thể gộp nếu báo cáo chỉ cần màn đại diện, nhưng phải mô tả các vai trò/luồng còn thiếu:

| Mục đề nghị | Nội dung hình và bảng đối tượng cần thêm |
| --- | --- |
| 2.6. Account/Profile chung | Profile Student; đổi mật khẩu trong phiên thường/hạn chế; logout; điều kiện và lỗi. |
| 2.7. Giáo vụ | Ca thi, cấu hình program, Class/placement, preview lịch/dời/thay/HELD, review cả lớp. |
| 2.8. Kế toán | Invoice lookup, xác nhận Payment/correction, báo cáo/CSV đúng scope. |
| 2.9. Director | Ba nhóm tổng hợp không định danh; không export/phê duyệt. |
| 2.10. Admin | Cấp/reset và bàn giao mật khẩu tạm một lần; status với self/last-active guards; audit lookup/detail. |
| 2.11. Student xử lý chính | Booking/thi lại/xác nhận nhập học/in Invoice/kết quả chốt nếu hình home chưa giải thích hành vi. |

Mỗi màn viết **Ý nghĩa → Vai trò/phạm vi → Input/Output → Đối tượng UI và biến cố → Điều kiện cho phép → Thành công/lỗi/xung đột**. Dữ liệu mẫu không có secret thật hoặc field ngoài projection. Hình thiết kế không phải bằng chứng code đã chạy.

### VII.4. Lược đồ tuần tự — mười hình cuối B549–B579

Giữ các hình đã phù hợp, đồng bộ văn bản xung quanh. **Đặt heading “3. Lược đồ tuần tự xử lý” trước hình SQ01 đầu tiên**, vì phần này hiện đứng sau UI mà chưa có đề mục rõ.

| Hình hiện có | Hành động |
| --- | --- |
| SQ01 đăng ký | Giữ Account+Profile+audit nguyên tử, không tự đăng nhập; sửa UI 2.2 cho khớp. |
| SQ03-A đăng nhập | Giữ lỗi chung, server role, phiên thường/hạn chế; mô tả throttle ngoài hình nếu cần. |
| SQ03-B đăng xuất | Giữ thu hồi phiên hiện tại, không logout-all. |
| SQ04 đổi mật khẩu | Giữ kiểm mật khẩu hiện tại, normal/restricted, revoke mọi phiên, audit bắt buộc, đăng nhập mới, không replay credential. |
| SQ07-A đăng ký khóa | Giữ unfinished/Course ACTIVE/PENDING, không sao chép kết quả cũ. |
| SQ07-B hủy hồ sơ | Giữ preview/recheck, Student trước CONFIRMED, hủy booking tương lai, giữ lịch sử và rollback. |
| SQ11 xác nhận | Giữ guard lượt thi chờ, CONFIRMED, chưa Invoice/Class. |
| SQ13-A Student Invoice | Giữ đọc mới có quyền/in trình duyệt; UNPAID không biên lai. |
| SQ13-B Accountant Invoice | Giữ read-only đúng scope; không thêm in/export danh mục Invoice. |
| SQ15-A Course | Giữ code bất biến/giá snapshot/stale/atomic; chưa đủ đại diện cho Program/component/freeze. |

**Chèn hình còn thiếu theo nhóm gần nhất:**

| Sau hình hiện có | Luồng nên thêm | Nguồn |
| --- | --- | --- |
| SQ04 | Admin cấp/reset/status; Student sửa Profile | W01/W03/W04; SQ02/SQ05/SQ06 |
| SQ07-B | AA quản lý ca; Student booking/thi lại; Teacher nhập/sửa điểm, ghi vắng và Band | W06/W07/W08; SQ08–SQ10 |
| SQ11 | Admissions duyệt/từ chối/hủy và phát hành Invoice; Accountant xác nhận/correction Payment | W10/W12; SQ12/SQ14 |
| SQ15-A | Program/component/kích hoạt/freeze; lifecycle Class/placement; batch lịch/dời/thay/HELD | W13–W16; phần còn lại SQ15/SQ16–SQ18 |
| Nhóm lịch/lớp mới | Điểm danh; nháp một Student; gửi/trả/chốt cả lớp | W17–W19; SQ19–SQ22 |
| Cuối phần sequence | Báo cáo/CSV, Director tổng hợp, audit lookup | W20/W21; SQ23–SQ25 |

Danh mục SQ là quy hoạch trình bày, **không bắt buộc phải vẽ đủ 25 hình**. Mỗi hình được chọn cần actor đúng, trạng thái đầu/guard, thành công/effects nguyên tử, từ chối/rollback và xử lý mất phản hồi khi liên quan. Không ghép nhiều vai trò thành một cuộc gọi đồng bộ nếu họ thao tác ở thời điểm khác nhau.

## Phần cuối báo cáo — chỉ thêm nếu cần trình bày triển khai

Nếu đây là **báo cáo thiết kế**, giữ khung I–VII. Nếu cần kết quả thực hiện, chèn **VIII. Trạng thái triển khai và kiểm chứng** sau sequence, trước kết luận/tài liệu tham khảo nếu có. Đoạn đề nghị:

> Thiết kế E01–E05 đã được chấp nhận. Schema hiện hành gồm 24 bảng/230 cột/50 FK NO ACTION/78 indexes/70 CHECK/20 rowversion, với 5 trigger/6 view/6 UDF/7 SP, 4 SQL roles và 102 permissions. Checkpoint được lưu ngày 06/10/2026 đến R02/R03 ghi nhận 1.386 tests PASS và build không warning/error. Đây là bằng chứng các stage đã hoàn tất, không xác nhận toàn bộ chức năng sản phẩm đã triển khai. Host mặc định chưa mount adapters; profile/key/principal thật và vận hành hoàn chỉnh còn chờ kiểm chứng. Hình UI là minh họa thiết kế, không bằng chứng runtime.

Trước nộp, đọc lại CURRENT_STATE để cập nhật checkpoint nếu có. Không ghi tests đã chạy lại ngày 07/10 vì lượt này chỉ biên soạn tài liệu.

## Kiểm tra cuối sau khi sửa Word

- [ ] I–III/V cùng phạm vi 37 NV/32 F/bảy role; không ghi danh/đặt ca thay.
- [ ] Không học bổng, /10, phạt nợ, thu từng phần, refund/chuyển lớp, Director approval/export hoặc biên lai riêng.
- [ ] Dictionary 24 bảng/230 cột; tiêu đề field/bảng/hình khớp; đủ Booking/AuthSession/Replay/Counter.
- [ ] Đủ 50 FK, hai FK ghép, NO ACTION; bỏ các trường/quan hệ dữ liệu cũ đã loại.
- [ ] Cấu hình/freeze, OPEN placement, current Schedule/replacement/HELD, review cả lớp có guard đúng.
- [ ] Điểm thiếu khác 0; điểm danh chưa nhập khác Vắng; final_score chỉ FINALIZED.
- [ ] Doanh thu/phải thu trong kỳ tách công nợ hiện tại; Invoice snapshot/nguyên VND/HD-YYYY-NNNNNNNN/VN+7.
- [ ] UI/signup/sequence không mâu thuẫn; bỏ tải phiếu điểm; sửa nhánh Tuyển sinh/Admin.
- [ ] Phân biệt hình thiết kế với bằng chứng triển khai; không hứa runtime hoàn chỉnh.
- [ ] Cập nhật mục lục, số bảng/số hình và tham chiếu chéo sau khi chèn/xóa.

## Phụ lục A — Bảng thay thế mục III

### A.1. Học viên

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 1 | Đăng ký, đăng nhập và quản lý thông tin cá nhân | Học viên tự tạo tài khoản bằng họ tên, ngày sinh, email, số điện thoại, mật khẩu; địa chỉ tùy chọn. Email và số điện thoại không trùng. Đăng nhập bằng email/mật khẩu khi tài khoản đang hoạt động. Chỉ sửa họ tên, ngày sinh, số điện thoại, địa chỉ của mình; email đăng nhập không đổi. | — | Đăng nhập dùng chung cho 7 vai trò. Người dùng được đổi mật khẩu bằng mật khẩu hiện tại và đăng xuất; quên mật khẩu do Quản trị viên xử lý. Không tự đăng ký tài khoản nhân sự. |
| 2 | Xem khóa học, đăng ký và hủy hồ sơ đăng ký | Chỉ tạo hồ sơ cho khóa học đang hoạt động; mỗi học viên chỉ có một hồ sơ chưa kết thúc cho cùng khóa. Học viên chỉ hủy trước đã xác nhận nhập học. Khóa học của hồ sơ đã tạo không được đổi. | Hồ sơ đăng ký khóa học | Chọn khóa khác cần hủy hợp lệ và tạo hồ sơ mới, thi đầu vào lại; không sao chép điểm/mức trình độ cũ. Hủy giữ lịch sử, đồng thời hủy lịch đặt ca thi tương lai và hóa đơn chưa thanh toán nếu có. Hồ sơ học chỉ kết thúc khi lớp đã hoàn tất và bảng điểm đã chốt, kể cả không đạt; đã hủy/đã từ chối cũng là kết thúc. |
| 3 | Đăng ký, đổi, hủy và xem lịch thi đầu vào | Ca thi phải đang mở, chưa bắt đầu, còn chỗ và không trùng lịch của học viên. Tối đa hai lượt thi mỗi hồ sơ; lượt hai chỉ đặt sau khi có điểm lượt một và trước đã xác nhận nhập học. Đổi/hủy lịch đặt ca thi chỉ khi ca nguồn đang mở và chưa bắt đầu. | Lịch và kết quả thi đầu vào | Vắng hoặc ca bị trung tâm hủy được đặt lại cùng lượt, không tính thành lượt thứ ba. Lịch sử lịch đặt ca thi khác số lượt thi. Ca đóng chặn thao tác lịch đặt ca thi của học viên. |
| 4 | Xem kết quả thi đầu vào và mức trình độ | Chỉ xem kết quả hồ sơ của mình; hiển thị điểm từng lượt đã chấm, điểm tốt nhất và mức trình độ tương ứng. | Lịch và kết quả thi đầu vào | Chọn điểm cao nhất trong các lượt đã hoàn tất; bằng điểm chọn lượt một và vẫn giữ cả hai kết quả. Vắng thi không được tính là điểm 0. |
| 5 | Xác nhận nhập học | Chỉ xác nhận hồ sơ đã xác định trình độ có mức trình độ hợp lệ, không còn lượt hai chờ thi hoặc đã thi chưa chấm. | Hồ sơ đăng ký khóa học | Sau xác nhận, hồ sơ chuyển đã xác nhận nhập học và khóa thi lại/sửa kết quả đầu vào. Bước này chưa duyệt nhập học, chưa tạo hóa đơn và chưa xếp lớp. |
| 6 | Xem, in hóa đơn và hướng dẫn thanh toán | Chỉ xem/in hóa đơn của mình. Thanh toán đủ một lần bên ngoài hệ thống; học viên không tự xác nhận đã thanh toán. | Hóa đơn học phí | In qua trình duyệt từ dữ liệu mới hợp lệ; không chức năng xuất PDF riêng hoặc phát hành biên lai. hóa đơn chưa thanh toán không là xác nhận đã thu tiền. Quá hạn chỉ cảnh báo. |
| 7 | Xem lớp và thời khóa biểu cá nhân | Chỉ xem lớp đã được xếp và lịch thuộc lớp của mình. | Thời khóa biểu | Hiển thị lịch hiện hành khi dữ liệu được cập nhật; học viên không tự xếp, chuyển lớp hoặc sửa lịch. |
| 8 | Xem kết quả học tập | Chỉ xem bảng điểm đã chốt của mình, sau khi lớp đã hoàn tất và Giáo vụ chốt. | Bảng điểm cá nhân | Không xem điểm nháp/đã gửi/đã trả lại. Điểm thang 0–100; tổng đã chốt từ 50 trở lên là đạt. Không chức năng tải phiếu điểm riêng. |

### A.2. Nhân viên tuyển sinh

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 9 | Tra cứu và kiểm tra hồ sơ | Tra cứu theo thông tin được phép, kiểm tra hồ sơ và theo dõi trạng thái tuyển sinh. | Hồ sơ đăng ký khóa học | Không tạo tài khoản, ghi danh, đặt ca hoặc sửa hồ sơ cá nhân thay học viên. Hỗ trợ tại quầy vẫn hướng dẫn học viên tự thực hiện. |
| 12 | Duyệt, từ chối hoặc hủy hồ sơ nhập học | Duyệt/từ chối chỉ khi hồ sơ đăng ký đã xác nhận nhập học có mức trình độ hợp lệ. Từ chối có lý do. Tuyển sinh được hủy đã xác nhận nhập học hoặc đã được duyệt nhập học có hóa đơn chưa thanh toán, kèm lý do; không hủy hồ sơ đã đã thanh toán. | Hồ sơ đăng ký khóa học | Duyệt chuyển đã được duyệt nhập học và tạo đúng một hóa đơn chưa thanh toán trong cùng giao dịch. Hủy đồng thời xử lý lịch đặt ca thi tương lai/hóa đơn chưa thanh toán, giữ lịch sử. Duyệt không đồng nghĩa đã thanh toán hoặc đã xếp lớp. |

### A.3. Giáo vụ

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 13 | Quản lý khóa học, chương trình đào tạo và cấu hình mức trình độ | Quản lý khóa học đang hoạt động/ngừng sử dụng; chương trình đào tạo nháp/đang hoạt động/ngừng sử dụng theo cặp khóa học/mức trình độ duy nhất. Chương trình đang hoạt động phải có số buổi học nguyên dương, tổng trọng số 100 và đúng một thành phần thi cuối khóa. | — | Không qua phê duyệt Ban giám đốc. Chương trình đào tạo khóa khóa học/mức trình độ, số buổi và cấu hình điểm từ lần đầu tạo lớp học, kể cả lớp học về sau bị hủy. Toàn bộ ngưỡng mức trình độ khóa từ kết quả đầu vào đã hoàn tất đầu tiên. Giá mới chỉ áp dụng hóa đơn phát hành sau; ngừng sử dụng không tự hủy hồ sơ/lớp hiện có. |
| 14 | Tổ chức và quản lý ca thi đầu vào | Tạo ca đóng, phòng, giáo viên chấm đang hoạt động và sức chứa 1–20; không trùng phòng/giáo viên với ca thi hoặc lịch lớp. Mở/đóng/hủy ca chỉ trước giờ bắt đầu theo điều kiện hợp lệ. | Lịch và kết quả thi đầu vào | Đến giờ bắt đầu, ca được coi đóng. Hủy ca giữ lịch sử, giải phóng lịch đặt ca thi và để học viên tự đặt lại. Đổi giáo viên chấm cần lý do, trước giờ bắt đầu và trước mọi điểm/vắng; sửa giờ/phòng trước giờ bắt đầu và khi chưa có lịch đặt ca thi. |
| 15 | Theo dõi kết quả thi đầu vào và mức trình độ | Theo dõi ca thi, tiến độ chấm, điểm từng lượt và mức trình độ theo phạm vi Giáo vụ. | Lịch và kết quả thi đầu vào | Không nhập hoặc ghi đè điểm/mức trình độ thay giáo viên/hệ thống. đóng không ngăn việc chấm hợp lệ. |
| 16 | Tạo và quản lý lớp học | Lớp thuộc một chương trình đào tạo, một giáo viên và sĩ số tối đa từ 1–15. Vòng đời: đang chuẩn bị → đang mở → đang học → đã hoàn tất; hủy chỉ đang chuẩn bị/đang mở còn trống và chưa dạy. | Danh sách lớp | Mở lớp cần chương trình đào tạo đang hoạt động, giáo viên hợp lệ và ít nhất một buổi học tương lai. Bắt đầu không trước buổi đầu, có học viên và đủ lịch buổi học. Hoàn tất khi mọi nghĩa vụ buổi học/thi cuối khóa đã đã tổ chức và đủ điểm danh buổi học; không tự chuyển trạng thái theo thời gian. |
| 17 | Phân công hoặc đổi giáo viên phụ trách | Mỗi lớp một giáo viên hợp lệ; không xung đột lịch. Chỉ đổi trước đang học/thực tế giảng dạy và chưa có điểm danh/điểm thành phần. | Danh sách lớp / Thời khóa biểu | Khóa/ngừng tài khoản không tự đổi giáo viên hoặc phân công lại. Không thay giáo viên sau khi đã bắt đầu dạy. |
| 18 | Xếp học viên vào lớp | hồ sơ đăng ký đã được duyệt nhập học, hóa đơn đã thanh toán, đúng khóa học/mức trình độ; lớp học đang mở, còn chỗ và không trùng lịch thi/lớp của học viên. | Danh sách lớp | Một việc xếp học viên vào lớp mỗi hồ sơ; tạo bảng điểm nháp cùng giao dịch, chưa tạo điểm thành phần/điểm danh. hồ sơ đăng ký vẫn đã được duyệt nhập học. Không xếp đang học, chuyển/gỡ lớp hoặc tự mở lớp nếu chưa có lớp phù hợp. |
| 19 | Quản lý lịch học, lịch thi và buổi thay thế | Lập lịch buổi học theo tuần hoặc từng buổi; thi cuối khóa riêng sau buổi buổi học cuối. Không trùng phòng, lớp, giáo viên hoặc lịch học viên. Lịch tuần lưu toàn bộ hoặc không buổi nào. | Thời khóa biểu | Không thêm buổi học vượt số nghĩa vụ chương trình. Dời trước giờ bắt đầu khi chưa điểm danh/điểm liên quan. Buổi không tổ chức ghi không tổ chức, lý do và buổi thay thế tương lai cùng nghĩa vụ khi đủ điều kiện. Giáo vụ xác nhận đã tổ chức sau giờ kết thúc khi lớp đang học; không tự đã tổ chức từ đồng hồ/điểm danh. |
| 20 | Kiểm tra, trả lại và chốt bảng điểm | Chỉ xử lý lớp học đã hoàn tất với toàn bộ bảng điểm đã gửi. Trả lại bắt buộc lý do; trả/chốt cả lớp trong một thao tác thành công toàn bộ hoặc không ai. | Bảng điểm cá nhân | Trả lại thành đã trả lại để giáo viên sửa/gửi lại. Chốt thành đã chốt, lưu tổng điểm và công bố cho học viên; không sửa điểm thay giáo viên, không mở lại bảng đã chốt hoặc thêm tầng phê duyệt. |

### A.4. Giáo viên

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 21 | Xem ca thi được phân công | Chỉ xem ca thi và danh sách thí sinh thuộc ca mình đang được phân công chấm. | Lịch và kết quả thi đầu vào | Không truy cập ca thi ngoài phạm vi phân công. |
| 22 | Nhập, sửa điểm thi đầu vào hoặc ghi vắng | Giáo viên được phân công nhập điểm 0–100 hoặc vắng thi từ khi ca kết thúc. Sửa kết quả đã hoàn tất chỉ bởi người chấm ban đầu còn được phân công, khi hồ sơ đang thi đầu vào/đã xác định trình độ và có lý do. | Lịch và kết quả thi đầu vào | Hồ sơ đã xác nhận nhập học/đã từ chối/đã hủy không được chấm/sửa; ca đã hủy không được chấm. Sửa hợp lệ tính lại best score/mức trình độ và ghi nhật ký cùng giao dịch. Không tự ghi vắng từ điểm trống. |
| 23 | Xem lịch dạy và danh sách lớp | Chỉ lớp mình được phân công và danh sách học viên/lịch thuộc lớp đó. | Danh sách lớp / Thời khóa biểu | Không xem hoặc xử lý lớp ngoài phạm vi phụ trách. |
| 24 | Ghi và cập nhật điểm danh | Ghi Có mặt/Vắng từng học viên cho buổi buổi học từ giờ bắt đầu đến trước lớp học đã hoàn tất; đúng lớp phụ trách, buổi không không tổ chức. | Bảng điểm danh | Một record mỗi học viên/buổi học; chưa ghi khác vắng. Lưu nhóm thao tác một buổi thành công toàn bộ hoặc hoàn tác toàn bộ; được ghi một phần danh sách, nhưng thiếu điểm danh chặn hoàn tất lớp. Điểm danh không tự xác nhận đã tổ chức. |
| 25 | Nhập điểm, lưu nháp và gửi bảng điểm | Điểm thành phần 0–100, tối đa hai chữ số thập phân, theo cấu hình chương trình đào tạo. Lưu nháp từng học viên khi lớp đang học/đã hoàn tất và bảng nháp/đã trả lại; điểm gắn lịch chỉ nhập từ giờ bắt đầu của buổi hợp lệ. Chỉ gửi cả lớp khi đã hoàn tất, đủ điểm và không còn sửa chưa lưu. | Bảng điểm cá nhân | Điểm/nhận xét một học viên lưu cùng nhau; thiếu điểm không là 0, ô trống không xóa điểm. Gửi toàn lớp thành đã gửi hoặc không ai; học viên chưa thấy điểm. điểm tổng kết chính thức chưa lưu trước đã chốt. |
| 26 | Sửa và gửi lại bảng điểm bị trả | Giáo viên hiện phụ trách sửa nháp khi đã trả lại; gửi lại cả lớp sau khi đủ điểm, lớp học đã hoàn tất và không còn sửa chưa lưu. | Bảng điểm cá nhân | Giữ điểm, nhận xét, lý do trả và lịch sử; chu kỳ trả/sửa/gửi lại có thể lặp. Không sửa đã gửi/đã chốt hoặc gửi riêng một học viên. |

### A.5. Kế toán

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 27 | Tra cứu hóa đơn và theo dõi công nợ | Tra cứu hóa đơn chưa thanh toán/đã thanh toán/đã hủy theo phạm vi tài chính; quá hạn khi chưa thanh toán và hạn thanh toán trước ngày hiện tại tại Việt Nam. | Hóa đơn học phí | Hạn thanh toán là ngày phát hành tại Việt Nam +7 ngày lịch. Quá hạn chỉ cảnh báo; không trạng thái OVERDUE riêng, phạt tiền hoặc tự đổi trạng thái học viên. |
| 28 | Đối chiếu, xác nhận thanh toán và sửa thông tin phương thức | Đối chiếu tiền nhận bên ngoài; tạo một thanh toán đúng toàn bộ tiền hóa đơn chưa thanh toán và chuyển đã thanh toán cùng giao dịch. Phương thức tiền mặt/chuyển khoản; chuyển khoản bắt buộc mã tham chiếu. | Hóa đơn học phí / Thông tin xác nhận thanh toán | Không trả góp, nhiều lần thu, gateway hoặc biên lai riêng. Sau xác nhận chỉ sửa phương thức thanh toán/mã tham chiếu với lý do/nhật ký; tiền, liên kết hóa đơn và thời điểm xác nhận bất biến. Không hủy/xóa thanh toán hoặc hoàn tiền. |
| 29 | Xem và xuất báo cáo tài chính | Doanh thu theo thanh toán được xác nhận trong kỳ; tổng phải thu theo hóa đơn không đã hủy phát hành trong kỳ; công nợ hiện tại là tổng hóa đơn chưa thanh toán, quá hạn là tập con. | Báo cáo tổng hợp học phí | Không lấy phải thu trong kỳ trừ doanh thu trong kỳ làm công nợ. Chỉ Kế toán xuất CSV theo toàn bộ kết quả/cột/bộ lọc được phép; tối đa 10.000 dòng hoặc 5 MB, vượt thì từ chối toàn file. Không XLSX, chi phí/lợi nhuận hoặc số dư lịch sử tại ngày quá khứ. |

### A.6. Ban giám đốc

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 30 | Xem báo cáo tuyển sinh | Chỉ xem tổng hợp theo bộ lọc đã chốt; đếm học viên phân biệt trong từng khóa và toàn báo cáo, không cộng nhóm để suy tổng. | Báo cáo tuyển sinh | Không định danh học viên, xem hồ sơ cá nhân, duyệt hồ sơ hoặc xuất file. Trạng thái là hiện tại, không tái dựng trạng thái quá khứ. |
| 31 | Xem báo cáo đào tạo | Xem tổng hợp lớp, tiến độ nghĩa vụ buổi học đã đã tổ chức so với số buổi chương trình; thi cuối khóa theo dõi riêng. Tỷ lệ đạt theo kết quả đã chốt trong kỳ ngày chốt. | Báo cáo đào tạo | Không lấy thời gian trôi qua/điểm danh thay đã tổ chức. Chưa chốt không tính đạt/rớt; mẫu số rỗng hiển thị Chưa có dữ liệu. Không dữ liệu cá nhân, chỉnh sửa hoặc xuất file. |
| 32 | Xem báo cáo tài chính | Chỉ xem tổng hợp doanh thu, phải thu trong kỳ, công nợ/quá hạn hiện tại theo định nghĩa chung với Kế toán. | Báo cáo tổng hợp học phí | Không xem hóa đơn/thanh toán cá nhân, xác nhận thanh toán hoặc xuất file; không chức năng phê duyệt tài chính. |

### A.7. Quản trị viên

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 33 | Cấp tài khoản nhân sự và đặt lại mật khẩu | Cấp một vai trò nhân sự có sẵn; email đăng nhập bất biến. Cấp/đặt lại tạo mật khẩu tạm ngẫu nhiên 32 ký tự, hạn 24 giờ, chỉ hiển thị một lần; bắt đổi trước truy cập nghiệp vụ. | — | Quản trị viên có thể đặt lại mật khẩu tài khoản theo quyền; không tự mở khóa/kích hoạt lại. Reset thu hồi mọi phiên. Không gửi email recovery, lưu/xuất mật khẩu hoặc xem lại thông tin bí mật; phiên bắt đổi chỉ cho đổi mật khẩu/đăng xuất, tối đa 10 phút và không vượt hạn thông tin bí mật. |
| 34 | Khóa, mở, ngừng và khôi phục tài khoản | Quản lý đang hoạt động/bị khóa/ngừng sử dụng; không khóa/ngừng chính mình hoặc Quản trị viên đang hoạt động cuối cùng. Không đổi dữ liệu nghiệp vụ hay phân công khi đổi trạng thái tài khoản. | — | Khóa/ngừng thu hồi mọi phiên; mở/khôi phục cùng tài khoản giữ vai trò/lịch sử nhưng cần đăng nhập mới. Throttle sai mật khẩu không là bị khóa. Không xóa vật lý tài khoản có lịch sử nghiệp vụ. |
| 35 | Tra cứu nhật ký hoạt động | Chỉ Quản trị viên xem/tìm/lọc nhật ký, chi tiết sự kiện và nhóm thao tác liên quan theo phạm vi được phép. | Nhật ký hệ thống | Không sửa/xóa/undo/xuất file log, không từ log mở quyền xem nghiệp vụ khác. Không hiển thị mật khẩu, hash, mã bảo mật hoặc toàn bộ nội dung yêu cầu nhạy cảm. |

### A.8. Chức năng tự động của hệ thống

Nhóm này mô tả xử lý tự động, không phải vai trò tài khoản thứ tám.

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 36 | Tự động xác định mức trình độ | Chọn điểm cao nhất từ tối đa hai lượt đã hoàn tất của cùng hồ sơ; lấy mức trình độ có ngưỡng điểm tối thiểu lớn nhất nhưng không vượt quá điểm. | Lịch và kết quả thi đầu vào | Ngưỡng đã chốt: 0, 25, 45, 65, 85; bằng điểm chọn lượt một. Tính lại khi sửa thông tin hợp lệ, không tự chấm bài thi hoặc sửa mức trình độ sau đã xác nhận nhập học. Toàn bộ ngưỡng khóa từ kết quả đã hoàn tất đầu tiên. |
| 37 | Tự động phát hành hóa đơn | Khi Tuyển sinh duyệt hồ sơ thành đã được duyệt nhập học, tạo đúng một hóa đơn chưa thanh toán trong cùng giao dịch, lấy học phí khóa học tại thời điểm phát hành. | Hóa đơn học phí | Tiền giá trị được giữ nguyên tại thời điểm phát hành dương, VND nguyên đồng; không đổi khi khóa học đổi giá. Hạn ngày phát hành VN +7 ngày lịch; duyệt lặp không tạo hóa đơn thứ hai. |
| 38 | Tự động tính điểm tổng kết và kết quả đạt | Tổng = Σ(điểm thành phần × trọng số /100) theo chương trình đào tạo đã khóa cấu hình. Chỉ khi đã chốt mới lưu điểm tổng kết chính thức; làm tròn theo quy tắc từ 5 trở lên một lần đến hai chữ số thập phân. | Bảng điểm cá nhân | Trước chốt chỉ tính tổng nháp từ điểm thành phần khi đọc, chưa ghi nhận điểm tổng kết chính thức. Tổng đã chốt ≥50 là đạt; không thêm điều kiện tỷ lệ chuyên cần/điểm tối thiểu từng thành phần. Thiếu điểm không được coi là 0. |
| 39 | Ghi nhật ký nghiệp vụ và sự kiện bảo mật | Mutation nghiệp vụ/tài khoản thuộc danh mục bắt buộc ghi tác nhân, hành động, đối tượng, thời điểm, old/new/lý do theo loại sự kiện và mã thao tác. Audit bắt buộc cùng giao dịch; lỗi thì hoàn tác toàn bộ thao tác thay đổi. | Nhật ký hệ thống | Security-only như đăng nhập/đăng xuất/tạm chặn đăng nhập/denied-access dùng best-effort. Giữ lịch sử nghiệp vụ/tài khoản; bảo mật log/IP giữ 90 ngày theo chính sách vận hành. Không log mọi lần đọc, bí mật hoặc toàn bộ request/phản hồi. |

## Phụ lục B — Dictionary thay thế mục VI.2

Quy ước: N=NOT NULL, Y=NULL có chủ đích; ID=IDENTITY(1,1); RV=rowversion. Cột “Khóa/default” giữ thông tin dictionary E03; các guard service nêu tại VI.3 không tự biến thành FK/CHECK. Bảng vật lý và giới hạn input Unicode phải phân biệt.

### B.T01 — Role

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| role_id | int | N | PK, ID | Khóa nội bộ. |
| role_code | varchar(32) | N | UQ_Role_Code | Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin. |
| role_name | nvarchar(200) | N | — | Nhãn Việt ngữ, giới hạn kỹ thuật đề xuất 100 code point. |

### B.T02 — Account

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| account_id | int | N | PK, ID | Khóa Account. |
| role_id | int | N | FK Role | Role cố định của tài khoản. |
| email | varchar(254) | N | UQ_Account_Email | ASCII chuẩn hóa, bất biến; unique toàn Account kể cả blocked. |
| phone | varchar(10) | Y | UQ_Account_Phone khi khác NULL | Chuẩn 0xxxxxxxxx; service bắt buộc đối với Student. |
| password_hash | varchar(512) | N | — | Chỉ PasswordHasher output; dung lượng kỹ thuật đề xuất, xác minh khi khóa runtime. |
| full_name | nvarchar(200) | N | — | 1–100 code point. |
| status | varchar(16) | N | default ACTIVE | ACTIVE/LOCKED/DISABLED. |
| created_at | datetime2(3) | N | server UTC | Thời điểm tạo. |
| security_version | bigint | N | default 1 | Tăng khi phải revoke mọi session; không giảm. |
| failed_login_count | int | N | default 0 | Bộ đếm độc lập manual lock. |
| failure_window_started_at | datetime2(3) | Y | — | Cửa sổ sai credential. |
| login_throttled_until | datetime2(3) | Y | — | Hạn throttle login mới; không tự unlock Account. |
| must_change_password | bit | N | server | Student password thường false; cấp/reset tạm true. |
| temp_issued_at | datetime2(3) | Y | — | Bắt buộc khi must_change_password=true. |
| temp_expires_at | datetime2(3) | Y | — | issued+24h; cả hai NULL khi credential thường. |
| row_version | rowversion | N | RV | Admin/profile/security update recheck ở service. |

### B.T03 — StudentProfile

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| student_id | int | N | PK, ID | Mã học viên hiển thị là ID này. |
| account_id | int | N | FK Account, UQ_StudentProfile_Account | Chỉ Account Student. |
| date_of_birth | date | N | — | Không sau ngày VN hiện tại, không giới hạn tuổi. |
| address | nvarchar(510) | Y | — | ≤255 code point, optional-empty→NULL. |
| row_version | rowversion | N | RV | Sửa DOB/address cùng Account name/phone theo W04. |

### B.T04 — AuthSession

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| sid | binary(32) | N | PK | Random 256 bit, đề xuất vật lý của khóa phiên ADR-092. |
| account_id | int | N | FK Account | Chủ session. |
| issued_at | datetime2(3) | N | server UTC | Login time. |
| expires_at | datetime2(3) | N | server | Normal issued+2h; restricted min(issued+10min,temp expiry). |
| last_activity_at | datetime2(3) | N | initial issued_at | Chỉ request nghiệp vụ do user thao tác, cập nhật đơn điệu. |
| revoked_at | datetime2(3) | Y | — | Logout hoặc revocation. |
| security_version | bigint | N | snapshot Account | Phải bằng version hiện tại khi xác thực. |
| session_scope | varchar(24) | N | server | NORMAL/PASSWORD_CHANGE; là phạm vi kỹ thuật, không role mới. |
| row_version | rowversion | N | RV | Không đưa vào form nghiệp vụ; activity update không overwrite revoke. |

### B.T05 — Course

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| course_id | int | N | PK, ID | Khóa nội bộ. |
| course_code | varchar(30) | N | UQ_Course_Code | Mã uppercase 1–30; bất biến/không tái dùng. |
| course_name | nvarchar(300) | N | — | 1–150 code point; không global unique. |
| base_price | decimal(18,0) | N | — | Dương, VND; không default 0. |
| status | varchar(16) | N | server | ACTIVE/INACTIVE; explicit khi tạo. |
| row_version | rowversion | N | RV | Sửa giá/name/status theo W13. |

### B.T06 — Band

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| band_id | int | N | PK, ID | Khóa nội bộ. |
| band_code | varchar(30) | N | UQ_Band_Code | Mã kỹ thuật đề xuất ≤30 ASCII; UI dùng nhãn Band tương ứng. |
| min_score | decimal(5,2) | N | UQ_Band_MinScore | Ngưỡng tối thiểu 0–100; không max_score. |
| row_version | rowversion | N | RV | Freeze toàn bộ set sau completed entry result đầu tiên. |

### B.T07 — TrainingProgram

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| program_id | int | N | PK, ID | Khóa nội bộ. |
| course_id | int | N | FK Course | Khóa liên kết; frozen sau first Class. |
| band_id | int | N | FK Band | Band liên kết; frozen sau first Class. |
| program_name | nvarchar(300) | N | — | 1–150 code point. |
| status | varchar(16) | N | default DRAFT | DRAFT/ACTIVE/INACTIVE. |
| expected_lesson_count | int | N | — | >0, chỉ LESSON, frozen. |
| row_version | rowversion | N | RV | Config/status transaction. |

### B.T08 — ScoreComponentConfig

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| component_id | int | N | PK, ID | Khóa component. |
| program_id | int | N | FK TrainingProgram | Parent configuration. |
| component_name | nvarchar(200) | N | UQ trong program, CI_AS_SC | 1–100 code point; không phân biệt case, có phân biệt dấu. |
| weight | decimal(5,2) | N | — | >0 và ≤100. |
| kind | varchar(16) | N | — | GENERAL/LESSON/FINAL_EXAM. |
| row_version | rowversion | N | RV | Config edit trước freeze. |

### B.T09 — Registration

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| registration_id | int | N | PK, ID | Một lượt đăng ký/học, giữ lịch sử. |
| student_id | int | N | FK StudentProfile | Chủ hồ sơ. |
| course_id | int | N | FK Course | Bất biến từ lúc tạo. |
| status | varchar(16) | N | default PENDING, đường chuyển duyệt ADR-133 | Enum đã chốt, không COMPLETED. |
| is_unfinished | bit | N | initial 1, server-owned | Predicate ADR-022/090; filtered UQ. |
| created_at | datetime2(3) | N | server UTC | Khởi tạo. |
| admission_confirmed_at | datetime2(3) | Y | — | Khi Student-own CONFIRMED; actor trong audit/chủ sở hữu bất biến. |
| admission_approved_by | int | Y | FK Account | Admissions duyệt. |
| admission_approved_at | datetime2(3) | Y | — | Khi ADMITTED, giữ sau hủy. |
| rejected_by | int | Y | FK Account | Admissions; bundle bắt buộc khi REJECTED. |
| rejected_at | datetime2(3) | Y | — | UTC. |
| rejection_reason | nvarchar(2000) | Y | — | 1–1000 code point khi REJECTED. |
| rejected_from_status | varchar(16) | Y | — | CONFIRMED. |
| rejection_operation_id | uniqueidentifier | Y | — | Cùng audit rejection. |
| cancelled_by | int | Y | FK Account | Student-own hoặc Admissions theo guard. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Bắt buộc Admissions cancellation; không bắt Student nhập lý do mới. |
| cancelled_from_status | varchar(16) | Y | — | State trước khi hủy. |
| cancellation_operation_id | uniqueidentifier | Y | — | Cùng dependent Invoice/booking/audit. |
| row_version | rowversion | N | RV | Aggregate state/config result races. |

### B.T10 — EntryTestSession

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| session_id | int | N | PK, ID | Khóa ca. |
| start_at | datetime2(3) | N | — | UTC từ ngày/giờ VN theo phút. |
| end_at | datetime2(3) | N | — | Cùng ngày VN/start<end. |
| room | varchar(30) | N | — | Mã phòng chuẩn hóa. |
| capacity | int | N | — | 1–20. |
| grader_id | int | N | FK Account | Teacher hiện phân công; Account blocked không tự mất assignment. |
| status | varchar(16) | N | default CLOSED | OPEN/CLOSED/CANCELLED. |
| cancelled_by | int | Y | FK Account | AcademicAffairs khi hủy. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Mandatory 1–1000. |
| cancellation_operation_id | uniqueidentifier | Y | — | Parent cancellation/audit. |
| row_version | rowversion | N | RV | Open/close/cancel/edit. |

### B.T11 — EntryTestAttempt

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| attempt_id | int | N | PK, ID | Logical attempt. |
| registration_id | int | N | FK Registration | Parent hồ sơ. |
| attempt_number | int | N | UQ với registration_id | 1 hoặc 2. |
| score | decimal(5,2) | Y | — | Thi chưa chấm/vắng: NULL; 0 khi thi được nhập 0. |
| graded_by | int | Y | FK Account | Original score Teacher; không đổi khi correction. |
| graded_at | datetime2(3) | Y | — | Initial grading time; correction giữ trong audit. |
| completed_booking_id | int | Y | FK composite EntryTestBooking | Booking COMPLETED của đúng attempt. |
| row_version | rowversion | N | RV | Initial grade/correction. |

### B.T12 — EntryTestBooking

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| booking_id | int | N | PK, ID | Mỗi appointment riêng. |
| attempt_id | int | N | FK EntryTestAttempt | Logical attempt, immutable. |
| session_id | int | N | FK EntryTestSession | Ca cụ thể, immutable; đổi ca tạo row mới. |
| status | varchar(16) | N | default SCHEDULED | SCHEDULED/COMPLETED/ABSENT/CANCELLED. |
| booked_at | datetime2(3) | N | server UTC | Lúc đặt. |
| absent_by | int | Y | FK Account | Assigned Teacher xác nhận. |
| absent_at | datetime2(3) | Y | — | Sau end, không suy từ thiếu điểm. |
| absent_operation_id | uniqueidentifier | Y | — | Nhóm audit. |
| cancelled_by | int | Y | FK Account | Actor hủy/đổi hoặc parent cancellation. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Parent reason khi có; Student booking cancel không bắt thêm lý do. |
| cancelled_from_status | varchar(16) | Y | — | SCHEDULED cho đường hợp lệ hiện tại. |
| cancellation_operation_id | uniqueidentifier | Y | — | Parent Registration/session hoặc booking-change operation. |
| row_version | rowversion | N | RV | State mutation. |

### B.T13 — BandAssignment

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| band_assignment_id | int | N | PK, ID | Current best/Band projection được lưu. |
| registration_id | int | N | FK Registration, UQ | Một assignment Band/hồ sơ. |
| best_attempt_id | int | N | FK composite EntryTestAttempt | `(best_attempt_id,registration_id)` cùng hồ sơ. |
| band_id | int | N | FK Band | Kết quả ngưỡng lớn nhất≤best score. |
| assigned_at | datetime2(3) | N | server UTC | Lần recalculation gần nhất; history trong AuditLog. |
| row_version | rowversion | N | RV | Recalculate/confirmation race. |

### B.T14 — Class

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| class_id | int | N | PK, ID | Khóa lớp. |
| program_id | int | N | FK TrainingProgram | Bất biến; first Class khóa cấu hình program. |
| teacher_id | int | N | FK Account | Teacher hiện tại; giữ assignment khi Account blocked. |
| class_code | varchar(30) | N | UQ_Class_Code | ASCII uppercase, bất biến kể cả CANCELLED. |
| status | varchar(16) | N | default PLANNED | PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED. |
| max_size | int | N | — | 1–15; không thấp hơn placed count. |
| created_at | datetime2(3) | N | server UTC | Danh sách/trace tạo lớp. |
| cancelled_by | int | Y | FK Account | AcademicAffairs. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Mandatory 1–1000. |
| cancellation_operation_id | uniqueidentifier | Y | — | Audit class cancellation. |
| row_version | rowversion | N | RV | State/capacity/Teacher mutation. |

### B.T15 — StudentClassAssignment

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| class_assignment_id | int | N | PK, ID | Một lượt học trong lớp. |
| class_id | int | N | FK Class | Lớp được xếp, bất biến. |
| registration_id | int | N | FK Registration, UQ | Tối đa một lớp/hồ sơ. |
| assigned_by | int | N | FK Account | AcademicAffairs, đề xuất trường provenance. |
| assigned_at | datetime2(3) | N | server UTC | Khi xếp. |

### B.T16 — Schedule

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| schedule_id | int | N | PK, ID | Một occurrence cụ thể, kể cả history. |
| class_id | int | N | FK Class | Parent lớp. |
| session_type | varchar(16) | N | — | LESSON/FINAL_EXAM. |
| obligation_key | uniqueidentifier | N | server | Root và mọi replacement cùng nghĩa vụ/cùng Class/type. |
| replaces_schedule_id | int | Y | FK Schedule, filtered UQ khi có | NULL ở root; một predecessor có tối đa một successor. |
| is_current | bit | N | initial 1, server-owned | Projection đã duyệt ADR-131/134; filtered unique/current query. |
| session_date | date | N | — | Ngày VN. |
| start_time | time(0) | N | — | Giây=0, start<end. |
| end_time | time(0) | N | — | Cùng session_date. |
| room | varchar(30) | N | — | Mã phòng chuẩn hóa. |
| occurrence_state | varchar(16) | N | default PLANNED | PLANNED/HELD/NOT_HELD. |
| held_by | int | Y | FK Account | AcademicAffairs, khi HELD. |
| held_at | datetime2(3) | Y | — | Sau scheduled end, Class ONGOING. |
| not_held_by | int | Y | FK Account | AcademicAffairs khi NOT_HELD. |
| not_held_at | datetime2(3) | Y | — | Tại/sau scheduled start. |
| not_held_reason | nvarchar(2000) | Y | — | Mandatory 1–1000. |
| replacement_operation_id | uniqueidentifier | Y | — | Cùng old/new replacement/audit. |
| row_version | rowversion | N | RV | Move/replacement/HELD. |

### B.T17 — Attendance

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| attendance_id | int | N | PK, ID | Record đã chủ động ghi. |
| schedule_id | int | N | FK Schedule | Current LESSON của cùng Class. |
| class_assignment_id | int | N | FK StudentClassAssignment | Student của lớp. |
| present | bit | N | Không default | 1=Có mặt, 0=Vắng; chưa nhập=không row. |
| entered_by | int | N | FK Account | Teacher hiện assigned, provenance duyệt ADR-135. |
| entered_at | datetime2(3) | N | server UTC | Lần ghi gần nhất, provenance duyệt ADR-135. |
| row_version | rowversion | N | RV | Batch delta theo một LESSON. |

### B.T18 — Score

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| score_id | int | N | PK, ID | Một Score/học viên/component. |
| class_assignment_id | int | N | FK StudentClassAssignment | Lượt học. |
| component_id | int | N | FK ScoreComponentConfig | Component thuộc Class.program. |
| schedule_id | int | Y | FK Schedule | NULL chỉ GENERAL; LESSON/FINAL_EXAM bắt buộc. |
| score_value | decimal(5,2) | N | Không default | 0–100 đã validate≤2 decimals. |
| entered_by | int | N | FK Account | Current Teacher. |
| entered_at | datetime2(3) | N | server UTC | Lần lưu gần nhất; history đã bắt buộc trong audit. |
| row_version | rowversion | N | RV | Per-Student draft all-or-none. |

### B.T19 — GradeReport

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| report_id | int | N | PK, ID | Bảng điểm/lượt học. |
| class_assignment_id | int | N | FK StudentClassAssignment, UQ | Một report/assignment. |
| final_score | decimal(5,2) | Y | — | Chỉ có khi FINALIZED; draft/submitted total là tính toán. |
| status | varchar(16) | N | default DRAFT | DRAFT/SUBMITTED/RETURNED/FINALIZED. |
| teacher_comment | nvarchar(2000) | Y | — | ≤1000 code point, optional-empty→NULL. |
| submitted_by | int | Y | FK Account | Teacher gửi/gửi lại gần nhất. |
| submitted_at | datetime2(3) | Y | — | Lần gửi gần nhất. |
| submitted_operation_id | uniqueidentifier | Y | — | Cùng mọi report của Class trong lần gửi. |
| reviewed_by | int | Y | FK Account | AcademicAffairs trả/chốt gần nhất. |
| reviewed_at | datetime2(3) | Y | — | Lần review gần nhất. |
| review_operation_id | uniqueidentifier | Y | — | Cùng mọi report khi return/finalize. |
| returned_reason | nvarchar(2000) | Y | — | Mandatory khi RETURNED; latest reason có thể giữ sau resubmit. |
| finalized_at | datetime2(3) | Y | — | Khi FINALIZED, bằng thời điểm review chốt. |
| row_version | rowversion | N | RV | Draft/comment/whole-Class workflow. |

### B.T20 — Invoice

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| invoice_id | int | N | PK, ID | Khóa Invoice. |
| invoice_number | varchar(16) | N | UQ_Invoice_Number | HD-YYYY-NNNNNNNN, server cấp, không tái dùng. |
| registration_id | int | N | FK Registration, UQ | Chỉ tạo cùng admission approval. |
| amount | decimal(18,0) | N | — | Course price snapshot dương bất biến. |
| issued_at | datetime2(3) | N | server UTC | Issue time, không backdate. |
| due_date | date | N | server | Ngày VN issued+7 calendar days. |
| status | varchar(16) | N | default UNPAID | UNPAID/PAID/CANCELLED, không OVERDUE. |
| cancelled_by | int | Y | FK Account | Actor cancellation parent Registration. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Parent reason được yêu cầu tại workflow. |
| cancelled_from_status | varchar(16) | Y | — | UNPAID. |
| cancellation_operation_id | uniqueidentifier | Y | — | Chung Registration/booking/audit. |
| row_version | rowversion | N | RV | Payment vs cancellation. |

### B.T21 — Payment

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| payment_id | int | N | PK, ID | Payment đã xác nhận. |
| invoice_id | int | N | FK Invoice, UQ | Tối đa một Payment/Invoice. |
| amount_paid | decimal(18,0) | N | — | Bằng Invoice.amount; bất biến. |
| method | varchar(16) | N | — | CASH/TRANSFER. |
| transaction_reference | nvarchar(200) | Y | — | ≤100 code point; bắt buộc TRANSFER, không unique. |
| confirmed_by | int | N | FK Account | Accountant xác nhận. |
| paid_at | datetime2(3) | N | server UTC | Confirmation time bất biến, không ngày nhận tiền riêng. |
| last_corrected_by | int | Y | FK Account | Accountant sửa method/reference gần nhất. |
| last_corrected_at | datetime2(3) | Y | — | UTC correction. |
| last_correction_reason | nvarchar(2000) | Y | — | Mandatory 1–1000 khi correction. |
| row_version | rowversion | N | RV | Metadata correction conflict. |

### B.T22 — AuditLog

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| audit_id | int | N | PK, ID | Event; không aggregate whole-Class thành một event mất chi tiết. |
| actor_account_id | int | Y | FK Account | NULL chỉ khi chưa xác thực. |
| action | varchar(80) | N | — | Mã action allowlist, giới hạn kỹ thuật đề xuất. |
| entity_type | varchar(64) | N | — | Loại đối tượng allowlist. |
| entity_id | varchar(100) | Y | — | int/UUID/session reference khi hợp lệ; NULL nếu chưa có đối tượng xác minh. |
| result | varchar(16) | N | — | SUCCESS/FAILURE đề xuất; denial/throttle phân biệt bằng action/details. |
| details | nvarchar(max) | Y | CK ISJSON khi có | Structured allowlist old/new/reason/prior-state; không raw payload. |
| occurred_at | datetime2(3) | N | server UTC | Event time. |
| ip_address | varchar(45) | Y | — | IP thực sự cần cho security event, proxy tin cậy theo E04. |
| operation_id | uniqueidentifier | N | server | Nhóm parent/dependent hoặc một security request. |
| audit_category | varchar(16) | N | — | MANDATORY/SECURITY, kỹ thuật retention. |
| security_expires_at | datetime2(3) | Y | — | SECURITY→occurred+90d; MANDATORY→NULL. |
| ip_expires_at | datetime2(3) | Y | — | Có IP→occurred+90d; không IP→NULL. |

### B.T23 — ReplayOperation (bảng kỹ thuật)

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| replay_id | int | N | PK, ID | Khóa lưu kết quả. |
| actor_account_id | int | N | FK Account | Actor đã xác thực; không dùng chung theo browser. |
| action_code | varchar(80) | N | UQ với actor/request_id | Canonical action, không URL tùy ý. |
| request_id | uniqueidentifier | N | UQ với actor/action | Khóa request do client tạo cho một intent. |
| input_digest | binary(32) | N | — | SHA-256 canonical validated nonsecret business input/target/expected versions. |
| operation_id | uniqueidentifier | N | server | Chung effects và audit. |
| result_mode | varchar(32) | N | — | SAFE_REFERENCE/SECRET_RESULT_UNAVAILABLE. |
| result_reference | nvarchar(max) | N | CK ISJSON | IDs, outcome code/operation; không response có PII hay secret. |
| completed_at | datetime2(3) | N | server UTC | Chỉ persist cùng transaction thành công. |
| expires_at | datetime2(3) | N | server | Ít nhất completed+24h; đề xuất đúng 24h cho maintenance. |

### B.T24 — InvoiceNumberCounter (bảng kỹ thuật)

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| issue_year | smallint | N | PK | Năm ngày phát hành VN, 1–9999. |
| last_value | int | N | initial 0 | Counter 0–99.999.999; number mới tăng một. |
| row_version | rowversion | N | RV | Internal; allocator dùng row/range lock, không form edit. |

## Phụ lục C — Danh sách50 FK thay thế VI.3.4

Mỗi mũi tên trong **các dòng dữ liệu** là một constraint. Hai FK ghép được tính mỗi cái một; các actor FK không tựchứngminhrole. Tất cả NO ACTION.

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
