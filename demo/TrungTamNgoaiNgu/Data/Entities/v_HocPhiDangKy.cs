using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_HocPhiDangKy
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public string? MaLopDau { get; set; }

    public decimal? PhaiNop { get; set; }

    public decimal DaThu { get; set; }

    public decimal? ConNo { get; set; }

    public int HieuLuc { get; set; }
}
