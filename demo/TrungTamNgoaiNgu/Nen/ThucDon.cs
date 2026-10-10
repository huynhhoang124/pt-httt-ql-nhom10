using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>Thực đơn phân cấp (06_giao_dien mục 5.1): phân hệ → module → màn hình. Tên module, màn hình lấy từ ThucDon.g.cs.</summary>
public static partial class ThucDon
{
    /// <summary>Tên phân hệ = tên chức năng cấp 1 trên BFD; các module nền (N) hiện thành mục "Cá nhân" (06_giao_dien mục 5.1).</summary>
    public static readonly Dictionary<string, string> PhanHe = new()
    {
        ["1.0"] = "Quản lý danh mục và hồ sơ", ["2.0"] = "Quản lý lớp học và lịch học", ["3.0"] = "Quản lý học phí",
        ["4.0"] = "Quản lý học tập", ["5.0"] = "Quản lý hệ thống và báo cáo", ["N"] = "Cá nhân, tiện ích",
    };

    /// <summary>Tên phân hệ để hiện: mục nền không ghi mã "N".</summary>
    public static string TenPhanHe(string ma) => ma == "N" ? PhanHe[ma] : $"{ma} {PhanHe[ma]}";

    /// <summary>Các màn hình đã làm trong bản demo (05_module mục 8): mã màn hình → địa chỉ trang, quyền cần để thấy mục.</summary>
    public static readonly (string Ma, string Url, string Can)[] DaLam =
    {
        ("F1.1", "/DanhMuc/HocVien", ""),
        ("F1.2", "/DanhMuc/GiaoVien", "X"),
        ("F1.3", "/DanhMuc/KhoaHoc", ""),
        ("F1.4", "/DanhMuc/PhongHoc", ""),
        ("F2.1", "/LopHoc/MoLop", "X"),
        ("F2.2", "/LopHoc/DangKy", "T"),
        ("R2.2", "/LopHoc/KetQuaDangKy", "X"),
        ("F2.3", "/LopHoc/XepLich", "T"),
        ("F2.4", "/LopHoc/XuLyDon", "T"),
        ("F3.1", "/HocPhi/KhoanPhaiThu", "T"),
        ("F3.2", "/HocPhi/PhieuThu", "T"),
        ("R3.2", "/HocPhi/DanhSachPhieuThu", ""),
        ("R3.3", "/HocPhi/CongNo", ""),
        ("F4.1", "/HocTap/DiemDanh", "T"),
        ("F4.2", "/HocTap/NhapDiem", "T"),
        ("R4.3a", "/HocTap/TongKet", ""),
        ("R4.4", "/HocTap/ChungNhan", ""),
        ("R3.4", "/BaoCao/DoanhThu", "X"),
        ("R5.3b", "/BaoCao/TinhTrangLop", "X"),
        ("R5.4a", "/BaoCao/HocTap", ""),
        ("F5.1a", "/HeThong/TaiKhoan", "S"),
        ("F5.1b", "/HeThong/ThamSo", "S"),
        ("R5.1", "/HeThong/NhatKy", ""),
        ("R5.2", "/HeThong/ThongBao", ""),
        ("FN1", "/DoiMatKhau", ""),
        ("FN4", "/HeThong/SaoLuu", "T"),
    };

    public static string Ten(string maManHinh) => $"{maManHinh} {ManHinh[maManHinh].Ten}";

    public record MucManHinh(string Ma, string Ten, string Url);
    public record MucModule(string Ma, string Ten, List<MucManHinh> ManHinh);
    public record MucPhanHe(string Ma, string Ten, List<MucModule> Module);

    /// <summary>Thực đơn của một vai trò (trang chủ và thanh bên dùng chung): chỉ module có ít nhất một màn hình vai trò này
    /// dùng được; học viên, phụ huynh: "Thông báo của tôi" (R5.2) nằm ở mục Cá nhân (06_giao_dien mục 5.1).</summary>
    public static List<MucPhanHe> ChoVaiTro(string? vt)
    {
        var caNhan = PhanQuyen.ChiCuaMinh(vt, "5.2");
        return Module.Where(m => PhanQuyen.Co(vt, m.Ma))
            .Select(m => (m, ds: DaLam.Where(d => ManHinh[d.Ma].Module == m.Ma && PhanQuyen.Co(vt, m.Ma, d.Can))
                                     .Select(d => new MucManHinh(d.Ma, ManHinh[d.Ma].Ten, d.Url)).ToList()))
            .Where(x => x.ds.Count > 0)
            .GroupBy(x => caNhan && x.m.Ma == "5.2" ? "N" : x.m.PhanHe)
            .OrderBy(g => g.Key == "N" ? "Z" : g.Key)
            .Select(g => new MucPhanHe(g.Key, TenPhanHe(g.Key), g.Select(x => new MucModule(x.m.Ma, x.m.Ten, x.ds)).ToList()))
            .ToList();
    }

    /// <summary>Tên đầy đủ của vai trò để hiện (mã cột ma trận → tên trong CSDL).</summary>
    public static string TenVaiTro(string? vt) => PhanQuyen.TuVaiTroCsdl.FirstOrDefault(p => p.Value == vt).Key ?? vt ?? "";
}

/// <summary>N2 – Lọc dữ liệu theo phạm vi cho quyền "x": HV của mình, PH các con, GV học viên các lớp mình dạy.</summary>
public static class PhamVi
{
    /// <summary>Mã học viên được xem; null = xem toàn bộ (quyền X).</summary>
    public static IQueryable<string>? HocVien(TrungTamContext db, ClaimsPrincipal u, string module)
    {
        if (!PhanQuyen.ChiCuaMinh(u.VaiTro(), module)) return null;
        string? hv = u.MaHV(), ph = u.MaPH();
        var lop = LopCuaGiaoVien(db, u.MaGV());
        return u.VaiTro() switch
        {
            "HV" => db.HocVien.Where(h => h.MaHV == hv).Select(h => h.MaHV),
            "PH" => db.HocVienPhuHuynh.Where(x => x.MaPH == ph).Select(x => x.MaHV),
            "GV" => db.DangKy.Where(d => lop.Contains(d.MaLop)).Select(d => d.MaHV),
            _ => db.HocVien.Where(_ => false).Select(h => h.MaHV),
        };
    }

    /// <summary>Lớp giáo viên phụ trách hoặc có dạy ít nhất một buổi (dạy thay).</summary>
    public static IQueryable<string> LopCuaGiaoVien(TrungTamContext db, string? maGV) =>
        db.LopHoc.Where(l => l.MaGV == maGV || l.BuoiHoc.Any(b => b.MaGV == maGV)).Select(l => l.MaLop);
}
