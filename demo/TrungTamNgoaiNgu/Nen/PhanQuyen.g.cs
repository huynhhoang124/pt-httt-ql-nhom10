// Sinh tự động bởi demo/sinh_ma.py từ thiet_ke/05_module.md mục 5 – không sửa tay; sửa bảng trong .md rồi chạy lại.

namespace TrungTamNgoaiNgu.Nen;

public static partial class PhanQuyen
{
    public static readonly string[] VaiTro = { "HV", "PH", "GV", "TS", "KT", "DT", "GD", "QT" };

    static readonly Dictionary<string, string[]> MaTran = new()
    {
        ["1.1"] = new[] { "x", "x", "x", "TSX", "X", "X", "X", "X" },
        ["1.2"] = new[] { "", "", "x", "X", "", "TSX", "X", "X" },
        ["1.3"] = new[] { "X", "X", "X", "X", "X", "TSX", "X", "" },
        ["1.4"] = new[] { "", "", "", "", "", "TSX", "X", "" },
        ["2.1"] = new[] { "X", "X", "x", "X", "X", "TSX", "X", "" },
        ["2.2"] = new[] { "x", "x", "x", "TSX", "X", "X", "X", "" },
        ["2.3"] = new[] { "x", "x", "x", "X", "", "TSX", "X", "" },
        ["2.4"] = new[] { "x", "x", "", "TSX", "X", "X", "X", "" },
        ["3.1"] = new[] { "x", "x", "", "X", "TSX", "", "X", "" },
        ["3.2"] = new[] { "x", "x", "", "X", "TX", "", "X", "" },
        ["3.3"] = new[] { "x", "x", "", "X", "X", "", "X", "" },
        ["3.4"] = new[] { "", "", "", "", "X", "", "X", "" },
        ["4.1"] = new[] { "x", "x", "TSx", "X", "", "X", "X", "" },
        ["4.2"] = new[] { "x", "x", "TSx", "", "", "X", "X", "" },
        ["4.3"] = new[] { "x", "x", "x", "", "", "DX", "X", "" },
        ["4.4"] = new[] { "x", "x", "", "X", "", "IX", "X", "" },
        ["5.1"] = new[] { "", "", "", "", "", "", "", "TSX" },
        ["5.2"] = new[] { "x", "x", "", "X", "X", "X", "", "X" },
        ["5.3"] = new[] { "", "", "", "X", "", "X", "X", "" },
        ["5.4"] = new[] { "", "", "x", "", "", "X", "X", "" },
        ["N1"] = new[] { "x", "x", "x", "x", "x", "x", "x", "x" },
        ["N4"] = new[] { "", "", "", "", "", "", "", "TX" },
    };
}
