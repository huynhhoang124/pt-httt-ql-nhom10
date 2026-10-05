"""Dựng báo cáo Word + PDF từ các file .md của dự án (nội dung chỉ sửa ở .md, không sửa tay file Word).

Định dạng giữ như Bài tập 1: Times New Roman, đen trắng, bảng viền xám nhạt, đầu bảng xám.
Chạy: python bao_cao/lam_bao_cao.py [phan_tich] [thiet_ke] [tong]  ->  bao_cao/Bao_cao_GD1_GD2_Nhom_10, Bao_cao_GD3_Nhom_10,
Bao_cao_tong_Nhom_10 (.docx, .pdf; báo cáo tổng = BT1 + GĐ1 → GĐ5)
(xuất PDF và cập nhật mục lục cần Microsoft Word trên Windows, qua pywin32).
"""
import copy
import io
import re
import sys
from pathlib import Path

from docx import Document
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Inches, Pt, RGBColor
from PIL import Image

GOC = Path(__file__).resolve().parents[1]
FONT, MONO = "Times New Roman", "Consolas"
RONG_TRANG = 6.6  # inch, bề rộng vùng chữ
CAO_TRANG = 9.0   # inch, chiều cao tối đa cho một hình
THANH_VIEN = ["Hoàng Văn Huynh", "Nguyễn Gia Ân", "Vũ Văn Hùng",
              "Nguyễn Văn Luận", "Trần Thu Thủy", "Trần Minh Quân"]


# ---------- định dạng (lấy từ create_report.py của Bài tập 1) ----------

def font(run, size=12, bold=None, italic=None, name=FONT):
    run.font.name = name
    run.font.size = Pt(size)
    rf = run._element.get_or_add_rPr().get_or_add_rFonts()
    for k in ("w:ascii", "w:hAnsi", "w:eastAsia", "w:cs"):
        rf.set(qn(k), name)
    run.font.color.rgb = RGBColor(0, 0, 0)
    if bold is not None:
        run.bold = bold
    if italic is not None:
        run.italic = italic


def _tc(cell, tag):
    pr = cell._tc.get_or_add_tcPr()
    el = pr.find(qn(tag))
    if el is None:
        el = OxmlElement(tag)
        pr.append(el)
    return el


def to_nen(cell, mau):
    _tc(cell, "w:shd").set(qn("w:fill"), mau)


def ke_vien(cell, mau="BFBFBF"):
    vien = _tc(cell, "w:tcBorders")
    for canh in ("top", "left", "bottom", "right"):
        el = OxmlElement(f"w:{canh}")
        el.set(qn("w:val"), "single")
        el.set(qn("w:sz"), "6")
        el.set(qn("w:color"), mau)
        vien.append(el)


def le_o(cell, tren=60, ben=80):
    mar = _tc(cell, "w:tcMar")
    for canh, v in (("top", tren), ("start", ben), ("bottom", tren), ("end", ben)):
        el = OxmlElement(f"w:{canh}")
        el.set(qn("w:w"), str(v))
        el.set(qn("w:type"), "dxa")
        mar.append(el)


def thiet_lap_kieu(doc):
    s = doc.sections[0]
    s.page_width, s.page_height = Inches(8.5), Inches(11)
    s.top_margin, s.bottom_margin = Inches(0.85), Inches(0.75)
    s.left_margin, s.right_margin = Inches(1.0), Inches(0.85)
    s.different_first_page_header_footer = True
    for ten, co in (("Normal", 12), ("Title", 20), ("Heading 1", 15), ("Heading 2", 13),
                    ("Heading 3", 12), ("Heading 4", 12)):
        st = doc.styles[ten]
        st.font.name, st.font.size = FONT, Pt(co)
        st.font.color.rgb = RGBColor(0, 0, 0)
        rf = st._element.get_or_add_rPr().get_or_add_rFonts()
        for k in ("w:ascii", "w:hAnsi", "w:eastAsia", "w:cs"):
            rf.set(qn(k), FONT)
        for k in ("w:asciiTheme", "w:hAnsiTheme", "w:eastAsiaTheme", "w:cstheme"):
            rf.attrib.pop(qn(k), None)
        if ten.startswith("Heading"):
            st.font.bold, st.font.italic = True, ten == "Heading 4"
            st.paragraph_format.keep_with_next = True
            st.paragraph_format.space_before = Pt(12 if ten == "Heading 1" else 8)
            st.paragraph_format.space_after = Pt(6)
    doc.styles["Normal"].paragraph_format.line_spacing = 1.3
    doc.styles["Normal"].paragraph_format.space_after = Pt(6)


def so_trang(doc):
    p = doc.sections[0].footer.paragraphs[0]
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("Nhóm 10  |  Trang ")
    font(r, 9)
    _truong(r, "PAGE")


def _truong(run, ma):
    for loai, text in (("begin", None), (None, f" {ma} "), ("separate", None), ("end", None)):
        if loai:
            el = OxmlElement("w:fldChar")
            el.set(qn("w:fldCharType"), loai)
        else:
            el = OxmlElement("w:instrText")
            el.set(qn("xml:space"), "preserve")
            el.text = text
        run._r.append(el)


def doan(doc, text="", size=12, bold=False, italic=False, can=WD_ALIGN_PARAGRAPH.JUSTIFY, sau=6, thut=0.0):
    p = doc.add_paragraph()
    p.alignment = can
    p.paragraph_format.space_after = Pt(sau)
    if thut:
        p.paragraph_format.left_indent = Cm(thut)
    noi_dung(p, text, size, bold, italic)
    return p


def noi_dung(p, text, size=12, bold=False, italic=False):
    """Chữ có **đậm**, *nghiêng*, `mã` kiểu markdown."""
    text = text.replace("\\|", "|")
    for m in re.split(r"(\*\*[^*]+\*\*|\*[^*\s][^*]*\*|`[^`]+`)", text):
        if not m:
            continue
        if m.startswith("**"):
            font(p.add_run(m[2:-2]), size, True, italic)
        elif m.startswith("`"):
            font(p.add_run(m[1:-1]), size - 1, bold, italic, MONO)
        elif m.startswith("*") and len(m) > 2:
            font(p.add_run(m[1:-1]), size, bold, True)
        else:
            font(p.add_run(m), size, bold, italic)


