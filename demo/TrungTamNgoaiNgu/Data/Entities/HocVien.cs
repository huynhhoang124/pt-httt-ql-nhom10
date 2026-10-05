using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class HocVien
{
    public string MaHV { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateOnly NgaySinh { get; set; }

    public string GioiTinh { get; set; } = null!;

    public string DienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public string? TrinhDoDauVao { get; set; }

    public DateOnly? NgayKiemTra { get; set; }

    public DateOnly NgayTiepNhan { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<DangKy> DangKy { get; set; } = new List<DangKy>();

    public virtual ICollection<HocVienPhuHuynh> HocVienPhuHuynh { get; set; } = new List<HocVienPhuHuynh>();

    public virtual TaiKhoan? TaiKhoan { get; set; }

    public virtual ICollection<ThongBao> ThongBao { get; set; } = new List<ThongBao>();
}
