# Đề xuất giao diện v2 – "Vở ô li và mực tím"

Trạng thái: **đề xuất, chờ nhóm chốt** (12/10/2026). Bản mẫu xem trực tiếp: `thiet_ke/de_xuat_giao_dien_v2.html` (mở bằng trình duyệt). Giao diện đang chạy (v1, đen – trắng – xám) giữ nguyên cho tới khi nhóm đồng ý.

## 1. Ý tưởng

Lấy hình ảnh quen thuộc nhất của việc học ở Việt Nam: chữ **mực tím** trên trang **vở ô li**, lỗi được thầy cô sửa bằng **bút đỏ**. Người dùng là nhân viên trung tâm, giáo viên, học viên và phụ huynh, nên giao diện vừa phải gọn để nhập liệu nhanh, vừa có "chất trường lớp" khi học viên, phụ huynh xem lịch, điểm.

- Màu chính tím mực: mọi thứ người dùng bấm hoặc đang chọn (nút, mục thực đơn đang mở, tiêu đề).
- Bút đỏ: **chỉ** cho lỗi, cảnh báo, không đạt, quá hạn – đúng nghĩa "sửa bài" (vẫn giữ nguyên tắc một màu nhấn của 06_giao_dien 5.2).
- Một điểm nhấn duy nhất: **dải đầu trang kẻ ô li có đường lề đỏ nhạt** như trang đầu cuốn vở, nơi ghi tên màn hình và ô "Mẫu". Vùng nhập liệu, bảng vẫn nền trắng, gọn.

So với v1: v1 đúng quy ước nhưng lạnh, giống phần mềm kế toán; v2 giữ toàn bộ bố cục, phím tắt, nút lệnh của v1, chỉ đổi màu, chữ, dải đầu trang và cách hiện điểm.

## 2. Bộ màu

| Tên | Mã | Dùng cho |
|---|---|---|
| Tím mực | `#43307f` (đậm `#33245f`) | Nút chính, viền ô đang nhập, mục thực đơn đang mở, tiêu đề, đường kẻ dưới thanh trên |
| Chữ | `#22203a` | Chữ thường |
| Giấy | `#ffffff` | Nền nội dung |
| Giấy vở | `#f7f6fc` | Thanh thực đơn, ô thông tin, dòng trợ giúp |
| Đường kẻ | `#dcd6ee` | Viền bảng, ô nhập, đường kẻ ô li |
| Chữ phụ | `#625d7a` | Đường dẫn, ghi chú, mã màn hình trong thực đơn |
| Bút đỏ | `#c62828` (nền nhạt `#fdeceb`) | Lỗi, cảnh báo, không đạt, quá hạn, dấu \* trường bắt buộc |
| Lề vở | `#efb4b1` | Chỉ đường lề trong dải đầu trang (trang trí) |

Đã tránh: tím chàm sáng kiểu phần mềm SaaS (`#4f46e5`), nền kem, thẻ bo tròn có bóng đổ; chọn tím mực tối ngả đỏ như mực thật.

## 3. Chữ

- Tiêu đề màn hình, điểm số trong bảng điểm: **Literata** (chữ có chân, dễ đọc, có tiếng Việt).
- Giao diện, nhãn, bảng: **Be Vietnam Pro** (như v1).
- Chứng từ in (phiếu thu, chứng nhận): **Times New Roman** (như v1, khớp báo cáo của nhóm).
- Số luôn thẳng cột (`tabular-nums`); điểm hiện dấu phẩy thập phân kiểu Việt Nam (7,4).

## 4. Thành phần thay đổi

| Thành phần | v1 | v2 |
|---|---|---|
| Thanh trên | trắng, gạch dưới xám | trắng, gạch dưới tím mực 3px |
| Thực đơn trái | mục đang mở nền đen | nền giấy vở; mục đang mở có vạch tím bên trái, chữ tím đậm (như dây đánh dấu trang) |
| Đầu trang | trắng | dải kẻ ô li + đường lề đỏ nhạt; tên màn hình chữ Literata màu tím đậm; ô "Mẫu" viền tím |
| Nút | đen | tím mực; nút phụ viền tím; bo góc 6px |
| Ô nhập đang gõ | viền đen | viền tím + quầng tím nhạt |
| Ô nhập sai | viền đỏ | viền đỏ, câu báo đỏ ngay dưới ô (như lời phê) |
| Bảng | đầu bảng xám | đầu bảng tím rất nhạt, gạch dưới tím; dòng kẻ ngang như dòng vở |
| Xếp loại | chữ thường | nhãn: Giỏi (nền tím), Khá / Trung bình (nền vở), Không đạt (viền đỏ); dòng không đạt chữ đỏ |
| Ô thông tin / lỗi | vạch đen / đỏ | vạch tím / đỏ, bo góc |
| Điểm danh F4.1 | ô đen | ô tím, bo 8px |

## 5. Việc cần làm khi được chốt (ước 1–2 giờ, không đổi trang nghiệp vụ)

1. `wwwroot/css/site.css`: thay biến màu ở `:root` theo mục 2; thêm biến `--but-do`, `--le`, `--serif`; viết lại các khối `.thanh-tren`, `.thuc-don a[aria-current]`, `.dau-trang` (ảnh nền kẻ ô li: xem `.dau-trang` trong bản mẫu), `.btn-primary/.btn-outline-primary`, `.table thead th`, `.hop-tb/.hop-loi`, `.chon-dd`.
2. `Pages/Shared/_Layout.cshtml`: thêm Literata vào liên kết Google Fonts (vẫn tải không chặn hiển thị); bọc đường dẫn + tiêu đề + ô Mẫu trong một khối `.dau-trang` toàn bề ngang.
3. Bảng điểm (R4.3a, R4.4, sau này R4.3b): lớp `xep` cho ô xếp loại, `khong-dat` cho dòng không đạt; điểm dùng `HienThi` với dấu phẩy.
4. Sửa `thiet_ke/06_giao_dien.md` mục 5.2 dòng "Màu sắc": "Trắng, xám và tím mực làm màu chính; chỉ một màu nhấn đỏ cho lỗi, cảnh báo, không đạt".
5. Kiểm tra lại như ca TC43 (8 vai trò, 375/1280 px, độ tương phản chữ ≥ 4.5:1 – đã tính: tím mực trên trắng 10,7:1; chữ phụ trên giấy vở 5,8:1; bút đỏ trên nền đỏ nhạt 4,9:1).

## 6. Bước sau (nên làm cùng các màn hình của nhóm)

- Trang chủ riêng cho học viên, phụ huynh dạng "sổ liên lạc": lịch học tuần này (R2.3a), chuyên cần (R4.1), học phí còn nợ (R3.3), kết quả mới nhất (R4.3b) – dùng các màn hình Ân, Luận, Hùng đang làm.
- Phiếu báo kết quả R4.3b in theo mẫu bảng điểm, điểm chữ Literata.
