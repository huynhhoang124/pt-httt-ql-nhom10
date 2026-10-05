using TrungTamNgoaiNgu.Nen;
using TrungTamNgoaiNgu.NghiepVu;
using static TrungTamNgoaiNgu.NghiepVu.QuyTac;

namespace KiemThu;

/// <summary>Kiểm thử đơn vị các quy tắc nghiệp vụ không phụ thuộc CSDL (05_module mục 8, bước 5).
/// Mỗi quy tắc của bảng quyết định là một test case (04_dac_ta mục 3.1, phan_tich/05_cai_dat.md mục kiểm thử).</summary>
public class KiemThuQuyTac
{
    static readonly DateOnly NgayDK = new(2027, 1, 4);
    static readonly UuDaiCo[] UuDai =
    [
        new("UD-HVCU", "Học viên cũ", 10, new(2026, 1, 1), new(2027, 12, 31)),
        new("UD-NHOM", "Đăng ký nhóm", 5, new(2026, 1, 1), new(2027, 12, 31)),
        new("UD-1LAN", "Đóng một lần", 5, new(2026, 1, 1), new(2027, 12, 31)),
    ];

    // ---------------- 3.1 – Bảng quyết định tính học phí R1–R8 (QT01–QT05) ----------------

    [Theory]
    [InlineData("R1", true, true, true, 15, 1)]     // cộng dồn 20%, trần 15% (QT04)
    [InlineData("R2", true, true, false, 15, 2)]
    [InlineData("R3", true, false, true, 15, 1)]
    [InlineData("R4", true, false, false, 10, 2)]
    [InlineData("R5", false, true, true, 10, 1)]
    [InlineData("R6", false, true, false, 5, 2)]
    [InlineData("R7", false, false, true, 5, 1)]
    [InlineData("R8", false, false, false, 0, 2)]
    public void BangQuyetDinh_HocPhi(string r, bool hvCu, bool nhom, bool motLan, decimal giam, byte soDot)
    {
        var (tyLe, _) = XetUuDai(hvCu, nhom, motLan, UuDai, NgayDK, 15);
        var dot = ChiaDot(2_400_000, tyLe, motLan ? (byte)1 : (byte)2, 50, new(2027, 1, 9), new(2027, 2, 6));
        Assert.True(giam == tyLe, $"{r}: giảm {tyLe}%, mong đợi {giam}%");
        Assert.Equal(soDot, dot.Count);
        Assert.Equal(2_400_000 * (100 - giam) / 100, dot.Sum(d => d.SoTien));
    }

    [Fact]
    public void UuDai_HetHan_KhongApDung()  // lỗi tìm ra khi thử POST: ưu đãi hết hạn 31/12/2026 trước ngày demo
    {
        UuDaiCo[] cu = [new("UD-HVCU", "Học viên cũ", 10, new(2026, 1, 1), new(2026, 12, 31))];
        Assert.Equal(0, XetUuDai(true, false, false, cu, NgayDK, 15).TyLe);
        Assert.Equal(10, XetUuDai(true, false, false, cu, new(2026, 12, 31), 15).TyLe);
    }

    [Fact]
    public void UuDai_CungLoai_LayMucCaoNhat()
    {
        UuDaiCo[] u = [.. UuDai, new("UD-HVCU2", "Học viên cũ", 12, new(2027, 1, 1), new(2027, 1, 31))];
        var (tyLe, ma) = XetUuDai(true, false, false, u, NgayDK, 15);
        Assert.Equal(12, tyLe);
        Assert.Equal(["UD-HVCU2"], ma);
    }

    [Fact]
    public void ChungTuMau_DK2026_0158()  // 04_dac_ta mục 3.1: HV cũ, một mình, 2 đợt → R4, phải nộp 6.480.000
    {
        var (tyLe, _) = XetUuDai(true, false, false, UuDai, new(2026, 6, 1), 15);
        Assert.Equal(6_480_000, ChiaDot(7_200_000, tyLe, 2, 50, new(2026, 6, 8), new(2026, 7, 6)).Sum(d => d.SoTien));
    }

    [Fact]
    public void ChiaDot_LamTronNghin_TongKhongDoi()
    {
        var dot = ChiaDot(2_030_000, 15, 2, 50, new(2027, 1, 9), new(2027, 2, 6));  // phải nộp 1.725.500
        Assert.Equal(863_000, dot[0].SoTien);
        Assert.Equal(862_500, dot[1].SoTien);
        Assert.Equal(new DateOnly(2027, 1, 9), dot[0].Han);
        Assert.Equal(new DateOnly(2027, 2, 6), dot[1].Han);
    }

    [Theory]
    [InlineData(1, "2027-01-04")]
    [InlineData(2, "2027-01-06")]
    [InlineData(3, "2027-01-11")]
    public void NgayBuoiThu_LichT2T4(int n, string mong)  // 04/01/2027 là thứ Hai
        => Assert.Equal(DateOnly.Parse(mong), NgayBuoiThu(n, new(2027, 1, 4), [2, 4]));

