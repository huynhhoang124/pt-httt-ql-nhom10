# Phân công sửa demo và tài liệu – Nhóm 10

Lập ngày 11/10/2026 từ đợt rà soát demo và tài liệu sau khi xong "Code bổ sung" (KE_HOACH.md). Mỗi người một phần, chia theo mảng đã làm trong bảng phân công của KE_HOACH. Hoàng Văn Huynh điều phối và duyệt pull request.

| Người | Việc | Mã ca kiểm thử dành sẵn |
|---|---|---|
| Nguyễn Gia Ân | A1 R2.3a lịch học của học viên + R2.3b lịch dạy, danh sách lớp · A2 sửa câu "email" ở 01_ke_hoach | TC35, TC36 |
| Vũ Văn Hùng | H1 R3.1 bảng học phí phải thu · H2 ghi phiên bản CSDL vào bảng ThamSo | TC37, TC38 |
| Nguyễn Văn Luận | L1 R4.1 chuyên cần của học viên · L2 R4.3b phiếu báo kết quả học tập | TC39, TC40 |
| Trần Thu Thủy | T1 R5.3a báo cáo tuyển sinh · T2 sửa R5.4b đếm buổi dạy theo ngày, thêm xuất Excel | TC41, (sửa TC32) |
| Trần Minh Quân | Q3 script kiểm tra phân quyền · Q4 sửa 06_bao_tri · Q5 thử giao diện trên điện thoại thật và bản in (Q1, Q2 đã làm) | TC42, (TC43) |

Sau đợt này demo có đủ 37/37 màn hình đã thiết kế ở `thiet_ke/06_giao_dien.md` (hiện có 31).

---

## 0. Chuẩn bị chung (mọi người đọc trước)

**Lấy mã và chạy thử**
1. `git pull` nhánh `main` (Huynh đẩy lên trước khi giao việc).
2. Tạo CSDL mẫu: `powershell -ExecutionPolicy Bypass -File csdl/chay.ps1` (phải báo `KẾT QUẢ: ĐẠT`). Máy cần SQL Server Express; nếu tên instance khác `.\SQLEXPRESS04` thì sửa ở `csdl/chay.ps1` và `demo/TrungTamNgoaiNgu/appsettings.json`, **không commit** phần sửa này.
3. Chạy: `cd demo/TrungTamNgoaiNgu && dotnet run --launch-profile http` → http://localhost:5223. Giờ hệ thống cố định 04/01/2027 19:30.
4. Tài khoản mẫu (mật khẩu = tên đăng nhập): nv001 Giám đốc, nv002 QL đào tạo, nv003 Quản trị, nv004 Kế toán, nv005 Tuyển sinh, gv023 Giáo viên (lớp TOE-2611 đang học), hv0412 (3 lớp đã kết thúc, đều Đạt), hv0398 (IEK-2609, Không đạt), hv0480 (TOE-2611 đang học; lần đầu đăng nhập phải đổi mật khẩu), ph0301 (phụ huynh của HV0412), ph0320 (phụ huynh, con học IEK-2610).

