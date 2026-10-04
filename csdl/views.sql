/* =====================================================================
   View và hàm phục vụ xử lý và báo cáo MIS (3.4, 5.3, 5.4).
   Các thuộc tính thứ sinh của 3.1 (mục 5) được TÍNH ở đây, không lưu trong bảng.
   Con số chính sách đọc từ bảng ThamSo qua dbo.fn_ThamSo, không viết cứng.
   ===================================================================== */
USE TrungTamNgoaiNgu;
GO

/* Giá trị một tham số (bảng ThamSo – 3.1 E26) */
CREATE FUNCTION dbo.fn_ThamSo (@Ma VARCHAR(30)) RETURNS DECIMAL(10, 2)
AS BEGIN
    RETURN (SELECT GiaTri FROM dbo.ThamSo WHERE MaThamSo = @Ma);
END;
GO

/* Chuỗi đăng ký (quan hệ bậc 1 Q12): mỗi đăng ký và đăng ký ĐẦU CHUỖI của nó.
   Khoản học phí chỉ nằm ở đăng ký đầu chuỗi (3.1 E13), nên mọi phép tính học phí
   của một đăng ký chuyển lớp / học lại đều đi về đầu chuỗi. */
CREATE VIEW dbo.v_ChuoiDangKy AS
WITH c AS (
    SELECT MaHV, MaLop, MaLop AS MaLopDau FROM dbo.DangKy WHERE MaLopGoc IS NULL
    UNION ALL
    SELECT d.MaHV, d.MaLop, c.MaLopDau
    FROM dbo.DangKy d JOIN c ON d.MaHV = c.MaHV AND d.MaLopGoc = c.MaLop
)
SELECT MaHV, MaLop, MaLopDau FROM c;
GO

/* Học phí của từng đăng ký: Phải nộp, Đã thu, Còn nợ, Hiệu lực (QT05) – 04_dac_ta mục 4.5 */
CREATE VIEW dbo.v_HocPhiDangKy AS
SELECT d.MaHV, d.MaLop, d.TrangThai, c.MaLopDau,
       p.PhaiNop,
       ISNULL(t.DaThu, 0) AS DaThu,
       p.PhaiNop - ISNULL(t.DaThu, 0) AS ConNo,
       CASE WHEN d.TrangThai = N'Đã đăng ký'
             AND ISNULL(t.DaThu, 0) >= p.PhaiNop * dbo.fn_ThamSo('TY_LE_DOT_1') / 100
            THEN 1 ELSE 0 END AS HieuLuc
FROM dbo.DangKy d
JOIN dbo.v_ChuoiDangKy c ON c.MaHV = d.MaHV AND c.MaLop = d.MaLop
JOIN (SELECT MaHV, MaLop, SUM(SoTien) AS PhaiNop FROM dbo.DotHocPhi GROUP BY MaHV, MaLop) p
     ON p.MaHV = c.MaHV AND p.MaLop = c.MaLopDau
LEFT JOIN (SELECT MaHV, MaLop, SUM(SoTien) AS DaThu FROM dbo.ChiTietPhieuThu GROUP BY MaHV, MaLop) t
     ON t.MaHV = c.MaHV AND t.MaLop = c.MaLopDau;
GO

/* Công nợ tại một ngày (3.3, QT10, QT16): chỉ xét đăng ký HIỆN TẠI của mỗi chuỗi
   (không có đăng ký nào chuyển từ nó), bỏ đăng ký Nghỉ học. Quá hạn = đã qua hạn
   của đợt đầu tiên chưa đóng đủ (tính lũy kế theo đợt). */
CREATE FUNCTION dbo.fn_CongNo (@Ngay DATE) RETURNS TABLE
AS RETURN
SELECT h.MaHV, hv.HoTen, h.MaLop, h.MaLopDau, h.PhaiNop, h.DaThu, h.ConNo, han.HanDong,
       CASE WHEN han.HanDong < @Ngay THEN 1 ELSE 0 END AS QuaHan
FROM dbo.v_HocPhiDangKy h
JOIN dbo.HocVien hv ON hv.MaHV = h.MaHV
OUTER APPLY (
    SELECT MIN(dt.HanDong) AS HanDong
    FROM dbo.DotHocPhi dt
    WHERE dt.MaHV = h.MaHV AND dt.MaLop = h.MaLopDau
      AND (SELECT SUM(x.SoTien) FROM dbo.DotHocPhi x
           WHERE x.MaHV = dt.MaHV AND x.MaLop = dt.MaLop AND x.Dot <= dt.Dot) > h.DaThu
) han
WHERE h.ConNo > 0
  AND h.TrangThai <> N'Nghỉ học'
  AND NOT EXISTS (SELECT 1 FROM dbo.DangKy n WHERE n.MaHV = h.MaHV AND n.MaLopGoc = h.MaLop);
