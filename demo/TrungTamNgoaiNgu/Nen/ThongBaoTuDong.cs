using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Data.Entities;
using TrungTamNgoaiNgu.NghiepVu;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>5.2 – Lô thông báo hằng ngày (04_dac_ta mục 1.8): lịch học thay đổi trong 7 ngày tới, học phí sắp đến hạn / quá hạn
/// (QT16), cảnh báo vắng (QT12, cây 2.2), kết quả vừa duyệt. Gửi học viên, thêm phụ huynh nếu dưới 18 tuổi. Không gửi lại
/// thông báo đã có (cùng loại, người nhận, nội dung), nên chạy lại nhiều lần trong ngày vẫn an toàn.</summary>
public static class ThongBaoTuDong
{
    static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public static async Task<int> ChayAsync(TrungTamContext db, DateTime bay)
    {
        var homNay = DateOnly.FromDateTime(bay);
        var moi = new List<(string Loai, string MaHV, string NoiDung)>();

        // Lịch học thay đổi: buổi nghỉ, học bù, dạy thay trong 7 ngày tới → mọi học viên đang học của lớp
        var den = homNay.AddDays(7);
        var buoi = await db.BuoiHoc.Where(b => b.Ngay >= homNay && b.Ngay <= den
                                               && (b.Loai == "Học bù" || b.TrangThai == "Hủy" || b.MaGV != b.MaLopNavigation.MaGV))
                           .Select(b => new { b.MaLop, b.SoBuoi, b.Ngay, b.GioBatDau, b.MaPhong, b.MaGV, b.Loai, b.TrangThai,
                                              HV = b.MaLopNavigation.DangKy.Where(d => d.TrangThai == "Đã đăng ký").Select(d => d.MaHV).ToList() })
                           .ToListAsync();
        foreach (var b in buoi)
        {
            var nd = b.TrangThai == "Hủy" ? $"Lớp {b.MaLop} nghỉ buổi {b.SoBuoi} ngày {b.Ngay:dd/MM/yyyy}."
                : b.Loai == "Học bù" ? $"Lớp {b.MaLop} học bù buổi {b.SoBuoi} ngày {b.Ngay:dd/MM/yyyy} lúc {b.GioBatDau:HH\\:mm}, phòng {b.MaPhong}."
                : $"Lớp {b.MaLop} buổi {b.SoBuoi} ngày {b.Ngay:dd/MM/yyyy}: giáo viên {b.MaGV} dạy thay.";
            moi.AddRange(b.HV.Select(hv => ("Lịch học", hv, nd)));
        }

        // Học phí sắp đến hạn hoặc quá hạn (QT16)
        var soNgay = await db.ThamSoAsync("SO_NGAY_NHAC_HOC_PHI");
        foreach (var c in await db.CongNoTai(homNay).ToListAsync())
            if (QuyTac.NhacHocPhi(c.MaLop, c.ConNo, c.HanDong, homNay, soNgay) is { } nd) moi.Add(("Học phí", c.MaHV, nd));

        // Cảnh báo vắng (QT12) của lớp đang học
        decimal nguong = await db.ThamSoAsync("NGUONG_CANH_BAO_VANG"), dat = await db.ThamSoAsync("NGUONG_CHUYEN_CAN_DAT");
        var vang = await db.v_ChuyenCan.Where(x => x.CanhBaoVang == 1 && x.TrangThai == "Đã đăng ký"
                                                   && db.LopHoc.Any(l => l.MaLop == x.MaLop && l.TrangThai == "Đang học"))
                           .Join(db.HocVien, x => x.MaHV, h => h.MaHV, (x, h) => new { x.MaHV, x.MaLop, x.NguyCoKhongDat, h.HoTen }).ToListAsync();
        moi.AddRange(vang.Select(v => ("Chuyên cần", v.MaHV, QuyTac.CanhBaoVang(v.HoTen, v.MaLop, nguong, v.NguyCoKhongDat == 1, dat))));

        // Kết quả vừa được duyệt (4.3) trong 7 ngày qua
        var tu = homNay.AddDays(-7);
        foreach (var k in await db.KetQua.Where(k => k.NgayDuyet >= tu).ToListAsync())
            moi.Add(("Kết quả", k.MaHV, $"Kết quả lớp {k.MaLop}: điểm tổng kết {k.DiemTongKet.ToString("0.0", Vi)} – {k.XepLoai} – {k.KetQua1}."));

        if (moi.Count == 0) return 0;
        var maHV = moi.Select(m => m.MaHV).Distinct().ToList();
        var ngaySinh = await db.HocVien.Where(h => maHV.Contains(h.MaHV)).ToDictionaryAsync(h => h.MaHV, h => h.NgaySinh);
        var ph = (await db.HocVienPhuHuynh.Where(x => maHV.Contains(x.MaHV)).ToListAsync()).ToLookup(x => x.MaHV, x => x.MaPH);
        var daGui = (await db.ThongBao.Where(t => maHV.Contains(t.MaHV)).Select(t => new { t.Loai, t.MaHV, t.MaPH, t.NoiDung }).ToListAsync())
                    .Select(t => (t.Loai, t.MaHV, t.MaPH, t.NoiDung)).ToHashSet();
        int so = 0;
        foreach (var (loai, hv, nd) in moi.Distinct())
            foreach (var nhan in QuyTac.NguoiNhan(ngaySinh[hv], ph[hv], homNay))
                if (daGui.Add((loai, hv, nhan, nd)))
                {
                    db.ThongBao.Add(new ThongBao { Loai = loai, MaHV = hv, MaPH = nhan, NoiDung = nd, ThoiGianGui = bay, TrangThaiGui = "Đã gửi" });
                    so++;
                }
        await db.SaveChangesAsync();
        return so;
    }
}

/// <summary>Chạy lô 5.2 lúc khởi động máy chủ rồi mỗi 24 giờ.</summary>
// ponytail: hẹn giờ trong tiến trình web; cần đúng "đầu ngày" khi máy chủ chạy liên tục thì dùng Task Scheduler gọi lô này
public class LoThongBao(IServiceScopeFactory scope, DongHo dongHo, ILogger<LoThongBao> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken dung)
    {
        while (!dung.IsCancellationRequested)
        {
            try
            {
                using var s = scope.CreateScope();
                var so = await ThongBaoTuDong.ChayAsync(s.ServiceProvider.GetRequiredService<TrungTamContext>(), dongHo.Bay);
                log.LogInformation("5.2: đã tạo {So} thông báo mới", so);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "5.2: lô thông báo lỗi, sẽ chạy lại lần sau");
            }
            await Task.Delay(TimeSpan.FromDays(1), dung);
        }
    }
}
