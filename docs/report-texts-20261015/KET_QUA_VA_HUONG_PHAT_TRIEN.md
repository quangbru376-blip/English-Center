# KẾT QUẢ ĐẠT ĐƯỢC VÀ HƯỚNG PHÁT TRIỂN

## 1. Kết quả đạt được

### 1.1. Về phân tích và thiết kế hệ thống

Qua quá trình thực hiện đề tài, nhóm đã phân tích và xác định các nghiệp vụ chính của hệ thống quản lý trung tâm tiếng Anh, từ đăng ký khóa học, kiểm tra đầu vào và xét duyệt nhập học đến quản lý học phí, xếp lớp, tổ chức giảng dạy và công bố kết quả học tập. Trách nhiệm và phạm vi truy cập của bảy nhóm người dùng được xác định rõ, làm cơ sở xây dựng chức năng và kiểm soát quyền sử dụng hệ thống.

Nhóm đã xây dựng các tài liệu phân tích, thiết kế gồm danh sách và đặc tả use case, các sơ đồ mô tả tương tác, thiết kế cơ sở dữ liệu và mô tả giao diện. Các quy tắc liên quan đến trạng thái hồ sơ, lượt thi, thanh toán, xếp lớp và bảng điểm được làm rõ để duy trì sự thống nhất giữa các bước xử lý.

### 1.2. Về chức năng phần mềm

Hệ thống đã triển khai các nhóm chức năng phục vụ học viên, nhân viên tuyển sinh, giáo vụ, giáo viên, kế toán, giám đốc và quản trị viên. Học viên có thể đăng ký tài khoản, chọn khóa học, đặt lịch kiểm tra đầu vào, xem kết quả, xác nhận nhập học và theo dõi hóa đơn, lớp, lịch học cùng kết quả đã công bố của mình.

Đối với hoạt động quản lý, hệ thống hỗ trợ tuyển sinh tra cứu và xét duyệt hồ sơ; giáo vụ quản lý khóa học, chương trình, ca thi, lớp, lịch và xếp học viên; giáo viên thực hiện chấm đầu vào, điểm danh, nhập điểm và gửi bảng điểm theo phân công. Kế toán có thể đối chiếu, ghi nhận học phí đã thanh toán bên ngoài ứng dụng, theo dõi hóa đơn và lập báo cáo tài chính. Giám đốc được cung cấp các báo cáo tổng hợp về tuyển sinh, đào tạo và tài chính; quản trị viên quản lý tài khoản và tra cứu nhật ký kiểm toán.

Các chức năng được liên kết thành những luồng nghiệp vụ thống nhất. Việc duyệt hồ sơ gắn với phát hành hóa đơn, ghi nhận thanh toán là điều kiện phục vụ xếp lớp, còn kết quả học tập chỉ được hiển thị cho học viên sau khi bảng điểm được chốt và công bố. Hệ thống cũng xử lý các trường hợp như từ chối hoặc hủy hồ sơ hợp lệ, thay đổi lịch theo điều kiện cho phép và trả lại bảng điểm để giáo viên điều chỉnh.

### 1.3. Về kỹ thuật và giao diện

Ứng dụng được xây dựng với React, TypeScript, ASP.NET Core Web API, Entity Framework Core và SQL Server. Cơ sở dữ liệu thể hiện quan hệ giữa tài khoản, hồ sơ, ca thi, lớp, lịch, học phí và kết quả học tập; có các ràng buộc và cơ chế kiểm soát cập nhật nhằm bảo vệ tính nhất quán của dữ liệu.

Hệ thống đã có cơ chế xác thực, quản lý phiên và phân quyền theo vai trò, đồng thời kiểm tra phạm vi dữ liệu ở phía máy chủ. Học viên truy cập thông tin của mình; giáo viên thực hiện nhiệm vụ trong phạm vi được phân công. Các thao tác cần theo dõi được ghi nhận theo quy tắc nhật ký của hệ thống.

Giao diện sử dụng tiếng Việt, tổ chức theo công việc của từng vai trò và thích ứng với máy tính, máy tính bảng, điện thoại. Các phần tìm kiếm, biểu mẫu, danh sách, chi tiết và thông báo xử lý được trình bày thống nhất. Nhóm cũng đã chuẩn bị bộ ảnh và bảng mô tả các giao diện quan trọng để phục vụ thuyết minh sản phẩm trong báo cáo.