def bang(doc, dau, dong, can=None):
    n = len(dau)
    co = 10.5 if n <= 4 else 9.5 if n <= 7 else 7.5
    do_dai = [max([len(dau[i])] + [len(r[i]) for r in dong]) for i in range(n)]
    san = 3 if n > 8 else 7  # bề rộng tối thiểu (ký tự), để mã "2.2", "QT01–QT05" không bị bẻ dòng
    trong_so = [min(max(d, san), 48) ** 0.8 for d in do_dai]
    rong = [RONG_TRANG * t / sum(trong_so) for t in trong_so]
    if n <= 8:  # cột hẹp nhất vẫn đủ chỗ cho "Mã", "2.2"
        hep = [i for i, r in enumerate(rong) if r < 0.5]
        du = RONG_TRANG - 0.5 * len(hep)
        rong_con = sum(r for i, r in enumerate(rong) if i not in hep)
        rong = [0.5 if i in hep else r * du / rong_con for i, r in enumerate(rong)]
    can = can or [WD_ALIGN_PARAGRAPH.LEFT] * n
    t = doc.add_table(rows=0, cols=n)
    t.alignment = WD_ALIGN_PARAGRAPH.CENTER
    t.autofit = False
    lo = OxmlElement("w:tblLayout")
    lo.set(qn("w:type"), "fixed")
    t._tbl.tblPr.append(lo)
    for ci, col in enumerate(t.columns):  # Word lấy độ rộng từ tblGrid, không chỉ từ ô
        col.width = Inches(rong[ci])
    for ri, hang in enumerate([dau] + dong):
        row = t.add_row()
        tr = row._tr.get_or_add_trPr()
        tr.append(OxmlElement("w:cantSplit"))
        if ri == 0:
            h = OxmlElement("w:tblHeader")
            h.set(qn("w:val"), "true")
            tr.append(h)
        for ci, gt in enumerate(hang):
            c = row.cells[ci]
            c.width = Inches(rong[ci])
            c.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            ke_vien(c)
            le_o(c)
            if ri == 0:
                to_nen(c, "E7E6E6")
            p = c.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if ri == 0 else can[ci]
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.05
            noi_dung(p, gt, co, bold=ri == 0)
    doc.add_paragraph().paragraph_format.space_after = Pt(2)


def hinh(doc, duong_dan, chu_thich, xoay=False):
    """xoay: hình quá ngang (rộng > 1,8 lần cao) được xoay dọc trang cho chữ đủ lớn."""
    anh = Image.open(duong_dan)
    if xoay and anh.width > 1.8 * anh.height:
        anh = anh.rotate(90, expand=True)
    w, h = anh.size
    rong = min(RONG_TRANG, CAO_TRANG * w / h)
    tep = io.BytesIO()
    anh.save(tep, "PNG")
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.keep_with_next = True
    p.add_run().add_picture(tep, width=Inches(rong))
    doan(doc, chu_thich, 11, italic=True, can=WD_ALIGN_PARAGRAPH.CENTER, sau=10)


LECH = 0  # báo cáo tổng: 1 – mọi tiêu đề lùi một cấp để "PHẦN …" là Heading 1


def tieu_de(doc, text, cap):
    doc.add_paragraph(text.replace("`", ""), style=f"Heading {min(cap + LECH, 4)}")


# ---------- markdown -> docx ----------

SO_HINH = {}  # chương -> số hình đã chèn, để đánh số "Hình <chương>.n"


def doc_md(doc, md, chuong, bo_muc=(), thay=(), giu_dau=False, thu_muc=GOC):
    """Thêm nội dung một file .md vào báo cáo.
    chuong: số chương để đánh số lại tiêu đề ("## 1. X" -> "2.1 X"); tiêu đề "#" thành Heading 1 sang trang mới.
    Bỏ phần mở đầu (trước dấu --- đầu tiên, trừ khi giu_dau) và các mục có tiêu đề nằm trong bo_muc.
    Hình "![chú thích](đường dẫn)": đường dẫn tính từ thu_muc, chú thích đánh số "Hình <chương>.n"."""
    for a, b in thay:
        md = re.sub(a, b, md)
    if chuong:  # tham chiếu "mục x.y" trong cùng file -> "mục <chương>.x.y"; tham chiếu đã xử lý ở 'thay' viết "mục "
        md = re.sub(r"(?<=[Mm]ục )(\d+(?:\.\d+)*)(?:–(\d+)\b)?",
                    lambda k: f"{chuong}.{k.group(1)}" + (f"–{chuong}.{k.group(2)}" if k.group(2) else ""), md)
        if not giu_dau:
            md = re.sub(r"\((\d\.\d\.\d)\)", lambda k: f"({chuong}.{k.group(1)})", md)  # "(4.2.1)" trong 2.1
    md = md.replace("mục ", "mục ").replace("Mục ", "Mục ")
    dong = md.split("\n")
    if "---" in [d.strip() for d in dong] and not giu_dau:
        dong = dong[[d.strip() for d in dong].index("---") + 1:]
    i, bo_cap = 0, None
    while i < len(dong):
        d = dong[i]
        m = re.match(r"^(#{1,6})\s+(.*)$", d)
        if m:
            cap, text = len(m.group(1)), m.group(2).strip()
            if bo_cap is not None and cap > bo_cap:
                i += 1
                continue
            bo_cap = None
            if any(b in text for b in bo_muc):
                bo_cap = cap
                i += 1
                continue
            if cap == 1:
                sang_chuong(doc, text)
            else:
                text = re.sub(r"^(\d+(?:\.\d+)*)\.?\s", lambda k: f"{chuong}.{k.group(1)} ", text) if chuong else text
                tieu_de(doc, text, cap)
            i += 1
            continue
        if bo_cap is not None:
            i += 1
            continue
        if d.strip() in ("", "---"):
            i += 1
        elif re.match(r"^!\[.*\]\(.*\)$", d.strip()):
            alt, duong = re.match(r"^!\[(.*)\]\((.*)\)$", d.strip()).groups()
            SO_HINH[chuong] = SO_HINH.get(chuong, 0) + 1
            hinh(doc, (thu_muc / duong).resolve(), f"Hình {chuong}.{SO_HINH[chuong]}. {alt}", xoay=True)
            i += 1
        elif d.startswith("```"):
            j = i + 1
            while not dong[j].startswith("```"):
                j += 1
            khoi_ma(doc, dong[i + 1:j])
            i = j + 1
        elif d.lstrip().startswith("|"):
            j = i
            while j < len(dong) and dong[j].lstrip().startswith("|"):
                j += 1
            o = [[x.strip() for x in re.split(r"(?<!\\)\|", r.strip())[1:-1]] for r in dong[i:j]]
            can = [WD_ALIGN_PARAGRAPH.CENTER if x.startswith(":") and x.endswith(":") else WD_ALIGN_PARAGRAPH.LEFT
                   for x in o[1]]
            bang(doc, o[0], o[2:], can)
            i = j
        elif d.startswith(">"):
            j = i
            while j < len(dong) and dong[j].startswith(">"):
                j += 1
            for q in dong[i:j]:
                q = q.lstrip(">").strip()
                if q.startswith("- "):
                    doan(doc, "– " + q[2:], 11.5, sau=2, thut=1.4)
                elif q:
                    doan(doc, q, 11.5, sau=3, thut=0.8)
            i = j
        elif re.match(r"^\s*([-*]|\d+\.)\s+", d):
            m = re.match(r"^(\s*)([-*]|\d+\.)\s+(.*)$", d)
            cap = len(m.group(1)) // 2
            dau = "–" if m.group(2) in "-*" else m.group(2)
            p = doan(doc, f"{dau} {m.group(3)}", sau=3, can=WD_ALIGN_PARAGRAPH.LEFT)
            p.paragraph_format.left_indent = Cm(0.8 + 0.6 * cap)
            p.paragraph_format.first_line_indent = Cm(-0.45)
            i += 1
        else:
            j = i
            while j < len(dong) and dong[j].strip() and not re.match(r"^(#|\||>|```|\s*([-*]|\d+\.)\s)", dong[j]):
                j += 1
            doan(doc, " ".join(x.strip() for x in dong[i:j]))
            i = j


