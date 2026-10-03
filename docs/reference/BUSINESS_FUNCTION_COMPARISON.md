# Đối chiếu nghiệp vụ và chức năng với báo cáo nguồn

Ngày đối chiếu: 02/10/2026. Phạm vi: nội dung văn bản nghiệp vụ/chức năng,
không phải nghiệm thu E05 hoặc xác nhận phần mềm đã chạy.

## 1. Nguồn và cách đọc kết quả

- Báo cáo: `C:\Study\SOEN\final_workplace\Nhom18_ThietKeGiaoDien.docx`.
  SHA-256: `7B7F8C0ABC6A44476E70A8D9DE8B03C592B9AA29DC3ED5F97AAB247F0877C258`.
- Đọc trực tiếp văn bản mục I–III, V, các ràng buộc nghiệp vụ VI.3 và mô tả
  giao diện VII. Mục IV là biểu mẫu minh họa, không dùng để thêm yêu cầu.
  Chưa kiểm tra hình ảnh nhúng trong lượt đối chiếu này.
- Chuẩn hiện hành: [requirements](../requirements.md),
  [E01](../design/FUNCTION_SPEC.md), [E02](../design/WORKFLOW_SPEC.md),
  [quyết định đã duyệt](../DECISIONS.md); provenance trước đó:
  [REPORT_BASELINE](REPORT_BASELINE.md).
- `Pxxx` là chỉ số đoạn XML trong `word/document.xml`, tính cả đoạn trong
  bảng; **không phải số trang Word**. Mã NV theo mục III; số thứ tự 1–30
  trong mục V là danh sách khác, không được coi là NV01–NV30.
- “Giữ” nghĩa là giữ chức năng cốt lõi, vẫn áp dụng guard đã duyệt.
  “Chuẩn hóa” nghĩa là thiết kế bổ sung/làm rõ điều kiện vận hành.
  “Đổi đã duyệt” và “Loại” không phải lỗi thiếu chức năng cần khôi phục.
  Các đề nghị dưới đây là nội dung nên đồng bộ khi sửa báo cáo; chưa sửa Word.

## 2. Kết luận về độ bao phủ

Mục III có **39 NV nguồn**. Thiết kế giữ **37 NV**, loại NV10/NV11
(Tuyển sinh đăng ký khóa/đặt ca thay, ADR-012). E01 tổ chức thành **32 F còn
hiệu lực** (F15 bị loại); E02 có **21 W**. Hệ thống tự động xử lý là một
nhóm chức năng, không là vai trò người dùng thứ tám.

Mục V chỉ có 30 chức năng. So với mục III, mục V không liệt kê riêng NV03,
NV04, NV05, NV11, NV14, NV15, NV21, NV22, NV26 và NV36; đồng thời gộp
NV07/NV23 thành một dòng xem lịch. Vì vậy không thể dùng riêng mục V để
kết luận thiết kế thiếu nghiệp vụ. Những chức năng giữ lại trong nhóm này
đã có F/W tương ứng ở bảng mục 4; NV11 là loại có chủ đích.

Luồng chính được giữ: Student tự đăng ký → chọn Course → thi đầu vào →
Band → Student xác nhận → Tuyển sinh duyệt và phát hành Invoice → Kế toán
xác nhận thu đủ tiền → Giáo vụ xếp lớp/lập lịch → Giáo viên điểm danh/nhập
điểm/gửi → Giáo vụ trả hoặc chốt → Student xem kết quả đã chốt.

## 3. Các chỗ cần đồng bộ trong báo cáo

