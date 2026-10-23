using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.LopHoc;

/// <summary>R2.3a/R2.3b - Lich hoc cua hoc vien, lich day va danh sach lop.</summary>
[Quyen("2.3")]
public class LichHocModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Buoi(
        short SoBuoi,
        DateOnly Ngay,
        TimeOnly GioBatDau,
        TimeOnly GioKetThuc,
        string MaPhong,
        string TenPhong,
        string MaGV,
        string TenGV,
        string Loai,
        string TrangThai,
        string? NoiDung
    );

    public record LopLich(Data.Entities.LopHoc Lop, List<Buoi> CacBuoi);

    public record HocVienLop(HocVien HocVien, int SoBuoiVang, decimal? TyLeChuyenCan);

    public record LopDay(Data.Entities.LopHoc Lop, List<Buoi> CacBuoi, List<HocVienLop> DanhSachHocVien);

    [BindProperty(SupportsGet = true)] public string? MaHV { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? TuNgay { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? DenNgay { get; set; }

    public bool CoQuyenNhapHV { get; set; }
    public HocVien? HocVienChon { get; set; }
    public List<HocVien> CacHocVienDuocXem { get; set; } = [];
    public List<LopLich> LichHoc { get; set; } = [];
    public List<LopDay> LichDay { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        TuNgay ??= dongHo.HomNay;
        DenNgay ??= TuNgay.Value.AddDays(28);
        if (DenNgay < TuNgay)
        {
            ModelState.AddModelError("", "Ngày kết thúc phải từ ngày bắt đầu trở đi.");
            return Page();
        }

        var vaiTro = User.VaiTro();
        CoQuyenNhapHV = !PhanQuyen.ChiCuaMinh(vaiTro, "2.3");

        if (vaiTro == "GV")
        {
            await NapLichDayAsync();
            return Page();
        }

        var duocXem = PhamVi.HocVien(db, User, "2.3");
        if (vaiTro == "HV")
        {
            MaHV = User.MaHV();
        }
        else if (vaiTro == "PH")
        {
            var maCon = await duocXem!.Order().ToListAsync();
            CacHocVienDuocXem = await db.HocVien.AsNoTracking()
                .Where(h => maCon.Contains(h.MaHV)).OrderBy(h => h.MaHV).ToListAsync();
            if (string.IsNullOrEmpty(MaHV) || !maCon.Contains(MaHV))
                MaHV = maCon.FirstOrDefault();
        }
        else
        {
            MaHV = MaHV?.Trim().ToUpperInvariant();
        }

        if (string.IsNullOrEmpty(MaHV)) return Page();
        if (duocXem is { } phamVi && !await phamVi.AnyAsync(h => h == MaHV)) return Forbid();

        HocVienChon = await db.HocVien.AsNoTracking().FirstOrDefaultAsync(h => h.MaHV == MaHV);
        if (HocVienChon == null)
        {
            ModelState.AddModelError("", $"Không tìm thấy học viên có mã '{MaHV}'. Gợi ý: kiểm tra lại mã học viên.");
            return Page();
        }

        await NapLichHocAsync(MaHV);
        return Page();
    }

    async Task NapLichHocAsync(string maHV)
    {
        var dangKy = await db.DangKy.AsNoTracking()
            .Include(d => d.MaLopNavigation).ThenInclude(l => l.MaKHNavigation)
            .Include(d => d.MaLopNavigation).ThenInclude(l => l.MaGVNavigation)
            .Where(d => d.MaHV == maHV && d.TrangThai == "Đã đăng ký")
            .OrderByDescending(d => d.MaLopNavigation.NgayKhaiGiang)
            .ToListAsync();
        var maLop = dangKy.Select(d => d.MaLop).ToList();
        var buoi = await LayBuoiAsync(maLop);

        LichHoc = dangKy.Select(d => new LopLich(
            d.MaLopNavigation,
            buoi.Where(b => b.MaLop == d.MaLop).Select(TaoBuoi).ToList()
        )).ToList();
    }

    async Task NapLichDayAsync()
    {
        var maGV = User.MaGV();
        var maLop = await PhamVi.LopCuaGiaoVien(db, maGV).ToListAsync();
        var buoi = await db.BuoiHoc.AsNoTracking()
            .Include(b => b.MaLopNavigation).ThenInclude(l => l.MaKHNavigation)
            .Include(b => b.MaLopNavigation).ThenInclude(l => l.MaGVNavigation)
            .Include(b => b.MaGVNavigation)
            .Include(b => b.MaPhongNavigation)
            .Where(b => b.MaGV == maGV && maLop.Contains(b.MaLop) && b.Ngay >= TuNgay && b.Ngay <= DenNgay)
            .OrderBy(b => b.Ngay).ThenBy(b => b.GioBatDau)
            .ToListAsync();

        foreach (var nhom in buoi.GroupBy(b => b.MaLop))
        {
            var lop = nhom.First().MaLopNavigation;
            var dangKy = await db.DangKy.AsNoTracking()
                .Include(d => d.MaHVNavigation)
                .Where(d => d.MaLop == lop.MaLop && d.TrangThai == "Đã đăng ký")
                .OrderBy(d => d.MaHV).ToListAsync();
            var maHV = dangKy.Select(d => d.MaHV).ToList();
            var chuyenCan = await db.v_ChuyenCan.AsNoTracking()
                .Where(c => c.MaLop == lop.MaLop && maHV.Contains(c.MaHV))
                .ToDictionaryAsync(c => c.MaHV);
            var danhSach = dangKy.Select(d =>
            {
                var cc = chuyenCan.GetValueOrDefault(d.MaHV);
                var daHoc = cc?.SoBuoiDaHoc ?? 0;
                var coMat = cc?.SoBuoiCoMat ?? 0;
                return new HocVienLop(d.MaHVNavigation, Math.Max(0, daHoc - coMat), cc?.TyLeChuyenCan);
            }).ToList();
            LichDay.Add(new LopDay(lop, nhom.Select(TaoBuoi).ToList(), danhSach));
        }
    }

    async Task<List<BuoiHoc>> LayBuoiAsync(List<string> maLop) => await db.BuoiHoc.AsNoTracking()
        .Include(b => b.MaGVNavigation)
        .Include(b => b.MaPhongNavigation)
        .Where(b => maLop.Contains(b.MaLop) && b.Ngay >= TuNgay && b.Ngay <= DenNgay)
        .OrderBy(b => b.Ngay).ThenBy(b => b.GioBatDau).ToListAsync();

    static Buoi TaoBuoi(BuoiHoc b) => new(
        b.SoBuoi, b.Ngay, b.GioBatDau, b.GioKetThuc, b.MaPhong, b.MaPhongNavigation.TenPhong,
        b.MaGV, b.MaGVNavigation.HoTen, b.Loai, b.TrangThai, b.NoiDung);
}