using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_DoanhThu
{
    public string SoPT { get; set; } = null!;

    public DateOnly NgayThu { get; set; }

    public int? Nam { get; set; }

    public int? Thang { get; set; }

    public string HinhThuc { get; set; } = null!;

    public string MaKH { get; set; } = null!;

    public string TenKH { get; set; } = null!;

    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public byte Dot { get; set; }

    public decimal SoTien { get; set; }
}
