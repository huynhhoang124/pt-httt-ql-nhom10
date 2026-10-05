using System.IO.Compression;
using System.Security;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace TrungTamNgoaiNgu.Nen;

/// <summary>N5 – Xuất báo cáo ra tệp Excel (.xlsx) một trang tính. Tự ghi các phần XML tối thiểu của định dạng Office Open XML
/// bằng System.IO.Compression, không cần thư viện ngoài. Xuất PDF dùng lệnh in của trình duyệt ("In / lưu PDF").</summary>
// ponytail: chỉ chữ và số, không định dạng ô; cần định dạng, công thức thì thêm ClosedXML
public static class XuatExcel
{
    public static FileContentResult Tep(string ten, IReadOnlyList<string> cot, IEnumerable<IReadOnlyList<object?>> dong)
    {
        var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            void Ghi(string duongDan, string xml)
            {
                using var w = new StreamWriter(zip.CreateEntry(duongDan).Open(), new UTF8Encoding(false));
                w.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" + xml);
            }
            const string ns = "http://schemas.openxmlformats.org/";
            Ghi("[Content_Types].xml", $"<Types xmlns=\"{ns}package/2006/content-types\">" +
                "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
            Ghi("_rels/.rels", $"<Relationships xmlns=\"{ns}package/2006/relationships\">" +
                $"<Relationship Id=\"r1\" Type=\"{ns}officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            Ghi("xl/workbook.xml", $"<workbook xmlns=\"{ns}spreadsheetml/2006/main\" xmlns:r=\"{ns}officeDocument/2006/relationships\">" +
                "<sheets><sheet name=\"BaoCao\" sheetId=\"1\" r:id=\"r1\"/></sheets></workbook>");
            Ghi("xl/_rels/workbook.xml.rels", $"<Relationships xmlns=\"{ns}package/2006/relationships\">" +
                $"<Relationship Id=\"r1\" Type=\"{ns}officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
            var sb = new StringBuilder($"<worksheet xmlns=\"{ns}spreadsheetml/2006/main\"><sheetData>");
            foreach (var d in dong.Prepend(cot.Cast<object?>().ToList()))
            {
                sb.Append("<row>");
                foreach (var o in d)
                    sb.Append(o switch
                    {
                        null => "<c/>",
                        decimal or int or short or byte or long or double => $"<c><v>{Convert.ToString(o, System.Globalization.CultureInfo.InvariantCulture)}</v></c>",
                        DateOnly n => $"<c t=\"inlineStr\"><is><t>{n:dd/MM/yyyy}</t></is></c>",
                        _ => $"<c t=\"inlineStr\"><is><t xml:space=\"preserve\">{SecurityElement.Escape(o.ToString())}</t></is></c>",
                    });
                sb.Append("</row>");
            }
            Ghi("xl/worksheets/sheet1.xml", sb.Append("</sheetData></worksheet>").ToString());
        }
        return new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = ten + ".xlsx" };
    }
}
