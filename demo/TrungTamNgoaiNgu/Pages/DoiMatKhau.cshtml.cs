using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages;

/// <summary>FN1 – Đổi mật khẩu (module N1).</summary>
[Quyen("N1")]
public class DoiMatKhauModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu hiện tại."), DataType(DataType.Password), Display(Name = "Mật khẩu hiện tại")]
    public string Cu { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Nhập mật khẩu mới."), DataType(DataType.Password), Display(Name = "Mật khẩu mới")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới cần ít nhất 8 ký tự.")]
    public string Moi { get; set; } = "";

    [BindProperty, DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu mới")]
    [Compare(nameof(Moi), ErrorMessage = "Hai lần nhập mật khẩu mới không giống nhau.")]
    public string NhapLai { get; set; } = "";

    public bool DaDoi { get; set; }
    public bool BatBuoc => User.HasClaim(c => c.Type == DangNhapModel.PhaiDoiMatKhau);

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var tk = await db.TaiKhoan.FindAsync(User.TenDangNhap());
        if (tk == null || !MatKhau.Dung(Cu, tk.MatKhauBam))
        {
            ModelState.AddModelError(nameof(Cu), "Mật khẩu hiện tại không đúng.");
            return Page();
        }
        if (Moi == Cu)
        {
            ModelState.AddModelError(nameof(Moi), "Mật khẩu mới phải khác mật khẩu hiện tại.");
            return Page();
        }
        tk.MatKhauBam = MatKhau.Bam(Moi);
        tk.PhaiDoiMatKhau = false;
        db.GhiNhatKy(User, "Sửa", "TaiKhoan", tk.TenDangNhap, dongHo.Bay);
        await db.SaveChangesAsync();
        if (BatBuoc)  // B2: cấp lại phiên không còn dấu "phải đổi mật khẩu"
        {
            var id = new ClaimsIdentity(User.Claims.Where(c => c.Type != DangNhapModel.PhaiDoiMatKhau), CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            return Redirect("/");
        }
        DaDoi = true;
        return Page();
    }
}