def khoi_ma(doc, dong):
    for k, d in enumerate(dong):
        p = doc.add_paragraph()
        p.paragraph_format.space_after = Pt(0 if k < len(dong) - 1 else 8)
        p.paragraph_format.line_spacing = 1.0
        p.paragraph_format.left_indent = Cm(0.3)
        p.paragraph_format.keep_together = True
        font(p.add_run(d if d else " "), 7.5, name=MONO)


# ---------- báo cáo phân tích ----------

THAY_CHUNG = [
    (r"`?(?:phan_tich/)?02_thu_thap\.md`?,? mục (\d[\d.]*\d)", r"mục 3.\1"),
    (r"mục (9\.\d) của thu thập thông tin", r"mục 3.\1"),
    (r"`?(?:phan_tich/)?02_thu_thap\.md`?", "chương 3"),
    (r"`so_do/can_bang_dfd\.md`", "Phụ lục A"),
    (r"BFD_v3 \(`so_do/BFD_v3\.png`\)", "BFD (Hình 4.1)"),
    (r"BFD_v3", "BFD"),
]


def bia(doc, ten_bao_cao, thang):
    for text, co, sau in (("MÔN PHÁT TRIỂN HỆ THỐNG THÔNG TIN QUẢN LÝ", 13, 90),
                          (ten_bao_cao, 20, 12), ("HỆ THỐNG QUẢN LÝ TRUNG TÂM NGOẠI NGỮ", 15, 34),
                          ("NHÓM 10", 15, 18)):
        doan(doc, text, co, bold=True, can=WD_ALIGN_PARAGRAPH.CENTER, sau=sau)
    for k, tv in enumerate(THANH_VIEN, 1):
        doan(doc, f"{k}  {tv}", can=WD_ALIGN_PARAGRAPH.CENTER, sau=5)
    doan(doc, thang, can=WD_ALIGN_PARAGRAPH.CENTER, sau=0).paragraph_format.space_before = Pt(60)
    doc.add_page_break()


def muc_luc(doc):
    doan(doc, "MỤC LỤC", 15, bold=True, can=WD_ALIGN_PARAGRAPH.CENTER, sau=12)
    _truong(doc.add_paragraph().add_run(), 'TOC \\o "1-3" \\h \\z \\u')
    doc.add_page_break()


def sang_chuong(doc, text, ngat=True):
    p = doc.add_paragraph(text, style=f"Heading {1 + LECH}")
    p.paragraph_format.page_break_before = ngat


