using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HocPhi;

/// <summary>R3.1 – Toàn bộ học phí đã lập, gồm cả đăng ký đã kết thúc hoặc nghỉ học.</summary>
[Quyen("3.1")]
public class HocPhiPhaiThuModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }
    [BindProperty(SupportsGet = true)] public string? MaHV { get; set; }
    public List<Data.Entities.HocPhi> DanhSach { get; set; } = [];
    public List<string> CacLop { get; set; } = [];
    public TrangDs Trang { get; set; } = new(1, 50, 0);
    public decimal TongGoc { get; set; }
    public decimal TongPhaiNop { get; set; }
    public decimal TongDaThu { get; set; }
    public DateTime LapLuc => dongHo.Bay;

    public static decimal PhaiNop(Data.Entities.HocPhi h) => h.DotHocPhi.Sum(d => d.SoTien);
    public static decimal DaThu(Data.Entities.DotHocPhi d) => d.ChiTietPhieuThu.Sum(c => c.SoTien);
    public static string TyLe(decimal value) => value.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
    public static string UuDai(Data.Entities.HocPhi h) => string.Join("; ", h.MaUD.OrderBy(u => u.MaUD)
        .Select(u => $"{u.TenUD} ({TyLe(u.TyLeGiam)}%)"));

    public async Task<IActionResult> OnGetAsync(string? xuat, int? trang, int? so)
    {
        var q = db.HocPhi.AsNoTracking().AsQueryable();
        if (PhamVi.HocVien(db, User, "3.1") is { } hv) q = q.Where(h => hv.Contains(h.MaHV));
        CacLop = await q.Select(h => h.MaLop).Distinct().OrderBy(l => l).ToListAsync();
        MaHV = MaHV?.Trim().ToUpperInvariant();
        if (!string.IsNullOrEmpty(Lop)) q = q.Where(h => h.MaLop == Lop);
        if (!string.IsNullOrEmpty(MaHV)) q = q.Where(h => h.MaHV == MaHV);
        TongGoc = await q.SumAsync(h => (decimal?)h.HocPhiGoc) ?? 0;
        var dot = q.SelectMany(h => h.DotHocPhi);
        TongPhaiNop = await dot.SumAsync(d => (decimal?)d.SoTien) ?? 0;
        TongDaThu = await dot.SelectMany(d => d.ChiTietPhieuThu).SumAsync(c => (decimal?)c.SoTien) ?? 0;
        var chiTiet = q.Include(h => h.DangKy).ThenInclude(d => d.MaHVNavigation)
            .Include(h => h.MaUD).Include(h => h.DotHocPhi).ThenInclude(d => d.ChiTietPhieuThu)
            .AsSplitQuery().OrderBy(h => h.MaLop).ThenBy(h => h.MaHV);
        if (xuat == "excel")
        {
            var tatCa = await chiTiet.ToListAsync();
            // Một dòng cho một khoản học phí; chi tiết các đợt nằm trong các cột riêng,
            // tránh nhân học phí gốc và số phải nộp theo số đợt khi cộng Excel.
            var dong = tatCa.Select(h => new object?[] { h.MaHV, h.DangKy.MaHVNavigation.HoTen,
                h.MaLop, h.HocPhiGoc, UuDai(h), h.TyLeGiam, PhaiNop(h),
                string.Join("; ", h.DotHocPhi.OrderBy(d => d.Dot).Select(d => $"Đợt {d.Dot}: {HienThi.Tien(d.SoTien)}; hạn {HienThi.Ngay(d.HanDong)}; đã thu {HienThi.Tien(DaThu(d))}")),
                h.DotHocPhi.Sum(DaThu) }).ToList();
            dong.Add(["Tổng", null, null, TongGoc, null, null, TongPhaiNop, null, TongDaThu]);
            return XuatExcel.Tep($"HocPhiPhaiThu_{dongHo.HomNay:yyyyMMdd}",
                ["Mã HV", "Họ tên", "Lớp", "Học phí gốc", "Ưu đãi áp dụng", "Tổng giảm (%)", "Phải nộp", "Chi tiết các đợt", "Đã thu"], dong);
        }
        (DanhSach, Trang) = await chiTiet.LayTrangAsync(trang, so);
        return Page();
    }
}
