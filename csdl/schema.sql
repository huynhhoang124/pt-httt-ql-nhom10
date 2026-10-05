/* =====================================================================
   CSDL vật lý – Hệ thống quản lý trung tâm ngoại ngữ (Nhóm 10)
   Thiết kế: thiet_ke/01_thuc_the.md (26 thực thể), 02_quan_he.md (29 quan hệ),
             03_chuan_hoa.md (đã kiểm tra 3NF). Mô tả từng tệp: csdl/mo_ta_bang.md.
   Hệ quản trị: Microsoft SQL Server 2019 trở lên.
   Chạy: powershell -File csdl/chay.ps1   (hoặc mở trong SSMS và chạy lần lượt
         schema.sql → views.sql → seed.sql → kiem_tra.sql)

   Quy ước:
   - Mỗi cột một dòng; mỗi ràng buộc một dòng, đặt tên PK_/FK_/UQ_/CK_/DF_.
   - CHECK chỉ dùng cho bất biến không đổi theo chính sách (miền giá trị, > 0,
     điểm 0–10, tổng trọng số 100%). Con số chính sách (sĩ số 8/20, 15% ưu đãi…)
     nằm ở bảng ThamSo và được kiểm tra ở tầng ứng dụng (3.1 – E26).
   - Không xóa cứng dữ liệu nghiệp vụ (chuyển Trạng thái = Ngừng), nên mọi khóa
     ngoại dùng NO ACTION.
   ===================================================================== */

USE master;
GO
IF DB_ID(N'TrungTamNgoaiNgu') IS NULL
    CREATE DATABASE TrungTamNgoaiNgu COLLATE Vietnamese_100_CI_AS;
GO
USE TrungTamNgoaiNgu;
GO

/* Chạy lại được: xóa view, hàm, bảng cũ theo thứ tự ngược phụ thuộc */
DROP VIEW IF EXISTS v_TuyenSinh, v_GiangDay, v_KetQuaTinh, v_ChuyenCan, v_SiSoLop,
                    v_DoanhThu, v_HocPhiDangKy, v_HocVienCu, v_ChuoiDangKy;
DROP FUNCTION IF EXISTS fn_CongNo, fn_ThamSo;
DROP TABLE IF EXISTS ThamSo, NhatKy, ThongBao, TaiKhoan, ChungNhan, KetQua, Diem, DiemDanh,
                     ChiTietPhieuThu, PhieuThu, ApDungUuDai, DotHocPhi, HocPhi, UuDai,
                     DangKy, PhieuDangKy, BuoiHoc, LichTuan, LopHoc, PhongHoc, KhoaHoc,
                     GiaoVien, HocVienPhuHuynh, PhuHuynh, HocVien, NhanVien;
GO

/* ---------------------------- Kho D1 – Hồ sơ ---------------------------- */

CREATE TABLE HocVien (
    MaHV            VARCHAR(10)     NOT NULL,
    HoTen           NVARCHAR(100)   NOT NULL,
    NgaySinh        DATE            NOT NULL,
    GioiTinh        NVARCHAR(3)     NOT NULL,
    DienThoai       VARCHAR(10)     NOT NULL,
    Email           VARCHAR(100)    NULL,
    DiaChi          NVARCHAR(200)   NULL,
    TrinhDoDauVao   VARCHAR(2)      NULL,
    NgayKiemTra     DATE            NULL,
    NgayTiepNhan    DATE            NOT NULL CONSTRAINT DF_HocVien_NgayTiepNhan DEFAULT (CAST(GETDATE() AS DATE)),
    TrangThai       NVARCHAR(20)    NOT NULL CONSTRAINT DF_HocVien_TrangThai DEFAULT (N'Hoạt động'),
    CONSTRAINT PK_HocVien PRIMARY KEY (MaHV),
    CONSTRAINT CK_HocVien_GioiTinh CHECK (GioiTinh IN (N'Nam', N'Nữ')),
    CONSTRAINT CK_HocVien_DienThoai CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_HocVien_Email CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_HocVien_TrinhDoDauVao CHECK (TrinhDoDauVao IN ('A1', 'A2', 'B1', 'B2', 'C1', 'C2')),
    CONSTRAINT CK_HocVien_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Ngừng')),
    CONSTRAINT CK_HocVien_KiemTra CHECK ((TrinhDoDauVao IS NULL AND NgayKiemTra IS NULL) OR (TrinhDoDauVao IS NOT NULL AND NgayKiemTra IS NOT NULL))
);

