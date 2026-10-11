using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HeThong;

/// <summary>F5.1b – Tham số hệ thống (module 5.1; bảng ThamSo). Con số chính sách của QT04–QT15 đổi ở đây, không sửa mã.</summary>
[Quyen("5.1", "S")]
public class ThamSoModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    [BindProperty] public Dictionary<string, decimal?> GiaTri { get; set; } = [];
    public List<Data.Entities.ThamSo> DanhSach { get; set; } = [];
    public string? ThongBao { get; set; }

    public async Task OnGetAsync() => DanhSach = await db.ThamSo.OrderBy(t => t.QuyTac ?? "zz").ThenBy(t => t.MaThamSo).ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        DanhSach = await db.ThamSo.OrderBy(t => t.QuyTac ?? "zz").ThenBy(t => t.MaThamSo).ToListAsync();
        var doi = new List<string>();
        foreach (var t in DanhSach)
        {
            if (GiaTri.GetValueOrDefault(t.MaThamSo) is not { } v || v == t.GiaTri) continue;
            if (v <= 0 || (t.DonVi == "%" && v > 100) || (t.DonVi == "điểm" && v > 10))
            {
                ModelState.AddModelError("", $"{t.TenThamSo}: giá trị {v} không hợp lệ. Gợi ý: lớn hơn 0; tỷ lệ không quá 100%, điểm không quá 10.");
                continue;
            }
            doi.Add($"{t.MaThamSo} {t.GiaTri:0.##}→{v:0.##}");
            t.GiaTri = v;
        }
        if (!ModelState.IsValid) return Page();
        foreach (var d in doi) db.GhiNhatKy(User, "Sửa", "ThamSo", d, dongHo.Bay);
        await db.SaveChangesAsync();
        ThongBao = doi.Count == 0 ? "Không có giá trị nào thay đổi." : $"Đã lưu: {string.Join("; ", doi)}.";
        return Page();
    }
}
