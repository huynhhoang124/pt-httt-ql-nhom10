"""DFD mức ngữ cảnh và mức 0, sinh từ cùng một bộ luồng nên luôn cân bằng.

LUONG_TN và LUONG_KHO là nguồn duy nhất; sơ đồ ngữ cảnh gộp LUONG_TN theo tác nhân.
Chạy: python so_do/src/dfd.py  ->  kiểm tra quy tắc, ghi DFD_muc_ngu_canh.png,
DFD_muc_0.png và can_bang_dfd.md trong so_do/.
"""
import sys
from pathlib import Path
from textwrap import fill

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, Rectangle

from bfd import CHUC_NANG

THU_MUC = Path(__file__).resolve().parents[1]

TAC_NHAN = {
    "HV": "Học viên /\nPhụ huynh",
    "TS": "Nhân viên tuyển sinh /\nChăm sóc học viên",
    "KT": "Nhân viên kế toán",
    "GV": "Giáo viên",
    "DT": "Quản lý đào tạo",
    "QT": "Quản trị viên",
    "GD": "Giám đốc",
}
KHO = {
    "D1": "Hồ sơ",
    "D2": "Lớp và lịch học",
    "D3": "Học phí",
    "D4": "Học tập",
    "D5": "Tài khoản và thông báo",
}

# (tác nhân, tiến trình, hướng, tên luồng); "vao" = tác nhân -> hệ thống, "ra" = hệ thống -> tác nhân
LUONG_TN = [
    ("TS", "1.0", "vao", "Hồ sơ học viên, kết quả kiểm tra đầu vào"),
    ("DT", "1.0", "vao", "Thông tin khóa học, giáo viên, phòng học"),
    ("HV", "2.0", "vao", "Yêu cầu đăng ký, chuyển lớp, bảo lưu"),
    ("TS", "2.0", "vao", "Phiếu đăng ký, đơn chuyển lớp, bảo lưu"),
    ("DT", "2.0", "vao", "Kế hoạch mở lớp, phân công giáo viên"),
    ("TS", "2.0", "ra", "Kết quả đăng ký, danh sách lớp"),
    ("HV", "2.0", "ra", "Lịch học"),
    ("GV", "2.0", "ra", "Lịch dạy, danh sách lớp"),
    ("HV", "3.0", "vao", "Tiền học phí"),
    ("KT", "3.0", "vao", "Thông tin thu tiền, ưu đãi"),
    ("GD", "3.0", "vao", "Yêu cầu báo cáo doanh thu"),
    ("HV", "3.0", "ra", "Phiếu thu"),
    ("KT", "3.0", "ra", "Học phí phải thu, phiếu thu, công nợ"),
    ("GD", "3.0", "ra", "Báo cáo doanh thu, công nợ"),
    ("GV", "4.0", "vao", "Điểm danh, điểm số, nhận xét"),
    ("HV", "4.0", "ra", "Chuyên cần, kết quả học tập, chứng nhận"),
    ("QT", "5.0", "vao", "Tài khoản, phân quyền, cấu hình"),
    ("GD", "5.0", "vao", "Yêu cầu báo cáo tổng hợp"),
    ("DT", "5.0", "ra", "Báo cáo lớp học, chuyên cần, kết quả, giảng dạy"),
    ("HV", "5.0", "ra", "Thông báo"),
    ("GD", "5.0", "ra", "Báo cáo tuyển sinh, kết quả học tập"),
    ("QT", "5.0", "ra", "Nhật ký hệ thống"),
]

