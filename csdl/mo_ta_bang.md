# Mô tả các tệp dữ liệu (sinh tự động từ csdl/schema.sql – không sửa tay)

Chạy `python csdl/sinh_mo_ta.py` để kiểm tra lại với thiết kế 3.1 và sinh lại tệp này. Cột *Ý nghĩa* lấy từ bảng thực thể của 3.1 (`thiet_ke/01_thuc_the.md`).

## Bảng 1. HocVien – Học viên (kho D1, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; NOT NULL | Mã học viên. Hệ thống cấp, dạng HVxxxx (vd HV0412) |
| HoTen | NVARCHAR(100) | NOT NULL | Họ và tên |
| NgaySinh | DATE | NOT NULL | Ngày sinh. Dùng để kiểm tra trùng và tính tuổi (S) |
| GioiTinh | NVARCHAR(3) | NOT NULL; CHECK (GioiTinh IN (N'Nam', N'Nữ')) | Giới tính. [Nam \| Nữ] |
| DienThoai | VARCHAR(10) | NOT NULL; CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%') | Điện thoại. 10 chữ số. Bộ (Điện thoại, Ngày sinh) dùng để kiểm tra trùng hồ sơ (1.1) |
| Email | VARCHAR(100) | NULL; CHECK (Email LIKE '%_@_%._%') | Email |
| DiaChi | NVARCHAR(200) | NULL | Địa chỉ. Để một trường, vì không cần thống kê theo phường, quận |
| TrinhDoDauVao | VARCHAR(2) | NULL; CHECK (TrinhDoDauVao IN ('A1', 'A2', 'B1', 'B2', 'C1', 'C2')) | Trình độ đầu vào. [A1 \| A2 \| B1 \| B2 \| C1 \| C2] (CEFR). Phải có trước khi đăng ký học (2.2) |
| NgayKiemTra | DATE | NULL | Ngày kiểm tra. Ngày kiểm tra đầu vào gần nhất. Bắt buộc khi có Trình độ |
| NgayTiepNhan | DATE | NOT NULL; mặc định CAST(GETDATE() AS DATE) | Ngày tiếp nhận. Ngày lập hồ sơ |
| TrangThai | NVARCHAR(20) | NOT NULL; mặc định N'Hoạt động'; CHECK (TrangThai IN (N'Hoạt động', N'Ngừng')) | Trạng thái. [Hoạt động \| Ngừng]. Hồ sơ "Ngừng" không bị xóa (04_dac_ta mục 1.1) |

Ràng buộc trên nhiều trường: CHECK ((TrinhDoDauVao IS NULL AND NgayKiemTra IS NULL) OR (TrinhDoDauVao IS NOT NULL AND NgayKiemTra IS NOT NULL)).

## Bảng 2. PhuHuynh – Phụ huynh (kho D1, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaPH | VARCHAR(10) | PK; NOT NULL | Mã phụ huynh. Dạng PHxxxx |
| HoTen | NVARCHAR(100) | NOT NULL | Họ và tên |
| DienThoai | VARCHAR(10) | NOT NULL; CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%') | Điện thoại. Dùng để nhận thông báo (5.2) |
| Email | VARCHAR(100) | NULL; CHECK (Email LIKE '%_@_%._%') | Email |

## Bảng 3. HocVienPhuHuynh – Học viên – Phụ huynh (kho D1, thực thể quan hệ)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → HocVien; NOT NULL | Mã học viên |
| MaPH | VARCHAR(10) | PK; FK → PhuHuynh; NOT NULL | Mã phụ huynh |
| QuanHe | NVARCHAR(20) | NOT NULL; CHECK (QuanHe IN (N'Bố', N'Mẹ', N'Người giám hộ')) | Quan hệ. [Bố \| Mẹ \| Người giám hộ] |