CREATE TABLE PhuHuynh (
    MaPH            VARCHAR(10)     NOT NULL,
    HoTen           NVARCHAR(100)   NOT NULL,
    DienThoai       VARCHAR(10)     NOT NULL,
    Email           VARCHAR(100)    NULL,
    CONSTRAINT PK_PhuHuynh PRIMARY KEY (MaPH),
    CONSTRAINT CK_PhuHuynh_DienThoai CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_PhuHuynh_Email CHECK (Email LIKE '%_@_%._%')
);

CREATE TABLE HocVienPhuHuynh (
    MaHV            VARCHAR(10)     NOT NULL,
    MaPH            VARCHAR(10)     NOT NULL,
    QuanHe          NVARCHAR(20)    NOT NULL,
    CONSTRAINT PK_HocVienPhuHuynh PRIMARY KEY (MaHV, MaPH),
    CONSTRAINT FK_HocVienPhuHuynh_HocVien FOREIGN KEY (MaHV) REFERENCES HocVien (MaHV),
    CONSTRAINT FK_HocVienPhuHuynh_PhuHuynh FOREIGN KEY (MaPH) REFERENCES PhuHuynh (MaPH),
    CONSTRAINT CK_HocVienPhuHuynh_QuanHe CHECK (QuanHe IN (N'Bố', N'Mẹ', N'Người giám hộ'))
);

CREATE TABLE GiaoVien (
    MaGV            VARCHAR(10)     NOT NULL,
    HoTen           NVARCHAR(100)   NOT NULL,
    NgaySinh        DATE            NOT NULL,
    DienThoai       VARCHAR(10)     NOT NULL,
    Email           VARCHAR(100)    NOT NULL,
    NgonNguDay      NVARCHAR(10)    NOT NULL,
    ChuyenMon       NVARCHAR(200)   NULL,
    BangCap         NVARCHAR(200)   NULL,
    TinhTrang       NVARCHAR(20)    NOT NULL CONSTRAINT DF_GiaoVien_TinhTrang DEFAULT (N'Đang dạy'),
    CONSTRAINT PK_GiaoVien PRIMARY KEY (MaGV),
    CONSTRAINT UQ_GiaoVien_DienThoai UNIQUE (DienThoai),
    CONSTRAINT UQ_GiaoVien_Email UNIQUE (Email),
    CONSTRAINT CK_GiaoVien_DienThoai CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_GiaoVien_Email CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_GiaoVien_NgonNguDay CHECK (NgonNguDay IN (N'Anh', N'Trung', N'Nhật', N'Hàn')),
    CONSTRAINT CK_GiaoVien_TinhTrang CHECK (TinhTrang IN (N'Đang dạy', N'Tạm nghỉ', N'Nghỉ việc'))
);

CREATE TABLE KhoaHoc (
    MaKH            VARCHAR(10)     NOT NULL,
    TenKH           NVARCHAR(100)   NOT NULL,
    NgonNgu         NVARCHAR(10)    NOT NULL,
    TrinhDoDauVao   VARCHAR(2)      NOT NULL,
    TrinhDoDauRa    VARCHAR(2)      NOT NULL,
    SoBuoi          SMALLINT        NOT NULL,
    ThoiLuongBuoi   SMALLINT        NOT NULL,
    HocPhi          DECIMAL(12, 0)  NOT NULL,
    TSChuyenCan     TINYINT         NOT NULL,
    TSGiuaKy        TINYINT         NOT NULL,
    TSCuoiKy        TINYINT         NOT NULL,
    MoTa            NVARCHAR(500)   NULL,
    TrangThai       NVARCHAR(20)    NOT NULL CONSTRAINT DF_KhoaHoc_TrangThai DEFAULT (N'Đang mở'),
    CONSTRAINT PK_KhoaHoc PRIMARY KEY (MaKH),
    CONSTRAINT CK_KhoaHoc_NgonNgu CHECK (NgonNgu IN (N'Anh', N'Trung', N'Nhật', N'Hàn')),
    CONSTRAINT CK_KhoaHoc_TrinhDoDauVao CHECK (TrinhDoDauVao IN ('A1', 'A2', 'B1', 'B2', 'C1', 'C2')),
    CONSTRAINT CK_KhoaHoc_TrinhDoDauRa CHECK (TrinhDoDauRa IN ('A1', 'A2', 'B1', 'B2', 'C1', 'C2')),
    CONSTRAINT CK_KhoaHoc_TrinhDo CHECK (TrinhDoDauRa > TrinhDoDauVao),
    CONSTRAINT CK_KhoaHoc_SoBuoi CHECK (SoBuoi > 0),
    CONSTRAINT CK_KhoaHoc_ThoiLuongBuoi CHECK (ThoiLuongBuoi > 0),
    CONSTRAINT CK_KhoaHoc_HocPhi CHECK (HocPhi > 0),
    CONSTRAINT CK_KhoaHoc_TrongSo CHECK (TSChuyenCan + TSGiuaKy + TSCuoiKy = 100),
    CONSTRAINT CK_KhoaHoc_TrangThai CHECK (TrangThai IN (N'Đang mở', N'Ngừng'))
);

