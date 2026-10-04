"""Đối chiếu csdl/schema.sql với thiết kế 3.1, rồi sinh bảng mô tả từng tệp (csdl/mo_ta_bang.md).

Kiểm tra (dừng nếu sai):
  - đủ 26 bảng, mỗi bảng đúng các trường và thứ tự trường của 3.1 (01_thuc_the.md, bảng E01–E26);
  - khóa chính = thuộc tính # ; NOT NULL <=> cột "Bắt buộc" = Có;
  - khóa ngoại = 34 thuộc tính quan hệ của 3.1 mục 9 (+ các khóa ngoại bổ sung ghi ở BO_SUNG).
Chạy: python csdl/sinh_mo_ta.py
"""
import re
import sys
from pathlib import Path

GOC = Path(__file__).resolve().parents[1]
# Khóa ngoại thêm ở mức vật lý, không có trong 3.1 mục 9 – kèm lý do
BO_SUNG = {"FK_ThongBao_GiamHo": "phụ huynh nhận thông báo phải là người giám hộ của học viên đó (3.1 E24)"}


def doc_schema():
    sql = (GOC / "csdl/schema.sql").read_text(encoding="utf-8")
    bang = {}
    for ten, than in re.findall(r"^CREATE TABLE (\w+) \((.*?)^\);", sql, re.M | re.S):
        cot, rb = [], []
        for dong in than.splitlines():
            dong = dong.strip().rstrip(",")
            if dong.startswith("CONSTRAINT"):
                rb.append(dong)
            elif dong:
                m = re.match(r"(\w+)\s+(\w+(?:\([\d, ]+\))?)\s+(NOT NULL|NULL)\s*(IDENTITY\([\d, ]+\))?"
                             r"(?:\s*CONSTRAINT \w+ DEFAULT \((.*)\))?$", dong)
                assert m, f"{ten}: không đọc được dòng cột: {dong}"
                cot.append(dict(ten=m[1], kieu=m[2], null=m[3] == "NULL", identity=m[4], mac_dinh=m[5]))
        bang[ten] = dict(cot=cot, rb=rb)
    for ten, rb in re.findall(r"^ALTER TABLE (\w+) ADD (CONSTRAINT .*);", sql, re.M):
        bang[ten]["rb"].append(rb)
    chi_muc = re.findall(r"^CREATE (UNIQUE )?INDEX (\w+) ON (\w+) \(([^)]*)\)(?: WHERE ([^;]*))?;\s*(?:--\s*(.*))?", sql, re.M)
    return bang, chi_muc


def doc_thiet_ke():
    tt = (GOC / "thiet_ke/01_thuc_the.md").read_text(encoding="utf-8")
    thuc_the = {}
    for khoi in re.split(r"^#### E\d+\. ", tt.split("\n## 5.")[0], flags=re.M)[1:]:
        ten_vn, tep, loai = re.match(r"(.+?) \(`(\w+)`\) – thực thể (.+)", khoi).groups()
        dong = []
        for d in khoi.splitlines():  # tách ô theo dấu | không bị escape (ghi chú có dạng [A \| B])
            o = [x.strip() for x in re.split(r"(?<!\\)\|", d)[1:-1]]
            if len(o) == 5 and re.fullmatch(r"\w+", o[1]) and o[3] in ("Có", "Không"):
                dong.append(o)
        thuc_the[tep] = dict(ten=ten_vn, loai=loai, cot=[dict(vn=a, ten=b, loai=c, bat_buoc=d == "Có",
                                                              ghi_chu=e.replace("\\|", "|")) for a, b, c, d, e in dong])
    muc8 = tt.split("\n## 8.")[1].split("\n## 9.")[0]
    for tep, kho in re.findall(r"^\| \d+ \| [^|]+ \| (\w+) \| [^|]+ \| (D\d) \|", muc8, re.M):
        thuc_the[tep]["kho"] = kho
    muc9 = tt.split("\n## 9.")[1].split("\n## 10.")[0]
    fk = {(chua, tuple(c.strip() for c in cot.strip("()").split(",")), toi)
          for chua, cot, toi in re.findall(r"^\| \d+ \| (\w+) \| ([^|]+?) \| (\w+) \|", muc9, re.M)}
    return thuc_the, fk


