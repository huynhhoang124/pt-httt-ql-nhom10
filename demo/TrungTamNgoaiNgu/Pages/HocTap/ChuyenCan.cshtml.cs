using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.HocTap;

/// <summary>R4.1 – Chuyên cần của học viên (module 4.1; 06_giao_dien mục 4 dòng R4.1; QT12).
/// Nguồn: v_ChuyenCan, DiemDanh, BuoiHoc.
/// Học viên xem của mình, phụ huynh xem các con, nhân viên có quyền X tra cứu theo mã học viên.
/// Mỗi lớp: tỷ lệ chuyên cần, số buổi có mặt / vắng có phép / vắng không phép, cảnh báo nếu vượt ngưỡng vắng (QT12);
/// bấm vào lớp xem từng buổi (ngày, trạng thái, ghi chú).</summary>
[Quyen("4.1")]
public class ChuyenCanModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record ThongKeLop(
        LopHoc Lop,
        string TrangThaiDangKy,
        int SoBuoiDaHoc,
        int SoCoMat,
        int SoDiMuon,
        int SoVangCoPhep,
        int SoVangKhongPhep,
        decimal? TyLeChuyenCan,
        bool CanhBaoVang,
        bool NguyCoKhongDat
    );

    public record ChiTietBuoi(
        short SoBuoi,
        DateOnly Ngay,
        TimeOnly GioBatDau,
        TimeOnly GioKetThuc,
        string MaPhong,
        string TrangThaiBuoi,
        string? TrangThaiDiemDanh,
        string TenTrangThai,
        string? GhiChu,
        string? NoiDung
    );

    public static readonly Dictionary<string, string> TenTrangThai = new()
    {
        ["x"] = "Có mặt",
        ["M"] = "Đi muộn",
        ["P"] = "Vắng có phép",
        ["K"] = "Vắng không phép",
    };

    [BindProperty(SupportsGet = true)] public string? MaHV { get; set; }
    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }

    public bool CoQuyenNhapHV { get; set; }
    public HocVien? HocVienChon { get; set; }
    public List<HocVien> CacHocVienDuocXem { get; set; } = [];
    public List<ThongKeLop> DanhSachLop { get; set; } = [];
    public LopHoc? LopChon { get; set; }
    public ThongKeLop? ThongKeLopChon { get; set; }
    public List<ChiTietBuoi> DanhSachBuoi { get; set; } = [];
    public decimal NguongCanhBaoVang { get; set; }
    public decimal NguongChuyenCanDat { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        NguongCanhBaoVang = await db.ThamSoAsync("NGUONG_CANH_BAO_VANG");
        NguongChuyenCanDat = await db.ThamSoAsync("NGUONG_CHUYEN_CAN_DAT");

        var vaiTro = User.VaiTro();
        CoQuyenNhapHV = !PhanQuyen.ChiCuaMinh(vaiTro, "4.1");

        if (vaiTro == "HV")
        {
            MaHV = User.MaHV();
        }
        else if (vaiTro == "PH")
        {
            var dsCon = await db.HocVienPhuHuynh.AsNoTracking()
                .Where(x => x.MaPH == User.MaPH())
                .Select(x => x.MaHV)
                .ToListAsync();
            CacHocVienDuocXem = await db.HocVien.AsNoTracking()
                .Where(h => dsCon.Contains(h.MaHV))
                .OrderBy(h => h.MaHV)
                .ToListAsync();
            if (string.IsNullOrEmpty(MaHV) || !dsCon.Contains(MaHV))
                MaHV = dsCon.FirstOrDefault();
        }
        else if (vaiTro == "GV")
        {
            var dsLopGV = PhamVi.LopCuaGiaoVien(db, User.MaGV());
            var hvLopGV = await db.DangKy.AsNoTracking()
                .Where(d => dsLopGV.Contains(d.MaLop))
                .Select(d => d.MaHV)
                .Distinct()
                .ToListAsync();
            CacHocVienDuocXem = await db.HocVien.AsNoTracking()
                .Where(h => hvLopGV.Contains(h.MaHV))
                .OrderBy(h => h.MaHV)
                .ToListAsync();
            if (string.IsNullOrEmpty(MaHV) || !hvLopGV.Contains(MaHV))
                MaHV = hvLopGV.FirstOrDefault();
        }
        else
        {
            MaHV = MaHV?.Trim().ToUpper();
        }

        if (string.IsNullOrEmpty(MaHV))
        {
            return Page();
        }

        // Kiểm tra phạm vi dữ liệu
        if (PhamVi.HocVien(db, User, "4.1") is { } duocXem)
        {
            if (!await duocXem.AnyAsync(h => h == MaHV))
                return Forbid();
        }

        HocVienChon = await db.HocVien.AsNoTracking().FirstOrDefaultAsync(h => h.MaHV == MaHV);
        if (HocVienChon == null)
        {
            ModelState.AddModelError("", $"Không tìm thấy học viên có mã '{MaHV}'. Gợi ý: kiểm tra lại mã học viên.");
            return Page();
        }

        // Lấy danh sách đăng ký các lớp của học viên
        var dangKy = await db.DangKy.AsNoTracking()
            .Include(d => d.MaLopNavigation).ThenInclude(l => l.MaKHNavigation)
            .Include(d => d.MaLopNavigation).ThenInclude(l => l.MaGVNavigation)
            .Where(d => d.MaHV == MaHV && d.TrangThai != "Hủy")
            .OrderByDescending(d => d.MaLopNavigation.NgayKhaiGiang)
            .ToListAsync();

        var cacMaLop = dangKy.Select(d => d.MaLop).ToList();

        // Lấy dữ liệu từ view v_ChuyenCan
        var chuyenCan = await db.v_ChuyenCan.AsNoTracking()
            .Where(c => c.MaHV == MaHV && cacMaLop.Contains(c.MaLop))
            .ToDictionaryAsync(c => c.MaLop);

        // Lấy chi tiết điểm danh các lớp này
        var diemDanh = await db.DiemDanh.AsNoTracking()
            .Where(dd => dd.MaHV == MaHV && cacMaLop.Contains(dd.MaLop))
            .ToListAsync();

        DanhSachLop = dangKy.Select(d =>
        {
            var l = d.MaLopNavigation;
            var ddLop = diemDanh.Where(x => x.MaLop == l.MaLop).ToList();
            var cc = chuyenCan.GetValueOrDefault(l.MaLop);

            int coMat = ddLop.Count(x => x.TrangThai == "x");
            int diMuon = ddLop.Count(x => x.TrangThai == "M");
            int vangPhep = ddLop.Count(x => x.TrangThai == "P");
            int vangKoPhep = ddLop.Count(x => x.TrangThai == "K");
            int soBuoiDaHoc = cc?.SoBuoiDaHoc ?? ddLop.Count;

            return new ThongKeLop(
                l,
                d.TrangThai,
                soBuoiDaHoc,
                coMat,
                diMuon,
                vangPhep,
                vangKoPhep,
                cc?.TyLeChuyenCan,
                cc?.CanhBaoVang == 1,
                cc?.NguyCoKhongDat == 1
            );
        }).ToList();

        if (string.IsNullOrEmpty(Lop))
            Lop = DanhSachLop.FirstOrDefault()?.Lop.MaLop;

        ThongKeLopChon = DanhSachLop.FirstOrDefault(x => x.Lop.MaLop == Lop);
        LopChon = ThongKeLopChon?.Lop;

        if (LopChon != null)
        {
            var buoiHocs = await db.BuoiHoc.AsNoTracking()
                .Where(b => b.MaLop == LopChon.MaLop)
                .OrderBy(b => b.SoBuoi)
                .ToListAsync();

            var ddLopChon = diemDanh.Where(x => x.MaLop == LopChon.MaLop).ToDictionary(x => x.SoBuoi);

            DanhSachBuoi = buoiHocs.Select(b =>
            {
                var dd = ddLopChon.GetValueOrDefault(b.SoBuoi);
                string tt = dd?.TrangThai != null && TenTrangThai.TryGetValue(dd.TrangThai, out var t)
                    ? t
                    : (b.TrangThai == "Đã dạy" ? "Chưa điểm danh" : b.TrangThai);

                return new ChiTietBuoi(
                    b.SoBuoi,
                    b.Ngay,
                    b.GioBatDau,
                    b.GioKetThuc,
                    b.MaPhong,
                    b.TrangThai,
                    dd?.TrangThai,
                    tt,
                    dd?.GhiChu,
                    b.NoiDung
                );
            }).ToList();
        }

        return Page();
    }
}