def bao_cao_phan_tich(doc=None):
    """doc=None: báo cáo riêng (bìa, mục lục, phụ lục, lưu tệp). Có doc: ghi phần nội dung vào báo cáo tổng."""
    rieng = doc is None
    md = lambda ten: (GOC / ten).read_text(encoding="utf-8")
    sd = GOC / "so_do"
    if rieng:
        doc = Document()
        thiet_lap_kieu(doc)
        doc.core_properties.title = "Báo cáo xác định và phân tích hệ thống quản lý trung tâm ngoại ngữ"
        doc.core_properties.author = "Nhóm 10"
        bia(doc, "BÁO CÁO XÁC ĐỊNH VÀ PHÂN TÍCH HỆ THỐNG", "Tháng 10 năm 2026")
        so_trang(doc)
        muc_luc(doc)

    # Chương 1
    sang_chuong(doc, "CHƯƠNG 1. GIỚI THIỆU VÀ PHƯƠNG PHÁP LUẬN", ngat=False)
    tieu_de(doc, "1.1 Mục đích của báo cáo", 2)
    doan(doc, "Báo cáo tổng hợp kết quả hai giai đoạn đầu trong vòng đời phát triển hệ thống thông tin (xác định – phân "
              "tích – thiết kế – cài đặt – bảo trì): **giai đoạn 1 – xác định, lựa chọn và lập kế hoạch** (chương 2) và "
              "**giai đoạn 2 – phân tích hệ thống** (chương 3 đến chương 6), cho đề tài *Hệ thống quản lý "
              "trung tâm ngoại ngữ*. Kết quả của báo cáo là đầu vào trực tiếp cho giai đoạn thiết kế: các chức năng, "
              "luồng dữ liệu, kho dữ liệu, quy tắc nghiệp vụ và từ điển dữ liệu được xác định ở đây sẽ được chuyển "
              "thành cơ sở dữ liệu, module chương trình, biểu mẫu và báo cáo.")
    tieu_de(doc, "1.2 Phạm vi và giới hạn", 2)
    doan(doc, "Đối tượng phân tích là một trung tâm ngoại ngữ **giả định** có một cơ sở, khoảng 500 học viên và 30 giáo "
              "viên (như Bài tập 1). Do không khảo sát một trung tâm thực tế, các chứng từ mẫu, kết quả phỏng vấn, quan "
              "sát và phiếu điều tra trong chương 3 là kịch bản giả định, được xây dựng sao cho nhất quán với nhau và "
              "với các quy tắc nghiệp vụ. Ngoài phạm vi: thanh toán trực tuyến, điểm danh bằng mã QR, quản lý nhiều cơ sở.")
    tieu_de(doc, "1.3 Phương pháp luận", 2)
    doan(doc, "Nhóm áp dụng phương pháp **tiếp cận hệ thống từ trên xuống (top-down)** và **phân tích có cấu trúc** "
              "theo bài giảng Hệ thống thông tin quản lý (PTIT). Quy trình gồm bốn bước:")
    for k, t in enumerate(["Thu thập thông tin bằng sáu phương pháp: nghiên cứu tài liệu, quan sát, phỏng vấn, phiếu "
                           "điều tra, JAD và làm mẫu (chương 3).",
                           "Phân tích chức năng và lập sơ đồ chức năng kinh doanh BFD theo hai nguyên tắc phân rã "
                           "*thực chất* và *đầy đủ* (chương 4).",
                           "Lập sơ đồ luồng dữ liệu DFD theo ký pháp Gane & Sarson ở ba mức: ngữ cảnh, mức 0, mức 1, "
                           "bảo đảm cân bằng giữa các mức (chương 5).",
                           "Đặc tả các xử lý mức cơ sở bằng ngôn ngữ có cấu trúc giản lược, cây quyết định, bảng quyết "
                           "định và lập từ điển dữ liệu (chương 6)."], 1):
        p = doan(doc, f"{k}. {t}", sau=3)
        p.paragraph_format.left_indent, p.paragraph_format.first_line_indent = Cm(0.8), Cm(-0.45)
    doan(doc, "Mã chức năng thống nhất xuyên suốt: chức năng *x.y* trên BFD chính là xử lý *x.y* trên DFD mức 1 và sẽ là "
              "module *x.y* ở giai đoạn thiết kế. Các quy tắc nghiệp vụ được đánh mã QT01–QT16 (mục 3.9.3) và được "
              "dẫn chiếu lại ở các chương sau.")
    # Chương 2: GĐ1
    sang_chuong(doc, "CHƯƠNG 2. XÁC ĐỊNH, LỰA CHỌN VÀ LẬP KẾ HOẠCH HỆ THỐNG")
    doc_md(doc, md("phan_tich/01_ke_hoach.md"), 2, thay=THAY_CHUNG)

    # Chương 3–4
    sang_chuong(doc, "CHƯƠNG 3. KẾT QUẢ THU THẬP THÔNG TIN")
    doc_md(doc, md("phan_tich/02_thu_thap.md"), 3, thay=THAY_CHUNG)
    sang_chuong(doc, "CHƯƠNG 4. PHÂN TÍCH CHỨC NĂNG VÀ SƠ ĐỒ BFD")
    doan(doc, "Sơ đồ chức năng kinh doanh được xây dựng theo ba bước của bài giảng: khảo sát chức năng (tên, mô tả, đầu "
              "vào, đầu ra) → mô tả bằng văn bản → vẽ sơ đồ hình cây. Cột *Đầu vào / Đầu ra* ghi tên tác nhân và kho "
              "D1–D5 đúng như trên DFD (chương 5); mã QTxx là quy tắc nghiệp vụ ở mục 3.9.3.")
    bfd_md = md("phan_tich/03_chuc_nang.md").replace("Sơ đồ: `so_do/BFD_v3.png`.", "@@BFD@@")
    phan = bfd_md.split("@@BFD@@")
    doc_md(doc, phan[0], 4, bo_muc=["Đầu ra cho các bước sau"], thay=THAY_CHUNG)
    hinh(doc, sd / "BFD_v3.png", "Hình 4.1. Sơ đồ chức năng kinh doanh (BFD) của hệ thống quản lý trung tâm ngoại ngữ")
    doc_md(doc, "---\n" + phan[1], 4, bo_muc=["Đầu ra cho các bước sau"], thay=THAY_CHUNG)

    # Chương 4
    sang_chuong(doc, "CHƯƠNG 5. SƠ ĐỒ LUỒNG DỮ LIỆU (DFD)")
    tieu_de(doc, "5.1 Ký pháp và nguyên tắc", 2)
    doan(doc, "Các sơ đồ dùng ký pháp Gane & Sarson: xử lý là hình chữ nhật góc tròn, phần trên ghi số định danh, phần "
              "dưới ghi tên (trùng tên chức năng trên BFD); kho dữ liệu là hình chữ nhật hở một đầu, ghi mã D1–D5; tác "
              "nhân ngoài là hình chữ nhật; luồng dữ liệu là mũi tên có tên là danh từ. Tác nhân và kho được phép vẽ "
              "lặp để sơ đồ dễ đọc: tác nhân vẽ lặp có gạch chéo ở góc, kho vẽ lặp có thêm một vạch đứng.")
    doan(doc, "Các quy tắc sau được kiểm tra tự động bằng chương trình khi sinh sơ đồ: mỗi xử lý và mỗi kho có cả luồng "
              "vào và luồng ra; luồng ra của một xử lý khác luồng vào của nó; tác nhân không nối trực tiếp với kho, "
              "kho không nối với kho, tác nhân không nối với tác nhân; các mức DFD cân bằng với nhau (Phụ lục A).")
    tieu_de(doc, "5.2 Các tác nhân và kho dữ liệu", 2)
    bang(doc, ["Tác nhân ngoài", "Vai trò đối với hệ thống"], [
        ["Học viên / Phụ huynh", "Gửi yêu cầu đăng ký, chuyển lớp, bảo lưu, nộp học phí; nhận lịch học, phiếu thu, chuyên cần, kết quả, chứng nhận, thông báo"],
        ["Nhân viên tuyển sinh / Chăm sóc học viên", "Nhập hồ sơ, kết quả kiểm tra đầu vào, phiếu đăng ký, đơn chuyển lớp/bảo lưu; nhận kết quả đăng ký"],
        ["Nhân viên kế toán", "Nhập thông tin thu tiền, ưu đãi; nhận học phí phải thu, phiếu thu, công nợ"],
        ["Giáo viên", "Nhập điểm danh, điểm, nhận xét; nhận lịch dạy, danh sách lớp"],
        ["Quản lý đào tạo", "Nhập khóa học, giáo viên, phòng học, kế hoạch mở lớp; nhận báo cáo đào tạo"],
        ["Quản trị viên", "Quản lý tài khoản, phân quyền, cấu hình; xem nhật ký"],
        ["Giám đốc", "Yêu cầu và nhận báo cáo tuyển sinh, doanh thu, công nợ, kết quả học tập"],
    ])
    bang(doc, ["Kho", "Nội dung", "Xử lý ghi"], [
        ["D1 Hồ sơ", "Học viên, phụ huynh, giáo viên, khóa học, phòng học", "1.0"],
        ["D2 Lớp và lịch học", "Lớp, đăng ký, buổi học", "2.0"],
        ["D3 Học phí", "Ưu đãi, học phí phải thu, phiếu thu", "3.0"],
        ["D4 Học tập", "Điểm danh, điểm thành phần, kết quả, chứng nhận", "4.0"],
        ["D5 Tài khoản và thông báo", "Nhân viên, tài khoản, thông báo, nhật ký", "5.0"],
    ])
    tieu_de(doc, "5.3 DFD mức ngữ cảnh", 2)
    doan(doc, "Toàn bộ hệ thống là một xử lý duy nhất (số 0) trao đổi dữ liệu với 7 tác nhân ngoài; mức này không có kho "
              "dữ liệu. Luồng ở mức ngữ cảnh là gộp các luồng tương ứng ở mức 0, nên hai mức luôn cân bằng.")
    hinh(doc, sd / "DFD_muc_ngu_canh.png", "Hình 5.1. DFD mức ngữ cảnh")
    tieu_de(doc, "5.4 DFD mức 0", 2)
    doan(doc, "Xử lý 0 được phân rã thành 5 xử lý 1.0–5.0, ứng với 5 chức năng cấp 1 của BFD; giữ nguyên tác nhân và "
              "luồng ở mức ngữ cảnh, bổ sung 5 kho D1–D5 cùng các luồng đọc/ghi kho. Các xử lý trao đổi dữ liệu với "
              "nhau thông qua kho: ví dụ 3.0 đọc đăng ký (D2) để tính học phí, 2.0 đọc tình trạng đóng học phí (D3) "
              "để xác nhận đăng ký, 5.0 đọc D1–D4 để lập báo cáo.")
    hinh(doc, sd / "DFD_muc_0.png", "Hình 5.2. DFD mức 0")
    tieu_de(doc, "5.5 DFD mức 1", 2)
    doan(doc, "Mỗi xử lý x.0 được phân rã thành 4 xử lý con x.1–x.4 (đúng 4 chức năng con trên BFD). Mỗi luồng của x.0 ở "
              "mức 0 được giữ nguyên hoặc tách thành các luồng nhỏ hơn đi tới đúng xử lý con cần nó; bảng đối chiếu "
              "ở Phụ lục A. Các xử lý x.y là xử lý mức cơ sở, được đặc tả ở chương 6.")
    mo_ta_1 = {
        "1": "Bốn xử lý con cập nhật bốn loại hồ sơ, danh mục vào kho D1; riêng 1.1 đọc hồ sơ hiện có để kiểm tra trùng.",
        "2": "2.1 mở lớp và chuyển *Lớp đã mở* cho 2.3 xếp lịch, vì vậy 2.3 được vẽ ngay dưới 2.1. 2.2 và 2.4 đọc "
             "tình trạng đóng học phí từ D3 để xác nhận đăng ký (QT05) và xét bảo lưu (QT09).",
        "3": "3.1 lập khoản phải thu, 3.2 thu tiền, 3.3 tính công nợ và chuyển *Tổng công nợ* cho 3.4 lập báo cáo doanh thu.",
        "4": "4.1 và 4.2 ghi điểm danh, điểm vào D4; 4.3 đọc lại cùng trọng số điểm (D1) để tổng kết và chuyển "
             "*Danh sách học viên đạt* cho 4.4 cấp chứng nhận.",
        "5": "5.1 quản trị tài khoản; 5.2 đọc D1–D4 để gửi thông báo; 5.3 và 5.4 chỉ đọc dữ liệu để lập báo cáo MIS.",
    }
    for k, (ma, mo_ta) in enumerate(mo_ta_1.items(), 1):
        from_bfd = ["Quản lý danh mục và hồ sơ", "Quản lý lớp học và lịch học", "Quản lý học phí",
                    "Quản lý học tập", "Quản lý hệ thống và báo cáo"][k - 1]
        tieu_de(doc, f"5.5.{k} DFD-{ma}.0 {from_bfd}", 3)
        doan(doc, mo_ta)
        hinh(doc, sd / f"DFD_muc_1_{ma}.png", f"Hình 5.{k + 2}. DFD mức 1 của xử lý {ma}.0 – {from_bfd}")

    # Chương 5
    sang_chuong(doc, "CHƯƠNG 6. ĐẶC TẢ XỬ LÝ VÀ TỪ ĐIỂN DỮ LIỆU")
    doc_md(doc, md("phan_tich/04_dac_ta.md"), 6, bo_muc=["Đầu ra cho các bước sau"], thay=THAY_CHUNG)

    # Kết luận
    sang_chuong(doc, "KẾT LUẬN" if rieng else "TIỂU KẾT PHẦN II")
    doan(doc, "Hai giai đoạn đầu đã xác định được dự án và mô hình hóa đầy đủ hệ thống quản lý trung tâm ngoại ngữ "
              "ở cả hai mặt chức năng và dữ liệu. Các kết quả chính:")
    for t in ["Xác định 6 mục tiêu đo được; so sánh 3 phương án và chọn tự xây dựng (4,05 điểm); dự án khả thi về "
              "kinh tế (hoàn vốn 1,3–3,7 năm), kỹ thuật, tác nghiệp, pháp lý và chính trị; lập lịch 15 tuần và bảng rủi ro.",
              "Thu thập thông tin bằng sáu phương pháp, xây dựng 5 chứng từ mẫu, xác định 8 vấn đề của cách làm hiện tại "
              "và 16 quy tắc nghiệp vụ QT01–QT16.",
              "BFD gồm 5 chức năng cấp 1 và 20 chức năng lá, thỏa hai nguyên tắc thực chất và đầy đủ; ma trận thực thể – "
              "chức năng với 19 thực thể dự kiến.",
              "Hệ thống DFD ba mức (ngữ cảnh, mức 0, năm sơ đồ mức 1) với 7 tác nhân và 5 kho dữ liệu, được kiểm tra tự "
              "động về quy tắc vẽ và tính cân bằng giữa các mức.",
              "Đặc tả các xử lý mức cơ sở bằng ngôn ngữ có cấu trúc, 2 cây quyết định, 2 bảng quyết định; từ điển dữ liệu "
              "cho toàn bộ kho và luồng dữ liệu. Các ví dụ tính toán đã được đối chiếu khớp với chứng từ mẫu."]:
        p = doan(doc, "– " + t, sau=3)
        p.paragraph_format.left_indent, p.paragraph_format.first_line_indent = Cm(0.8), Cm(-0.45)
    doan(doc, "Giai đoạn tiếp theo là **thiết kế hệ thống**: xác định thực thể và lập ERD từ cấu trúc kho trong từ điển dữ "
              "liệu, chuẩn hóa đến dạng chuẩn 3 từ các chứng từ mẫu, thiết kế cơ sở dữ liệu vật lý trên SQL Server, thiết "
              "kế module theo mã chức năng và thiết kế biểu mẫu, báo cáo theo các luồng vào/ra của DFD.")
    if not rieng:
        return

    phu_luc_can_bang(doc, "PHỤ LỤC A. BẢNG CÂN BẰNG DFD")
    ra = GOC / "bao_cao" / "Bao_cao_GD1_GD2_Nhom_10.docx"
    doc.save(ra)
    print("Đã ghi", ra)
    return ra


