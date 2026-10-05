using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.DanhMuc;

/// <summary>F1.1 – Tìm hồ sơ học viên (module 1.1). Quyền x chỉ thấy hồ sơ trong phạm vi của mình (N2).</summary>
[Quyen("1.1")]
public class HocVienModel(TrungTamContext db) : PageModel
{
    public string? Q { get; set; }
    public List<HocVien> DanhSach { get; set; } = [];

    public async Task OnGetAsync(string? q)
    {
        Q = q?.Trim();
        var ds = db.HocVien.AsNoTracking();
        if (PhamVi.HocVien(db, User, "1.1") is { } duocXem) ds = ds.Where(h => duocXem.Contains(h.MaHV));
        if (!string.IsNullOrEmpty(Q)) ds = ds.Where(h => h.HoTen.Contains(Q) || h.DienThoai.Contains(Q) || h.MaHV == Q);
        DanhSach = await ds.OrderBy(h => h.MaHV).Take(200).ToListAsync();
    }
}