# (kho, tiến trình, hướng, tên luồng); "ghi" = tiến trình -> kho, "doc" = kho -> tiến trình
LUONG_KHO = [
    ("D1", "1.0", "ghi", "Hồ sơ cập nhật"),
    ("D1", "1.0", "doc", "Hồ sơ hiện có"),
    ("D1", "2.0", "doc", "Học viên, giáo viên, khóa học, phòng học"),
    ("D2", "2.0", "ghi", "Lớp, đăng ký, lịch học"),
    ("D2", "2.0", "doc", "Sĩ số, lịch đã xếp"),
    ("D3", "2.0", "doc", "Tình trạng đóng học phí"),
    ("D1", "3.0", "doc", "Học phí khóa học"),
    ("D2", "3.0", "doc", "Đăng ký học"),
    ("D3", "3.0", "ghi", "Ưu đãi, học phí, phiếu thu"),
    ("D3", "3.0", "doc", "Số đã thu, công nợ"),
    ("D2", "4.0", "doc", "Danh sách lớp, buổi học"),
    ("D4", "4.0", "ghi", "Điểm danh, điểm, kết quả"),
    ("D4", "4.0", "doc", "Điểm danh, điểm thành phần"),
    ("D1", "5.0", "doc", "Hồ sơ học viên, giáo viên"),
    ("D2", "5.0", "doc", "Lớp, đăng ký, buổi học"),
    ("D3", "5.0", "doc", "Công nợ"),
    ("D4", "5.0", "doc", "Chuyên cần, kết quả"),
    ("D5", "5.0", "ghi", "Tài khoản, thông báo, nhật ký"),
    ("D5", "5.0", "doc", "Quyền truy cập"),
]


def kiem_tra():
    """Quy tắc DFD trong ly_thuyet_ap_dung.md mục 2.3. Tác nhân/kho không nối trực tiếp
    với nhau vì mỗi luồng luôn có một tiến trình ở một đầu."""
    for tn, p, h, _ in LUONG_TN:
        assert tn in TAC_NHAN and p in CHUC_NANG and h in ("vao", "ra"), (tn, p, h)
    for k, p, h, _ in LUONG_KHO:
        assert k in KHO and p in CHUC_NANG and h in ("ghi", "doc"), (k, p, h)
    for p in CHUC_NANG:
        vao = {n for _, q, h, n in LUONG_TN if q == p and h == "vao"} | \
              {n for _, q, h, n in LUONG_KHO if q == p and h == "doc"}
        ra = {n for _, q, h, n in LUONG_TN if q == p and h == "ra"} | \
             {n for _, q, h, n in LUONG_KHO if q == p and h == "ghi"}
        assert vao and ra, f"Tiến trình {p} thiếu luồng vào hoặc ra"
        assert not vao & ra, f"Tiến trình {p} có luồng ra trùng luồng vào: {vao & ra}"
    for k in KHO:
        hs = {h for x, _, h, _ in LUONG_KHO if x == k}
        assert hs == {"ghi", "doc"}, f"Kho {k} phải có cả luồng vào và luồng ra"
    for tn in TAC_NHAN:
        assert any(x == tn for x, *_ in LUONG_TN), f"Tác nhân {tn} không có luồng"
    print("Kiểm tra quy tắc DFD: đạt")


def ngu_canh(tn, h):
    """Luồng ở mức ngữ cảnh = gộp các luồng mức 0 của tác nhân theo hướng (bảo đảm cân bằng)."""
    return list(dict.fromkeys(n for x, _, hh, n in LUONG_TN if x == tn and hh == h))


# ---------- ký hiệu Gane & Sarson ----------

def tac_nhan(ax, x, y, w, h, ma, lap=False):
    ax.add_patch(Rectangle((x, y - h / 2), w, h, fill=False, lw=1.5))
    ax.text(x + w / 2, y, TAC_NHAN[ma], ha="center", va="center", fontsize=9.5, weight="bold")
    if lap:  # tác nhân vẽ lặp: gạch chéo góc dưới phải
        ax.plot([x + w - 0.35, x + w], [y - h / 2, y - h / 2 + 0.35], color="black", lw=1)


def tien_trinh(ax, cx, cy, w, h, ma, ten, co=10):
    ax.add_patch(FancyBboxPatch((cx - w / 2, cy - h / 2), w, h, fill=False, lw=1.7,
                                boxstyle="round,pad=0,rounding_size=0.3"))
    ax.plot([cx - w / 2, cx + w / 2], [cy + h / 2 - 0.55] * 2, color="black", lw=1.2)
    ax.text(cx, cy + h / 2 - 0.27, ma, ha="center", va="center", fontsize=co, weight="bold")
    ax.text(cx, cy - 0.27, ten, ha="center", va="center", fontsize=co + 1, weight="bold", linespacing=1.3)