def phu_luc_can_bang(doc, ten):
    sang_chuong(doc, ten)
    doan(doc, "Bảng được sinh tự động từ dữ liệu dùng để vẽ sơ đồ DFD, bảo đảm sơ đồ và bảng luôn khớp nhau.", italic=True)
    can_bang = (GOC / "so_do/can_bang_dfd.md").read_text(encoding="utf-8")
    doc_md(doc, "---\n" + can_bang.split("\n", 1)[1], None)


def phu_luc_mo_ta_bang(doc, ten, thay):
    sang_chuong(doc, ten)
    doan(doc, "Phụ lục được sinh tự động từ tệp tạo CSDL (schema.sql) sau khi đối chiếu với bảng thực thể ở 3.1.",
         italic=True)
    mo_ta = (GOC / "csdl/mo_ta_bang.md").read_text(encoding="utf-8").split("\n", 3)[3]
    doc_md(doc, "---\n" + re.sub(r"(?m)^## ", "#### ", mo_ta), None, thay=thay)


# ---------- báo cáo thiết kế ----------

SO = r"(\d+(?:\.\d+)*)"
CHUONG_PT = {"1": 2, "2": 3, "3": 4, "4": 6}  # phan_tich/0k_*.md -> chương trong báo cáo phân tích
TEP_PT = r"`?(?:phan_tich/)?0([1-4])_(?:ke_hoach|thu_thap|chuc_nang|dac_ta)(?:\.md)?`?"
TEP_TK = r"`?(?:thiet_ke/)?0([1-6])_(?:thuc_the|quan_he|chuan_hoa|csdl|module|giao_dien)(?:\.md)?`?"
NB = " "  # "mục" + khoảng trắng không ngắt = tham chiếu đã xử lý, doc_md không đánh số lại