### 1.4. Về kiểm thử và minh họa sản phẩm

Nhóm đã thực hiện kiểm thử đơn vị, kiểm thử tích hợp và kiểm tra giao diện trong các phạm vi được xác định. Nội dung kiểm tra bao gồm luồng hợp lệ, dữ liệu không hợp lệ, truy cập sai quyền, chuyển trạng thái và một số tình huống cập nhật đồng thời. Các kết quả kiểm tra được lưu lại để đối chiếu với yêu cầu và hỗ trợ điều chỉnh phần mềm.

Một môi trường demo cục bộ với dữ liệu mẫu đã được chuẩn bị để minh họa các vai trò và luồng nghiệp vụ. Kết quả này phục vụ trình bày sản phẩm trong phạm vi đồ án; chưa thay thế việc đánh giá khi triển khai thực tế với tải lớn và dữ liệu vận hành của trung tâm. Việc hoàn thiện hiển thị tường minh điểm đầu vào tốt nhất, đánh giá giao diện cuối cùng và kiểm chứng đầy đủ phương án sao lưu, khôi phục vẫn cần tiếp tục thực hiện.

## 2. Hướng phát triển

### 2.1. Hoàn thiện giao diện và trải nghiệm sử dụng

Ưu tiên hoàn thiện các chi tiết giao diện còn thiếu, đặc biệt là cách hiển thị điểm kiểm tra đầu vào tốt nhất và lượt thi được sử dụng để xác định Band. Tiếp tục rà soát bố cục, nhãn điều khiển, thông báo và các bước xác nhận để người dùng dễ hiểu trạng thái hồ sơ và công việc cần thực hiện. Đồng thời, thu thập phản hồi khi dùng thử để điều chỉnh thao tác trên máy tính và điện thoại.

### 2.2. Hoàn thiện phương án vận hành và bảo vệ dữ liệu

Xây dựng và kiểm chứng đầy đủ quy trình sao lưu, khôi phục dữ liệu, bao gồm bảo vệ bản sao lưu, quản lý quyền truy cập và thử khôi phục trên môi trường độc lập. Bổ sung hướng dẫn vận hành, theo dõi lỗi và xử lý sự cố để tạo cơ sở đưa hệ thống từ môi trường minh họa sang môi trường sử dụng thực tế.

### 2.3. Mở rộng kiểm thử và đánh giá hiệu năng

Mở rộng các tình huống kiểm thử theo dữ liệu và cách sử dụng thực tế của trung tâm. Thực hiện đánh giá khi nhiều người dùng đồng thời tìm kiếm, đặt lịch, nhập điểm hoặc cập nhật học phí; từ đó xác định giới hạn xử lý và tối ưu những thành phần cần thiết. Việc triển khai thử nghiệm cần đi kèm đánh giá về phân quyền, an toàn thông tin và khả năng phục hồi sau sự cố.

### 2.4. Bổ sung tiện ích hỗ trợ người dùng

Có thể nghiên cứu chức năng thông báo hoặc nhắc lịch thi, lịch học, thời hạn thanh toán và kết quả đã công bố. Nội dung, thời điểm, kênh gửi và quyền nhận thông báo cần được xác định phù hợp với nhu cầu của trung tâm, đồng thời bảo vệ thông tin cá nhân của học viên.

### 2.5. Phát triển báo cáo và khả năng mở rộng

Tiếp tục phát triển các hình thức trình bày báo cáo trực quan, hỗ trợ so sánh số liệu tuyển sinh, tiến độ đào tạo và học phí giữa các kỳ. Khi có nhu cầu mở rộng, có thể nghiên cứu mô hình quản lý nhiều cơ sở hoặc tích hợp dịch vụ thanh toán trực tuyến. Những nội dung này cần được phân tích yêu cầu, bổ sung quy tắc nghiệp vụ và đánh giá tác động trước khi triển khai; không thuộc các chức năng đã hoàn thành trong phạm vi đồ án hiện tại.
