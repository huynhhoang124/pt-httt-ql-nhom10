using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class ChungNhan
{
    public string SoCN { get; set; } = null!;

    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public DateOnly NgayCap { get; set; }

    public virtual KetQua KetQua { get; set; } = null!;
}