THAY_TK = [
    (r"(?m) – Hệ thống quản lý trung tâm ngoại ngữ$", ""),
    (r"\s*\(KE_HOACH\.md, mục [\d.]+\)", ""),
    (r"([Bb]ài giảng,? \(?)mục ", rf"\1mục{NB}"),                                  # mục của bài giảng: giữ nguyên
    (TEP_PT + r",? mục " + SO, lambda k: f"báo cáo phân tích, mục{NB}{CHUONG_PT[k[1]]}.{k[2]}"),
    (r"\b2\.1 mục " + SO, rf"báo cáo phân tích, mục{NB}3.\1"),                     # 2.1 = thu thập thông tin
    (r"\b2\.1, GĐ1 mục " + SO, rf"báo cáo phân tích, chương 3 và mục{NB}2.\1"),
    (r"\bGĐ1 mục " + SO, rf"báo cáo phân tích, mục{NB}2.\1"),
    (r"mục (\d+), mục (\d+) của " + TEP_TK, rf"mục{NB}3.\3.\1, mục{NB}3.\3.\2"),
    (TEP_TK + r",? mục " + SO, rf"mục{NB}3.\1.\2"),
    (r"mục " + SO + r" của " + TEP_TK, rf"mục{NB}3.\2.\1"),
    (r"\b3\.([1-6]) mục " + SO, rf"mục{NB}3.\1.\2"),
    (r"\b3\.([1-6]) \(mục " + SO, rf"3.\1 (mục{NB}3.\1.\2"),
    (r"\b([Mm]ục) 3\.([1-6]) \(", rf"\1{NB}3.\2 ("),                               # "Mục 3.3 (chuẩn hóa)" = bước 3.3
    (TEP_PT, lambda k: f"báo cáo phân tích, chương {CHUONG_PT[k[1]]}"),
    (TEP_TK, rf"mục{NB}3.\1"),
]


def bao_cao_thiet_ke(doc=None):
    """doc=None: báo cáo riêng. Có doc: ghi phần nội dung vào báo cáo tổng (tham chiếu "báo cáo phân tích" → "Phần II")."""
    rieng = doc is None
    md = lambda ten: (GOC / ten).read_text(encoding="utf-8")
    thay = THAY_TK if rieng else THAY_TK + [(r"báo cáo phân tích", "Phần II"), (r"KE_HOACH", "kế hoạch dự án")]
    if rieng:
        doc = Document()
        thiet_lap_kieu(doc)
        doc.core_properties.title = "Báo cáo thiết kế hệ thống quản lý trung tâm ngoại ngữ"
        doc.core_properties.author = "Nhóm 10"
        bia(doc, "BÁO CÁO THIẾT KẾ HỆ THỐNG", "Tháng 10 năm 2026")
        so_trang(doc)
        muc_luc(doc)

    sang_chuong(doc, "MỞ ĐẦU", ngat=False)
    doan(doc, "Báo cáo trình bày kết quả **giai đoạn 3 – thiết kế hệ thống** của đề tài *Hệ thống quản lý trung tâm "
              "ngoại ngữ*. Đầu vào là báo cáo xác định và phân tích hệ thống (giai đoạn 1–2): BFD, DFD các mức, đặc tả "
              "xử lý, từ điển dữ liệu, 5 chứng từ mẫu và 16 quy tắc nghiệp vụ QT01–QT16. " +
              ("Khi báo cáo này dẫn *báo cáo phân tích, mục x.y* là chỉ mục x.y của báo cáo đó." if rieng else
               "Khi phần này dẫn *Phần II, mục x.y* là chỉ mục x.y của Phần II."))
    doan(doc, "Thiết kế gồm sáu bước theo bài giảng, mỗi bước là một chương mang mã 3.1–3.6:")
    for t in ["3.1 Xác định thực thể và thuộc tính, lập bảng thực thể.",
              "3.2 Xác định quan hệ giữa các thực thể và vẽ sơ đồ ERD.",
              "3.3 Chuẩn hóa các chứng từ mẫu đến dạng chuẩn 3 và đối chiếu với 3.1–3.2.",
              "3.4 Thiết kế cơ sở dữ liệu vật lý trên SQL Server.",
              "3.5 Thiết kế phần mềm: sơ đồ module Top-down, liên kết module – dữ liệu, ma trận phân quyền.",
              "3.6 Thiết kế giao diện: form, báo cáo, thực đơn, phác thảo màn hình, trợ giúp và thông báo lỗi."]:
        p = doan(doc, "– " + t, sau=3)
        p.paragraph_format.left_indent, p.paragraph_format.first_line_indent = Cm(0.8), Cm(-0.45)
    doan(doc, "Mã thống nhất xuyên suốt: chức năng *x.y* trên BFD = xử lý *x.y* trên DFD mức 1 = module *x.y* = mục "
              "*x.y* trên thực đơn. Các bảng, sơ đồ trong báo cáo được sinh bằng chương trình từ cùng một nguồn dữ liệu; "
              "chương trình kiểm tra các quy tắc (cân bằng DFD, nhất quán DFD – ERD, chuẩn hóa, phân quyền, ánh xạ "
              "form – luồng dữ liệu) trước khi vẽ, nên sơ đồ và bảng luôn khớp nhau.")

    for k, ten in enumerate(["01_thuc_the", "02_quan_he", "03_chuan_hoa", "04_csdl", "05_module", "06_giao_dien"], 1):
        doc_md(doc, md(f"thiet_ke/{ten}.md"), f"3.{k}", bo_muc=["Đầu ra cho các bước sau"], thay=thay,
               giu_dau=True, thu_muc=GOC / "thiet_ke")

    sang_chuong(doc, "KẾT LUẬN" if rieng else "TIỂU KẾT PHẦN III")
    doan(doc, "Giai đoạn thiết kế đã chuyển kết quả phân tích thành bản thiết kế đủ để cài đặt. Các kết quả chính:")
    for t in ["26 thực thể (1 xác thực, 9 chức năng, 10 sự kiện, 6 quan hệ) chia theo kho D1–D5, đã đối chiếu từng "
              "trường của 5 chứng từ mẫu.",
              "29 quan hệ và ERD hai hình theo ký pháp bài giảng; mọi luồng đọc, ghi kho và mọi báo cáo đều có đường đi trên ERD.",
              "Chuẩn hóa 5 chứng từ đến dạng chuẩn 3, trộn bảng; kết quả trùng với tập thực thể ở 3.1.",
              "CSDL SQL Server 26 bảng, 35 khóa ngoại, 63 ràng buộc CHECK, 16 chỉ mục, 9 view và 2 hàm cho báo cáo MIS; "
              "dữ liệu mẫu sinh tự động, 24/24 kiểm tra đạt.",
              "34 module theo mã BFD/DFD và ma trận phân quyền cho 8 vai trò.",
              "18 form ứng với luồng vào, 18 báo cáo ứng với luồng ra của DFD, thực đơn phân cấp, 4 phác thảo màn hình "
              "và 22 thông báo lỗi."]:
        p = doan(doc, "– " + t, sau=3)
        p.paragraph_format.left_indent, p.paragraph_format.first_line_indent = Cm(0.8), Cm(-0.45)
    doan(doc, "Giai đoạn tiếp theo là **cài đặt và khai thác**: xây dựng bản demo trên CSDL đã có cho các module chính, "
              "lập kế hoạch cài đặt, chuyển đổi dữ liệu, huấn luyện và kiểm thử.")
    if not rieng:
        return

    phu_luc_mo_ta_bang(doc, "PHỤ LỤC A. MÔ TẢ CÁC TỆP DỮ LIỆU", THAY_TK)
    ra = GOC / "bao_cao" / "Bao_cao_GD3_Nhom_10.docx"
    doc.save(ra)
    print("Đã ghi", ra)
    return ra


