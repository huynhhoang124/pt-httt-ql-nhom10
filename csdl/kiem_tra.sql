/* =====================================================================
   Kiểm tra CSDL sau khi nạp dữ liệu mẫu (ngày chốt 31/12/2026).
   Phần 1: đối chiếu với 5 chứng từ mẫu và các quy tắc QT01–QT16 – mỗi dòng Đạt / LỖI
           (chay.ps1 trả mã lỗi nếu có dòng LỖI).
   Phần 2: các báo cáo MIS (3.4, 5.3, 5.4) chạy trên dữ liệu mẫu.
   ===================================================================== */
USE TrungTamNgoaiNgu;
GO
SET NOCOUNT ON;
DECLARE @kq TABLE (STT INT, KiemTra NVARCHAR(150), KetQua NVARCHAR(10), ChiTiet NVARCHAR(300));
DECLARE @n INT, @n2 INT, @v DECIMAL(14, 1), @s NVARCHAR(300), @s2 NVARCHAR(300);

/* ---------------- Đối chiếu chứng từ mẫu (02_thu_thap mục 4.2) ---------------- */
SELECT @v = SUM(h.PhaiNop) FROM v_HocPhiDangKy h
JOIN DangKy d ON d.MaHV = h.MaHV AND d.MaLop = h.MaLop WHERE d.SoPhieuDK = 'DK2026-0158';
SELECT @n = COUNT(*) FROM DangKy WHERE SoPhieuDK = 'DK2026-0158';
INSERT @kq VALUES (1, N'Phiếu đăng ký DK2026-0158: 2 lớp, phải nộp 6.480.000',
    IIF(@v = 6480000 AND @n = 2, N'Đạt', N'LỖI'), CONCAT(@n, N' lớp, phải nộp ', FORMAT(@v, 'N0', 'vi-VN')));

SELECT @v = SUM(SoTien), @n = COUNT(*) FROM ChiTietPhieuThu WHERE SoPT = 'PT2026-0731';
SELECT @s = STRING_AGG(CONCAT(MaLop, N' đợt ', Dot), ', ') WITHIN GROUP (ORDER BY MaLop DESC, Dot) FROM ChiTietPhieuThu WHERE SoPT = 'PT2026-0731';
INSERT @kq VALUES (2, N'Phiếu thu PT2026-0731: tổng nộp 5.400.000',
    IIF(@v = 5400000 AND @n = 3, N'Đạt', N'LỖI'), CONCAT(FORMAT(@v, 'N0', 'vi-VN'), N' = ', @s));

SELECT @v = (SELECT SUM(SoTien) FROM DotHocPhi WHERE MaHV = 'HV0412' AND MaLop IN ('IEK-2609', 'GT-2604'))
          - (SELECT SUM(c.SoTien) FROM ChiTietPhieuThu c JOIN PhieuThu p ON p.SoPT = c.SoPT
             WHERE c.MaHV = 'HV0412' AND c.MaLop IN ('IEK-2609', 'GT-2604') AND p.NgayThu <= '2026-09-06');
SELECT @s = FORMAT(HanDong, 'dd/MM/yyyy') FROM DotHocPhi WHERE MaHV = 'HV0412' AND MaLop = 'GT-2604' AND Dot = 2;
INSERT @kq VALUES (3, N'Còn nợ sau PT2026-0731 = 1.080.000, hạn đợt 2 ngày 17/10/2026',
    IIF(@v = 1080000 AND @s = '17/10/2026', N'Đạt', N'LỖI'), CONCAT(N'Còn nợ ', FORMAT(@v, 'N0', 'vi-VN'), N', hạn ', @s));

SELECT @s = STRING_AGG(TrangThai, '') WITHIN GROUP (ORDER BY SoBuoi) FROM DiemDanh WHERE MaHV = 'HV0412' AND MaLop = 'IEK-2609' AND SoBuoi <= 4;
SELECT @s2 = STRING_AGG(TrangThai, '') WITHIN GROUP (ORDER BY SoBuoi) FROM DiemDanh WHERE MaHV = 'HV0398' AND MaLop = 'IEK-2609' AND SoBuoi <= 4;
SELECT @n = COUNT(*) FROM BuoiHoc WHERE MaLop = 'IEK-2609' AND SoBuoi <= 4
    AND FORMAT(Ngay, 'dd/MM') IN ('15/09', '17/09', '22/09', '24/09') AND MaPhong = 'P203';
