# UI refresh — Student còn lại
1. Owner “tiếp tục” duyệt cổng3; stage4 theo mẫu, không thêm gate riêng.
2. Profile/booking agenda dùng StudentPersonal props thuần, field/validation cũ.
3. Invoice list tự GET lúc mở; detail/print vẫn đọc mới, lỗi không in cũ.
4. Tiền vẫn chuỗi digit, wrap khi dài, không Number/ellipsis/che overflow.
5. Grade list/detail chỉ FINALIZED; score/passed do server, plain-text comments.
6. Student class cards không management/peer; staff branch giữ handlers/layout.
7. Calendar Student tự đọc current, phân biệt history/replacement/NOT_HELD.
8. Effect timer hủy khi cleanup; mock StrictMode mỗi loại1read/write0.
9. Filter vẫn áp dụng chủ động; thay filter bỏ list cũ, pending khóa controls.
10.Password/session/CSRF/RAM/unknown guards giữ từ các giai đoạn trước.
11.181frontend/types/lint/build và21Edge cuối PASS/3widths.
12.First18PASS/3FAIL (money overflow/duplicate label) giữ; fixes→21PASS.
13.593ảnh final/14Student chọn lọc;42scratch ID/GUID-paired xóa/live0.
14.Working tree ui-refresh; stage5 tiếp theo, M4recovery/custody không đổi.
