using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.DanhMuc;

/// <summary>F1.2 – Hồ sơ giáo viên (module 1.2; khuôn cập nhật danh mục 04_dac_ta mục 1.1). Trùng điện thoại hoặc email
/// do ràng buộc UNIQUE chặn; ngừng = đổi Tình trạng, không xóa.</summary>
[Quyen("1.2", "S")]
public class GiaoVienModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public static readonly string[] NgonNgu = ["Anh", "Trung", "Nhật", "Hàn"];
    public static readonly string[] TinhTrang = ["Đang dạy", "Tạm nghỉ", "Nghỉ việc"];

    [BindProperty(SupportsGet = true)] public string? Ma { get; set; }
    [BindProperty] public Data.Entities.GiaoVien F { get; set; } = new() { NgonNguDay = "Anh", TinhTrang = "Đang dạy" };
    public List<Data.Entities.GiaoVien> DanhSach { get; set; } = [];
    public string? ThongBao { get; set; }

    public async Task<IActionResult> OnGetAsync(string? tb = null)
    {
        if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "1.2")) return Forbid();   // GV xem hồ sơ của mình ở trang thông tin cá nhân
        await NapAsync();
        if (Ma != null) F = DanhSach.FirstOrDefault(g => g.MaGV == Ma) ?? F;
        ThongBao = tb;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        if (string.IsNullOrWhiteSpace(F.HoTen) || string.IsNullOrWhiteSpace(F.DienThoai) || string.IsNullOrWhiteSpace(F.Email) || F.NgaySinh == default)
        {
            ModelState.AddModelError("", "Nhập đủ họ tên, ngày sinh, điện thoại, email.");
            return Page();
        }
        var x = Ma == null ? null : await db.GiaoVien.FindAsync(Ma);
        if (Ma != null && x == null) return NotFound();
        if (x == null)
        {
            if (!User.Co("1.2", "T")) return Forbid();
            x = db.GiaoVien.Add(new Data.Entities.GiaoVien { MaGV = CsdlMoRong.MaMoi(DanhSach.Select(g => g.MaGV), "GV", 3) }).Entity;
        }
        F.MaGV = x.MaGV;
        db.Entry(x).CurrentValues.SetValues(F);   // chỉ chép trường đơn, bỏ qua mọi thuộc tính điều hướng gửi kèm
        (x.HoTen, x.DienThoai, x.Email) = (F.HoTen.Trim(), F.DienThoai.Trim(), F.Email.Trim().ToLowerInvariant());
        db.GhiNhatKy(User, Ma == null ? "Thêm" : x.TinhTrang == "Nghỉ việc" ? "Ngừng" : "Sửa", "GiaoVien", x.MaGV, dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/DanhMuc/GiaoVien?ma={x.MaGV}&tb={Uri.EscapeDataString($"Đã lưu {x.MaGV}.")}");
    }

    async Task NapAsync() => DanhSach = await db.GiaoVien.AsNoTracking().OrderBy(g => g.MaGV).ToListAsync();
}
