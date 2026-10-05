using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_HocVienCu
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public DateOnly? NgayKetThuc { get; set; }
}
