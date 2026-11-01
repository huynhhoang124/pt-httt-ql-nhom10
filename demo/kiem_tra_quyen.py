"""TC42 – Kiểm tra phân quyền 8 vai trò trên bản demo đang chạy (PHAN_CONG_SUA Q3).

Cách chạy (ứng dụng đang chạy ở http://localhost:5223, CSDL mẫu vừa dựng bằng csdl/chay.ps1):
    python demo/kiem_tra_quyen.py [http://localhost:5223]

Script tự đọc từ mã nguồn, không chép tay:
  - danh sách trang đã làm: ThucDon.DaLam trong Nen/ThucDon.cs (mã màn hình → địa chỉ);
  - module của từng màn hình: ManHinh trong Nen/ThucDon.g.cs;
  - ma trận quyền: MaTran trong Nen/PhanQuyen.g.cs (sinh từ thiet_ke/05_module.md mục 5).

Với mỗi vai trò (đăng nhập tài khoản mẫu, mật khẩu = tên đăng nhập; dùng hv0412 vì hv0480 bị bắt đổi mật khẩu):
  1. mọi liên kết trên thực đơn chính (trang /) phải mở được (200);
  2. mọi trang trong ThucDon.DaLam: vai trò không có quyền nào ở module đó phải bị chuyển về /KhongCoQuyen,
     vai trò có quyền (kể cả chỉ xem) phải mở được (200) – trừ hai quy tắc riêng mà trang ghi trong OnGet
     (script đọc từ Pages/…cshtml.cs, in ra ở đầu bảng):
       - `if (!User.Co("m", "T")) return Forbid();` → trang nhập liệu, phải có quyền ghi T mới mở (F2.2, F3.1, F3.2);
       - `if (PhanQuyen.ChiCuaMinh(User.VaiTro(), "m")…) return Forbid();` → vai trò chỉ có quyền "x" (của mình)
         xem ở trang báo cáo tương ứng (vd HV, PH, GV xem lịch ở R2.3a, R2.3b thay cho F2.3).
In bảng vai trò × trang, cuối cùng in "đạt" hoặc danh sách sai; mã thoát 1 nếu có sai.
Chỉ dùng thư viện chuẩn của Python 3.8+.
"""
import html
import http.cookiejar
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

GOC = Path(__file__).resolve().parent / "TrungTamNgoaiNgu" / "Nen"
TRANG = GOC.parent / "Pages"
BASE = (sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5223").rstrip("/")

TAI_KHOAN = [  # (cột ma trận, tên vai trò, tài khoản mẫu)
    ("HV", "Học viên", "hv0412"), ("PH", "Phụ huynh", "ph0301"), ("GV", "Giáo viên", "gv023"),
    ("TS", "NV tuyển sinh", "nv005"), ("KT", "NV kế toán", "nv004"), ("DT", "QL đào tạo", "nv002"),
    ("GD", "Giám đốc", "nv001"), ("QT", "Quản trị viên", "nv003"),
]


def doc_ma_nguon():
    td = (GOC / "ThucDon.cs").read_text(encoding="utf-8")
    khoi = re.search(r"DaLam\s*=\s*\{(.*?)\};", td, re.S).group(1)
    da_lam = re.findall(r'\("([^"]+)",\s*"([^"]+)",\s*"([^"]*)"\)', khoi)
    tdg = (GOC / "ThucDon.g.cs").read_text(encoding="utf-8")
    module_cua = dict(re.findall(r'\["([^"]+)"\]\s*=\s*\("[^"]*",\s*"([^"]+)"\)', tdg))
    pq = (GOC / "PhanQuyen.g.cs").read_text(encoding="utf-8")
    vai_tro = re.findall(r'"(\w+)"', re.search(r"VaiTro\s*=\s*\{([^}]*)\}", pq).group(1))
    ma_tran = {m: re.findall(r'"([^"]*)"', q) for m, q in re.findall(r'\["([^"]+)"\]\s*=\s*new\[\]\s*\{([^}]*)\}', pq)}
    trang = {}  # địa chỉ → (các mã màn hình, module)
    for ma, url, _can in da_lam:
        cu = trang.get(url, ([], module_cua[ma]))
        cu[0].append(ma)
        trang[url] = cu
    return vai_tro, ma_tran, trang


def quy_tac_rieng(url):
    """Điều kiện thêm trong OnGet của trang: (module cần T | None, module chặn quyền chỉ-của-mình | None, vai trò được miễn)."""
    tep = TRANG / (url.strip("/") + ".cshtml.cs")
    if not tep.exists():
        return None, None, None
    ma = tep.read_text(encoding="utf-8")
    m = re.search(r"OnGet\w*\((.*?)(?=\n    (?:public|async|private|static|\[)|\Z)", ma, re.S)
    than = m.group(1) if m else ""
    can_t = re.search(r'if \(!User\.Co\("([\w.]+)", "T"\)\) return Forbid\(\);', than)
    chi = re.search(r'if \(PhanQuyen\.ChiCuaMinh\(User\.VaiTro\(\), "([\w.]+)"\)(?: && User\.VaiTro\(\) != "(\w+)")?\) return Forbid\(\);', than)
    return (can_t.group(1) if can_t else None), (chi.group(1) if chi else None), (chi.group(2) if chi else None)


class KhongChuyenHuong(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, *a, **k):
        return None


def phien():
    jar = http.cookiejar.CookieJar()
    tu_dong = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
    dung = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar), KhongChuyenHuong)
    return tu_dong, dung


