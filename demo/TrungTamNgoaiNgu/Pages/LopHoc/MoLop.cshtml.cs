using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.LopHoc;

/// <summary>F2.1 – Mở lớp, phân công giáo viên (module 2.1; 04_dac_ta mục 1.1; QT06). Lớp "Dự kiến" sửa được cả lịch tuần;
/// đến ngày khai giảng thì chốt: đủ số đăng ký có hiệu lực → Đang học (sang 2.3 xếp lịch), không thì Hủy.</summary>
[Quyen("2.1", "S")]
public class MoLopModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public class Lich { public byte? Thu { get; set; } public TimeOnly? Gio { get; set; } public string? Phong { get; set; } }

    [BindProperty(SupportsGet = true)] public string? Ma { get; set; }
    [BindProperty] public string? MaLop { get; set; }
    [BindProperty] public string? MaKH { get; set; }
    [BindProperty] public string? MaGV { get; set; }
    [BindProperty] public DateOnly? NgayKhaiGiang { get; set; }
    [BindProperty] public short SiSoToiDa { get; set; } = 20;
    [BindProperty] public List<Lich> LichTuan { get; set; } = [];
    public List<v_SiSoLop> DanhSach { get; set; } = [];
    public List<KhoaHoc> CacKhoa { get; set; } = [];
    public List<GiaoVien> CacGV { get; set; } = [];
    public List<PhongHoc> CacPhong { get; set; } = [];
    public Data.Entities.LopHoc? Chon { get; set; }
    public string? ThongBao { get; set; }
    public bool SuaDuoc => Chon == null || Chon.TrangThai == "Dự kiến";

    public async Task<IActionResult> OnGetAsync(string? tb = null)
    {
        if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "2.1")) return Forbid();   // GV xem lớp mình ở R2.3b
        await NapAsync();
        ThongBao = tb;
        if (Chon != null)
        {
            (MaLop, MaKH, MaGV, NgayKhaiGiang, SiSoToiDa) = (Chon.MaLop, Chon.MaKH, Chon.MaGV, Chon.NgayKhaiGiang, Chon.SiSoToiDa);
            LichTuan = Chon.LichTuan.OrderBy(l => l.Thu).Select(l => new Lich { Thu = l.Thu, Gio = l.GioBatDau, Phong = l.MaPhong }).ToList();
        }
        while (LichTuan.Count < 3) LichTuan.Add(new Lich());
        return Page();
    }

    public async Task<IActionResult> OnPostLuuAsync()
    {
        await NapAsync();
        while (LichTuan.Count < 3) LichTuan.Add(new Lich());
        if (!SuaDuoc) { ModelState.AddModelError("", $"Lớp đang ở trạng thái {Chon!.TrangThai}, không sửa được. Gợi ý: đổi lịch từng buổi ở F2.3."); return Page(); }
        var lich = LichTuan.Where(l => l.Thu != null || l.Gio != null || !string.IsNullOrEmpty(l.Phong)).ToList();
        if (string.IsNullOrWhiteSpace(MaLop) || MaKH == null || MaGV == null || NgayKhaiGiang == null || lich.Count == 0
            || lich.Any(l => l.Thu == null || l.Gio == null || string.IsNullOrEmpty(l.Phong)))
        {
            ModelState.AddModelError("", "Nhập đủ mã lớp, khóa học, giáo viên, ngày khai giảng và ít nhất một dòng lịch tuần (thứ, giờ, phòng).");
            return Page();
        }
        if (lich.Select(l => l.Thu).Distinct().Count() < lich.Count) { ModelState.AddModelError("", "Mỗi thứ chỉ có một dòng lịch tuần."); return Page(); }
        var kh = CacKhoa.FirstOrDefault(k => k.MaKH == MaKH);
        var gv = CacGV.FirstOrDefault(g => g.MaGV == MaGV);
        if (kh == null || gv == null || lich.Any(l => CacPhong.All(p => p.MaPhong != l.Phong)))
        {
            ModelState.AddModelError("", "Khóa học, giáo viên hoặc phòng không còn sử dụng. Gợi ý: chọn lại trong danh sách.");
            return Page();
        }
        if (gv.NgonNguDay != kh.NgonNgu)
        {
            ModelState.AddModelError("", $"Giáo viên {gv.MaGV} dạy tiếng {gv.NgonNguDay}, khóa {kh.MaKH} là tiếng {kh.NgonNgu}. Gợi ý: chọn giáo viên dạy đúng ngôn ngữ.");
            return Page();
        }
        try
        {
            QuyTac.KiemTraSiSo(SiSoToiDa, await db.ThamSoAsync("SISO_TOI_DA"), lich.Min(l => CacPhong.First(p => p.MaPhong == l.Phong).SucChua));
        }
        catch (LoiNghiepVu ex)
        {
            ThongBaoLoi.Them(ModelState, ex);
            return Page();
        }
        var ma = Chon?.MaLop ?? MaLop.Trim().ToUpperInvariant();
        if (Chon == null && DanhSach.Any(l => l.MaLop == ma)) { ModelState.AddModelError("", $"Mã lớp {ma} đã có."); return Page(); }
        var lop = Chon == null ? db.LopHoc.Add(new Data.Entities.LopHoc { MaLop = ma, TrangThai = "Dự kiến" }).Entity : Chon;
        (lop.MaKH, lop.MaGV, lop.NgayKhaiGiang, lop.SiSoToiDa) = (kh.MaKH, gv.MaGV, NgayKhaiGiang.Value, SiSoToiDa);
        // lịch tuần: khóa (MaLop, Thu) → xóa thứ bỏ đi, sửa thứ còn giữ, thêm thứ mới
        var cu = await db.LichTuan.Where(l => l.MaLop == ma).ToDictionaryAsync(l => l.Thu);
        db.LichTuan.RemoveRange(cu.Values.Where(c => lich.All(l => l.Thu != c.Thu)));
        foreach (var l in lich)
        {
            var x = cu.GetValueOrDefault(l.Thu!.Value) ?? db.LichTuan.Add(new Data.Entities.LichTuan { MaLop = ma, Thu = l.Thu.Value }).Entity;
            (x.GioBatDau, x.MaPhong) = (l.Gio!.Value, l.Phong!);
        }
        db.GhiNhatKy(User, Chon == null ? "Thêm" : "Sửa", "LopHoc", ma, dongHo.Bay);
        return await LuuAsync(ma, $"Đã lưu lớp {ma}.");
    }

    /// <summary>Chốt lớp tại ngày khai giảng (QT06).</summary>
    public async Task<IActionResult> OnPostChotAsync()
    {
        await NapAsync();
        while (LichTuan.Count < 3) LichTuan.Add(new Lich());
        if (Chon == null) return NotFound();
        if (Chon.TrangThai != "Dự kiến" || Chon.NgayKhaiGiang > dongHo.HomNay)
        {
            ModelState.AddModelError("", $"Chỉ chốt lớp Dự kiến đã đến ngày khai giảng ({HienThi.Ngay(Chon.NgayKhaiGiang)}).");
            return Page();
        }
        var hieuLuc = await db.v_HocPhiDangKy.CountAsync(h => h.MaLop == Chon.MaLop && h.HieuLuc == 1);
        Chon.TrangThai = QuyTac.ChotLop(hieuLuc, await db.ThamSoAsync("SISO_TOI_THIEU"));
        db.GhiNhatKy(User, Chon.TrangThai == "Hủy" ? "Ngừng" : "Sửa", "LopHoc", $"{Chon.MaLop}: {Chon.TrangThai}", dongHo.Bay);
        return await LuuAsync(Chon.MaLop, Chon.TrangThai == "Hủy"
            ? $"Lớp {Chon.MaLop} chỉ có {hieuLuc} đăng ký có hiệu lực: HỦY. Gợi ý: chuyển học viên sang lớp cùng khóa ở F2.4."
            : $"Lớp {Chon.MaLop} có {hieuLuc} đăng ký có hiệu lực: mở lớp (Đang học). Tiếp theo: sinh lịch ở F2.3.");
    }

    async Task<IActionResult> LuuAsync(string ma, string tb)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/LopHoc/MoLop?ma={ma}&tb={Uri.EscapeDataString(tb)}");
    }

    async Task NapAsync()
    {
        DanhSach = await db.v_SiSoLop.AsNoTracking().OrderByDescending(l => l.NgayKhaiGiang).ToListAsync();
        CacKhoa = await db.KhoaHoc.AsNoTracking().Where(k => k.TrangThai == "Đang mở").OrderBy(k => k.MaKH).ToListAsync();
        CacGV = await db.GiaoVien.AsNoTracking().Where(g => g.TinhTrang == "Đang dạy").OrderBy(g => g.MaGV).ToListAsync();
        CacPhong = await db.PhongHoc.AsNoTracking().Where(p => p.TinhTrang == "Sẵn sàng").OrderBy(p => p.MaPhong).ToListAsync();
        if (Ma != null) Chon = await db.LopHoc.Include(l => l.LichTuan).FirstOrDefaultAsync(l => l.MaLop == Ma);   // theo dõi để sửa
    }
}
