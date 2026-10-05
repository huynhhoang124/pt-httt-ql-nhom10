using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>Thực đơn phân cấp (06_giao_dien mục 5.1): phân hệ → module → màn hình. Tên module, màn hình lấy từ ThucDon.g.cs.</summary>
public static partial class ThucDon
{
    /// <summary>Tên phân hệ = tên chức năng cấp 1 trên BFD.</summary>
    public static readonly Dictionary<string, string> PhanHe = new()
    {
        ["1.0"] = "Quản lý danh mục và hồ sơ", ["2.0"] = "Quản lý lớp học và lịch học", ["3.0"] = "Quản lý học phí",
        ["4.0"] = "Quản lý học tập", ["5.0"] = "Quản lý hệ thống và báo cáo", ["N"] = "Chức năng nền",
    };

    /// <summary>Các màn hình đã làm trong bản demo (05_module mục 8): mã màn hình → địa chỉ trang, quyền cần để thấy mục.</summary>
    public static readonly (string Ma, string Url, string Can)[] DaLam =
    {
        ("F1.1", "/DanhMuc/HocVien", ""),
        ("F2.2", "/LopHoc/DangKy", "T"),
        ("R2.2", "/LopHoc/KetQuaDangKy", "X"),
        ("F3.1", "/HocPhi/KhoanPhaiThu", "T"),
        ("F3.2", "/HocPhi/PhieuThu", "T"),
        ("R3.2", "/HocPhi/DanhSachPhieuThu", ""),
        ("F4.1", "/HocTap/DiemDanh", "T"),
        ("F4.2", "/HocTap/NhapDiem", "T"),
        ("R3.4", "/BaoCao/DoanhThu", "X"),
        ("R5.3b", "/BaoCao/TinhTrangLop", "X"),
        ("FN1", "/DoiMatKhau", ""),
    };

    public static string Ten(string maManHinh) => $"{maManHinh} {ManHinh[maManHinh].Ten}";
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
