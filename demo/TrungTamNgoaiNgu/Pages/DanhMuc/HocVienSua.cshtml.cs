using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.DanhMuc;

/// <summary>F1.1 – Hồ sơ học viên, kết quả kiểm tra đầu vào (module 1.1; đặc tả 04_dac_ta mục 1.1).</summary>
[Quyen("1.1", "T")]
public class HocVienSuaModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public class HoSo
    {
        [Required(ErrorMessage = "Nhập họ tên."), StringLength(100), Display(Name = "Họ tên")] public string HoTen { get; set; } = "";
        [Required(ErrorMessage = "Nhập ngày sinh."), Display(Name = "Ngày sinh")] public DateOnly? NgaySinh { get; set; }
        [Required, Display(Name = "Giới tính")] public string GioiTinh { get; set; } = "Nam";
        [Required(ErrorMessage = "Nhập điện thoại."), Display(Name = "Điện thoại")] public string DienThoai { get; set; } = "";
        [Display(Name = "Email")] public string? Email { get; set; }
        [Display(Name = "Địa chỉ")] public string? DiaChi { get; set; }
        [Display(Name = "Trình độ đầu vào")] public string? TrinhDoDauVao { get; set; }
        [Display(Name = "Ngày kiểm tra")] public DateOnly? NgayKiemTra { get; set; }
        [Display(Name = "Trạng thái")] public string TrangThai { get; set; } = "Hoạt động";
        // thêm một phụ huynh (tùy chọn)
        [Display(Name = "Họ tên phụ huynh")] public string? PhHoTen { get; set; }
        [Display(Name = "Điện thoại phụ huynh")] public string? PhDienThoai { get; set; }
        [Display(Name = "Email phụ huynh")] public string? PhEmail { get; set; }
        [Display(Name = "Quan hệ")] public string? QuanHe { get; set; }
    }

    public static readonly string[] TrinhDo = ["A1", "A2", "B1", "B2", "C1", "C2"];
    public static readonly string[] QuanHeCo = ["Bố", "Mẹ", "Người giám hộ"];

    [BindProperty(SupportsGet = true)] public string? Ma { get; set; }
    [BindProperty] public HoSo F { get; set; } = new();
    public List<HocVienPhuHuynh> PhuHuynh { get; set; } = [];
    public bool DaLuu { get; set; }
    public bool ChoSua => Ma == null ? User.Co("1.1", "T") : User.Co("1.1", "S");

    public async Task<IActionResult> OnGetAsync(bool daLuu = false)
    {
        DaLuu = daLuu;
        if (Ma == null) return User.Co("1.1", "T") ? Page() : Forbid();
        var h = await TimAsync();
        if (h == null) return NotFound();
        F = new HoSo
        {
            HoTen = h.HoTen, NgaySinh = h.NgaySinh, GioiTinh = h.GioiTinh, DienThoai = h.DienThoai, Email = h.Email, DiaChi = h.DiaChi,
            TrinhDoDauVao = h.TrinhDoDauVao, NgayKiemTra = h.NgayKiemTra, TrangThai = h.TrangThai,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ChoSua) return Forbid();
        HocVien? h = null;
        if (Ma != null && (h = await TimAsync()) == null) return NotFound();
        if (!ModelState.IsValid) return Page();

        // Kiểm tra trùng: cùng điện thoại và ngày sinh (04_dac_ta mục 1.1)
        var trung = await db.HocVien.Where(x => x.DienThoai == F.DienThoai.Trim() && x.NgaySinh == F.NgaySinh && x.MaHV != Ma)
                                    .Select(x => x.MaHV + " " + x.HoTen).FirstOrDefaultAsync();
        if (trung != null)
        {
            ModelState.AddModelError("", $"Trùng hồ sơ đã có: {trung} (cùng điện thoại và ngày sinh). Gợi ý: mở hồ sơ cũ để cập nhật, không thêm mới.");
            return Page();
        }
        bool themPh = !string.IsNullOrWhiteSpace(F.PhHoTen) || !string.IsNullOrWhiteSpace(F.PhDienThoai);
        if (themPh && (string.IsNullOrWhiteSpace(F.PhHoTen) || string.IsNullOrWhiteSpace(F.PhDienThoai) || F.QuanHe == null))
        {
            ModelState.AddModelError("", "Thêm phụ huynh cần đủ họ tên, điện thoại và quan hệ.");
            return Page();
        }
        var homNay = dongHo.HomNay;
        if (F.NgaySinh!.Value.AddYears(18) > homNay && !themPh && !PhuHuynh.Any())
        {
            ModelState.AddModelError("", "Học viên dưới 18 tuổi bắt buộc có ít nhất 1 phụ huynh (04_dac_ta mục 1.1). Gợi ý: nhập phụ huynh ở phần dưới.");
            return Page();
        }

        bool moi = h == null;
        if (moi)
        {
            h = new HocVien { MaHV = await MaMoiAsync(db.HocVien.Select(x => x.MaHV), "HV"), NgayTiepNhan = homNay };
            db.HocVien.Add(h);
        }
        h!.HoTen = F.HoTen.Trim(); h.NgaySinh = F.NgaySinh.Value; h.GioiTinh = F.GioiTinh; h.DienThoai = F.DienThoai.Trim();
        h.Email = Rong(F.Email); h.DiaChi = Rong(F.DiaChi); h.TrinhDoDauVao = Rong(F.TrinhDoDauVao); h.NgayKiemTra = F.NgayKiemTra;
        h.TrangThai = F.TrangThai;
        if (themPh)
        {
            var ph = await db.PhuHuynh.FirstOrDefaultAsync(p => p.DienThoai == F.PhDienThoai!.Trim());
            if (ph == null)
            {
                ph = new PhuHuynh { MaPH = await MaMoiAsync(db.PhuHuynh.Select(x => x.MaPH), "PH"), HoTen = F.PhHoTen!.Trim(),
                                    DienThoai = F.PhDienThoai!.Trim(), Email = Rong(F.PhEmail) };
                db.PhuHuynh.Add(ph);
            }
            if (!PhuHuynh.Any(x => x.MaPH == ph.MaPH))
                db.HocVienPhuHuynh.Add(new HocVienPhuHuynh { MaHV = h.MaHV, MaPH = ph.MaPH, QuanHe = F.QuanHe! });
        }
        db.GhiNhatKy(User, moi ? "Thêm" : (h.TrangThai == "Ngừng" ? "Ngừng" : "Sửa"), "HocVien", h.MaHV, dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        return Redirect($"/DanhMuc/HocVienSua?ma={h.MaHV}&daLuu=true");
    }

    async Task<HocVien?> TimAsync()
    {
        var ds = db.HocVien.Where(x => x.MaHV == Ma);
        if (PhamVi.HocVien(db, User, "1.1") is { } duocXem) ds = ds.Where(x => duocXem.Contains(x.MaHV));
        var h = await ds.FirstOrDefaultAsync();
        if (h != null)
            PhuHuynh = await db.HocVienPhuHuynh.Include(x => x.MaPHNavigation).Where(x => x.MaHV == h.MaHV).ToListAsync();
        return h;
    }

    static async Task<string> MaMoiAsync(IQueryable<string> ma, string tienTo)
    {
        var so = (await ma.ToListAsync()).Select(m => int.TryParse(m[tienTo.Length..], out var n) ? n : 0).DefaultIfEmpty(0).Max();
        return $"{tienTo}{so + 1:D4}";
    }

    static string? Rong(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