## Bảng 4. GiaoVien – Giáo viên (kho D1, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaGV | VARCHAR(10) | PK; NOT NULL | Mã giáo viên. Dạng GVxxx |
| HoTen | NVARCHAR(100) | NOT NULL | Họ và tên |
| NgaySinh | DATE | NOT NULL | Ngày sinh |
| DienThoai | VARCHAR(10) | NOT NULL; UNIQUE; CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%') | Điện thoại. Trùng Điện thoại hoặc Email thì coi là trùng giáo viên (1.2) |
| Email | VARCHAR(100) | NOT NULL; UNIQUE; CHECK (Email LIKE '%_@_%._%') | Email |
| NgonNguDay | NVARCHAR(10) | NOT NULL; CHECK (NgonNguDay IN (N'Anh', N'Trung', N'Nhật', N'Hàn')) | Ngôn ngữ dạy. [Anh \| Trung \| Nhật \| Hàn]. Dùng ở 2.1: giáo viên phải dạy đúng ngôn ngữ của khóa |
| ChuyenMon | NVARCHAR(200) | NULL | Chuyên môn. Văn bản mô tả, vd "IELTS; tiếng Anh thiếu nhi" |
| BangCap | NVARCHAR(200) | NULL | Bằng cấp. Văn bản mô tả |
| TinhTrang | NVARCHAR(20) | NOT NULL; mặc định N'Đang dạy'; CHECK (TinhTrang IN (N'Đang dạy', N'Tạm nghỉ', N'Nghỉ việc')) | Tình trạng làm việc. [Đang dạy \| Tạm nghỉ \| Nghỉ việc] |

## Bảng 5. KhoaHoc – Khóa học (kho D1, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaKH | VARCHAR(10) | PK; NOT NULL | Mã khóa học. Viết tắt tên khóa, vd IEK, GT |
| TenKH | NVARCHAR(100) | NOT NULL | Tên khóa học. vd "IELTS Kids Foundation" |
| NgonNgu | NVARCHAR(10) | NOT NULL; CHECK (NgonNgu IN (N'Anh', N'Trung', N'Nhật', N'Hàn')) | Ngôn ngữ. Cùng miền với GiaoVien.NgonNguDay |
| TrinhDoDauVao | VARCHAR(2) | NOT NULL; CHECK (TrinhDoDauVao IN ('A1', 'A2', 'B1', 'B2', 'C1', 'C2')) | Trình độ đầu vào. CEFR |
| TrinhDoDauRa | VARCHAR(2) | NOT NULL; CHECK (TrinhDoDauRa IN ('A1', 'A2', 'B1', 'B2', 'C1', 'C2')) | Trình độ đầu ra. CEFR, cao hơn trình độ đầu vào |
| SoBuoi | SMALLINT | NOT NULL; CHECK (SoBuoi > 0) | Số buổi. > 0 |
| ThoiLuongBuoi | SMALLINT | NOT NULL; CHECK (ThoiLuongBuoi > 0) | Thời lượng buổi. Số phút, > 0 (vd 90) |
| HocPhi | DECIMAL(12, 0) | NOT NULL; CHECK (HocPhi > 0) | Học phí. Đồng, > 0. Là giá **hiện hành**, mỗi đăng ký sẽ chốt giá riêng ở HocPhi.HocPhiGoc |
| TSChuyenCan | TINYINT | NOT NULL | Trọng số chuyên cần. %, |
| TSGiuaKy | TINYINT | NOT NULL | Trọng số giữa kỳ. %, |
| TSCuoiKy | TINYINT | NOT NULL | Trọng số cuối kỳ. %. Tổng 3 trọng số = 100 (QT13) |
| MoTa | NVARCHAR(500) | NULL | Mô tả |
| TrangThai | NVARCHAR(20) | NOT NULL; mặc định N'Đang mở'; CHECK (TrangThai IN (N'Đang mở', N'Ngừng')) | Trạng thái. [Đang mở \| Ngừng] |

Ràng buộc trên nhiều trường: CHECK (TrinhDoDauRa > TrinhDoDauVao); CHECK (TSChuyenCan + TSGiuaKy + TSCuoiKy = 100).

## Bảng 6. PhongHoc – Phòng học (kho D1, thực thể xác thực)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaPhong | VARCHAR(10) | PK; NOT NULL | Mã phòng. vd P203 |
| TenPhong | NVARCHAR(50) | NOT NULL | Tên phòng |
| SucChua | SMALLINT | NOT NULL; CHECK (SucChua > 0) | Sức chứa. > 0 (QT06: sĩ số lớp không vượt sức chứa) |
| ThietBi | NVARCHAR(200) | NULL | Thiết bị. Văn bản mô tả |
| TinhTrang | NVARCHAR(20) | NOT NULL; mặc định N'Sẵn sàng'; CHECK (TinhTrang IN (N'Sẵn sàng', N'Bảo trì', N'Ngừng')) | Tình trạng. [Sẵn sàng \| Bảo trì \| Ngừng]. Chỉ phòng "Sẵn sàng" mới được xếp lịch |