def kho(ax, x, y, w, h, ma, lap=False):
    ax.plot([x + w, x, x, x + w], [y + h / 2, y + h / 2, y - h / 2, y - h / 2], color="black", lw=1.5)
    ax.plot([x + 0.8] * 2, [y - h / 2, y + h / 2], color="black", lw=1.2)
    if lap:  # kho vẽ lặp: thêm vạch đứng
        ax.plot([x + 0.15] * 2, [y - h / 2, y + h / 2], color="black", lw=1.2)
    ax.text(x + 0.47, y, ma, ha="center", va="center", fontsize=9.5, weight="bold")
    ax.text(x + 0.95, y, fill(KHO[ma], 24), ha="left", va="center", fontsize=9.5)


def boc(s, rong):
    return "\n".join(fill(d, rong) for d in s.split("\n"))


def mui_ten(ax, x1, y1, x2, y2):
    ax.annotate("", xy=(x2, y2), xytext=(x1, y1),
                arrowprops=dict(arrowstyle="-|>", color="black", lw=1.1, shrinkA=0, shrinkB=0,
                                mutation_scale=13))


def cap_luong(ax, xa, xb, y, sang_phai, sang_trai, rong, co):
    """Luồng giữa hai hình đặt ngang hàng; hai chiều thì vẽ 2 mũi tên ngược chiều."""
    lech = 0.17 if sang_phai and sang_trai else 0
    mx = (xa + xb) / 2
    if sang_phai:
        mui_ten(ax, xa, y + lech, xb, y + lech)
        ax.text(mx, y + lech + 0.07, boc(sang_phai, rong), ha="center", va="bottom", fontsize=co)
    if sang_trai:
        mui_ten(ax, xb, y - lech, xa, y - lech)
        ax.text(mx, y - lech - (0.07 if lech else -0.07), boc(sang_trai, rong), ha="center",
                va="top" if lech else "bottom", fontsize=co)


