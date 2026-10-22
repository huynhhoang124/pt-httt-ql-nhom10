using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.BaoCao;

[Quyen("5.3")]
public class TuyenSinhModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record TuyenSinhRow(int Nam, int Thang, Dictionary<string, int> SoLuongTheoKhoa, int TongThang);

    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public List<KhoaHoc> DanhSachKhoaHoc { get; set; } = [];
    public List<TuyenSinhRow> DuLieuKyNay { get; set; } = [];
    public List<TuyenSinhRow> DuLieuKyTruoc { get; set; } = [];
    public int TongTuyenSinhKyNay { get; set; }
    public int TongTuyenSinhKyTruoc { get; set; }
    public DateTime LapLuc { get; set; }

    public async Task<IActionResult> OnGetAsync(DateOnly? tuNgay, DateOnly? denNgay, string? xuat)
    {
        LapLuc = dongHo.Bay;
        DenNgay = denNgay ?? dongHo.HomNay;
        TuNgay = tuNgay ?? DenNgay.AddYears(-1);
        if (TuNgay > DenNgay) (TuNgay, DenNgay) = (DenNgay, TuNgay);

        ViewData["Ma"] = "F5.3, R5.3a";

        DanhSachKhoaHoc = await db.KhoaHoc.AsNoTracking().OrderBy(k => k.MaKH).ToListAsync();

        DuLieuKyNay = await LayDuLieuTuyenSinhAsync(TuNgay, DenNgay);
        TongTuyenSinhKyNay = DuLieuKyNay.Sum(r => r.TongThang);

        var soNgay = DenNgay.DayNumber - TuNgay.DayNumber + 1;
        DuLieuKyTruoc = await LayDuLieuTuyenSinhAsync(TuNgay.AddDays(-soNgay), TuNgay.AddDays(-1));
        TongTuyenSinhKyTruoc = DuLieuKyTruoc.Sum(r => r.TongThang);

        if (xuat == "excel")
        {
            var headers = new List<string> { "Năm", "Tháng" };
            headers.AddRange(DanhSachKhoaHoc.Select(k => k.TenKH));
            headers.Add("Tổng");

            var rows = DuLieuKyNay.Select(r =>
            {
                var rowData = new List<object?> { r.Nam, r.Thang };
                foreach (var k in DanhSachKhoaHoc)
                {
                    rowData.Add(r.SoLuongTheoKhoa.GetValueOrDefault(k.MaKH, 0));
                }
                rowData.Add(r.TongThang);
                return rowData.ToArray();
            });

            return XuatExcel.Tep($"BaoCaoTuyenSinh_{TuNgay:yyyyMMdd}_{DenNgay:yyyyMMdd}", headers.ToArray(), rows);
        }

        return Page();
    }

    private async Task<List<TuyenSinhRow>> LayDuLieuTuyenSinhAsync(DateOnly tu, DateOnly den)
    {
        var ds = await db.v_TuyenSinh
            .AsNoTracking()
            .Where(x => x.NgayDangKyDau >= tu && x.NgayDangKyDau <= den)
            .ToListAsync();

        var nhom = ds.GroupBy(x => new { x.Nam, x.Thang })
                     .OrderBy(g => g.Key.Nam).ThenBy(g => g.Key.Thang)
                     .ToList();

        List<TuyenSinhRow> ketQua = [];
        foreach (var g in nhom)
        {
            var dict = new Dictionary<string, int>();
            foreach (var kh in DanhSachKhoaHoc)
            {
                dict[kh.MaKH] = g.Count(x => x.MaKH == kh.MaKH);
            }
            ketQua.Add(new TuyenSinhRow(g.Key.Nam ?? tu.Year, g.Key.Thang ?? tu.Month, dict, g.Count()));
        }
        return ketQua;
    }
}