## Bảng 7. LopHoc – Lớp học (kho D2, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaLop | VARCHAR(15) | PK; NOT NULL | Mã lớp. vd IEK-2609 |
| MaKH | VARCHAR(10) | FK → KhoaHoc; NOT NULL | Mã khóa học |
| MaGV | VARCHAR(10) | FK → GiaoVien; NOT NULL | Mã giáo viên phụ trách. Phân công ở 2.1 |
| NgayKhaiGiang | DATE | NOT NULL | Ngày khai giảng |
| SiSoToiDa | SMALLINT | NOT NULL; CHECK (SiSoToiDa > 0) | Sĩ số tối đa. ≤ tham số SISO_TOI_DA (20) và ≤ sức chứa phòng ở LichTuan (QT06) |
| TrangThai | NVARCHAR(20) | NOT NULL; mặc định N'Dự kiến'; CHECK (TrangThai IN (N'Dự kiến', N'Đang học', N'Kết thúc', N'Hủy')) | Trạng thái. [Dự kiến \| Đang học \| Kết thúc \| Hủy] |

## Bảng 8. LichTuan – Lịch tuần (kho D2, thực thể quan hệ (Lớp học – Phòng học))

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaLop | VARCHAR(15) | PK; FK → LopHoc; NOT NULL | Mã lớp |
| Thu | TINYINT | PK; NOT NULL; CHECK (Thu BETWEEN 2 AND 8) | Thứ. [2 … 7 \| 8] (8 = Chủ nhật) |
| GioBatDau | TIME(0) | NOT NULL | Giờ bắt đầu |
| MaPhong | VARCHAR(10) | FK → PhongHoc; NOT NULL | Mã phòng. Phòng mặc định của khung giờ này |

## Bảng 9. BuoiHoc – Buổi học (kho D2, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaLop | VARCHAR(15) | PK; FK → LopHoc; NOT NULL | Mã lớp |
| SoBuoi | SMALLINT | PK; NOT NULL; CHECK (SoBuoi > 0) | Số thứ tự buổi. 1, 2, 3… Buổi học bù lấy số tiếp theo |
| Ngay | DATE | NOT NULL | Ngày |
| GioBatDau | TIME(0) | NOT NULL | Giờ bắt đầu |
| GioKetThuc | TIME(0) | NOT NULL | Giờ kết thúc. > Giờ bắt đầu. **Lưu** riêng, vì buổi thi hoặc học bù có thể dài ngắn khác thời lượng chuẩn. Kiểm tra trùng lịch (QT07) dựa trên giờ này |
| MaPhong | VARCHAR(10) | FK → PhongHoc; NOT NULL | Mã phòng. Mặc định lấy từ LichTuan, có thể đổi |
| MaGV | VARCHAR(10) | FK → GiaoVien; NOT NULL | Mã giáo viên dạy. Mặc định là giáo viên phụ trách lớp, khác đi khi có người **dạy thay**. Vì vậy không phải thứ sinh |
| Loai | NVARCHAR(10) | NOT NULL; mặc định N'Thường'; CHECK (Loai IN (N'Thường', N'Học bù')) | Loại buổi. [Thường \| Học bù] |
| NoiDung | NVARCHAR(200) | NULL | Nội dung. Nội dung bài học (BT1) |
| TrangThai | NVARCHAR(20) | NOT NULL; mặc định N'Kế hoạch'; CHECK (TrangThai IN (N'Kế hoạch', N'Đã dạy', N'Hủy')) | Trạng thái. [Kế hoạch \| Đã dạy \| Hủy] |

Ràng buộc trên nhiều trường: CHECK (GioKetThuc > GioBatDau).

## Bảng 10. PhieuDangKy – Phiếu đăng ký (kho D2, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| SoPhieuDK | VARCHAR(15) | PK; NOT NULL | Số phiếu đăng ký. Dạng DKyyyy-xxxx (vd DK2026-0158) |
| NgayDK | DATE | NOT NULL | Ngày đăng ký |
| MaNV | VARCHAR(10) | FK → NhanVien; NOT NULL | Mã nhân viên tiếp nhận. Lấy từ phiên đăng nhập |