INSERT @kq VALUES (4, N'Sổ điểm danh IEK-2609, buổi 1–4 (15/09–24/09, phòng P203)',
    IIF(@s COLLATE Latin1_General_BIN = 'xMxP' AND @s2 COLLATE Latin1_General_BIN = 'xKKx' AND @n = 4, N'Đạt', N'LỖI'),
    CONCAT(N'HV0412: ', @s, N'; HV0398: ', @s2, N'; ', @n, N'/4 buổi đúng ngày'));

SELECT @s = STRING_AGG(CONCAT(MaHV, ' ', DiemTongKet, ' ', XepLoai, ' ', KetQua), N'; ') WITHIN GROUP (ORDER BY MaHV DESC)
FROM KetQua WHERE MaLop = 'IEK-2609' AND MaHV IN ('HV0412', 'HV0398');
INSERT @kq VALUES (5, N'Bảng điểm IEK-2609: HV0412 8,0 Khá Đạt; HV0398 4,5 Không đạt',
    IIF(@s = N'HV0412 8.0 Khá Đạt; HV0398 4.5 Không đạt Không đạt', N'Đạt', N'LỖI'), @s);

SELECT @s = CONCAT(c.MaHV, ' ', c.MaLop, ' ', FORMAT(c.NgayCap, 'dd/MM/yyyy'), N', học ',
                   FORMAT(MIN(b.Ngay), 'dd/MM/yyyy'), ' – ', FORMAT(MAX(b.Ngay), 'dd/MM/yyyy'))
FROM ChungNhan c JOIN BuoiHoc b ON b.MaLop = c.MaLop AND b.TrangThai = N'Đã dạy'
WHERE c.SoCN = 'CN2026-0089' GROUP BY c.MaHV, c.MaLop, c.NgayCap;
INSERT @kq VALUES (6, N'Chứng nhận CN2026-0089: HV0412, IEK-2609, 15/09–15/12/2026, cấp 20/12/2026',
    IIF(@s = N'HV0412 IEK-2609 20/12/2026, học 15/09/2026 – 15/12/2026', N'Đạt', N'LỖI'), @s);

/* ---------------- Thuộc tính "chốt" khớp với tính lại ---------------- */
SELECT @n = COUNT(*) FROM KetQua k FULL JOIN v_KetQuaTinh t ON t.MaHV = k.MaHV AND t.MaLop = k.MaLop
WHERE k.MaHV IS NULL OR t.MaHV IS NULL OR k.DiemTongKet <> t.DiemTongKet OR k.XepLoai <> t.XepLoai OR k.KetQua <> t.KetQua;
SELECT @n2 = COUNT(*) FROM KetQua;
INSERT @kq VALUES (7, N'QT13–QT15: kết quả đã chốt = kết quả tính lại từ điểm danh, điểm, trọng số',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(@n2, N' kết quả, lệch ', @n));

/* ---------------- Học phí và ưu đãi (QT01–QT05) ---------------- */
SELECT @n = COUNT(*) FROM ApDungUuDai a JOIN HocPhi h ON h.MaHV = a.MaHV AND h.MaLop = a.MaLop
WHERE a.MaUD = 'UD-HVCU' AND NOT EXISTS (SELECT 1 FROM v_HocVienCu c WHERE c.MaHV = a.MaHV AND c.NgayKetThuc < h.NgayLap);
SELECT @n2 = COUNT(*) FROM HocPhi h
WHERE EXISTS (SELECT 1 FROM v_HocVienCu c WHERE c.MaHV = h.MaHV AND c.NgayKetThuc < h.NgayLap)
  AND NOT EXISTS (SELECT 1 FROM ApDungUuDai a WHERE a.MaHV = h.MaHV AND a.MaLop = h.MaLop AND a.MaUD = 'UD-HVCU');