**Làm một màn hình mới: chép cách của trang có sẵn**
- Trang báo cáo theo kỳ: `Pages/BaoCao/HocTap.cshtml(.cs)`. Trang danh sách có lọc + Excel: `Pages/HocPhi/CongNo`. Chứng từ in gửi học viên: `Pages/HocTap/ChungNhan`, `Pages/HocPhi/DanhSachPhieuThu`.
- Đầu lớp xử lý: `[Quyen("<module>")]`. Vai trò có quyền "x" (chữ thường, ma trận ở `thiet_ke/05_module.md` mục 5) chỉ được thấy dữ liệu của mình: dùng `PhanQuyen.ChiCuaMinh(...)` và `PhamVi.HocVien(...)` / `PhamVi.LopCuaGiaoVien(...)` trong `Nen/ThucDon.cs`.
- Trang `.cshtml`: `ViewData["Ma"] = "<mã màn hình>"` và `ViewData["TroGiup"] = "..."` (câu ngắn, tiếng Việt). Báo cáo nội bộ thêm `<partial name="_XuatBaoCao" />` (Excel + in PDF; lớp xử lý trả `XuatExcel.Tep(...)` khi `xuat == "excel"`). Báo cáo theo kỳ in "lập lúc …" ở đầu trang.
- Thêm một dòng vào `ThucDon.DaLam` trong `Nen/ThucDon.cs` để trang hiện trên thực đơn. **Đặt đúng thứ tự mã** để đỡ xung đột khi gộp.
- Ngày, tiền, số: dùng `HienThi.Ngay`, `HienThi.Tien`, `HienThi.So`.
- Quy ước giao diện (06_giao_dien 5.2, đã làm sẵn, chỉ cần dùng đúng):
  - hàng nút cuối form: `<partial name="_NutLenh" model="new NutLenh()" />` (Lưu · Hủy · Thoát; Ctrl+S, Esc tự có); form chỉ xem: `new NutLenh(null)`;
  - mỗi ô nhập có nhãn `<label for="…">`; ô trong bảng không có nhãn thì thêm `aria-label`; trường có quy tắc định dạng thêm `title="…"` (chú thích khi trỏ chuột, cũng là câu báo khi nhập sai `pattern`);
  - trường bắt buộc: chỉ cần nhãn có lớp `bat-buoc` (hiện dấu `*`), trình duyệt tự chặn để trống;
  - ô nhập mã học viên: `list="ds-hoc-vien"` và thêm `<partial name="_GoiYHocVien" />` cuối trang;
  - danh sách có thể dài: `.LayTrangAsync(trang, so)` (`Nen/PhanTrang.cs`) và `<partial name="_PhanTrang" model="Model.Trang" />`;
  - chỉ dùng màu có sẵn (giao diện v2: tím mực cho nút, đỏ `canh-bao` cho cảnh báo); không thêm màu riêng;
  - bảng điểm: điểm `HienThi.Diem(x)` (dấu phẩy) trong ô `class="so diem"`, xếp loại `<span class="@HienThi.Xep(xl)">`, dòng không đạt `class="khong-dat"` (mẫu: `HocTap/TongKet.cshtml`).

**Xong một việc khi**
1. `dotnet build` không lỗi; `dotnet test demo/KiemThu` đạt hết (hiện 80/80). Quy tắc có tính toán thì thêm test vào `demo/KiemThu/KiemThuQuyTac.cs`.
2. `python demo/sinh_ma.py` báo "đạt" (kiểm tra mã module, mã màn hình, mã lỗi).
3. Đăng nhập **đủ 8 vai trò** mở trang: vai trò có quyền thấy đúng phạm vi, vai trò không quyền bị chuyển về "Không có quyền".
4. Số liệu trên trang khớp truy vấn SQL trực tiếp (ghi câu truy vấn đã dùng vào mô tả pull request).
5. Thêm dòng ca kiểm thử của mình vào bảng `phan_tich/05_cai_dat.md` mục 4.2 (đúng mã TC được giao ở bảng trên).
6. Chạy lại `csdl/chay.ps1` trước khi commit (thử xong dữ liệu đã bị đổi).

**Nhánh và pull request** (theo `phan_tich/06_bao_tri.md` mục 3.1)
- Nhánh `them/<mô-tả>` cho màn hình mới, `sua/<mô-tả>` cho sửa lỗi; mỗi việc (A1, H1…) một pull request vào `main`, Huynh duyệt.
- Mô tả pull request ghi: mã việc, màn hình, kết quả 6 bước "Xong khi" ở trên.
- Hai chỗ dễ xung đột: dòng trong `ThucDon.DaLam` và bảng ca kiểm thử ở `05_cai_dat.md`. Trước khi mở pull request: `git pull origin main`, sửa xung đột (giữ cả hai dòng), chạy lại bước 1–2.

