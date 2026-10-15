# Sơ đồ logic 6.1 đường thẳng và bảng thẳng hàng

Bản riêng theo yêu cầu bố cục như ảnh báo cáo. 24 bảng bố trí thành 6 cột ×4 hàng, hộp chữ nhật cùng kích thước, nét đen/nền trắng. Tất cả đường nối là đoạn ngang/dọc và góc vuông; self-link Schedule cũng là đường gấp vuông, không có đường cong.

- [PNG](../../SO_DO_LOGIC_6_1_THANG_HANG_20261014.png), [SVG vector](../../SO_DO_LOGIC_6_1_THANG_HANG_20261014.svg), [PDF A3 ngang](../../SO_DO_LOGIC_6_1_THANG_HANG_20261014.pdf).
- [Bố cục và đường nối có tọa độ](layout.json).
- [18 FK actor và danh mục đầy đủ](../logic-diagram-20261014/README.md).

Giữ cùng 24 bảng/32FK chính của bản trước; 18FK actor phụ trỏ về Account ở phụ lục. Mũi tên từ bảng con tới bảng cha; đầu rỗng là nullable, nét đứt là FK ghép. Điểm giao cắt không là junction hoặc quan hệ mới. InvoiceNumberCounter không có FK. Không sửa báo cáo Word, source schema hoặc dữ liệu.
