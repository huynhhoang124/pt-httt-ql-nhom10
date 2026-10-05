using System;
using System.Collections.Generic;

namespace TrungTamNgoaiNgu.Data.Entities;

public partial class DangKy
{
    public string MaHV { get; set; } = null!;

    public string MaLop { get; set; } = null!;

    public string SoPhieuDK { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public DateOnly? NgayThayDoi { get; set; }

    public string? LyDo { get; set; }

    public DateOnly? HanBaoLuu { get; set; }

    public string? MaLopGoc { get; set; }

    public virtual DangKy? DangKyNavigation { get; set; }

    public virtual ICollection<Diem> Diem { get; set; } = new List<Diem>();

    public virtual ICollection<DiemDanh> DiemDanh { get; set; } = new List<DiemDanh>();

    public virtual HocPhi? HocPhi { get; set; }

    public virtual ICollection<DangKy> InverseDangKyNavigation { get; set; } = new List<DangKy>();

    public virtual KetQua? KetQua { get; set; }

    public virtual HocVien MaHVNavigation { get; set; } = null!;

    public virtual LopHoc MaLopNavigation { get; set; } = null!;

    public virtual PhieuDangKy SoPhieuDKNavigation { get; set; } = null!;
}