CREATE TABLE PhongHoc (
    MaPhong         VARCHAR(10)     NOT NULL,
    TenPhong        NVARCHAR(50)    NOT NULL,
    SucChua         SMALLINT        NOT NULL,
    ThietBi         NVARCHAR(200)   NULL,
    TinhTrang       NVARCHAR(20)    NOT NULL CONSTRAINT DF_PhongHoc_TinhTrang DEFAULT (N'Sẵn sàng'),
    CONSTRAINT PK_PhongHoc PRIMARY KEY (MaPhong),
    CONSTRAINT CK_PhongHoc_SucChua CHECK (SucChua > 0),
    CONSTRAINT CK_PhongHoc_TinhTrang CHECK (TinhTrang IN (N'Sẵn sàng', N'Bảo trì', N'Ngừng'))
);

/* ------------------------- Kho D2 – Lớp và lịch học ------------------------- */

CREATE TABLE LopHoc (
    MaLop           VARCHAR(15)     NOT NULL,
    MaKH            VARCHAR(10)     NOT NULL,
    MaGV            VARCHAR(10)     NOT NULL,
    NgayKhaiGiang   DATE            NOT NULL,
    SiSoToiDa       SMALLINT        NOT NULL,
    TrangThai       NVARCHAR(20)    NOT NULL CONSTRAINT DF_LopHoc_TrangThai DEFAULT (N'Dự kiến'),
    CONSTRAINT PK_LopHoc PRIMARY KEY (MaLop),
    CONSTRAINT FK_LopHoc_KhoaHoc FOREIGN KEY (MaKH) REFERENCES KhoaHoc (MaKH),
    CONSTRAINT FK_LopHoc_GiaoVien FOREIGN KEY (MaGV) REFERENCES GiaoVien (MaGV),
    CONSTRAINT CK_LopHoc_SiSoToiDa CHECK (SiSoToiDa > 0),
    CONSTRAINT CK_LopHoc_TrangThai CHECK (TrangThai IN (N'Dự kiến', N'Đang học', N'Kết thúc', N'Hủy'))
);

CREATE TABLE LichTuan (
    MaLop           VARCHAR(15)     NOT NULL,
    Thu             TINYINT         NOT NULL,
    GioBatDau       TIME(0)         NOT NULL,
    MaPhong         VARCHAR(10)     NOT NULL,
    CONSTRAINT PK_LichTuan PRIMARY KEY (MaLop, Thu),
    CONSTRAINT FK_LichTuan_LopHoc FOREIGN KEY (MaLop) REFERENCES LopHoc (MaLop),
    CONSTRAINT FK_LichTuan_PhongHoc FOREIGN KEY (MaPhong) REFERENCES PhongHoc (MaPhong),
    CONSTRAINT CK_LichTuan_Thu CHECK (Thu BETWEEN 2 AND 8)
);

