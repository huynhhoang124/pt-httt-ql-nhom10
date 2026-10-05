"""Thiết kế giao diện 3.6: kiểm tra form/report với DFD, CSDL, module, phân quyền; vẽ thực đơn và phác thảo màn hình.

Không chép lại dữ liệu: form, report, thông báo lỗi đọc từ thiet_ke/06_giao_dien.md (mục 3, 4, 6);
luồng DFD từ dfd.py; bảng, trường từ thiet_ke/01_thuc_the.md mục 8; module, quyền từ thiet_ke/05_module.md;
ràng buộc từ csdl/schema.sql; view từ csdl/views.sql; họ tên học viên mẫu từ csdl/seed.sql.
Chạy: python so_do/src/giao_dien.py  ->  kiểm tra rồi ghi so_do/Thuc_don.png, Mau_F2_2.png, Mau_F3_2.png,
      Mau_F4_1.png, Mau_R4_3.png
"""
import re
import sys
from pathlib import Path
from textwrap import fill

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle, FancyBboxPatch, Circle

from bfd import CHUC_NANG, hop
from dfd import MUC_1
import module as mo

THU_MUC = Path(__file__).resolve().parents[1]
DU_AN = THU_MUC.parent
VAI_TRO = ["HV", "PH", "GV", "TS", "KT", "DT", "GD", "QT"]
TAC_NHAN = {"HV": "HV", "PH": "HV", "GV": "GV", "TS": "TS", "KT": "KT", "DT": "DT", "GD": "GD", "QT": "QT"}
XAM = "#d9d9d9"


# ------------------------------ đọc tài liệu ------------------------------

def bang_md(phan):
    return [mo.o(d) for d in phan.splitlines() if d.startswith("| ") and not d.startswith("|---")][1:]


def doc():
    md = (DU_AN / "thiet_ke/06_giao_dien.md").read_text(encoding="utf-8")
    form = {c[0]: dict(ten=c[1], module=[m.strip() for m in c[2].split(",")], luong=c[3], dung=c[4], kieu=c[5], truong=c[6])
            for c in bang_md(mo.muc(md, 3)) if len(c) == 7}
    bao_cao = {c[0]: dict(ten=c[1], module=[m.strip() for m in c[2].split(",")], luong=c[3], dung=c[4], loai=c[5],
                          xu_ly=c[6], nguon=c[7]) for c in bang_md(mo.muc(md, 4)) if len(c) == 8}
    loi = [dict(nguon=c[0], man_hinh=c[1], thong_bao=c[2]) for c in bang_md(mo.muc(md, 6).split("**Thông báo lỗi**")[1])
           if len(c) == 4]
    return form, bao_cao, loi


def luong(s):
    """'TS: Phiếu đăng ký; HV: Yêu cầu đăng ký' -> [('TS', 'Phiếu đăng ký'), ...]"""
    return [] if s.strip() == "–" else [tuple(x.strip() for x in p.split(":", 1)) for p in s.split(";")]


def truong(s):
    """'HocVien: HoTen, NgaySinh; Tham số: ...' -> {'HocVien': [...]} (bỏ tham số không lưu)"""
    kq = {}
    for p in s.split(";"):
        bang, cot = (x.strip() for x in p.split(":", 1))
        if bang != "Tham số":
            kq[bang] = [c.strip() for c in cot.split(",")]
    return kq


# ------------------------------ kiểm tra ------------------------------

