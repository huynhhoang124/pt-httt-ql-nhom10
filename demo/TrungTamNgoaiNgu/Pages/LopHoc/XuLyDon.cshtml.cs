using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.LopHoc;

/// <summary>F2.4 Đơn chuyển lớp, bảo lưu, nghỉ học + R2.4 Kết quả xử lý đơn (module 2.4; cây quyết định 04_dac_ta mục 2.1;
/// QT08–QT10). NV tuyển sinh nhập thay yêu cầu của học viên; hệ thống xét theo cây rồi cập nhật đăng ký.</summary>
[Quyen("2.4", "T")]
public class XuLyDonModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public static readonly string[] LoaiDon = ["Chuyển lớp", "Bảo lưu", "Nghỉ học"];
    public record Dong(DangKy Dk, int DaHoc, decimal ConNo);

    [BindProperty(SupportsGet = true)] public string? Hv { get; set; }
    [BindProperty] public string? Lop { get; set; }
    [BindProperty] public string Loai { get; set; } = "Chuyển lớp";
    [BindProperty] public string? LopDich { get; set; }
    [BindProperty] public string? LyDo { get; set; }
    public HocVien? HocVien { get; set; }
    public List<Dong> CacDangKy { get; set; } = [];
    public List<v_SiSoLop> CacLopMo { get; set; } = [];
    public List<DangKy> GanDay { get; set; } = [];   // R2.4
    public string? ThongBao { get; set; }

    public async Task<IActionResult> OnGetAsync(string? tb = null)
    {
        if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "2.4")) return Forbid();  // HV, PH gửi yêu cầu qua quầy tuyển sinh
        await NapAsync();
        ThongBao = tb;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        var d = CacDangKy.FirstOrDefault(x => x.Dk.MaLop == Lop);
        if (HocVien == null || d == null || !LoaiDon.Contains(Loai)) { ModelState.AddModelError("", "Chọn học viên, đăng ký và loại đơn."); return Page(); }
        if (string.IsNullOrWhiteSpace(LyDo)) { ModelState.AddModelError("", "Nhập lý do của học viên."); return Page(); }
        var dk = d.Dk;
        db.DangKy.Attach(dk);   // theo dõi thay đổi từ đây (HanBaoLuu, TrangThai…)
        var homNay = dongHo.HomNay;
        string ketQua;
        try
        {
            if (Loai == "Chuyển lớp")
            {
                var dich = CacLopMo.FirstOrDefault(l => l.MaLop == LopDich) ?? await db.v_SiSoLop.FirstOrDefaultAsync(l => l.MaLop == LopDich);
                if (dich == null || dich.MaLop == dk.MaLop) { ModelState.AddModelError("", "Chọn lớp đích khác lớp hiện tại."); return Page(); }
                var conCho = (dich.TrangThai is "Dự kiến" or "Đang học") && (dich.SoDangKy ?? 0) < dich.SiSoToiDa
                             && !await db.DangKy.AnyAsync(x => x.MaHV == dk.MaHV && x.MaLop == dich.MaLop);
                QuyTac.XetChuyenLop(d.DaHoc, await db.ThamSoAsync("SO_BUOI_CHUYEN_LOP"), dich.MaKH == dk.MaLopNavigation.MaKH, conCho);
                db.DangKy.Add(new DangKy { MaHV = dk.MaHV, MaLop = dich.MaLop, SoPhieuDK = dk.SoPhieuDK, TrangThai = "Đã đăng ký", MaLopGoc = dk.MaLop });
                ketQua = $"chuyển sang {dich.MaLop}, giữ số đã thu, không thu thêm phí";
            }
            else if (Loai == "Bảo lưu")
            {
                // "Mỗi khóa bảo lưu 1 lần": xét cả chuỗi đăng ký gốc → chuyển lớp → học lại (v_ChuoiDangKy)
                var dau = await db.v_ChuoiDangKy.Where(x => x.MaHV == dk.MaHV && x.MaLop == dk.MaLop).Select(x => x.MaLopDau).FirstAsync();
                var chuoi = await db.v_ChuoiDangKy.Where(c => c.MaHV == dk.MaHV && c.MaLopDau == dau).Select(c => c.MaLop).ToListAsync();
                var daBaoLuu = await db.DangKy.AnyAsync(x => x.MaHV == dk.MaHV && chuoi.Contains(x.MaLop) && x.TrangThai == "Bảo lưu");
                QuyTac.XetBaoLuu(d.ConNo, d.DaHoc, dk.MaLopNavigation.MaKHNavigation.SoBuoi, await db.ThamSoAsync("TY_LE_BUOI_BAO_LUU"), daBaoLuu);
                dk.HanBaoLuu = homNay.AddMonths((int)await db.ThamSoAsync("THANG_BAO_LUU"));
                ketQua = $"bảo lưu đến {HienThi.Ngay(dk.HanBaoLuu)}";
            }
            else ketQua = d.ConNo > 0 ? $"nghỉ học, không hoàn tiền; hủy khoản còn nợ {HienThi.Tien(d.ConNo)} (QT10)" : "nghỉ học, không hoàn tiền (QT10)";
        }
        catch (LoiNghiepVu ex)
        {
            ThongBaoLoi.Them(ModelState, ex);
            return Page();
        }
        dk.TrangThai = Loai;
        dk.NgayThayDoi = homNay;
        dk.LyDo = LyDo.Trim();
        db.GhiNhatKy(User, "Sửa", "DangKy", $"{dk.MaHV}/{dk.MaLop}: {Loai}", dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/LopHoc/XuLyDon?hv={dk.MaHV}&tb={Uri.EscapeDataString($"CHẤP NHẬN – {dk.MaHV} lớp {dk.MaLop}: {ketQua}.")}");
    }

    async Task NapAsync()
    {
        GanDay = await db.DangKy.AsNoTracking().Include(x => x.MaHVNavigation).Where(x => x.NgayThayDoi != null)
                            .OrderByDescending(x => x.NgayThayDoi).ThenBy(x => x.MaHV).Take(10).ToListAsync();
        if (string.IsNullOrWhiteSpace(Hv)) return;
        HocVien = await db.HocVien.FindAsync(Hv.Trim().ToUpperInvariant());
        if (HocVien == null) return;
        var dk = await db.DangKy.AsNoTracking().Include(x => x.MaLopNavigation.MaKHNavigation)
                         .Where(x => x.MaHV == HocVien.MaHV && x.TrangThai == "Đã đăng ký").ToListAsync();
        var lop = dk.Select(x => x.MaLop).ToList();
        var daHoc = await db.BuoiHoc.Where(b => lop.Contains(b.MaLop) && b.TrangThai == "Đã dạy")
                            .GroupBy(b => b.MaLop).ToDictionaryAsync(g => g.Key, g => g.Count());
        var no = await db.v_HocPhiDangKy.Where(h => h.MaHV == HocVien.MaHV && lop.Contains(h.MaLop)).ToDictionaryAsync(h => h.MaLop, h => h.ConNo ?? 0);
        CacDangKy = dk.Select(x => new Dong(x, daHoc.GetValueOrDefault(x.MaLop), no.GetValueOrDefault(x.MaLop))).ToList();
        var kh = dk.Select(x => x.MaLopNavigation.MaKH).ToList();
        CacLopMo = await db.v_SiSoLop.Where(l => kh.Contains(l.MaKH) && (l.TrangThai == "Dự kiến" || l.TrangThai == "Đang học"))
                           .OrderBy(l => l.MaLop).ToListAsync();
    }
}