def main():
    bang, chi_muc = doc_schema()
    tk, fk_tk = doc_thiet_ke()
    assert set(bang) == set(tk), f"Bảng lệch với 3.1: {set(bang) ^ set(tk)}"
    fk_sql = {}
    for t, b in bang.items():
        cot_tk = tk[t]["cot"]
        assert [c["ten"] for c in b["cot"]] == [c["ten"] for c in cot_tk], f"{t}: trường lệch với 3.1"
        pk = next(re.search(r"PRIMARY KEY \(([^)]*)\)", r)[1] for r in b["rb"] if "PRIMARY KEY" in r)
        assert {c.strip() for c in pk.split(",")} == {c["ten"] for c in cot_tk if "#" in c["loai"]}, f"{t}: khóa chính lệch"
        for c, d in zip(b["cot"], cot_tk):
            assert c["null"] != d["bat_buoc"], f"{t}.{c['ten']}: NOT NULL lệch với cột Bắt buộc của 3.1"
        for r in b["rb"]:
            m = re.match(r"CONSTRAINT (\w+) FOREIGN KEY \(([^)]*)\) REFERENCES (\w+)", r)
            if m:
                fk_sql[m[1]] = (t, tuple(c.strip() for c in m[2].split(",")), m[3])
    thieu = fk_tk - {v for k, v in fk_sql.items() if k not in BO_SUNG}
    thua = {k for k, v in fk_sql.items() if v not in fk_tk} - set(BO_SUNG)
    assert not thieu and not thua, f"Khóa ngoại lệch 3.1: thiếu {thieu}, thừa {thua}"

    # ---------- sinh mô tả ----------
    out = ["# Mô tả các tệp dữ liệu (sinh tự động từ csdl/schema.sql – không sửa tay)", "",
           "Chạy `python csdl/sinh_mo_ta.py` để kiểm tra lại với thiết kế 3.1 và sinh lại tệp này. "
           "Cột *Ý nghĩa* lấy từ bảng thực thể của 3.1 (`thiet_ke/01_thuc_the.md`).", ""]
    for i, (t, b) in enumerate(bang.items(), 1):
        d = tk[t]
        out += [f"## Bảng {i}. {t} – {d['ten']} (kho {d['kho']}, thực thể {d['loai']})", "",
                "| Tên trường | Kiểu dữ liệu | Ràng buộc | Ý nghĩa |", "|---|---|---|---|"]
        rieng = {}
        for r in b["rb"]:
            m = re.match(r"CONSTRAINT (\w+) (.*)", r)
            ten_rb, noi_dung = m[1], m[2]
            if noi_dung.startswith("CHECK"):
                cot = ten_rb.split("_", 2)[2] if ten_rb.count("_") >= 2 else ""
                rieng.setdefault(cot, []).append(noi_dung[len("CHECK "):])
        pk_cols = {c.strip() for c in re.search(r"PRIMARY KEY \(([^)]*)\)", " ".join(b["rb"]))[1].split(",")}
        for c, dtk in zip(b["cot"], d["cot"]):
            rb = []
            if c["ten"] in pk_cols:
                rb.append("PK")
            for ten_fk, (chua, cot, toi) in fk_sql.items():
                if chua == t and c["ten"] in cot:
                    rb.append(f"FK → {toi}")
            rb.append("NULL" if c["null"] else "NOT NULL")
            if c["identity"]:
                rb.append("tự tăng")
            if c["mac_dinh"]:
                rb.append(f"mặc định {c['mac_dinh']}")
            for r in b["rb"]:
                m = re.match(r"CONSTRAINT \w+ UNIQUE \(([^)]*)\)", r)
                if m and [x.strip() for x in m[1].split(",")] == [c["ten"]]:
                    rb.append("UNIQUE")
            rb += [f"CHECK {e}" for e in rieng.pop(c["ten"], [])]
            y_nghia = dtk["vn"] + (f". {dtk['ghi_chu']}" if dtk["ghi_chu"] else "")
            o = lambda s: s.replace("|", "\\|")  # ký tự | trong bảng Markdown
            out.append(f"| {c['ten']} | {c['kieu']} | {o('; '.join(rb))} | {o(y_nghia)} |")
        nhieu_cot = [r for r in b["rb"] if re.match(r"CONSTRAINT \w+ UNIQUE \([^)]*,", r)] + \
                    [f"CHECK {e}" for es in rieng.values() for e in es] + \
                    [f"{k}: khóa ngoại bổ sung – {BO_SUNG[k]}" for k, v in fk_sql.items() if k in BO_SUNG and v[0] == t] + \
                    [f"FOREIGN KEY ({', '.join(cot)}) → {toi}" for k, (chua, cot, toi) in fk_sql.items()
                     if chua == t and len(cot) > 1 and k not in BO_SUNG]
        if nhieu_cot:
            out += ["", "Ràng buộc trên nhiều trường: " + "; ".join(
                re.sub(r"^CONSTRAINT \w+ ", "", x).replace("|", "\\|") for x in nhieu_cot) + "."]
        out.append("")
    out += ["## Chỉ mục", "", "| Chỉ mục | Bảng | Trường | Mục đích |", "|---|---|---|---|"]
    for unique, ten, t, cot, dk, muc_dich in chi_muc:
        out.append(f"| {ten}{' (UNIQUE)' if unique else ''} | {t} | {cot}{f' khi {dk}' if dk else ''} | "
                   f"{muc_dich or ('Quan hệ 1–1: mỗi chủ sở hữu tối đa một tài khoản' if unique else '')} |")
    (GOC / "csdl/mo_ta_bang.md").write_text("\n".join(out) + "\n", encoding="utf-8")
    so_cot = sum(len(b["cot"]) for b in bang.values())
    print(f"schema.sql ↔ 3.1: đạt – {len(bang)} bảng, {so_cot} trường, {len(fk_sql)} khóa ngoại "
          f"({len(fk_tk)} theo 3.1 + {len(BO_SUNG)} bổ sung), {len(chi_muc)} chỉ mục")
    print("Đã ghi csdl/mo_ta_bang.md")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
