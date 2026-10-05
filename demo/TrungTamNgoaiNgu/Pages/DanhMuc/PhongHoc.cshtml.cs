using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.DanhMuc;

/// <summary>F1.4 – Phòng học (module 1.4; 04_dac_ta mục 1.1). Sức chứa > 0 do CHECK của CSDL chặn.</summary>
[Quyen("1.4", "S")]
public class PhongHocModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public static readonly string[] TinhTrang = ["Sẵn sàng", "Bảo trì", "Ngừng"];

    [BindProperty(SupportsGet = true)] public string? Ma { get; set; }
    [BindProperty] public Data.Entities.PhongHoc F { get; set; } = new() { TinhTrang = "Sẵn sàng" };
    public List<Data.Entities.PhongHoc> DanhSach { get; set; } = [];
    public string? ThongBao { get; set; }

    public async Task OnGetAsync(string? tb = null)
    {
        await NapAsync();
        if (Ma != null) F = DanhSach.FirstOrDefault(p => p.MaPhong == Ma) ?? F;
        ThongBao = tb;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        if (string.IsNullOrWhiteSpace(F.MaPhong) || string.IsNullOrWhiteSpace(F.TenPhong))
        {
            ModelState.AddModelError("", "Nhập mã và tên phòng.");
            return Page();
        }
        F.MaPhong = F.MaPhong.Trim().ToUpperInvariant();
        var x = await db.PhongHoc.FindAsync(Ma ?? F.MaPhong);
        if (Ma == null && x != null) { ModelState.AddModelError("", $"Phòng {F.MaPhong} đã có. Gợi ý: chọn phòng trong danh sách để sửa."); return Page(); }
        if (Ma != null && x == null) return NotFound();
        if (x == null)
        {
            if (!User.Co("1.4", "T")) return Forbid();
            x = db.PhongHoc.Add(new Data.Entities.PhongHoc { MaPhong = F.MaPhong }).Entity;
        }
        F.MaPhong = x.MaPhong;
        db.Entry(x).CurrentValues.SetValues(F);
        db.GhiNhatKy(User, Ma == null ? "Thêm" : x.TinhTrang == "Ngừng" ? "Ngừng" : "Sửa", "PhongHoc", x.MaPhong, dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/DanhMuc/PhongHoc?ma={x.MaPhong}&tb={Uri.EscapeDataString($"Đã lưu phòng {x.MaPhong}.")}");
    }

    async Task NapAsync() => DanhSach = await db.PhongHoc.AsNoTracking().OrderBy(p => p.MaPhong).ToListAsync();
}
