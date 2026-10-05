using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data.Entities;

namespace TrungTamNgoaiNgu.Data;

public partial class TrungTamContext : DbContext
{
    public TrungTamContext(DbContextOptions<TrungTamContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BuoiHoc> BuoiHoc { get; set; }

    public virtual DbSet<ChiTietPhieuThu> ChiTietPhieuThu { get; set; }

    public virtual DbSet<ChungNhan> ChungNhan { get; set; }

    public virtual DbSet<DangKy> DangKy { get; set; }

    public virtual DbSet<Diem> Diem { get; set; }

    public virtual DbSet<DiemDanh> DiemDanh { get; set; }

    public virtual DbSet<DotHocPhi> DotHocPhi { get; set; }

    public virtual DbSet<GiaoVien> GiaoVien { get; set; }

    public virtual DbSet<HocPhi> HocPhi { get; set; }

    public virtual DbSet<HocVien> HocVien { get; set; }

    public virtual DbSet<HocVienPhuHuynh> HocVienPhuHuynh { get; set; }

    public virtual DbSet<KetQua> KetQua { get; set; }

    public virtual DbSet<KhoaHoc> KhoaHoc { get; set; }

    public virtual DbSet<LichTuan> LichTuan { get; set; }

    public virtual DbSet<LopHoc> LopHoc { get; set; }

    public virtual DbSet<NhanVien> NhanVien { get; set; }

    public virtual DbSet<NhatKy> NhatKy { get; set; }

    public virtual DbSet<PhieuDangKy> PhieuDangKy { get; set; }

    public virtual DbSet<PhieuThu> PhieuThu { get; set; }

    public virtual DbSet<PhongHoc> PhongHoc { get; set; }

    public virtual DbSet<PhuHuynh> PhuHuynh { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoan { get; set; }

    public virtual DbSet<ThamSo> ThamSo { get; set; }

    public virtual DbSet<ThongBao> ThongBao { get; set; }

    public virtual DbSet<UuDai> UuDai { get; set; }

    public virtual DbSet<v_ChuoiDangKy> v_ChuoiDangKy { get; set; }

    public virtual DbSet<v_ChuyenCan> v_ChuyenCan { get; set; }

    public virtual DbSet<v_DoanhThu> v_DoanhThu { get; set; }

    public virtual DbSet<v_GiangDay> v_GiangDay { get; set; }

    public virtual DbSet<v_HocPhiDangKy> v_HocPhiDangKy { get; set; }

    public virtual DbSet<v_HocVienCu> v_HocVienCu { get; set; }

    public virtual DbSet<v_KetQuaTinh> v_KetQuaTinh { get; set; }

    public virtual DbSet<v_SiSoLop> v_SiSoLop { get; set; }

    public virtual DbSet<v_TuyenSinh> v_TuyenSinh { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Vietnamese_100_CI_AS");

        modelBuilder.Entity<BuoiHoc>(entity =>
        {
            entity.HasKey(e => new { e.MaLop, e.SoBuoi });

            entity.HasIndex(e => new { e.MaGV, e.Ngay }, "IX_BuoiHoc_GiaoVien_Ngay");

            entity.HasIndex(e => new { e.MaPhong, e.Ngay }, "IX_BuoiHoc_Phong_Ngay");

            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.GioBatDau).HasPrecision(0);
            entity.Property(e => e.GioKetThuc).HasPrecision(0);
            entity.Property(e => e.Loai)
                .HasMaxLength(10)
                .HasDefaultValue("Thường");
            entity.Property(e => e.MaGV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.NoiDung).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Kế hoạch");

            entity.HasOne(d => d.MaGVNavigation).WithMany(p => p.BuoiHoc)
                .HasForeignKey(d => d.MaGV)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiHoc_GiaoVien");

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.BuoiHoc)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiHoc_LopHoc");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.BuoiHoc)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiHoc_PhongHoc");
        });

