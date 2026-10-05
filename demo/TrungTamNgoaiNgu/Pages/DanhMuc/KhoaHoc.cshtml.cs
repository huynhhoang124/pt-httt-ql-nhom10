using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.DanhMuc;

/// <summary>F1.3 – Khóa học (module 1.3; 04_dac_ta mục 1.1). Tổng ba trọng số = 100% (QT13), học phí, số buổi > 0:
/// do CHECK của CSDL chặn, N7 đổi thành câu báo lỗi.</summary>
[Quyen("1.3", "S")]
public class KhoaHocModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public static readonly string[] TrinhDo = ["A1", "A2", "B1", "B2", "C1", "C2"];

    [BindProperty(SupportsGet = true)] public string? Ma { get; set; }
    [BindProperty] public Data.Entities.KhoaHoc F { get; set; } = new() { NgonNgu = "Anh", TrangThai = "Đang mở", TSChuyenCan = 10, TSGiuaKy = 30, TSCuoiKy = 60 };
    public List<Data.Entities.KhoaHoc> DanhSach { get; set; } = [];
    public string? ThongBao { get; set; }

    public async Task OnGetAsync(string? tb = null)
    {
        await NapAsync();
        if (Ma != null) F = DanhSach.FirstOrDefault(k => k.MaKH == Ma) ?? F;
        ThongBao = tb;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        if (string.IsNullOrWhiteSpace(F.MaKH) || string.IsNullOrWhiteSpace(F.TenKH))
        {
            ModelState.AddModelError("", "Nhập mã và tên khóa học.");
            return Page();
        }
        F.MaKH = F.MaKH.Trim().ToUpperInvariant();
        var x = await db.KhoaHoc.FindAsync(Ma ?? F.MaKH);
        if (Ma == null && x != null) { ModelState.AddModelError("", $"Mã khóa học {F.MaKH} đã có. Gợi ý: chọn khóa trong danh sách để sửa."); return Page(); }
        if (Ma != null && x == null) return NotFound();
        if (x == null)
        {
            if (!User.Co("1.3", "T")) return Forbid();
            x = db.KhoaHoc.Add(new Data.Entities.KhoaHoc { MaKH = F.MaKH }).Entity;
        }
        F.MaKH = x.MaKH;   // không đổi mã khi sửa (lớp, đăng ký đang tham chiếu)
        db.Entry(x).CurrentValues.SetValues(F);   // chỉ chép trường đơn, bỏ qua thuộc tính điều hướng gửi kèm
        db.GhiNhatKy(User, Ma == null ? "Thêm" : x.TrangThai == "Ngừng" ? "Ngừng" : "Sửa", "KhoaHoc", x.MaKH, dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/DanhMuc/KhoaHoc?ma={x.MaKH}&tb={Uri.EscapeDataString($"Đã lưu khóa {x.MaKH}.")}");
    }

    async Task NapAsync() => DanhSach = await db.KhoaHoc.AsNoTracking().OrderBy(k => k.MaKH).ToListAsync();
}
