using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HocTap;

/// <summary>F4.2 – Nhập điểm thành phần (module 4.2). Giáo viên nhập cho lớp mình phụ trách; điểm bị khóa khi học viên đã
/// có kết quả tổng kết hoặc lớp đã Kết thúc / Hủy.</summary>
[Quyen("4.2", "T")]
public class NhapDiemModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public static readonly string[] LoaiDiemCo = ["Giữa kỳ", "Cuối kỳ"];
    public record Dong(HocVien Hv, Diem? Diem, bool Khoa);

    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }
    [BindProperty(SupportsGet = true)] public string Loai { get; set; } = "Giữa kỳ";
    [BindProperty] public Dictionary<string, string?> DiemNhap { get; set; } = [];
    [BindProperty] public Dictionary<string, string?> NhanXet { get; set; } = [];
    public List<Data.Entities.LopHoc> CacLop { get; set; } = [];
    public Data.Entities.LopHoc? Chon { get; set; }
    public List<Dong> DanhSach { get; set; } = [];
    public bool DuocGhi { get; set; }
    public bool DaLuu { get; set; }

    public async Task<IActionResult> OnGetAsync(bool daLuu = false)
    {
        if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "4.2") && User.VaiTro() != "GV") return Forbid();
        await NapAsync();
        DaLuu = daLuu;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await NapAsync();
        if (Chon == null || !LoaiDiemCo.Contains(Loai)) return NotFound();
        if (!DuocGhi) { ModelState.AddModelError("", "Lớp này không nhập điểm được (không phải lớp bạn phụ trách, hoặc lớp đã kết thúc)."); return Page(); }
        int so = 0;
        foreach (var d in DanhSach.Where(d => !d.Khoa))
        {
            var s = DiemNhap.GetValueOrDefault(d.Hv.MaHV)?.Trim();
            if (string.IsNullOrEmpty(s)) continue;  // chưa có điểm thì bỏ trống
            if (!decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var diem))
            {
                ThongBaoLoi.Them(ModelState, new LoiNghiepVu("CK_Diem_Diem", $"{d.Hv.MaHV}: \"{s}\""));
                continue;
            }
            var x = d.Diem ?? db.Diem.Add(new Diem { MaHV = d.Hv.MaHV, MaLop = Chon.MaLop, LoaiDiem = Loai }).Entity;
            if (d.Diem != null) db.Diem.Attach(x);
            x.Diem1 = diem;   // ngoài 0–10 thì CHECK CK_Diem_Diem chặn, N7 đổi thành câu báo lỗi
            x.NhanXet = string.IsNullOrWhiteSpace(NhanXet.GetValueOrDefault(d.Hv.MaHV)) ? null : NhanXet[d.Hv.MaHV]!.Trim();
            so++;
        }
        if (!ModelState.IsValid) return Page();
        db.GhiNhatKy(User, "Sửa", "Diem", $"{Chon.MaLop}/{Loai}", dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/HocTap/NhapDiem?lop={Chon.MaLop}&loai={Uri.EscapeDataString(Loai)}&daLuu=true");
    }

    async Task NapAsync()
    {
        var gv = User.MaGV();
        var q = db.LopHoc.AsNoTracking().Include(l => l.MaKHNavigation).Where(l => l.TrangThai != "Hủy" && l.TrangThai != "Dự kiến");
        if (User.VaiTro() == "GV") q = q.Where(l => l.MaGV == gv);  // phạm vi x: lớp mình phụ trách
        CacLop = await q.OrderByDescending(l => l.NgayKhaiGiang).ToListAsync();
        Lop ??= CacLop.FirstOrDefault()?.MaLop;
        Chon = CacLop.FirstOrDefault(l => l.MaLop == Lop);
        if (Chon == null) return;
        DuocGhi = User.Co("4.2", "T") && Chon.MaGV == gv && Chon.TrangThai == "Đang học";
        var dk = await db.DangKy.AsNoTracking().Include(d => d.MaHVNavigation).Include(d => d.KetQua)
                         .Where(d => d.MaLop == Chon.MaLop && d.TrangThai == "Đã đăng ký").OrderBy(d => d.MaHV).ToListAsync();
        var diem = await db.Diem.AsNoTracking().Where(x => x.MaLop == Chon.MaLop && x.LoaiDiem == Loai).ToDictionaryAsync(x => x.MaHV);
        DanhSach = dk.Select(d => new Dong(d.MaHVNavigation, diem.GetValueOrDefault(d.MaHV), d.KetQua != null)).ToList();
    }
}
