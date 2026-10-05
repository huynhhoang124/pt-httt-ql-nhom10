using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.BaoCao;

/// <summary>F5.3 chọn kỳ + R5.3b Báo cáo tình trạng lớp (module 5.3; v_SiSoLop): sĩ số, tỷ lệ lấp đầy, số lớp mở và hủy;
/// lớp dự kiến chưa đủ sĩ số tối thiểu để mở (QT06) được đánh dấu.</summary>
[Quyen("5.3")]
public class TinhTrangLopModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Dong(string MaLop, string Khoa, string GiaoVien, DateOnly KhaiGiang, string TrangThai, int SoDangKy, int SiSoToiDa, decimal LapDay, bool DuoiToiThieu);

    public DateOnly TuNgay { get; set; }
    public DateOnly DenNgay { get; set; }
    public List<Dong> DanhSach { get; set; } = [];
    public decimal ToiThieu { get; set; }
    public DateTime LapLuc { get; set; }

    public async Task OnGetAsync(DateOnly? tuNgay, DateOnly? denNgay)
    {
        LapLuc = dongHo.Bay;
        DenNgay = denNgay ?? dongHo.HomNay.AddMonths(3);
        TuNgay = tuNgay ?? new DateOnly(dongHo.HomNay.Year - 1, 1, 1);
        ToiThieu = await db.ThamSoAsync("SISO_TOI_THIEU");
        var ds = await (from s in db.v_SiSoLop
                        join l in db.LopHoc on s.MaLop equals l.MaLop
                        where s.NgayKhaiGiang >= TuNgay && s.NgayKhaiGiang <= DenNgay
                        orderby s.NgayKhaiGiang
                        select new { s, Khoa = l.MaKHNavigation.TenKH, Gv = l.MaGVNavigation.HoTen }).ToListAsync();
        DanhSach = ds.Select(x => new Dong(x.s.MaLop, x.Khoa, x.Gv, x.s.NgayKhaiGiang, x.s.TrangThai, x.s.SoDangKy ?? 0, x.s.SiSoToiDa,
                                           x.s.TyLeLapDay ?? 0, x.s.TrangThai == "Dự kiến" && (x.s.SoDangKy ?? 0) < ToiThieu)).ToList();
    }
}
