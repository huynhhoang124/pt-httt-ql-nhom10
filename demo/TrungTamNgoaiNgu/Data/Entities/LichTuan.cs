using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class LichTuan
{
    public string MaLop { get; set; } = null!;

    public byte Thu { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public string MaPhong { get; set; } = null!;

    public virtual LopHoc MaLopNavigation { get; set; } = null!;

    public virtual PhongHoc MaPhongNavigation { get; set; } = null!;
}
