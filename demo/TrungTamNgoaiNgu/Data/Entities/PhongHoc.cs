using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class PhongHoc
{
    public string MaPhong { get; set; } = null!;

    public string TenPhong { get; set; } = null!;

    public short SucChua { get; set; }

    public string? ThietBi { get; set; }

    public string TinhTrang { get; set; } = null!;

    public virtual ICollection<BuoiHoc> BuoiHoc { get; set; } = new List<BuoiHoc>();

    public virtual ICollection<LichTuan> LichTuan { get; set; } = new List<LichTuan>();
}