CREATE TABLE BuoiHoc (
    MaLop           VARCHAR(15)     NOT NULL,
    SoBuoi          SMALLINT        NOT NULL,
    Ngay            DATE            NOT NULL,
    GioBatDau       TIME(0)         NOT NULL,
    GioKetThuc      TIME(0)         NOT NULL,
    MaPhong         VARCHAR(10)     NOT NULL,
    MaGV            VARCHAR(10)     NOT NULL,
    Loai            NVARCHAR(10)    NOT NULL CONSTRAINT DF_BuoiHoc_Loai DEFAULT (N'Thường'),
    NoiDung         NVARCHAR(200)   NULL,
    TrangThai       NVARCHAR(20)    NOT NULL CONSTRAINT DF_BuoiHoc_TrangThai DEFAULT (N'Kế hoạch'),
    CONSTRAINT PK_BuoiHoc PRIMARY KEY (MaLop, SoBuoi),
    CONSTRAINT FK_BuoiHoc_LopHoc FOREIGN KEY (MaLop) REFERENCES LopHoc (MaLop),
    CONSTRAINT FK_BuoiHoc_PhongHoc FOREIGN KEY (MaPhong) REFERENCES PhongHoc (MaPhong),
    CONSTRAINT FK_BuoiHoc_GiaoVien FOREIGN KEY (MaGV) REFERENCES GiaoVien (MaGV),
    CONSTRAINT CK_BuoiHoc_SoBuoi CHECK (SoBuoi > 0),
    CONSTRAINT CK_BuoiHoc_Gio CHECK (GioKetThuc > GioBatDau),
    CONSTRAINT CK_BuoiHoc_Loai CHECK (Loai IN (N'Thường', N'Học bù')),
    CONSTRAINT CK_BuoiHoc_TrangThai CHECK (TrangThai IN (N'Kế hoạch', N'Đã dạy', N'Hủy'))
);

CREATE TABLE PhieuDangKy (
    SoPhieuDK       VARCHAR(15)     NOT NULL,
    NgayDK          DATE            NOT NULL,
    MaNV            VARCHAR(10)     NOT NULL,
    CONSTRAINT PK_PhieuDangKy PRIMARY KEY (SoPhieuDK)
);

CREATE TABLE DangKy (
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    SoPhieuDK       VARCHAR(15)     NOT NULL,
    TrangThai       NVARCHAR(20)    NOT NULL CONSTRAINT DF_DangKy_TrangThai DEFAULT (N'Đã đăng ký'),
    NgayThayDoi     DATE            NULL,
    LyDo            NVARCHAR(200)   NULL,
    HanBaoLuu       DATE            NULL,
    MaLopGoc        VARCHAR(15)     NULL,
    CONSTRAINT PK_DangKy PRIMARY KEY (MaHV, MaLop),
    CONSTRAINT UQ_DangKy_PhieuLop UNIQUE (SoPhieuDK, MaLop),
    CONSTRAINT FK_DangKy_HocVien FOREIGN KEY (MaHV) REFERENCES HocVien (MaHV),
    CONSTRAINT FK_DangKy_LopHoc FOREIGN KEY (MaLop) REFERENCES LopHoc (MaLop),
    CONSTRAINT FK_DangKy_PhieuDangKy FOREIGN KEY (SoPhieuDK) REFERENCES PhieuDangKy (SoPhieuDK),
    CONSTRAINT FK_DangKy_DangKyGoc FOREIGN KEY (MaHV, MaLopGoc) REFERENCES DangKy (MaHV, MaLop),
    CONSTRAINT CK_DangKy_TrangThai CHECK (TrangThai IN (N'Đã đăng ký', N'Chuyển lớp', N'Bảo lưu', N'Nghỉ học')),
    CONSTRAINT CK_DangKy_LopGoc CHECK (MaLopGoc <> MaLop),
    CONSTRAINT CK_DangKy_NgayThayDoi CHECK (TrangThai = N'Đã đăng ký' OR NgayThayDoi IS NOT NULL),
    CONSTRAINT CK_DangKy_HanBaoLuu CHECK (TrangThai <> N'Bảo lưu' OR HanBaoLuu IS NOT NULL)
);

/* ----------------------------- Kho D3 – Học phí ----------------------------- */

CREATE TABLE UuDai (
    MaUD            VARCHAR(15)     NOT NULL,
    TenUD           NVARCHAR(100)   NOT NULL,
    LoaiUD          NVARCHAR(20)    NOT NULL,
    TyLeGiam        DECIMAL(5, 2)   NOT NULL,
    NgayBatDau      DATE            NOT NULL,
    NgayKetThuc     DATE            NOT NULL,
    DieuKien        NVARCHAR(200)   NULL,
    CONSTRAINT PK_UuDai PRIMARY KEY (MaUD),
    CONSTRAINT CK_UuDai_LoaiUD CHECK (LoaiUD IN (N'Học viên cũ', N'Đăng ký nhóm', N'Đóng một lần')),
    CONSTRAINT CK_UuDai_TyLeGiam CHECK (TyLeGiam > 0 AND TyLeGiam <= 100),
    CONSTRAINT CK_UuDai_Ngay CHECK (NgayKetThuc >= NgayBatDau)
);