SELECT @s = CONCAT(COUNT(*), N' khoản hưởng ưu đãi học viên cũ') FROM ApDungUuDai WHERE MaUD = 'UD-HVCU';
INSERT @kq VALUES (8, N'QT01: ưu đãi học viên cũ đúng người (đã học hết một lớp trước ngày đăng ký)',
    IIF(@n = 0 AND @n2 = 0, N'Đạt', N'LỖI'), CONCAT(@s, N'; sai ', @n, N', bỏ sót ', @n2));

WITH s AS (SELECT h.MaHV, h.MaLop, h.TyLeGiam, ISNULL(SUM(u.TyLeGiam), 0) AS Tong
           FROM HocPhi h LEFT JOIN ApDungUuDai a ON a.MaHV = h.MaHV AND a.MaLop = h.MaLop
           LEFT JOIN UuDai u ON u.MaUD = a.MaUD GROUP BY h.MaHV, h.MaLop, h.TyLeGiam)
SELECT @n = SUM(IIF(TyLeGiam <> IIF(Tong > dbo.fn_ThamSo('TY_LE_GIAM_TOI_DA'), dbo.fn_ThamSo('TY_LE_GIAM_TOI_DA'), Tong), 1, 0)),
       @n2 = SUM(IIF(Tong > dbo.fn_ThamSo('TY_LE_GIAM_TOI_DA'), 1, 0)) FROM s;
INSERT @kq VALUES (9, N'QT04: tỷ lệ giảm = tổng ưu đãi, chặn ở 15%',
    IIF(@n = 0 AND @n2 > 0, N'Đạt', N'LỖI'), CONCAT(N'Sai ', @n, N'; ', @n2, N' khoản bị chặn ở 15%'));

SELECT @n = COUNT(*) FROM HocPhi h
CROSS APPLY (SELECT SUM(SoTien) AS Tong, COUNT(*) AS SoDong, MIN(IIF(Dot = 1, SoTien, NULL)) AS Dot1
             FROM DotHocPhi x WHERE x.MaHV = h.MaHV AND x.MaLop = h.MaLop) t
WHERE t.Tong <> h.HocPhiGoc * (100 - h.TyLeGiam) / 100 OR t.SoDong <> h.SoDot
   OR t.Dot1 < t.Tong * dbo.fn_ThamSo('TY_LE_DOT_1') / 100;
SELECT @n2 = COUNT(*) FROM HocPhi;
INSERT @kq VALUES (10, N'QT05: tổng các đợt = phải nộp; số đợt khớp; đợt 1 ≥ 50%',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(@n2, N' khoản học phí, sai ', @n));

SELECT @n = COUNT(*) FROM DotHocPhi d
CROSS APPLY (SELECT SUM(SoTien) AS DaNop FROM ChiTietPhieuThu c WHERE c.MaHV = d.MaHV AND c.MaLop = d.MaLop AND c.Dot = d.Dot) t
WHERE t.DaNop > d.SoTien;
SELECT @n2 = COUNT(*) FROM (SELECT MaHV, MaLop, Dot FROM ChiTietPhieuThu GROUP BY MaHV, MaLop, Dot HAVING COUNT(*) > 1) x;
INSERT @kq VALUES (11, N'Không đợt nào bị nộp vượt số phải nộp (một đợt có thể nộp qua nhiều phiếu)',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(N'Vượt ', @n, N'; ', @n2, N' đợt nộp qua nhiều phiếu'));