## Bảng 11. DangKy – Đăng ký học (kho D2, thực thể quan hệ (Học viên – Lớp học: "Học"))

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → HocVien; FK → DangKy; NOT NULL | Mã học viên |
| MaLop | VARCHAR(15) | PK; FK → LopHoc; NOT NULL | Mã lớp. Mỗi học viên chỉ có 1 đăng ký ở mỗi lớp (2.2 từ chối đăng ký trùng) |
| SoPhieuDK | VARCHAR(15) | FK → PhieuDangKy; NOT NULL | Số phiếu đăng ký. Đăng ký sinh ra do chuyển lớp thì giữ số phiếu của đăng ký gốc |
| TrangThai | NVARCHAR(20) | NOT NULL; mặc định N'Đã đăng ký'; CHECK (TrangThai IN (N'Đã đăng ký', N'Chuyển lớp', N'Bảo lưu', N'Nghỉ học')) | Trạng thái. [Đã đăng ký \| Chuyển lớp \| Bảo lưu \| Nghỉ học] |
| NgayThayDoi | DATE | NULL; CHECK (TrangThai = N'Đã đăng ký' OR NgayThayDoi IS NOT NULL) | Ngày thay đổi. Ngày chấp nhận chuyển lớp, bảo lưu hoặc nghỉ học (2.4) |
| LyDo | NVARCHAR(200) | NULL | Lý do. Lý do ghi trên đơn chuyển lớp, bảo lưu |
| HanBaoLuu | DATE | NULL; CHECK (TrangThai <> N'Bảo lưu' OR HanBaoLuu IS NOT NULL) | Hạn bảo lưu. = Ngày thay đổi + 6 tháng (QT09). Chỉ có giá trị khi Trạng thái = Bảo lưu |
| MaLopGoc | VARCHAR(15) | FK → DangKy; NULL | Mã lớp gốc. Cặp (MaHV, MaLopGoc) trỏ tới **đăng ký gốc** của cùng học viên, khi đăng ký này sinh ra do chuyển lớp hoặc học lại sau bảo lưu. Dùng để cộng *Đã thu* (04_dac_ta mục 4.5) |

Ràng buộc trên nhiều trường: UNIQUE (SoPhieuDK, MaLop); CHECK (MaLopGoc <> MaLop); FOREIGN KEY (MaHV, MaLopGoc) → DangKy.

## Bảng 12. UuDai – Ưu đãi (kho D3, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaUD | VARCHAR(15) | PK; NOT NULL | Mã ưu đãi. vd UD-HVCU |
| TenUD | NVARCHAR(100) | NOT NULL | Tên ưu đãi |
| LoaiUD | NVARCHAR(20) | NOT NULL; CHECK (LoaiUD IN (N'Học viên cũ', N'Đăng ký nhóm', N'Đóng một lần')) | Loại ưu đãi. [Học viên cũ \| Đăng ký nhóm \| Đóng một lần]. Ứng với điều kiện C1, C2, C3 trong bảng quyết định 3.1 |
| TyLeGiam | DECIMAL(5, 2) | NOT NULL; CHECK (TyLeGiam > 0 AND TyLeGiam <= 100) | Tỷ lệ giảm. %, trong khoảng 0–100 |
| NgayBatDau | DATE | NOT NULL | Ngày bắt đầu |
| NgayKetThuc | DATE | NOT NULL | Ngày kết thúc. ≥ Ngày bắt đầu. Chỉ áp dụng ưu đãi còn hiệu lực (QT04) |
| DieuKien | NVARCHAR(200) | NULL | Điều kiện áp dụng. Văn bản mô tả |

Ràng buộc trên nhiều trường: CHECK (NgayKetThuc >= NgayBatDau).

## Bảng 13. HocPhi – Học phí (kho D3, thực thể sự kiện (khoản phải thu của một đăng ký))

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → DangKy; NOT NULL | Mã học viên. Quan hệ 1–1 với DangKy |
| MaLop | VARCHAR(15) | PK; FK → DangKy; NOT NULL | Mã lớp |
| HocPhiGoc | DECIMAL(12, 0) | NOT NULL; CHECK (HocPhiGoc > 0) | Học phí gốc. = KhoaHoc.HocPhi tại ngày lập |
| TyLeGiam | DECIMAL(5, 2) | NOT NULL; mặc định 0; CHECK (TyLeGiam BETWEEN 0 AND 100) | Tỷ lệ giảm. Tính theo bảng quyết định 3.1, tối đa 15% (QT04) |
| SoDot | TINYINT | NOT NULL; CHECK (SoDot IN (1, 2)) | Số đợt. [1 \| 2]. Do học viên chọn (QT03, QT05) |
| NgayLap | DATE | NOT NULL; mặc định CAST(GETDATE() AS DATE) | Ngày lập |

