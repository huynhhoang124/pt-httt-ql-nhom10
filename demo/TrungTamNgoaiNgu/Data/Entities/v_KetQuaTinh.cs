using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_KetQuaTinh
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public decimal? DiemChuyenCan { get; set; }

    public decimal DiemGiuaKy { get; set; }

    public decimal DiemCuoiKy { get; set; }

    public decimal? DiemTongKet { get; set; }

    public string XepLoai { get; set; } = null!;

    public string KetQua { get; set; } = null!;
}
