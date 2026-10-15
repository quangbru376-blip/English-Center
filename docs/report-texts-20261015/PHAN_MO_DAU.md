# PHẦN MỞ ĐẦU

## 1. Lý do chọn đề tài

Hoạt động của một trung tâm tiếng Anh bao gồm nhiều nghiệp vụ có quan hệ chặt chẽ với nhau, từ tiếp nhận đăng ký, tổ chức kiểm tra đầu vào, xác định trình độ, xét duyệt nhập học đến thu học phí, xếp lớp và theo dõi kết quả học tập. Trong quá trình đó, thông tin cần được trao đổi giữa học viên, bộ phận tuyển sinh, giáo vụ, giáo viên, kế toán và ban giám đốc. Việc quản lý nhất quán dữ liệu giữa các bộ phận là cơ sở để tổ chức hoạt động đào tạo và giải quyết các yêu cầu của học viên.

Khi dữ liệu được lưu trên nhiều tệp hoặc quản lý riêng tại từng bộ phận, việc đối chiếu hồ sơ, lịch học, học phí và điểm số có thể gặp khó khăn. Một thay đổi về lịch thi, trạng thái thanh toán hoặc kết quả học tập cần được phản ánh đúng ở các bước liên quan. Nếu thiếu cơ chế quản lý thống nhất, quá trình xử lý dễ phát sinh thông tin trùng lặp, cập nhật thiếu hoặc nhầm lẫn khi tổng hợp báo cáo.

Xuất phát từ yêu cầu trên, nhóm lựa chọn đề tài **“Xây dựng hệ thống quản lý trung tâm tiếng Anh”**. Đề tài hướng đến xây dựng một ứng dụng web hỗ trợ các nghiệp vụ chính trên cùng một hệ thống, đồng thời phân định rõ trách nhiệm và quyền truy cập của từng nhóm người dùng. Đây cũng là cơ hội để nhóm vận dụng kiến thức của học phần Công nghệ phần mềm vào việc phân tích yêu cầu, thiết kế, xây dựng và kiểm thử một sản phẩm có quy trình nghiệp vụ liên kết từ đầu đến cuối.

## 2. Mục tiêu

### 2.1. Mục tiêu tổng quát

Xây dựng hệ thống quản lý trung tâm tiếng Anh dưới dạng ứng dụng web, hỗ trợ quản lý thống nhất quá trình từ đăng ký học đến công bố kết quả học tập; cung cấp thông tin phục vụ công tác tuyển sinh, đào tạo, quản lý học phí và theo dõi hoạt động của trung tâm.

### 2.2. Mục tiêu cụ thể

- Phân tích các nhóm người dùng, yêu cầu chức năng và quy tắc nghiệp vụ; xác định mối liên hệ giữa hồ sơ đăng ký, kết quả đầu vào, hóa đơn, lớp học và bảng điểm.
- Thiết kế cơ sở dữ liệu có cấu trúc rõ ràng, hạn chế trùng lặp và bảo đảm tính toàn vẹn của các dữ liệu liên quan.
- Hỗ trợ học viên tự đăng ký tài khoản, chọn khóa học, đăng ký lịch kiểm tra đầu vào, xác nhận nhập học và theo dõi hóa đơn, lịch học, kết quả của mình.
- Hỗ trợ nhân viên xử lý hồ sơ tuyển sinh, quản lý chương trình và lớp học, tổ chức lịch thi, xếp lớp, điểm danh, nhập điểm và xét duyệt bảng điểm theo đúng trách nhiệm.
- Hỗ trợ kế toán đối chiếu, ghi nhận thanh toán học phí và lập báo cáo; cung cấp báo cáo tổng hợp về tuyển sinh, đào tạo và tài chính cho giám đốc.
- Xây dựng cơ chế xác thực, phân quyền và ghi nhận các thao tác cần theo dõi; kiểm soát việc truy cập dữ liệu theo vai trò và phạm vi được giao.
- Thiết kế giao diện tiếng Việt, bố cục phù hợp với máy tính và thiết bị di động; kiểm thử các luồng chính và các trường hợp dữ liệu không hợp lệ.

