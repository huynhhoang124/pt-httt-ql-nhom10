using System.ComponentModel.DataAnnotations;
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
        tk.MatKhauBam = MatKhau.Bam(Moi);
        db.GhiNhatKy(User, "Sửa", "TaiKhoan", tk.TenDangNhap, dongHo.Bay);
        await db.SaveChangesAsync();
        DaDoi = true;
        return Page();
    }
}