GO

/* Doanh thu (3.4): từng dòng tiền đã thu, kèm kỳ, khóa học, hình thức */
CREATE VIEW dbo.v_DoanhThu AS
SELECT pt.SoPT, pt.NgayThu, YEAR(pt.NgayThu) AS Nam, MONTH(pt.NgayThu) AS Thang, pt.HinhThuc,
       l.MaKH, k.TenKH, ct.MaHV, ct.MaLop, ct.Dot, ct.SoTien
FROM dbo.PhieuThu pt
JOIN dbo.ChiTietPhieuThu ct ON ct.SoPT = pt.SoPT
JOIN dbo.LopHoc l ON l.MaLop = ct.MaLop
JOIN dbo.KhoaHoc k ON k.MaKH = l.MaKH;
GO

/* Sĩ số và tỷ lệ lấp đầy (2.1, 2.2, 5.3): đăng ký giữ chỗ = không ở trạng thái Chuyển lớp, Nghỉ học */
CREATE VIEW dbo.v_SiSoLop AS
SELECT l.MaLop, l.MaKH, l.TrangThai, l.NgayKhaiGiang, l.SiSoToiDa,
       COUNT(CASE WHEN d.TrangThai NOT IN (N'Chuyển lớp', N'Nghỉ học') THEN 1 END) AS SoDangKy,
       CAST(100.0 * COUNT(CASE WHEN d.TrangThai NOT IN (N'Chuyển lớp', N'Nghỉ học') THEN 1 END)
            / l.SiSoToiDa AS DECIMAL(5, 1)) AS TyLeLapDay
FROM dbo.LopHoc l
LEFT JOIN dbo.DangKy d ON d.MaLop = l.MaLop
GROUP BY l.MaLop, l.MaKH, l.TrangThai, l.NgayKhaiGiang, l.SiSoToiDa;
GO

/* Chuyên cần (4.1, QT11, QT12, cây quyết định 04_dac_ta mục 2.2).
   Số buổi đã học = số buổi đã điểm danh cho đăng ký đó (giáo viên điểm danh mọi học viên
   của lớp ở mỗi buổi đã dạy, nên học viên chuyển vào giữa chừng chỉ tính từ khi vào lớp). */
CREATE VIEW dbo.v_ChuyenCan AS
WITH t AS (
    SELECT d.MaHV, d.MaLop, d.TrangThai,
           COUNT(dd.SoBuoi) AS SoBuoiDaHoc,
           SUM(CASE WHEN dd.TrangThai IN ('x', 'M') THEN 1 ELSE 0 END) AS SoBuoiCoMat
    FROM dbo.DangKy d
    LEFT JOIN dbo.DiemDanh dd ON dd.MaHV = d.MaHV AND dd.MaLop = d.MaLop
    GROUP BY d.MaHV, d.MaLop, d.TrangThai
)
SELECT MaHV, MaLop, TrangThai, SoBuoiDaHoc, SoBuoiCoMat,
       CAST(CASE WHEN SoBuoiDaHoc = 0 THEN NULL ELSE 100.0 * SoBuoiCoMat / SoBuoiDaHoc END AS DECIMAL(5, 1)) AS TyLeChuyenCan,
       CASE WHEN SoBuoiDaHoc > 0 AND 100.0 * (SoBuoiDaHoc - SoBuoiCoMat) / SoBuoiDaHoc > dbo.fn_ThamSo('NGUONG_CANH_BAO_VANG')
            THEN 1 ELSE 0 END AS CanhBaoVang,
       CASE WHEN SoBuoiDaHoc > 0 AND 100.0 * SoBuoiCoMat / SoBuoiDaHoc < dbo.fn_ThamSo('NGUONG_CHUYEN_CAN_DAT')
            THEN 1 ELSE 0 END AS NguyCoKhongDat
FROM t;
GO

/* Kết quả TÍNH LẠI theo QT13–QT15 (bảng quyết định 04_dac_ta mục 3.2), để đối chiếu với
   giá trị đã chốt trong bảng KetQua. Chỉ xét đăng ký đã có đủ điểm giữa kỳ và cuối kỳ. */