---

## 1. Nguyễn Gia Ân – phân hệ 2.0

### A1. R2.3a Lịch học của học viên, R2.3b Lịch dạy, danh sách lớp (module 2.3)
- **Vì sao:** học viên, phụ huynh, giáo viên hiện **chưa xem được lịch**. Khảo sát (`phan_tich/02_thu_thap.md` mục 7): 71% học viên biết lịch qua giáo viên hoặc Zalo. Phỏng vấn giáo viên (mục 6.3): muốn xem danh sách lớp kèm số buổi vắng của từng học viên.
- **Đặc tả:** `thiet_ke/06_giao_dien.md` mục 4 (dòng R2.3a, R2.3b); nguồn dữ liệu `BuoiHoc`, `LopHoc`, `DangKy`, `HocVien`, `v_ChuyenCan`.
- **Làm:** một trang `Pages/LopHoc/LichHoc.cshtml(.cs)`, `ViewData["Ma"] = "R2.3a, R2.3b"`, `[Quyen("2.3")]`.
  - Học viên: các buổi của lớp mình đang học (ngày, giờ, phòng, giáo viên, buổi học bù / nghỉ). Phụ huynh: của các con. Nhân viên có quyền X: nhập mã học viên để xem.
  - Giáo viên: lịch dạy của mình, **tính cả buổi dạy thay** (`PhamVi.LopCuaGiaoVien`), và danh sách học viên từng lớp kèm số buổi vắng, tỷ lệ chuyên cần.
  - Mặc định từ hôm nay đến 4 tuần sau; có ô chọn khoảng ngày. In được (bên ngoài: có tên trung tâm như R4.4).
  - Không sửa `XepLich.cshtml.cs` (F2.3 vẫn chặn HV, PH, GV).
- **Thử:** hv0480 (đổi mật khẩu lần đầu rồi xem), gv023, ph0301, nv002; tài khoản kế toán nv004 bị chặn (module 2.3 kế toán không có quyền).
- **Ca kiểm thử:** TC35 (R2.3a), TC36 (R2.3b).

### A2. Sửa câu "và email" ở GĐ1
- `phan_tich/01_ke_hoach.md` dòng "Gửi SMS/Zalo cần tích hợp dịch vụ bên ngoài nên bản đầu chỉ dùng thông báo trong hệ thống và email." Demo **không gửi email**; `06_bao_tri.md` mục 5 xếp email vào hướng phát triển. Sửa thành: bản đầu chỉ dùng thông báo trong hệ thống (R5.2); email, SMS/Zalo để hướng phát triển (06_bao_tri mục 5).
- Rà thêm các chỗ khác hứa gửi email cho bản đầu (tìm chữ "email" trong `phan_tich/`, `thiet_ke/`), sửa cho khớp.

---

## 2. Vũ Văn Hùng – phân hệ 3.0 và CSDL

### H1. R3.1 Bảng học phí phải thu (module 3.1)
- **Đặc tả:** `06_giao_dien.md` mục 4 dòng R3.1: người nhận Kế toán, nguồn `HocPhi`, `DotHocPhi`, `ApDungUuDai`, `UuDai`, `DangKy`.
- **Hiện có:** `Pages/HocPhi/KhoanPhaiThu` là F3.1 (chỉ các đăng ký **chờ lập** học phí). Chưa có bảng toàn bộ khoản phải thu.
- **Làm:** `Pages/HocPhi/HocPhiPhaiThu.cshtml(.cs)`, `ViewData["Ma"] = "R3.1"`, `[Quyen("3.1")]`. Mỗi dòng: học viên, lớp, học phí gốc, ưu đãi áp dụng (tên, %), số phải nộp, từng đợt (số tiền, hạn, đã thu). Lọc theo lớp, theo mã học viên; dòng tổng; xuất Excel. Học viên, phụ huynh (quyền x) chỉ thấy khoản của mình. Chép cách lọc và Excel của `HocPhi/CongNo`.
- **Thử:** nv004 (xem hết), hv0412, ph0301 (chỉ của mình), nv002 bị chặn. Tổng số phải nộp khớp `SELECT SUM(SoTienPhaiNop) FROM HocPhi` (đối chiếu tên cột thật trong `csdl/schema.sql`).
- **Ca kiểm thử:** TC37.

