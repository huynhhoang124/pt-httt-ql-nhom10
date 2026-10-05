using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TrungTamContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("TrungTam")));
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All)); // giữ nguyên chữ tiếng Việt trong HTML
builder.Services.AddSingleton<DongHo>();
builder.Services.AddSingleton<KhoaDangNhap>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/DangNhap";                 // N1
        o.AccessDeniedPath = "/KhongCoQuyen";      // N2
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/");
    o.Conventions.AllowAnonymousToPage("/DangNhap");
    o.Conventions.AllowAnonymousToPage("/Error");
}).AddMvcOptions(o =>
{
    o.Filters.Add<KiemTraQuyen>();  // N2: mọi trang có [Quyen] đều qua ma trận phân quyền
    // Trường bắt buộc do trang tự kiểm tra và báo bằng tiếng Việt (06_giao_dien mục 6); tắt [Required] ngầm của string không-null
    o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

public partial class Program { } // cho dự án kiểm thử (WebApplicationFactory)
