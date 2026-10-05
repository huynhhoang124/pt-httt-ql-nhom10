using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class TaiKhoan
{
    public string TenDangNhap { get; set; } = null!;

    public string MatKhauBam { get; set; } = null!;

    public string VaiTro { get; set; } = null!;

    public string? MaNV { get; set; }

    public string? MaGV { get; set; }

    public string? MaHV { get; set; }

    public string? MaPH { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual GiaoVien? MaGVNavigation { get; set; }

    public virtual HocVien? MaHVNavigation { get; set; }

    public virtual NhanVien? MaNVNavigation { get; set; }

    public virtual PhuHuynh? MaPHNavigation { get; set; }

    public virtual ICollection<NhatKy> NhatKy { get; set; } = new List<NhatKy>();
}
