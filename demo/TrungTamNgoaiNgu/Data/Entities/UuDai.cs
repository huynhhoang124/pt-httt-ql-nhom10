using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class UuDai
{
    public string MaUD { get; set; } = null!;

    public string TenUD { get; set; } = null!;

    public string LoaiUD { get; set; } = null!;

    public decimal TyLeGiam { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public string? DieuKien { get; set; }

    public virtual ICollection<HocPhi> HocPhi { get; set; } = new List<HocPhi>();
}
