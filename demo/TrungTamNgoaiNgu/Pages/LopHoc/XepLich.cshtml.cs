using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.LopHoc;

/// <summary>F2.3 – Xếp lịch, đổi lịch, học bù (module 2.3; 04_dac_ta mục 1.3; QT07). Sinh các buổi học từ lịch tuần;
/// đổi ngày, giờ, phòng, giáo viên (dạy thay) của buổi chưa dạy; thêm buổi học bù. Mọi buổi đều qua kiểm tra trùng lịch.</summary>
[Quyen("2.3", "T")]
public class XepLichModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }
    [BindProperty] public short? SoBuoi { get; set; }      // trống = thêm buổi học bù
    [BindProperty] public DateOnly? Ngay { get; set; }
    [BindProperty] public TimeOnly? GioBatDau { get; set; }
    [BindProperty] public TimeOnly? GioKetThuc { get; set; }
    [BindProperty] public string? MaPhong { get; set; }
    [BindProperty] public string? MaGV { get; set; }
    [BindProperty] public string? NoiDung { get; set; }
    public List<Data.Entities.LopHoc> CacLop { get; set; } = [];
    public Data.Entities.LopHoc? Chon { get; set; }
    public List<BuoiHoc> CacBuoi { get; set; } = [];
    public List<string> CacPhong { get; set; } = [];
    public List<GiaoVien> CacGV { get; set; } = [];
    public string? ThongBao { get; set; }

    public async Task<IActionResult> OnGetAsync(string? tb = null)
    {
        if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "2.3")) return Forbid();  // HV, PH, GV xem lịch ở R2.3a, R2.3b
        await NapAsync();
        ThongBao = tb;
        return Page();
    }

    /// <summary>Sinh toàn bộ buổi học của lớp chưa có buổi nào. Trùng một buổi thì không ghi buổi nào.</summary>
    public async Task<IActionResult> OnPostSinhAsync()
    {
        await NapAsync();
        if (Chon == null) return NotFound();
        if (CacBuoi.Count > 0) { ModelState.AddModelError("", "Lớp đã có lịch. Gợi ý: đổi từng buổi hoặc thêm buổi học bù."); return Page(); }
        var lich = await db.LichTuan.Where(l => l.MaLop == Chon.MaLop).Select(l => new { l.Thu, l.GioBatDau, l.MaPhong }).ToListAsync();
        if (lich.Count == 0) { ModelState.AddModelError("", "Lớp chưa có lịch tuần. Gợi ý: nhập lịch tuần ở F2.1 Mở lớp."); return Page(); }
        var kh = Chon.MaKHNavigation;
        var moi = QuyTac.SinhBuoi(Chon.MaLop, Chon.NgayKhaiGiang, lich.Select(l => (l.Thu, l.GioBatDau, l.MaPhong)).ToList(),
                                  kh.SoBuoi, kh.ThoiLuongBuoi, Chon.MaGV);
        var daXep = await DaXepAsync(moi[0].Ngay, moi[^1].Ngay);
        foreach (var b in moi)
            if (QuyTac.TrungLich(b, daXep) is { } x) return LoiTrung(b, x);
        db.BuoiHoc.AddRange(moi.Select(b => new BuoiHoc
        {
            MaLop = b.MaLop, SoBuoi = b.SoBuoi, Ngay = b.Ngay, GioBatDau = b.BatDau, GioKetThuc = b.KetThuc,
            MaPhong = b.MaPhong, MaGV = b.MaGV, Loai = "Thường", NoiDung = $"Bài {b.SoBuoi}", TrangThai = "Kế hoạch",
        }));
        db.GhiNhatKy(User, "Thêm", "BuoiHoc", $"{Chon.MaLop}: {moi.Count} buổi", dongHo.Bay);
        return await LuuAsync($"Đã sinh {moi.Count} buổi, từ {HienThi.Ngay(moi[0].Ngay)} đến {HienThi.Ngay(moi[^1].Ngay)}.");
    }

    /// <summary>Đổi lịch một buổi chưa dạy (SoBuoi có giá trị) hoặc thêm buổi học bù (SoBuoi trống).</summary>
    public async Task<IActionResult> OnPostLuuBuoiAsync()
    {
        await NapAsync();
        if (Chon == null) return NotFound();
        if (Ngay is null || GioBatDau is null || GioKetThuc is null || string.IsNullOrEmpty(MaPhong) || string.IsNullOrEmpty(MaGV))
        {
            ModelState.AddModelError("", "Nhập đủ ngày, giờ bắt đầu, giờ kết thúc, phòng và giáo viên.");
            return Page();
        }
        var cu = SoBuoi is null ? null : CacBuoi.FirstOrDefault(b => b.SoBuoi == SoBuoi);
        if (SoBuoi != null && (cu == null || cu.TrangThai != "Kế hoạch"))
        {
            ModelState.AddModelError("", $"Buổi {SoBuoi} không đổi được (không có hoặc đã dạy / đã hủy). Gợi ý: thêm buổi học bù.");
            return Page();
        }
        var so = cu?.SoBuoi ?? (short)(CacBuoi.Select(b => (int)b.SoBuoi).DefaultIfEmpty(0).Max() + 1);
        var b = new QuyTac.Ca(Chon.MaLop, so, Ngay.Value, GioBatDau.Value, GioKetThuc.Value, MaPhong, MaGV);
        if (QuyTac.TrungLich(b, await DaXepAsync(b.Ngay, b.Ngay)) is { } x) return LoiTrung(b, x);
        var buoi = cu ?? db.BuoiHoc.Add(new BuoiHoc { MaLop = Chon.MaLop, SoBuoi = so, Loai = "Học bù", TrangThai = "Kế hoạch" }).Entity;
        if (cu != null) db.BuoiHoc.Attach(buoi);
        (buoi.Ngay, buoi.GioBatDau, buoi.GioKetThuc, buoi.MaPhong, buoi.MaGV) = (b.Ngay, b.BatDau, b.KetThuc, b.MaPhong, b.MaGV);
        buoi.NoiDung = string.IsNullOrWhiteSpace(NoiDung) ? buoi.NoiDung : NoiDung.Trim();
        db.GhiNhatKy(User, cu == null ? "Thêm" : "Sửa", "BuoiHoc", $"{Chon.MaLop}/{so}", dongHo.Bay);
        return await LuuAsync(cu == null ? $"Đã thêm buổi học bù số {so}." : $"Đã đổi lịch buổi {so}.");
    }

    IActionResult LoiTrung(QuyTac.Ca b, QuyTac.Ca x)
    {
        var cai = x.MaGV == b.MaGV ? $"giáo viên {x.MaGV}" : $"phòng {x.MaPhong}";
        ThongBaoLoi.Them(ModelState, new LoiNghiepVu("QT07",
            $"buổi {b.SoBuoi} ngày {HienThi.Ngay(b.Ngay)} {b.BatDau:HH\\:mm}–{b.KetThuc:HH\\:mm} trùng {cai} với {x.MaLop} buổi {x.SoBuoi} {x.BatDau:HH\\:mm}–{x.KetThuc:HH\\:mm}"));
        return Page();
    }

    async Task<IActionResult> LuuAsync(string tb)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/LopHoc/XepLich?lop={Chon!.MaLop}&tb={Uri.EscapeDataString(tb)}");
    }

    /// <summary>Lịch đã xếp (D2) trong khoảng ngày, trừ buổi đã hủy.</summary>
    Task<List<QuyTac.Ca>> DaXepAsync(DateOnly tu, DateOnly den) =>
        db.BuoiHoc.Where(x => x.TrangThai != "Hủy" && x.Ngay >= tu && x.Ngay <= den)
          .Select(x => new QuyTac.Ca(x.MaLop, x.SoBuoi, x.Ngay, x.GioBatDau, x.GioKetThuc, x.MaPhong, x.MaGV)).ToListAsync();

    async Task NapAsync()
    {
        CacLop = await db.LopHoc.AsNoTracking().Include(l => l.MaKHNavigation).Include(l => l.LichTuan)
                         .Where(l => l.TrangThai == "Dự kiến" || l.TrangThai == "Đang học")
                         .OrderByDescending(l => l.NgayKhaiGiang).ToListAsync();
        Lop ??= CacLop.FirstOrDefault()?.MaLop;
        Chon = CacLop.FirstOrDefault(l => l.MaLop == Lop);
        CacPhong = await db.PhongHoc.Select(p => p.MaPhong).OrderBy(p => p).ToListAsync();
        CacGV = await db.GiaoVien.AsNoTracking().Where(g => g.TinhTrang == "Đang dạy").OrderBy(g => g.MaGV).ToListAsync();
        if (Chon == null) return;
        CacBuoi = await db.BuoiHoc.AsNoTracking().Where(b => b.MaLop == Chon.MaLop).OrderBy(b => b.SoBuoi).ToListAsync();
    }
}
