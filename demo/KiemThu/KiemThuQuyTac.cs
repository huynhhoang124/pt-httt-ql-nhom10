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

    // ---------------- 4.3 – Bảng quyết định xếp loại R1–R5 (QT13–QT15), ca TC22 ----------------

    static readonly NguongXepLoai Nguong = new(8.5m, 7.0m, 5.0m, 70);

    [Theory]
    [InlineData("R1", 20, 18, 9.0, 9.0, 9.0, "Giỏi", "Đạt")]
    [InlineData("R2", 20, 16, 7.0, 7.5, 8.0, "Khá", "Đạt")]
    [InlineData("R3", 20, 14, 5.5, 5.0, 6.0, "Trung bình", "Đạt")]          // chuyên cần đúng 70% vẫn đạt
    [InlineData("R4", 20, 13, 9.0, 9.0, 9.0, "Giỏi", "Không đạt")]          // 65% < 70%: giữ xếp loại theo điểm
    [InlineData("R5", 20, 20, 4.0, 4.0, 4.5, "Không đạt", "Không đạt")]
    public void BangQuyetDinh_XepLoai(string r, int daHoc, int coMat, double cc, double gk, double ck, string xepLoai, string ketQua)
    {
        _ = cc;  // điểm chuyên cần = tỷ lệ × 10, ghi trong ca để đọc cho dễ
        var k = TongKet(daHoc, coMat, (decimal)gk, (decimal)ck, 10, 30, 60, Nguong);
        Assert.True(xepLoai == k.XepLoai && ketQua == k.KetQua, $"{r}: {k.XepLoai}/{k.KetQua} (TK {k.DiemTongKet})");
    }

    [Fact]
    public void ChungTuMau_BangDiemIEK2609()  // 04_dac_ta mục 1.7: HV0412 7,95 → 8,0 Khá; HV0398 4,5 Không đạt
    {
        var a = TongKet(10, 9, 7.5m, 8.0m, 10, 30, 60, Nguong);
        Assert.Equal((8.0m, "Khá", "Đạt"), (a.DiemTongKet, a.XepLoai, a.KetQua));
        var b = TongKet(10, 6, 5.0m, 4.0m, 10, 30, 60, Nguong);
        Assert.Equal((4.5m, "Không đạt", "Không đạt"), (b.DiemTongKet, b.XepLoai, b.KetQua));
        Assert.Equal(0, TongKet(0, 0, 10, 10, 10, 30, 60, Nguong).DiemChuyenCan);  // chưa có buổi nào: không chia cho 0
    }

    [Fact]
    public void TongKet_LamTronTruocKhiXepLoai()
    {
        var k = TongKet(20, 19, 7.0m, 9.0m, 10, 30, 60, Nguong);   // 0,95 + 2,1 + 5,4 = 8,45 → 8,5
        Assert.Equal((8.5m, "Giỏi"), (k.DiemTongKet, k.XepLoai));
        k = TongKet(20, 19, 7.0m, 8.9m, 10, 30, 60, Nguong);       // 8,39 → 8,4
        Assert.Equal((8.4m, "Khá"), (k.DiemTongKet, k.XepLoai));
    }

    [Theory]  // 4.4 – QT15, ca TC23
    [InlineData("Đạt", true, true)]
    [InlineData("Không đạt", true, false)]
    [InlineData("Đạt", false, false)]      // chưa duyệt
    [InlineData(null, false, false)]       // chưa tổng kết
    public void CapChungNhan(string? ketQua, bool daDuyet, bool mong) =>
        Assert.Equal(mong, DuocCapChungNhan(ketQua, daDuyet ? new DateOnly(2027, 1, 4) : null));

    // ---------------- 2.3 – Trùng lịch (QT07), ca TC06 ----------------

    static readonly Ca BuoiTOE = new("TOE-2611", 20, new(2027, 1, 6), new(19, 0), new(21, 0), "P305", "GV023");

    [Theory]
    [InlineData("19:30", "21:00", "P101", "GV023", true)]   // cùng giáo viên, chồng giờ
    [InlineData("18:00", "19:30", "P305", "GV011", true)]   // cùng phòng, chồng giờ
    [InlineData("17:00", "19:00", "P305", "GV023", false)]  // nối tiếp đúng 19:00: không trùng
    [InlineData("19:00", "21:00", "P101", "GV011", false)]  // cùng giờ nhưng khác giáo viên, khác phòng
    public void TrungLich_QT07(string tu, string den, string phong, string gv, bool trung)
    {
        var b = new Ca("GT-2701", 11, new(2027, 1, 6), TimeOnly.Parse(tu), TimeOnly.Parse(den), phong, gv);
        Assert.Equal(trung, TrungLich(b, [BuoiTOE]) != null);
        Assert.Null(TrungLich(b with { Ngay = new(2027, 1, 7) }, [BuoiTOE]));       // khác ngày
        Assert.Null(TrungLich(BuoiTOE with { MaPhong = "P101" }, [BuoiTOE]));       // đổi lịch: không so với chính nó
    }

    [Fact]
    public void SinhBuoi_TheoLichTuan()  // GT-2701: thứ 7 08:00, 10 buổi × 120 phút, khai giảng 09/01/2027 (thứ 7)
    {
        var b = SinhBuoi("GT-2701", new(2027, 1, 9), [(7, new(8, 0), "P101")], 10, 120, "GV011");
        Assert.Equal(10, b.Count);
        Assert.Equal((new DateOnly(2027, 1, 9), new TimeOnly(10, 0)), (b[0].Ngay, b[0].KetThuc));
        Assert.Equal(new DateOnly(2027, 3, 13), b[^1].Ngay);
        var hai = SinhBuoi("TOE", new(2027, 1, 5), [(2, new(19, 0), "P305"), (4, new(19, 0), "P305")], 3, 120, "GV023");
        Assert.Equal([new(2027, 1, 6), new(2027, 1, 11), new DateOnly(2027, 1, 13)], hai.Select(x => x.Ngay));  // khai giảng thứ 3
    }

    // ---------------- 2.4 – Cây quyết định chuyển lớp, bảo lưu (QT08, QT09), ca TC07 ----------------

    [Theory]  // mỗi lá của nhánh "Chuyển lớp"
    [InlineData(4, true, true, "QT08")]
    [InlineData(3, false, true, "QT08#2")]
    [InlineData(3, true, false, "QT08#3")]
    [InlineData(4, false, false, "QT08")]     // xét số buổi trước
    [InlineData(0, true, true, null)]
    public void CayQuyetDinh_ChuyenLop(int daHoc, bool cungKhoa, bool conCho, string? loi) =>
        Assert.Equal(loi, Record.Exception(() => XetChuyenLop(daHoc, 3, cungKhoa, conCho)) is LoiNghiepVu l ? l.Khoa : null);

    [Theory]  // mỗi lá của nhánh "Bảo lưu"; khóa 24 buổi, ngưỡng 50%
    [InlineData(1_000, 0, false, "QT09")]
    [InlineData(0, 12, false, "QT09#2")]      // đúng 50% đã bị từ chối
    [InlineData(0, 11, true, "QT09#3")]
    [InlineData(0, 11, false, null)]
    public void CayQuyetDinh_BaoLuu(int conNo, int daHoc, bool daBaoLuu, string? loi) =>
        Assert.Equal(loi, Record.Exception(() => XetBaoLuu(conNo, daHoc, 24, 50, daBaoLuu)) is LoiNghiepVu l ? l.Khoa : null);

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
