"""Sơ đồ Quan hệ – Thực thể (ERD), ký pháp bài giảng: chữ nhật = thực thể, hình thoi = quan hệ (động từ),
1/N ở hai đầu; thực thể quan hệ N–N = hình thoi chữ hoa kèm elip thuộc tính riêng (Hình 4.45).

Không chép lại dữ liệu: thực thể (mục 8) và thuộc tính quan hệ (mục 9) đọc từ thiet_ke/01_thuc_the.md,
bảng quan hệ (mục 2) đọc từ thiet_ke/02_quan_he.md. File này chỉ giữ bố cục và các bước kiểm tra.
Chạy: python so_do/src/erd.py  ->  kiểm tra rồi ghi so_do/ERD_1_nghiep_vu.png, ERD_2_he_thong.png.
"""
import re
import sys
from collections import Counter
from pathlib import Path
from textwrap import fill

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Ellipse, Polygon, Rectangle

GOC = Path(__file__).resolve().parents[2]
THU_MUC = GOC / "so_do"


# ---------- đọc dữ liệu từ tài liệu thiết kế ----------

def _muc(text, so):
    return re.split(r"^## \d+\.", text.split(f"\n## {so}.", 1)[1], maxsplit=1, flags=re.M)[0]


def doc_du_lieu():
    tt = (GOC / "thiet_ke/01_thuc_the.md").read_text(encoding="utf-8")
    qh = (GOC / "thiet_ke/02_quan_he.md").read_text(encoding="utf-8")
    thuc_the = {}  # tên tệp -> (tên có dấu, loại, [thuộc tính], [khóa])
    for ten, tep, loai, thuoc_tinh in re.findall(
            r"^\| \d+ \| ([^|]+?) \| (\w+) \| ([^|]+?) \| D\d \| ([^|]+) \|", _muc(tt, 8), re.M):
        ds = [x.strip() for x in thuoc_tinh.split(",")]
        thuc_the[tep] = (ten, loai, [x.lstrip("#") for x in ds], [x[1:] for x in ds if x.startswith("#")])
    khoa_ngoai = {}  # số dòng mục 9 -> (thực thể chứa, [cột], thực thể được trỏ tới)
    for so, chua, cot, toi in re.findall(r"^\| (\d+) \| (\w+) \| ([^|]+?) \| (\w+) \|", _muc(tt, 9), re.M):
        khoa_ngoai[int(so)] = (chua, [c.strip() for c in cot.strip("()").split(",")], toi)
    ten_tt = {}  # (thực thể, tên trường) -> tên thuộc tính có dấu, lấy từ các bảng E01–E26
    for khoi in re.split(r"^#### E\d+\. ", tt.split("\n## 5.")[0], flags=re.M)[1:]:
        tep = re.search(r"\(`(\w+)`\)", khoi).group(1)
        for ten, truong in re.findall(r"^\| ([^|]+?) \| (\w+) \|", khoi, re.M):
            ten_tt[tep, truong] = ten
    quan_he = {}  # mã -> dict
    for ma, ten, a, b, bac, kieu, cot, tqh in re.findall(
            r"^\| (Q\d+) \| ([^|]+?) \| [^|]*\((\w+)\) \| [^|]*\((\w+)\) \| (\d) \| ([1N]-[1N]) \| ([^|]+?) \| ([^|]+?) \|",
            _muc(qh, 2), re.M):
        quan_he[ma] = dict(ten=ten, a=a, b=b, bac=int(bac), kieu=kieu,
                           kn=[int(x) for x in cot.split(",")], tqh=None if tqh == "–" else tqh)
    return thuc_the, khoa_ngoai, ten_tt, quan_he


# ---------- kiểm tra ----------

# N–N mà khóa thực thể quan hệ không chứa đủ khóa hai bên – có lý do ghi ở 02_quan_he.md
NGOAI_LE_KHOA_NN = {("LichTuan", "MaPhong"): "mỗi thứ một lớp học một phòng (giả định G2), nên khóa là (MaLop, Thu)"}
CAU_HINH = {"ThamSo"}  # dữ liệu cấu hình, không có quan hệ (bài giảng mục 5.3.1)


