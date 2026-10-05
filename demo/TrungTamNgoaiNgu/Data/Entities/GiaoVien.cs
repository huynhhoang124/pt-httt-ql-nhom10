using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class GiaoVien
{
    public string MaGV { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateOnly NgaySinh { get; set; }

    public string DienThoai { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string NgonNguDay { get; set; } = null!;

    public string? ChuyenMon { get; set; }

    public string? BangCap { get; set; }

    public string TinhTrang { get; set; } = null!;

    public virtual ICollection<BuoiHoc> BuoiHoc { get; set; } = new List<BuoiHoc>();

    public virtual ICollection<LopHoc> LopHoc { get; set; } = new List<LopHoc>();

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
