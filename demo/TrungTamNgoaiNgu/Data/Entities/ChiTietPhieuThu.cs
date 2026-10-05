using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class ChiTietPhieuThu
{
    public string SoPT { get; set; } = null!;

    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public byte Dot { get; set; }

    public decimal SoTien { get; set; }

    public virtual DotHocPhi DotHocPhi { get; set; } = null!;

    public virtual PhieuThu SoPTNavigation { get; set; } = null!;
}