        modelBuilder.Entity<ChiTietPhieuThu>(entity =>
        {
            entity.HasKey(e => new { e.SoPT, e.MaHV, e.MaLop, e.Dot });

            entity.HasIndex(e => new { e.MaHV, e.MaLop, e.Dot }, "IX_ChiTietPhieuThu_Dot");

            entity.Property(e => e.SoPT)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(12, 0)");

            entity.HasOne(d => d.SoPTNavigation).WithMany(p => p.ChiTietPhieuThu)
                .HasForeignKey(d => d.SoPT)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietPhieuThu_PhieuThu");

            entity.HasOne(d => d.DotHocPhi).WithMany(p => p.ChiTietPhieuThu)
                .HasForeignKey(d => new { d.MaHV, d.MaLop, d.Dot })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChiTietPhieuThu_DotHocPhi");
        });

        modelBuilder.Entity<ChungNhan>(entity =>
        {
            entity.HasKey(e => e.SoCN);

            entity.HasIndex(e => new { e.MaHV, e.MaLop }, "UQ_ChungNhan_KetQua").IsUnique();

            entity.Property(e => e.SoCN)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.KetQua).WithOne(p => p.ChungNhan)
                .HasForeignKey<ChungNhan>(d => new { d.MaHV, d.MaLop })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChungNhan_KetQua");
        });

        modelBuilder.Entity<DangKy>(entity =>
        {
            entity.HasKey(e => new { e.MaHV, e.MaLop });

            entity.HasIndex(e => new { e.MaLop, e.TrangThai }, "IX_DangKy_MaLop");

            entity.HasIndex(e => new { e.SoPhieuDK, e.MaLop }, "UQ_DangKy_PhieuLop").IsUnique();

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LyDo).HasMaxLength(200);
            entity.Property(e => e.MaLopGoc)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SoPhieuDK)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Đã đăng ký");

            entity.HasOne(d => d.MaHVNavigation).WithMany(p => p.DangKy)
                .HasForeignKey(d => d.MaHV)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKy_HocVien");

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.DangKy)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKy_LopHoc");

            entity.HasOne(d => d.SoPhieuDKNavigation).WithMany(p => p.DangKy)
                .HasForeignKey(d => d.SoPhieuDK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKy_PhieuDangKy");

            entity.HasOne(d => d.DangKyNavigation).WithMany(p => p.InverseDangKyNavigation)
                .HasForeignKey(d => new { d.MaHV, d.MaLopGoc })
                .HasConstraintName("FK_DangKy_DangKyGoc");
        });

        modelBuilder.Entity<Diem>(entity =>
        {
            entity.HasKey(e => new { e.MaHV, e.MaLop, e.LoaiDiem });

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.LoaiDiem).HasMaxLength(10);
            entity.Property(e => e.Diem1)
                .HasColumnType("decimal(3, 1)")
                .HasColumnName("Diem");
            entity.Property(e => e.NhanXet).HasMaxLength(500);

            entity.HasOne(d => d.DangKy).WithMany(p => p.Diem)
                .HasForeignKey(d => new { d.MaHV, d.MaLop })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Diem_DangKy");
        });

        modelBuilder.Entity<DiemDanh>(entity =>
        {
            entity.HasKey(e => new { e.MaHV, e.MaLop, e.SoBuoi });

            entity.HasIndex(e => new { e.MaLop, e.SoBuoi }, "IX_DiemDanh_Buoi");

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.DangKy).WithMany(p => p.DiemDanh)
                .HasForeignKey(d => new { d.MaHV, d.MaLop })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_DangKy");

            entity.HasOne(d => d.BuoiHoc).WithMany(p => p.DiemDanh)
                .HasForeignKey(d => new { d.MaLop, d.SoBuoi })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiemDanh_BuoiHoc");
        });

        modelBuilder.Entity<DotHocPhi>(entity =>
        {
            entity.HasKey(e => new { e.MaHV, e.MaLop, e.Dot });

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(12, 0)");

            entity.HasOne(d => d.HocPhi).WithMany(p => p.DotHocPhi)
                .HasForeignKey(d => new { d.MaHV, d.MaLop })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DotHocPhi_HocPhi");
        });

        modelBuilder.Entity<GiaoVien>(entity =>
        {
            entity.HasKey(e => e.MaGV);

            entity.HasIndex(e => e.DienThoai, "UQ_GiaoVien_DienThoai").IsUnique();

            entity.HasIndex(e => e.Email, "UQ_GiaoVien_Email").IsUnique();

            entity.Property(e => e.MaGV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.BangCap).HasMaxLength(200);
            entity.Property(e => e.ChuyenMon).HasMaxLength(200);
            entity.Property(e => e.DienThoai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgonNguDay).HasMaxLength(10);
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(20)
                .HasDefaultValue("Đang dạy");
        });

        modelBuilder.Entity<HocPhi>(entity =>
        {
            entity.HasKey(e => new { e.MaHV, e.MaLop });

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.HocPhiGoc).HasColumnType("decimal(12, 0)");
            entity.Property(e => e.NgayLap).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.TyLeGiam).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.DangKy).WithOne(p => p.HocPhi)
                .HasForeignKey<HocPhi>(d => new { d.MaHV, d.MaLop })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocPhi_DangKy");

            entity.HasMany(d => d.MaUD).WithMany(p => p.HocPhi)
                .UsingEntity<Dictionary<string, object>>(
                    "ApDungUuDai",
                    r => r.HasOne<UuDai>().WithMany()
                        .HasForeignKey("MaUD")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ApDungUuDai_UuDai"),
                    l => l.HasOne<HocPhi>().WithMany()
                        .HasForeignKey("MaHV", "MaLop")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ApDungUuDai_HocPhi"),
                    j =>
                    {
                        j.HasKey("MaHV", "MaLop", "MaUD");
                        j.IndexerProperty<string>("MaHV")
                            .HasMaxLength(10)
                            .IsUnicode(false);
                        j.IndexerProperty<string>("MaLop")
                            .HasMaxLength(15)
                            .IsUnicode(false);
                        j.IndexerProperty<string>("MaUD")
                            .HasMaxLength(15)
                            .IsUnicode(false);
                    });
        });

        modelBuilder.Entity<HocVien>(entity =>
        {
            entity.HasKey(e => e.MaHV);

            entity.HasIndex(e => new { e.DienThoai, e.NgaySinh }, "IX_HocVien_DienThoai");

            entity.HasIndex(e => e.HoTen, "IX_HocVien_HoTen");

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.DienThoai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(3);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.NgayTiepNhan).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Hoạt động");
            entity.Property(e => e.TrinhDoDauVao)
                .HasMaxLength(2)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HocVienPhuHuynh>(entity =>
        {
            entity.HasKey(e => new { e.MaHV, e.MaPH });

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaPH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.QuanHe).HasMaxLength(20);

            entity.HasOne(d => d.MaHVNavigation).WithMany(p => p.HocVienPhuHuynh)
                .HasForeignKey(d => d.MaHV)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocVienPhuHuynh_HocVien");

            entity.HasOne(d => d.MaPHNavigation).WithMany(p => p.HocVienPhuHuynh)
                .HasForeignKey(d => d.MaPH)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HocVienPhuHuynh_PhuHuynh");
        });

        modelBuilder.Entity<KetQua>(entity =>
        {
            entity.HasKey(e => new { e.MaHV, e.MaLop });

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.DiemTongKet).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.KetQua1)
                .HasMaxLength(10)
                .HasColumnName("KetQua");
            entity.Property(e => e.MaNVDuyet)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.XepLoai).HasMaxLength(20);

            entity.HasOne(d => d.MaNVDuyetNavigation).WithMany(p => p.KetQua)
                .HasForeignKey(d => d.MaNVDuyet)
                .HasConstraintName("FK_KetQua_NhanVien");

            entity.HasOne(d => d.DangKy).WithOne(p => p.KetQua)
                .HasForeignKey<KetQua>(d => new { d.MaHV, d.MaLop })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQua_DangKy");
        });

        modelBuilder.Entity<KhoaHoc>(entity =>
        {
            entity.HasKey(e => e.MaKH);

            entity.Property(e => e.MaKH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.HocPhi).HasColumnType("decimal(12, 0)");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgonNgu).HasMaxLength(10);
            entity.Property(e => e.TenKH).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Đang mở");
            entity.Property(e => e.TrinhDoDauRa)
                .HasMaxLength(2)
                .IsUnicode(false);
            entity.Property(e => e.TrinhDoDauVao)
                .HasMaxLength(2)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LichTuan>(entity =>
        {
            entity.HasKey(e => new { e.MaLop, e.Thu });

            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.GioBatDau).HasPrecision(0);
            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.MaLopNavigation).WithMany(p => p.LichTuan)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichTuan_LopHoc");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.LichTuan)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichTuan_PhongHoc");
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.MaLop);

            entity.HasIndex(e => e.MaGV, "IX_LopHoc_MaGV");

            entity.HasIndex(e => e.MaKH, "IX_LopHoc_MaKH");

            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.MaGV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaKH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Dự kiến");

            entity.HasOne(d => d.MaGVNavigation).WithMany(p => p.LopHoc)
                .HasForeignKey(d => d.MaGV)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_GiaoVien");

            entity.HasOne(d => d.MaKHNavigation).WithMany(p => p.LopHoc)
                .HasForeignKey(d => d.MaKH)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LopHoc_KhoaHoc");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNV);

            entity.Property(e => e.MaNV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.BoPhan).HasMaxLength(30);
            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.DienThoai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Đang làm");
        });

        modelBuilder.Entity<NhatKy>(entity =>
        {
            entity.HasKey(e => e.MaNK);

            entity.HasIndex(e => e.ThoiDiem, "IX_NhatKy_ThoiDiem");

            entity.Property(e => e.DoiTuong)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MaDoiTuong)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ThaoTac).HasMaxLength(20);
            entity.Property(e => e.ThoiDiem)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.TenDangNhapNavigation).WithMany(p => p.NhatKy)
                .HasForeignKey(d => d.TenDangNhap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NhatKy_TaiKhoan");
        });

        modelBuilder.Entity<PhieuDangKy>(entity =>
        {
            entity.HasKey(e => e.SoPhieuDK);

            entity.Property(e => e.SoPhieuDK)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.MaNV)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.MaNVNavigation).WithMany(p => p.PhieuDangKy)
                .HasForeignKey(d => d.MaNV)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhieuDangKy_NhanVien");
        });

        modelBuilder.Entity<PhieuThu>(entity =>
        {
            entity.HasKey(e => e.SoPT);

            entity.HasIndex(e => e.NgayThu, "IX_PhieuThu_NgayThu");

            entity.Property(e => e.SoPT)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.HinhThuc).HasMaxLength(20);
            entity.Property(e => e.MaNV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.NguoiNop).HasMaxLength(100);

            entity.HasOne(d => d.MaNVNavigation).WithMany(p => p.PhieuThu)
                .HasForeignKey(d => d.MaNV)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhieuThu_NhanVien");
        });

        modelBuilder.Entity<PhongHoc>(entity =>
        {
            entity.HasKey(e => e.MaPhong);

            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenPhong).HasMaxLength(50);
            entity.Property(e => e.ThietBi).HasMaxLength(200);
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(20)
                .HasDefaultValue("Sẵn sàng");
        });

        modelBuilder.Entity<PhuHuynh>(entity =>
        {
            entity.HasKey(e => e.MaPH);

            entity.Property(e => e.MaPH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DienThoai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TenDangNhap);

            entity.HasIndex(e => e.MaGV, "UX_TaiKhoan_MaGV")
                .IsUnique()
                .HasFilter("([MaGV] IS NOT NULL)");

            entity.HasIndex(e => e.MaHV, "UX_TaiKhoan_MaHV")
                .IsUnique()
                .HasFilter("([MaHV] IS NOT NULL)");

            entity.HasIndex(e => e.MaNV, "UX_TaiKhoan_MaNV")
                .IsUnique()
                .HasFilter("([MaNV] IS NOT NULL)");

            entity.HasIndex(e => e.MaPH, "UX_TaiKhoan_MaPH")
                .IsUnique()
                .HasFilter("([MaPH] IS NOT NULL)");

            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaGV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaNV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaPH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MatKhauBam)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Hoạt động");
            entity.Property(e => e.VaiTro).HasMaxLength(20);

            entity.HasOne(d => d.MaGVNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaGV)
                .HasConstraintName("FK_TaiKhoan_GiaoVien");

            entity.HasOne(d => d.MaHVNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaHV)
                .HasConstraintName("FK_TaiKhoan_HocVien");

            entity.HasOne(d => d.MaNVNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaNV)
                .HasConstraintName("FK_TaiKhoan_NhanVien");

            entity.HasOne(d => d.MaPHNavigation).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaPH)
                .HasConstraintName("FK_TaiKhoan_PhuHuynh");
        });

        modelBuilder.Entity<ThamSo>(entity =>
        {
            entity.HasKey(e => e.MaThamSo);

            entity.Property(e => e.MaThamSo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DonVi).HasMaxLength(20);
            entity.Property(e => e.GiaTri).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.QuyTac)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenThamSo).HasMaxLength(100);
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.MaTB);

            entity.HasIndex(e => new { e.MaHV, e.ThoiGianGui }, "IX_ThongBao_MaHV");

            entity.Property(e => e.Loai).HasMaxLength(20);
            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaPH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.NoiDung).HasMaxLength(1000);
            entity.Property(e => e.ThoiGianGui)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThaiGui)
                .HasMaxLength(10)
                .HasDefaultValue("Chờ gửi");

            entity.HasOne(d => d.MaHVNavigation).WithMany(p => p.ThongBao)
                .HasForeignKey(d => d.MaHV)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThongBao_HocVien");

            entity.HasOne(d => d.MaPHNavigation).WithMany(p => p.ThongBao)
                .HasForeignKey(d => d.MaPH)
                .HasConstraintName("FK_ThongBao_PhuHuynh");

            entity.HasOne(d => d.HocVienPhuHuynh).WithMany(p => p.ThongBao)
                .HasForeignKey(d => new { d.MaHV, d.MaPH })
                .HasConstraintName("FK_ThongBao_GiamHo");
        });

        modelBuilder.Entity<UuDai>(entity =>
        {
            entity.HasKey(e => e.MaUD);

            entity.Property(e => e.MaUD)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.DieuKien).HasMaxLength(200);
            entity.Property(e => e.LoaiUD).HasMaxLength(20);
            entity.Property(e => e.TenUD).HasMaxLength(100);
            entity.Property(e => e.TyLeGiam).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<v_ChuoiDangKy>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ChuoiDangKy");

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.MaLopDau)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<v_ChuyenCan>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ChuyenCan");

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(20);
            entity.Property(e => e.TyLeChuyenCan).HasColumnType("decimal(5, 1)");
        });

        modelBuilder.Entity<v_DoanhThu>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_DoanhThu");

            entity.Property(e => e.HinhThuc).HasMaxLength(20);
            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaKH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SoPT)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SoTien).HasColumnType("decimal(12, 0)");
            entity.Property(e => e.TenKH).HasMaxLength(100);
        });

        modelBuilder.Entity<v_GiangDay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_GiangDay");

            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MaGV)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<v_HocPhiDangKy>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_HocPhiDangKy");

            entity.Property(e => e.ConNo).HasColumnType("decimal(38, 0)");
            entity.Property(e => e.DaThu).HasColumnType("decimal(38, 0)");
            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.MaLopDau)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.PhaiNop).HasColumnType("decimal(38, 0)");
            entity.Property(e => e.TrangThai).HasMaxLength(20);
        });

        modelBuilder.Entity<v_HocVienCu>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_HocVienCu");

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<v_KetQuaTinh>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_KetQuaTinh");

            entity.Property(e => e.DiemChuyenCan).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.DiemCuoiKy).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.DiemGiuaKy).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.DiemTongKet).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.KetQua).HasMaxLength(9);
            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.XepLoai).HasMaxLength(10);
        });

        modelBuilder.Entity<v_SiSoLop>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_SiSoLop");

            entity.Property(e => e.MaKH)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaLop)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(20);
            entity.Property(e => e.TyLeLapDay).HasColumnType("decimal(5, 1)");
        });

        modelBuilder.Entity<v_TuyenSinh>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_TuyenSinh");

            entity.Property(e => e.MaHV)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaKH)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
