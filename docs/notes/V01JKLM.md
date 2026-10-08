# V01 J/K/L/M — cấu hình và khóa readiness
J kiểm phiên bản runtime đang nạp và hash/length của năm artifact đã duyệt.
Manifest/reference/ngày review được cung cấp rõ; không tự chọn patch hoặc tải mới.
SQL version resolver phải bind đúng database reference; test J dùng SQL version giả.
K đọc keyring mới, đối chiếu issuer/audience/reference và cửa sổ rotation.
Khóa active phải ký và xác minh JWT thật, đủ vòng đời session2h; không tạo session DB.
SID/token kiểm thử chỉ RAM, không log hoặc đưa vào worker context.
L đọc XML khóa framework đã có, từ chối plaintext và decryptor ngoài DPAPI.
Key manager mới giải mã sau restart; challenge mã hóa/xác minh chỉ RAM.
Repository L read-only, không tạo/ghi/revoke key khi readiness chạy.
M đọc file profile được chỉ định, byte/deadline hữu hạn, JSON strict và không fallback.
Field lạ/trùng, scalar sai kiểu, enum sai hoặc profile thiếu bị từ chối.
Profile hợp lệ chưa phải ready; gate vẫn cần đủ chín dependency thật.
Codex review/test/apply PASS; AGY chỉ sinh policy thuần96dòng trong scope.
Chưa mount host: hồ sơ/principal/PKI/bảo trì thực tế còn thiếu, M0 chưa đóng.