| Mã | Vị trí và nội dung nguồn | Đối chiếu với chuẩn đã duyệt | Cách diễn đạt cần dùng |
| --- | --- | --- | --- |
| B01 | I P35; III NV10/NV11 P135–144; V.7 P821–823: hỗ trợ ghi danh/đăng ký/đặt ca thay | Đổi đã duyệt, ADR-012; F15 bị loại | Student tự tạo hồ sơ và đặt ca; Tuyển sinh tra cứu, duyệt/từ chối/hủy theo quyền. Bỏ hai chức năng làm thay khỏi danh sách còn hiệu lực. |
| B02 | II P68: tự động chấm đầu vào; III NV22 P207–211: giáo viên nhập điểm | Mâu thuẫn nội bộ nguồn; W08 giữ chấm bởi Teacher được phân công | Teacher nhập điểm 0–100; hệ thống chọn điểm tốt nhất và suy Band, không tự chấm bài thi. |
| B03 | II P61: học bổng | Ngoài phạm vi đã duyệt | Bỏ học bổng, giảm học phí từ học bổng và thực thể tương ứng khỏi yêu cầu hiện hành. |
| B04 | II P57/P71: đổi trạng thái do nợ quá 15 ngày | Đổi đã duyệt; requirements phần tài chính | Quá hạn chỉ là cảnh báo của Invoice UNPAID sau due_date; không phạt, tăng tiền hoặc tự đổi trạng thái Student. Hạn đã chốt: ngày phát hành VN +7 ngày lịch. |
| B05 | II P70: công nợ = phải thu − đã thu; V.21 P863–865: doanh thu theo hóa đơn PAID | Công thức chưa phân biệt kỳ; W20 đã làm rõ | Doanh thu theo Payment xác nhận trong kỳ; phải thu theo Invoice không CANCELLED phát hành trong kỳ; công nợ là tổng Invoice UNPAID hiện tại, không lấy hai tổng khác kỳ trừ nhau. |
| B06 | III NV38 P311–315; V.9/V.29 P827–829/P887–889: số buổi/trọng số thuộc Course | Không thống nhất với VI dùng TrainingProgram/ScoreComponentConfig | Course giữ thông tin khóa và học phí; TrainingProgram theo Course/Band giữ số LESSON, thành phần và trọng số. Class tham chiếu program; cấu hình khóa từ lần dùng đầu. |
| B07 | V.8 P824–826: duyệt khi đủ thông tin/không trùng | Thiếu guard đã có ngay tại III NV12 và VI P1835 | Chỉ duyệt Registration CONFIRMED có Band hợp lệ; duyệt + ADMITTED + một Invoice UNPAID cùng giao dịch, không tự PAID/xếp lớp. |
| B08 | V.10 P830–832: tối đa 15 hoặc cấu hình | Câu mở có thể vượt giới hạn nguồn III NV16 | max_size cấu hình trong 1–15, không cho vượt 15. |
| B09 | V.12 P836–838: chỉ nêu đã thanh toán; VI P1845 cho OPEN hoặc ONGOING | Đổi/làm rõ đã duyệt, W15 | Chỉ xếp vào Class OPEN, Registration ADMITTED, Invoice PAID, đúng Course/Band, còn chỗ và không xung đột lịch Student. Không xếp ONGOING, chuyển lớp hoặc gỡ sau xếp. |
| B10 | III NV19; VI P1794–1795/P1848–1849: chống trùng phòng/lớp/giáo viên | Chuẩn hóa đã duyệt, W07/W15/W16 | Kiểm tra thêm lịch Student giữa ca thi và lịch lớp; lịch tuần lưu toàn bộ hoặc không lưu buổi nào; kiểm lại tại commit. |
| B11 | III NV03: tối đa hai lượt; chưa tách vắng/hủy/đặt lại | Chuẩn hóa đã duyệt, W07–W09 | Hai logical attempts mỗi Registration; lượt hai sau điểm lượt một và trước CONFIRMED. Vắng không là 0, đặt lại cùng lượt; history booking không tiêu thêm lượt. Không xác nhận khi còn lượt chờ thi/chấm. |
| B12 | VI P1853: có thể sửa điểm theo phân quyền đặc biệt | Ngoại lệ không được chuẩn hiện hành cấp quyền | Teacher đúng phân công nhập/sửa trong cửa sổ hợp lệ; không Admin/Giáo vụ override điểm, không mở lại FINALIZED. Sửa điểm đầu vào còn phải đúng original grader và trước xác nhận. |
| B13 | III NV25/NV26/NV20; VI P1857–1859: gửi/trả/chốt chưa nói rõ đơn vị | Chuẩn hóa đã duyệt, W18/W19 | Nháp lưu từng Student; gửi/gửi lại/trả/chốt chuyển **cả lớp** hoặc không ai. Class COMPLETED, đủ điểm mới gửi; trả có lý do; Student chỉ thấy FINALIZED. |
| B14 | V.29: tính tổng khi nhập; VI công thức tổng | Chuẩn hóa E03/ADR-135 | Tổng nháp tính khi đọc, final_score NULL trước FINALIZED. Chốt mới lưu tổng, decimal/làm tròn half-up một lần hai chữ số; đạt khi tổng đã chốt ≥50. |
| B15 | V.21: Excel/CSV | Đổi đã duyệt trong E01 F30/W20 | Kế toán xuất CSV theo cùng bộ lọc/cột; không XLSX. Director chỉ xem tổng hợp, không dữ liệu cá nhân hoặc xuất file. |
| B16 | II P73: hóa đơn/biên lai; V.3: Xem & Thanh toán | Cần tách chứng từ và trách nhiệm | Student xem Invoice/hướng dẫn và trả tiền ngoài hệ thống; Kế toán xác nhận toàn bộ một lần. Invoice UNPAID không là biên nhận, không payment gateway hoặc biên lai riêng. |
| B17 | I P53/V.30 P890–892: audit mọi cập nhật/tạo-sửa-xóa | Rộng hơn III NV39 và scope đã duyệt | Audit mutation nghiệp vụ/tài khoản bắt buộc theo danh mục, cùng giao dịch; security-only best-effort. Không suy quyền xóa dữ liệu hoặc log mọi lần đọc. Admin chỉ tra cứu, không sửa/xóa/export. |
| B18 | VI P1764: locked_until cho khóa tạm; V.26 chỉ Active/Locked | Chuẩn hóa tài khoản đã duyệt | ACTIVE/LOCKED/DISABLED tách biệt; throttle đăng nhập có thời hạn không chuyển Account sang LOCKED. Admin khóa/ngừng/reset thu hồi phiên; mở lại không phục hồi phiên cũ. |
| B19 | VII.2.2 P1911: tài khoản Student do quản trị cấp, nhưng form cho tự tạo | Mâu thuẫn nội bộ nguồn | Student tự đăng ký; Admin cấp tài khoản nhân sự. Mô tả giao diện phải tách việc tạo Account khỏi việc đăng nhập và xác thực phiên. |
| B20 | VII.2.4 P2065–2117: tên Trang chủ Giáo vụ nhưng duyệt hồ sơ/ghi chú Tuyển sinh | Sai tên vai trò theo III NV09/NV12 | Đổi tên thành Trang chủ Tuyển sinh; Giáo vụ dùng nhóm lớp/lịch/xếp lớp/bảng điểm. Khung ghi chú minh họa không tự cấp chức năng lưu ghi chú nội bộ mới. |
| B21 | VII.2.3 P2061–2062: Tải phiếu điểm | Không có chức năng tải phiếu điểm trong E01 F12 | Giữ xem bảng điểm cá nhân FINALIZED; chưa coi nút minh họa là quyền/API xuất phiếu điểm. Nếu muốn thêm cần quyết định riêng của owner. |

