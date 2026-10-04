# Tự host web đồ án trên laptop và chia sẻ qua Internet

Cập nhật: 29/09/2026.

Đây là bản tổng hợp ý tưởng và quy trình sử dụng theo yêu cầu của chủ dự án,
không phải hướng dẫn cài đặt đã kiểm chứng. Hiện chưa có ứng dụng chạy được,
chưa cài tunnel, chưa mở cổng mạng hoặc đưa dữ liệu dự án ra Internet.
Việc tạo tài liệu không tự phê duyệt D hay nghiệm thu E. Sau đó owner duyệt
D14/D16/D17 riêng (ADR-118–ADR-120); D12/D13 vẫn hoãn/chưa duyệt.

## 1. Nhu cầu và hướng sử dụng

Chủ dự án chỉ cần web hoạt động lúc phát triển, test, chỉnh sửa và báo cáo
cho giáo viên; không cần duy trì 24/7. Đồng thời, thiết bị khác cần có thể mở
web, không giới hạn ở trình duyệt trên laptop chủ.

Hướng đang cân nhắc là lấy laptop cá nhân làm máy chủ. Backend và SQL Server
chạy tại máy; khi cần chia sẻ từ xa thì bật một đường hầm mạng (tunnel).
Chưa cần mua máy chủ vật lý hoặc thuê VPS/cloud cho ứng dụng và database.
Cloudflare Tunnel/Quick Tunnel là phương án minh họa đã trao đổi, chưa cài đặt.

Tự host không có nghĩa là không dùng dịch vụ bên ngoài: với tunnel, laptop
vẫn chạy ứng dụng, còn nhà cung cấp tunnel làm trung gian chuyển tiếp truy cập.

## 2. Ba cách truy cập cùng một hệ thống

| Cách dùng | Thiết bị truy cập | Điều kiện |
| --- | --- | --- |
| Tại máy (localhost) | Trình duyệt trên laptop chủ | Laptop và hệ thống đang chạy; nghiệp vụ có thể hoạt động không cần Internet nếu toàn bộ thành phần ở máy. |
| Mạng nội bộ (LAN) | Điện thoại/máy tính cùng mạng | Dùng địa chỉ mạng của laptop; ứng dụng phải nhận kết nối LAN, firewall cho phép phạm vi cần thiết và mạng không cô lập các thiết bị. |
| Internet qua tunnel | Thiết bị ở cùng hoặc khác mạng | Dùng link HTTPS công khai; laptop, hệ thống, tunnel và Internet phải hoạt động. |

`localhost` luôn chỉ chính thiết bị đang mở trình duyệt. Giáo viên nhập
`localhost` trên máy của họ sẽ không truy cập laptop của chủ dự án.
Thiết bị khách chỉ cần trình duyệt, không phải cài .NET, SQL Server hay tunnel
trong mô hình chia sẻ web công khai này.

LAN không tự động hoạt động chỉ vì đã kết nối cùng Wi-Fi. Cấu hình địa chỉ
lắng nghe và HTTPS/chứng chỉ cần được chuẩn bị khi triển khai; không tắt
firewall toàn bộ để giải quyết kết nối. Cơ chế lắng nghe được mô tả trong
[tài liệu Kestrel của Microsoft](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints).

## 3. Thành phần và trách nhiệm

- Laptop chủ: cung cấp tài nguyên tính toán, ổ lưu dữ liệu và kết nối mạng.
- Frontend React/Vite/TypeScript/Tailwind: laptop phục vụ các file giao diện;
  mã giao diện chạy trong trình duyệt của từng người dùng.
- ASP.NET Core Web API: xử lý đăng nhập, quyền, ownership, nghiệp vụ,
  validation và transaction theo các quyết định đã chốt.
- SQL Server: lưu dữ liệu nghiệp vụ, tài khoản/phiên và audit tại laptop.
- Chương trình tunnel trên laptop: kết nối ứng dụng tại máy với dịch vụ
  chuyển tiếp bên ngoài, không xử lý thay các quy tắc nghiệp vụ.
- Dịch vụ tunnel: tiếp nhận truy cập qua link công khai và chuyển tiếp đến
  ứng dụng. Nó không thay laptop chạy backend hoặc thay SQL Server lưu DB.

