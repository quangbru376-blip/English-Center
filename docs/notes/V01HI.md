# V01-H/I — ba probe cụ thể, chưa gắn vào Program
H kiểm timezone Asia/Ho_Chi_Minh qua BCL; Windows có mapping được duyệt, không dùng host fallback.
Offset/rule hiện tại và tương lai phải UTC+7; lịch sử cũ không làm sai kiểm tra hiện tại.
Probe blocklist đọc mới metadata/corpus/license từ thư mục local đã bind rõ reference.
Metadata có pin revision/hash/count/license/review window, không cho duplicate hoặc tự kéo dài hạn.
LoadAsync dùng kích thước/hash đã duyệt, UTF8 strict và union ordinal; giữ Load cũ tương đương.
I bind connection reference và manifest embedded; resolver chỉ RAM, không đọc cấu hình mặc định.
Mỗi lượt dùng context mới, EF160/shared memory local, timeout hữu hạn, không pooling/retry/caller TX.
Quyền không rõ, dbo/sysadmin/CONTROL/role hoặc grant hiệu lực ngoài phạm vi đều bị từ chối.
Sau guard, inspector bắt buộc runtime true kiểm exact catalog/snapshot; không tự sửa grants.
Counter không nhạy cảm chứng minh query quyền thật; không log connection/key/path/raw errors.
45test H +33test I +3SQL readonly mới PASS;1061defaultPASS/58opt-in SKIP/build sạch hai checkout.
D07/catalog/102permissions/24bảng rỗng và bốn file owner giữ nguyên; AGY chỉ viết hai policy thuần.
Chưa có profile/login runtime/PKI/secret store thật; SQL diagnostic bị từ chối không phải runtime-ready.
Còn sáu nhóm probe; tiếp tục runtime manifest/keys, giữ D12/D13 hoãn và chưa bật endpoint/runtime.
