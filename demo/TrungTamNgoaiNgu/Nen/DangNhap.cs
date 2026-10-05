using System.Security.Cryptography;
using System.Text;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>N1 – Băm và kiểm tra mật khẩu (TaiKhoan.MatKhauBam).</summary>
public static class MatKhau
{
    const int VongLap = 100_000;

    /// <summary>Mật khẩu mới: PBKDF2-SHA256, muối ngẫu nhiên 16 byte, dạng "pbkdf2$vòng$muối$băm".</summary>
    public static string Bam(string matKhau)
    {
        var muoi = RandomNumberGenerator.GetBytes(16);
        var bam = Rfc2898DeriveBytes.Pbkdf2(matKhau, muoi, VongLap, HashAlgorithmName.SHA256, 32);
        return $"pbkdf2${VongLap}${Convert.ToBase64String(muoi)}${Convert.ToBase64String(bam)}";
    }

    public static bool Dung(string matKhau, string luu)
    {
        var p = luu.Split('$');
        if (p[0] == "pbkdf2" && p.Length == 4)
        {
            var bam = Rfc2898DeriveBytes.Pbkdf2(matKhau, Convert.FromBase64String(p[2]), int.Parse(p[1]),
                                                HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(bam, Convert.FromBase64String(p[3]));
        }
        if (p[0] == "sha256" && p.Length == 2) // dạng băm của dữ liệu mẫu (csdl/sinh_du_lieu.py); đổi mật khẩu sẽ chuyển sang pbkdf2
        {
            var bam = SHA256.HashData(Encoding.UTF8.GetBytes("nhom10:" + matKhau));
            return CryptographicOperations.FixedTimeEquals(bam, Convert.FromHexString(p[1]));
        }
        return false;
    }
}

/// <summary>N1 – Khóa tạm tài khoản khi nhập sai nhiều lần. Số lần sai và mốc thời gian lưu ở TaiKhoan.SoLanSai, KhoaDen
/// (bảo trì B4: trước đây đếm trong bộ nhớ, mất khi khởi động lại máy chủ).</summary>
public static class KhoaDangNhap
{
    public const int SoLanSai = 5;
    public static readonly TimeSpan ThoiGianKhoa = TimeSpan.FromMinutes(15);

    public static bool DangKhoa(int lan, DateTime? den, DateTime bay) => lan >= SoLanSai && bay < den;

    /// <summary>Thêm một lần sai: quá mốc <paramref name="den"/> thì đếm lại từ 1; mốc mới = bây giờ + 15 phút.</summary>
    public static (byte Lan, DateTime Den) Sai(int lan, DateTime? den, DateTime bay) =>
        ((byte)(den is null || bay >= den ? 1 : Math.Min(lan + 1, byte.MaxValue)), bay + ThoiGianKhoa);
}

/// <summary>Giờ hệ thống. Bản demo đặt cố định trong appsettings (HeThong:ThoiDiem) cho khớp dữ liệu mẫu.</summary>
public class DongHo(IConfiguration cfg)
{
    readonly DateTime? _coDinh = DateTime.TryParse(cfg["HeThong:ThoiDiem"], out var t) ? t : null;
    public DateTime Bay => _coDinh ?? DateTime.Now;
    public DateOnly HomNay => DateOnly.FromDateTime(Bay);
}
