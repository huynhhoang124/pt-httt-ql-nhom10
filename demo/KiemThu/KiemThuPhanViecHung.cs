using System.IO.Compression;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.Pages.HeThong;
using TrungTamNgoaiNgu.Pages.HocPhi;

namespace KiemThu;

public class KiemThuPhanViecHung
{
    static TrungTamContext Context() => new(new DbContextOptionsBuilder<TrungTamContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    static DongHo Clock() => new(new ConfigurationBuilder().AddInMemoryCollection(
        new Dictionary<string, string?> { ["HeThong:ThoiDiem"] = "2027-01-04T19:30:00" }).Build());
    static PageContext Page(string role, string? claim = null, string? code = null) => new()
    {
        RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
        ActionDescriptor = new CompiledPageActionDescriptor { ViewEnginePath = "/HocPhi/HocPhiPhaiThu" },
        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.Name, "test") }
                .Concat(claim == null ? [] : new[] { new Claim(claim, code!) }), "test")) }
    };

    static async Task Seed(TrungTamContext db, int count = 3)
    {
        foreach (var i in Enumerable.Range(1, count))
        {
            var ma = $"HV{i:000}";
            var ten = $"Học viên {i}";
            var hv = new HocVien { MaHV = ma, HoTen = ten, GioiTinh = "Nam", DienThoai = "0900000000", TrangThai = "Hoạt động" };
            var dk = new DangKy { MaHV = ma, MaLop = "L01", SoPhieuDK = "DK" + ma, TrangThai = "Nghỉ học", MaHVNavigation = hv };
            db.HocPhi.Add(new() { MaHV = ma, MaLop = "L01", HocPhiGoc = 1000, TyLeGiam = 10, SoDot = 1,
                DangKy = dk, DotHocPhi = [new() { MaHV = ma, MaLop = "L01", Dot = 1, SoTien = 900 }] });
        }
        db.HocVienPhuHuynh.Add(new() { MaHV = "HV001", MaPH = "PH001", QuanHe = "Bố" });
        db.HocVienPhuHuynh.Add(new() { MaHV = "HV002", MaPH = "PH001", QuanHe = "Bố" });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task R31_TongVaExcelBaoGomMoiTrang()
    {
        using var db = Context();
        await Seed(db, 23);
        var p = new HocPhiPhaiThuModel(db, Clock()) { PageContext = Page("KT") };
        await p.OnGetAsync(null, 2, 20);
        Assert.Equal(3, p.DanhSach.Count);
        Assert.Equal(23, p.Trang.Tong);
        Assert.Equal(20700m, p.TongPhaiNop);
        var file = Assert.IsType<FileContentResult>(await p.OnGetAsync("excel", 2, 20));
        using var zip = new ZipArchive(new MemoryStream(file.FileContents));
        using var r = new StreamReader(zip.GetEntry("xl/worksheets/sheet1.xml")!.Open());
        var xml = System.Xml.Linq.XDocument.Parse(await r.ReadToEndAsync());
        System.Xml.Linq.XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        Assert.Equal(25, xml.Descendants(ns + "row").Count()); // tiêu đề + 23 khoản + tổng
    }

    [Theory]
    [InlineData("HV", true)]
    [InlineData("PH", true)]
    [InlineData("GV", false)]
    [InlineData("TS", true)]
    [InlineData("KT", true)]
    [InlineData("DT", false)]
    [InlineData("GD", true)]
    [InlineData("QT", false)]
    public async Task R31_BoLocQuyenTamVaiTro(string role, bool allowed)
    {
        using var db = Context();
        var p = new HocPhiPhaiThuModel(db, Clock());
        var context = new PageHandlerExecutingContext(Page(role), new List<IFilterMetadata>(), null!,
            new Dictionary<string, object?>(), p);
        bool called = false;
        await new KiemTraQuyen().OnPageHandlerExecutionAsync(context, () =>
        {
            called = true;
            return Task.FromResult<PageHandlerExecutedContext>(null!);
        });
        Assert.Equal(allowed, called);
        if (!allowed) Assert.IsType<ForbidResult>(context.Result);
        Assert.Equal(allowed, ThucDon.ChoVaiTro(role).SelectMany(x => x.Module).SelectMany(x => x.ManHinh).Any(x => x.Ma == "R3.1"));
    }

    [Theory]
    [InlineData("HV", "MaHV", "HV001", 1, 900)]
    [InlineData("PH", "MaPH", "PH001", 2, 1800)]
    [InlineData("KT", null, null, 3, 2700)]
    public async Task R31_PhamViVaExcel(string role, string? claim, string? code, int count, int total)
    {
        using var db = Context();
        await Seed(db);
        var p = new HocPhiPhaiThuModel(db, Clock()) { PageContext = Page(role, claim, code) };
        Assert.IsType<PageResult>(await p.OnGetAsync(null, 1, 20));
        Assert.Equal(count, p.Trang.Tong);
        Assert.Equal(total, p.TongPhaiNop);
        Assert.All(p.DanhSach, h => Assert.Equal("Nghỉ học", h.DangKy.TrangThai));
        var file = Assert.IsType<FileContentResult>(await p.OnGetAsync("excel", 1, 20));
        using var zip = new ZipArchive(new MemoryStream(file.FileContents));
        using var r = new StreamReader(zip.GetEntry("xl/worksheets/sheet1.xml")!.Open());
        var xml = await r.ReadToEndAsync();
        if (role != "KT") Assert.DoesNotContain("HV003", xml);
        if (role == "HV") Assert.DoesNotContain("HV002", xml);
        Assert.Contains($"<v>{total}</v>", xml);
        p.MaHV = "HV003";
        await p.OnGetAsync(null, 1, 20);
        Assert.Equal(role == "KT" ? 1 : 0, p.Trang.Tong);
        Assert.Equal(role == "KT" ? 900m : 0m, p.TongPhaiNop);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    public async Task F51b_ChanPostPhienBan(decimal value)
    {
        using var db = Context();
        db.ThamSo.Add(new() { MaThamSo = "PHIEN_BAN_CSDL", TenThamSo = "Phiên bản CSDL", GiaTri = 1, DonVi = "phiên bản" });
        await db.SaveChangesAsync();
        var p = new ThamSoModel(db, Clock()) { PageContext = Page("QT"), GiaTri = new() { ["PHIEN_BAN_CSDL"] = value } };
        Assert.IsType<PageResult>(await p.OnPostAsync());
        Assert.False(p.ModelState.IsValid);
        db.ChangeTracker.Clear();
        Assert.Equal(1m, (await db.ThamSo.SingleAsync()).GiaTri);
        Assert.Empty(await db.NhatKy.ToListAsync());
    }

    [Fact]
    public async Task F51b_VanLuuThamSoChinhSach()
    {
        using var db = Context();
        db.ThamSo.Add(new() { MaThamSo = "PHIEN_BAN_CSDL", TenThamSo = "Phiên bản CSDL", GiaTri = 1, DonVi = "phiên bản" });
        db.ThamSo.Add(new() { MaThamSo = "SISO_TOI_DA", TenThamSo = "Sĩ số", GiaTri = 20, DonVi = "học viên" });
        await db.SaveChangesAsync();
        var p = new ThamSoModel(db, Clock()) { PageContext = Page("QT"), GiaTri = new() { ["SISO_TOI_DA"] = 18 } };
        await p.OnPostAsync();
        Assert.True(p.ModelState.IsValid);
        db.ChangeTracker.Clear();
        Assert.Equal(1m, (await db.ThamSo.FindAsync("PHIEN_BAN_CSDL"))!.GiaTri);
        Assert.Equal(18m, (await db.ThamSo.FindAsync("SISO_TOI_DA"))!.GiaTri);
        Assert.Single(await db.NhatKy.ToListAsync());
    }
}
