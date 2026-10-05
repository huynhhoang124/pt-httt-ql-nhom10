using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class NhanVien
{
    public string MaNV { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string BoPhan { get; set; } = null!;

    public string ChucVu { get; set; } = null!;

    public string DienThoai { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<KetQua> KetQua { get; set; } = new List<KetQua>();

    public virtual ICollection<PhieuDangKy> PhieuDangKy { get; set; } = new List<PhieuDangKy>();

    public virtual ICollection<PhieuThu> PhieuThu { get; set; } = new List<PhieuThu>();

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
