using System.Globalization;
using TrungTamNgoaiNgu.Data.Entities;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>Định dạng hiển thị dùng chung. Không đổi văn hóa của luồng xử lý, để ô điểm vẫn nhập "7.5" (06_giao_dien mục 6).</summary>
public static class HienThi
{
    static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public static string Tien(decimal x) => x.ToString("N0", Vi) + " đ";
    public static string So(decimal x) => x.ToString("N0", Vi);
    public static string Ngay(DateOnly? d) => d?.ToString("dd/MM/yyyy") ?? "–";
    public static string Thu(byte thu) => thu == 8 ? "CN" : $"T{thu}";

    static readonly string[] ChuSo = ["không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín"];
    static readonly string[] DonVi = ["", " nghìn", " triệu", " tỷ"];

    /// <summary>Số tiền bằng chữ trên phiếu thu, vd 5400000 → "Năm triệu bốn trăm nghìn đồng".</summary>
    public static string BangChu(decimal tien)
    {
        var n = (long)Math.Round(tien);
        if (n == 0) return "Không đồng";
        var nhom = new List<int>();
        for (; n > 0; n /= 1000) nhom.Add((int)(n % 1000));
        var phan = new List<string>();
        for (int i = nhom.Count - 1; i >= 0; i--)
            if (nhom[i] > 0) phan.Add(Doc3(nhom[i], i < nhom.Count - 1) + DonVi[i]);  // ponytail: đọc tới hàng tỷ, đủ cho học phí
        var s = string.Join(" ", phan);
        return char.ToUpper(s[0]) + s[1..] + " đồng";
    }

    static string Doc3(int so, bool du)  // du: không phải nhóm đầu → đọc đủ "không trăm", "linh"
    {
        int tram = so / 100, chuc = so / 10 % 10, dv = so % 10;
        var p = new List<string>();
        if (du || tram > 0) p.Add($"{ChuSo[tram]} trăm");
        if (chuc == 0 && dv > 0) p.Add(du || tram > 0 ? $"linh {ChuSo[dv]}" : ChuSo[dv]);
        else if (chuc == 1) p.Add(dv == 0 ? "mười" : $"mười {(dv == 5 ? "lăm" : ChuSo[dv])}");
        else if (chuc > 1) p.Add($"{ChuSo[chuc]} mươi" + (dv == 0 ? "" : dv == 1 ? " mốt" : dv == 5 ? " lăm" : $" {ChuSo[dv]}"));
        return string.Join(" ", p);
    }

    /// <summary>Lịch tuần của lớp, vd "T3, T5 18:00 · P203".</summary>
    public static string Lich(IEnumerable<LichTuan> lich) => string.Join("; ", lich.GroupBy(l => (l.GioBatDau, l.MaPhong))
        .Select(g => $"{string.Join(", ", g.OrderBy(x => x.Thu).Select(x => Thu(x.Thu)))} {g.Key.GioBatDau:HH\\:mm} · {g.Key.MaPhong}"));
}
