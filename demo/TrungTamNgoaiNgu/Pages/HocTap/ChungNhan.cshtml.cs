using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.HocTap;

/// <summary>R4.4 – Cấp và in giấy chứng nhận hoàn thành khóa học (module 4.4; 04_dac_ta mục 1.8; QT15).
/// QL đào tạo chọn học viên trong danh sách đạt của lớp đã duyệt (4.3) để cấp số CN{năm}-xxxx; HV, PH xem chứng nhận của mình.</summary>
[Quyen("4.4", "I")]
public class ChungNhanModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Dong(HocVien Hv, KetQua Kq);

    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }
    [BindProperty] public List<string> Chon { get; set; } = [];
    public List<Data.Entities.LopHoc> CacLop { get; set; } = [];
    public Data.Entities.LopHoc? LopChon { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public List<Dong> DanhSach { get; set; } = [];
    public bool DuocCap { get; set; }
    public bool In { get; set; }
    public string? ThongBao { get; set; }

    public async Task OnGetAsync(string? tb = null)
    {
        await NapAsync();
        ThongBao = tb;
    }

    public async Task<IActionResult> OnPostCapAsync()
    {
        await NapAsync();
        if (LopChon == null) return NotFound();
        if (Chon.Count == 0) { ModelState.AddModelError("", "Chưa chọn học viên nào. Gợi ý: đánh dấu học viên cần cấp chứng nhận."); return Page(); }
        var daCo = await db.ChungNhan.Select(c => c.SoCN).ToListAsync();
        var cap = new List<string>();
        foreach (var ma in Chon.Distinct())
        {
            var d = DanhSach.FirstOrDefault(d => d.Hv.MaHV == ma);
            if (d == null || !QuyTac.DuocCapChungNhan(d.Kq.KetQua1, d.Kq.NgayDuyet))
            {
                ThongBaoLoi.Them(ModelState, new LoiNghiepVu("QT15", $"{ma}: {d?.Kq.KetQua1 ?? "chưa có kết quả"}"));
                return Page();
            }
            if (d.Kq.ChungNhan != null) continue;   // đã cấp rồi thì bỏ qua
            var so = CsdlMoRong.SoTiepTheo(daCo, "CN", dongHo.HomNay.Year);
            daCo.Add(so);
            cap.Add(so);
            db.ChungNhan.Add(new Data.Entities.ChungNhan { SoCN = so, MaHV = ma, MaLop = LopChon.MaLop, NgayCap = dongHo.HomNay });
        }
        if (cap.Count == 0) return Redirect(DiaChi("Các học viên đã chọn đều đã có chứng nhận."));
        db.GhiNhatKy(User, "Thêm", "ChungNhan", $"{LopChon.MaLop}: {cap[0]}…{cap[^1]}", dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect(DiaChi($"Đã cấp {cap.Count} chứng nhận: {string.Join(", ", cap)}."));
    }

    /// <summary>Bấm In: ghi nhật ký (N3) rồi mở hộp thoại in của trình duyệt.</summary>
    public async Task<IActionResult> OnPostInAsync()
    {
        await NapAsync();
        if (LopChon == null) return NotFound();
        db.GhiNhatKy(User, "In", "ChungNhan", LopChon.MaLop, dongHo.Bay);
        await db.SaveChangesAsync();
        In = true;
        return Page();
    }

    string DiaChi(string tb) => $"/HocTap/ChungNhan?lop={LopChon!.MaLop}&tb={Uri.EscapeDataString(tb)}";

    async Task NapAsync()
    {
        var hv = PhamVi.HocVien(db, User, "4.4");   // null = xem mọi học viên
        var q = db.LopHoc.AsNoTracking().Include(l => l.MaKHNavigation).Where(l => l.DangKy.Any(d => d.KetQua != null));
        if (hv != null) q = q.Where(l => l.DangKy.Any(d => hv.Contains(d.MaHV) && d.KetQua != null));
        CacLop = await q.OrderByDescending(l => l.NgayKhaiGiang).ToListAsync();
        Lop ??= CacLop.FirstOrDefault()?.MaLop;
        LopChon = CacLop.FirstOrDefault(l => l.MaLop == Lop);
        if (LopChon == null) return;
        NgayKetThuc = await db.BuoiHoc.Where(b => b.MaLop == LopChon.MaLop && b.TrangThai == "Đã dạy").MaxAsync(b => (DateOnly?)b.Ngay);
        var kq = db.KetQua.AsNoTracking().Include(k => k.ChungNhan).Include(k => k.DangKy.MaHVNavigation).Where(k => k.MaLop == LopChon.MaLop);
        if (hv != null) kq = kq.Where(k => hv.Contains(k.MaHV));
        DanhSach = (await kq.OrderBy(k => k.MaHV).ToListAsync()).Select(k => new Dong(k.DangKy.MaHVNavigation, k)).ToList();
        DuocCap = User.Co("4.4", "I");
    }
}
