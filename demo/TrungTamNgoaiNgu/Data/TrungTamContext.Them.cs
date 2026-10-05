using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Data;

// Phần viết tay của DbContext (TrungTamContext.cs do "dotnet ef dbcontext scaffold" sinh, không sửa tay).
public partial class TrungTamContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<CongNo>().HasNoKey().ToView(null);

    /// <summary>Công nợ tại một ngày: dbo.fn_CongNo (3.3, QT10, QT16).</summary>
    public IQueryable<CongNo> CongNoTai(DateOnly ngay) =>
        Set<CongNo>().FromSqlInterpolated($"SELECT * FROM dbo.fn_CongNo({ngay.ToDateTime(TimeOnly.MinValue)})");
}
