"""Sinh mã C# cho bản demo từ các bảng thiết kế (một nguồn sự thật) rồi kiểm tra mã nguồn dùng đúng các bảng đó.

- thiet_ke/05_module.md mục 5 (ma trận phân quyền)      -> TrungTamNgoaiNgu/Nen/PhanQuyen.g.cs  (module N2)
- thiet_ke/06_giao_dien.md mục 6 (thông báo lỗi)         -> TrungTamNgoaiNgu/Nen/ThongBaoLoi.g.cs (module N7)
- 05_module mục 3 (module) + 06_giao_dien mục 3, 4 (form, báo cáo) -> TrungTamNgoaiNgu/Nen/ThucDon.g.cs (thực đơn)

Kiểm tra trên mã nguồn .cs/.cshtml:
  1. mỗi [Quyen("x.y", ...)] trỏ về một dòng có thật của ma trận, quyền ghi đòi hỏi có ít nhất một vai trò được cấp;
  2. mỗi new LoiNghiepVu("KHOA") dùng khóa có trong bảng thông báo lỗi;
  3. mỗi mã màn hình ghi trên trang (ViewData["Ma"] = "F2.2") có trong bảng form / báo cáo của 06_giao_dien mục 3, 4.
Chạy: python demo/sinh_ma.py   (thoát mã 1 nếu có lỗi)
"""
import json
import re
import sys
from pathlib import Path

GOC = Path(__file__).resolve().parents[1]
APP = GOC / "demo" / "TrungTamNgoaiNgu"
VAI_TRO = ["HV", "PH", "GV", "TS", "KT", "DT", "GD", "QT"]


def bang_sau(md, tieu_de, cot_dau):
    """Các dòng của bảng đầu tiên nằm sau tiêu đề `tieu_de` có ô đầu tiên của dòng tiêu đề là `cot_dau`."""
    phan = md.split(tieu_de, 1)[1]
    dong = phan.split("\n")
    i = next(k for k, d in enumerate(dong) if d.startswith(f"| {cot_dau} |"))
    hang = []
    for d in dong[i + 2:]:
        if not d.startswith("|"):
            break
        hang.append([x.strip() for x in re.split(r"(?<!\\)\|", d.strip())[1:-1]])
    return hang


def chu(s):  # bỏ định dạng markdown của ô
    return s.replace("\\|", "|").replace("\\*", "*").replace("**", "").replace("`", "").replace("*", "")


def lit(s):
    return json.dumps(s, ensure_ascii=False)


