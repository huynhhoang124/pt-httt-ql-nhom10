using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.LopHoc;

/// <summary>F2.2 – Phiếu đăng ký học (module 2.2; đặc tả 04_dac_ta mục 1.2; QT05, QT06).</summary>
[Quyen("2.2", "T")]
public class DangKyModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record LopMo(Data.Entities.LopHoc Lop, int GiuCho, string Lich);

    [BindProperty(SupportsGet = true)] public string? Hv { get; set; }
    [BindProperty] public List<string> Lop { get; set; } = [];
    public HocVien? HocVien { get; set; }
    public List<LopMo> DanhSachLop { get; set; } = [];
    public List<string> DaDangKy { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Co("2.2", "T")) return Forbid();
        await NapAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        if (HocVien == null) { ModelState.AddModelError("", "Chọn học viên trước."); return Page(); }
        if (HocVien.TrangThai != "Hoạt động") { ModelState.AddModelError("", "Hồ sơ học viên đang ở trạng thái Ngừng. Gợi ý: mở lại hồ sơ ở F1.1."); return Page(); }
        if (HocVien.TrinhDoDauVao == null)  // phải kiểm tra trình độ ở 1.1 trước (04_dac_ta mục 1.2)
        {
            ModelState.AddModelError("", "Học viên chưa có trình độ đầu vào. Gợi ý: ghi kết quả kiểm tra đầu vào ở F1.1 rồi đăng ký.");
            return Page();
        }
        if (Lop.Count == 0) { ModelState.AddModelError("", "Chọn ít nhất một lớp."); return Page(); }

        var toiDa = await db.ThamSoAsync("SISO_TOI_DA");
        foreach (var ma in Lop.Distinct())  // kiểm tra từng lớp; có lỗi thì không ghi cả phiếu
        {
            var l = DanhSachLop.FirstOrDefault(x => x.Lop.MaLop == ma);
            if (l == null) { ModelState.AddModelError("", $"Lớp {ma} không nhận đăng ký (chỉ lớp Dự kiến, Đang học)."); continue; }
            if (DaDangKy.Contains(ma)) { ThongBaoLoi.Them(ModelState, new LoiNghiepVu("PK_DangKy", ma)); continue; }
            if (l.GiuCho >= Math.Min(l.Lop.SiSoToiDa, toiDa))
            {
                var goiY = DanhSachLop.Where(x => x.Lop.MaKH == l.Lop.MaKH && x.Lop.MaLop != ma && x.GiuCho < Math.Min(x.Lop.SiSoToiDa, toiDa))
                                      .Select(x => x.Lop.MaLop).ToList();
                ThongBaoLoi.Them(ModelState, new LoiNghiepVu("QT06", $"{ma}; lớp cùng khóa còn chỗ: {(goiY.Count > 0 ? string.Join(", ", goiY) : "không có")}"));
            }
        }
        if (!ModelState.IsValid) return Page();

        var homNay = dongHo.HomNay;
        var so = CsdlMoRong.SoTiepTheo(await db.PhieuDangKy.Select(p => p.SoPhieuDK).ToListAsync(), "DK", homNay.Year);
        db.PhieuDangKy.Add(new PhieuDangKy { SoPhieuDK = so, NgayDK = homNay, MaNV = User.MaNV()! });
        foreach (var ma in Lop.Distinct())
            db.DangKy.Add(new Data.Entities.DangKy { MaHV = HocVien.MaHV, MaLop = ma, SoPhieuDK = so, TrangThai = "Đã đăng ký" });
        db.GhiNhatKy(User, "Thêm", "PhieuDangKy", so, dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/LopHoc/KetQuaDangKy?phieu={so}");
    }

    async Task NapAsync()
    {
        if (!string.IsNullOrWhiteSpace(Hv))
        {
            HocVien = await db.HocVien.FindAsync(Hv.Trim().ToUpperInvariant());
            if (HocVien != null)
                DaDangKy = await db.DangKy.Where(d => d.MaHV == HocVien.MaHV).Select(d => d.MaLop).ToListAsync();
        }
        var lop = await db.LopHoc.AsNoTracking().Include(l => l.MaKHNavigation).Include(l => l.LichTuan)
                              .Where(l => l.TrangThai == "Dự kiến" || l.TrangThai == "Đang học").OrderBy(l => l.NgayKhaiGiang).ToListAsync();
        var siSo = await db.v_SiSoLop.ToDictionaryAsync(s => s.MaLop, s => s.SoDangKy ?? 0);
        DanhSachLop = lop.Select(l => new LopMo(l, siSo.GetValueOrDefault(l.MaLop), HienThi.Lich(l.LichTuan))).ToList();
    }
}