SELECT @s = STRING_AGG(MaHV, ', ') WITHIN GROUP (ORDER BY MaHV) FROM dbo.fn_CongNo('2026-12-31') WHERE QuaHan = 1;
SELECT @n = COUNT(*) FROM v_HocPhiDangKy WHERE TrangThai = N'Nghỉ học' AND ConNo > 0;
SELECT @n2 = COUNT(*) FROM dbo.fn_CongNo('2026-12-31') c JOIN DangKy d ON d.MaHV = c.MaHV AND d.MaLop = c.MaLop WHERE d.TrangThai = N'Nghỉ học';
INSERT @kq VALUES (12, N'QT10, QT16: công nợ tại 31/12 bỏ đăng ký nghỉ học; quá hạn đúng 3 học viên TOE',
    IIF(@s = 'HV0485, HV0486, HV0487' AND @n > 0 AND @n2 = 0, N'Đạt', N'LỖI'),
    CONCAT(N'Quá hạn: ', @s, N'; ', @n, N' đăng ký nghỉ học còn nợ đã được bỏ'));

/* ---------------- Lớp, lịch, đăng ký (QT06–QT09) ---------------- */
SELECT @n = COUNT(*) FROM LopHoc l WHERE l.TrangThai IN (N'Đang học', N'Kết thúc')
   AND (SELECT COUNT(*) FROM DangKy d WHERE d.MaLop = l.MaLop AND d.MaLopGoc IS NULL) < dbo.fn_ThamSo('SISO_TOI_THIEU');
SELECT @n2 = COUNT(*) FROM v_SiSoLop s
WHERE s.SoDangKy > s.SiSoToiDa OR s.SiSoToiDa > dbo.fn_ThamSo('SISO_TOI_DA')
   OR s.SiSoToiDa > (SELECT MIN(p.SucChua) FROM LichTuan t JOIN PhongHoc p ON p.MaPhong = t.MaPhong WHERE t.MaLop = s.MaLop);
INSERT @kq VALUES (13, N'QT06: lớp mở có ≥ 8 đăng ký; sĩ số ≤ tối đa ≤ 20 và ≤ sức chứa phòng',
    IIF(@n = 0 AND @n2 = 0, N'Đạt', N'LỖI'), CONCAT(N'Lớp thiếu sĩ số: ', @n, N'; vượt sĩ số/phòng: ', @n2));

SELECT @n = COUNT(*) FROM BuoiHoc a JOIN BuoiHoc b
  ON a.Ngay = b.Ngay AND (a.MaLop < b.MaLop OR (a.MaLop = b.MaLop AND a.SoBuoi < b.SoBuoi))
 AND a.GioBatDau < b.GioKetThuc AND b.GioBatDau < a.GioKetThuc AND (a.MaGV = b.MaGV OR a.MaPhong = b.MaPhong)
WHERE a.TrangThai <> N'Hủy' AND b.TrangThai <> N'Hủy';
SELECT @n2 = COUNT(*) FROM BuoiHoc;
INSERT @kq VALUES (14, N'QT07: không có 2 buổi trùng giờ cùng giáo viên hoặc cùng phòng',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(@n2, N' buổi, trùng ', @n));

SELECT @n = COUNT(*) FROM BuoiHoc b JOIN LopHoc l ON l.MaLop = b.MaLop JOIN KhoaHoc k ON k.MaKH = l.MaKH
WHERE b.Loai = N'Thường' AND (DATEDIFF(MINUTE, b.GioBatDau, b.GioKetThuc) <> k.ThoiLuongBuoi
   OR NOT EXISTS (SELECT 1 FROM LichTuan t WHERE t.MaLop = b.MaLop AND t.GioBatDau = b.GioBatDau
                  AND t.Thu = DATEDIFF(DAY, '19000101', b.Ngay) % 7 + 2));   -- 01/01/1900 là thứ Hai
INSERT @kq VALUES (15, N'Buổi học thường đúng thứ, giờ của lịch tuần và đúng thời lượng của khóa',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(N'Sai ', @n));

SELECT @n = COUNT(*) FROM DangKy c JOIN DangKy m ON m.MaHV = c.MaHV AND m.MaLopGoc = c.MaLop
JOIN LopHoc lc ON lc.MaLop = c.MaLop JOIN LopHoc lm ON lm.MaLop = m.MaLop
WHERE c.TrangThai = N'Chuyển lớp' AND (lc.MaKH <> lm.MaKH OR
      (SELECT COUNT(*) FROM BuoiHoc b WHERE b.MaLop = c.MaLop AND b.TrangThai = N'Đã dạy' AND b.Ngay < c.NgayThayDoi)
       > dbo.fn_ThamSo('SO_BUOI_CHUYEN_LOP'));
