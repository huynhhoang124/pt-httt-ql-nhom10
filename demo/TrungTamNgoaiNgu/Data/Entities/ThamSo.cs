using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class ThamSo
{
    public string MaThamSo { get; set; } = null!;

    public string TenThamSo { get; set; } = null!;

    public decimal GiaTri { get; set; }

    public string DonVi { get; set; } = null!;

    public string? QuyTac { get; set; }
}