Ràng buộc trên nhiều trường: FOREIGN KEY (MaHV, MaLop) → DangKy.

## Bảng 14. DotHocPhi – Đợt học phí (kho D3, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → HocPhi; NOT NULL | Mã học viên |
| MaLop | VARCHAR(15) | PK; FK → HocPhi; NOT NULL | Mã lớp |
| Dot | TINYINT | PK; NOT NULL; CHECK (Dot IN (1, 2)) | Đợt. [1 \| 2] |
| SoTien | DECIMAL(12, 0) | NOT NULL; CHECK (SoTien > 0) | Số tiền. Đợt 1 ≥ 50% Phải nộp (QT05) |
| HanDong | DATE | NOT NULL | Hạn đóng. Đợt 1: ngày khai giảng; đợt 2: ngày của buổi học giữa khóa |

Ràng buộc trên nhiều trường: FOREIGN KEY (MaHV, MaLop) → HocPhi.

## Bảng 15. ApDungUuDai – Áp dụng ưu đãi (kho D3, thực thể quan hệ (Học phí – Ưu đãi: "Được hưởng"))

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → HocPhi; NOT NULL | Mã học viên |
| MaLop | VARCHAR(15) | PK; FK → HocPhi; NOT NULL | Mã lớp |
| MaUD | VARCHAR(15) | PK; FK → UuDai; NOT NULL | Mã ưu đãi |

Ràng buộc trên nhiều trường: FOREIGN KEY (MaHV, MaLop) → HocPhi.

## Bảng 16. PhieuThu – Phiếu thu (kho D3, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| SoPT | VARCHAR(15) | PK; NOT NULL | Số phiếu thu. Dạng PTyyyy-xxxx, tăng dần |
| NgayThu | DATE | NOT NULL | Ngày thu |
| NguoiNop | NVARCHAR(100) | NOT NULL | Người nộp. Họ tên người trực tiếp nộp: học viên hoặc phụ huynh |
| HinhThuc | NVARCHAR(20) | NOT NULL; CHECK (HinhThuc IN (N'Tiền mặt', N'Chuyển khoản')) | Hình thức. [Tiền mặt \| Chuyển khoản] |
| MaNV | VARCHAR(10) | FK → NhanVien; NOT NULL | Mã nhân viên thu. Lấy từ phiên đăng nhập |

## Bảng 17. ChiTietPhieuThu – Chi tiết phiếu thu (kho D3, thực thể quan hệ (Phiếu thu – Đợt học phí))

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| SoPT | VARCHAR(15) | PK; FK → PhieuThu; NOT NULL | Số phiếu thu |
| MaHV | VARCHAR(10) | PK; FK → DotHocPhi; NOT NULL | Mã học viên |
| MaLop | VARCHAR(15) | PK; FK → DotHocPhi; NOT NULL | Mã lớp |
| Dot | TINYINT | PK; FK → DotHocPhi; NOT NULL | Đợt |
| SoTien | DECIMAL(12, 0) | NOT NULL; CHECK (SoTien > 0) | Số tiền. > 0 và ≤ số còn nợ của đợt (04_dac_ta mục 1.4) |

Ràng buộc trên nhiều trường: FOREIGN KEY (MaHV, MaLop, Dot) → DotHocPhi.

## Bảng 18. DiemDanh – Điểm danh (kho D4, thực thể quan hệ (Đăng ký – Buổi học: "Tham dự"))

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → DangKy; NOT NULL | Mã học viên |
| MaLop | VARCHAR(15) | PK; FK → DangKy; FK → BuoiHoc; NOT NULL | Mã lớp. Dùng chung cho cả hai quan hệ. Vì vậy học viên chỉ được điểm danh ở buổi học của **chính lớp mình** |
| SoBuoi | SMALLINT | PK; FK → BuoiHoc; NOT NULL | Số thứ tự buổi |
| TrangThai | CHAR(1) | NOT NULL; CHECK (TrangThai IN ('x', 'M', 'P', 'K')) | Trạng thái. [x \| M \| P \| K] = có mặt, đi muộn, vắng có phép, vắng không phép (QT11) |
| GhiChu | NVARCHAR(200) | NULL | Ghi chú |

