"""Đối chiếu kết quả chuẩn hóa (03_chuan_hoa.md mục 8) với bảng thực thể đã thiết kế (01_thuc_the.md mục 8).

Đạt khi, với mọi thực thể của 3.1:
  - khóa thu được từ chuẩn hóa trùng khóa đã thiết kế (với thực thể có chứng từ),
  - mọi thuộc tính thu được từ chuẩn hóa đều có trong thiết kế (không có bảng hay trường nào thừa),
  - cột "bổ sung" ghi đúng phần chênh lệch giữa thiết kế và kết quả chuẩn hóa.
Chạy: python thiet_ke/kiem_tra_chuan_hoa.py
"""
import re
import sys
from pathlib import Path

GOC = Path(__file__).resolve().parent


def muc(text, so):
    return re.split(r"^## \d+\.", text.split(f"\n## {so}.", 1)[1], maxsplit=1, flags=re.M)[0]


def tach(s):
    s = re.sub(r"\([^)]*\)", "", s).strip()
    return [] if s in ("", "–") else [x.strip() for x in s.split(",")]


def main():
    tk = {}
    for tep, ds in re.findall(r"^\| \d+ \| [^|]+ \| (\w+) \| [^|]+ \| D\d \| ([^|]+) \|",
                              muc((GOC / "01_thuc_the.md").read_text(encoding="utf-8"), 8), re.M):
        ds = tach(ds)
        tk[tep] = ([x.lstrip("#") for x in ds], {x[1:] for x in ds if x.startswith("#")})

    dong = re.findall(r"^\| (\w+) \| ([^|]+) \| ([^|]+) \| ([^|]+) \|",
                      muc((GOC / "03_chuan_hoa.md").read_text(encoding="utf-8"), 8), re.M)
    dong = [d for d in dong if d[0] in tk]
    assert sorted(d[0] for d in dong) == sorted(tk), \
        f"Bảng đối chiếu thiếu/thừa thực thể: {set(tk) ^ {d[0] for d in dong}}"

    so_tu_ct, so_tt_ct = 0, 0
    for tep, chuan_hoa, chung_tu, bo_sung in dong:
        ch = tach(chuan_hoa)
        thuoc_tinh, khoa = tk[tep]
        ten = [x.lstrip("#") for x in ch]
        thua = set(ten) - set(thuoc_tinh)
        assert not thua, f"{tep}: chuẩn hóa ra thuộc tính không có trong 3.1: {thua}"
        if ch:
            so_tu_ct += 1
            so_tt_ct += len(ch)
            k = {x[1:] for x in ch if x.startswith("#")}
            assert k == khoa, f"{tep}: khóa sau chuẩn hóa {k} khác khóa 3.1 {khoa}"
            assert chung_tu.strip() != "–", f"{tep}: có thuộc tính nhưng không ghi chứng từ"
        can = [x for x in thuoc_tinh if x not in ten]
        assert set(tach(bo_sung)) == set(can), \
            f"{tep}: cột bổ sung phải là {can}, đang ghi {tach(bo_sung)}"

    tong = sum(len(v[0]) for v in tk.values())
    print("Đối chiếu chuẩn hóa ↔ 3.1: đạt")
    print(f"  - {so_tu_ct}/{len(tk)} thực thể có từ chuẩn hóa chứng từ; khóa trùng khóa thiết kế")
    print(f"  - {so_tt_ct}/{tong} thuộc tính lấy từ chứng từ; không có thuộc tính thừa")
    print(f"  - {len(tk) - so_tu_ct} thực thể không có chứng từ: "
          + ", ".join(t for t, c, *_ in dong if not tach(c)))


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
