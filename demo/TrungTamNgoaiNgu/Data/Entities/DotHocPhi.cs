using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class DotHocPhi
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public byte Dot { get; set; }

    public decimal SoTien { get; set; }

    public DateOnly HanDong { get; set; }

    public virtual ICollection<ChiTietPhieuThu> ChiTietPhieuThu { get; set; } = new List<ChiTietPhieuThu>();

    public virtual HocPhi HocPhi { get; set; } = null!;
}