# ---------- báo cáo tổng (BT1 + GĐ1 → GĐ5) ----------

# Tham chiếu trong phan_tich/05_cai_dat.md, 06_bao_tri.md → vị trí trong báo cáo tổng (Phần IV: chương 1 = 05, chương 2 = 06)
THAY_IV = [
    (r"(?m)^# .*$", ""),                                                            # tên chương do bao_cao_tong đặt
    (r"\s*\(KE_HOACH\.md, mục Giai đoạn \d\)", ""),
    (r"`?(?:phan_tich/)?05_cai_dat(?:\.md)?`? mục " + SO, rf"mục{NB}1.\1"),
    (r"`?(?:phan_tich/)?06_bao_tri(?:\.md)?`? mục " + SO, rf"mục{NB}2.\1"),
    (r"\(`?(?:phan_tich/)?05_cai_dat(?:\.md)?`?\)", "(chương 1)"),
    (r"`?(?:phan_tich/)?05_cai_dat(?:\.md)?`?", "chương 1"),
    (r"`?(?:phan_tich/)?02_thu_thap(?:\.md)?`?,? mục " + SO, rf"Phần II, mục{NB}3.\1"),
    (r"`?(?:phan_tich/)?02_thu_thap(?:\.md)?`?", "Phần II, chương 3"),
    (r"(?:01_ke_hoach|GĐ1) mục " + SO, rf"Phần II, mục{NB}2.\1"),
    (r"`?(?:thiet_ke/)?0([5-6])_(?:module|giao_dien)(?:\.md)?`? mục " + SO, rf"Phần III, mục{NB}3.\1.\2"),
    (r"của 05_module, 06_giao_dien", "của 3.5, 3.6 (Phần III)"),
]


def chen_bt1(doc):
    """Chép nội dung Bài tập 1 (chỉ có bản Word) từ sau trang bìa; tiêu đề lùi một cấp. BT1 không có hình nên chép XML là đủ."""
    body = Document(GOC / "bai_tap_1" / "Bao_cao_Bai_tap_1_Nhom_10.docx").element.body
    phan = list(body)
    dau = next(i for i, el in enumerate(phan) if "".join(el.itertext()).startswith("Giới thiệu đề tài"))
    cuoi = doc.element.body[-1]  # sectPr
    for el in phan[dau:]:
        if el.tag == qn("w:sectPr"):
            continue
        el = copy.deepcopy(el)
        for ps in el.iter(qn("w:pStyle")):
            if m := re.fullmatch(r"Heading(\d)", ps.get(qn("w:val"))):
                ps.set(qn("w:val"), f"Heading{int(m[1]) + 1}")
        cuoi.addprevious(el)


def gach(doc, ds):
    for t in ds:
        p = doan(doc, "– " + t, sau=3)
        p.paragraph_format.left_indent, p.paragraph_format.first_line_indent = Cm(0.8), Cm(-0.45)