def main():
    loi = []
    md5 = (GOC / "thiet_ke/05_module.md").read_text(encoding="utf-8")
    md6 = (GOC / "thiet_ke/06_giao_dien.md").read_text(encoding="utf-8")

    ma_tran = {}
    for h in bang_sau(md5, "## 5. Ma trận phân quyền", "Mã"):
        if len(h) != 1 + len(VAI_TRO):
            loi.append(f"Ma trận: dòng {h[0]} có {len(h) - 1} cột, cần {len(VAI_TRO)}")
            continue
        q = ["" if x == "–" else x.replace(" ", "") for x in h[1:]]
        for x in q:
            if not re.fullmatch(r"[TSDIXx]*", x):
                loi.append(f"Ma trận: ký hiệu quyền lạ '{x}' ở dòng {h[0]}")
        ma_tran[h[0]] = q

    thong_bao, dem = {}, {}
    for h in bang_sau(md6, "## 6. Trợ giúp và thông báo lỗi", "Nguồn lỗi"):
        nguon = chu(h[0])
        dem[nguon] = dem.get(nguon, 0) + 1
        khoa = nguon if dem[nguon] == 1 else f"{nguon}#{dem[nguon]}"
        thong_bao[khoa] = (chu(h[1]), chu(h[2]), chu(h[3]))

    module = [(h[0], chu(h[1]), h[2]) for h in bang_sau(md5, "## 3. Danh sách module", "Mã")]
    man_hinh = {h[0]: (chu(h[1]), h[2].split(",")[0].strip())
                for h in bang_sau(md6, "## 3. Form nhập liệu", "Mã") + bang_sau(md6, "## 4. Báo cáo, chứng từ đầu ra", "Mã")}

    # ---- kiểm tra mã nguồn ----
    nguon = [p for p in APP.rglob("*") if p.suffix in (".cs", ".cshtml") and "obj" not in p.parts
             and "bin" not in p.parts and not p.name.endswith(".g.cs")]
    for p in nguon:
        s = p.read_text(encoding="utf-8")
        ten = p.relative_to(APP)
        for m in re.finditer(r'\[Quyen\("([^"]+)"(?:,\s*"([^"]*)")?', s):
            mod, ghi = m.group(1), m.group(2) or ""
            if mod not in ma_tran:
                loi.append(f"{ten}: module {mod} không có trong ma trận phân quyền")
            elif ghi and not any(set(ghi) <= set(q) for q in ma_tran[mod]):
                loi.append(f"{ten}: không vai trò nào có quyền '{ghi}' ở module {mod}")
        for m in re.finditer(r'new LoiNghiepVu\("([^"]+)"', s):
            if m.group(1) not in thong_bao:
                loi.append(f"{ten}: khóa thông báo lỗi '{m.group(1)}' không có trong 06_giao_dien mục 6")
        for m in re.finditer(r'ViewData\["Ma"\]\s*=\s*"([^"]+)"', s):
            for ma in m.group(1).split(", "):
                if ma not in man_hinh:
                    loi.append(f"{ten}: mã màn hình {ma} không có trong 06_giao_dien mục 3, 4")

    if loi:
        print("CÓ LỖI:\n  " + "\n  ".join(loi))
        sys.exit(1)

    dau = "// Sinh tự động bởi demo/sinh_ma.py từ {} – không sửa tay; sửa bảng trong .md rồi chạy lại.\n"
    pq = [dau.format("thiet_ke/05_module.md mục 5"), "namespace TrungTamNgoaiNgu.Nen;\n",
          "public static partial class PhanQuyen\n{",
          f"    public static readonly string[] VaiTro = {{ {', '.join(lit(v) for v in VAI_TRO)} }};\n",
          "    static readonly Dictionary<string, string[]> MaTran = new()\n    {"]
    pq += [f"        [{lit(k)}] = new[] {{ {', '.join(lit(x) for x in v)} }}," for k, v in ma_tran.items()]
    pq += ["    };", "}", ""]
    (APP / "Nen/PhanQuyen.g.cs").write_text("\n".join(pq), encoding="utf-8")

    tb = [dau.format("thiet_ke/06_giao_dien.md mục 6"), "namespace TrungTamNgoaiNgu.Nen;\n",
          "public static partial class ThongBaoLoi\n{",
          "    static readonly Dictionary<string, (string ManHinh, string ThongBao, string GoiY)> Bang = new()\n    {"]
    tb += [f"        [{lit(k)}] = ({lit(a)}, {lit(b)}, {lit(c)})," for k, (a, b, c) in thong_bao.items()]
    tb += ["    };", "}", ""]
    (APP / "Nen/ThongBaoLoi.g.cs").write_text("\n".join(tb), encoding="utf-8")

    mh = [dau.format("thiet_ke/05_module.md mục 3 và 06_giao_dien.md mục 3, 4"), "namespace TrungTamNgoaiNgu.Nen;\n",
          "public static partial class ThucDon\n{",
          "    public static readonly (string Ma, string Ten, string PhanHe)[] Module =\n    {"]
    mh += [f"        ({lit(a)}, {lit(b)}, {lit(c)})," for a, b, c in module]
    mh += ["    };\n", "    public static readonly Dictionary<string, (string Ten, string Module)> ManHinh = new()\n    {"]
    mh += [f"        [{lit(k)}] = ({lit(a)}, {lit(b)})," for k, (a, b) in man_hinh.items()]
    mh += ["    };", "}", ""]
    (APP / "Nen/ThucDon.g.cs").write_text("\n".join(mh), encoding="utf-8")

    print(f"Đã sinh ThucDon.g.cs ({len(module)} module, {len(man_hinh)} màn hình), PhanQuyen.g.cs ({len(ma_tran)} module) và ThongBaoLoi.g.cs ({len(thong_bao)} thông báo); "
          f"kiểm tra {len(nguon)} tệp mã nguồn: đạt.")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    main()