## 3. Đối tượng

### 3.1. Đối tượng nghiên cứu

Đối tượng nghiên cứu của đề tài là các quy trình nghiệp vụ và dữ liệu phục vụ hoạt động quản lý một trung tâm tiếng Anh. Nội dung tập trung vào quá trình đăng ký khóa học, kiểm tra đầu vào, xác định Band, xét duyệt nhập học, quản lý học phí, xếp lớp, lập lịch, điểm danh, đánh giá kết quả học tập và tổng hợp báo cáo.

Bên cạnh từng nghiệp vụ riêng lẻ, đề tài nghiên cứu sự liên kết giữa các bước xử lý, điều kiện chuyển trạng thái của dữ liệu và trách nhiệm của các bộ phận tham gia. Đây là cơ sở để thiết kế chức năng, cơ sở dữ liệu và cơ chế phân quyền phù hợp.

### 3.2. Đối tượng sử dụng

Hệ thống phục vụ bảy nhóm người dùng: học viên, nhân viên tuyển sinh, giáo vụ, giáo viên, kế toán, giám đốc và quản trị viên. Học viên sử dụng hệ thống để thực hiện các yêu cầu đăng ký và theo dõi thông tin của mình. Nhân viên tuyển sinh xử lý hồ sơ; giáo vụ tổ chức chương trình, lớp và lịch; giáo viên thực hiện nhiệm vụ giảng dạy, điểm danh và đánh giá. Kế toán theo dõi hóa đơn, xác nhận thanh toán; giám đốc xem báo cáo tổng hợp; quản trị viên quản lý tài khoản và tra cứu nhật ký hoạt động theo quyền được cấp.

## 4. Phạm vi

### 4.1. Phạm vi nghiệp vụ

Đề tài tập trung vào hoạt động quản lý của một trung tâm tiếng Anh, với các nhóm chức năng chính sau:

- **Tài khoản và tuyển sinh:** đăng ký, đăng nhập, quản lý thông tin cá nhân, đăng ký khóa học, theo dõi và xét duyệt hồ sơ nhập học.
- **Kiểm tra đầu vào:** quản lý ca thi, phân công giáo viên chấm, đặt hoặc đổi lịch theo điều kiện cho phép, nhập kết quả và xác định Band phục vụ xếp lớp. Việc kiểm tra được tổ chức trực tiếp; hệ thống quản lý lịch và kết quả.
- **Quản lý đào tạo:** quản lý khóa học, chương trình, cấu hình thành phần điểm, lớp học, phân công giáo viên, xếp học viên và tổ chức lịch học, lịch thi cuối khóa.
- **Theo dõi học tập:** điểm danh từng buổi, nhập điểm và nhận xét, gửi bảng điểm, trả lại để điều chỉnh hoặc chốt và công bố kết quả theo quy trình.
- **Học phí và báo cáo:** phát hành hóa đơn khi hồ sơ được duyệt, ghi nhận học phí đã thanh toán, theo dõi công nợ và cung cấp báo cáo theo quyền của kế toán, giám đốc.
- **Quản trị:** cấp tài khoản nhân sự, quản lý trạng thái tài khoản, hỗ trợ đặt lại mật khẩu và tra cứu nhật ký kiểm toán.

Việc thanh toán được thực hiện bên ngoài ứng dụng; kế toán đối chiếu và xác nhận trên hệ thống. Đề tài không bao gồm cổng thanh toán trực tuyến, trả góp, hoàn tiền, quản lý học bổng hoặc tổ chức thi trực tuyến. Giám đốc sử dụng báo cáo để theo dõi hoạt động, không tham gia phê duyệt mở lớp, chương trình hoặc các hồ sơ nghiệp vụ trên hệ thống.

### 4.2. Phạm vi kỹ thuật và triển khai

