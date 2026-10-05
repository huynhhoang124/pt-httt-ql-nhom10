using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class BuoiHoc
{
    public string MaLop { get; set; } = null!;

    public short SoBuoi { get; set; }

    public DateOnly Ngay { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public string MaPhong { get; set; } = null!;

    public string MaGV { get; set; } = null!;

    public string Loai { get; set; } = null!;

    public string? NoiDung { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<DiemDanh> DiemDanh { get; set; } = new List<DiemDanh>();

    public virtual GiaoVien MaGVNavigation { get; set; } = null!;

    public virtual LopHoc MaLopNavigation { get; set; } = null!;

    public virtual PhongHoc MaPhongNavigation { get; set; } = null!;
}