## 4. Truy vết đủ 39 nghiệp vụ nguồn

F là màn hình E01, W là workflow E02. Bảng giữ mã nguồn để tiện sửa danh sách
báo cáo mà không nhầm mã sau khi bỏ chức năng.

| NV | Nghiệp vụ mục III / vị trí | Kết quả đối chiếu | F / W |
| --- | --- | --- | --- |
| NV01 | Đăng ký/đăng nhập, P83–88 | Giữ; Student tự tạo, staff do Admin cấp, phiên/quyền tách rõ | F01–02, F32 / W01–03 |
| NV02 | Đăng ký khóa học, P89–93 | Giữ; Course ACTIVE, một hồ sơ chưa kết thúc mỗi Student/Course | F04–05 / W05 |
| NV03 | Đăng ký thi đầu vào/thi lại, P94–98 | Chuẩn hóa hai lượt và booking history, không thi lại sau xác nhận | F06–07 / W07 |
| NV04 | Xem kết quả/Band, P99–103 | Giữ; kết quả đúng hồ sơ, best score từ lượt COMPLETED | F08 / W08 |
| NV05 | Xác nhận nhập học, P104–108 | Giữ; Band hợp lệ, không còn lượt chờ, chưa tạo Invoice tại bước này | F09 / W09 |
| NV06 | Xem hóa đơn/thanh toán, P109–113 | Giữ; thanh toán bên ngoài, không Student đánh PAID | F10 / W11–12 |
| NV07 | Xem lịch lớp, P114–118 | Giữ; chỉ lịch thuộc assignment của mình | F11 / W16 |
| NV08 | Xem bảng điểm, P119–123 | Giữ; chỉ own FINALIZED | F12 / W19 |
| NV09 | Tra cứu hồ sơ, P130–134 | Giữ; đúng phạm vi Tuyển sinh, không sửa hồ sơ Student thay | F13 / W10 |
| NV10 | Đăng ký khóa thay, P135–139 | Loại, ADR-012 | F15 bị loại / không W thực hiện |
| NV11 | Đặt ca thay, P140–144 | Loại, ADR-012 | F15 bị loại / không W thực hiện |
| NV12 | Duyệt nhập học, P145–149 | Giữ; CONFIRMED + Band, atomic Invoice, thêm nhánh từ chối/hủy đã duyệt | F14 / W10–11 |
| NV13 | Quản lý Course/Program/Band, P156–160 | Chuẩn hóa quan hệ cấu hình, lifecycle/freeze; không Director duyệt | F18 / W13 |
| NV14 | Quản lý ca thi, P161–165 | Giữ; capacity 1–20, grader, room, giờ và trạng thái hợp lệ | F16–17 / W06 |
| NV15 | Theo dõi kết quả đầu vào, P166–170 | Giữ; Giáo vụ theo dõi, không ghi đè điểm/Band | F16–17 / W08 |
| NV16 | Tạo/quản lý lớp, P171–175 | Giữ; program, capacity 1–15, lifecycle explicit | F19–20 / W14 |
| NV17 | Phân công giáo viên, P176–180 | Giữ; một Teacher/lớp, đổi chỉ trước dạy và chưa dữ liệu phụ thuộc | F20 / W14 |
| NV18 | Xếp lớp, P181–185 | Giữ; đủ ADMITTED/PAID/Course/Band, OPEN/capacity/conflicts | F22 / W15 |
| NV19 | Lập thời khóa biểu, P186–190 | Chuẩn hóa LESSON/exam, preview, thay buổi cùng obligation, HELD explicit | F21 / W16 |
| NV20 | Duyệt/chốt bảng điểm, P191–195 | Giữ; trả có lý do hoặc finalize toàn lớp | F23 / W19 |
| NV21 | Xem ca được giao chấm, P202–206 | Giữ; current grader scope | F25 / W06–08 |
| NV22 | Nhập điểm đầu vào, P207–211 | Giữ; Teacher chấm 0–100, ABSENT riêng, correction có guard/lý do | F25 / W08 |
| NV23 | Xem lịch giảng dạy, P212–216 | Giữ; lớp được phân công và lịch hiện hành | F24 / W14–16 |
| NV24 | Điểm danh, P217–221 | Giữ; một Student/LESSON, chưa ghi khác vắng, batch một buổi | F26 / W17 |
| NV25 | Nhập/gửi bảng điểm, P222–226 | Giữ; cấu hình Program, nháp từng người/gửi cả lớp | F27 / W18 |
| NV26 | Sửa/gửi lại bảng bị trả, P227–231 | Giữ; RETURNED, bảo toàn lịch sử, gửi lại toàn lớp | F27 / W18 |
| NV27 | Tra cứu hóa đơn, P238–242 | Giữ; finance scope và cảnh báo overdue, không penalty | F28 / W11 |
| NV28 | Xác nhận thu học phí, P243–247 | Giữ; một Payment đúng tiền Invoice + PAID atomic | F29 / W12 |
| NV29 | Báo cáo tài chính/xuất, P248–252 | Đổi đã duyệt: định nghĩa kỳ/current debt, chỉ CSV | F30 / W20 |
| NV30 | Báo cáo tuyển sinh, P259–263 | Giữ; Director chỉ tổng hợp, không phê duyệt | F31 / W20 |
| NV31 | Báo cáo đào tạo, P264–268 | Chuẩn hóa tiến độ obligations và kết quả FINALIZED | F31 / W20 |
| NV32 | Báo cáo tài chính Director, P269–273 | Giữ; tổng hợp cùng định nghĩa Kế toán, không export/PII | F31 / W20 |
| NV33 | Tạo tài khoản nhân sự, P280–284 | Giữ; một fixed role, temp password/must-change đã duyệt | F32 / W01–03 |
| NV34 | Khóa/mở tài khoản, P285–289 | Chuẩn hóa DISABLED/reset/revocation và bảo vệ Admin cuối | F32 / W03 |
| NV35 | Xem nhật ký, P290–294 | Giữ; Admin-only, read/filter, không sửa/xóa/export | F33 / W21 |
| NV36 | Tính Band tự động, P301–305 | Giữ; best COMPLETED, tie lượt1, ngưỡng khóa theo first use | F08, F25 / W08 |
| NV37 | Tạo hóa đơn tự động, P306–310 | Giữ; một Invoice lúc ADMITTED, snapshot học phí tại issue | F14, F10, F28 / W10–11 |
| NV38 | Tính tổng điểm, P311–315 | Chuẩn hóa weighted Program, final_score chỉ FINALIZED | F27, F23, F12 / W18–19 |
| NV39 | Ghi nhật ký, P316–320 | Giữ; phạm vi/sự kiện/atomicity/retention đã chuẩn hóa | F33 và mutation liên quan / W21 |

