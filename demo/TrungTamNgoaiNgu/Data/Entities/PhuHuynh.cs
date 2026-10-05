using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class PhuHuynh
{
    public string MaPH { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string DienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public virtual ICollection<HocVienPhuHuynh> HocVienPhuHuynh { get; set; } = new List<HocVienPhuHuynh>();

    public virtual TaiKhoan? TaiKhoan { get; set; }

    public virtual ICollection<ThongBao> ThongBao { get; set; } = new List<ThongBao>();
}
