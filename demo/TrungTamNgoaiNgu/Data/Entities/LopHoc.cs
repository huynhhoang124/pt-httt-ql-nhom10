using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class LopHoc
{
    public string MaLop { get; set; } = null!;

    public string MaKH { get; set; } = null!;

    public string MaGV { get; set; } = null!;

    public DateOnly NgayKhaiGiang { get; set; }

    public short SiSoToiDa { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<BuoiHoc> BuoiHoc { get; set; } = new List<BuoiHoc>();

    public virtual ICollection<DangKy> DangKy { get; set; } = new List<DangKy>();

    public virtual ICollection<LichTuan> LichTuan { get; set; } = new List<LichTuan>();

    public virtual GiaoVien MaGVNavigation { get; set; } = null!;

    public virtual KhoaHoc MaKHNavigation { get; set; } = null!;
}