def kiem_tra(thuc_the, khoa_ngoai, quan_he):
    ket_qua = []
    dung = Counter(n for q in quan_he.values() for n in q["kn"])
    assert set(dung) == set(khoa_ngoai), f"Thuộc tính quan hệ chưa thuộc quan hệ nào: {set(khoa_ngoai) - set(dung)}"
    assert max(dung.values()) == 1, f"Thuộc tính quan hệ dùng 2 lần: {[n for n, c in dung.items() if c > 1]}"
    ket_qua.append(f"Cả {len(khoa_ngoai)} thuộc tính quan hệ của 3.1 đều thuộc đúng một quan hệ.")

    for ma, q in quan_he.items():
        a, b, kn = q["a"], q["b"], [khoa_ngoai[n] for n in q["kn"]]
        assert (q["bac"] == 1) == (a == b), f"{ma}: bậc 1 khi và chỉ khi A = B"
        if q["kieu"] == "N-N":
            r = q["tqh"]
            assert r and len(kn) == 2, f"{ma}: N–N phải có thực thể quan hệ và 2 thuộc tính quan hệ"
            assert thuc_the[r][1] == "Quan hệ", f"{ma}: {r} phải là thực thể loại Quan hệ"
            assert all(c == r for c, _, _ in kn) and {t for *_, t in kn} == {a, b}, f"{ma}: sai thực thể"
            for _, cot, _ in kn:  # khóa thực thể quan hệ = tổ hợp khóa hai thực thể gốc
                for c in cot:
                    assert c in thuc_the[r][3] or (r, c) in NGOAI_LE_KHOA_NN, f"{ma}: {c} phải thuộc khóa của {r}"
        else:
            assert q["tqh"] is None and len(kn) == 1, f"{ma}: 1–1 / 1–N chỉ có 1 thuộc tính quan hệ"
            chua, cot, toi = kn[0]
            if q["kieu"] == "1-N":  # khóa đầu 1 đặt ở đầu N
                assert (chua, toi) == (b, a), f"{ma}: thuộc tính quan hệ phải nằm ở đầu N ({b}) và trỏ về {a}"
            else:
                assert {chua, toi} == {a, b}, f"{ma}: sai thực thể"
        for chua, cot, toi in kn:  # thuộc tính quan hệ trỏ tới đủ khóa của thực thể đích
            ten_goc = [{"MaLopGoc": "MaLop", "MaNVDuyet": "MaNV"}.get(c, c) for c in cot]
            assert sorted(ten_goc) == sorted(thuc_the[toi][3]), f"{ma}: {cot} không khớp khóa {thuc_the[toi][3]} của {toi}"
    ket_qua.append("Kiểu 1–N: thuộc tính quan hệ luôn nằm ở thực thể đầu N, trỏ về định danh của đầu 1.")
    ket_qua.append("Kiểu N–N: có thực thể quan hệ loại *Quan hệ*, khóa gồm định danh của 2 thực thể gốc "
                   "(ngoại lệ có lý do: " + "; ".join(f"{r}.{c} – {ly_do}" for (r, c), ly_do in NGOAI_LE_KHOA_NN.items()) + ").")
    ket_qua.append("Bậc 1 khi và chỉ khi hai đầu là cùng một thực thể; không có quan hệ bậc ≥ 3.")

    co_quan_he = {q[k] for q in quan_he.values() for k in ("a", "b")} | {q["tqh"] for q in quan_he.values()}
    le = set(thuc_the) - co_quan_he
    assert le == CAU_HINH, f"Thực thể không có quan hệ: {le - CAU_HINH}"
    ket_qua.append(f"Mọi thực thể đều tham gia ít nhất một quan hệ, trừ {', '.join(sorted(CAU_HINH))} (dữ liệu cấu hình).")

    ve = Counter(m for h in HINH for m in h["quan_he"])
    assert set(ve) == set(quan_he) and max(ve.values()) == 1, "Mỗi quan hệ phải vẽ ở đúng một hình"
    for h in HINH:
        for m in h["quan_he"]:
            q = quan_he[m]
            can = {q["a"], q["b"]}
            assert can <= set(h["thuc_the"]), f"{m}: hình '{h['tieu_de']}' thiếu {can - set(h['thuc_the'])}"
            assert q["tqh"] is None or q["tqh"] not in h["thuc_the"], f"{m}: thực thể quan hệ vẽ bằng hình thoi"
    nn = {q["tqh"] for q in quan_he.values() if q["tqh"]}
    da_ve = {t for h in HINH for t in h["thuc_the"]} | nn
    assert da_ve == set(thuc_the), f"Thực thể chưa vẽ: {set(thuc_the) - da_ve}"
    ket_qua.append(f"Hai hình ERD vẽ đủ {len(thuc_the)} thực thể ({len(nn)} thực thể quan hệ N–N vẽ bằng hình thoi) "
                   f"và {len(quan_he)} quan hệ, mỗi quan hệ đúng một lần.")

    kieu = Counter(q["kieu"] for q in quan_he.values())
    bac = Counter(q["bac"] for q in quan_he.values())
    thong_ke = (f"{len(quan_he)} quan hệ: bậc 2 có {bac[2]}, bậc 1 có {bac[1]}; "
                f"kiểu 1–N có {kieu['1-N']}, 1–1 có {kieu['1-1']}, N–N có {kieu['N-N']} "
                f"(cộng thêm quan hệ N–N Học viên – Lớp học qua Đăng ký học, mục 2.2).")
    print("Kiểm tra ERD: đạt")
    for d in ket_qua:
        print("  -", d)
    print("  Thống kê:", thong_ke)
    return ket_qua, thong_ke