def luu(fig, ten):
    duong_dan = THU_MUC / ten
    fig.savefig(duong_dan, dpi=160, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    print("Đã ghi", duong_dan)


# ---------- sơ đồ ----------

def ve_ngu_canh():
    trai, phai, HANG = ["HV", "TS", "KT"], ["GV", "DT", "QT"], 3.4
    fig, ax = plt.subplots(figsize=(24 * 0.6, 16 * 0.6))
    tien_trinh(ax, 11.8, -1.5 * HANG + 0.05, 4.2, 3 * HANG + 0.5, "0",
               "HỆ THỐNG\nQUẢN LÝ\nTRUNG TÂM\nNGOẠI NGỮ", co=12)
    gop = lambda tn, h: "\n".join(ngu_canh(tn, h))
    for i, tn in enumerate(trai):
        y = -HANG * (i + 0.5)
        tac_nhan(ax, 0, y, 4.2, 1.2, tn)
        cap_luong(ax, 4.2, 9.7, y, gop(tn, "vao"), gop(tn, "ra"), 40, 8.5)
    for i, tn in enumerate(phai):
        y = -HANG * (i + 0.5)
        tac_nhan(ax, 19.4, y, 4.2, 1.2, tn)
        cap_luong(ax, 13.9, 19.4, y, gop(tn, "ra"), gop(tn, "vao"), 40, 8.5)
    y_gd = -3 * HANG - 2.8
    tac_nhan(ax, 9.8, y_gd, 4.0, 1.1, "GD")
    day = -3 * HANG - 0.2
    mui_ten(ax, 11.3, y_gd + 0.55, 11.3, day)
    ax.text(11.15, (y_gd + day) / 2, gop("GD", "vao"), ha="right", va="center", fontsize=8.5)
    mui_ten(ax, 12.3, day, 12.3, y_gd + 0.55)
    ax.text(12.45, (y_gd + day) / 2, gop("GD", "ra"), ha="left", va="center", fontsize=8.5)
    ax.text(11.8, 1.3, "SƠ ĐỒ DFD MỨC NGỮ CẢNH – HỆ THỐNG QUẢN LÝ TRUNG TÂM NGOẠI NGỮ",
            ha="center", fontsize=13, weight="bold")
    ax.set_xlim(-0.3, 23.9)
    ax.set_ylim(y_gd - 0.9, 1.8)
    ax.set_aspect("equal")
    ax.axis("off")
    luu(fig, "DFD_muc_ngu_canh.png")


def ve_muc_0():
    HANG, KHOANG = 1.9, 1.0
    X_TN, W_TN, X_P, W_P, X_K, W_K = 0, 4.2, 9.2, 3.8, 18.0, 4.2
    dai = []
    for p in CHUC_NANG:
        tns = list(dict.fromkeys(tn for tn, q, *_ in LUONG_TN if q == p))
        khos = sorted({k for k, q, *_ in LUONG_KHO if q == p})
        dai.append((p, tns, khos, max(len(tns), len(khos))))
    cao = sum(n * HANG + KHOANG for *_, n in dai)
    fig, ax = plt.subplots(figsize=(22.5 * 0.62, (cao + 1.5) * 0.62))

    da_ve_tn, da_ve_kho, yt = set(), set(), 0.0
    for p, tns, khos, n in dai:
        cy = yt - n * HANG / 2
        tien_trinh(ax, X_P + W_P / 2, cy, W_P, n * HANG - 0.6, p, fill(CHUC_NANG[p][0], 18))
        for i, tn in enumerate(tns):
            y = yt - HANG * (i + 0.5 + (n - len(tns)) / 2)
            tac_nhan(ax, X_TN, y, W_TN, 1.0, tn, lap=tn in da_ve_tn)
            da_ve_tn.add(tn)
            vao = "\n".join(t for x, q, h, t in LUONG_TN if x == tn and q == p and h == "vao")
            ra = "\n".join(t for x, q, h, t in LUONG_TN if x == tn and q == p and h == "ra")
            cap_luong(ax, X_TN + W_TN, X_P, y, vao, ra, 42, 8)
        for i, k in enumerate(khos):
            y = yt - HANG * (i + 0.5 + (n - len(khos)) / 2)
            kho(ax, X_K, y, W_K, 0.8, k, lap=k in da_ve_kho)
            da_ve_kho.add(k)
            ghi = "\n".join(t for x, q, h, t in LUONG_KHO if x == k and q == p and h == "ghi")
            doc = "\n".join(t for x, q, h, t in LUONG_KHO if x == k and q == p and h == "doc")
            cap_luong(ax, X_P + W_P, X_K, y, ghi, doc, 42, 8)
        yt -= n * HANG + KHOANG

    ax.text((X_TN + X_K + W_K) / 2, 1.0, "SƠ ĐỒ DFD MỨC 0 – HỆ THỐNG QUẢN LÝ TRUNG TÂM NGOẠI NGỮ",
            ha="center", fontsize=13, weight="bold")
    ax.text(X_TN, yt + 0.3, "Ghi chú: tác nhân có gạch chéo góc và kho có thêm vạch đứng là hình vẽ lặp.",
            fontsize=8.5, style="italic")
    ax.set_xlim(-0.3, X_K + W_K + 0.3)
    ax.set_ylim(yt - 0.2, 1.5)
    ax.set_aspect("equal")
    ax.axis("off")
    luu(fig, "DFD_muc_0.png")


def ghi_bang_can_bang():
    ten_tn = lambda tn: TAC_NHAN[tn].replace(" /\n", "/").replace("\n", " ")
    dong = ["# Bảng cân bằng DFD (sinh tự động từ so_do/src/dfd.py – không sửa tay)", "",
            "## Tác nhân: mức ngữ cảnh ↔ mức 0", "",
            "| Tác nhân | Hướng | Luồng ở ngữ cảnh | Tiến trình mức 0 |", "|---|---|---|---|"]
    for tn in TAC_NHAN:
        for h, ten_h in (("vao", "Vào hệ thống"), ("ra", "Ra khỏi hệ thống")):
            for n in ngu_canh(tn, h):
                ps = ", ".join(p for x, p, hh, t in LUONG_TN if x == tn and hh == h and t == n)
                dong.append(f"| {ten_tn(tn)} | {ten_h} | {n} | {ps} |")
    dong += ["", "## Kho dữ liệu ↔ tiến trình mức 0", "",
             "| Kho | Tiến trình | Hướng | Luồng |", "|---|---|---|---|"]
    for k, p, h, n in sorted(LUONG_KHO):
        dong.append(f"| {k} {KHO[k]} | {p} | {'Ghi vào kho' if h == 'ghi' else 'Đọc từ kho'} | {n} |")
    (THU_MUC / "can_bang_dfd.md").write_text("\n".join(dong) + "\n", encoding="utf-8")
    print("Đã ghi", THU_MUC / "can_bang_dfd.md")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # console Windows mặc định cp1252
    kiem_tra()
    ve_ngu_canh()
    ve_muc_0()
    ghi_bang_can_bang()