def mo(opener, url, data=None):
    """Trả (mã HTTP, Location, nội dung)."""
    try:
        r = opener.open(BASE + url, data=urllib.parse.urlencode(data).encode() if data else None, timeout=30)
        return r.status, "", r.read().decode("utf-8", "replace")
    except urllib.error.HTTPError as e:
        return e.code, e.headers.get("Location", ""), ""


def dang_nhap(ten):
    tu_dong, dung = phien()
    _, _, h = mo(tu_dong, "/DangNhap")
    token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', h).group(1)
    ma, vi_tri, _ = mo(dung, "/DangNhap", {"__RequestVerificationToken": token, "TenDangNhap": ten, "MatKhau": ten})
    if ma not in (302, 303) or "DangNhap" in vi_tri or "DoiMatKhau" in vi_tri:
        raise SystemExit(f"Không đăng nhập được {ten} (HTTP {ma} → {vi_tri or 'ở lại trang đăng nhập'}). "
                         "Dựng lại CSDL mẫu bằng csdl/chay.ps1 rồi chạy lại.")
    return tu_dong, dung


def main():
    vai_tro, ma_tran, trang = doc_ma_nguon()
    try:
        mo(phien()[0], "/DangNhap")
    except urllib.error.URLError as e:
        raise SystemExit(f"Không mở được {BASE}: {e.reason}. Chạy ứng dụng trước (dotnet run --launch-profile http).")
    print(f"TC42 – kiểm tra phân quyền tại {BASE}: {len(trang)} trang ({sum(len(v[0]) for v in trang.values())} mã màn hình), "
          f"{len(ma_tran)} module, {len(TAI_KHOAN)} vai trò\n")
    rieng = {url: quy_tac_rieng(url) for url in trang}
    for url, (t, c, mien) in rieng.items():
        if t or c:
            print(f"  quy tắc riêng {'/'.join(trang[url][0]):<6} {url}: "
                  + (f"cần quyền ghi T ở {t}" if t else f"chặn vai trò chỉ có quyền x ở {c}" + (f" (trừ {mien})" if mien else "")))
    print()
    sai, bang, so_lan = [], {}, 0
    for cot, ten_vt, tk in TAI_KHOAN:
        i = vai_tro.index(cot)
        tu_dong, dung = dang_nhap(tk)
        _, _, chu = mo(tu_dong, "/")
        tren_menu = {html.unescape(u) for u in re.findall(r'href="(/[^"#?]+)"', chu)} & set(trang)
        for url, (cac_ma, module) in trang.items():
            quyen = ma_tran.get(module, [""] * len(vai_tro))[i]
            t, c, mien = rieng[url]
            duoc_mo = bool(quyen)
            if t and "T" not in ma_tran[t][i]:
                duoc_mo = False
            if c and "X" not in ma_tran[c][i] and cot != mien:
                duoc_mo = False
            ma, vi_tri, _ = mo(dung, url)
            so_lan += 1
            bi_chan = ma in (302, 303) and "/KhongCoQuyen" in vi_tri
            if duoc_mo:
                dung_mt, o = ma == 200, ("mở" if ma == 200 else f"SAI {ma}")
            else:
                dung_mt, o = bi_chan, ("chặn" if bi_chan else f"LỌT {ma}")
            if url in tren_menu and ma != 200:
                dung_mt, o = False, f"MENU {ma}"
            if url in tren_menu and not duoc_mo:
                dung_mt, o = False, "MENU lộ"
            bang[(cot, url)] = o + ("*" if url in tren_menu else "")
            if not dung_mt:
                sai.append(f"{ten_vt} ({tk}) – {'/'.join(cac_ma)} {url}: quyền '{quyen}' ở module {module}, HTTP {ma} {vi_tri}")

    rong = max(len("/".join(v[0])) for v in trang.values())
    print(f"{'Màn hình':<{rong}}  {'Module':<6} " + " ".join(f"{c:<6}" for c, _, _ in TAI_KHOAN))
    for url, (cac_ma, module) in trang.items():
        print(f"{'/'.join(cac_ma):<{rong}}  {module:<6} " + " ".join(f"{bang[(c, url)]:<6}" for c, _, _ in TAI_KHOAN))
    print("\n(* = có trên thực đơn của vai trò đó; mở = 200, chặn = chuyển về /KhongCoQuyen)")
    print(f"Đã thử {so_lan} lượt mở trang.")
    if sai:
        print(f"\nCÓ SAI ({len(sai)}):")
        for s in sai:
            print(" -", s)
        sys.exit(1)
    print("\nđạt")


if __name__ == "__main__":
    main()
