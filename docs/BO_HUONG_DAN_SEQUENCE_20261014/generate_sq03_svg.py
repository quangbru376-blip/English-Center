import os

def generate_svg():
    svg_content = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1350 1360" width="1350" height="1360" style="background:#ffffff; font-family:'Segoe UI', -apple-system, BlinkMacSystemFont, Roboto, Helvetica, Arial, sans-serif;">
  <defs>
    <!-- Arrowhead markers -->
    <marker id="arrow-sync" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="7" markerHeight="7" orient="auto">
      <path d="M 0 1 L 10 5 L 0 9 z" fill="#2C3E50"/>
    </marker>
    <marker id="arrow-return" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="8" markerHeight="8" orient="auto">
      <path d="M 0 2 L 9 5 L 0 8" fill="none" stroke="#2C3E50" stroke-width="1.8"/>
    </marker>
    <!-- Filter for soft drop shadow -->
    <filter id="shadow" x="-5%" y="-5%" width="112%" height="115%">
      <feDropShadow dx="1.5" dy="2" stdDeviation="2.5" flood-opacity="0.12"/>
    </filter>
  </defs>

  <style>
    .title { font-size: 14.5px; font-weight: bold; fill: #2C3E50; }
    .lifeline-name { font-size: 13.5px; font-weight: 600; fill: #2C3E50; text-anchor: middle; }
    .lifeline-sub { font-size: 11px; fill: #7F8C8D; text-anchor: middle; }
    .msg-text { font-size: 12px; font-weight: 500; fill: #1C2833; }
    .msg-return { font-size: 12px; font-weight: 500; fill: #2C3E50; }
    .guard-text { font-size: 12.5px; font-weight: bold; fill: #1B4F72; }
    .note-title { font-size: 11px; font-weight: bold; fill: #7D6608; }
    .note-text { font-size: 11px; fill: #4A4A4A; }
    .fragment-tag { font-size: 11px; font-weight: bold; }
    .ref-title { font-size: 11.5px; font-weight: bold; fill: #935116; }
  </style>

  <!-- Outer Diagram Frame -->
  <rect x="20" y="20" width="1310" height="1320" fill="#FFFFFF" stroke="#7F8C8D" stroke-width="1.2" rx="2"/>
  <path d="M 20 20 L 220 20 L 235 40 L 20 40 Z" fill="#EAECEE" stroke="#7F8C8D" stroke-width="1"/>
  <text x="30" y="34" class="title">sd SQ03-A — Đăng nhập</text>

  <!-- ================= LIFELINES ================= -->

  <!-- Lifeline 1: Người dùng (Actor) -->
  <g id="actor-lifeline" transform="translate(160, 50)">
    <circle cx="0" cy="12" r="10" fill="#FEF9E7" stroke="#2C3E50" stroke-width="1.5"/>
    <line x1="0" y1="22" x2="0" y2="42" stroke="#2C3E50" stroke-width="1.5"/>
    <line x1="-14" y1="28" x2="14" y2="28" stroke="#2C3E50" stroke-width="1.5"/>
    <line x1="0" y1="42" x2="-10" y2="58" stroke="#2C3E50" stroke-width="1.5"/>
    <line x1="0" y1="42" x2="10" y2="58" stroke="#2C3E50" stroke-width="1.5"/>
    <text x="0" y="76" class="lifeline-name">Người dùng</text>
    <text x="0" y="90" class="lifeline-sub">(Actor — 1 trong 7 vai trò)</text>
  </g>
  <line x1="160" y1="145" x2="160" y2="1290" stroke="#7F8C8D" stroke-width="1" stroke-dasharray="5,4"/>

  <!-- Lifeline 2: Giao diện tài khoản (Boundary) -->
  <g id="boundary-lifeline" transform="translate(480, 50)">
    <circle cx="0" cy="28" r="18" fill="#EBF5FB" stroke="#2C3E50" stroke-width="1.5"/>
    <line x1="-18" y1="10" x2="-18" y2="46" stroke="#2C3E50" stroke-width="1.5"/>
    <line x1="-18" y1="28" x2="0" y2="28" stroke="#2C3E50" stroke-width="1.5"/>
    <text x="0" y="66" class="lifeline-name">Giao diện tài khoản</text>
    <text x="0" y="80" class="lifeline-sub">&lt;&lt;boundary&gt;&gt; F01</text>
  </g>
  <line x1="480" y1="145" x2="480" y2="1290" stroke="#7F8C8D" stroke-width="1" stroke-dasharray="5,4"/>

  <!-- Lifeline 3: Hệ thống quản lý trung tâm (Control) -->
  <g id="control-lifeline" transform="translate(880, 50)">
    <circle cx="0" cy="28" r="18" fill="#E8F8F5" stroke="#2C3E50" stroke-width="1.5"/>
    <path d="M 0 10 L 8 4 L 0 2" fill="none" stroke="#2C3E50" stroke-width="1.5"/>
    <text x="0" y="66" class="lifeline-name">Hệ thống quản lý trung tâm</text>
    <text x="0" y="80" class="lifeline-sub">&lt;&lt;control&gt;&gt;</text>
  </g>
  <line x1="880" y1="145" x2="880" y2="1290" stroke="#7F8C8D" stroke-width="1" stroke-dasharray="5,4"/>

  <!-- ================= ACTIVATION BARS ================= -->
  <!-- User Activation: initial form opening and submitting -->
  <rect x="154" y="165" width="12" height="95" fill="#FEF9E7" stroke="#B7950B" stroke-width="1"/>

  <!-- Boundary Activation -->
  <rect x="474" y="170" width="12" height="40" fill="#EBF5FB" stroke="#2980B9" stroke-width="1"/>
  <rect x="474" y="245" width="12" height="960" fill="#EBF5FB" stroke="#2980B9" stroke-width="1"/>

  <!-- Control Activation -->
  <rect x="874" y="285" width="12" height="920" fill="#E8F8F5" stroke="#16A085" stroke-width="1"/>

  <!-- ================= SEQUENCE FLOW ================= -->

  <!-- A01: Mở trang đăng nhập -->
  <line x1="166" y1="175" x2="474" y2="175" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
  <text x="320" y="168" text-anchor="middle" class="msg-text">A01: Mở trang đăng nhập</text>

  <!-- A02: Hiển thị biểu mẫu đăng nhập -->
  <line x1="474" y1="205" x2="166" y2="205" stroke="#2C3E50" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#arrow-return)"/>
  <text x="320" y="198" text-anchor="middle" class="msg-return">A02: Hiển thị biểu mẫu đăng nhập (Email, Mật khẩu)</text>

  <!-- A03: Nhập email, mật khẩu và chọn 'Đăng nhập' -->
  <line x1="166" y1="250" x2="474" y2="250" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
  <text x="320" y="243" text-anchor="middle" class="msg-text">A03: Nhập email, mật khẩu và chọn "Đăng nhập"</text>

  <!-- A04: Gửi yêu cầu đăng nhập -->
  <line x1="486" y1="285" x2="874" y2="285" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
  <text x="680" y="278" text-anchor="middle" class="msg-text">A04: Gửi yêu cầu đăng nhập (email, mật khẩu)</text>

  <!-- A05: Kiểm tra điều kiện truy cập và thông tin đăng nhập -->
  <path d="M 886 315 L 935 315 L 935 340 L 886 340" fill="none" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
  <text x="945" y="332" class="msg-text">A05: Kiểm tra điều kiện truy cập và thông tin đăng nhập</text>

  <!-- Note for A05 (placed between Boundary & Control) -->
  <g transform="translate(520, 305)" filter="url(#shadow)">
    <path d="M 0 0 L 320 0 L 335 15 L 335 68 L 0 68 Z" fill="#FFFFE0" stroke="#BDB76B" stroke-width="1"/>
    <path d="M 320 0 L 320 15 L 335 15 Z" fill="#F0E68C" stroke="#BDB76B" stroke-width="1"/>
    <text x="12" y="18" class="note-title">Ghi chú xác thực (A05):</text>
    <text x="12" y="34" class="note-text">• Hệ thống tự xác định role (không cho chọn role từ UI).</text>
    <text x="12" y="49" class="note-text">• Lỗi trước xác thực không tiết lộ email tồn tại hoặc</text>
    <text x="18" y="62" class="note-text">  trạng thái tài khoản (LOCKED / DISABLED).</text>
  </g>

  <!-- ================= OUTER COMBINED FRAGMENT: alt ================= -->
  <g id="outer-alt" transform="translate(40, 395)">
    <!-- Outer alt border -->
    <rect x="0" y="0" width="1270" height="850" fill="none" stroke="#2980B9" stroke-width="1.4" rx="2"/>
    <!-- Alt label tag -->
    <path d="M 0 0 L 60 0 L 70 20 L 0 20 Z" fill="#D4E6F1" stroke="#2980B9" stroke-width="1"/>
    <text x="12" y="14" class="fragment-tag" fill="#1B4F72">alt</text>

    <!-- Branch 1: Không đáp ứng điều kiện đăng nhập -->
    <text x="80" y="15" class="guard-text">[Không đáp ứng điều kiện đăng nhập]</text>

    <!-- A06: Cập nhật kiểm soát đăng nhập và ghi sự kiện bảo mật -->
    <path d="M 846 45 L 895 45 L 895 70 L 846 70" fill="none" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
    <text x="905" y="62" class="msg-text">A06: Cập nhật kiểm soát đăng nhập &amp; ghi sự kiện bảo mật</text>

    <!-- Note for A06 -->
    <g transform="translate(480, 50)" filter="url(#shadow)">
      <path d="M 0 0 L 340 0 L 355 15 L 355 64 L 0 64 Z" fill="#FFFFE0" stroke="#BDB76B" stroke-width="1"/>
      <path d="M 340 0 L 340 15 L 355 15 Z" fill="#F0E68C" stroke="#BDB76B" stroke-width="1"/>
      <text x="10" y="17" class="note-title">Kiểm soát &amp; Nhật ký bảo mật (A06):</text>
      <text x="10" y="32" class="note-text">• Chỉ tăng đếm sai khi sai password (5 lần sai/15p thì chặn 15p).</text>
      <text x="10" y="46" class="note-text">• Ghi nhật ký bảo mật best-effort (không log password/token);</text>
      <text x="10" y="59" class="note-text">  lỗi log không bỏ qua kiểm tra xác thực hoặc phục hồi phiên.</text>
    </g>

    <!-- A07: Trả kết quả đăng nhập không thành công -->
    <line x1="834" y1="145" x2="446" y2="145" stroke="#2C3E50" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#arrow-return)"/>
    <text x="640" y="138" text-anchor="middle" class="msg-return">A07: Trả kết quả đăng nhập không thành công hoặc yêu cầu chờ thử lại</text>

    <!-- A08: Hiển thị thông báo đăng nhập -->
    <line x1="434" y1="175" x2="126" y2="175" stroke="#2C3E50" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#arrow-return)"/>
    <text x="280" y="168" text-anchor="middle" class="msg-return">A08: Hiển thị thông báo đăng nhập (lỗi chung / chờ 15 phút)</text>

    <!-- Divider line for alt outer -->
    <line x1="0" y1="205" x2="1270" y2="205" stroke="#2980B9" stroke-width="1.2" stroke-dasharray="6,4"/>

    <!-- Branch 2: Đáp ứng điều kiện đăng nhập -->
    <text x="20" y="225" class="guard-text">[Đáp ứng điều kiện đăng nhập]</text>

    <!-- A09: Xác định vai trò và loại phiên; đặt lại đếm sai -->
    <path d="M 846 240 L 895 240 L 895 265 L 846 265" fill="none" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
    <text x="905" y="256" class="msg-text">A09: Xác định vai trò &amp; loại phiên; đặt lại đếm sai sau đăng nhập thành công</text>

    <!-- ================= INNER COMBINED FRAGMENT: alt ================= -->
    <g id="inner-alt" transform="translate(25, 285)">
      <rect x="0" y="0" width="1220" height="540" fill="none" stroke="#27AE60" stroke-width="1.3" rx="2"/>
      <path d="M 0 0 L 55 0 L 65 20 L 0 20 Z" fill="#D5F5E3" stroke="#27AE60" stroke-width="1"/>
      <text x="12" y="14" class="fragment-tag" fill="#196F3D">alt</text>

      <!-- Sub-branch 1: Không bắt buộc đổi mật khẩu tạm -->
      <text x="75" y="15" class="guard-text" fill="#196F3D">[Không bắt buộc đổi mật khẩu tạm]</text>

      <!-- A10: Tạo phiên thường và ghi sự kiện đăng nhập -->
      <path d="M 821 35 L 870 35 L 870 60 L 821 60" fill="none" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
      <text x="880" y="52" class="msg-text">A10: Tạo phiên thường &amp; ghi sự kiện đăng nhập</text>

      <!-- Note for A10 -->
      <g transform="translate(460, 45)" filter="url(#shadow)">
        <path d="M 0 0 L 335 0 L 350 15 L 350 56 L 0 56 Z" fill="#FFFFE0" stroke="#BDB76B" stroke-width="1"/>
        <path d="M 335 0 L 335 15 L 350 15 Z" fill="#F0E68C" stroke="#BDB76B" stroke-width="1"/>
        <text x="10" y="17" class="note-title">Quy tắc phiên thường (NORMAL):</text>
        <text x="10" y="32" class="note-text">• Tối đa 2 giờ tuyệt đối hoặc 30 phút không hoạt động (idle timeout).</text>
        <text x="10" y="47" class="note-text">• Không refresh token / remember-me; không thu hồi phiên khác thiết bị.</text>
      </g>

      <!-- A11: Trả kết quả đăng nhập với phiên thường và vai trò -->
      <line x1="809" y1="135" x2="421" y2="135" stroke="#2C3E50" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#arrow-return)"/>
      <text x="615" y="128" text-anchor="middle" class="msg-return">A11: Trả kết quả đăng nhập với phiên thường và vai trò</text>

      <!-- A12: Hiển thị trang đầu của vai trò -->
      <line x1="409" y1="165" x2="101" y2="165" stroke="#2C3E50" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#arrow-return)"/>
      <text x="255" y="158" text-anchor="middle" class="msg-return">A12: Hiển thị trang đầu của vai trò (Student, Teacher, Academic,...)</text>

      <!-- Divider line for inner alt -->
      <line x1="0" y1="205" x2="1220" y2="205" stroke="#27AE60" stroke-width="1.2" stroke-dasharray="6,4"/>

      <!-- Sub-branch 2: Bắt buộc đổi mật khẩu tạm -->
      <text x="20" y="225" class="guard-text" fill="#B9770E">[Bắt buộc đổi mật khẩu tạm]</text>

      <!-- A13: Tạo phiên hạn chế và ghi sự kiện đăng nhập -->
      <path d="M 821 245 L 870 245 L 870 270 L 821 270" fill="none" stroke="#2C3E50" stroke-width="1.3" marker-end="url(#arrow-sync)"/>
      <text x="880" y="262" class="msg-text">A13: Tạo phiên hạn chế &amp; ghi sự kiện đăng nhập</text>

      <!-- Note for A13 -->
      <g transform="translate(460, 255)" filter="url(#shadow)">
        <path d="M 0 0 L 335 0 L 350 15 L 350 56 L 0 56 Z" fill="#FFFFE0" stroke="#BDB76B" stroke-width="1"/>
        <path d="M 335 0 L 335 15 L 350 15 Z" fill="#F0E68C" stroke="#BDB76B" stroke-width="1"/>
        <text x="10" y="17" class="note-title">Quy tắc phiên hạn chế (RESTRICTED):</text>
        <text x="10" y="32" class="note-text">• Tối đa 10 phút và không vượt hạn mật khẩu tạm 24 giờ.</text>
        <text x="10" y="47" class="note-text">• Không cấp quyền nghiệp vụ; chỉ được đổi mật khẩu hoặc đăng xuất.</text>
      </g>

      <!-- A14: Trả kết quả cần đổi mật khẩu tạm -->
      <line x1="809" y1="350" x2="421" y2="350" stroke="#2C3E50" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#arrow-return)"/>
      <text x="615" y="343" text-anchor="middle" class="msg-return">A14: Trả kết quả cần đổi mật khẩu tạm (phiên hạn chế)</text>

      <!-- A15: Hiển thị màn hình bắt đổi mật khẩu -->
      <line x1="409" y1="380" x2="101" y2="380" stroke="#2C3E50" stroke-width="1.2" stroke-dasharray="4,3" marker-end="url(#arrow-return)"/>
      <text x="255" y="373" text-anchor="middle" class="msg-return">A15: Hiển thị màn hình bắt đổi mật khẩu</text>

      <!-- Ref box for SQ04 -->
      <g transform="translate(50, 420)" filter="url(#shadow)">
        <rect x="0" y="0" width="410" height="75" fill="#FEF9E7" stroke="#D4AC0D" stroke-width="1.2" rx="2"/>
        <path d="M 0 0 L 45 0 L 55 18 L 0 18 Z" fill="#F9E79F" stroke="#D4AC0D" stroke-width="1"/>
        <text x="10" y="13" class="fragment-tag" fill="#7D6608">ref</text>
        <text x="62" y="14" class="ref-title">SQ04 — Tự đổi mật khẩu</text>
        <text x="15" y="38" font-size="11.5px" fill="#2C3E50">Người dùng thực hiện đổi mật khẩu tạm sang mật khẩu chính thức.</text>
        <text x="15" y="55" font-size="11px" fill="#7F8C8D">Sau khi hoàn tất SQ04 thành công, hệ thống thu hồi phiên hạn chế</text>
        <text x="15" y="68" font-size="11px" fill="#7F8C8D">và cấp phiên thường mới.</text>
      </g>

    </g> <!-- end inner-alt -->

  </g> <!-- end outer-alt -->

</svg>"""
    return svg_content

if __name__ == "__main__":
    out_dir = r"C:\Users\asus\.gemini\antigravity-ide\brain\118bc032-2b0e-4500-84bd-299c5f298cef"
    svg_path = os.path.join(out_dir, "SQ03_A_Dang_Nhap.svg")
    with open(svg_path, "w", encoding="utf-8") as f:
        f.write(generate_svg())
    print("SVG generated successfully at:", svg_path)
