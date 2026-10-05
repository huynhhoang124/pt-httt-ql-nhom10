using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.HocPhi;

/// <summary>F3.1 – Xác nhận khoản học phí của đăng ký mới (module 3.1; bảng quyết định 04_dac_ta mục 3.1, QT01–QT05).
/// Kế toán chọn đóng 1 hay 2 đợt (C3) và đăng ký nhóm (C2); học viên cũ (C1) do hệ thống tự xét.</summary>
[Quyen("3.1", "T")]
public class KhoanPhaiThuModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record ChoLap(Data.Entities.DangKy Dk, DateOnly NgayDK, bool HocVienCu);

    public List<ChoLap> DanhSach { get; set; } = [];
    public Data.Entities.HocPhi? VuaLap { get; set; }

    public async Task<IActionResult> OnGetAsync(string? hv, string? lop)
    {
        if (!User.Co("3.1", "T")) return Forbid();
        await NapAsync();
        if (hv != null && lop != null)
            VuaLap = await db.HocPhi.Include(h => h.DotHocPhi).Include(h => h.MaUD).FirstOrDefaultAsync(h => h.MaHV == hv && h.MaLop == lop);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string hv, string lop, byte soDot, bool nhom)
    {
        await NapAsync();
        var c = DanhSach.FirstOrDefault(x => x.Dk.MaHV == hv && x.Dk.MaLop == lop);
        if (c == null || soDot is not (1 or 2))
        {
            ModelState.AddModelError("", "Đăng ký không còn ở danh sách chờ lập học phí, hoặc số đợt không hợp lệ.");
            return Page();
        }
        var l = c.Dk.MaLopNavigation;
        var uuDai = await db.UuDai.AsNoTracking()
            .Select(u => new QuyTac.UuDaiCo(u.MaUD, u.LoaiUD, u.TyLeGiam, u.NgayBatDau, u.NgayKetThuc)).ToListAsync();
        var (tyLe, maUD) = QuyTac.XetUuDai(c.HocVienCu, nhom, soDot == 1, uuDai, c.NgayDK, await db.ThamSoAsync("TY_LE_GIAM_TOI_DA"));
        var giuaKhoa = (l.MaKHNavigation.SoBuoi + 1) / 2;
        var hanDot2 = l.BuoiHoc.FirstOrDefault(b => b.SoBuoi == giuaKhoa)?.Ngay
                      ?? QuyTac.NgayBuoiThu(giuaKhoa, l.NgayKhaiGiang, l.LichTuan.Select(x => x.Thu).ToList());
        var hp = new Data.Entities.HocPhi
        {
            MaHV = hv, MaLop = lop, HocPhiGoc = l.MaKHNavigation.HocPhi, TyLeGiam = tyLe, SoDot = soDot, NgayLap = dongHo.HomNay,
        };
        foreach (var (dot, tien, han) in QuyTac.ChiaDot(hp.HocPhiGoc, tyLe, soDot, await db.ThamSoAsync("TY_LE_DOT_1"), l.NgayKhaiGiang, hanDot2))
            hp.DotHocPhi.Add(new DotHocPhi { MaHV = hv, MaLop = lop, Dot = dot, SoTien = tien, HanDong = han });
        foreach (var u in await db.UuDai.Where(u => maUD.Contains(u.MaUD)).ToListAsync())
            hp.MaUD.Add(u);
        db.HocPhi.Add(hp);
        db.GhiNhatKy(User, "Thêm", "HocPhi", $"{hv}/{lop}", dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/HocPhi/KhoanPhaiThu?hv={hv}&lop={lop}");
    }

    async Task NapAsync()
    {
        // Đăng ký mới, không có đăng ký gốc, chưa có khoản học phí (liên kết 2.2 → 3.1, 05_module mục 4.1)
        var dk = await db.DangKy.Include(d => d.MaHVNavigation).Include(d => d.SoPhieuDKNavigation)
            .Include(d => d.MaLopNavigation).ThenInclude(l => l.MaKHNavigation)
            .Include(d => d.MaLopNavigation).ThenInclude(l => l.LichTuan)
            .Include(d => d.MaLopNavigation).ThenInclude(l => l.BuoiHoc)
            .Where(d => d.MaLopGoc == null && d.TrangThai == "Đã đăng ký" && d.HocPhi == null)
            .OrderBy(d => d.SoPhieuDK).ToListAsync();
        var maHV = dk.Select(d => d.MaHV).ToList();
        var cu = await db.v_HocVienCu.Where(v => maHV.Contains(v.MaHV)).ToListAsync();
        DanhSach = dk.Select(d => new ChoLap(d, d.SoPhieuDKNavigation.NgayDK,
            cu.Any(v => v.MaHV == d.MaHV && v.NgayKetThuc < d.SoPhieuDKNavigation.NgayDK))).ToList();  // QT01
    }
}
