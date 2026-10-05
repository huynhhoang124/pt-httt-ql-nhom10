using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class NhatKy
{
    public long MaNK { get; set; }

    public DateTime ThoiDiem { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string ThaoTac { get; set; } = null!;

    public string DoiTuong { get; set; } = null!;

    public string MaDoiTuong { get; set; } = null!;

    public virtual TaiKhoan TenDangNhapNavigation { get; set; } = null!;
}
