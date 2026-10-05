using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class HocVienPhuHuynh
{
    public string MaHV { get; set; } = null!;

    public string MaPH { get; set; } = null!;

    public string QuanHe { get; set; } = null!;

    public virtual HocVien MaHVNavigation { get; set; } = null!;

    public virtual PhuHuynh MaPHNavigation { get; set; } = null!;

    public virtual ICollection<ThongBao> ThongBao { get; set; } = new List<ThongBao>();
}
