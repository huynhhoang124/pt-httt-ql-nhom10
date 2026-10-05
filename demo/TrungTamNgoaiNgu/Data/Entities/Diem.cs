using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class Diem
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public string LoaiDiem { get; set; } = null!;

    public decimal Diem1 { get; set; }

    public string? NhanXet { get; set; }

    public virtual DangKy DangKy { get; set; } = null!;
}
