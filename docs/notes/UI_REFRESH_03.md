# UI refresh — Student pilot
1. Owner “tiếp tục đi” duyệt cổng2; giai đoạn3 theo UI_REFRESH_PLAN.
2. StudentEnrollmentPage giữ command/manifest/UUID/RV/unknown lookup cũ.
3. Effect trì hoãn một nhịp, cleanup hủy timer; initial GET có scope, không polling.
4. StudentOverview chỉ trình bày props: mỗi hồ sơ riêng, server count/step/action.
5. Tiền dùng chuỗi digit và dấu chấm, không Number hoặc tổng giả.
6. Card khóa học chỉ tạo ý định; ghi dữ liệu vẫn qua dialog xác nhận.
7. Hồ sơ/attempt/Band và ca thi dùng token, focus vào phần vừa đọc.
8. Đổi bộ lọc xóa chọn ca, đọc lại mới chọn được; không dùng snapshot cũ.
9. Profile draft cảnh báo rời trang; hủy giữ nguyên, đồng ý mới bỏ.
10. Read lỗi bỏ dữ liệu cũ; unknown chỉ đối chiếu, không tự gửi lại.
11.178frontend/21Edge cuối PASS;4Student có assertions/ảnh, không cộng trùng.
12.StrictMode main DEV Home1/courses1/write0; mock này không là auth proof.
13.46scratch ID/GUID-paired xóa/live0;178ảnh SQL và18ảnh preview riêng.
14.Working tree ui-refresh; cổng3 pending, giai đoạn4 chưa bắt đầu; custody giữ.