Ràng buộc trên nhiều trường: FOREIGN KEY (MaHV, MaLop) → DangKy; FOREIGN KEY (MaLop, SoBuoi) → BuoiHoc.

## Bảng 19. Diem – Điểm thành phần (kho D4, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → DangKy; NOT NULL | Mã học viên |
| MaLop | VARCHAR(15) | PK; FK → DangKy; NOT NULL | Mã lớp |
| LoaiDiem | NVARCHAR(10) | PK; NOT NULL; CHECK (LoaiDiem IN (N'Giữa kỳ', N'Cuối kỳ')) | Loại điểm. [Giữa kỳ \| Cuối kỳ] |
| Diem | DECIMAL(3, 1) | NOT NULL; CHECK (Diem BETWEEN 0 AND 10) | Điểm. 0 ≤ Điểm ≤ 10 |
| NhanXet | NVARCHAR(500) | NULL | Nhận xét |

Ràng buộc trên nhiều trường: FOREIGN KEY (MaHV, MaLop) → DangKy.

## Bảng 20. KetQua – Kết quả học tập (kho D4, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaHV | VARCHAR(10) | PK; FK → DangKy; NOT NULL | Mã học viên. Quan hệ 1–1 với DangKy (không bắt buộc phải có) |
| MaLop | VARCHAR(15) | PK; FK → DangKy; NOT NULL | Mã lớp |
| DiemTongKet | DECIMAL(3, 1) | NOT NULL; CHECK (DiemTongKet BETWEEN 0 AND 10) | Điểm tổng kết. 0–10, làm tròn 1 chữ số thập phân (QT13) |
| XepLoai | NVARCHAR(20) | NOT NULL; CHECK (XepLoai IN (N'Giỏi', N'Khá', N'Trung bình', N'Không đạt')) | Xếp loại. [Giỏi \| Khá \| Trung bình \| Không đạt] (QT14) |
| KetQua | NVARCHAR(10) | NOT NULL; CHECK (KetQua IN (N'Đạt', N'Không đạt')) | Kết quả. [Đạt \| Không đạt] (QT15) |
| MaNVDuyet | VARCHAR(10) | FK → NhanVien; NULL | Người duyệt. QL đào tạo duyệt. Rỗng = chưa duyệt |
| NgayDuyet | DATE | NULL | Ngày duyệt |

Ràng buộc trên nhiều trường: CHECK (NOT (KetQua = N'Đạt' AND XepLoai = N'Không đạt')); CHECK ((MaNVDuyet IS NULL AND NgayDuyet IS NULL) OR (MaNVDuyet IS NOT NULL AND NgayDuyet IS NOT NULL)); FOREIGN KEY (MaHV, MaLop) → DangKy.

## Bảng 21. ChungNhan – Chứng nhận (kho D4, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| SoCN | VARCHAR(15) | PK; NOT NULL | Số chứng nhận. Dạng CNyyyy-xxxx, tăng dần theo năm |
| MaHV | VARCHAR(10) | FK → KetQua; NOT NULL | Mã học viên. Cặp (MaHV, MaLop) là duy nhất: mỗi kết quả có tối đa 1 chứng nhận |
| MaLop | VARCHAR(15) | FK → KetQua; NOT NULL | Mã lớp |
| NgayCap | DATE | NOT NULL | Ngày cấp |

Ràng buộc trên nhiều trường: UNIQUE (MaHV, MaLop); FOREIGN KEY (MaHV, MaLop) → KetQua.

## Bảng 22. NhanVien – Nhân viên (kho D5, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaNV | VARCHAR(10) | PK; NOT NULL | Mã nhân viên. Dạng NVxxx |
| HoTen | NVARCHAR(100) | NOT NULL | Họ và tên |
| BoPhan | NVARCHAR(30) | NOT NULL; CHECK (BoPhan IN (N'Ban giám đốc', N'Đào tạo', N'Tuyển sinh – CSHV', N'Kế toán', N'CNTT')) | Bộ phận. [Ban giám đốc \| Đào tạo \| Tuyển sinh – CSHV \| Kế toán \| CNTT] (02_thu_thap mục 2.1) |
| ChucVu | NVARCHAR(50) | NOT NULL | Chức vụ |
| DienThoai | VARCHAR(10) | NOT NULL; CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%') | Điện thoại |
| Email | VARCHAR(100) | NOT NULL; CHECK (Email LIKE '%_@_%._%') | Email |
| TrangThai | NVARCHAR(20) | NOT NULL; mặc định N'Đang làm'; CHECK (TrangThai IN (N'Đang làm', N'Nghỉ việc')) | Trạng thái. [Đang làm \| Nghỉ việc] |

