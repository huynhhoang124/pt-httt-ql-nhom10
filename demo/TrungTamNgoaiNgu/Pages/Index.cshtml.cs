using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages;

/// <summary>Trang chủ: mục lục chức năng theo vai trò (06_giao_dien mục 5.1). Nhân viên có thêm thẻ số liệu nhanh, mỗi thẻ chỉ hiện
/// khi vai trò có quyền xem toàn bộ (X) ở module nguồn. Học viên, phụ huynh có "sổ liên lạc": lịch 7 ngày tới (2.3), chuyên cần (4.1),
/// học phí còn nợ (3.3), kết quả đã duyệt mới nhất (4.3), thông báo (5.2) – cùng phạm vi dữ liệu như các màn hình đó.</summary>
public class IndexModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record TheSo(string BieuTuong, string Nhan, string GiaTri, string Phu, string Url, bool Nguy = false);

    public List<TheSo> CacThe { get; set; } = [];

    // Sổ liên lạc
    public bool LaHvPh { get; set; }
    public List<HocVien> CacCon { get; set; } = [];
    public HocVien? Con { get; set; }
    public List<BuoiHoc> Lich { get; set; } = [];
    public v_ChuyenCan? ChuyenCan { get; set; }
    public string? LopChuyenCan { get; set; }
    public List<CongNo> No { get; set; } = [];
    public KetQua? KetQuaMoi { get; set; }
    public List<Data.Entities.ThongBao> ThongBao { get; set; } = [];

    public async Task OnGetAsync(string? hv)
    {
        var vt = User.VaiTro();
        var homNay = dongHo.HomNay;
        LaHvPh = vt is "HV" or "PH";
        if (LaHvPh) await SoLienLacAsync(vt!, hv, homNay);
        else await TheSoAsync(vt, homNay);
    }

    async Task TheSoAsync(string? vt, DateOnly homNay)
    {
        bool xem(string m) => PhanQuyen.Co(vt, m) && !PhanQuyen.ChiCuaMinh(vt, m);
        if (xem("2.1"))
        {
            var n = await db.LopHoc.CountAsync(l => l.TrangThai == "Đang học");
            var duKien = await db.LopHoc.CountAsync(l => l.TrangThai == "Dự kiến");
            CacThe.Add(new("book-open", "Lớp đang học", HienThi.So(n), $"{duKien} lớp dự kiến mở", "/BaoCao/TinhTrangLop"));
        }
        if (PhanQuyen.Co(vt, "2.3") || vt == "GV")
        {
            var gv = User.MaGV();
            var q = db.BuoiHoc.Where(b => b.Ngay == homNay && b.TrangThai != "Hủy");
            if (vt == "GV") q = q.Where(b => b.MaGV == gv);
            var buoi = await q.OrderBy(b => b.GioBatDau).Select(b => new { b.MaLop, b.GioBatDau }).ToListAsync();
            var dau = buoi.FirstOrDefault(b => b.GioBatDau >= TimeOnly.FromDateTime(dongHo.Bay)) ?? buoi.LastOrDefault();
            CacThe.Add(new("calendar-days", vt == "GV" ? "Buổi dạy hôm nay" : "Buổi học hôm nay", HienThi.So(buoi.Count),
                dau == null ? "Không có buổi nào" : $"{dau.GioBatDau:HH\\:mm} · {dau.MaLop}", vt == "GV" ? "/HocTap/DiemDanh" : "/LopHoc/XepLich"));
        }
        if (xem("1.1"))
        {
            var n = await db.DangKy.Where(d => d.TrangThai == "Đã đăng ký" && d.MaLopNavigation.TrangThai == "Đang học")
                .Select(d => d.MaHV).Distinct().CountAsync();
            CacThe.Add(new("users", "Học viên đang học", HienThi.So(n), "Có đăng ký ở lớp đang học", "/DanhMuc/HocVien"));
        }
        if (xem("3.3"))
        {
            var qh = await db.CongNoTai(homNay).Where(c => c.QuaHan == 1).ToListAsync();
            CacThe.Add(new("wallet", "Công nợ quá hạn", HienThi.Tien(qh.Sum(c => c.ConNo)), $"{qh.Count} khoản đã qua hạn đóng",
                "/HocPhi/CongNo?quaHan=true", qh.Count > 0));
        }
    }

    async Task SoLienLacAsync(string vt, string? hv, DateOnly homNay)
    {
        string? maHV = User.MaHV(), maPH = User.MaPH();
        CacCon = vt == "HV"
            ? await db.HocVien.Where(h => h.MaHV == maHV).ToListAsync()
            : await db.HocVienPhuHuynh.Where(x => x.MaPH == maPH).Select(x => x.MaHVNavigation).OrderBy(h => h.MaHV).ToListAsync();
        // phụ huynh chỉ chọn được con mình: mã lạ thì về con đầu tiên
        Con = CacCon.FirstOrDefault(h => h.MaHV == hv?.Trim().ToUpperInvariant()) ?? CacCon.FirstOrDefault();
        if (Con == null) return;
        var ma = Con.MaHV;

        var lopDangHoc = db.DangKy.Where(d => d.MaHV == ma && d.TrangThai == "Đã đăng ký").Select(d => d.MaLop);
        Lich = await db.BuoiHoc.AsNoTracking()
            .Where(b => lopDangHoc.Contains(b.MaLop) && b.TrangThai != "Hủy" && b.Ngay >= homNay && b.Ngay <= homNay.AddDays(6))
            .Include(b => b.MaLopNavigation).ThenInclude(l => l.MaKHNavigation).Include(b => b.MaGVNavigation)
            .OrderBy(b => b.Ngay).ThenBy(b => b.GioBatDau).Take(10).ToListAsync();

        // chuyên cần của lớp đang học (nếu không còn lớp nào đang học: lớp gần nhất)
        var cc = await db.v_ChuyenCan.Where(c => c.MaHV == ma && c.SoBuoiDaHoc > 0)
            .Join(db.LopHoc, c => c.MaLop, l => l.MaLop, (c, l) => new { c, l.TrangThai, l.NgayKhaiGiang, l.MaKHNavigation.TenKH })
            .OrderByDescending(x => x.TrangThai == "Đang học").ThenByDescending(x => x.NgayKhaiGiang).FirstOrDefaultAsync();
        ChuyenCan = cc?.c;
        LopChuyenCan = cc == null ? null : $"{cc.c.MaLop} · {cc.TenKH}";

        No = await db.CongNoTai(homNay).Where(c => c.MaHV == ma).OrderBy(c => c.HanDong).ToListAsync();

        KetQuaMoi = await db.KetQua.AsNoTracking().Where(k => k.MaHV == ma && k.NgayDuyet != null)
            .Include(k => k.DangKy).ThenInclude(d => d.MaLopNavigation).ThenInclude(l => l.MaKHNavigation)
            .OrderByDescending(k => k.NgayDuyet).FirstOrDefaultAsync();

        var tb = db.ThongBao.AsNoTracking().Where(t => t.MaHV == ma);   // cùng phạm vi với R5.2
        tb = vt == "PH" ? tb.Where(t => t.MaPH == maPH) : tb.Where(t => t.MaPH == null);
        ThongBao = await tb.OrderByDescending(t => t.ThoiGianGui).ThenByDescending(t => t.MaTB).Take(5).ToListAsync();
    }
}