### H2. Ghi phiên bản CSDL vào bảng ThamSo (06_bao_tri mục 3.3)
- **Vì sao:** `06_bao_tri.md` mục 3.3 viết "bảng ThamSo lưu thêm số của tệp nâng cấp cuối cùng đã chạy", nhưng chưa có dòng này và `csdl/nang_cap/001_tai_khoan_b2_b4.sql` không cập nhật gì.
- **Làm:**
  1. `csdl/sinh_du_lieu.py`: thêm dòng ThamSo `PHIEN_BAN_CSDL` (giá trị 1 = đã gồm tệp 001), sinh lại `seed.sql`. Kiểm tra CHECK của bảng ThamSo trong `schema.sql` cho phép giá trị và đơn vị này.
  2. `csdl/nang_cap/002_phien_ban_csdl.sql`: thêm dòng `PHIEN_BAN_CSDL` nếu chưa có, đặt = 2. Ghi quy ước ở đầu tệp: mỗi tệp nâng cấp sau kết thúc bằng `UPDATE ThamSo SET GiaTri = <số tệp> WHERE MaThamSo = 'PHIEN_BAN_CSDL'`.
  3. Trang F5.1b (`Pages/HeThong/ThamSo`) **không cho sửa** dòng này (chỉ hiện, không có ô nhập).
  4. Cập nhật số dòng mẫu ThamSo ở `thiet_ke/04_csdl.md` (14 → 15), chạy `python csdl/sinh_mo_ta.py`, `csdl/chay.ps1` vẫn 24/24.
- **Ca kiểm thử:** TC38 (chạy 002 trên CSDL đã có dữ liệu: thêm đúng 1 dòng, chạy lại lần 2 không lỗi; F5.1b không sửa được).

---

## 3. Nguyễn Văn Luận – phân hệ 4.0

### L1. R4.1 Chuyên cần của học viên (module 4.1)
- **Vì sao:** 93% phụ huynh muốn biết khi con vắng (02_thu_thap mục 7). Hiện chỉ có thông báo tự động, chưa có trang xem chi tiết.
- **Đặc tả:** `06_giao_dien.md` mục 4 dòng R4.1: bên ngoài, nguồn `v_ChuyenCan`, `DiemDanh`, `BuoiHoc`. Quy tắc QT12 (vắng quá ngưỡng `NGUONG_CANH_BAO_VANG` trong ThamSo thì cảnh báo).
- **Làm:** `Pages/HocTap/ChuyenCan.cshtml(.cs)`, `ViewData["Ma"] = "R4.1"`, `[Quyen("4.1")]`. Mỗi lớp: tỷ lệ chuyên cần, số buổi có mặt / vắng có phép / vắng không phép, dòng cảnh báo nếu vượt ngưỡng; bấm vào lớp xem từng buổi (ngày, trạng thái, ghi chú). Học viên xem của mình, phụ huynh xem các con, nhân viên có quyền X nhập mã học viên. Không sửa F4.1 (`DiemDanh` vẫn chặn HV, PH).
- **Thử:** hv0480 (TOE-2611, 18 buổi đã dạy), ph0320, hv0412 (lớp đã kết thúc); tỷ lệ khớp `v_ChuyenCan`. Kế toán nv004 bị chặn.
- **Ca kiểm thử:** TC39.

