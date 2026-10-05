using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>Vi phạm một quy tắc QT ở tầng ứng dụng. Khóa là "Nguồn lỗi" trong bảng 06_giao_dien mục 6 ("QT05", "QT05#2"…).</summary>
public class LoiNghiepVu(string khoa, string? chiTiet = null) : Exception(khoa)
{
    public string Khoa { get; } = khoa;
    public string? ChiTiet { get; } = chiTiet;
}

/// <summary>N7 – Đổi lỗi (quy tắc QT hoặc ràng buộc CSDL bị vi phạm) thành câu báo lỗi kèm gợi ý sửa.</summary>
public static partial class ThongBaoLoi
{
    public static string Cau(string khoa, string? chiTiet = null) =>
        Bang.TryGetValue(khoa, out var t) ? $"{t.ThongBao}{(chiTiet is null ? "" : $" ({chiTiet})")} Gợi ý: {t.GoiY}." : khoa;

    /// <summary>Tên ràng buộc trong thông báo của SQL Server → câu báo lỗi; null nếu không phải lỗi ràng buộc đã biết.</summary>
    public static string? TuCsdl(Exception ex)
    {
        for (var e = ex; e != null; e = e.InnerException)
            if (e is SqlException sql)
            {
                var khoa = Bang.Keys.FirstOrDefault(k => !k.StartsWith("QT") && (sql.Message.Contains($"\"{k}\"") || sql.Message.Contains($"'{k}'")));
                return khoa is null ? $"Dữ liệu vi phạm ràng buộc của CSDL: {sql.Message}" : Cau(khoa);
            }
        return null;
    }

    /// <summary>Đưa lỗi vào ModelState để hiện trên form; trả false nếu không phải lỗi nghiệp vụ / ràng buộc (để ném tiếp).</summary>
    public static bool Them(ModelStateDictionary ms, Exception ex)
    {
        var cau = ex is LoiNghiepVu l ? Cau(l.Khoa, l.ChiTiet) : TuCsdl(ex);
        if (cau is null) return false;
        ms.AddModelError("", cau);
        return true;
    }
}

public static class CsdlMoRong
{
    /// <summary>N3 – Ghi nhật ký thao tác; lưu cùng lần SaveChanges với dữ liệu nghiệp vụ.</summary>
    public static void GhiNhatKy(this TrungTamContext db, ClaimsPrincipal u, string thaoTac, string doiTuong, string ma, DateTime bay) =>
        db.NhatKy.Add(new NhatKy { TenDangNhap = u.TenDangNhap(), ThaoTac = thaoTac, DoiTuong = doiTuong, MaDoiTuong = ma, ThoiDiem = bay });

    /// <summary>Giá trị tham số chính sách (bảng ThamSo – 3.1 E26).</summary>
    public static async Task<decimal> ThamSoAsync(this TrungTamContext db, string ma) =>
        (await db.ThamSo.FindAsync(ma))?.GiaTri ?? throw new InvalidOperationException($"Thiếu tham số {ma}");

    /// <summary>Số chứng từ tiếp theo dạng TIỀNTỐ + năm + "-" + 4 chữ số (DK2026-0158, PT2026-0731).</summary>
    public static string SoTiepTheo(IEnumerable<string> daCo, string tienTo, int nam)
    {
        var dau = $"{tienTo}{nam}-";
        var max = daCo.Where(s => s.StartsWith(dau)).Select(s => int.TryParse(s[dau.Length..], out var n) ? n : 0).DefaultIfEmpty(0).Max();
        return $"{dau}{max + 1:D4}";
    }
}

/// <summary>Kết quả của dbo.fn_CongNo(@Ngay) (3.3).</summary>
public class CongNo
{
    public string MaHV { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string MaLop { get; set; } = "";
    public string MaLopDau { get; set; } = "";
    public decimal PhaiNop { get; set; }
    public decimal DaThu { get; set; }
    public decimal ConNo { get; set; }
    public DateOnly? HanDong { get; set; }
    public int QuaHan { get; set; }
}