def kiem_tra(form, bao_cao, loi):
    module, du_lieu, quyen, kho, view = mo.doc()
    thuc_the = {t: a.split(", ") for t, a in re.findall(
        r"^\| \d+ \| [^|]+ \| (\w+) \| [^|]+ \| D\d \| ([^|]+) \|",
        mo.muc((DU_AN / "thiet_ke/01_thuc_the.md").read_text(encoding="utf-8"), 8), re.M)}
    thuc_the = {t: [x.strip().lstrip("#") for x in a] for t, a in thuc_the.items()}
    kq = []

    # 1. form/report <-> luồng tác nhân của DFD mức 1
    da_phu = set()
    for loai, ds, huong in (("form", form, "vao"), ("báo cáo", bao_cao, "ra")):
        for ma, f in ds.items():
            assert all(m in module for m in f["module"]), f"{ma}: module lạ {f['module']}"
            if all(m[0] == "N" for m in f["module"]):
                continue
            ls = luong(f["luong"])
            assert ls, f"{ma}: {loai} mồ côi – không gắn với luồng DFD nào"
            for tn, ten in ls:
                khop = [(l[0], l[1], l[2], l[3]) for m in f["module"] for l in MUC_1[m[0] + ".0"]["tn"]
                        if l[0] == tn and l[1] == m and l[2] == huong and l[3] == ten]
                assert khop, f"{ma}: không có luồng {huong} '{tn}: {ten}' ở module {f['module']} trên DFD mức 1"
                da_phu |= set(khop)
    tat_ca = {(l[0], l[1], l[2], l[3]) for d in MUC_1.values() for l in d["tn"]}
    assert da_phu == tat_ca, f"Luồng DFD chưa có form/báo cáo: {sorted(tat_ca - da_phu)}"
    kq.append(f"Cả {len(tat_ca)} luồng giữa tác nhân và hệ thống trên DFD mức 1 đều có form (luồng vào) hoặc báo cáo "
              f"(luồng ra); không có form, báo cáo nào mồ côi.")

    # 2. trường nhập có trong CSDL và thuộc bảng mà module được ghi; người dùng có quyền
    so_truong = 0
    for ma, f in form.items():
        tr = truong(f["truong"])
        ghi = {t for m in f["module"] for t in du_lieu[m]["ghi"]}
        for bang, cot in tr.items():
            assert bang in thuc_the, f"{ma}: không có bảng {bang}"
            assert set(cot) <= set(thuc_the[bang]), f"{ma}: {bang} không có trường {set(cot) - set(thuc_the[bang])}"
            assert bang in ghi, f"{ma}: module {f['module']} không được ghi bảng {bang} (3.5 mục 4)"
            so_truong += len(cot)
        for vt in (v.strip() for v in f["dung"].split(",")):
            assert vt in VAI_TRO, f"{ma}: vai trò lạ {vt}"
            q = set().union(*(quyen.get(m, {}).get(vt, set()) for m in f["module"]))
            can = mo.NHAP if tr and not all(m[0] == "N" for m in f["module"]) else set("TSDIXx")
            assert q & can, f"{ma}: vai trò {vt} không có quyền {''.join(sorted(can))} ở {f['module']} (3.5 mục 5)"
    kq.append(f"{len(form)} form, {so_truong} trường nhập: mọi trường có trong CSDL và thuộc bảng mà module được ghi; "
              f"người dùng mỗi form có quyền nhập (T/S/D) ở module đó.")

    # 3. báo cáo: nguồn nằm trong dữ liệu module đọc được, view có thật, người nhận có quyền xem
    for ma, r in bao_cao.items():
        doc_duoc = {t for m in r["module"] for k in ("ghi", "doc", "tra_cuu") for t in du_lieu[m][k]}
        for x in (x.strip() for x in r["nguon"].replace(";", ",").split(",")):
            if x.startswith(("v_", "fn_")):
                assert x in view, f"{ma}: không có view/hàm {x}"
            else:
                assert x in doc_duoc, f"{ma}: module {r['module']} không đọc bảng {x} (3.5 mục 4)"
        for vt in (v.strip() for v in r["dung"].split(",")):
            assert any(quyen[m][vt] for m in r["module"]), f"{ma}: {vt} không có quyền xem ở {r['module']}"
    kq.append(f"{len(bao_cao)} báo cáo: nguồn dữ liệu nằm trong các bảng, view mà module đọc được; người nhận có quyền xem.")

    # 4. mọi module chức năng có ít nhất một màn hình
    co = {m for d in (form, bao_cao) for f in d.values() for m in f["module"]}
    thieu = [m for m in module if m[0] != "N" and m not in co]
    assert not thieu, f"Module chưa có form/báo cáo nào: {thieu}"
    kq.append("Cả 20 module chức năng đều có ít nhất một form hoặc báo cáo (thực đơn không có mục rỗng).")

    # 5. thông báo lỗi trỏ về ràng buộc / quy tắc có thật và màn hình có thật
    schema = (DU_AN / "csdl/schema.sql").read_text(encoding="utf-8")
    qt = set(re.findall(r"^\| (QT\d\d) \|", (DU_AN / "phan_tich/02_thu_thap.md").read_text(encoding="utf-8"), re.M))
    for l in loi:
        if l["nguon"].startswith("QT"):
            assert l["nguon"] in qt, f"Quy tắc lạ {l['nguon']}"
        else:
            assert re.search(rf"CONSTRAINT {l['nguon']}\b", schema), f"Không có ràng buộc {l['nguon']} trong schema.sql"
        assert l["man_hinh"] in form or l["man_hinh"] in bao_cao, f"Màn hình lạ {l['man_hinh']}"
    kq.append(f"{len(loi)} thông báo lỗi đều trỏ về ràng buộc có trong csdl/schema.sql hoặc quy tắc QT có trong 2.1, "
              f"và về màn hình có thật.")
    return kq


