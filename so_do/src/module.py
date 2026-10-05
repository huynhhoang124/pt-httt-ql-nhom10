"""Sơ đồ module (Top-down) và các kiểm tra của thiết kế phần mềm 3.5.

Không chép lại dữ liệu: danh sách module, bảng module – dữ liệu, ma trận phân quyền đọc từ
thiet_ke/05_module.md (mục 3, 4, 5); chức năng lấy từ bfd.py, luồng DFD mức 1 từ dfd.py,
kho của từng bảng từ thiet_ke/01_thuc_the.md mục 8, tên view/hàm từ csdl/views.sql.
Chạy: python so_do/src/module.py  ->  kiểm tra rồi ghi so_do/So_do_module.png
"""
import re
import sys
from pathlib import Path
from textwrap import fill

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt

from bfd import CHUC_NANG, hop
from dfd import MUC_1

THU_MUC = Path(__file__).resolve().parents[1]
DU_AN = THU_MUC.parent
VAI_TRO = {"HV": "HV", "PH": "HV", "GV": "GV", "TS": "TS", "KT": "KT", "DT": "DT", "GD": "GD", "QT": "QT"}  # vai trò -> tác nhân DFD
NHAP = set("TSD")          # quyền đưa dữ liệu vào (cần luồng vào trên DFD)
TU_DONG = {"5.2"}           # module tự chạy, không vai trò nào nhập
# Thiếu sót của DFD mức 1 do bước này phát hiện: (vai trò, module) -> mô tả luồng cần bổ sung
THIEU_LUONG = {}  # 2.3 và 4.3 đã được bổ sung vào dfd.py (2026-10-05)


def muc(text, so):
    return re.split(r"^## \d+\.", text.split(f"\n## {so}.", 1)[1], maxsplit=1, flags=re.M)[0]


def o(dong):
    return [x.strip() for x in dong.strip().strip("|").split("|")]


def tach(s):
    return [] if s.strip() in ("–", "") else [x.strip() for x in s.split(",")]


def doc():
    md = (DU_AN / "thiet_ke/05_module.md").read_text(encoding="utf-8")
    module = {c[0]: dict(ten=c[1], phan_he=c[2], con=c[4]) for c in map(o, muc(md, 3).splitlines())
              if len(c) == 5 and re.fullmatch(r"\d\.\d|N\d", c[0])}
    du_lieu = {c[0]: dict(ghi=tach(c[1]), doc=tach(c[2]), tra_cuu=tach(c[3]), view=c[4])
               for c in map(o, muc(md, 4).split("### 4.1")[0].splitlines())
               if len(c) == 5 and re.fullmatch(r"\d\.\d|N\d", c[0])}
    dong5 = [o(d) for d in muc(md, 5).splitlines() if d.startswith("|")]
    vai = dong5[0][1:]
    quyen = {c[0]: dict(zip(vai, [set(x.split()) - {"–"} for x in c[1:]])) for c in dong5[2:]}
    tt = (DU_AN / "thiet_ke/01_thuc_the.md").read_text(encoding="utf-8")
    kho = dict(re.findall(r"^\| \d+ \| [^|]+ \| (\w+) \| [^|]+ \| (D\d) \|", muc(tt, 8), re.M))
    view = set(re.findall(r"CREATE (?:VIEW|FUNCTION) dbo\.(\w+)", (DU_AN / "csdl/views.sql").read_text(encoding="utf-8")))
    return module, du_lieu, quyen, kho, view


