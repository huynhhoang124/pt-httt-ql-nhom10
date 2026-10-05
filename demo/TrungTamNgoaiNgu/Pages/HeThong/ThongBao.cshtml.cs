using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HeThong;

/// <summary>R5.2 – Thông báo (module 5.2). Học viên xem thông báo gửi mình, phụ huynh xem thông báo gửi mình; nhân viên xem tất cả.
/// Thông báo do lô tự động sinh (Nen/ThongBaoTuDong.cs), không ai nhập tay.</summary>
[Quyen("5.2")]
public class ThongBaoModel(TrungTamContext db) : PageModel
{
    public static readonly string[] Loai = ["Lịch học", "Học phí", "Chuyên cần", "Kết quả"];

    [BindProperty(SupportsGet = true)] public string? LoaiTB { get; set; }
    [BindProperty(SupportsGet = true)] public string? Hv { get; set; }
    public List<Data.Entities.ThongBao> DanhSach { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string? xuat)
    {
        var q = db.ThongBao.AsNoTracking().Include(t => t.MaHVNavigation).AsQueryable();
        if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "5.2"))
        {
            string? hv = User.MaHV(), ph = User.MaPH();
            q = User.VaiTro() == "PH" ? q.Where(t => t.MaPH == ph) : q.Where(t => t.MaHV == hv && t.MaPH == null);
        }
        if (!string.IsNullOrEmpty(LoaiTB)) q = q.Where(t => t.Loai == LoaiTB);
        if (!string.IsNullOrWhiteSpace(Hv)) q = q.Where(t => t.MaHV == Hv.Trim().ToUpper());
        DanhSach = await q.OrderByDescending(t => t.ThoiGianGui).ThenByDescending(t => t.MaTB).Take(300).ToListAsync();
        if (xuat == "excel")
            return XuatExcel.Tep("ThongBao", ["Thời gian", "Loại", "Mã HV", "Học viên", "Gửi phụ huynh", "Nội dung", "Trạng thái"],
                DanhSach.Select(t => new object?[] { t.ThoiGianGui.ToString("dd/MM/yyyy HH:mm"), t.Loai, t.MaHV, t.MaHVNavigation.HoTen, t.MaPH, t.NoiDung, t.TrangThaiGui }));
        return Page();
    }
}
