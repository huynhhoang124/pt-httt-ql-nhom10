# Phát triển HTTT quản lý – Nhóm 10

Đề tài: **Hệ thống quản lý trung tâm ngoại ngữ**

## Thành viên
1. Hoàng Văn Huynh (trưởng nhóm)
2. Nguyễn Gia Ân
3. Vũ Văn Hùng
4. Nguyễn Văn Luận
5. Trần Thu Thủy
6. Trần Minh Quân

## Nội dung
- `bai_tap_1/` – Báo cáo phân tích theo 6 góc nhìn (Word + PDF)
- `so_do/` – Sơ đồ BFD_v3, DFD mức ngữ cảnh, DFD mức 0, DFD mức 1 (`DFD_muc_1_1.png` … `DFD_muc_1_5.png`), bảng cân bằng DFD (`can_bang_dfd.md`)
- `phan_tich/` – GĐ1 xác định, khả thi, lập kế hoạch (`01_ke_hoach.md`); giai đoạn phân tích: 2.1 thu thập thông tin (quy tắc nghiệp vụ QT01–QT16), 2.2 phân tích chức năng (mô tả 20 chức năng, ma trận thực thể – chức năng), 2.4 đặc tả xử lý (bảng/cây quyết định) và từ điển dữ liệu
- `bao_cao/` – Báo cáo GĐ1 + GĐ2 (`Bao_cao_GD1_GD2_Nhom_10`, Word + PDF), dựng tự động từ các file .md bằng `python bao_cao/lam_bao_cao.py` (cần python-docx và Microsoft Word)
- `so_do/src/` – Script sinh sơ đồ: sửa dữ liệu trong `bfd.py` / `dfd.py` rồi chạy `python so_do/src/bfd.py` và `python so_do/src/dfd.py` (cần matplotlib)
- `KE_HOACH.md` – Kế hoạch xuyên suốt dự án (giai đoạn, sản phẩm, phân công, kiểm tra)

## Quy ước
Nhánh `main` do Huynh quản lý. Thành viên làm trên nhánh riêng và mở Pull Request vào `main`.