CREATE VIEW dbo.v_KetQuaTinh AS
WITH d AS (
    SELECT c.MaHV, c.MaLop, c.SoBuoiDaHoc, c.SoBuoiCoMat,
           CAST(10.0 * c.SoBuoiCoMat / NULLIF(c.SoBuoiDaHoc, 0) AS DECIMAL(10, 4)) AS DiemChuyenCan,
           gk.Diem AS DiemGiuaKy, ck.Diem AS DiemCuoiKy, k.TSChuyenCan, k.TSGiuaKy, k.TSCuoiKy
    FROM dbo.v_ChuyenCan c
    JOIN dbo.LopHoc l ON l.MaLop = c.MaLop
    JOIN dbo.KhoaHoc k ON k.MaKH = l.MaKH
    JOIN dbo.Diem gk ON gk.MaHV = c.MaHV AND gk.MaLop = c.MaLop AND gk.LoaiDiem = N'Giữa kỳ'
    JOIN dbo.Diem ck ON ck.MaHV = c.MaHV AND ck.MaLop = c.MaLop AND ck.LoaiDiem = N'Cuối kỳ'
), t AS (
    SELECT *, CAST(ROUND((TSChuyenCan * DiemChuyenCan + TSGiuaKy * DiemGiuaKy + TSCuoiKy * DiemCuoiKy) / 100, 1)
                   AS DECIMAL(3, 1)) AS DiemTongKet
    FROM d
)
SELECT MaHV, MaLop, CAST(DiemChuyenCan AS DECIMAL(4, 2)) AS DiemChuyenCan, DiemGiuaKy, DiemCuoiKy, DiemTongKet,
       CASE WHEN DiemTongKet >= dbo.fn_ThamSo('DIEM_GIOI') THEN N'Giỏi'
            WHEN DiemTongKet >= dbo.fn_ThamSo('DIEM_KHA') THEN N'Khá'
            WHEN DiemTongKet >= dbo.fn_ThamSo('DIEM_DAT') THEN N'Trung bình'
            ELSE N'Không đạt' END AS XepLoai,
       CASE WHEN DiemTongKet >= dbo.fn_ThamSo('DIEM_DAT')
             AND 100.0 * SoBuoiCoMat / SoBuoiDaHoc >= dbo.fn_ThamSo('NGUONG_CHUYEN_CAN_DAT')
            THEN N'Đạt' ELSE N'Không đạt' END AS KetQua
FROM t;
GO

/* Học viên cũ (QT01): đã học hết một lớp (lớp Kết thúc, đăng ký không chuyển đi, không nghỉ, không bảo lưu).
   NgayKetThuc = ngày buổi học cuối đã dạy; dùng để xét ưu đãi tại ngày đăng ký. */
CREATE VIEW dbo.v_HocVienCu AS
SELECT d.MaHV, d.MaLop, MAX(b.Ngay) AS NgayKetThuc
FROM dbo.DangKy d
JOIN dbo.LopHoc l ON l.MaLop = d.MaLop AND l.TrangThai = N'Kết thúc'
JOIN dbo.BuoiHoc b ON b.MaLop = d.MaLop AND b.TrangThai = N'Đã dạy'
WHERE d.TrangThai = N'Đã đăng ký'
GROUP BY d.MaHV, d.MaLop;
GO

/* Giảng dạy (5.4): số buổi đã dạy thực tế theo giáo viên, theo tháng (tính cả dạy thay) */
CREATE VIEW dbo.v_GiangDay AS
SELECT b.MaGV, g.HoTen, YEAR(b.Ngay) AS Nam, MONTH(b.Ngay) AS Thang, COUNT(*) AS SoBuoiDaDay
FROM dbo.BuoiHoc b
JOIN dbo.GiaoVien g ON g.MaGV = b.MaGV
WHERE b.TrangThai = N'Đã dạy'
GROUP BY b.MaGV, g.HoTen, YEAR(b.Ngay), MONTH(b.Ngay);
GO

/* Tuyển sinh (5.3): học viên MỚI = tính theo đăng ký đầu tiên của học viên (ngày trên phiếu) */
CREATE VIEW dbo.v_TuyenSinh AS
WITH t AS (
    SELECT d.MaHV, p.NgayDK, l.MaKH,
           ROW_NUMBER() OVER (PARTITION BY d.MaHV ORDER BY p.NgayDK, d.MaLop) AS ThuTu
    FROM dbo.DangKy d
    JOIN dbo.PhieuDangKy p ON p.SoPhieuDK = d.SoPhieuDK
    JOIN dbo.LopHoc l ON l.MaLop = d.MaLop
    WHERE d.MaLopGoc IS NULL
)
SELECT MaHV, NgayDK AS NgayDangKyDau, YEAR(NgayDK) AS Nam, MONTH(NgayDK) AS Thang, MaKH
FROM t WHERE ThuTu = 1;
GO
