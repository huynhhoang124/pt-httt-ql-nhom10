using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrungTamNgoaiNgu.Pages;

/// <summary>N1 – Đăng xuất.</summary>
public class DangXuatModel : PageModel
{
    public string TenDangNhap { get; set; } = string.Empty;

    public IActionResult OnGet() => Redirect("/");

    public async Task<IActionResult> OnPostAsync()
    {
        // Lấy tên đăng nhập trước khi tiến hành SignOut để truyền ra view xóa localStorage
        TenDangNhap = User.TenDangNhap();
        
        await HttpContext.SignOutAsync();
        
        // Trả về trang DangXuat.cshtml để thực hiện script xóa nháp phía client trước khi chuyển hướng
        return Page();
    }
}