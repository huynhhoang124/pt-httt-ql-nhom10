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
- `bai_tap_1/` – Báo cáo phân tích theo 6 góc nhìn (Word + PDF); `ly_thuyet/` – lý thuyết áp dụng
- `phan_tich/` – GĐ1 xác định, khả thi, kế hoạch (`01_ke_hoach.md`); GĐ2 phân tích: thu thập thông tin, quy tắc QT01–QT16 (`02_thu_thap.md`), mô tả 20 chức năng (`03_chuc_nang.md`), đặc tả xử lý, từ điển dữ liệu (`04_dac_ta.md`); GĐ4 cài đặt, chuyển đổi, kiểm thử 46 ca (`05_cai_dat.md`); GĐ5 bảo trì (`06_bao_tri.md`)
- `thiet_ke/` – GĐ3 thiết kế: 26 thực thể (`01_thuc_the.md`), quan hệ và ERD (`02_quan_he.md`), chuẩn hóa (`03_chuan_hoa.md`; kiểm tra: `python thiet_ke/kiem_tra_chuan_hoa.py`), CSDL vật lý (`04_csdl.md`), module và ma trận phân quyền (`05_module.md`), form, báo cáo, thực đơn, thông báo lỗi – 37 màn hình (`06_giao_dien.md`)
- `so_do/` – BFD_v3, DFD ngữ cảnh, mức 0, mức 1 (`DFD_muc_1_1.png` … `DFD_muc_1_5.png`), bảng cân bằng DFD (`can_bang_dfd.md`), ERD, sơ đồ module, thực đơn, phác thảo màn hình. Script sinh và tự kiểm tra ở `so_do/src/` (`bfd.py`, `dfd.py`, `erd.py`, `module.py`, `giao_dien.py`; cần matplotlib)
- `csdl/` – CSDL SQL Server: `schema.sql`, `views.sql`, `seed.sql` (sinh bằng `sinh_du_lieu.py`), `kiem_tra.sql` (24 kiểm tra), tệp nâng cấp `nang_cap/`, mô tả bảng `mo_ta_bang.md` (sinh bằng `sinh_mo_ta.py`)
- `demo/` – Bản demo ASP.NET Core 8 (`TrungTamNgoaiNgu/`), kiểm thử đơn vị xUnit (`KiemThu/`), sinh mã và kiểm tra đồng bộ thiết kế – mã (`sinh_ma.py`), kiểm tra phân quyền 8 vai trò (`kiem_tra_quyen.py`)
- `bao_cao/` – Báo cáo GĐ1 + GĐ2, GĐ3 và báo cáo tổng (Word + PDF), dựng từ các tệp .md bằng `python bao_cao/lam_bao_cao.py` (cần python-docx và Microsoft Word); phân công sửa (`Phan_cong_sua_Nhom_10.docx`, từ `PHAN_CONG_SUA.md`)
- `KE_HOACH.md` – Kế hoạch xuyên suốt dự án (giai đoạn, sản phẩm, phân công, kiểm tra)

## Chạy demo và kiểm thử
1. Cài .NET 8 SDK và SQL Server (Express); dựng CSDL: `powershell -ExecutionPolicy Bypass -File csdl/chay.ps1 [-May .\SQLEXPRESS]` (kết quả cuối phải là "KẾT QUẢ: ĐẠT", 24/24 kiểm tra).
2. Sửa chuỗi kết nối `TrungTam` trong `demo/TrungTamNgoaiNgu/appsettings.json` nếu tên máy SQL khác, rồi `dotnet run --project demo/TrungTamNgoaiNgu --launch-profile http` → http://localhost:5223.
3. Tài khoản mẫu, mật khẩu = tên đăng nhập: nv001 Giám đốc, nv002 QL đào tạo, nv003 Quản trị, nv004 Kế toán, nv005 Tuyển sinh, gv023 Giáo viên, hv0412 Học viên, ph0301 Phụ huynh. Giờ hệ thống của demo cố định 04/01/2027 19:30 (`HeThong:ThoiDiem`).
4. Kiểm thử: `dotnet test demo/KiemThu` (98 ca), `python demo/sinh_ma.py` (đồng bộ thiết kế – mã), `python demo/kiem_tra_quyen.py` khi ứng dụng đang chạy (phân quyền, 248 lượt). Ca kiểm thử theo tiến trình: `phan_tich/05_cai_dat.md` mục 4.2.

## Quy ước
Nhánh `main` do Huynh quản lý (bắt buộc duyệt). Mỗi việc một nhánh `them/<mô-tả>` hoặc `sua/<mô-tả>` tạo từ `main`, mở Pull Request vào `main`; quy trình đầy đủ ở `phan_tich/06_bao_tri.md` mục 2 và `PHAN_CONG_SUA.md` mục 0.
