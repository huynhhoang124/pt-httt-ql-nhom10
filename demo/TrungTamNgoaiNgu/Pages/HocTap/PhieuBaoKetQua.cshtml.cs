using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HocTap;

/// <summary>R4.3b – Phiếu báo kết quả học tập (module 4.3; 06_giao_dien mục 4 dòng R4.3b; mẫu 02_thu_thap mục 4.2.4).
/// Nguồn: KetQua, Diem, HocVien, LopHoc. Chỉ lấy kết quả đã duyệt (NgayDuyet có giá trị).
/// Một phiếu cho một học viên – một lớp: điểm chuyên cần, giữa kỳ, cuối kỳ, điểm tổng kết, xếp loại, Đạt/Không đạt,
/// nhận xét, người duyệt, ngày duyệt. Có tên trung tâm, in 1 trang A4. Học viên, phụ huynh, giáo viên chỉ thấy phạm vi của mình.</summary>
[Quyen("4.3")]
public class PhieuBaoKetQuaModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }
    [BindProperty(SupportsGet = true)] public string? Hv { get; set; }

    public List<Data.Entities.LopHoc> CacLop { get; set; } = [];
    public Data.Entities.LopHoc? LopChon { get; set; }
    public List<HocVien> CacHocVien { get; set; } = [];
    public HocVien? HocVienChon { get; set; }
    public KetQua? KetQuaChon { get; set; }
    public decimal? DiemChuyenCan { get; set; }
    public decimal? DiemGiuaKy { get; set; }
    public decimal? DiemCuoiKy { get; set; }
    public string? NhanXetGV { get; set; }
    public bool In { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await NapAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostInAsync()
    {
        await NapAsync();
        if (LopChon == null || HocVienChon == null || KetQuaChon == null) return NotFound();
        db.GhiNhatKy(User, "In", "KetQua", $"{LopChon.MaLop}/{HocVienChon.MaHV}", dongHo.Bay);
        await db.SaveChangesAsync();
        In = true;
        return Page();
    }

    async Task NapAsync()
    {
        var duocXem = PhamVi.HocVien(db, User, "4.3");
        var qLop = db.LopHoc.AsNoTracking().Include(l => l.MaKHNavigation).Include(l => l.MaGVNavigation).AsQueryable();

        if (User.VaiTro() == "GV")
        {
            var lopGV = PhamVi.LopCuaGiaoVien(db, User.MaGV());
            qLop = qLop.Where(l => lopGV.Contains(l.MaLop));
        }
        else if (duocXem != null)
        {
            qLop = qLop.Where(l => l.DangKy.Any(d => duocXem.Contains(d.MaHV)));
        }

        CacLop = await qLop.OrderByDescending(l => l.NgayKhaiGiang).ToListAsync();
        Lop ??= CacLop.FirstOrDefault()?.MaLop;
        LopChon = CacLop.FirstOrDefault(l => l.MaLop == Lop);

        if (LopChon == null) return;

        var qHv = db.DangKy.AsNoTracking()
            .Include(d => d.MaHVNavigation)
            .Where(d => d.MaLop == LopChon.MaLop && d.TrangThai != "Hủy");

        if (duocXem != null)
        {
            qHv = qHv.Where(d => duocXem.Contains(d.MaHV));
        }

        CacHocVien = await qHv.OrderBy(d => d.MaHV).Select(d => d.MaHVNavigation).ToListAsync();
        if (string.IsNullOrEmpty(Hv) || !CacHocVien.Any(h => h.MaHV == Hv))
        {
            Hv = CacHocVien.FirstOrDefault()?.MaHV;
        }

        HocVienChon = CacHocVien.FirstOrDefault(h => h.MaHV == Hv);
        if (HocVienChon == null) return;

        // Chỉ lấy kết quả đã duyệt (NgayDuyet có giá trị)
        KetQuaChon = await db.KetQua.AsNoTracking()
            .Include(k => k.ChungNhan)
            .Include(k => k.MaNVDuyetNavigation)
            .FirstOrDefaultAsync(k => k.MaLop == LopChon.MaLop && k.MaHV == HocVienChon.MaHV && k.NgayDuyet != null);

        if (KetQuaChon != null)
        {
            var diems = await db.Diem.AsNoTracking()
                .Where(x => x.MaLop == LopChon.MaLop && x.MaHV == HocVienChon.MaHV)
                .ToListAsync();

            DiemGiuaKy = diems.FirstOrDefault(x => x.LoaiDiem == "Giữa kỳ")?.Diem1;
            DiemCuoiKy = diems.FirstOrDefault(x => x.LoaiDiem == "Cuối kỳ")?.Diem1;
            NhanXetGV = diems.FirstOrDefault(x => x.LoaiDiem == "Cuối kỳ" && !string.IsNullOrEmpty(x.NhanXet))?.NhanXet
                     ?? diems.FirstOrDefault(x => x.LoaiDiem == "Giữa kỳ" && !string.IsNullOrEmpty(x.NhanXet))?.NhanXet;

            var cc = await db.v_ChuyenCan.AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLop == LopChon.MaLop && x.MaHV == HocVienChon.MaHV);
            if (cc is { SoBuoiDaHoc: > 0 })
            {
                DiemChuyenCan = Math.Round(10.0m * cc.SoBuoiCoMat!.Value / cc.SoBuoiDaHoc.Value, 1, MidpointRounding.AwayFromZero);
            }
        }
    }
}
