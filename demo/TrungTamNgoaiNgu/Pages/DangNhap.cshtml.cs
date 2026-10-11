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
public class DangNhapModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    /// <summary>Claim đánh dấu phiên phải đổi mật khẩu trước khi dùng hệ thống (bảo trì B2).</summary>
    public const string PhaiDoiMatKhau = "PhaiDoiMatKhau";

    [BindProperty, Required(ErrorMessage = "Nhập tên đăng nhập."), Display(Name = "Tên đăng nhập")]
    public string TenDangNhap { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu."), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        if (!ModelState.IsValid) return Page();
        var ten = TenDangNhap.Trim().ToLowerInvariant();
        var bay = dongHo.Bay;
        var tk = await db.TaiKhoan.FindAsync(ten);
        if (tk != null && KhoaDangNhap.DangKhoa(tk.SoLanSai, tk.KhoaDen, bay))
        {
            ModelState.AddModelError("", $"Tài khoản tạm khóa do nhập sai {KhoaDangNhap.SoLanSai} lần. Thử lại sau {KhoaDangNhap.ThoiGianKhoa.TotalMinutes:0} phút hoặc liên hệ quản trị viên.");
            return Page();
        }
        if (tk == null || tk.TrangThai != "Hoạt động" || !Nen.MatKhau.Dung(MatKhau, tk.MatKhauBam))
        {
            if (tk != null)
            {
                (tk.SoLanSai, tk.KhoaDen) = KhoaDangNhap.Sai(tk.SoLanSai, tk.KhoaDen, bay);  // B4: lưu CSDL, còn sau khi khởi động lại
                await db.SaveChangesAsync();
            }
            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");  // không nói rõ sai phần nào
            return Page();
        }
        (tk.SoLanSai, tk.KhoaDen) = (0, null);
        var claims = new List<Claim> { new(ClaimTypes.Name, tk.TenDangNhap), new(ClaimTypes.Role, PhanQuyen.TuVaiTroCsdl[tk.VaiTro]) };
        foreach (var (k, v) in new[] { ("MaNV", tk.MaNV), ("MaGV", tk.MaGV), ("MaHV", tk.MaHV), ("MaPH", tk.MaPH) })
            if (v != null) claims.Add(new Claim(k, v));
        if (tk.PhaiDoiMatKhau) claims.Add(new Claim(PhaiDoiMatKhau, "1"));
        var nguoi = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, nguoi);
        db.GhiNhatKy(nguoi, "Đăng nhập", "TaiKhoan", tk.TenDangNhap, bay);
        await db.SaveChangesAsync();
        if (tk.PhaiDoiMatKhau) return Redirect("/DoiMatKhau");   // B2: lần đăng nhập đầu
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }
}
