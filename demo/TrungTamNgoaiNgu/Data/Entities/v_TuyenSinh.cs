using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_TuyenSinh
{
    public string MaHV { get; set; } = null!;

    public DateOnly NgayDangKyDau { get; set; }

    public int? Nam { get; set; }

    public int? Thang { get; set; }

    public string MaKH { get; set; } = null!;
}