def kiem_tra(module, du_lieu, quyen, kho, view):
    kq = []
    la = {f"{p[0]}.{i}": ten for p, (_, cons) in CHUC_NANG.items() for i, ten in enumerate(cons, 1)}
    chuc_nang = {m: v for m, v in module.items() if m[0] != "N"}
    assert {m: v["ten"] for m, v in chuc_nang.items()} == la, "Module chức năng phải trùng mã và tên 20 chức năng lá BFD"
    assert all(v["phan_he"] == m[0] + ".0" for m, v in chuc_nang.items()), "Sai phân hệ"
    kq.append(f"20 module chức năng trùng mã và tên với 20 chức năng lá của BFD; thêm {len(module) - 20} module nền "
              f"→ {1 + 6 + len(module)} module, nhiều hơn {len(CHUC_NANG) + len(la)} tiến trình DFD.")

    assert set(du_lieu) == set(module), "Mục 4 phải có đủ các module của mục 3"
    for m, d in du_lieu.items():
        for t in d["ghi"] + d["doc"] + d["tra_cuu"]:
            assert t in kho, f"{m}: không có bảng {t}"
        for v in re.findall(r"\b(?:v_\w+|fn_\w+)", d["view"]):
            assert v in view, f"{m}: không có view/hàm {v} trong csdl/views.sql"
        if m in la:  # đối chiếu với luồng kho DFD mức 1
            luong = MUC_1[m[0] + ".0"]["kho"]
            ghi = {l[0] for l in luong if l[1] == m and l[2] == "ghi"}
            doc_ = {l[0] for l in luong if l[1] == m and l[2] == "doc"} | ghi
            sai = [t for t in d["ghi"] if kho[t] not in ghi] + [t for t in d["doc"] if kho[t] not in doc_]
            assert not sai, f"{m}: bảng {sai} thuộc kho mà DFD mức 1 không cho {m} ghi/đọc"
    ghi_boi = {t for d in du_lieu.values() for t in d["ghi"]}
    assert ghi_boi == set(kho), f"Bảng không module nào ghi: {set(kho) - ghi_boi}"
    kq.append(f"Mọi bảng ghi/đọc của 20 module chức năng nằm đúng kho mà tiến trình DFD mức 1 tương ứng ghi/đọc; "
              f"cả {len(kho)} bảng đều có module tạo dữ liệu; mọi view/hàm nêu tên đều có trong csdl/views.sql.")

    assert set(quyen) <= set(module) and set(la) <= set(quyen), "Ma trận phải có đủ 20 module chức năng"
    assert all(set(q) == set(VAI_TRO) for q in quyen.values()), "Ma trận phải đủ 8 vai trò"
    assert all(c <= set("TSDIXx") for q in quyen.values() for c in q.values()), "Ký hiệu quyền lạ"
    phat_hien = []
    for m in la:
        tn = MUC_1[m[0] + ".0"]["tn"]
        vao = {l[0] for l in tn if l[1] == m and l[2] == "vao"}
        ra = {l[0] for l in tn if l[1] == m and l[2] == "ra"}
        for vt, q in quyen[m].items():
            if q & NHAP and VAI_TRO[vt] not in vao:  # nhập dữ liệu nhưng DFD không có luồng vào
                assert (vt, m) in THIEU_LUONG, f"{vt} có quyền {q & NHAP} ở {m} nhưng DFD-{m[0]}.0 không có luồng vào từ tác nhân này"
                phat_hien.append(THIEU_LUONG[vt, m])
            if VAI_TRO[vt] in ra:
                assert q, f"{vt} nhận luồng ra từ {m} trên DFD nhưng không có quyền xem"
        if du_lieu[m]["ghi"] and m not in TU_DONG:
            assert any(q & (NHAP | {"I"}) for q in quyen[m].values()), f"{m} ghi dữ liệu nhưng không vai trò nào thao tác"
    assert len(phat_hien) == len(THIEU_LUONG), "THIEU_LUONG có mục đã hết thiếu – xóa khỏi danh sách"
    kq.append("Vai trò có quyền T/S/D ở module nào thì tác nhân tương ứng có luồng vào tiến trình đó trên DFD mức 1; "
              "tác nhân nhận luồng ra thì vai trò tương ứng có quyền xem.")
    return kq, phat_hien


def ve(module):
    nhom = {**{p: (ten, [(f"{p[0]}.{i}", c) for i, c in enumerate(cons, 1)]) for p, (ten, cons) in CHUC_NANG.items()},
            "N": ("Chức năng nền", [(m, v["ten"]) for m, v in module.items() if m[0] == "N"])}
    W, GAP, H, BUOC = 3.9, 0.45, 1.05, 1.4
    tong = len(nhom) * W + (len(nhom) - 1) * GAP
    sau = max(len(c) for _, c in nhom.values())
    day = 4.85 - (sau - 1) * BUOC - 0.5
    fig, ax = plt.subplots(figsize=(tong * 0.75, (9.6 - day) * 0.75))
    hop(ax, tong / 2 - 3.4, 8.2, 6.8, 1.1, "MODULE CHÍNH:\nHỆ THỐNG QUẢN LÝ TRUNG TÂM NGOẠI NGỮ", dam=True, co=11)
    ax.plot([tong / 2] * 2, [8.2, 7.7], color="black", lw=1.2)
    ax.plot([W / 2, tong - W / 2], [7.7] * 2, color="black", lw=1.2)
    for i, (ma, (ten, cons)) in enumerate(nhom.items()):
        x = i * (W + GAP)
        ax.plot([x + W / 2] * 2, [7.7, 7.2], color="black", lw=1.2)
        tieu_de = f"{ma}. {ten}" if ma != "N" else "N. Chức năng nền\n(không có trên DFD)"
        hop(ax, x, 6.2, W, H, fill(tieu_de, 22) if ma != "N" else tieu_de, dam=True, co=10.5)
        for j, (m, c) in enumerate(cons, 1):
            y = 4.85 - (j - 1) * BUOC
            hop(ax, x + 0.55, y, W - 0.55, H, fill(f"{m}. {c}", 25), co=9.5)
            ax.plot([x + 0.25, x + 0.55], [y + H / 2] * 2, color="black", lw=1.2)
        ax.plot([x + 0.25] * 2, [6.2, 4.85 - (len(cons) - 1) * BUOC + H / 2], color="black", lw=1.2)
    ax.text(0, day + 0.1, "Mã module = mã chức năng BFD = mã tiến trình DFD = mã mục thực đơn. "
            "Module nền N1–N7 xử lý các yêu cầu không thuộc chức năng (bài giảng mục 5.5.1.2).",
            fontsize=9, style="italic", va="bottom")
    ax.set_xlim(-0.2, tong + 0.2)
    ax.set_ylim(day - 0.2, 9.5)
    ax.set_aspect("equal")
    ax.axis("off")
    ra = THU_MUC / "So_do_module.png"
    fig.savefig(ra, dpi=160, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    print("Đã ghi", ra)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    du = doc()
    kq, phat_hien = kiem_tra(*du)
    print("Kiểm tra thiết kế module: đạt")
    for d in kq:
        print("  -", d)
    for d in phat_hien:
        print("  ! DFD mức 1 cần bổ sung luồng:", d)
    ve(du[0])