SELECT @n2 = COUNT(*) FROM DangKy c WHERE c.TrangThai = N'Chuyển lớp'
   AND (SELECT COUNT(*) FROM DangKy m WHERE m.MaHV = c.MaHV AND m.MaLopGoc = c.MaLop) <> 1;
SELECT @s = CONCAT(COUNT(*), N' lần chuyển lớp') FROM DangKy WHERE TrangThai = N'Chuyển lớp';
INSERT @kq VALUES (16, N'QT08: chuyển lớp trong 3 buổi đầu, cùng khóa; mỗi lần chuyển có đúng 1 đăng ký mới',
    IIF(@n = 0 AND @n2 = 0, N'Đạt', N'LỖI'), CONCAT(@s, N'; sai điều kiện ', @n, N'; sai chuỗi ', @n2));

SELECT @n = COUNT(*) FROM DangKy d JOIN LopHoc l ON l.MaLop = d.MaLop JOIN KhoaHoc k ON k.MaKH = l.MaKH
WHERE d.TrangThai = N'Bảo lưu' AND (
      ISNULL((SELECT SUM(c.SoTien) FROM ChiTietPhieuThu c JOIN PhieuThu p ON p.SoPT = c.SoPT
              WHERE c.MaHV = d.MaHV AND c.MaLop = d.MaLop AND p.NgayThu <= d.NgayThayDoi), 0)
        < (SELECT SUM(SoTien) FROM DotHocPhi x WHERE x.MaHV = d.MaHV AND x.MaLop = d.MaLop)
   OR 100.0 * (SELECT COUNT(*) FROM BuoiHoc b WHERE b.MaLop = d.MaLop AND b.TrangThai = N'Đã dạy' AND b.Ngay < d.NgayThayDoi)
        / k.SoBuoi >= dbo.fn_ThamSo('TY_LE_BUOI_BAO_LUU')
   OR d.HanBaoLuu <> DATEADD(MONTH, CAST(dbo.fn_ThamSo('THANG_BAO_LUU') AS INT), d.NgayThayDoi));
SELECT @s = STRING_AGG(CONCAT(MaHV, ' ', MaLop, N' đến ', FORMAT(HanBaoLuu, 'dd/MM/yyyy')), '; ') FROM DangKy WHERE TrangThai = N'Bảo lưu';
INSERT @kq VALUES (17, N'QT09: bảo lưu khi đã đóng đủ, học < 50% số buổi; hạn = ngày bảo lưu + 6 tháng',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(@s, N'; sai ', @n));

/* ---------------- Học tập (QT11–QT15) ---------------- */
SELECT @n = COUNT(*) FROM DiemDanh d JOIN BuoiHoc b ON b.MaLop = d.MaLop AND b.SoBuoi = d.SoBuoi WHERE b.TrangThai <> N'Đã dạy';
SELECT @n2 = COUNT(*) FROM DiemDanh;
INSERT @kq VALUES (18, N'QT11: chỉ điểm danh ở buổi đã dạy, đúng lớp của đăng ký',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(@n2, N' lượt điểm danh, sai ', @n));

SELECT @n = COUNT(*) FROM ChungNhan c JOIN KetQua k ON k.MaHV = c.MaHV AND k.MaLop = c.MaLop WHERE k.KetQua <> N'Đạt';
SELECT @n2 = COUNT(*) FROM KetQua k WHERE k.KetQua = N'Đạt' AND k.NgayDuyet IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM ChungNhan c WHERE c.MaHV = k.MaHV AND c.MaLop = k.MaLop);
SELECT @s = CONCAT(COUNT(*), N' chứng nhận') FROM ChungNhan;
INSERT @kq VALUES (19, N'QT15: chỉ cấp chứng nhận cho kết quả Đạt; mọi kết quả Đạt đã duyệt đều có chứng nhận',
    IIF(@n = 0 AND @n2 = 0, N'Đạt', N'LỖI'), CONCAT(@s, N'; cấp sai ', @n, N'; thiếu ', @n2));