CREATE TABLE HocPhi (
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    HocPhiGoc       DECIMAL(12, 0)  NOT NULL,
    TyLeGiam        DECIMAL(5, 2)   NOT NULL CONSTRAINT DF_HocPhi_TyLeGiam DEFAULT (0),
    SoDot           TINYINT         NOT NULL,
    NgayLap         DATE            NOT NULL CONSTRAINT DF_HocPhi_NgayLap DEFAULT (CAST(GETDATE() AS DATE)),
    CONSTRAINT PK_HocPhi PRIMARY KEY (MaHV, MaLop),
    CONSTRAINT FK_HocPhi_DangKy FOREIGN KEY (MaHV, MaLop) REFERENCES DangKy (MaHV, MaLop),
    CONSTRAINT CK_HocPhi_HocPhiGoc CHECK (HocPhiGoc > 0),
    CONSTRAINT CK_HocPhi_TyLeGiam CHECK (TyLeGiam BETWEEN 0 AND 100),
    CONSTRAINT CK_HocPhi_SoDot CHECK (SoDot IN (1, 2))
);

CREATE TABLE DotHocPhi (
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    Dot             TINYINT         NOT NULL,
    SoTien          DECIMAL(12, 0)  NOT NULL,
    HanDong         DATE            NOT NULL,
    CONSTRAINT PK_DotHocPhi PRIMARY KEY (MaHV, MaLop, Dot),
    CONSTRAINT FK_DotHocPhi_HocPhi FOREIGN KEY (MaHV, MaLop) REFERENCES HocPhi (MaHV, MaLop),
    CONSTRAINT CK_DotHocPhi_Dot CHECK (Dot IN (1, 2)),
    CONSTRAINT CK_DotHocPhi_SoTien CHECK (SoTien > 0)
);

CREATE TABLE ApDungUuDai (
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    MaUD            VARCHAR(15)     NOT NULL,
    CONSTRAINT PK_ApDungUuDai PRIMARY KEY (MaHV, MaLop, MaUD),
    CONSTRAINT FK_ApDungUuDai_HocPhi FOREIGN KEY (MaHV, MaLop) REFERENCES HocPhi (MaHV, MaLop),
    CONSTRAINT FK_ApDungUuDai_UuDai FOREIGN KEY (MaUD) REFERENCES UuDai (MaUD)
);

CREATE TABLE PhieuThu (
    SoPT            VARCHAR(15)     NOT NULL,
    NgayThu         DATE            NOT NULL,
    NguoiNop        NVARCHAR(100)   NOT NULL,
    HinhThuc        NVARCHAR(20)    NOT NULL,
    MaNV            VARCHAR(10)     NOT NULL,
    CONSTRAINT PK_PhieuThu PRIMARY KEY (SoPT),
    CONSTRAINT CK_PhieuThu_HinhThuc CHECK (HinhThuc IN (N'Tiền mặt', N'Chuyển khoản'))
);

CREATE TABLE ChiTietPhieuThu (
    SoPT            VARCHAR(15)     NOT NULL,
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    Dot             TINYINT         NOT NULL,
    SoTien          DECIMAL(12, 0)  NOT NULL,
    CONSTRAINT PK_ChiTietPhieuThu PRIMARY KEY (SoPT, MaHV, MaLop, Dot),
    CONSTRAINT FK_ChiTietPhieuThu_PhieuThu FOREIGN KEY (SoPT) REFERENCES PhieuThu (SoPT),
    CONSTRAINT FK_ChiTietPhieuThu_DotHocPhi FOREIGN KEY (MaHV, MaLop, Dot) REFERENCES DotHocPhi (MaHV, MaLop, Dot),
    CONSTRAINT CK_ChiTietPhieuThu_SoTien CHECK (SoTien > 0)
);

/* ----------------------------- Kho D4 – Học tập ----------------------------- */

CREATE TABLE DiemDanh (
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    SoBuoi          SMALLINT        NOT NULL,
    TrangThai       CHAR(1)         NOT NULL,
    GhiChu          NVARCHAR(200)   NULL,
    CONSTRAINT PK_DiemDanh PRIMARY KEY (MaHV, MaLop, SoBuoi),
    CONSTRAINT FK_DiemDanh_DangKy FOREIGN KEY (MaHV, MaLop) REFERENCES DangKy (MaHV, MaLop),
    CONSTRAINT FK_DiemDanh_BuoiHoc FOREIGN KEY (MaLop, SoBuoi) REFERENCES BuoiHoc (MaLop, SoBuoi),
    CONSTRAINT CK_DiemDanh_TrangThai CHECK (TrangThai IN ('x', 'M', 'P', 'K'))
);

