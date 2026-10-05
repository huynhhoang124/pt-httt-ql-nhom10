using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class ThongBao
{
    public int MaTB { get; set; }

    public string Loai { get; set; } = null!;

    public string MaHV { get; set; } = null!;

    public string? MaPH { get; set; }

    public string NoiDung { get; set; } = null!;

    public DateTime ThoiGianGui { get; set; }

    public string TrangThaiGui { get; set; } = null!;

    public virtual HocVienPhuHuynh? HocVienPhuHuynh { get; set; }

    public virtual HocVien MaHVNavigation { get; set; } = null!;

    public virtual PhuHuynh? MaPHNavigation { get; set; }
}
