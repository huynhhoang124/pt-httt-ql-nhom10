using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.BaoCao;

/// <summary>F3.4 chọn kỳ + R3.4 Báo cáo doanh thu, công nợ (module 3.4; v_DoanhThu, fn_CongNo). Doanh thu theo kỳ, khóa,
/// hình thức, tháng; so với kỳ trước cùng độ dài; kèm tổng công nợ tại cuối kỳ (luồng "Tổng công nợ" 3.3 → 3.4).</summary>
[Quyen("3.4")]
public class DoanhThuModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Nhom(string Ten, decimal Tien, int SoPhieu);

    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public string? Khoa { get; set; }
    public Dictionary<string, string> CacKhoa { get; set; } = [];
    public decimal Tong { get; set; }
    public decimal TongKyTruoc { get; set; }
    public (DateOnly Tu, DateOnly Den) KyTruoc { get; set; }
    public List<Nhom> TheoKhoa { get; set; } = [];
    public List<Nhom> TheoHinhThuc { get; set; } = [];
    public List<Nhom> TheoThang { get; set; } = [];
    public decimal CongNo { get; set; }
    public int SoKhoanNo { get; set; }
    public decimal QuaHan { get; set; }
    public int SoKhoanQuaHan { get; set; }
    public DateTime LapLuc { get; set; }

    public async Task OnGetAsync(DateOnly? tuNgay, DateOnly? denNgay, string? khoa)
    {
        LapLuc = dongHo.Bay;
        DenNgay = denNgay ?? dongHo.HomNay;
        TuNgay = tuNgay ?? new DateOnly(DenNgay.Year, DenNgay.Month, 1).AddMonths(-3);
        if (TuNgay > DenNgay) (TuNgay, DenNgay) = (DenNgay, TuNgay);
        Khoa = string.IsNullOrEmpty(khoa) ? null : khoa;
        CacKhoa = await db.KhoaHoc.OrderBy(k => k.MaKH).ToDictionaryAsync(k => k.MaKH, k => k.TenKH);

        var soNgay = DenNgay.DayNumber - TuNgay.DayNumber + 1;
        KyTruoc = (TuNgay.AddDays(-soNgay), TuNgay.AddDays(-1));
        var dt = db.v_DoanhThu.Where(x => Khoa == null || x.MaKH == Khoa);
        var ky = await dt.Where(x => x.NgayThu >= TuNgay && x.NgayThu <= DenNgay).ToListAsync();
        Tong = ky.Sum(x => x.SoTien);
        TongKyTruoc = await dt.Where(x => x.NgayThu >= KyTruoc.Tu && x.NgayThu <= KyTruoc.Den).SumAsync(x => x.SoTien);
        Nhom Gop(IGrouping<string, Data.Entities.v_DoanhThu> g) => new(g.Key, g.Sum(x => x.SoTien), g.Select(x => x.SoPT).Distinct().Count());
        TheoKhoa = ky.GroupBy(x => $"{x.MaKH} – {x.TenKH}").Select(Gop).OrderByDescending(n => n.Tien).ToList();
        TheoHinhThuc = ky.GroupBy(x => x.HinhThuc).Select(Gop).OrderByDescending(n => n.Tien).ToList();
        TheoThang = ky.GroupBy(x => $"{x.Thang:00}/{x.Nam}").Select(Gop).OrderBy(n => n.Ten[3..] + n.Ten[..2]).ToList();

        var no = db.CongNoTai(DenNgay);
        if (Khoa != null) no = no.Where(c => db.LopHoc.Any(l => l.MaLop == c.MaLopDau && l.MaKH == Khoa));
        var ds = await no.ToListAsync();
        (CongNo, SoKhoanNo) = (ds.Sum(c => c.ConNo), ds.Count);
        (QuaHan, SoKhoanQuaHan) = (ds.Where(c => c.QuaHan == 1).Sum(c => c.ConNo), ds.Count(c => c.QuaHan == 1));
    }
}