    [Fact]
    public void NgayBuoiThu_ChuNhat_Va_ThamSoSai()
    {
        Assert.Equal(new DateOnly(2027, 1, 10), NgayBuoiThu(1, new(2027, 1, 4), [8]));
        Assert.Throws<ArgumentException>(() => NgayBuoiThu(0, new(2027, 1, 4), [2]));
        Assert.Throws<ArgumentException>(() => NgayBuoiThu(1, new(2027, 1, 4), []));
    }

    // ---------------- 3.2 – Phiếu thu (QT05) ----------------

    static DongThu Dong(decimal tien, byte dot = 1, decimal daThu = 0, decimal conNo = 1_080_000) =>
        new("HV0394", "GT-2701", dot, 2_160_000, daThu, conNo, tien);

    [Theory]
    [InlineData(500_000, "QT05")]                     // đợt 1 dưới 50%
    [InlineData(1_500_000, "QT05#2")]                 // vượt số còn nợ của đợt
    [InlineData(0, "CK_ChiTietPhieuThu_SoTien")]
    [InlineData(-1, "CK_ChiTietPhieuThu_SoTien")]
    public void PhieuThu_Sai(decimal tien, string khoa)
        => Assert.Equal(khoa, Assert.Throws<LoiNghiepVu>(() => KiemTraPhieuThu([Dong(tien)], 50)).Khoa);

    [Fact]
    public void PhieuThu_Dung()
    {
        KiemTraPhieuThu([Dong(1_080_000)], 50);                                   // đủ 50%
        KiemTraPhieuThu([Dong(600_000), Dong(480_000, dot: 2)], 50);              // đợt 1 thiếu nhưng cùng phiếu nộp đợt 2 bù đủ
        KiemTraPhieuThu([Dong(100_000, daThu: 1_000_000, conNo: 100_000)], 50);  // đã thu trước đó, nộp nốt
        Assert.Equal("CK_ChiTietPhieuThu_SoTien", Assert.Throws<LoiNghiepVu>(() => KiemTraPhieuThu([], 50)).Khoa);
    }

    // ---------------- 4.1 – Điểm danh (QT11): từ lúc bắt đầu đến 24 giờ sau khi kết thúc ----------------

    [Theory]
    [InlineData("2027-01-04 19:30", true)]
    [InlineData("2027-01-04 19:00", true)]
    [InlineData("2027-01-04 18:59", false)]   // chưa đến giờ học
    [InlineData("2027-01-05 21:00", true)]
    [InlineData("2027-01-05 21:01", false)]   // quá 24 giờ
    public void DiemDanh_QT11(string bay, bool duoc)
        => Assert.Equal(duoc, DuocDiemDanh(new(2027, 1, 4), new(19, 0), new(21, 0), DateTime.Parse(bay), 24));

    // ---------------- R3.2 – Số tiền bằng chữ ----------------

    [Theory]
    [InlineData(0, "Không đồng")]
    [InlineData(15, "Mười lăm đồng")]
    [InlineData(21, "Hai mươi mốt đồng")]
    [InlineData(105, "Một trăm linh năm đồng")]
    [InlineData(1_080_000, "Một triệu không trăm tám mươi nghìn đồng")]   // PT2026-0731, PT2027-0001
    [InlineData(2_040_000, "Hai triệu không trăm bốn mươi nghìn đồng")]
    [InlineData(5_400_000, "Năm triệu bốn trăm nghìn đồng")]
    [InlineData(1_000_005, "Một triệu không trăm linh năm đồng")]
    [InlineData(1_000_000_000, "Một tỷ đồng")]
    public void BangChu(long tien, string mong) => Assert.Equal(mong, HienThi.BangChu(tien));

    // ---------------- N1 – Mật khẩu, khóa tạm ----------------

    [Fact]
    public void MatKhau_Pbkdf2()
    {
        var a = MatKhau.Bam("bi-mat");
        Assert.StartsWith("pbkdf2$", a);
        Assert.NotEqual(a, MatKhau.Bam("bi-mat"));  // muối ngẫu nhiên
        Assert.True(MatKhau.Dung("bi-mat", a));
        Assert.False(MatKhau.Dung("Bi-mat", a));
    }

    [Fact]
    public void MatKhau_DangSha256CuaSeed()  // csdl/sinh_du_lieu.py: sha256("nhom10:" + mật khẩu)
    {
        var luu = "sha256$" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData("nhom10:nv004"u8.ToArray()));
        Assert.True(MatKhau.Dung("nv004", luu));
        Assert.False(MatKhau.Dung("nv005", luu));
        Assert.False(MatKhau.Dung("nv004", "khong-hop-le"));
    }

    [Fact]
    public void KhoaDangNhap_5LanSai_Khoa15Phut()
    {
        var k = new KhoaDangNhap();
        var t = new DateTime(2027, 1, 4, 19, 30, 0);
        for (int i = 0; i < 4; i++) k.Sai("nv004", t);
        Assert.False(k.DangKhoa("nv004", t));
        k.Sai("nv004", t);
        Assert.True(k.DangKhoa("nv004", t.AddMinutes(14)));
        Assert.False(k.DangKhoa("nv004", t.AddMinutes(15)));
        Assert.False(k.DangKhoa("nv005", t));
        k.Dung("nv004");
        Assert.False(k.DangKhoa("nv004", t));
    }
}
