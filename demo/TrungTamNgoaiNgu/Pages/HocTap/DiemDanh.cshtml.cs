using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.HocTap;

/// <summary>F4.1 – Điểm danh buổi học (module 4.1; đặc tả 04_dac_ta mục 1.6; QT11, QT12).
/// Giáo viên điểm danh buổi mình dạy; người có quyền X chỉ xem.</summary>
[Quyen("4.1", "T")]
public class DiemDanhModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Dong(HocVien Hv, string? TrangThai, string? GhiChu, v_ChuyenCan? ChuyenCan);

    [BindProperty(SupportsGet = true)] public string? Buoi { get; set; }   // "MaLop|SoBuoi"
    [BindProperty] public Dictionary<string, string> TrangThai { get; set; } = [];
    [BindProperty] public Dictionary<string, string?> GhiChu { get; set; } = [];
    public List<BuoiHoc> CacBuoi { get; set; } = [];
    public BuoiHoc? Chon { get; set; }
    public List<Dong> DanhSach { get; set; } = [];
    public bool DuocGhi { get; set; }
    public string? DaLuu { get; set; }

    public async Task<IActionResult> OnGetAsync(bool daLuu = false)
    {
        if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "4.1") && User.VaiTro() != "GV") return Forbid();  // HV, PH xem chuyên cần ở R4.1
        await NapAsync();
        if (daLuu && Chon != null) DaLuu = "Đã lưu điểm danh.";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        if (Chon == null) return NotFound();
        if (!DuocGhi)
        {
            ThongBaoLoi.Them(ModelState, new LoiNghiepVu("QT11"));
            return Page();
        }
        var thieu = DanhSach.Where(d => !QuyTac.TrangThaiDiemDanh.Contains(TrangThai.GetValueOrDefault(d.Hv.MaHV))).Select(d => d.Hv.MaHV).ToList();
        if (thieu.Count > 0)
        {
            ModelState.AddModelError("", $"Chưa chọn trạng thái cho học viên {string.Join(", ", thieu)}. Gợi ý: chọn x, M, P hoặc K cho mọi học viên.");
            return Page();
        }
        var cu = await db.DiemDanh.Where(x => x.MaLop == Chon.MaLop && x.SoBuoi == Chon.SoBuoi).ToDictionaryAsync(x => x.MaHV);
        foreach (var d in DanhSach)
        {
            if (!cu.TryGetValue(d.Hv.MaHV, out var dd))
                db.DiemDanh.Add(dd = new Data.Entities.DiemDanh { MaHV = d.Hv.MaHV, MaLop = Chon.MaLop, SoBuoi = Chon.SoBuoi });
            dd.TrangThai = TrangThai[d.Hv.MaHV];
            dd.GhiChu = string.IsNullOrWhiteSpace(GhiChu.GetValueOrDefault(d.Hv.MaHV)) ? null : GhiChu[d.Hv.MaHV]!.Trim();
        }
        db.GhiNhatKy(User, cu.Count == 0 ? "Thêm" : "Sửa", "DiemDanh", $"{Chon.MaLop}/{Chon.SoBuoi}", dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/HocTap/DiemDanh?buoi={Uri.EscapeDataString(Buoi!)}&daLuu=true");
    }

    async Task NapAsync()
    {
        var homNay = dongHo.HomNay;
        var q = db.BuoiHoc.AsNoTracking().Include(b => b.MaLopNavigation.MaKHNavigation)
                  .Where(b => b.TrangThai != "Hủy" && b.Ngay <= homNay);
        var gv = User.MaGV();
        if (User.VaiTro() == "GV") q = q.Where(b => b.MaGV == gv);  // phạm vi x: buổi mình dạy (kể cả dạy thay)
        CacBuoi = await q.OrderByDescending(b => b.Ngay).ThenByDescending(b => b.GioBatDau).Take(30).ToListAsync();
        Buoi ??= CacBuoi.Select(b => $"{b.MaLop}|{b.SoBuoi}").FirstOrDefault();
        Chon = CacBuoi.FirstOrDefault(b => $"{b.MaLop}|{b.SoBuoi}" == Buoi);
        if (Chon == null) return;

        DuocGhi = User.Co("4.1", "T") && Chon.MaGV == gv
                  && QuyTac.DuocDiemDanh(Chon.Ngay, Chon.GioBatDau, Chon.GioKetThuc, dongHo.Bay, await db.ThamSoAsync("GIO_SUA_DIEM_DANH"));
        // Chỉ học viên có đăng ký "Đã đăng ký" (04_dac_ta mục 1.6); học viên đã có điểm danh ở buổi này vẫn hiện để xem lại
        var dk = await db.DangKy.AsNoTracking().Include(d => d.MaHVNavigation)
                         .Where(d => d.MaLop == Chon.MaLop && (d.TrangThai == "Đã đăng ký" || d.DiemDanh.Any(x => x.SoBuoi == Chon.SoBuoi)))
                         .OrderBy(d => d.MaHV).ToListAsync();
        var dd = await db.DiemDanh.Where(x => x.MaLop == Chon.MaLop && x.SoBuoi == Chon.SoBuoi).ToDictionaryAsync(x => x.MaHV);
        var cc = await db.v_ChuyenCan.Where(x => x.MaLop == Chon.MaLop).ToDictionaryAsync(x => x.MaHV);
        DanhSach = dk.Select(d => new Dong(d.MaHVNavigation, dd.GetValueOrDefault(d.MaHV)?.TrangThai, dd.GetValueOrDefault(d.MaHV)?.GhiChu,
                                           cc.GetValueOrDefault(d.MaHV))).ToList();
    }
}
