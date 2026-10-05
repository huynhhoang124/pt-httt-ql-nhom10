using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>N2 – Kiểm tra quyền truy cập theo ma trận 05_module mục 5 (bảng MaTran sinh trong PhanQuyen.g.cs).</summary>
public static partial class PhanQuyen
{
    /// <summary>Miền giá trị TaiKhoan.VaiTro (CK_TaiKhoan_VaiTro) → mã cột của ma trận.</summary>
    public static readonly Dictionary<string, string> TuVaiTroCsdl = new()
    {
        ["Học viên"] = "HV", ["Phụ huynh"] = "PH", ["Giáo viên"] = "GV", ["NV tuyển sinh"] = "TS",
        ["NV kế toán"] = "KT", ["QL đào tạo"] = "DT", ["Giám đốc"] = "GD", ["Quản trị viên"] = "QT",
    };

    public static string Quyen(string? vaiTro, string module)
    {
        int i = Array.IndexOf(VaiTro, vaiTro);
        return i >= 0 && MaTran.TryGetValue(module, out var q) ? q[i] : "";
    }

    /// <summary>Có mọi quyền trong chuỗi <paramref name="can"/> (vd "T", "TS"); chuỗi rỗng = có quyền bất kỳ (xem).</summary>
    public static bool Co(string? vaiTro, string module, string can = "")
    {
        var q = Quyen(vaiTro, module);
        return q.Length > 0 && can.All(q.Contains);
    }

    /// <summary>Quyền "x": chỉ xem trong phạm vi của mình (HV của mình, PH của con, GV lớp mình).</summary>
    public static bool ChiCuaMinh(string? vaiTro, string module) => !Quyen(vaiTro, module).Contains('X');
}

/// <summary>Gắn lên PageModel: module của trang và quyền cần để gửi dữ liệu (POST). GET chỉ cần có quyền bất kỳ.</summary>
[AttributeUsage(AttributeTargets.Class)]
public class QuyenAttribute(string module, string ghi = "") : Attribute
{
    public string Module { get; } = module;
    public string Ghi { get; } = ghi;
}

public class KiemTraQuyen : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var q = context.HandlerInstance.GetType().GetCustomAttribute<QuyenAttribute>();
        if (q != null)
        {
            var vt = context.HttpContext.User.VaiTro();
            var can = HttpMethods.IsPost(context.HttpContext.Request.Method) ? q.Ghi : "";
            if (!PhanQuyen.Co(vt, q.Module, can))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
        await next();
    }
}

public static class NguoiDung
{
    public static string? VaiTro(this ClaimsPrincipal u) => u.FindFirstValue(ClaimTypes.Role);
    public static string TenDangNhap(this ClaimsPrincipal u) => u.FindFirstValue(ClaimTypes.Name) ?? "";
    public static string? MaNV(this ClaimsPrincipal u) => u.FindFirstValue("MaNV");
    public static string? MaGV(this ClaimsPrincipal u) => u.FindFirstValue("MaGV");
    public static string? MaHV(this ClaimsPrincipal u) => u.FindFirstValue("MaHV");
    public static string? MaPH(this ClaimsPrincipal u) => u.FindFirstValue("MaPH");
    public static bool Co(this ClaimsPrincipal u, string module, string can = "") => PhanQuyen.Co(u.VaiTro(), module, can);
}