CREATE TABLE Diem (
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    LoaiDiem        NVARCHAR(10)    NOT NULL,
    Diem            DECIMAL(3, 1)   NOT NULL,
    NhanXet         NVARCHAR(500)   NULL,
    CONSTRAINT PK_Diem PRIMARY KEY (MaHV, MaLop, LoaiDiem),
    CONSTRAINT FK_Diem_DangKy FOREIGN KEY (MaHV, MaLop) REFERENCES DangKy (MaHV, MaLop),
    CONSTRAINT CK_Diem_LoaiDiem CHECK (LoaiDiem IN (N'Giữa kỳ', N'Cuối kỳ')),
    CONSTRAINT CK_Diem_Diem CHECK (Diem BETWEEN 0 AND 10)
);

CREATE TABLE KetQua (
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    DiemTongKet     DECIMAL(3, 1)   NOT NULL,
    XepLoai         NVARCHAR(20)    NOT NULL,
    KetQua          NVARCHAR(10)    NOT NULL,
    MaNVDuyet       VARCHAR(10)     NULL,
    NgayDuyet       DATE            NULL,
    CONSTRAINT PK_KetQua PRIMARY KEY (MaHV, MaLop),
    CONSTRAINT FK_KetQua_DangKy FOREIGN KEY (MaHV, MaLop) REFERENCES DangKy (MaHV, MaLop),
    CONSTRAINT CK_KetQua_DiemTongKet CHECK (DiemTongKet BETWEEN 0 AND 10),
    CONSTRAINT CK_KetQua_XepLoai CHECK (XepLoai IN (N'Giỏi', N'Khá', N'Trung bình', N'Không đạt')),
    CONSTRAINT CK_KetQua_KetQua CHECK (KetQua IN (N'Đạt', N'Không đạt')),
    CONSTRAINT CK_KetQua_Dat CHECK (NOT (KetQua = N'Đạt' AND XepLoai = N'Không đạt')),
    CONSTRAINT CK_KetQua_Duyet CHECK ((MaNVDuyet IS NULL AND NgayDuyet IS NULL) OR (MaNVDuyet IS NOT NULL AND NgayDuyet IS NOT NULL))
);

CREATE TABLE ChungNhan (
    SoCN            VARCHAR(15)     NOT NULL,
    MaHV            VARCHAR(10)     NOT NULL,
    MaLop           VARCHAR(15)     NOT NULL,
    NgayCap         DATE            NOT NULL,
    CONSTRAINT PK_ChungNhan PRIMARY KEY (SoCN),
    CONSTRAINT UQ_ChungNhan_KetQua UNIQUE (MaHV, MaLop),
    CONSTRAINT FK_ChungNhan_KetQua FOREIGN KEY (MaHV, MaLop) REFERENCES KetQua (MaHV, MaLop)
);

/* ---------------------- Kho D5 – Tài khoản và thông báo ---------------------- */

CREATE TABLE NhanVien (
    MaNV            VARCHAR(10)     NOT NULL,
    HoTen           NVARCHAR(100)   NOT NULL,
    BoPhan          NVARCHAR(30)    NOT NULL,
    ChucVu          NVARCHAR(50)    NOT NULL,
    DienThoai       VARCHAR(10)     NOT NULL,
    Email           VARCHAR(100)    NOT NULL,
    TrangThai       NVARCHAR(20)    NOT NULL CONSTRAINT DF_NhanVien_TrangThai DEFAULT (N'Đang làm'),
    CONSTRAINT PK_NhanVien PRIMARY KEY (MaNV),
    CONSTRAINT CK_NhanVien_BoPhan CHECK (BoPhan IN (N'Ban giám đốc', N'Đào tạo', N'Tuyển sinh – CSHV', N'Kế toán', N'CNTT')),
    CONSTRAINT CK_NhanVien_DienThoai CHECK (LEN(DienThoai) = 10 AND DienThoai NOT LIKE '%[^0-9]%'),
    CONSTRAINT CK_NhanVien_Email CHECK (Email LIKE '%_@_%._%'),
    CONSTRAINT CK_NhanVien_TrangThai CHECK (TrangThai IN (N'Đang làm', N'Nghỉ việc'))
);