SELECT @n = COUNT(*) FROM v_ChuyenCan WHERE CanhBaoVang = 1;
SELECT @s = STRING_AGG(CONCAT(MaHV, N' (', TyLeChuyenCan, N'%)'), ', ') FROM v_ChuyenCan WHERE MaHV = 'HV0398' AND MaLop = 'IEK-2609';
INSERT @kq VALUES (20, N'QT12: cảnh báo vắng > 20% (có HV0398 lớp IEK-2609)',
    IIF(@n > 0 AND EXISTS (SELECT 1 FROM v_ChuyenCan WHERE MaHV = 'HV0398' AND MaLop = 'IEK-2609' AND CanhBaoVang = 1 AND NguyCoKhongDat = 1),
        N'Đạt', N'LỖI'), CONCAT(@n, N' đăng ký bị cảnh báo; ', @s));

/* ---------------- Hồ sơ, phụ huynh ---------------- */
SELECT @n = COUNT(*) FROM HocVien h
WHERE DATEADD(YEAR, 18, h.NgaySinh) > '2026-12-31'
  AND NOT EXISTS (SELECT 1 FROM HocVienPhuHuynh g WHERE g.MaHV = h.MaHV);
SELECT @n2 = COUNT(*) FROM HocVien WHERE DATEADD(YEAR, 18, NgaySinh) > '2026-12-31';
SELECT @s = CONCAT((SELECT COUNT(*) FROM PhuHuynh), N' phụ huynh, ', (SELECT COUNT(*) FROM HocVienPhuHuynh), N' quan hệ giám hộ');
INSERT @kq VALUES (21, N'Học viên dưới 18 tuổi đều có ít nhất một phụ huynh (1.1)',
    IIF(@n = 0, N'Đạt', N'LỖI'), CONCAT(@n2, N' học viên dưới 18 tuổi, thiếu phụ huynh ', @n, N'; ', @s));

SELECT @s = STRING_AGG(MaHV, ', ') WITHIN GROUP (ORDER BY MaHV) FROM HocVienPhuHuynh WHERE MaPH = 'PH0320';
SELECT @n = COUNT(*) FROM PhieuThu p WHERE p.NguoiNop = N'Đỗ Văn Quân'
   AND (SELECT COUNT(DISTINCT MaHV) FROM ChiTietPhieuThu c WHERE c.SoPT = p.SoPT) = 2;
SELECT @s2 = STRING_AGG(MaHV, ', ') WITHIN GROUP (ORDER BY MaHV) FROM HocVienPhuHuynh WHERE MaHV = 'HV0412';
INSERT @kq VALUES (22, N'Phụ huynh PH0320 giám hộ 2 anh em, nộp chung một phiếu thu; HV0412 có cả bố và mẹ',
    IIF(@s = 'HV0451, HV0452' AND @n = 2 AND (SELECT COUNT(*) FROM HocVienPhuHuynh WHERE MaHV = 'HV0412') = 2, N'Đạt', N'LỖI'),
    CONCAT(N'PH0320 → ', @s, N'; ', @n, N' phiếu thu chung'));