# ---------- bố cục ----------
# Tọa độ lưới (cột, hàng), hàng tăng xuống dưới. "duong": điểm gấp khúc của đoạn nối (mã quan hệ, thực thể).
# "elip": vị trí elip thuộc tính riêng của thực thể quan hệ N–N, theo thứ tự thuộc tính.
HINH = [
    dict(
        tieu_de="SƠ ĐỒ ERD (HÌNH 1) – PHẦN NGHIỆP VỤ",
        ten_file="ERD_1_nghiep_vu.png",
        thuc_the={"PhongHoc": (1, 0), "KhoaHoc": (3, 0), "GiaoVien": (5, 0),
                  "PhuHuynh": (1, 2), "LopHoc": (3, 2), "BuoiHoc": (5, 2), "UuDai": (7, 2),
                  "HocVien": (1, 4), "DangKy": (3, 4), "HocPhi": (5, 4), "DotHocPhi": (7, 4),
                  "PhieuDangKy": (1, 6), "Diem": (3, 6), "KetQua": (5, 6), "PhieuThu": (7, 6),
                  "ChungNhan": (5, 8), "NhanVien": (7, 8)},
        quan_he={"Q01": (1, 3), "Q02": (3, 1), "Q03": (4, 1), "Q04": (2, 1), "Q05": (4, 2), "Q06": (6, 1),
                 "Q07": (5, 1), "Q08": (4, 9), "Q09": (2, 4), "Q10": (3, 3), "Q11": (2, 5), "Q12": (2, 3.25),
                 "Q13": (4, 4), "Q14": (6, 4), "Q15": (6, 3), "Q16": (7, 5), "Q17": (7, 7), "Q18": (4, 3),
                 "Q19": (3, 5), "Q20": (4, 5), "Q21": (6, 7), "Q22": (5, 7)},
        duong={("Q06", "PhongHoc"): [(1, -0.8), (6, -0.8)],
               ("Q08", "PhieuDangKy"): [(1, 9)],
               ("Q08", "NhanVien"): [(7, 9)],
               # quan hệ bậc 1: hai đoạn nối từ cạnh trên của Đăng ký học
               ("Q12", "DangKy", 0): [(3 - 0.15, 4), (3 - 0.15, 3.25)],
               ("Q12", "DangKy", 1): [(3 - 0.3, 4), (3 - 0.3, 3.6), (2, 3.6)]},
        # quan hệ bậc 1: thay nhãn 1 ở đầu thực thể bằng nhãn có vai trò, đặt tay
        vai_tro={("Q12", 0): ((2.55, 3.15), "1 (gốc)"), ("Q12", 1): ((2.3, 3.75), "1 (mới)")},
        elip={"Q01": [(0.25, 3)], "Q04": [(1.70, 0.12), (2.256, 0.12)],
              "Q16": [(6.25, 5)], "Q18": [(3.6, 2.6), (4.75, 3.0)]},
        lap=set(),
    ),
    dict(
        tieu_de="SƠ ĐỒ ERD (HÌNH 2) – PHẦN QUẢN TRỊ HỆ THỐNG",
        ten_file="ERD_2_he_thong.png",
        thuc_the={"NhanVien": (1, 0), "GiaoVien": (3, 0), "TaiKhoan": (2, 2), "NhatKy": (4.2, 2),
                  "HocVien": (1, 4), "PhuHuynh": (3, 4), "ThongBao": (2, 6), "ThamSo": (4.2, 6)},
        quan_he={"Q23": (1, 1), "Q24": (3, 1), "Q25": (1, 3), "Q26": (3, 3), "Q29": (3.1, 2),
                 "Q27": (1, 5), "Q28": (3, 5)},
        duong={}, vai_tro={}, elip={},
        lap={"NhanVien", "GiaoVien", "HocVien", "PhuHuynh"},
    ),
]