# ------------------------------ thực đơn ------------------------------

def ve_thuc_don(form, bao_cao):
    muc = {**{p: (ten, [f"{p[0]}.{i}. {c}" for i, c in enumerate(cons, 1)]) for p, (ten, cons) in CHUC_NANG.items()}}
    W, GAP = 4.7, 0.45
    cot = list(muc.items()) + [("CN", ("Cá nhân", []))]
    tong = len(cot) * W + (len(cot) - 1) * GAP

    def man_hinh(m):
        ds = [(k, v["ten"]) for d in (form, bao_cao) for k, v in d.items() if m in v["module"]]
        return sorted(ds, key=lambda x: (x[0][0] != "F", x[0]))

    # chiều cao mỗi ô module theo số dòng chữ
    def noi_dung(m, ten):
        dong = [fill(ten, 30)] + [fill(f"{k} {t}", 35, subsequent_indent="   ") for k, t in man_hinh(m)]
        return dong

    cao_cot = []
    for ma, (ten, cons) in cot:
        h = 0
        for c in cons:
            m = c.split(".")[0] + "." + c.split(".")[1]
            h += 0.37 * sum(x.count("\n") + 1 for x in noi_dung(m, c)) + 0.45
        cao_cot.append(h)
    cao = max(cao_cot) + 3.9
    fig, ax = plt.subplots(figsize=(tong * 0.62, cao * 0.62))
    y0 = cao - 1.2
    hop(ax, tong / 2 - 4.6, y0, 9.2, 0.9, "THỰC ĐƠN CHÍNH (thanh thực đơn trên cùng)", dam=True, co=11)
    ax.plot([tong / 2] * 2, [y0, y0 - 0.4], color="black", lw=1.2)
    ax.plot([W / 2, tong - W / 2], [y0 - 0.4] * 2, color="black", lw=1.2)
    for i, (ma, (ten, cons)) in enumerate(cot):
        x = i * (W + GAP)
        ax.plot([x + W / 2] * 2, [y0 - 0.4, y0 - 0.7], color="black", lw=1.2)
        hop(ax, x, y0 - 1.55, W, 0.85, fill(f"{ma}. {ten}" if ma != "CN" else "Cá nhân", 22), dam=True, co=10)
        y = y0 - 1.75
        if ma == "CN":
            dong = ["FN1 Đăng nhập, đổi mật khẩu", "R5.2 Thông báo của tôi"]
            h = 0.37 * len(dong) + 0.3
            ax.add_patch(Rectangle((x, y - h), W, h, fill=False, lw=1.1))
            ax.text(x + 0.12, y - 0.15, "\n".join(dong), va="top", fontsize=8.2, linespacing=1.45)
            continue
        for c in cons:
            m = ".".join(c.split(".")[:2])
            dong = noi_dung(m, c)
            h = 0.37 * sum(d.count("\n") + 1 for d in dong) + 0.3
            ax.add_patch(Rectangle((x, y - h), W, h, fill=False, lw=1.1))
            ax.add_patch(Rectangle((x, y - 0.37 * (dong[0].count("\n") + 1) - 0.18), W, 0.37 * (dong[0].count("\n") + 1) + 0.18,
                                   fc=XAM, ec="black", lw=1.1))
            ax.text(x + 0.12, y - 0.1, dong[0], va="top", fontsize=8.6, weight="bold", linespacing=1.3)
            ax.text(x + 0.12, y - 0.37 * (dong[0].count("\n") + 1) - 0.3, "\n".join(dong[1:]), va="top", fontsize=8.2,
                    linespacing=1.45)
            y -= h + 0.15
    ax.text(0, 0.1, "F = form nhập liệu (luồng vào DFD), R = báo cáo/chứng từ (luồng ra DFD). Mỗi vai trò chỉ thấy các mục "
            "mà ma trận phân quyền 3.5 cho phép.", fontsize=8.5, style="italic")
    ax.set_xlim(-0.2, tong + 0.2)
    ax.set_ylim(-0.2, cao)
    ax.set_aspect("equal")
    ax.axis("off")
    luu(fig, "Thuc_don.png")


