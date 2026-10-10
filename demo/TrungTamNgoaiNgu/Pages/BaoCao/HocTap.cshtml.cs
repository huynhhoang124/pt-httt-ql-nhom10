using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.BaoCao;

/// <summary>F5.3 chọn kỳ + R5.4a Báo cáo kết quả học tập + R5.4b Chuyên cần, kết quả, giảng dạy (module 5.4; KetQua, v_ChuyenCan,
/// v_GiangDay). Theo lớp khai giảng trong kỳ, có so sánh tỷ lệ đạt, chuyên cần với kỳ trước cùng độ dài (đặc trưng MIS).
/// Giáo viên chỉ thấy lớp mình dạy và dòng giảng dạy của mình.</summary>
[Quyen("5.4")]
public class HocTapModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Lop(string MaLop, string Khoa, string GiaoVien, string TrangThai, int SoHV, decimal? ChuyenCan, int CanhBao,
                      int SoKQ, int Dat, int Gioi, int Kha, int TrungBinh, int KhongDat, decimal? DiemTB);
    public record GiangDay(string MaGV, string HoTen, int SoBuoi, int SoLop, decimal? TyLeDat);

    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public List<Lop> CacLop { get; set; } = [];
    public List<GiangDay> CacGV { get; set; } = [];
    public (decimal? Dat, decimal? ChuyenCan) KyNay { get; set; }
    public (decimal? Dat, decimal? ChuyenCan) KyTruoc { get; set; }
    public DateTime LapLuc { get; set; }

    public async Task<IActionResult> OnGetAsync(DateOnly? tuNgay, DateOnly? denNgay, string? xuat)
    {
        LapLuc = dongHo.Bay;
        DenNgay = denNgay ?? dongHo.HomNay;
        TuNgay = tuNgay ?? DenNgay.AddYears(-1);
        if (TuNgay > DenNgay) (TuNgay, DenNgay) = (DenNgay, TuNgay);
        var gv = User.VaiTro() == "GV" ? User.MaGV() : null;   // phạm vi x
        CacLop = await TheoLopAsync(TuNgay, DenNgay, gv);
        var soNgay = DenNgay.DayNumber - TuNgay.DayNumber + 1;
        KyNay = TongHop(CacLop);
        KyTruoc = TongHop(await TheoLopAsync(TuNgay.AddDays(-soNgay), TuNgay.AddDays(-1), gv));

        int tuThang = TuNgay.Year * 12 + TuNgay.Month, denThang = DenNgay.Year * 12 + DenNgay.Month;
        var day = await db.v_GiangDay.Where(x => x.Nam * 12 + x.Thang >= tuThang && x.Nam * 12 + x.Thang <= denThang && (gv == null || x.MaGV == gv))
                          .GroupBy(x => new { x.MaGV, x.HoTen }).Select(g => new { g.Key.MaGV, g.Key.HoTen, SoBuoi = g.Sum(x => x.SoBuoiDaDay) ?? 0 }).ToListAsync();
        var gvLop = await db.LopHoc.Where(l => CacLop.Select(c => c.MaLop).Contains(l.MaLop)).ToDictionaryAsync(l => l.MaLop, l => l.MaGV);
        CacGV = day.Select(d =>
        {
            var lop = CacLop.Where(c => gvLop.GetValueOrDefault(c.MaLop) == d.MaGV).ToList();
            int kq = lop.Sum(c => c.SoKQ), dat = lop.Sum(c => c.Dat);
            return new GiangDay(d.MaGV, d.HoTen, d.SoBuoi, lop.Count, kq == 0 ? null : 100m * dat / kq);
        }).OrderBy(g => g.MaGV).ToList();

        if (xuat == "excel")
            return XuatExcel.Tep($"KetQuaHocTap_{TuNgay:yyyyMMdd}_{DenNgay:yyyyMMdd}",
                ["Lớp", "Khóa học", "Giáo viên", "Trạng thái", "Số HV", "Chuyên cần TB %", "Cảnh báo vắng", "Đã tổng kết", "Đạt", "Giỏi", "Khá", "Trung bình", "Không đạt", "Điểm TB"],
                CacLop.Select(c => new object?[] { c.MaLop, c.Khoa, c.GiaoVien, c.TrangThai, c.SoHV, Tron(c.ChuyenCan), c.CanhBao, c.SoKQ, c.Dat, c.Gioi, c.Kha, c.TrungBinh, c.KhongDat, Tron(c.DiemTB) }));
        return Page();
    }

    static decimal? Tron(decimal? x) => x is null ? null : Math.Round(x.Value, 1);

    public static (decimal? Dat, decimal? ChuyenCan) TongHop(List<Lop> ds)
    {
        int kq = ds.Sum(c => c.SoKQ);
        var cc = ds.Where(c => c.ChuyenCan != null).ToList();
        return (kq == 0 ? null : 100m * ds.Sum(c => c.Dat) / kq,
                cc.Sum(c => c.SoHV) == 0 ? null : cc.Sum(c => c.ChuyenCan!.Value * c.SoHV) / cc.Sum(c => c.SoHV));
    }

    async Task<List<Lop>> TheoLopAsync(DateOnly tu, DateOnly den, string? gv)
    {
        var lop = await db.LopHoc.AsNoTracking().Include(l => l.MaKHNavigation).Include(l => l.MaGVNavigation)
                          .Where(l => (l.TrangThai == "Đang học" || l.TrangThai == "Kết thúc") && l.NgayKhaiGiang >= tu && l.NgayKhaiGiang <= den
                                      && (gv == null || l.MaGV == gv))
                          .OrderBy(l => l.NgayKhaiGiang).ToListAsync();
        var ma = lop.Select(l => l.MaLop).ToList();
        var cc = await db.v_ChuyenCan.Where(x => ma.Contains(x.MaLop) && x.TrangThai == "Đã đăng ký").ToListAsync();
        var kq = await db.KetQua.Where(k => ma.Contains(k.MaLop) && k.NgayDuyet != null).ToListAsync();
        return lop.Select(l =>
        {
            var c = cc.Where(x => x.MaLop == l.MaLop && x.TyLeChuyenCan != null).ToList();
            var k = kq.Where(x => x.MaLop == l.MaLop).ToList();
            return new Lop(l.MaLop, l.MaKHNavigation.TenKH, l.MaGVNavigation.HoTen, l.TrangThai, c.Count,
                c.Count == 0 ? null : c.Average(x => x.TyLeChuyenCan!.Value), c.Count(x => x.CanhBaoVang == 1),
                k.Count, k.Count(x => x.KetQua1 == "Đạt"), k.Count(x => x.XepLoai == "Giỏi"), k.Count(x => x.XepLoai == "Khá"),
                k.Count(x => x.XepLoai == "Trung bình"), k.Count(x => x.XepLoai == "Không đạt"), k.Count == 0 ? null : k.Average(x => x.DiemTongKet));
        }).ToList();
    }
}