## Bảng 23. TaiKhoan – Tài khoản (kho D5, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| TenDangNhap | VARCHAR(50) | PK; NOT NULL | Tên đăng nhập. Duy nhất |
| MatKhauBam | VARCHAR(255) | NOT NULL | Mật khẩu (đã băm). Chỉ lưu chuỗi băm, không lưu mật khẩu gốc |
| VaiTro | NVARCHAR(20) | NOT NULL; CHECK (VaiTro IN (N'Học viên', N'Phụ huynh', N'Giáo viên', N'NV tuyển sinh', N'NV kế toán', N'QL đào tạo', N'Giám đốc', N'Quản trị viên')) | Vai trò. [Học viên \| Phụ huynh \| Giáo viên \| NV tuyển sinh \| NV kế toán \| QL đào tạo \| Giám đốc \| Quản trị viên]. Quyền của từng vai trò xem ở 3.5 |
| MaNV | VARCHAR(10) | FK → NhanVien; NULL | Mã nhân viên |
| MaGV | VARCHAR(10) | FK → GiaoVien; NULL | Mã giáo viên |
| MaHV | VARCHAR(10) | FK → HocVien; NULL | Mã học viên |
| MaPH | VARCHAR(10) | FK → PhuHuynh; NULL | Mã phụ huynh |
| TrangThai | NVARCHAR(20) | NOT NULL; mặc định N'Hoạt động'; CHECK (TrangThai IN (N'Hoạt động', N'Khóa')) | Trạng thái. [Hoạt động \| Khóa] |

Ràng buộc trên nhiều trường: CHECK ((VaiTro = N'Giáo viên' AND MaGV IS NOT NULL AND MaNV IS NULL AND MaHV IS NULL AND MaPH IS NULL) OR (VaiTro = N'Học viên' AND MaHV IS NOT NULL AND MaNV IS NULL AND MaGV IS NULL AND MaPH IS NULL) OR (VaiTro = N'Phụ huynh' AND MaPH IS NOT NULL AND MaNV IS NULL AND MaGV IS NULL AND MaHV IS NULL) OR (VaiTro IN (N'NV tuyển sinh', N'NV kế toán', N'QL đào tạo', N'Giám đốc', N'Quản trị viên') AND MaNV IS NOT NULL AND MaGV IS NULL AND MaHV IS NULL AND MaPH IS NULL)).

## Bảng 24. ThongBao – Thông báo (kho D5, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaTB | INT | PK; NOT NULL; tự tăng | Mã thông báo. Hệ thống cấp, tăng dần |
| Loai | NVARCHAR(20) | NOT NULL; CHECK (Loai IN (N'Lịch học', N'Học phí', N'Chuyên cần', N'Kết quả')) | Loại. [Lịch học \| Học phí \| Chuyên cần \| Kết quả] |
| MaHV | VARCHAR(10) | FK → HocVien; FK → HocVienPhuHuynh; NOT NULL | Mã học viên. Học viên được nói tới trong thông báo |
| MaPH | VARCHAR(10) | FK → PhuHuynh; FK → HocVienPhuHuynh; NULL | Mã phụ huynh. Có giá trị khi người nhận là phụ huynh. Rỗng nghĩa là gửi cho chính học viên |
| NoiDung | NVARCHAR(1000) | NOT NULL | Nội dung |
| ThoiGianGui | DATETIME2(0) | NOT NULL; mặc định SYSDATETIME() | Thời gian gửi |
| TrangThaiGui | NVARCHAR(10) | NOT NULL; mặc định N'Chờ gửi'; CHECK (TrangThaiGui IN (N'Chờ gửi', N'Đã gửi', N'Lỗi')) | Trạng thái gửi. [Chờ gửi \| Đã gửi \| Lỗi] |

Ràng buộc trên nhiều trường: FK_ThongBao_GiamHo: khóa ngoại bổ sung – phụ huynh nhận thông báo phải là người giám hộ của học viên đó (3.1 E24).