/* Khóa ngoại tới NhanVien của các bảng tạo trước (NhanVien thuộc kho D5 nên tạo sau) */
ALTER TABLE PhieuDangKy ADD CONSTRAINT FK_PhieuDangKy_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien (MaNV);
ALTER TABLE PhieuThu ADD CONSTRAINT FK_PhieuThu_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien (MaNV);
ALTER TABLE KetQua ADD CONSTRAINT FK_KetQua_NhanVien FOREIGN KEY (MaNVDuyet) REFERENCES NhanVien (MaNV);

CREATE TABLE TaiKhoan (
    TenDangNhap     VARCHAR(50)     NOT NULL,
    MatKhauBam      VARCHAR(255)    NOT NULL,
    VaiTro          NVARCHAR(20)    NOT NULL,
    MaNV            VARCHAR(10)     NULL,
    MaGV            VARCHAR(10)     NULL,
    MaHV            VARCHAR(10)     NULL,
    MaPH            VARCHAR(10)     NULL,
    TrangThai       NVARCHAR(20)    NOT NULL CONSTRAINT DF_TaiKhoan_TrangThai DEFAULT (N'Hoạt động'),
    CONSTRAINT PK_TaiKhoan PRIMARY KEY (TenDangNhap),
    CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien (MaNV),
    CONSTRAINT FK_TaiKhoan_GiaoVien FOREIGN KEY (MaGV) REFERENCES GiaoVien (MaGV),
    CONSTRAINT FK_TaiKhoan_HocVien FOREIGN KEY (MaHV) REFERENCES HocVien (MaHV),
    CONSTRAINT FK_TaiKhoan_PhuHuynh FOREIGN KEY (MaPH) REFERENCES PhuHuynh (MaPH),
    CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN (N'Học viên', N'Phụ huynh', N'Giáo viên', N'NV tuyển sinh', N'NV kế toán', N'QL đào tạo', N'Giám đốc', N'Quản trị viên')),
    CONSTRAINT CK_TaiKhoan_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Khóa')),
    CONSTRAINT CK_TaiKhoan_ChuSoHuu CHECK ((VaiTro = N'Giáo viên' AND MaGV IS NOT NULL AND MaNV IS NULL AND MaHV IS NULL AND MaPH IS NULL) OR (VaiTro = N'Học viên' AND MaHV IS NOT NULL AND MaNV IS NULL AND MaGV IS NULL AND MaPH IS NULL) OR (VaiTro = N'Phụ huynh' AND MaPH IS NOT NULL AND MaNV IS NULL AND MaGV IS NULL AND MaHV IS NULL) OR (VaiTro IN (N'NV tuyển sinh', N'NV kế toán', N'QL đào tạo', N'Giám đốc', N'Quản trị viên') AND MaNV IS NOT NULL AND MaGV IS NULL AND MaHV IS NULL AND MaPH IS NULL))
);

CREATE TABLE ThongBao (
    MaTB            INT             NOT NULL IDENTITY(1, 1),
    Loai            NVARCHAR(20)    NOT NULL,
    MaHV            VARCHAR(10)     NOT NULL,
    MaPH            VARCHAR(10)     NULL,
    NoiDung         NVARCHAR(1000)  NOT NULL,
    ThoiGianGui     DATETIME2(0)    NOT NULL CONSTRAINT DF_ThongBao_ThoiGianGui DEFAULT (SYSDATETIME()),
    TrangThaiGui    NVARCHAR(10)    NOT NULL CONSTRAINT DF_ThongBao_TrangThaiGui DEFAULT (N'Chờ gửi'),
    CONSTRAINT PK_ThongBao PRIMARY KEY (MaTB),
    CONSTRAINT FK_ThongBao_HocVien FOREIGN KEY (MaHV) REFERENCES HocVien (MaHV),
    CONSTRAINT FK_ThongBao_PhuHuynh FOREIGN KEY (MaPH) REFERENCES PhuHuynh (MaPH),
    CONSTRAINT FK_ThongBao_GiamHo FOREIGN KEY (MaHV, MaPH) REFERENCES HocVienPhuHuynh (MaHV, MaPH),
    CONSTRAINT CK_ThongBao_Loai CHECK (Loai IN (N'Lịch học', N'Học phí', N'Chuyên cần', N'Kết quả')),
    CONSTRAINT CK_ThongBao_TrangThaiGui CHECK (TrangThaiGui IN (N'Chờ gửi', N'Đã gửi', N'Lỗi'))
);

