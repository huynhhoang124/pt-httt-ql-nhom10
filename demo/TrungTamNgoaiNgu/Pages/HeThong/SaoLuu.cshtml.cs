using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TrungTamNgoaiNgu.Data;
using TrungTamNgoaiNgu.Nen;

namespace TrungTamNgoaiNgu.Pages.HeThong;

/// <summary>FN4 – Sao lưu dữ liệu (module N4). BACKUP DATABASE toàn bộ CSDL vào thư mục sao lưu mặc định của SQL Server;
/// danh sách lấy từ lịch sử sao lưu msdb. Phục hồi làm ngoài chương trình (dừng ứng dụng rồi RESTORE), xem trợ giúp trang.</summary>
[Quyen("N4", "T")]
public class SaoLuuModel(TrungTamContext db, DongHo dongHo) : PageModel
{
    public class BanSaoLuu
    {
        public DateTime ThoiGian { get; set; }
        public string Tep { get; set; } = "";
        public decimal MB { get; set; }
    }

    public List<BanSaoLuu> DanhSach { get; set; } = [];
    public string? ThongBao { get; set; }

    public async Task OnGetAsync() => DanhSach = await LichSuAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        var csdl = db.Database.GetDbConnection().Database;
        var tep = $"{csdl}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";   // giờ thật của máy chủ, không theo giờ demo, để tên tệp không trùng
        db.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
        await db.Database.ExecuteSqlInterpolatedAsync($"BACKUP DATABASE {csdl} TO DISK = {tep} WITH CHECKSUM, INIT");
        db.GhiNhatKy(User, "Thêm", "SaoLuu", tep, dongHo.Bay);
        await db.SaveChangesAsync();
        DanhSach = await LichSuAsync();
        ThongBao = $"Đã sao lưu: {DanhSach.FirstOrDefault()?.Tep ?? tep}.";
        return Page();
    }

    // ponytail: chỉ liệt kê 30 bản gần nhất, không tự xóa tệp cũ; thêm xóa (xp_delete_file hoặc Task Scheduler) khi ổ đầy
    Task<List<BanSaoLuu>> LichSuAsync() => db.Database.SqlQuery<BanSaoLuu>($"""
        SELECT TOP 30 b.backup_finish_date AS ThoiGian, m.physical_device_name AS Tep, CAST(b.backup_size / 1048576.0 AS DECIMAL(10, 1)) AS MB
        FROM msdb.dbo.backupset b JOIN msdb.dbo.backupmediafamily m ON m.media_set_id = b.media_set_id
        WHERE b.database_name = DB_NAME() AND b.type = 'D'
        ORDER BY b.backup_finish_date DESC
        """).ToListAsync();
}
