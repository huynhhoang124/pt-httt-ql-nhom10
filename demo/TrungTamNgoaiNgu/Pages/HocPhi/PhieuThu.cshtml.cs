using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.HocPhi;

/// <summary>F3.2 – Phiếu thu học phí (module 3.2; đặc tả 04_dac_ta mục 1.4; QT05). Phiếu đã lập không sửa (ma trận: 3.2 không có S).</summary>
[Quyen("3.2", "T")]
public class PhieuThuModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    /// <summary>Một đợt còn nợ. Khoản học phí nằm ở đăng ký đầu chuỗi (MaLop); MaLopHienTai là lớp đang học (khác nếu đã chuyển lớp).</summary>
    public record Dot(string MaLop, string MaLopHienTai, string TenKH, byte SoDot, decimal SoTienDot, decimal DaThuDot, DateOnly Han,
                      decimal PhaiNop, decimal DaThu)
    {
        public decimal ConNoDot => SoTienDot - DaThuDot;
    }

    [BindProperty(SupportsGet = true)] public string? Hv { get; set; }
    [BindProperty] public string NguoiNop { get; set; } = "";
    [BindProperty] public string HinhThuc { get; set; } = "Tiền mặt";
    [BindProperty] public List<decimal?> SoTien { get; set; } = [];
    public HocVien? HocVien { get; set; }
    public List<Dot> DanhSach { get; set; } = [];
    public List<string> PhuHuynh { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Co("3.2", "T")) return Forbid();
        await NapAsync();
        if (HocVien != null) NguoiNop = PhuHuynh.FirstOrDefault() ?? HocVien.HoTen;  // gợi ý sẵn, chỉ khi mở form
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        if (HocVien == null) { ModelState.AddModelError("", "Chọn học viên trước."); return Page(); }
        if (string.IsNullOrWhiteSpace(NguoiNop)) ModelState.AddModelError("", "Nhập họ tên người nộp.");
        if (HinhThuc is not ("Tiền mặt" or "Chuyển khoản")) ModelState.AddModelError("", "Chọn hình thức thanh toán.");
        if (!ModelState.IsValid) return Page();

        var dong = DanhSach.Select((d, i) => (d, tien: i < SoTien.Count ? SoTien[i] : null))
                           .Where(x => x.tien.HasValue)
                           .Select(x => new QuyTac.DongThu(HocVien.MaHV, x.d.MaLop, x.d.SoDot, x.d.PhaiNop, x.d.DaThu, x.d.ConNoDot, x.tien!.Value))
                           .ToList();
        try
        {
            QuyTac.KiemTraPhieuThu(dong, await db.ThamSoAsync("TY_LE_DOT_1"));
        }
        catch (LoiNghiepVu ex)
        {
            ThongBaoLoi.Them(ModelState, ex);
            return Page();
        }

        var homNay = dongHo.HomNay;
        var so = CsdlMoRong.SoTiepTheo(await db.PhieuThu.Select(p => p.SoPT).ToListAsync(), "PT", homNay.Year);
        var pt = new Data.Entities.PhieuThu { SoPT = so, NgayThu = homNay, NguoiNop = NguoiNop.Trim(), HinhThuc = HinhThuc, MaNV = User.MaNV()! };
        foreach (var d in dong)
            pt.ChiTietPhieuThu.Add(new ChiTietPhieuThu { SoPT = so, MaHV = d.MaHV, MaLop = d.MaLop, Dot = d.Dot, SoTien = d.SoTien });
        db.PhieuThu.Add(pt);
        db.GhiNhatKy(User, "Thêm", "PhieuThu", so, dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/HocPhi/InPhieuThu?so={so}");
    }

    async Task NapAsync()
    {
        if (string.IsNullOrWhiteSpace(Hv)) return;
        HocVien = await db.HocVien.FindAsync(Hv.Trim().ToUpperInvariant());
        if (HocVien == null) return;
        var ma = HocVien.MaHV;
        PhuHuynh = await db.HocVienPhuHuynh.Where(x => x.MaHV == ma).Select(x => x.MaPHNavigation.HoTen).ToListAsync();

        // Chỉ đăng ký hiện tại còn nợ, không tính đăng ký Nghỉ học (fn_CongNo, QT10)
        var no = await db.CongNoTai(dongHo.HomNay).Where(c => c.MaHV == ma).ToListAsync();
        var dau = no.Select(c => c.MaLopDau).ToList();
        var dot = await db.DotHocPhi.AsNoTracking().Include(d => d.HocPhi.DangKy.MaLopNavigation.MaKHNavigation)
                          .Where(d => d.MaHV == ma && dau.Contains(d.MaLop)).ToListAsync();
        var daThu = await db.ChiTietPhieuThu.Where(c => c.MaHV == ma && dau.Contains(c.MaLop))
                            .GroupBy(c => new { c.MaLop, c.Dot }).Select(g => new { g.Key.MaLop, g.Key.Dot, Tien = g.Sum(x => x.SoTien) })
                            .ToListAsync();
        DanhSach = dot.Select(d =>
            {
                var c = no.First(x => x.MaLopDau == d.MaLop);
                return new Dot(d.MaLop, c.MaLop, d.HocPhi.DangKy.MaLopNavigation.MaKHNavigation.TenKH, d.Dot, d.SoTien,
                               daThu.Where(t => t.MaLop == d.MaLop && t.Dot == d.Dot).Sum(t => t.Tien), d.HanDong, c.PhaiNop, c.DaThu);
            })
            .Where(d => d.ConNoDot > 0).OrderBy(d => d.Han).ThenBy(d => d.MaLop).ToList();
    }
}
