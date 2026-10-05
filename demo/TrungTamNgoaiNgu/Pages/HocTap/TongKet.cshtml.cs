using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Pages.HocTap;

/// <summary>R4.3a Bảng điểm lớp chờ duyệt + F4.3 Duyệt kết quả (module 4.3; đặc tả 04_dac_ta mục 1.7, bảng quyết định 3.2;
/// QT13–QT15). Hệ thống tính lại từ điểm danh và điểm thành phần; chỉ khi QL đào tạo duyệt mới ghi KetQua (khóa điểm,
/// chuyển danh sách học viên đạt sang 4.4). Lớp đã duyệt thì hiện kết quả đã chốt.</summary>
[Quyen("4.3", "D")]
public class TongKetModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public record Dong(HocVien Hv, decimal? GiuaKy, decimal? CuoiKy, QuyTac.KetQuaTinh? Tinh, KetQua? DaChot);

    [BindProperty(SupportsGet = true)] public string? Lop { get; set; }
    [BindProperty] public string? GhiChu { get; set; }
    public List<Data.Entities.LopHoc> CacLop { get; set; } = [];
    public Data.Entities.LopHoc? Chon { get; set; }
    public List<Dong> DanhSach { get; set; } = [];
    public int SoBuoiDaDay { get; set; }
    public bool SanSang { get; set; }     // đã dạy hết số buổi, mọi học viên đủ điểm giữa kỳ + cuối kỳ
    public bool DaDuyet { get; set; }
    public bool DuocDuyet { get; set; }
    public string? ThongBao { get; set; }

    public async Task OnGetAsync(string? tb = null)
    {
        await NapAsync();
        ThongBao = tb;
    }

    public async Task<IActionResult> OnPostDuyetAsync()
    {
        await NapAsync();
        if (Chon == null) return NotFound();
        if (DaDuyet) return Redirect(DiaChi("Lớp đã được duyệt trước đó."));
        if (!SanSang)
        {
            ThongBaoLoi.Them(ModelState, new LoiNghiepVu("QT13", $"{SoBuoiDaDay}/{Chon.MaKHNavigation.SoBuoi} buổi đã dạy"));
            return Page();
        }
        var maNV = User.MaNV();
        foreach (var d in DanhSach.Where(d => d.DaChot == null))
            db.KetQua.Add(new KetQua
            {
                MaHV = d.Hv.MaHV, MaLop = Chon.MaLop, DiemTongKet = d.Tinh!.DiemTongKet, XepLoai = d.Tinh.XepLoai,
                KetQua1 = d.Tinh.KetQua, MaNVDuyet = maNV, NgayDuyet = dongHo.HomNay,
            });
        if (Chon.TrangThai == "Đang học")
        {
            db.LopHoc.Attach(Chon);
            Chon.TrangThai = "Kết thúc";
        }
        db.GhiNhatKy(User, "Duyệt", "KetQua", Chon.MaLop, dongHo.Bay);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ThongBaoLoi.Them(ModelState, ex))
        {
            return Page();
        }
        var dat = DanhSach.Count(d => d.Tinh?.KetQua == "Đạt");
        return Redirect(DiaChi($"Đã duyệt kết quả {DanhSach.Count} học viên; {dat} học viên đạt được chuyển sang cấp chứng nhận (4.4)."));
    }

    /// <summary>"Yêu cầu xem lại": không ghi KetQua, giáo viên vẫn sửa được điểm; ghi chú lưu ở nhật ký.</summary>
    public async Task<IActionResult> OnPostXemLaiAsync()
    {
        await NapAsync();
        if (Chon == null) return NotFound();
        if (string.IsNullOrWhiteSpace(GhiChu))
        {
            ModelState.AddModelError("", "Nhập ghi chú cho giáo viên (điểm nào cần xem lại). Gợi ý: vd \"HV0398 thiếu điểm cuối kỳ\".");
            return Page();
        }
        var ghi = $"{Chon.MaLop} xem lại: {GhiChu.Trim()}";
        db.GhiNhatKy(User, "Duyệt", "KetQua", ghi[..Math.Min(ghi.Length, 100)], dongHo.Bay);
        await db.SaveChangesAsync();
        return Redirect(DiaChi("Đã gửi yêu cầu xem lại; kết quả chưa được chốt."));
    }

    string DiaChi(string tb) => $"/HocTap/TongKet?lop={Chon!.MaLop}&tb={Uri.EscapeDataString(tb)}";

    async Task NapAsync()
    {
        var hv = PhamVi.HocVien(db, User, "4.3");   // null = xem mọi học viên
        var q = db.LopHoc.AsNoTracking().Include(l => l.MaKHNavigation).Include(l => l.MaGVNavigation)
                  .Where(l => l.TrangThai == "Đang học" || l.TrangThai == "Kết thúc");
        var lopGV = PhamVi.LopCuaGiaoVien(db, User.MaGV());
        if (User.VaiTro() == "GV") q = q.Where(l => lopGV.Contains(l.MaLop));
        else if (hv != null) q = q.Where(l => l.DangKy.Any(d => hv.Contains(d.MaHV)));
        CacLop = await q.OrderByDescending(l => l.NgayKhaiGiang).ToListAsync();
        Lop ??= CacLop.FirstOrDefault()?.MaLop;
        Chon = CacLop.FirstOrDefault(l => l.MaLop == Lop);
        if (Chon == null) return;

        var kh = Chon.MaKHNavigation;
        var n = new QuyTac.NguongXepLoai(await db.ThamSoAsync("DIEM_GIOI"), await db.ThamSoAsync("DIEM_KHA"),
                                         await db.ThamSoAsync("DIEM_DAT"), await db.ThamSoAsync("NGUONG_CHUYEN_CAN_DAT"));
        SoBuoiDaDay = await db.BuoiHoc.CountAsync(b => b.MaLop == Chon.MaLop && b.TrangThai == "Đã dạy");
        // Không tính đăng ký Chuyển lớp, Nghỉ học, Bảo lưu (04_dac_ta mục 1.7)
        var dk = await db.DangKy.AsNoTracking().Include(d => d.MaHVNavigation).Include(d => d.KetQua)
                         .Where(d => d.MaLop == Chon.MaLop && d.TrangThai == "Đã đăng ký").OrderBy(d => d.MaHV).ToListAsync();
        var diem = await db.Diem.AsNoTracking().Where(x => x.MaLop == Chon.MaLop).ToListAsync();
        var cc = await db.v_ChuyenCan.AsNoTracking().Where(x => x.MaLop == Chon.MaLop).ToDictionaryAsync(x => x.MaHV);
        DanhSach = dk.Select(d =>
        {
            decimal? gk = diem.FirstOrDefault(x => x.MaHV == d.MaHV && x.LoaiDiem == "Giữa kỳ")?.Diem1,
                     ck = diem.FirstOrDefault(x => x.MaHV == d.MaHV && x.LoaiDiem == "Cuối kỳ")?.Diem1;
            var c = cc.GetValueOrDefault(d.MaHV);
            var tinh = gk is null || ck is null ? null
                : QuyTac.TongKet(c?.SoBuoiDaHoc ?? 0, c?.SoBuoiCoMat ?? 0, gk.Value, ck.Value, kh.TSChuyenCan, kh.TSGiuaKy, kh.TSCuoiKy, n);
            return new Dong(d.MaHVNavigation, gk, ck, tinh, d.KetQua);
        }).ToList();
        SanSang = DanhSach.Count > 0 && SoBuoiDaDay >= kh.SoBuoi && DanhSach.All(d => d.Tinh != null);
        DaDuyet = DanhSach.Count > 0 && DanhSach.All(d => d.DaChot?.NgayDuyet != null);
        DuocDuyet = User.Co("4.3", "D") && !DaDuyet;
        if (hv != null)
        {
            var cuaMinh = await hv.ToListAsync();
            DanhSach = DanhSach.Where(d => cuaMinh.Contains(d.Hv.MaHV)).ToList();  // lọc sau khi tính: SanSang xét cả lớp
        }
    }
}
