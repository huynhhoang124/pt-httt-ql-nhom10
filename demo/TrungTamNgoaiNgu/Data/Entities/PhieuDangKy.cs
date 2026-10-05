using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class PhieuDangKy
{
    public string SoPhieuDK { get; set; } = null!;

    public DateOnly NgayDK { get; set; }

    public string MaNV { get; set; } = null!;

    public virtual ICollection<DangKy> DangKy { get; set; } = new List<DangKy>();

    public virtual NhanVien MaNVNavigation { get; set; } = null!;
}