# ------------------------------ phác thảo màn hình (đen trắng) ------------------------------

def luu(fig, ten):
    fig.savefig(THU_MUC / ten, dpi=160, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    print("Đã ghi", THU_MUC / ten)


def ten_hv():
    seed = (DU_AN / "csdl/seed.sql").read_text(encoding="utf-8")
    khoi = seed.split("INSERT INTO HocVien ", 1)[1].split(";", 1)[0]   # chỉ lấy khối chèn bảng HocVien
    return dict(re.findall(r"\(N'(HV\d{4})', N'([^']+)'", khoi))


class Man:
    """Khung một màn hình phác thảo: tọa độ tính bằng đơn vị tùy ý, gốc ở góc trên trái."""

    def __init__(self, w, h, tieu_de, duong_dan, ten_file, dien_thoai=False):
        self.w, self.h, self.ten = w, h, ten_file
        self.fig, self.ax = plt.subplots(figsize=(w * 0.55, h * 0.55))
        ax = self.ax
        bo = 0.6 if dien_thoai else 0.15
        ax.add_patch(FancyBboxPatch((0, -h), w, h, boxstyle=f"round,pad=0,rounding_size={bo}", fill=False, lw=2))
        ax.add_patch(Rectangle((0, -0.75), w, 0.75, fc="black", ec="black"))
        ax.text(0.3, -0.38, "Trung tâm ngoại ngữ – Hệ thống quản lý" if not dien_thoai else "TTNN", color="white",
                va="center", fontsize=10, weight="bold")
        ax.text(w - 0.3, -0.38, "Đăng xuất" if not dien_thoai else "≡", color="white", va="center", ha="right", fontsize=9)
        ax.text(0.3, -1.15, duong_dan, va="center", fontsize=8.5, style="italic")
        ax.text(0.3, -1.75, tieu_de, va="center", fontsize=12.5 if not dien_thoai else 11, weight="bold")
        ax.plot([0.3, w - 0.3], [-2.15, -2.15], color="black", lw=0.8)

    def truong(self, x, y, nhan, gia_tri, w=4.0, ghi_chu=None, bat_buoc=False, chon=False, khoa=False):
        ax = self.ax
        ax.text(x, y, nhan + (" *" if bat_buoc else ""), va="center", fontsize=8.8)
        ax.add_patch(Rectangle((x, y - 0.95), w, 0.6, fc=XAM if khoa else "white", ec="black", lw=0.9))
        ax.text(x + 0.12, y - 0.65, gia_tri, va="center", fontsize=9)
        if chon:
            ax.text(x + w - 0.15, y - 0.65, "▾", va="center", ha="right", fontsize=10)
        if ghi_chu:
            ax.text(x, y - 1.25, ghi_chu, va="center", fontsize=7.8, style="italic")

    def bang(self, x, y, cot, rong, dong, cao=0.55, to_dong=()):
        ax = self.ax
        xs = [x]
        for r in rong:
            xs.append(xs[-1] + r)
        ax.add_patch(Rectangle((x, y - cao), xs[-1] - x, cao, fc=XAM, ec="black", lw=0.9))
        for i, c in enumerate(cot):
            ax.text(xs[i] + 0.1, y - cao / 2, c, va="center", fontsize=8.3, weight="bold")
        for j, d in enumerate(dong):
            yy = y - cao * (j + 2)
            ax.add_patch(Rectangle((x, yy), xs[-1] - x, cao, fc="#f2f2f2" if j in to_dong else "white", ec="black", lw=0.6))
            for i, v in enumerate(d):
                ax.text(xs[i] + 0.1, yy + cao / 2, v, va="center", fontsize=8.3)
        for xv in xs[1:-1]:
            ax.plot([xv, xv], [y, y - cao * (len(dong) + 1)], color="black", lw=0.5)
        return y - cao * (len(dong) + 1)

    def nut(self, x, y, chu, chinh=False, w=None):
        w = w or 0.22 * len(chu) + 0.6
        self.ax.add_patch(FancyBboxPatch((x, y - 0.6), w, 0.6, boxstyle="round,pad=0,rounding_size=0.12",
                                         fc="black" if chinh else "white", ec="black", lw=1))
        self.ax.text(x + w / 2, y - 0.3, chu, ha="center", va="center", fontsize=8.8, color="white" if chinh else "black",
                     weight="bold" if chinh else "normal")
        return x + w + 0.25

    def tro_giup(self, chu):
        self.ax.plot([0.3, self.w - 0.3], [-self.h + 0.85, -self.h + 0.85], color="black", lw=0.6)
        self.ax.text(0.3, -self.h + 0.45, chu, va="center", fontsize=8, style="italic")

    def xong(self):
        self.ax.set_xlim(-0.1, self.w + 0.1)
        self.ax.set_ylim(-self.h - 0.1, 0.1)
        self.ax.set_aspect("equal")
        self.ax.axis("off")
        luu(self.fig, self.ten)


def mau_f2_2():
    m = Man(22, 12.8, "F2.2  Phiếu đăng ký học", "2.0 Lớp học và lịch học  ›  2.2 Đăng ký học, xếp học viên vào lớp",
            "Mau_F2_2.png")
    m.truong(0.3, -2.6, "Số phiếu", "DK2026-0158", 3.6, "tự sinh khi lưu", khoa=True)
    m.truong(4.4, -2.6, "Ngày đăng ký", "05/09/2026", 3.4, bat_buoc=True)
    m.truong(8.3, -2.6, "Nhân viên tiếp nhận", "Trần Thị Hoa", 4.4, "lấy từ phiên đăng nhập", khoa=True)
    m.truong(0.3, -4.4, "Học viên (gõ mã, tên hoặc số điện thoại)", "HV0412 – Nguyễn Minh Anh", 8.0, bat_buoc=True, chon=True)
    m.ax.text(8.6, -5.05, "Sinh 14/03/2012 · Trình độ A2 (02/09/2026) · Phụ huynh: Nguyễn Văn Bình (Bố)\n"
              "Học viên cũ (đã học hết IES-2603) → xét ưu đãi QT01 ở 3.1", va="center", fontsize=8.3,
              linespacing=1.5)
    m.ax.text(0.3, -6.3, "Các lớp đăng ký", fontsize=10, weight="bold", va="center")
    y = m.bang(0.3, -6.65, ["STT", "Mã lớp", "Khóa học", "Lịch học", "Khai giảng", "Sĩ số", "Học phí", ""],
               [0.9, 2.2, 4.3, 3.6, 2.2, 1.7, 2.4, 1.2],
               [["1", "IEK-2609 ▾", "IELTS Kids Foundation", "T3, T5 18:00–19:30", "15/09/2026", "7 / 16", "4.800.000", "Bỏ"],
                ["2", "GT-2604 ▾", "Giao tiếp cơ bản", "T7 08:00–10:00", "19/09/2026", "12 / 20", "2.400.000", "Bỏ"],
                ["", "+ Thêm lớp", "", "", "", "", "", ""]])
    m.ax.text(21.7, y - 0.5, "Tổng học phí (trước ưu đãi):  7.200.000 đ", ha="right", fontsize=9.5, weight="bold", va="center")
    m.ax.text(21.7, y - 1.05, "Ưu đãi và số đợt do kế toán xác nhận ở F3.1; phải nộp dự kiến 6.480.000 đ", ha="right",
              fontsize=8, style="italic", va="center")
    x = 0.3
    for chu, chinh in (("Lưu", True), ("Lưu nháp (Ctrl+S)", False), ("In phiếu", False), ("Hủy", False), ("Thoát (Esc)", False)):
        x = m.nut(x, y - 1.8, chu, chinh)
    m.ax.text(0.3, y - 0.5, "Sĩ số = số đăng ký giữ chỗ / sĩ số tối đa", fontsize=7.8, style="italic", va="center")
    m.tro_giup("F1 Trợ giúp  ·  Chỉ chọn được lớp Dự kiến hoặc Đang học còn chỗ (QT06); lớp đã đủ thì hệ thống gợi ý "
               "lớp cùng khóa còn chỗ  ·  * trường bắt buộc")
    m.xong()


def mau_f3_2():
    m = Man(22, 14.0, "F3.2  Phiếu thu học phí", "3.0 Quản lý học phí  ›  3.2 Lập phiếu thu", "Mau_F3_2.png")
    m.truong(0.3, -2.6, "Số phiếu thu", "PT2026-0731", 3.6, "tự sinh khi lập", khoa=True)
    m.truong(4.4, -2.6, "Ngày thu", "06/09/2026", 3.4, bat_buoc=True)
    m.truong(8.3, -2.6, "Người thu", "Lê Thu Trang", 4.4, "lấy từ phiên đăng nhập", khoa=True)
    m.truong(0.3, -4.4, "Người nộp", "Nguyễn Văn Bình", 7.0, bat_buoc=True)
    m.ax.text(7.9, -4.4, "Hình thức *", fontsize=8.8, va="center")
    for i, (chu, chon) in enumerate((("Chuyển khoản", True), ("Tiền mặt", False))):
        cx = 8.1 + i * 3.4
        m.ax.add_patch(Circle((cx, -5.05), 0.17, fill=chon, fc="black", ec="black", lw=1))
        m.ax.text(cx + 0.35, -5.05, chu, va="center", fontsize=9)
    m.ax.text(0.3, -6.3, "Các đợt cần nộp (học viên HV0412 – Nguyễn Minh Anh, phiếu DK2026-0158)", fontsize=10,
              weight="bold", va="center")
    y = m.bang(0.3, -6.65, ["Mã lớp", "Đợt", "Phải nộp đợt", "Đã nộp", "Còn nợ", "Hạn đóng", "Nộp lần này"],
               [2.6, 1.2, 3.0, 2.4, 2.8, 2.8, 3.2],
               [["IEK-2609", "1", "2.160.000", "0", "2.160.000", "15/09/2026", "2.160.000"],
                ["IEK-2609", "2", "2.160.000", "0", "2.160.000", "27/10/2026", "2.160.000"],
                ["GT-2604", "1", "1.080.000", "0", "1.080.000", "19/09/2026", "1.080.000"],
                ["GT-2604", "2", "1.080.000", "0", "1.080.000", "17/10/2026", "0"],
                ["+ Thêm", "", "", "", "", "", ""]], to_dong=(3,))
    m.ax.text(0.3, y - 0.5, "+ Thêm: thêm đợt của học viên khác (một phụ huynh nộp cho nhiều con)", fontsize=7.8,
              style="italic", va="center")
    m.ax.text(21.7, y - 0.5, "Tổng nộp lần này:  5.400.000 đ", ha="right", fontsize=10, weight="bold", va="center")
    m.ax.text(21.7, y - 1.05, "(Năm triệu bốn trăm nghìn đồng)   ·   Còn nợ sau phiếu này: 1.080.000 đ – hạn 17/10/2026",
              ha="right", fontsize=8.5, style="italic", va="center")
    x = 0.3
    for chu, chinh in (("Lập phiếu thu", True), ("Lưu nháp (Ctrl+S)", False), ("In 2 liên", False), ("Hủy", False)):
        x = m.nut(x, y - 1.8, chu, chinh)
    m.tro_giup("F1 Trợ giúp  ·  Đợt 1 tối thiểu 50% (QT05); không nộp vượt số còn nợ của đợt  ·  Phiếu đã lập không sửa được, "
               "hệ thống hỏi xác nhận trước khi lập")
    m.xong()


def mau_f4_1():
    ten = ten_hv()
    m = Man(9.4, 17.5, "F4.1  Điểm danh", "4.0 Học tập › 4.1 Điểm danh", "Mau_F4_1.png", dien_thoai=True)
    m.truong(0.3, -2.6, "Lớp", "IEK-2609", 4.0, chon=True)
    m.truong(4.6, -2.6, "Buổi", "3 · 22/09/2026", 4.5, chon=True)
    m.ax.text(0.3, -4.0, "18:00–19:30 · Phòng P203 · GV Phạm Quốc Huy", fontsize=8.3, va="center", style="italic")
    hv = [("HV0412", "x", None), ("HV0398", "K", "Nhắc phụ huynh"), ("HV0390", "x", None), ("HV0391", "M", None),
          ("HV0413", "x", None), ("HV0414", "P", None)]
    y = -4.5
    for ma, tt, gc in hv:
        cao = 1.75 if gc else 1.25
        m.ax.add_patch(Rectangle((0.3, y - cao), 8.8, cao, fill=False, lw=0.7))
        m.ax.text(0.5, y - 0.38, f"{ma}  {ten.get(ma, '')}", va="center", fontsize=8.8, weight="bold")
        for i, k in enumerate("xMPK"):
            bx = 0.5 + i * 1.45
            m.ax.add_patch(Rectangle((bx, y - 1.1), 1.2, 0.5, fc="black" if k == tt else "white", ec="black", lw=0.8))
            m.ax.text(bx + 0.6, y - 0.85, k, ha="center", va="center", fontsize=9, color="white" if k == tt else "black",
                      weight="bold")
        if gc:
            m.ax.text(0.5, y - 1.45, "✎ Ghi chú: " + gc, va="center", fontsize=7.8, style="italic")
        y -= cao + 0.12
    m.ax.text(0.3, y - 0.35, "x có mặt · M đi muộn · P vắng có phép · K vắng không phép", fontsize=7.8, va="center")
    m.ax.text(0.3, y - 0.7, "Có mặt 4 / 6. HV0398 đã vắng > 20%:\nhệ thống sẽ gửi cảnh báo (QT12)", fontsize=7.8,
              va="top", weight="bold", linespacing=1.4)
    m.nut(0.3, y - 1.95, "Lưu điểm danh", True, w=8.8)
    m.tro_giup("Sửa được trong 24 giờ sau buổi học (QT11)")
    m.xong()


def mau_r4_3():
    m = Man(22, 13.2, "R4.3a  Bảng điểm lớp chờ duyệt   ·   F4.3  Duyệt kết quả",
            "4.0 Quản lý học tập  ›  4.3 Tổng kết, xếp loại", "Mau_R4_3.png")
    m.ax.text(0.3, -2.6, "Lớp IEK-2609 · IELTS Kids Foundation (A2 → B1) · GV Phạm Quốc Huy · Trọng số 10% – 30% – 60%\n"
              "Lập lúc 18/12/2026 08:00 · 26 buổi đã dạy · Trạng thái: chờ duyệt", va="top", fontsize=9, linespacing=1.6)
    y = m.bang(0.3, -3.9, ["Mã HV", "Họ tên", "Chuyên cần", "Giữa kỳ", "Cuối kỳ", "Tổng kết", "Xếp loại", "Kết quả"],
               [1.9, 4.4, 2.2, 1.9, 1.9, 2.1, 2.4, 2.5],
               [["HV0412", "Nguyễn Minh Anh", "9.2 (92%)", "7.5", "8.0", "8.0", "Khá", "Đạt"],
                ["HV0398", "Trần Gia Bảo", "6.2 (62%)", "5.0", "4.0", "4.5", "Không đạt", "Không đạt"],
                ["…", "(7 học viên khác)", "", "", "", "", "", ""]], to_dong=(1,))
    m.ax.text(0.3, y - 0.45, "Chuyên cần, tổng kết, xếp loại do hệ thống tính (QT13–QT15, bảng quyết định 4.3); "
              "dòng tô xám là học viên không đạt.", fontsize=8, style="italic", va="center")
    x = 0.3
    for chu, chinh in (("Duyệt kết quả", True), ("Yêu cầu xem lại", False), ("In bảng điểm", False), ("Xuất Excel", False)):
        x = m.nut(x, y - 1.0, chu, chinh)
    # hộp đối thoại xác nhận (thiết kế đối thoại)
    dx, dy, dw, dh = 9.5, y - 1.9, 11.8, 3.6
    m.ax.add_patch(Rectangle((dx + 0.12, dy - dh - 0.12), dw, dh, fc="#bfbfbf", ec="none"))
    m.ax.add_patch(Rectangle((dx, dy - dh), dw, dh, fc="white", ec="black", lw=1.5))
    m.ax.add_patch(Rectangle((dx, dy - 0.6), dw, 0.6, fc="black"))
    m.ax.text(dx + 0.25, dy - 0.3, "Xác nhận duyệt", color="white", va="center", fontsize=9.5, weight="bold")
    m.ax.text(dx + 0.25, dy - 0.9, "Duyệt kết quả của 9 học viên lớp IEK-2609?\nSau khi duyệt: điểm bị khóa, kết quả được gửi "
              "học viên,\nphụ huynh (5.2) và chứng nhận được cấp cho 8 học viên đạt (4.4).", va="top", fontsize=8.6,
              linespacing=1.5)
    bx = dx + dw - 4.6
    bx = m.nut(bx, dy - dh + 0.85, "Duyệt", True, w=1.9)
    m.nut(bx, dy - dh + 0.85, "Hủy", False, w=1.9)
    m.tro_giup("F1 Trợ giúp  ·  Chỉ QL đào tạo thấy nút Duyệt (ma trận 3.5: D)  ·  Thao tác được ghi nhật ký (N3)")
    m.xong()


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    form, bao_cao, loi = doc()
    for d in kiem_tra(form, bao_cao, loi):
        print("  -", d)
    print("Kiểm tra thiết kế giao diện: đạt")
    ve_thuc_don(form, bao_cao)
    mau_f2_2()
    mau_f3_2()
    mau_f4_1()
    mau_r4_3()
