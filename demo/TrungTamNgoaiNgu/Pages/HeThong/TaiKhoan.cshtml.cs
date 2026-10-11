using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HeThong;

/// <summary>F5.1a – Tài khoản, phân quyền (module 5.1; 04_dac_ta mục 1.1). Cấp tài khoản cho người đã có hồ sơ (nhân viên,
/// giáo viên, học viên, phụ huynh) với mật khẩu tạm, bắt đổi ở lần đăng nhập đầu (B2); khóa / mở; đặt lại mật khẩu.
/// Quyền theo vai trò lấy từ ma trận 05_module mục 5, không lưu theo từng tài khoản.</summary>
[Quyen("5.1", "S")]
public class TaiKhoanModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    [BindProperty] public string? Ten { get; set; }
    [BindProperty] public string VaiTro { get; set; } = "Học viên";
    [BindProperty] public string? MaChu { get; set; }
    public List<Data.Entities.TaiKhoan> DanhSach { get; set; } = [];
    [BindProperty(SupportsGet = true)] public string? Tim { get; set; }
    public TrangDs Trang { get; set; } = new(1, 50, 0);
    public string? ThongBao { get; set; }
    public string? MatKhauTam { get; set; }

    public async Task OnGetAsync() => await NapAsync();

    public async Task<IActionResult> OnPostThemAsync()
    {
        await NapAsync();
        if (!User.Co("5.1", "T")) return Forbid();
        var ten = Ten?.Trim().ToLowerInvariant();
        var ma = MaChu?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(ten) || string.IsNullOrEmpty(ma) || !PhanQuyen.TuVaiTroCsdl.ContainsKey(VaiTro))
        {
            ModelState.AddModelError("", "Nhập tên đăng nhập, vai trò và mã chủ tài khoản.");
            return Page();
        }
        if (await db.TaiKhoan.AnyAsync(t => t.TenDangNhap == ten)) { ModelState.AddModelError("", $"Tên đăng nhập {ten} đã có."); return Page(); }
        var tk = new Data.Entities.TaiKhoan { TenDangNhap = ten, VaiTro = VaiTro, TrangThai = "Hoạt động", PhaiDoiMatKhau = true };
        // Chủ tài khoản phải có hồ sơ và chưa có tài khoản (mỗi người một tài khoản)
        var (co, daCo) = PhanQuyen.TuVaiTroCsdl[VaiTro] switch
        {
            "GV" => (await db.GiaoVien.AnyAsync(x => x.MaGV == ma), await db.TaiKhoan.AnyAsync(x => x.MaGV == ma)),
            "HV" => (await db.HocVien.AnyAsync(x => x.MaHV == ma), await db.TaiKhoan.AnyAsync(x => x.MaHV == ma)),
            "PH" => (await db.PhuHuynh.AnyAsync(x => x.MaPH == ma), await db.TaiKhoan.AnyAsync(x => x.MaPH == ma)),
            _ => (await db.NhanVien.AnyAsync(x => x.MaNV == ma), await db.TaiKhoan.AnyAsync(x => x.MaNV == ma)),
        };
        switch (PhanQuyen.TuVaiTroCsdl[VaiTro])
        {
            case "GV": tk.MaGV = ma; break;
            case "HV": tk.MaHV = ma; break;
            case "PH": tk.MaPH = ma; break;
            default: tk.MaNV = ma; break;
        }
        if (!co) { ModelState.AddModelError("", $"Không có hồ sơ {ma} cho vai trò {VaiTro}. Gợi ý: nhập hồ sơ trước (F1.1, F1.2), rồi mới cấp tài khoản."); return Page(); }
        if (daCo) { ModelState.AddModelError("", $"{ma} đã có tài khoản. Gợi ý: đặt lại mật khẩu cho tài khoản cũ."); return Page(); }
        MatKhauTam = TaoMatKhau();
        tk.MatKhauBam = MatKhau.Bam(MatKhauTam);
        db.TaiKhoan.Add(tk);
        db.GhiNhatKy(User, "Thêm", "TaiKhoan", ten, dongHo.Bay);
        return await LuuAsync($"Đã cấp tài khoản {ten} ({VaiTro} {ma}).");
    }

    /// <summary>Đặt lại mật khẩu tạm và gỡ khóa tạm do nhập sai (B4); lần đăng nhập sau phải đổi mật khẩu (B2).</summary>
    public async Task<IActionResult> OnPostDatLaiAsync(string ten)
    {
        await NapAsync();
        var tk = await db.TaiKhoan.FindAsync(ten);
        if (tk == null) return NotFound();
        MatKhauTam = TaoMatKhau();
        (tk.MatKhauBam, tk.PhaiDoiMatKhau, tk.SoLanSai, tk.KhoaDen) = (MatKhau.Bam(MatKhauTam), true, 0, null);
        db.GhiNhatKy(User, "Sửa", "TaiKhoan", $"{ten}: đặt lại mật khẩu", dongHo.Bay);
        return await LuuAsync($"Đã đặt lại mật khẩu của {ten}.");
    }

    public async Task<IActionResult> OnPostKhoaAsync(string ten)
    {
        await NapAsync();
        var tk = await db.TaiKhoan.FindAsync(ten);
        if (tk == null) return NotFound();
        if (ten == User.TenDangNhap()) { ModelState.AddModelError("", "Không tự khóa tài khoản đang dùng."); return Page(); }
        tk.TrangThai = tk.TrangThai == "Hoạt động" ? "Khóa" : "Hoạt động";
        db.GhiNhatKy(User, tk.TrangThai == "Khóa" ? "Ngừng" : "Sửa", "TaiKhoan", $"{ten}: {tk.TrangThai}", dongHo.Bay);
        return await LuuAsync($"{ten}: {tk.TrangThai}.");
    }

    async Task<IActionResult> LuuAsync(string tb)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            MatKhauTam = null;
            return Page();
        }
        await NapAsync();
        ThongBao = tb;
        return Page();   // không chuyển trang: mật khẩu tạm chỉ hiện một lần, không đưa lên địa chỉ
    }

    /// <summary>Mật khẩu tạm 10 ký tự, bỏ các chữ dễ nhầm (0/O, 1/l/I).</summary>
    static string TaoMatKhau() => RandomNumberGenerator.GetString("abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789", 10);

    async Task NapAsync()
    {
        var q = db.TaiKhoan.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(Tim))
        {
            var t = Tim.Trim();
            q = q.Where(x => x.TenDangNhap.Contains(t) || x.MaNV == t || x.MaGV == t || x.MaHV == t || x.MaPH == t);
        }
        int.TryParse(Request.Query["trang"], out var trang);
        int.TryParse(Request.Query["so"], out var so);
        (DanhSach, Trang) = await q.OrderBy(t => t.VaiTro).ThenBy(t => t.TenDangNhap).LayTrangAsync(trang, so);
    }
}
