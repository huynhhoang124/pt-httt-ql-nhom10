using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages;

/// <summary>FN1 – Đăng nhập (module N1).</summary>
public class DangNhapModel(TrungTamContext db, DongHo dongHo, KhoaDangNhap khoa) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Nhập tên đăng nhập."), Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu."), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        if (!ModelState.IsValid) return Page();
        var ten = TenDangNhap.Trim().ToLowerInvariant();
        if (khoa.DangKhoa(ten, dongHo.Bay))
        {
            ModelState.AddModelError("", $"Tài khoản tạm khóa do nhập sai {KhoaDangNhap.SoLanSai} lần. Thử lại sau {KhoaDangNhap.ThoiGianKhoa.TotalMinutes:0} phút hoặc liên hệ quản trị viên.");
            return Page();
        }
        var tk = await db.TaiKhoan.FindAsync(ten);
        if (tk == null || tk.TrangThai != "Hoạt động" || !Nen.MatKhau.Dung(MatKhau, tk.MatKhauBam))
        {
            khoa.Sai(ten, dongHo.Bay);
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");  // không nói rõ sai phần nào
            return Page();
        }
        khoa.Dung(ten);
        var claims = new List<Claim> { new(ClaimTypes.Name, tk.TenDangNhap), new(ClaimTypes.Role, PhanQuyen.TuVaiTroCsdl[tk.VaiTro]) };
        foreach (var (k, v) in new[] { ("MaNV", tk.MaNV), ("MaGV", tk.MaGV), ("MaHV", tk.MaHV), ("MaPH", tk.MaPH) })
            if (v != null) claims.Add(new Claim(k, v));
        var nguoi = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, nguoi);
        db.GhiNhatKy(nguoi, "Đăng nhập", "TaiKhoan", tk.TenDangNhap, dongHo.Bay);
        await db.SaveChangesAsync();
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }
}
