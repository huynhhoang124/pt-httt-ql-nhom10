using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_GiangDay
{
    public string MaGV { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public int? Nam { get; set; }

    public int? Thang { get; set; }

    public int? SoBuoiDaDay { get; set; }
}
