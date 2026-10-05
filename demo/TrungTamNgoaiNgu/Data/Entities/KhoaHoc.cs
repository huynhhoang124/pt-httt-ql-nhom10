using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class KhoaHoc
{
    public string MaKH { get; set; } = null!;

    public string TenKH { get; set; } = null!;

    public string NgonNgu { get; set; } = null!;

    public string TrinhDoDauVao { get; set; } = null!;

    public string TrinhDoDauRa { get; set; } = null!;

    public short SoBuoi { get; set; }

    public short ThoiLuongBuoi { get; set; }

    public decimal HocPhi { get; set; }

    public byte TSChuyenCan { get; set; }

    public byte TSGiuaKy { get; set; }

    public byte TSCuoiKy { get; set; }

    public string? MoTa { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<LopHoc> LopHoc { get; set; } = new List<LopHoc>();
}