def bao_cao_tong():
    global LECH
    md = lambda ten: (GOC / ten).read_text(encoding="utf-8")
    doc = Document()
    thiet_lap_kieu(doc)
    doc.core_properties.title = "Báo cáo tổng kết – hệ thống quản lý trung tâm ngoại ngữ"
    doc.core_properties.author = "Nhóm 10"
    bia(doc, "BÁO CÁO TỔNG KẾT BÀI TẬP LỚN", "Tháng 10 năm 2026")
    so_trang(doc)
    muc_luc(doc)

    sang_chuong(doc, "LỜI GIỚI THIỆU", ngat=False)
    doan(doc, "Báo cáo tổng hợp toàn bộ bài tập lớn môn Phát triển hệ thống thông tin quản lý của Nhóm 10, đề tài "
              "*Hệ thống quản lý trung tâm ngoại ngữ*. Đề tài đi qua đủ năm giai đoạn của vòng đời phát triển hệ thống "
              "theo bài giảng: xác định và lập kế hoạch – phân tích – thiết kế – cài đặt và khai thác – bảo trì. "
              "Bài tập 1 (phân tích đề tài theo sáu góc nhìn) là điểm xuất phát.")
    bang(doc, ["Phần", "Giai đoạn", "Nội dung chính", "Sản phẩm kèm theo"], [
        ["I", "Bài tập 1", "Sáu góc nhìn: tổ chức, con người, quy trình, dữ liệu, công nghệ, chức năng", "–"],
        ["II", "1. Xác định, lập kế hoạch\n2. Phân tích", "Mục tiêu, phương án, khả thi, kế hoạch; thu thập thông tin, "
         "BFD, DFD ba mức, đặc tả xử lý, từ điển dữ liệu", "Sơ đồ BFD, DFD (so_do/)"],
        ["III", "3. Thiết kế", "Thực thể, ERD, chuẩn hóa, CSDL vật lý, module và phân quyền, giao diện",
         "CSDL SQL Server (csdl/)"],
        ["IV", "4. Cài đặt và khai thác\n5. Bảo trì", "Bản demo, kế hoạch cài đặt, chuyển đổi, kiểm thử; bảo trì, "
         "quản lý cấu hình, hướng phát triển", "Bản demo (demo/TrungTamNgoaiNgu), kiểm thử (demo/KiemThu)"],
    ])
    doan(doc, "Mỗi phần giữ cách đánh số chương của báo cáo giai đoạn tương ứng đã nộp; tham chiếu sang phần khác ghi rõ "
              "tên phần, ví dụ *Phần II, mục 3.9.3* là danh sách quy tắc nghiệp vụ. Mã thống nhất xuyên suốt: chức năng "
              "*x.y* trên BFD = xử lý *x.y* trên DFD mức 1 = module *x.y* = mục *x.y* trên thực đơn = thư mục mã nguồn "
              "của bản demo. Toàn bộ tài liệu nguồn, sơ đồ, CSDL và mã nguồn ở "
              "https://github.com/huynhhoang124/pt-httt-ql-nhom10.")

    phan = lambda text: setattr(doc.add_paragraph(text, style="Heading 1").paragraph_format, "page_break_before", True)
    LECH = 1
    try:
        phan("PHẦN I. TỔNG QUAN ĐỀ TÀI – BÀI TẬP 1")
        chen_bt1(doc)
        phan("PHẦN II. XÁC ĐỊNH VÀ PHÂN TÍCH HỆ THỐNG – GIAI ĐOẠN 1, 2")
        bao_cao_phan_tich(doc)
        phan("PHẦN III. THIẾT KẾ HỆ THỐNG – GIAI ĐOẠN 3")
        bao_cao_thiet_ke(doc)
        phan("PHẦN IV. CÀI ĐẶT, KHAI THÁC VÀ BẢO TRÌ – GIAI ĐOẠN 4, 5")
        sang_chuong(doc, "CHƯƠNG 1. CÀI ĐẶT VÀ KHAI THÁC", ngat=False)
        doc_md(doc, md("phan_tich/05_cai_dat.md"), 1, thay=THAY_IV, giu_dau=True)
        sang_chuong(doc, "CHƯƠNG 2. BẢO TRÌ")
        doc_md(doc, md("phan_tich/06_bao_tri.md"), 2, thay=THAY_IV, giu_dau=True)
    finally:
        LECH = 0

    sang_chuong(doc, "KẾT LUẬN")
    doan(doc, "Nhóm đã hoàn thành đủ năm giai đoạn phát triển hệ thống thông tin quản lý cho trung tâm ngoại ngữ, "
              "từ phân tích đề tài đến một bản chạy được có kiểm thử. Kết quả chính:")
    gach(doc, [
        "Xác định và phân tích: dự án khả thi về kinh tế, kỹ thuật, tác nghiệp, pháp lý; 8 vấn đề của cách làm hiện "
        "tại, 16 quy tắc nghiệp vụ; BFD 5 chức năng cấp 1 và 20 chức năng lá; DFD ba mức với 7 tác nhân, 5 kho, được "
        "kiểm tra tự động về quy tắc vẽ và cân bằng (Phụ lục A).",
        "Thiết kế: 26 thực thể và ERD; chuẩn hóa 5 chứng từ đến dạng chuẩn 3; CSDL SQL Server 26 bảng với các ràng "
        "buộc, view, hàm cho báo cáo (Phụ lục B); 34 module theo mã chức năng, ma trận phân quyền 8 vai trò; 18 form, "
        "18 báo cáo ứng với các luồng vào, ra của DFD.",
        "Cài đặt: bản demo ASP.NET Core trên CSDL đã thiết kế, gồm đăng nhập, phân quyền, nhật ký, báo lỗi và 10 màn "
        "hình chính (hồ sơ học viên, đăng ký, học phí, phiếu thu, điểm danh, nhập điểm, báo cáo). Kiểm thử: 38 ca kiểm "
        "thử đơn vị, 24 kiểm tra CSDL, kiểm tra phân quyền 8 vai trò, nhập lại chứng từ mẫu; 4 lỗi phát hiện đã sửa.",
        "Kế hoạch cài đặt theo giai đoạn (danh mục → lớp, lịch → học phí → học tập), huấn luyện 3 mức, chuyển đổi dữ "
        "liệu từ Excel; quy trình bảo trì, quản lý cấu hình bằng Git và 7 hướng phát triển.",
    ])
    doan(doc, "Bài học rút ra: dùng một bộ mã thống nhất từ BFD đến mã nguồn, và sinh sơ đồ, bảng, phân quyền bằng "
              "chương trình từ cùng một nguồn, giúp các phần do nhiều người làm luôn khớp nhau; nhiều lỗi chỉ lộ ra khi "
              "chạy với dữ liệu thật (ưu đãi hết hạn, mất dấu tiếng Việt), nên kiểm thử bằng chứng từ mẫu là bước không "
              "thể bỏ qua.")

    phu_luc_can_bang(doc, "PHỤ LỤC A. BẢNG CÂN BẰNG DFD")
    phu_luc_mo_ta_bang(doc, "PHỤ LỤC B. MÔ TẢ CÁC TỆP DỮ LIỆU", THAY_TK + [(r"báo cáo phân tích", "Phần II")])
    ra = GOC / "bao_cao" / "Bao_cao_tong_Nhom_10.docx"
    doc.save(ra)
    print("Đã ghi", ra)
    return ra


def xuat_pdf(docx):
    """Mở bằng Word để cập nhật mục lục, số trang rồi lưu lại và xuất PDF."""
    import win32com.client
    word = win32com.client.DispatchEx("Word.Application")
    word.Visible = False
    try:
        d = word.Documents.Open(str(docx))
        d.TablesOfContents(1).Update()
        d.Fields.Update()
        d.Save()
        pdf = docx.with_suffix(".pdf")
        d.SaveAs2(str(pdf), FileFormat=17)
        d.Close(False)
        print("Đã ghi", pdf)
    finally:
        try:
            word.Quit()
        except Exception:  # Word đôi khi đã tự đóng sau SaveAs2
            pass


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    chon = sys.argv[1:] or ["phan_tich", "thiet_ke", "tong"]
    for ten in chon:
        xuat_pdf({"phan_tich": bao_cao_phan_tich, "thiet_ke": bao_cao_thiet_ke, "tong": bao_cao_tong}[ten]())
