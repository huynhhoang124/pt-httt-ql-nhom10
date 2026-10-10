using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HeThong;

/// <summary>R5.1 – Nhật ký hệ thống (module 5.1; bảng NhatKy do N3 ghi). Lọc theo người dùng, thao tác, khoảng ngày.</summary>
[Quyen("5.1")]
public class NhatKyModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public static readonly string[] ThaoTac = ["Đăng nhập", "Thêm", "Sửa", "Ngừng", "Duyệt", "In"];

    [BindProperty(SupportsGet = true)] public string? Ten { get; set; }
    [BindProperty(SupportsGet = true)] public string? Loai { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? TuNgay { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? DenNgay { get; set; }
    public List<Data.Entities.NhatKy> DanhSach { get; set; } = [];
    public TrangDs Trang { get; set; } = new(1, 50, 0);

    public async Task<IActionResult> OnGetAsync(string? xuat, int? trang, int? so)
    {
        DenNgay ??= dongHo.HomNay;
        TuNgay ??= DenNgay.Value.AddDays(-30);
        DateTime tu = TuNgay.Value.ToDateTime(TimeOnly.MinValue), den = DenNgay.Value.ToDateTime(TimeOnly.MaxValue);
        var q = db.NhatKy.AsNoTracking().Where(n => n.ThoiDiem >= tu && n.ThoiDiem <= den);
        if (!string.IsNullOrWhiteSpace(Ten)) q = q.Where(n => n.TenDangNhap == Ten.Trim().ToLower());
        if (!string.IsNullOrEmpty(Loai)) q = q.Where(n => n.ThaoTac == Loai);
        var sx = q.OrderByDescending(n => n.MaNK);
        if (xuat == "excel")   // Excel lấy toàn bộ kết quả lọc, không theo trang
        {
            DanhSach = await sx.ToListAsync();
            return XuatExcel.Tep($"NhatKy_{TuNgay:yyyyMMdd}_{DenNgay:yyyyMMdd}", ["Thời điểm", "Người dùng", "Thao tác", "Đối tượng", "Mã đối tượng"],
                DanhSach.Select(n => new object?[] { n.ThoiDiem.ToString("dd/MM/yyyy HH:mm"), n.TenDangNhap, n.ThaoTac, n.DoiTuong, n.MaDoiTuong }));
        }
        (DanhSach, Trang) = await sx.LayTrangAsync(trang, so);
        return Page();
    }
}
