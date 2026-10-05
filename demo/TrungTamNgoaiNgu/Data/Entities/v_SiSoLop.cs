using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_SiSoLop
{
    public string MaLop { get; set; } = null!;

    public string MaKH { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public DateOnly NgayKhaiGiang { get; set; }

    public short SiSoToiDa { get; set; }

    public int? SoDangKy { get; set; }

    public decimal? TyLeLapDay { get; set; }
}
