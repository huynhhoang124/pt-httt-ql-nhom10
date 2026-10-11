using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HocPhi;

/// <summary>R3.3 – Công nợ học phí (module 3.3; 04_dac_ta mục 1.5; QT10, QT16). Công nợ = phải nộp − đã thu của đăng ký còn
/// hiệu lực, tính bằng dbo.fn_CongNo tại ngày xem; lọc theo lớp, theo quá hạn. HV, PH chỉ thấy khoản của mình.</summary>
[Quyen("3.3")]
public class CongNoModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }
    [BindProperty(SupportsGet = true)] public bool QuaHan { get; set; }
    public List<CongNo> DanhSach { get; set; } = [];
    public List<string> CacLop { get; set; } = [];
    public DateOnly Ngay { get; set; }

    public async Task<IActionResult> OnGetAsync(string? xuat)
    {
        Ngay = dongHo.HomNay;
        var q = db.CongNoTai(Ngay);
        if (PhamVi.HocVien(db, User, "3.3") is { } hv) q = q.Where(c => hv.Contains(c.MaHV));
        CacLop = (await q.Select(c => c.MaLop).Distinct().ToListAsync()).Order().ToList();
        if (!string.IsNullOrEmpty(Lop)) q = q.Where(c => c.MaLop == Lop);
        if (QuaHan) q = q.Where(c => c.QuaHan == 1);
        DanhSach = await q.OrderByDescending(c => c.QuaHan).ThenBy(c => c.HanDong).ThenBy(c => c.MaHV).ToListAsync();
        if (xuat == "excel")
            return XuatExcel.Tep($"CongNo_{Ngay:yyyyMMdd}", ["Mã HV", "Họ tên", "Lớp", "Phải nộp", "Đã thu", "Còn nợ", "Hạn đóng", "Quá hạn"],
                DanhSach.Select(c => new object?[] { c.MaHV, c.HoTen, c.MaLop, c.PhaiNop, c.DaThu, c.ConNo, c.HanDong, c.QuaHan == 1 ? "Quá hạn" : "" }));
        return Page();
    }
}