CREATE TABLE NhatKy (
    MaNK            BIGINT          NOT NULL IDENTITY(1, 1),
    ThoiDiem        DATETIME2(0)    NOT NULL CONSTRAINT DF_NhatKy_ThoiDiem DEFAULT (SYSDATETIME()),
    TenDangNhap     VARCHAR(50)     NOT NULL,
    ThaoTac         NVARCHAR(20)    NOT NULL,
    DoiTuong        VARCHAR(30)     NOT NULL,
    MaDoiTuong      NVARCHAR(100)   NOT NULL,
    CONSTRAINT PK_NhatKy PRIMARY KEY (MaNK),
    CONSTRAINT FK_NhatKy_TaiKhoan FOREIGN KEY (TenDangNhap) REFERENCES TaiKhoan (TenDangNhap),
    CONSTRAINT CK_NhatKy_ThaoTac CHECK (ThaoTac IN (N'Đăng nhập', N'Thêm', N'Sửa', N'Ngừng', N'Duyệt', N'In'))
);

CREATE TABLE ThamSo (
    MaThamSo        VARCHAR(30)     NOT NULL,
    TenThamSo       NVARCHAR(100)   NOT NULL,
    GiaTri          DECIMAL(10, 2)  NOT NULL,
    DonVi           NVARCHAR(20)    NOT NULL,
    QuyTac          VARCHAR(20)     NULL,
    CONSTRAINT PK_ThamSo PRIMARY KEY (MaThamSo)
);
GO

/* ------------------------------- Chỉ mục -------------------------------
   Khóa chính đã có chỉ mục cụm. Thêm chỉ mục cho các trường tra cứu thường xuyên
   và các khóa ngoại không đứng đầu khóa chính (dùng để JOIN). */
CREATE INDEX IX_HocVien_HoTen ON HocVien (HoTen);                                    -- 1.1, 2.2 tra cứu học viên theo tên
CREATE INDEX IX_HocVien_DienThoai ON HocVien (DienThoai, NgaySinh);           -- 1.1 kiểm tra trùng hồ sơ
CREATE INDEX IX_LopHoc_MaKH ON LopHoc (MaKH);                                      -- 3.4, 5.3 nhóm lớp theo khóa học
CREATE INDEX IX_LopHoc_MaGV ON LopHoc (MaGV);                                      -- 2.1 lớp do giáo viên phụ trách
CREATE INDEX IX_BuoiHoc_GiaoVien_Ngay ON BuoiHoc (MaGV, Ngay);                -- 2.3 kiểm tra trùng giáo viên (QT07)
CREATE INDEX IX_BuoiHoc_Phong_Ngay ON BuoiHoc (MaPhong, Ngay);                -- 2.3 kiểm tra trùng phòng (QT07)
CREATE INDEX IX_DangKy_MaLop ON DangKy (MaLop, TrangThai);                    -- sĩ số lớp (2.1, 2.2, 5.3)
CREATE INDEX IX_PhieuThu_NgayThu ON PhieuThu (NgayThu);                       -- 3.4 doanh thu theo kỳ
CREATE INDEX IX_ChiTietPhieuThu_Dot ON ChiTietPhieuThu (MaHV, MaLop, Dot);    -- số đã thu theo đợt (3.2, 3.3)
CREATE INDEX IX_DiemDanh_Buoi ON DiemDanh (MaLop, SoBuoi);                    -- 4.1 điểm danh theo buổi
CREATE INDEX IX_ThongBao_MaHV ON ThongBao (MaHV, ThoiGianGui);                    -- 5.2 thông báo của một học viên
CREATE INDEX IX_NhatKy_ThoiDiem ON NhatKy (ThoiDiem);                                  -- 5.1 xem nhật ký theo thời gian
/* Quan hệ 1–1 (3.2 Q23–Q26): mỗi nhân viên, giáo viên, học viên, phụ huynh có tối đa một tài khoản */
CREATE UNIQUE INDEX UX_TaiKhoan_MaNV ON TaiKhoan (MaNV) WHERE MaNV IS NOT NULL;
CREATE UNIQUE INDEX UX_TaiKhoan_MaGV ON TaiKhoan (MaGV) WHERE MaGV IS NOT NULL;
CREATE UNIQUE INDEX UX_TaiKhoan_MaHV ON TaiKhoan (MaHV) WHERE MaHV IS NOT NULL;
CREATE UNIQUE INDEX UX_TaiKhoan_MaPH ON TaiKhoan (MaPH) WHERE MaPH IS NOT NULL;
GO