### L2. R4.3b Phiếu báo kết quả học tập (module 4.3)
- **Đặc tả:** `06_giao_dien.md` mục 4 dòng R4.3b: bên ngoài, nguồn `KetQua`, `Diem`, `HocVien`, `LopHoc`. Mẫu in phải **trùng chứng từ mẫu "Bảng điểm lớp"** ở `phan_tich/02_thu_thap.md` mục 4.2.4 (yêu cầu kiểm thử chấp nhận ở `06_giao_dien.md` mục 7).
- **Làm:** `Pages/HocTap/PhieuBaoKetQua.cshtml(.cs)`, `ViewData["Ma"] = "R4.3b"`, `[Quyen("4.3")]`. Chỉ lấy kết quả **đã duyệt** (`NgayDuyet` có giá trị). Một phiếu cho một học viên – một lớp: điểm chuyên cần, giữa kỳ, cuối kỳ, điểm tổng kết, xếp loại, Đạt / Không đạt, nhận xét, người duyệt, ngày duyệt. Có tên trung tâm, in một trang A4 (chép khung in của `HocTap/ChungNhan`). Học viên, phụ huynh, giáo viên chỉ thấy phạm vi của mình.
- **Thử:** hv0412 (3 lớp Đạt), hv0398 (Không đạt), ph0301, gv023; lớp TOE-2611 chưa duyệt thì báo "chưa có kết quả". So bản in với chứng từ mẫu 4.2.4.
- **Ca kiểm thử:** TC40.

---

## 4. Trần Thu Thủy – báo cáo phân hệ 5.0

### T1. R5.3a Báo cáo tuyển sinh (module 5.3)
- **Vì sao:** Giám đốc yêu cầu báo cáo tuyển sinh theo khóa trong phỏng vấn (02_thu_thap mục 6.3); view đã có, chỉ thiếu trang.
- **Đặc tả:** `06_giao_dien.md` mục 4 dòng R5.3a: nội bộ, theo kỳ, nguồn `v_TuyenSinh`, `KhoaHoc`. Học viên **mới** = đăng ký đầu tiên của học viên (cột `NgayDangKyDau`, `Nam`, `Thang`, `MaKH` của view).
- **Làm:** `Pages/BaoCao/TuyenSinh.cshtml(.cs)`, `ViewData["Ma"] = "F5.3, R5.3a"`, `[Quyen("5.3")]`. Chọn từ ngày – đến ngày; bảng học viên mới theo tháng × khóa học, dòng tổng; **so với kỳ trước cùng độ dài** (chép cách tính `KyTruoc` của `BaoCao/HocTap.cshtml.cs`); "lập lúc …"; xuất Excel, in.
- **Thử:** nv001, nv002, nv005 xem được; nv004, gv023, hv0412 bị chặn. Tổng khớp `SELECT COUNT(*) FROM v_TuyenSinh WHERE NgayDangKyDau BETWEEN … AND …`.
- **Ca kiểm thử:** TC41.

### T2. Sửa R5.4b: đếm buổi dạy theo ngày, thêm xuất Excel
- **Lỗi:** `Pages/BaoCao/HocTap.cshtml.cs` đếm "Buổi đã dạy trong kỳ" từ `v_GiangDay` (gom theo **tháng**), nên chọn kỳ 15/07 – 31/12 vẫn đếm cả các buổi 01 – 14/07.
- **Sửa:** đếm thẳng từ `BuoiHoc` với `TrangThai = "Đã dạy"` và `Ngay` trong kỳ, gom theo `MaGV` (lấy họ tên từ `GiaoVien`). Thêm xuất Excel cho bảng R5.4b (hiện chỉ R5.4a xuất được), ví dụ `?xuat=excel-gv`.
- **Thử:** kỳ 15/07/2026 – 31/12/2026, so số buổi của 1 giáo viên với `SELECT COUNT(*) FROM BuoiHoc WHERE MaGV = … AND TrangThai = N'Đã dạy' AND Ngay BETWEEN …`.
- **Ca kiểm thử:** sửa dòng TC32 ở `05_cai_dat.md` (thêm kỳ 15/07 – 31/12 và Excel R5.4b).

