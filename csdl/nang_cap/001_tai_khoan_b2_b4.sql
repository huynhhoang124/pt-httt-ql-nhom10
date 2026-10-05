/* Nâng cấp 001 (06_bao_tri mục 3.3) – bảo trì B2 (bắt đổi mật khẩu lần đầu) và B4 (đếm đăng nhập sai trong CSDL).
   Chạy trên CSDL đang có dữ liệu; tài khoản đang dùng giữ PhaiDoiMatKhau = 0, tài khoản cấp sau mặc định = 1. */
USE TrungTamNgoaiNgu;
GO
ALTER TABLE TaiKhoan ADD
    PhaiDoiMatKhau  BIT             NOT NULL CONSTRAINT DF_TaiKhoan_PhaiDoiMatKhau DEFAULT (0),
    SoLanSai        TINYINT         NOT NULL CONSTRAINT DF_TaiKhoan_SoLanSai DEFAULT (0),
    KhoaDen         DATETIME2(0)    NULL;
GO
ALTER TABLE TaiKhoan DROP CONSTRAINT DF_TaiKhoan_PhaiDoiMatKhau;
ALTER TABLE TaiKhoan ADD CONSTRAINT DF_TaiKhoan_PhaiDoiMatKhau DEFAULT (1) FOR PhaiDoiMatKhau;
GO
