using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class PhieuThu
{
    public string SoPT { get; set; } = null!;

    public DateOnly NgayThu { get; set; }

    public string NguoiNop { get; set; } = null!;

    public string HinhThuc { get; set; } = null!;

    public string MaNV { get; set; } = null!;

    public virtual ICollection<ChiTietPhieuThu> ChiTietPhieuThu { get; set; } = new List<ChiTietPhieuThu>();

    public virtual NhanVien MaNVNavigation { get; set; } = null!;
}