/* ---------------- Ràng buộc chặn được dữ liệu sai (thử rồi hủy giao dịch) ---------------- */
DECLARE @thu TABLE (RangBuoc VARCHAR(40), BiChan BIT);
BEGIN TRANSACTION;
BEGIN TRY INSERT Diem VALUES ('HV0480', 'TOE-2611', N'Cuối kỳ', 11, NULL); INSERT @thu VALUES ('CK_Diem_Diem', 0); END TRY
BEGIN CATCH INSERT @thu VALUES ('CK_Diem_Diem', IIF(ERROR_MESSAGE() LIKE '%CK_Diem_Diem%', 1, 0)); END CATCH;
BEGIN TRY INSERT DiemDanh VALUES ('HV0412', 'IEK-2609', 28, 'x', NULL); INSERT @thu VALUES ('FK_DiemDanh_BuoiHoc', 0); END TRY
BEGIN CATCH INSERT @thu VALUES ('FK_DiemDanh_BuoiHoc', IIF(ERROR_MESSAGE() LIKE '%FK_DiemDanh_BuoiHoc%', 1, 0)); END CATCH;
BEGIN TRY INSERT KetQua VALUES ('HV0480', 'TOE-2611', 4.0, N'Không đạt', N'Đạt', NULL, NULL); INSERT @thu VALUES ('CK_KetQua_Dat', 0); END TRY
BEGIN CATCH INSERT @thu VALUES ('CK_KetQua_Dat', IIF(ERROR_MESSAGE() LIKE '%CK_KetQua_Dat%', 1, 0)); END CATCH;
BEGIN TRY INSERT TaiKhoan (TenDangNhap, MatKhauBam, VaiTro, MaHV) VALUES ('thu_sai', 'x', N'Giáo viên', 'HV0413'); INSERT @thu VALUES ('CK_TaiKhoan_ChuSoHuu', 0); END TRY
BEGIN CATCH INSERT @thu VALUES ('CK_TaiKhoan_ChuSoHuu', IIF(ERROR_MESSAGE() LIKE '%CK_TaiKhoan_ChuSoHuu%', 1, 0)); END CATCH;
BEGIN TRY INSERT ThongBao (Loai, MaHV, MaPH, NoiDung) VALUES (N'Học phí', 'HV0412', 'PH0320', N'Thử'); INSERT @thu VALUES ('FK_ThongBao_GiamHo', 0); END TRY
BEGIN CATCH INSERT @thu VALUES ('FK_ThongBao_GiamHo', IIF(ERROR_MESSAGE() LIKE '%FK_ThongBao_GiamHo%', 1, 0)); END CATCH;
ROLLBACK TRANSACTION;
SELECT @n = SUM(CAST(BiChan AS INT)), @n2 = COUNT(*) FROM @thu;
SELECT @s = STRING_AGG(RangBuoc, ', ') FROM @thu WHERE BiChan = 0;
INSERT @kq VALUES (23, N'Ràng buộc chặn dữ liệu sai: điểm 11, buổi của lớp khác, Đạt + Không đạt, sai chủ tài khoản, gửi nhầm phụ huynh',
    IIF(@n = @n2, N'Đạt', N'LỖI'), CONCAT(@n, '/', @n2, N' lần chèn sai bị đúng ràng buộc chặn', IIF(@s IS NULL, '', N'; lọt: ' + @s)));

/* ---------------- Cấu trúc ---------------- */
SELECT @n = COUNT(*) FROM sys.tables;
SELECT @n2 = COUNT(*) FROM sys.foreign_keys;
SELECT @s = CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS NVARCHAR(100));
INSERT @kq VALUES (24, N'CSDL có 26 bảng, đối chiếu chữ tiếng Việt (Vietnamese_100_CI_AS)',
    IIF(@n = 26 AND @s = 'Vietnamese_100_CI_AS', N'Đạt', N'LỖI'), CONCAT(@n, N' bảng, ', @n2, N' khóa ngoại, ', @s));

SELECT STT, KiemTra AS [Kiểm tra], KetQua, ChiTiet AS [Chi tiết] FROM @kq ORDER BY STT;
GO

/* ============================ PHẦN 2 – BÁO CÁO MIS ============================ */

/* 3.4 – Doanh thu theo tháng, so với tháng trước */
SELECT Nam AS [Năm], Thang AS [Tháng], FORMAT(SUM(SoTien), 'N0', 'vi-VN') AS [Doanh thu],
       FORMAT(SUM(SoTien) - LAG(SUM(SoTien)) OVER (ORDER BY Nam, Thang), 'N0', 'vi-VN') AS [So với tháng trước]
FROM v_DoanhThu GROUP BY Nam, Thang ORDER BY Nam, Thang;