## 5. Chi tiết thiết kế đã duyệt nhưng nguồn mô tả chưa đủ

Các chi tiết sau là phần làm rõ đã có trong requirements/E01/E02, không phải
chức năng mới đề xuất ở lượt đối chiếu: hủy/từ chối hồ sơ có lý do; đặt lại
cùng lượt khi vắng/hủy ca; sửa điểm đầu vào có giới hạn; lifecycle lớp/ca thi;
buổi không tổ chức và replacement; ghi HELD; điểm danh chưa ghi; lưu nháp
và chuyển bảng điểm toàn lớp; khóa cấu hình; phân biệt kỳ doanh thu và công
nợ hiện tại; reset/ngừng tài khoản và thu hồi phiên; xử lý thao tác lặp/xung
đột và audit bắt buộc. Khi sửa báo cáo nên bổ sung chúng vào điều kiện và
nhánh ngoại lệ tương ứng, không tạo thêm role hay mã NV tùy ý.

## 6. Ranh giới và bước sử dụng tiếp

Đối chiếu này cho thấy các NV còn hiệu lực đều có F/W; điều đó **không** chứng
minh toàn bộ API/DB/runtime hoàn chỉnh hoặc phần mềm hoạt động đúng. E04
vẫn đang hoàn thiện; E05 chưa nghiệm thu. Hình use-case/lược đồ và ảnh giao
diện cần lượt kiểm tra trực quan riêng nếu owner yêu cầu sửa phần đó.

Ưu tiên đồng bộ báo cáo: (1) danh sách III/V và vai trò; (2) luồng đầu vào,
duyệt/thanh toán/xếp lớp; (3) cấu hình/điểm/báo cáo; (4) quyền và nhãn giao
diện. Dùng mục 3 làm danh sách sửa, mục 4 làm bảng kiểm chống bỏ sót. Mọi
thay đổi phạm vi mới cần owner quyết định; bản Word nguồn được giữ nguyên.
