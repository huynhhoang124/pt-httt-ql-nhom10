using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class v_ChuyenCan
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public int? SoBuoiDaHoc { get; set; }

    public int? SoBuoiCoMat { get; set; }

    public decimal? TyLeChuyenCan { get; set; }

    public int CanhBaoVang { get; set; }

    public int NguyCoKhongDat { get; set; }
}