## Bảng 25. NhatKy – Nhật ký (kho D5, thực thể sự kiện)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaNK | BIGINT | PK; NOT NULL; tự tăng | Mã nhật ký. Hệ thống cấp, tăng dần. Bộ (Thời điểm, Tên đăng nhập) có thể trùng nên không dùng làm khóa |
| ThoiDiem | DATETIME2(0) | NOT NULL; mặc định SYSDATETIME() | Thời điểm |
| TenDangNhap | VARCHAR(50) | FK → TaiKhoan; NOT NULL | Tên đăng nhập |
| ThaoTac | NVARCHAR(20) | NOT NULL; CHECK (ThaoTac IN (N'Đăng nhập', N'Thêm', N'Sửa', N'Ngừng', N'Duyệt', N'In')) | Thao tác. [Đăng nhập \| Thêm \| Sửa \| Ngừng \| Duyệt \| In] |
| DoiTuong | VARCHAR(30) | NOT NULL | Đối tượng. Tên thực thể bị tác động, vd "DangKy" |
| MaDoiTuong | NVARCHAR(100) | NOT NULL | Mã đối tượng. Giá trị khóa của cá thể, vd "HV0412/IEK-2609" |

## Bảng 26. ThamSo – Tham số (kho D5, thực thể chức năng)

| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| MaThamSo | VARCHAR(30) | PK; NOT NULL | Mã tham số |
| TenThamSo | NVARCHAR(100) | NOT NULL | Tên tham số |
| GiaTri | DECIMAL(10, 2) | NOT NULL | Giá trị. Số |
| DonVi | NVARCHAR(20) | NOT NULL | Đơn vị |
| QuyTac | VARCHAR(20) | NULL | Quy tắc. Mã QTxx của quy tắc dùng tham số này |

## Chỉ mục

| Chỉ mục | Bảng | Trường | Mục đích |
|---|---|---|---|
| IX_HocVien_HoTen | HocVien | HoTen | 1.1, 2.2 tra cứu học viên theo tên |
| IX_HocVien_DienThoai | HocVien | DienThoai, NgaySinh | 1.1 kiểm tra trùng hồ sơ |
| IX_LopHoc_MaKH | LopHoc | MaKH | 3.4, 5.3 nhóm lớp theo khóa học |
| IX_LopHoc_MaGV | LopHoc | MaGV | 2.1 lớp do giáo viên phụ trách |
| IX_BuoiHoc_GiaoVien_Ngay | BuoiHoc | MaGV, Ngay | 2.3 kiểm tra trùng giáo viên (QT07) |
| IX_BuoiHoc_Phong_Ngay | BuoiHoc | MaPhong, Ngay | 2.3 kiểm tra trùng phòng (QT07) |
| IX_DangKy_MaLop | DangKy | MaLop, TrangThai | sĩ số lớp (2.1, 2.2, 5.3) |
| IX_PhieuThu_NgayThu | PhieuThu | NgayThu | 3.4 doanh thu theo kỳ |
| IX_ChiTietPhieuThu_Dot | ChiTietPhieuThu | MaHV, MaLop, Dot | số đã thu theo đợt (3.2, 3.3) |
| IX_DiemDanh_Buoi | DiemDanh | MaLop, SoBuoi | 4.1 điểm danh theo buổi |
| IX_ThongBao_MaHV | ThongBao | MaHV, ThoiGianGui | 5.2 thông báo của một học viên |
| IX_NhatKy_ThoiDiem | NhatKy | ThoiDiem | 5.1 xem nhật ký theo thời gian |
| UX_TaiKhoan_MaNV (UNIQUE) | TaiKhoan | MaNV khi MaNV IS NOT NULL | Quan hệ 1–1: mỗi chủ sở hữu tối đa một tài khoản |
| UX_TaiKhoan_MaGV (UNIQUE) | TaiKhoan | MaGV khi MaGV IS NOT NULL | Quan hệ 1–1: mỗi chủ sở hữu tối đa một tài khoản |
| UX_TaiKhoan_MaHV (UNIQUE) | TaiKhoan | MaHV khi MaHV IS NOT NULL | Quan hệ 1–1: mỗi chủ sở hữu tối đa một tài khoản |
| UX_TaiKhoan_MaPH (UNIQUE) | TaiKhoan | MaPH khi MaPH IS NOT NULL | Quan hệ 1–1: mỗi chủ sở hữu tối đa một tài khoản |
