using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HocPhi;

/// <summary>R3.2 – Tra cứu phiếu thu để xem, in lại. HV, PH chỉ thấy phiếu của mình (N2).</summary>
[Quyen("3.2")]
public class DanhSachPhieuThuModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Dong(string SoPT, DateOnly NgayThu, string MaHV, string HoTen, string NguoiNop, string HinhThuc, decimal Tong);

    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public string? Hv { get; set; }
    public List<Dong> DanhSach { get; set; } = [];

    public async Task OnGetAsync(DateOnly? tuNgay, DateOnly? denNgay, string? hv)
    {
        DenNgay = denNgay ?? dongHo.HomNay;
        TuNgay = tuNgay ?? DenNgay.AddMonths(-3);
        Hv = hv?.Trim().ToUpperInvariant();
        var ct = db.ChiTietPhieuThu.AsQueryable();
        if (PhamVi.HocVien(db, User, "3.2") is { } duocXem) ct = ct.Where(c => duocXem.Contains(c.MaHV));
        if (!string.IsNullOrEmpty(Hv)) ct = ct.Where(c => c.MaHV == Hv);
        var ds = await (from c in ct
                        join p in db.PhieuThu on c.SoPT equals p.SoPT
                        join h in db.HocVien on c.MaHV equals h.MaHV
                        where p.NgayThu >= TuNgay && p.NgayThu <= DenNgay
                        group c.SoTien by new { p.SoPT, p.NgayThu, c.MaHV, h.HoTen, p.NguoiNop, p.HinhThuc } into g
                        select new { g.Key, Tong = g.Sum() }).ToListAsync();
        DanhSach = ds.Select(x => new Dong(x.Key.SoPT, x.Key.NgayThu, x.Key.MaHV, x.Key.HoTen, x.Key.NguoiNop, x.Key.HinhThuc, x.Tong))
                     .OrderByDescending(d => d.NgayThu).ThenByDescending(d => d.SoPT).ToList();
    }
}
