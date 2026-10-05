using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class HocPhi
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public decimal HocPhiGoc { get; set; }

    public decimal TyLeGiam { get; set; }

    public byte SoDot { get; set; }

    public DateOnly NgayLap { get; set; }

    public virtual DangKy DangKy { get; set; } = null!;

    public virtual ICollection<DotHocPhi> DotHocPhi { get; set; } = new List<DotHocPhi>();

    public virtual ICollection<UuDai> MaUD { get; set; } = new List<UuDai>();
}
