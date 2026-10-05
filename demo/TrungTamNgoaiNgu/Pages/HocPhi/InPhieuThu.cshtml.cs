using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HocPhi;

/// <summary>R3.2 – Phiếu thu 2 liên (chứng từ xoay vòng; mẫu 02_thu_thap mục 4.2.2). HV, PH chỉ xem phiếu của mình (N2).</summary>
[Quyen("3.2")]
public class InPhieuThuModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record DongLop(string MaLop, string TenKH, string Dot, decimal HocPhiGoc, decimal Giam, decimal PhaiNop, decimal NopLanNay);

    public Data.Entities.PhieuThu Phieu { get; set; } = null!;
    public HocVien HocVien { get; set; } = null!;
    public string? SoPhieuDK { get; set; }
    public List<DongLop> Dong { get; set; } = [];
    public decimal ConNoSau { get; set; }
    public (byte Dot, DateOnly Han)? DotKeTiep { get; set; }
    public bool In { get; set; }

    public async Task<IActionResult> OnGetAsync(string so) => await NapAsync(so) ? Page() : NotFound();

    /// <summary>Bấm In: ghi nhật ký thao tác In (N3) rồi mở hộp thoại in của trình duyệt (N5).</summary>
    public async Task<IActionResult> OnPostAsync(string so)
    {
        if (!await NapAsync(so)) return NotFound();
        db.GhiNhatKy(User, "In", "PhieuThu", so, dongHo.Bay);
        await db.SaveChangesAsync();
        In = true;
        return Page();
    }

    async Task<bool> NapAsync(string so)
    {
        var p = await db.PhieuThu.AsNoTracking().Include(x => x.MaNVNavigation).Include(x => x.ChiTietPhieuThu)
                        .FirstOrDefaultAsync(x => x.SoPT == so);
        if (p == null || p.ChiTietPhieuThu.Count == 0) return false;
        var maHV = p.ChiTietPhieuThu.First().MaHV;
        if (PhamVi.HocVien(db, User, "3.2") is { } duocXem && !await duocXem.ContainsAsync(maHV)) return false;
        Phieu = p;
        HocVien = (await db.HocVien.FindAsync(maHV))!;
        var lop = p.ChiTietPhieuThu.Select(c => c.MaLop).Distinct().ToList();
        var hp = await db.HocPhi.AsNoTracking().Include(h => h.DotHocPhi).Include(h => h.DangKy.MaLopNavigation.MaKHNavigation)
                         .Where(h => h.MaHV == maHV && lop.Contains(h.MaLop)).ToListAsync();
        SoPhieuDK = hp.Select(h => h.DangKy.SoPhieuDK).Distinct().FirstOrDefault();
        // Đã thu tính đến hết phiếu này (theo ngày thu, rồi số phiếu) – để in đúng "còn nợ sau phiếu này" kể cả khi in lại về sau
        var daThu = await db.ChiTietPhieuThu.Where(c => c.MaHV == maHV && lop.Contains(c.MaLop))
                            .Where(c => c.SoPTNavigation.NgayThu < p.NgayThu || (c.SoPTNavigation.NgayThu == p.NgayThu && string.Compare(c.SoPT, p.SoPT) <= 0))
                            .GroupBy(c => new { c.MaLop, c.Dot }).Select(g => new { g.Key.MaLop, g.Key.Dot, Tien = g.Sum(x => x.SoTien) }).ToListAsync();
        Dong = hp.OrderBy(h => h.MaLop).Select(h =>
        {
            var phaiNop = h.DotHocPhi.Sum(d => d.SoTien);
            var ct = p.ChiTietPhieuThu.Where(c => c.MaLop == h.MaLop).ToList();
            return new DongLop(h.MaLop, h.DangKy.MaLopNavigation.MaKHNavigation.TenKH, string.Join(", ", ct.Select(c => c.Dot).OrderBy(x => x)),
                               h.HocPhiGoc, h.HocPhiGoc - phaiNop, phaiNop, ct.Sum(c => c.SoTien));
        }).ToList();
        var conNo = hp.SelectMany(h => h.DotHocPhi)
                      .Select(d => (d.Dot, d.HanDong, No: d.SoTien - daThu.Where(t => t.MaLop == d.MaLop && t.Dot == d.Dot).Sum(t => t.Tien)))
                      .Where(x => x.No > 0).ToList();
        ConNoSau = conNo.Sum(x => x.No);
        DotKeTiep = conNo.OrderBy(x => x.HanDong).Select(x => ((byte, DateOnly)?)(x.Dot, x.HanDong)).FirstOrDefault();
        return true;
    }
}