Hệ thống được phát triển theo mô hình ứng dụng web, sử dụng React và TypeScript cho giao diện, ASP.NET Core Web API và C# cho phần xử lý, Entity Framework Core kết hợp SQL Server để truy cập và lưu trữ dữ liệu. Giao diện sử dụng tiếng Việt và được thiết kế để thích ứng với các kích thước màn hình khác nhau.

Trong phạm vi đồ án, việc chạy thử và minh họa sản phẩm được thực hiện trên môi trường cục bộ với dữ liệu mẫu. Kết quả đánh giá tập trung vào chức năng, tính đúng đắn của quy trình, phân quyền và sự nhất quán của dữ liệu trong các tình huống được kiểm tra.

## 5. Phương pháp

Đề tài kết hợp phương pháp phân tích nghiệp vụ, mô hình hóa hệ thống, xây dựng phần mềm và kiểm thử. Trước hết, các tài liệu yêu cầu và quy trình quản lý được phân tích để xác định người sử dụng, dữ liệu đầu vào, kết quả đầu ra, điều kiện xử lý và những ràng buộc giữa các bộ phận. Các yêu cầu được chuẩn hóa thành danh sách chức năng và quy tắc nghiệp vụ làm cơ sở cho thiết kế.

Trên cơ sở đó, hệ thống được mô hình hóa thông qua sơ đồ use case, đặc tả use case, sơ đồ trình tự và mô hình dữ liệu. Các mô hình được đối chiếu với nhau để làm rõ tương tác giữa người dùng và hệ thống, thứ tự xử lý cũng như quan hệ giữa các đối tượng dữ liệu. Giao diện được thiết kế theo từng vai trò và gắn với các công việc mà người dùng được phép thực hiện.

Trong giai đoạn xây dựng, chức năng được triển khai theo từng nhóm nghiệp vụ có liên quan. Mỗi nhóm được kiểm tra về xử lý dữ liệu, điều kiện thực hiện và quyền truy cập trước khi tích hợp vào các luồng chung. Cách tổ chức này giúp phát hiện sai lệch giữa yêu cầu, thiết kế và hành vi thực tế trong quá trình phát triển.

Việc kiểm thử được thực hiện ở các mức đơn vị, tích hợp và giao diện. Các trường hợp kiểm tra bao gồm luồng hợp lệ, dữ liệu không hợp lệ, truy cập sai quyền, thay đổi trạng thái và tình huống có thao tác đồng thời. Kết quả kiểm thử được sử dụng để điều chỉnh phần mềm và hoàn thiện tài liệu, đồng thời xác định rõ những giới hạn còn tồn tại của sản phẩm.

## 6. Kết quả dự kiến

Đề tài dự kiến tạo ra một ứng dụng web quản lý trung tâm tiếng Anh có các chức năng chính theo phạm vi đã xác định. Hệ thống hỗ trợ sự phối hợp giữa các bộ phận thông qua dữ liệu thống nhất, giúp học viên theo dõi quá trình đăng ký và học tập, đồng thời cung cấp thông tin phục vụ quản lý tuyển sinh, đào tạo và học phí.

Về mặt tài liệu, kết quả dự kiến gồm yêu cầu hệ thống, danh sách và đặc tả use case, các sơ đồ mô tả xử lý, thiết kế cơ sở dữ liệu, thiết kế giao diện và nội dung kiểm thử. Các tài liệu cần thể hiện được mối liên hệ giữa yêu cầu nghiệp vụ, thiết kế và chức năng của sản phẩm.

Về mặt học tập, quá trình thực hiện đề tài giúp nhóm củng cố kỹ năng phân tích yêu cầu, mô hình hóa, lập trình ứng dụng web, tổ chức dữ liệu và kiểm thử phần mềm. Sản phẩm cùng bộ tài liệu là cơ sở để trình bày, minh họa và đánh giá việc vận dụng kiến thức Công nghệ phần mềm vào một bài toán quản lý cụ thể.