Giữ frontend và API cùng origin theo ADR-099. Một mô hình triển khai minh họa
là ASP.NET Core phục vụ cả SPA build và `/api/v1` trên cùng địa chỉ; đây là
phần đề xuất D13, chưa là cấu hình đã triển khai. Trình duyệt khách không được
gọi API bằng một địa chỉ `localhost` viết cứng trong frontend.

## 4. Luồng truy cập qua Internet

```text
Trình duyệt giáo viên / điện thoại / máy tính khác
                       ↕ HTTPS
              Dịch vụ tunnel (Cloudflare)
                       ↕ Kênh tunnel
              Chương trình tunnel trên laptop
                       ↕ Kết nối nội bộ
              Web frontend + ASP.NET Core API
                       ↕ EF Core
                 SQL Server trên laptop
```

Cloudflare sử dụng chương trình `cloudflared` để tạo kết nối chủ động từ
máy host ra mạng Cloudflare. Kết nối này chuyển dữ liệu hai chiều mà không
đòi hỏi IP công cộng cố định hoặc mở cổng nhận truy cập Internet trên router.
Mạng đang dùng vẫn phải cho phép kết nối outbound cần thiết; tunnel không
bảo đảm vượt qua mọi hạn chế mạng. [Cơ chế Cloudflare Tunnel](https://developers.cloudflare.com/cloudflare-one/networks/connectors/cloudflare-tunnel/)

Ví dụ người dùng xem hoặc cập nhật một bản ghi:

1. Trình duyệt mở link công khai và tải giao diện từ ứng dụng tại laptop.
2. Người dùng đăng nhập bằng tài khoản demo; API áp dụng cơ chế xác thực
   và phiên hiện có, không cấp quyền chỉ vì người đó biết link.
3. Khi chọn xem lớp hoặc gửi form, frontend gửi yêu cầu đến API cùng origin.
4. API kiểm tra vai trò, phạm vi bản ghi và điều kiện nghiệp vụ. Yêu cầu hợp
   lệ mới đọc hoặc ghi SQL Server; quyền vẫn như khi chạy tại máy.
5. Kết quả quay lại qua tunnel; trình duyệt cập nhật giao diện. Các thiết bị
   dùng chung dữ liệu đã lưu, nhưng không mặc nhiên cập nhật màn hình tức thời
   nếu chưa có cơ chế làm mới tương ứng.

Database không chuyển lên Cloudflare. Tuy nhiên, nội dung request/response
đi qua dịch vụ trung gian; không được diễn đạt thành “mọi dữ liệu luôn chỉ
nằm trong laptop, không có dữ liệu nào được gửi ra ngoài”.

## 5. Vòng sử dụng dự kiến sau khi có ứng dụng

Đây là quy trình vận hành ở mức ý tưởng, không phải kế hoạch viết code hoặc
bộ lệnh setup. Các bước chỉ có thể thực hiện sau khi hệ thống được xây dựng,
kiểm tra và chủ dự án cho phép chia sẻ qua Internet.

### 5.1. Test và chỉnh sửa tại máy

1. Bật laptop, khởi động database và ứng dụng.
2. Mở địa chỉ tại máy để kiểm tra giao diện, API và luồng nghiệp vụ cần sửa.
3. Dùng dữ liệu test riêng; chưa cần bật tunnel khi chỉ làm việc tại máy.
4. Khi ứng dụng thay đổi, kiểm tra lại ở máy trước khi chia sẻ bản demo.

### 5.2. Chia sẻ hoặc báo cáo qua Internet

1. Chọn bản ứng dụng ổn định và database demo; kiểm tra đăng nhập tại máy.
2. Chuẩn bị nguồn điện và giữ laptop không ngủ trong thời gian demo.
3. Bật tunnel trỏ đến ứng dụng web đã chuẩn bị, không trỏ đến SQL Server
   hoặc công cụ quản trị/debug.
4. Lấy link HTTPS hiện tại; kiểm tra trên thiết bị khác, tốt nhất bằng mạng
   khác như dữ liệu di động để xác nhận đây là truy cập Internet.
5. Kiểm tra đăng nhập, một lượt đọc và một thao tác ghi hợp lệ bằng dữ liệu
   demo; kiểm tra tải lại trang chi tiết và giao diện mobile.
6. Gửi link cho người được mời và giao thông tin tài khoản demo bằng kênh
   riêng. Không ghi mật khẩu/JWT/token tunnel trong tài liệu này.
7. Trong buổi báo cáo, giữ ứng dụng/database/tunnel chạy và theo dõi lỗi.
   Tránh thay cấu hình hoặc cập nhật bản ứng dụng giữa buổi nếu không cần.

Người xem có thể thao tác trên điện thoại hoặc máy tính của họ. Mỗi tài khoản
vẫn có đúng một vai trò; không thêm role switcher thật hoặc quyền ngoài
ma trận đã chốt. Chuẩn bị đăng nhập gần lúc demo để tránh phiên hết hạn:
phiên thường tối đa 2 giờ hoặc idle 30 phút, không tắt các giới hạn này để demo.

### 5.3. Kết thúc buổi sử dụng

1. Thông báo cho người đang dùng và chờ các thao tác ghi hoàn tất.
2. Dừng tunnel để đóng đường truy cập công khai từ Internet.
3. Lưu bản sao dữ liệu demo cần giữ theo phương án backup được duyệt sau này.
4. Nếu không còn cần test tại máy/LAN, dừng ứng dụng và tắt máy bình thường.

Dừng tunnel không xóa database và cũng không tự đăng xuất hoặc thu hồi mọi
phiên tài khoản. Dữ liệu đã commit vẫn được lưu khi tắt máy bình thường;
form chưa gửi hoặc nội dung chỉ trong bộ nhớ có thể mất. Không hứa an toàn
dữ liệu khi mất điện, ổ đĩa hỏng hoặc chưa có backup.

## 6. Link demo, chi phí và giới hạn

Quick Tunnel hiện miễn phí và tạo một subdomain ngẫu nhiên trên
`trycloudflare.com`, không cần tự mua domain để thử. Link có thể đổi khi mở
lại tunnel. Dịch vụ dành cho test/development, không có cam kết uptime/SLA;
hiện giới hạn 200 request đang xử lý đồng thời và không hỗ trợ SSE.
200 request không có nghĩa là 200 người dùng, cũng không phải kết quả đo
khả năng chịu tải của laptop. [Giới hạn Quick Tunnel](https://developers.cloudflare.com/cloudflare-one/networks/connectors/cloudflare-tunnel/do-more-with-tunnels/trycloudflare/)

Định hướng này tránh phí thuê máy chủ ứng dụng/DB, nhưng vẫn dùng điện,
Internet và tài nguyên laptop. Không chốt gói trả phí hoặc mua domain ở đây.
Nếu sau này cần link cố định hoặc vận hành thường xuyên, phải đánh giá lại
phương án tunnel/domain/hosting và được chủ dự án duyệt trước khi thay đổi.

## 7. Bảo mật phải giữ nguyên

- Chỉ công khai web/API phục vụ nghiệp vụ; không mở SQL Server, thư mục dữ
  liệu, công cụ quản trị, source, secret hoặc màn hình lỗi phát triển ra Internet.
- Ưu tiên dữ liệu giả và tài khoản riêng cho demo, không đưa hồ sơ thật của
  học viên hoặc dữ liệu vận hành trung tâm lên môi trường thử công khai.
- Biết link không thay cho đăng nhập. Link ngẫu nhiên không phải một biện
  pháp phân quyền; API vẫn kiểm tra role/ownership ở mọi yêu cầu.
- Giữ HTTPS, JWT cookie host-only `HttpOnly`/`Secure`/`SameSite=Strict`, CSRF
  và kiểm tra Origin cho login/write theo ADR-099. Không đổi sang lưu JWT trong
  localStorage hoặc bỏ bảo mật chỉ để tunnel hoạt động.
- Thiết kế và kiểm tra cả đoạn connector → ứng dụng, không coi HTTPS ở link
  công khai tự động giải quyết TLS/chứng chỉ bên trong. Cấu hình chứng chỉ
  và proxy là việc cần hoàn thiện trước triển khai, không vô hiệu hóa kiểm tra
  TLS hoặc tin mọi proxy/header để né lỗi kết nối.
- ASP.NET Core cần nhận đúng scheme/host/IP khi nằm sau proxy tin cậy, phục
  vụ cookie, redirect, CSRF và rate limit ADR-100. Chỉ tin forwarding từ proxy
  được cấu hình, không tin header do client tự gửi. [Tài liệu proxy Microsoft](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer)
- Không ghi mật khẩu/hash/JWT/token tunnel hoặc toàn payload nhạy cảm vào
  log, file hướng dẫn hay bản sao dữ liệu chia sẻ; giữ các quy tắc audit hiện có.

Nếu link đổi, cookie host-only ở địa chỉ cũ không chuyển sang địa chỉ mới;
người dùng cần mở link mới và đăng nhập lại. Điều đó không có nghĩa là dữ
liệu nghiệp vụ bị tạo lại hoặc database bị mất.

## 8. Chuẩn bị trước báo cáo và phương án dự phòng

Checklist này là việc cần kiểm tra khi đã có hệ thống, không là bằng chứng
các kiểm thử đã chạy hoặc bảng phê duyệt thiết kế thứ hai.

- Bản demo và dữ liệu mẫu sẵn sàng; đủ tài khoản cho những vai trò cần trình bày.
- Có bản sao dữ liệu cần giữ và biết cách khôi phục đã được thử trên dữ liệu demo.
- Laptop cắm nguồn, không ngủ trong thời gian trình bày; kiểm tra đồng hồ hệ
  thống vì phiên và nhiều điều kiện nghiệp vụ dựa thời gian server.
- Link hiện tại được thử từ thiết bị khác; login/đọc/ghi/quyền/HTTPS đúng.
- Frontend gọi API cùng origin, không dùng localhost của thiết bị khách.
- Có phương án demo ngay trên laptop nếu Internet/tunnel gặp sự cố.
- Nếu chuẩn bị LAN dự phòng, kiểm tra trước địa chỉ, firewall, mạng và chứng
  chỉ tin cậy trên thiết bị khách; không giả định chuyển sang LAN là tự chạy.

Khi gặp lỗi, xác định lớp bị lỗi trước khi thay cấu hình:

- Tại máy cũng không mở được: kiểm tra ứng dụng, database và cấu hình tại máy.
- Tại máy được nhưng link Internet lỗi: kiểm tra laptop còn thức, Internet,
  chương trình tunnel, link hiện tại và kết nối tới ứng dụng.
- Giao diện mở nhưng API/login lỗi: kiểm tra URL API, cookie/Origin/CSRF,
  proxy và phiên; không tắt bảo mật để xử lý nhanh.
- Một thiết bị dùng được nhưng thiết bị khác không: kiểm tra link/mạng/trình
  duyệt của thiết bị đó; nếu dùng LAN, kiểm tra quyền kết nối và chứng chỉ.
- Vừa thao tác ghi thì mất mạng: đọc lại trạng thái trước khi gửi lại. Không
  tự suy thất bại từ việc mất phản hồi; giữ chống ghi lặp theo ADR-093.

## 9. Phần còn cần hoàn thiện trước khi triển khai

Nhu cầu đã được chủ dự án xác nhận: chạy theo buổi test/chỉnh sửa/báo cáo,
không cần 24/7 và có nhu cầu truy cập từ thiết bị khác. Yêu cầu hiện tại là
lưu bản tổng hợp này và ghi nhận các quyết định owner duyệt sau đó; chưa cấp
quyền cài đặt hay công khai ứng dụng.

Các chính sách đã được owner duyệt riêng trong OWNER_REVIEW và xác nhận D17:

- D11 / ADR-117: phạm vi kiểm chứng một trung tâm/10.000 học viên/50 đồng thời
  với lịch sử; không trần nghiệp vụ hoặc bằng chứng tải đã đo.
- D14 / ADR-118: backup SQL hằng ngày/mã hóa/bản riêng khỏi ổ chính, giữ 7 bản
  ngày/4 bản tuần, thử restore tháng; RPO24h/RTO2h là mục tiêu tham chiếu,
  chưa bảo đảm. Laptop tắt có thể làm lỡ backup; quy trình phải xử lý để đáp
  ứng chính sách, không tự giảm tần suất hoặc tuyên bố đạt mục tiêu.
- D16 / ADR-119/162: .NET10 LTS/EF10/SQL2025 Express đang có cho dev-test-demo nội bộ
  non-production; giữ stack, chốt patch tương thích/lock sau baseline.
  Production cần lựa chọn riêng; SQL hiện có đã được kiểm tra, chưa cài mới/cấu hình DB.
- D17 / ADR-120: chủ dự án phụ trách toàn bộ release/secret/backup-restore/
  bảo trì mặc định. Chỉ hỏi lại người khi owner yêu cầu đổi; không thêm Admin/DBA.
- D05/D08/D09 / ADR-114–ADR-116: hash/generator/blocklist và audit failure/
  retention/IP đã duyệt; hợp đồng đầy đủ ở requirements mục 4.7, chưa triển khai.

D12/D13 giữ chưa duyệt và hoãn đến khi chốt nơi chạy web/phần mềm theo ghi
chú owner. Khi tiếp tục, đưa lựa chọn nhà cung cấp tunnel/cấu hình local-LAN-
public HTTPS/proxy/cách khởi động-dừng về OWNER_REVIEW. Nơi lưu backup,
quyền vận hành và quy trình cụ thể vẫn phải xác định trước khi chạy thật;
không hỏi lại tần suất/RPO-RTO/runtime/người phụ trách đã chốt.

Gói 3 FE11–FE19 đã duyệt riêng ADR-121–ADR-129; E01-v1 được owner nghiệm thu
ngày30/09/2026, E02-v1 cũng được nghiệm thu riêng cùng ngày; E03–E05 vẫn
chưa nghiệm thu.
Phê duyệt frontend không chốt D12/D13, nhà cung cấp tunnel hoặc cấp quyền công
khai web. Bản tổng hợp không đổi quyền/nghiệp vụ hoặc tự nghiệm thu baseline;
ADR-001–ADR-129 là nguồn quyết định.
Không có code, lệnh cài đặt, migration, Git/scaffold hoặc tunnel thật được tạo.

Tài liệu chuẩn và bảng duyệt:

- [Requirements](C:/Study/SOEN/main_workplace/docs/requirements.md)
- [Architecture](C:/Study/SOEN/main_workplace/docs/ARCHITECTURE.md)
- [Decisions](C:/Study/SOEN/main_workplace/docs/DECISIONS.md)
- [Owner Review](C:/Study/SOEN/main_workplace/docs/plan/OWNER_REVIEW.md)
- [Design baseline](C:/Study/SOEN/main_workplace/docs/plan/design-baseline.md)

Thông tin nhà cung cấp được đối chiếu ngày 29/09/2026; trước triển khai cần
kiểm tra lại các giới hạn và điều kiện dịch vụ. Quy trình vận hành/checklist
ở trên là đề xuất áp dụng cho project, chưa là kết quả triển khai hoặc đo kiểm.

## 10. Tóm tắt để trình bày với giáo viên

“Hệ thống được tự host trên laptop cá nhân cho mục đích test và demo đồ án.
Backend và SQL Server chạy trên laptop; khi cần truy cập từ xa, một tunnel
chuyển tiếp yêu cầu từ link HTTPS công khai về ứng dụng. Thiết bị khách chỉ
cần trình duyệt, đăng nhập theo vai trò và dùng chung database. Hệ thống
hoạt động trong thời gian laptop, ứng dụng và tunnel đang chạy; không phải
một dịch vụ vận hành liên tục 24/7. Khi kết thúc buổi demo, đường truy cập
Internet được đóng, dữ liệu đã lưu vẫn ở laptop.”

SQL target hiện hành: [assessment](../plan/SQL_INSTANCE_ASSESSMENT.md), ADR-162.
Express dùng external scheduling/encrypted archive; staging plaintext được
bảo vệ/cleanup, keys tách media/repo và phải decrypt/restore thật trước recovery
claim. Không cài/chạy backup jobs hoặc mở network trong assessment này.
