using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.NghiepVu;

/// <summary>Các quy tắc nghiệp vụ không phụ thuộc CSDL, để kiểm thử đơn vị (05_module mục 8, bước 5).
/// Con số chính sách (15%, 50%, 24 giờ…) luôn truyền vào từ bảng ThamSo, không viết cứng.</summary>
public static class QuyTac
{
    // ---------------- 3.1 – Bảng quyết định tính học phí (04_dac_ta mục 3.1, QT01–QT05) ----------------

    public record UuDaiCo(string MaUD, string LoaiUD, decimal TyLeGiam, DateOnly NgayBatDau, DateOnly NgayKetThuc);

    /// <summary>C1 học viên cũ, C2 đăng ký nhóm, C3 đóng một lần. Điều kiện chỉ tính "Có" khi có ưu đãi cùng loại
    /// còn hiệu lực tại ngày đăng ký (QT04); tổng giảm không vượt <paramref name="toiDa"/>.</summary>
    public static (decimal TyLe, List<string> MaUD) XetUuDai(bool hocVienCu, bool nhom, bool motLan,
        IEnumerable<UuDaiCo> uuDai, DateOnly ngay, decimal toiDa)
    {
        var dieuKien = new Dictionary<string, bool> { ["Học viên cũ"] = hocVienCu, ["Đăng ký nhóm"] = nhom, ["Đóng một lần"] = motLan };
        var chon = uuDai.Where(u => dieuKien.GetValueOrDefault(u.LoaiUD) && u.NgayBatDau <= ngay && ngay <= u.NgayKetThuc)
                        .GroupBy(u => u.LoaiUD)
                        .Select(g => g.MaxBy(u => u.TyLeGiam)!)
                        .ToList();
        return (Math.Min(chon.Sum(u => u.TyLeGiam), toiDa), chon.Select(u => u.MaUD).ToList());
    }

    /// <summary>Chia khoản phải nộp thành 1 đợt (hạn: ngày khai giảng) hoặc 2 đợt (đợt 1 ≥ tỷ lệ đợt 1, làm tròn
    /// nghìn đồng; hạn đợt 2: buổi giữa khóa) – QT05. Cách làm tròn giống csdl/sinh_du_lieu.py.</summary>
    public static List<(byte Dot, decimal SoTien, DateOnly Han)> ChiaDot(decimal hocPhiGoc, decimal tyLeGiam, byte soDot,
        decimal tyLeDot1, DateOnly khaiGiang, DateOnly hanDot2)
    {
        var phaiNop = Math.Round(hocPhiGoc * (100 - tyLeGiam) / 100, 0, MidpointRounding.AwayFromZero);
        if (soDot == 1) return [(1, phaiNop, khaiGiang)];
        var dot1 = Math.Round(phaiNop * tyLeDot1 / 100 / 1000, 0, MidpointRounding.AwayFromZero) * 1000;
        return [(1, dot1, khaiGiang), (2, phaiNop - dot1, hanDot2)];
    }