/* 3.4 – Doanh thu theo khóa học và hình thức thanh toán */
SELECT TenKH AS [Khóa học], FORMAT(SUM(IIF(HinhThuc = N'Tiền mặt', SoTien, 0)), 'N0', 'vi-VN') AS [Tiền mặt],
       FORMAT(SUM(IIF(HinhThuc = N'Chuyển khoản', SoTien, 0)), 'N0', 'vi-VN') AS [Chuyển khoản], FORMAT(SUM(SoTien), 'N0', 'vi-VN') AS [Tổng]
FROM v_DoanhThu GROUP BY TenKH ORDER BY SUM(SoTien) DESC;

/* 3.3 – Công nợ tại ngày 31/12/2026 */
SELECT MaHV, HoTen AS [Họ tên], MaLop, FORMAT(PhaiNop, 'N0', 'vi-VN') AS [Phải nộp], FORMAT(DaThu, 'N0', 'vi-VN') AS [Đã thu],
       FORMAT(ConNo, 'N0', 'vi-VN') AS [Còn nợ], FORMAT(HanDong, 'dd/MM/yyyy') AS [Hạn], IIF(QuaHan = 1, N'Quá hạn', N'') AS [Tình trạng]
FROM dbo.fn_CongNo('2026-12-31') ORDER BY QuaHan DESC, MaLop, MaHV;

/* 5.3 – Tình trạng lớp: sĩ số, tỷ lệ lấp đầy */
SELECT MaLop, MaKH, TrangThai AS [Trạng thái], FORMAT(NgayKhaiGiang, 'dd/MM/yyyy') AS [Khai giảng],
       SiSoToiDa AS [Sĩ số tối đa], SoDangKy AS [Số đăng ký], TyLeLapDay AS [Lấp đầy %]
FROM v_SiSoLop ORDER BY NgayKhaiGiang;

/* 5.3 – Tuyển sinh: học viên mới theo tháng và khóa */
SELECT Nam AS [Năm], Thang AS [Tháng], MaKH, COUNT(*) AS [Học viên mới]
FROM v_TuyenSinh GROUP BY Nam, Thang, MaKH ORDER BY Nam, Thang, MaKH;

/* 5.4 – Kết quả học tập theo lớp: tỷ lệ đạt và phân bố xếp loại */
SELECT k.MaLop, COUNT(*) AS [Số HV], CAST(100.0 * SUM(IIF(k.KetQua = N'Đạt', 1, 0)) / COUNT(*) AS DECIMAL(5, 1)) AS [Tỷ lệ đạt %],
       SUM(IIF(XepLoai = N'Giỏi', 1, 0)) AS [Giỏi], SUM(IIF(XepLoai = N'Khá', 1, 0)) AS [Khá],
       SUM(IIF(XepLoai = N'Trung bình', 1, 0)) AS [Trung bình], SUM(IIF(XepLoai = N'Không đạt', 1, 0)) AS [Không đạt]
FROM KetQua k GROUP BY k.MaLop ORDER BY k.MaLop;

/* 5.4 – Học viên bị cảnh báo chuyên cần */
SELECT c.MaLop, c.MaHV, h.HoTen AS [Họ tên], c.SoBuoiCoMat AS [Có mặt], c.SoBuoiDaHoc AS [Đã học], c.TyLeChuyenCan AS [Chuyên cần %],
       IIF(c.NguyCoKhongDat = 1, N'Nguy cơ không đạt', N'Cảnh báo') AS [Mức]
FROM v_ChuyenCan c JOIN HocVien h ON h.MaHV = c.MaHV WHERE c.CanhBaoVang = 1 ORDER BY c.MaLop, c.TyLeChuyenCan;

/* 5.4 – Số buổi giảng dạy thực tế của giáo viên (tính cả dạy thay) */
SELECT MaGV, HoTen AS [Họ tên], SUM(SoBuoiDaDay) AS [Số buổi đã dạy] FROM v_GiangDay GROUP BY MaGV, HoTen ORDER BY MaGV;
GO
