/* Nâng cấp 002 – ghi phiên bản CSDL, chạy lại an toàn trên CSDL đã có dữ liệu.
   Mỗi tệp nâng cấp sau phải kết thúc trong cùng giao dịch bằng:
   UPDATE ThamSo SET GiaTri = <số tệp> WHERE MaThamSo = 'PHIEN_BAN_CSDL';
   Seed mới có phiên bản 1 (đã gồm 001); chạy tệp này để chuyển lên 2.
   Không hạ phiên bản nếu CSDL đã được nâng cấp bằng tệp sau 002. */
USE TrungTamNgoaiNgu;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.ThamSo WITH (UPDLOCK, HOLDLOCK) WHERE MaThamSo = 'PHIEN_BAN_CSDL')
    INSERT dbo.ThamSo (MaThamSo, TenThamSo, GiaTri, DonVi, QuyTac)
    VALUES ('PHIEN_BAN_CSDL', N'Phiên bản CSDL', 2, N'phiên bản', NULL);
UPDATE dbo.ThamSo SET GiaTri = 2
WHERE MaThamSo = 'PHIEN_BAN_CSDL' AND GiaTri < 2;
COMMIT TRANSACTION;
GO