    /// <summary>Ngày của buổi thứ <paramref name="n"/> theo lịch tuần (thứ 2–8, 8 = chủ nhật), tính từ ngày khai giảng.
    /// Dùng làm hạn đợt 2 (buổi giữa khóa) khi lớp chưa sinh buổi học.</summary>
    public static DateOnly NgayBuoiThu(int n, DateOnly khaiGiang, IReadOnlyCollection<byte> thu)
    {
        if (n < 1 || thu.Count == 0) throw new ArgumentException("Cần n ≥ 1 và lịch tuần có ít nhất một ngày.");
        for (var d = khaiGiang; ; d = d.AddDays(1))
            if (thu.Contains((byte)(d.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)d.DayOfWeek + 1)) && --n == 0)
                return d;
    }

    // ---------------- 3.2 – Lập phiếu thu (04_dac_ta mục 1.4, QT05) ----------------

    /// <summary>Một dòng trên phiếu thu. PhaiNop, DaThu: của cả đăng ký; ConNoDot: của riêng đợt này.</summary>
    public record DongThu(string MaHV, string MaLop, byte Dot, decimal PhaiNop, decimal DaThu, decimal ConNoDot, decimal SoTien);

    /// <summary>Ném LoiNghiepVu ở dòng sai đầu tiên.</summary>
    public static void KiemTraPhieuThu(IReadOnlyList<DongThu> dong, decimal tyLeDot1)
    {
        if (dong.Count == 0) throw new LoiNghiepVu("CK_ChiTietPhieuThu_SoTien", "phiếu chưa có dòng nào");
        foreach (var d in dong)
        {
            var noi = $"{d.MaLop} đợt {d.Dot}";
            if (d.SoTien <= 0) throw new LoiNghiepVu("CK_ChiTietPhieuThu_SoTien", noi);
            if (d.SoTien > d.ConNoDot) throw new LoiNghiepVu("QT05#2", $"{noi}: còn nợ {d.ConNoDot:N0} đ");
            if (d.Dot == 1)
            {
                var nopPhieuNay = dong.Where(x => x.MaHV == d.MaHV && x.MaLop == d.MaLop).Sum(x => x.SoTien);
                if (d.DaThu + nopPhieuNay < d.PhaiNop * tyLeDot1 / 100)
                    throw new LoiNghiepVu("QT05", $"{noi}: tối thiểu {d.PhaiNop * tyLeDot1 / 100 - d.DaThu:N0} đ");
            }
        }
    }

    // ---------------- 4.1 – Điểm danh (04_dac_ta mục 1.6, QT11) ----------------

    /// <summary>Được ghi / sửa điểm danh từ lúc buổi học bắt đầu đến <paramref name="gioSua"/> giờ sau khi kết thúc.</summary>
    public static bool DuocDiemDanh(DateOnly ngay, TimeOnly batDau, TimeOnly ketThuc, DateTime bay, decimal gioSua) =>
        bay >= ngay.ToDateTime(batDau) && bay <= ngay.ToDateTime(ketThuc).AddHours((double)gioSua);

    public static readonly string[] TrangThaiDiemDanh = ["x", "M", "P", "K"];

    // ---------------- 4.3 – Tổng kết, bảng quyết định xếp loại R1–R5 (04_dac_ta mục 1.7, 3.2; QT13–QT15) ----------------

    /// <summary>Ngưỡng của QT14, QT15 (ThamSo DIEM_GIOI, DIEM_KHA, DIEM_DAT, NGUONG_CHUYEN_CAN_DAT).</summary>
    public record NguongXepLoai(decimal Gioi, decimal Kha, decimal Dat, decimal ChuyenCanDat);

    public record KetQuaTinh(decimal TyLeChuyenCan, decimal DiemChuyenCan, decimal DiemTongKet, string XepLoai, string KetQua);

    /// <summary>Trọng số tính theo %. Cách làm tròn giống view dbo.v_KetQuaTinh (điểm chuyên cần 4 chữ số, tổng kết 1 chữ số).</summary>
    public static KetQuaTinh TongKet(int soBuoiDaHoc, int soBuoiCoMat, decimal giuaKy, decimal cuoiKy,
        decimal tsChuyenCan, decimal tsGiuaKy, decimal tsCuoiKy, NguongXepLoai n)
    {
        var tyLe = soBuoiDaHoc == 0 ? 0 : 100m * soBuoiCoMat / soBuoiDaHoc;
        var cc = Math.Round(tyLe / 10, 4, MidpointRounding.AwayFromZero);
        var tk = Math.Round((tsChuyenCan * cc + tsGiuaKy * giuaKy + tsCuoiKy * cuoiKy) / 100, 1, MidpointRounding.AwayFromZero);
        var xepLoai = tk >= n.Gioi ? "Giỏi" : tk >= n.Kha ? "Khá" : tk >= n.Dat ? "Trung bình" : "Không đạt";
        var dat = tk >= n.Dat && tyLe >= n.ChuyenCanDat;   // R4: điểm đủ nhưng chuyên cần < 70% → Không đạt, giữ xếp loại theo điểm
        return new(Math.Round(tyLe, 1, MidpointRounding.AwayFromZero), cc, tk, xepLoai, dat ? "Đạt" : "Không đạt");
    }

    // ---------------- 4.4 – Cấp chứng nhận (04_dac_ta mục 1.8, QT15) ----------------

    /// <summary>Chỉ cấp cho học viên có kết quả "Đạt" đã được QL đào tạo duyệt (danh sách học viên đạt từ 4.3).</summary>
    public static bool DuocCapChungNhan(string? ketQua, DateOnly? ngayDuyet) => ketQua == "Đạt" && ngayDuyet != null;
}