O_X, O_Y = 4.3, 2.3           # khoảng cách cột, hàng
TT_W, TT_H = 3.5, 1.0         # thực thể
QH_W, QH_H = 2.6, 1.2         # hình thoi
EL_H = 0.62                   # elip thuộc tính (bề rộng theo độ dài chữ)
CO = 10


def xy(p):
    return p[0] * O_X, -p[1] * O_Y


def trong_hop(p, tam, w, h, thoi):
    dx, dy = abs(p[0] - tam[0]) / (w / 2), abs(p[1] - tam[1]) / (h / 2)
    return dx + dy <= 1 if thoi else max(dx, dy) <= 1


def diem_nhan(duong, w, h, thoi, cach=0.32, lech=0.22):
    """Vị trí ghi 1/N: trên đoạn nối, ngay khi ra khỏi hình ở đầu duong[0]; lệch sang một bên."""
    tam = duong[0]
    for (x1, y1), (x2, y2) in zip(duong, duong[1:]):
        dai = ((x2 - x1) ** 2 + (y2 - y1) ** 2) ** 0.5
        if not dai:
            continue
        ux, uy = (x2 - x1) / dai, (y2 - y1) / dai
        t = 0.0
        while t <= dai:
            p = (x1 + ux * t, y1 + uy * t)
            if not trong_hop(p, tam, w + 2 * cach, h + 2 * cach, thoi):
                return p[0] - uy * lech, p[1] + ux * lech
            t += 0.03
    return duong[-1]


def ve_thuc_the(ax, tam, ten, tep, quan_he_bao=False, lap=False):
    x, y = tam
    ax.add_patch(Rectangle((x - TT_W / 2, y - TT_H / 2), TT_W, TT_H, fc="white", ec="black", lw=1.5, zorder=3))
    if quan_he_bao:  # thực thể quan hệ vẽ dạng chữ nhật: viền đôi
        ax.add_patch(Rectangle((x - TT_W / 2 + 0.1, y - TT_H / 2 + 0.1), TT_W - 0.2, TT_H - 0.2,
                               fill=False, ec="black", lw=0.8, zorder=3))
    if lap:
        ax.plot([x + TT_W / 2 - 0.35, x + TT_W / 2], [y - TT_H / 2, y - TT_H / 2 + 0.35], color="black", lw=1, zorder=4)
    ax.text(x, y + 0.13, ten, ha="center", va="center", fontsize=CO - 0.5, weight="bold", zorder=5)
    ax.text(x, y - 0.24, tep, ha="center", va="center", fontsize=CO - 2, style="italic", zorder=5)


def ve_thoi(ax, tam, ten, hoa):
    x, y = tam
    ax.add_patch(Polygon([(x - QH_W / 2, y), (x, y + QH_H / 2), (x + QH_W / 2, y), (x, y - QH_H / 2)],
                         closed=True, fc="white", ec="black", lw=1.5 if hoa else 1.2, zorder=3))
    ax.text(x, y, fill(ten, 7 if hoa else 9), ha="center", va="center", fontsize=CO - (1.5 if hoa else 1),
            weight="bold" if hoa else "normal", linespacing=1.1, zorder=5)


def rong_elip(chu):
    return max(1.6, 0.17 * len(chu) + 0.5)


def ve_elip(ax, tam, chu, gach_chan):
    ax.add_patch(Ellipse(tam, rong_elip(chu), EL_H, fc="white", ec="black", lw=1, zorder=3))
    ax.text(tam[0], tam[1], chu, ha="center", va="center", fontsize=CO - 2, zorder=5)
    if gach_chan:
        rong = 0.14 * len(chu)
        ax.plot([tam[0] - rong / 2, tam[0] + rong / 2], [tam[1] - 0.17] * 2, color="black", lw=0.8, zorder=5)


