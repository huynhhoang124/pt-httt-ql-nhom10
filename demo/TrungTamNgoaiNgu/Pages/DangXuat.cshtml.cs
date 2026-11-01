using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrungTamNgoaiNgu.Pages;

/// <summary>N1 – Đăng xuất.</summary>
public class DangXuatModel : PageModel
{
    public IActionResult OnGet() => Redirect("/");

    public async Task<IActionResult> OnPostAsync()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/DangNhap");
    }
}
