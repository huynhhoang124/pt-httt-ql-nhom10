using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TrungTamNgoaiNgu.Pages;

/// <summary>N7 – Trang lỗi chung: lỗi chưa lường trước (500) và địa chỉ không có (404, qua UseStatusCodePages).
/// Chi tiết kỹ thuật chỉ ghi ở nhật ký máy chủ; người dùng thấy câu tiếng Việt và mã lỗi để báo quản trị viên.</summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    [BindProperty(SupportsGet = true)] public int? Ma { get; set; }

    public string RequestId => Activity.Current?.Id ?? HttpContext.TraceIdentifier;
}
