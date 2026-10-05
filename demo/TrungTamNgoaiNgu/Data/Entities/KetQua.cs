using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class KetQua
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public decimal DiemTongKet { get; set; }

    public string XepLoai { get; set; } = null!;

    public string KetQua1 { get; set; } = null!;

    public string? MaNVDuyet { get; set; }

    public DateOnly? NgayDuyet { get; set; }

    public virtual ChungNhan? ChungNhan { get; set; }

    public virtual DangKy DangKy { get; set; } = null!;

    public virtual NhanVien? MaNVDuyetNavigation { get; set; }
}