---

## 5. Trần Minh Quân – nền, phân quyền, bảo trì

Q1 (trang lỗi tiếng Việt, bắt lỗi sao lưu) và Q2 (lưu nháp theo tài khoản) **đã làm** trong đợt sửa giao diện ngày 11/10 (ca TC33, TC34 đã cập nhật).

### Q3. Script kiểm tra phân quyền 8 vai trò (làm sau cùng, khi các trang mới đã gộp)
- **Vì sao:** "Phân quyền: đạt" ở `05_cai_dat.md` mục 4.1 đang kiểm tra bằng tay (curl) mỗi lần, không lưu lại được.
- **Làm:** `demo/kiem_tra_quyen.py` (hoặc `.ps1`): đăng nhập 8 tài khoản mẫu (lấy `__RequestVerificationToken` từ /DangNhap; dùng hv0412 thay hv0480 vì hv0480 bị bắt đổi mật khẩu); với mỗi vai trò:
  - mọi liên kết trên thực đơn chính (trang `/`) phải mở được (200);
  - mọi trang trong `ThucDon.DaLam` không có trên thực đơn: vai trò **không có quyền nào** ở module đó (ma trận `Nen/PhanQuyen.g.cs`) phải bị chuyển về /KhongCoQuyen; vai trò có quyền xem thì được mở (vd Giám đốc xem F2.3).
  - In bảng vai trò × trang, cuối cùng "đạt" / danh sách sai.
- **Ca kiểm thử:** TC42; ghi lệnh chạy vào `05_cai_dat.md` mục 4.1 dòng "Phân quyền".

### Q4. Sửa `phan_tich/06_bao_tri.md` cho khớp hiện trạng
- Mục 2, bước 5: `dotnet test (38 ca)` → số ca hiện tại (80, cộng số test mọi người thêm).
- Mục 1, dòng "Phòng ngừa": đếm khóa đăng nhập trong CSDL, bắt đổi mật khẩu lần đầu đã làm (B2, B4) → ghi "(đã làm, B2, B4)".
- Mục 5, hướng SMS/Zalo: bảng `ThongBao` **đã có** cột `TrangThaiGui` → chỉ cần thêm cột kênh gửi.
- Mục 3.3: thống nhất câu về phiên bản CSDL với việc H2 của Hùng (gộp sau H2).

### Q5. Thử giao diện trên máy thật
- Giao diện mới mới được thử trên trình duyệt máy tính và giả lập điện thoại. Cần thử: điện thoại thật (Android, iPhone) cho F4.1 điểm danh, F4.2 nhập điểm; in thật ra giấy hoặc "Lưu PDF" cho R3.2 phiếu thu 2 liên và R4.4 giấy chứng nhận (chữ Times New Roman, không mất viền bảng, không cắt trang giữa một liên).
- Ghi kết quả vào dòng TC43 ở `05_cai_dat.md`; lỗi tìm thấy thì sửa ở `wwwroot/css/site.css` (mục `@media print`).

---

## 6. Hoàng Văn Huynh – điều phối

1. Trước khi giao: đẩy các commit đang có trên `main` máy mình (code bổ sung, sửa giao diện, giao diện mới) lên GitHub (qua nhánh + pull request đúng quy trình 06_bao_tri), báo nhóm `git pull`.
2. Duyệt từng pull request theo mục 0 "Xong khi"; gộp lần lượt, nhắc người sau `git pull` khi có xung đột ở `ThucDon.cs`, `05_cai_dat.md`.
3. Gộp xong tất cả: chạy `python demo/sinh_ma.py`, `python so_do/src/giao_dien.py`, `dotnet test`, `csdl/chay.ps1`, script Q3; cập nhật KE_HOACH.
