using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class DiemDanh
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public short SoBuoi { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual BuoiHoc BuoiHoc { get; set; } = null!;

    public virtual DangKy DangKy { get; set; } = null!;
}
