using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.LopHoc;

/// <summary>R2.2 – Kết quả đăng ký, danh sách lớp (module 2.2). Hiệu lực đăng ký tính từ v_HocPhiDangKy (QT05), không lưu.</summary>
[Quyen("2.2")]
public class KetQuaDangKyModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Dong(Data.Entities.DangKy Dk, v_HocPhiDangKy? Hp, string HieuLuc);

    public string? Phieu { get; set; }
    public string? Lop { get; set; }
    public PhieuDangKy? PhieuDK { get; set; }
    public v_SiSoLop? SiSo { get; set; }
    public List<string> CacLop { get; set; } = [];
    public List<Dong> DanhSach { get; set; } = [];
    public DateTime LapLuc { get; set; }

    public async Task OnGetAsync(string? phieu, string? lop)
    {
        Phieu = phieu; Lop = lop; LapLuc = dongHo.Bay;
        CacLop = await db.LopHoc.OrderByDescending(l => l.NgayKhaiGiang).Select(l => l.MaLop).ToListAsync();
        var ds = db.DangKy.AsNoTracking().Include(d => d.MaHVNavigation).Include(d => d.MaLopNavigation).ThenInclude(l => l.MaKHNavigation).AsQueryable();
        if (PhamVi.HocVien(db, User, "2.2") is { } duocXem) ds = ds.Where(d => duocXem.Contains(d.MaHV));
        if (phieu != null)
        {
            PhieuDK = await db.PhieuDangKy.Include(p => p.MaNVNavigation).FirstOrDefaultAsync(p => p.SoPhieuDK == phieu);
            ds = ds.Where(d => d.SoPhieuDK == phieu);
        }
        else if (lop != null)
        {
            SiSo = await db.v_SiSoLop.FirstOrDefaultAsync(s => s.MaLop == lop);
            ds = ds.Where(d => d.MaLop == lop);
        }
        else return;
        var dk = await ds.OrderBy(d => d.MaHVNavigation.HoTen).ToListAsync();
        var maHV = dk.Select(d => d.MaHV).Distinct().ToList();
        var hp = await db.v_HocPhiDangKy.Where(h => maHV.Contains(h.MaHV)).ToListAsync();
        DanhSach = dk.Select(d =>
        {
            var h = hp.FirstOrDefault(x => x.MaHV == d.MaHV && x.MaLop == d.MaLop);
            var hl = d.TrangThai != "Đã đăng ký" ? d.TrangThai
                   : h == null ? "Chờ lập học phí (F3.1)"
                   : h.HieuLuc == 1 ? "Đã xác nhận" : "Chờ đóng phí";
            return new Dong(d, h, hl);
        }).ToList();
    }
}
