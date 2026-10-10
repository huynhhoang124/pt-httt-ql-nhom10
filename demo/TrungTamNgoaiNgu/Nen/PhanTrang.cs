using Microsoft.EntityFrameworkCore;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>Một trang của danh sách dài (06_giao_dien mục 6, "Linh hoạt": người dùng tự chọn số dòng trên một trang).</summary>
public record TrangDs(int So, int Co, int Tong)
{
    public static readonly int[] CoChon = [20, 50, 100];
    public int SoTrang => Math.Max(1, (Tong + Co - 1) / Co);
}

public static class PhanTrang
{
    /// <summary>Lấy trang thứ <paramref name="trang"/> (từ 1), mỗi trang <paramref name="co"/> dòng (20/50/100, mặc định 50).
    /// Truy vấn phải đã sắp thứ tự.</summary>
    public static async Task<(List<T> Ds, TrangDs Trang)> LayTrangAsync<T>(this IQueryable<T> q, int? trang, int? co)
    {
        int c = co is int x && TrangDs.CoChon.Contains(x) ? x : 50, tong = await q.CountAsync();
        var t = new TrangDs(Math.Clamp(trang ?? 1, 1, Math.Max(1, (tong + c - 1) / c)), c, tong);
        return (await q.Skip((t.So - 1) * c).Take(c).ToListAsync(), t);
    }
}