def ve_hinh(h, thuc_the, khoa_ngoai, ten_tt, quan_he):
    pos = {t: xy(p) for t, p in h["thuc_the"].items()}
    diem = list(h["thuc_the"].values()) + list(h["quan_he"].values()) + [e for ds in h["elip"].values() for e in ds]
    cot, hang = [p[0] for p in diem], [p[1] for p in diem]
    rong, cao = (max(cot) - min(cot) + 1.6) * O_X, (max(hang) - min(hang) + 2.6) * O_Y
    fig, ax = plt.subplots(figsize=(rong * 0.43, cao * 0.43))
    nhan = lambda p, s: ax.text(p[0], p[1], s, ha="center", va="center", fontsize=CO - 0.5, zorder=6)

    for m, p in h["quan_he"].items():
        q, tam = quan_he[m], xy(p)
        hoa = q["kieu"] == "N-N"
        dau = [(q["a"], 0), (q["b"], 1)]
        for t, i in dau:
            g = h["duong"].get((m, t, i), h["duong"].get((m, t), []))
            duong = [pos[t]] + [xy(d) for d in g] + [tam]
            ax.plot(*zip(*duong), color="black", lw=1.1, zorder=1)
            if hoa:  # như Hình 4.45: 1 ở phía thực thể, N ở phía thực thể quan hệ
                sat_tt, sat_qh = "1", "N"
            else:
                sat_tt, sat_qh = q["kieu"].split("-")[i], None
            if (m, i) in h["vai_tro"]:
                p_vt, chu = h["vai_tro"][m, i]
                nhan(xy(p_vt), chu)
            else:
                nhan(diem_nhan(duong, TT_W, TT_H, False), sat_tt)
            if sat_qh:
                nhan(diem_nhan(duong[::-1], QH_W, QH_H, True), sat_qh)
        ve_thoi(ax, tam, q["ten"], hoa)
        if hoa:  # thuộc tính riêng của thực thể quan hệ (không lặp lại khóa của hai thực thể gốc)
            r = q["tqh"]
            chung = {c for n in q["kn"] for c in khoa_ngoai[n][1]}
            rieng = [c for c in thuc_the[r][2] if c not in chung]
            assert len(rieng) == len(h["elip"].get(m, [])), f"{m}: cần {len(rieng)} vị trí elip cho {rieng}"
            for c, p_el in zip(rieng, h["elip"].get(m, [])):
                e = xy(p_el)
                ax.plot([tam[0], e[0]], [tam[1], e[1]], color="black", lw=0.9, zorder=1)
                ve_elip(ax, e, ten_tt[r, c], c in thuc_the[r][3])

    for t, p in pos.items():
        ve_thuc_the(ax, p, thuc_the[t][0], t, quan_he_bao=thuc_the[t][1] == "Quan hệ", lap=t in h["lap"])
    if "ThamSo" in pos:
        x, y = pos["ThamSo"]
        ax.text(x, y - TT_H / 2 - 0.3, "(dữ liệu cấu hình,\nkhông có quan hệ)", ha="center", va="top",
                fontsize=CO - 2, style="italic")

    x0, x1 = (min(cot) - 0.6) * O_X, (max(cot) + 0.6) * O_X
    y_top, y_bot = -(min(hang) - 1.1) * O_Y, -(max(hang) + (1.0 if "ThamSo" in pos else 0.6)) * O_Y
    ax.text((x0 + x1) / 2, y_top + 0.4, h["tieu_de"], ha="center", fontsize=13, weight="bold")
    ky_hieu = ["hình chữ nhật = thực thể", "hình thoi = quan hệ, 1 và N ghi ở hai đầu"]
    if any(quan_he[m]["kieu"] == "N-N" for m in h["quan_he"]):
        ky_hieu.append("hình thoi chữ hoa = thực thể quan hệ N–N, elip = thuộc tính riêng của nó "
                       "(gạch chân = thuộc định danh)")
    if any(thuc_the[t][1] == "Quan hệ" for t in pos):
        ky_hieu.append("chữ nhật viền đôi = thực thể quan hệ vẽ dạng chữ nhật vì còn tham gia quan hệ khác")
    if h["lap"]:
        ky_hieu.append("gạch chéo góc = thực thể vẽ lặp (đã có ở hình 1)")
    chu_giai = fill("Ký hiệu: " + "; ".join(ky_hieu) + ".", int((x1 - x0) / 0.17))
    ax.text(x0, y_bot - 0.2, chu_giai, ha="left", va="top", fontsize=CO - 2, style="italic", linespacing=1.4)
    ax.set_xlim(x0, x1)
    ax.set_ylim(y_bot - 1.4, y_top + 1.2)
    ax.set_aspect("equal")
    ax.axis("off")
    duong_dan = THU_MUC / h["ten_file"]
    fig.savefig(duong_dan, dpi=170, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    print("Đã ghi", duong_dan)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # console Windows mặc định cp1252
    du_lieu = doc_du_lieu()
    thuc_the, khoa_ngoai, ten_tt, quan_he = du_lieu
    kiem_tra(thuc_the, khoa_ngoai, quan_he)
    for h in HINH:
        ve_hinh(h, *du_lieu)
