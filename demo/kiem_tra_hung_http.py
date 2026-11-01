"""TC37/TC38: HTTP thật và đối chiếu SQL Server, chỉ chạy trên CSDL demo.

Đặt NHOM10_SQL_CONNECTION trỏ tới cùng CSDL với ứng dụng rồi chạy:
python demo/kiem_tra_hung_http.py --url http://localhost:5223
Script đăng nhập 8 vai trò, thử HTML/Excel và POST chống sửa phiên bản.
Tham số chính sách được khôi phục trong finally; nhật ký thử vẫn được giữ.
"""
import argparse
import base64
import http.cookiejar
import io
import json
import os
import subprocess
import sys
import urllib.parse
import urllib.request
import zipfile
from decimal import Decimal
from html.parser import HTMLParser
from xml.etree import ElementTree as ET


def sql(query):
    # Mật khẩu chỉ đi qua biến môi trường, không xuất hiện trong lệnh hoặc kết quả.
    script = r"""
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.Encoding]::UTF8
$cn = New-Object System.Data.SqlClient.SqlConnection $env:NHOM10_SQL_CONNECTION
$cn.Open()
try {
    $cmd = $cn.CreateCommand()
    $cmd.CommandText = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('QUERY'))
    $reader = $cmd.ExecuteReader()
    while ($reader.Read()) { [Console]::Write($reader.GetString(0)) }
    $reader.Dispose()
} finally { $cn.Dispose() }
""".replace("QUERY", base64.b64encode(query.encode()).decode())
    result = subprocess.run(
        ["powershell", "-NoProfile", "-EncodedCommand",
         base64.b64encode(script.encode("utf-16le")).decode()],
        capture_output=True, encoding="utf-8", check=True)
    return json.loads(result.stdout.lstrip("\ufeff") or "[]", parse_float=Decimal)


class Html(HTMLParser):
    def __init__(self, value):
        super().__init__()
        self.token = None
        self.inputs = set()
        self.sections = {"tbody": [], "tfoot": []}
        self.section = None
        self.row = None
        self.cell = None
        self.feed(value)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == "input":
            self.inputs.add(attrs.get("name"))
            if attrs.get("name") == "__RequestVerificationToken":
                self.token = attrs.get("value")
        if tag in self.sections:
            self.section = tag
        if tag == "tr" and self.section:
            self.row = []
        if tag in ("td", "th") and self.row is not None:
            self.cell = ""
        if tag in ("br", "div") and self.cell is not None:
            self.cell += " "

    def handle_endtag(self, tag):
        if tag in ("td", "th") and self.cell is not None:
            self.row.append(" ".join(self.cell.split()))
            self.cell = None
        if tag == "tr" and self.row is not None:
            self.sections[self.section].append(self.row)
            self.row = None
        if tag in self.sections:
            self.section = None

    def handle_data(self, value):
        if self.cell is not None:
            self.cell += value


class Session:
    def __init__(self, url, user):
        self.url = url.rstrip("/")
        self.opener = urllib.request.build_opener(
            urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
        _, content = self.get("/DangNhap")
        target, _ = self.post("/DangNhap", {
            "TenDangNhap": user, "MatKhau": user,
            "__RequestVerificationToken": Html(content.decode()).token})
        assert urllib.parse.urlparse(target).path == "/", f"Đăng nhập {user} thất bại"

    def get(self, path):
        with self.opener.open(self.url + path, timeout=60) as response:
            assert response.status == 200
            return response.url, response.read()

    def post(self, path, fields):
        request = urllib.request.Request(self.url + path,
                                         urllib.parse.urlencode(fields).encode())
        with self.opener.open(request, timeout=60) as response:
            assert response.status == 200
            return response.url, response.read()


def money(value):
    return f"{value:,.0f}".replace(",", ".") + " đ"


def rate(value):
    return format(Decimal(value).normalize(), "f").replace(".", ",")


def expected_row(h):
    discounts = "; ".join(f"{u['TenUD']} ({rate(u['TyLeGiam'])}%)" for u in h.get('UuDai', []))
    details = "; ".join(
        f"Đợt {d['Dot']}: {money(d['SoTien'])}; hạn {d['Han']}; đã thu {money(d['DaThu'])}"
        for d in h['Dot'])
    return [h['MaHV'], h['HoTen'], h['MaLop'], h['HocPhiGoc'], discounts,
            h['TyLeGiam'], sum(d['SoTien'] for d in h['Dot']), details,
            sum(d['DaThu'] for d in h['Dot'])]


def check_report(session, rows, filters=None, page=1):
    params = dict(filters or {}, trang=page, so=20)
    path = "/HocPhi/HocPhiPhaiThu?" + urllib.parse.urlencode(params)
    target, content = session.get(path)
    assert urllib.parse.urlparse(target).path == "/HocPhi/HocPhiPhaiThu", target
    html = Html(content.decode())
    actual_page = max(1, min(page, (len(rows) + 19) // 20))
    visible = rows[(actual_page - 1) * 20:actual_page * 20]
    actual = html.sections['tbody']
    if not visible:
        assert actual == [["Không có khoản học phí phù hợp."]], actual
    else:
        assert len(actual) == len(visible)
        for cells, h in zip(actual, visible):
            e = expected_row(h)
            assert cells[:3] == [f"{e[0]} {e[1]}", e[2], money(e[3])], cells
            assert cells[3] == f"{e[4] or 'Không có'} Tổng giảm: {rate(e[5])}%", cells
            assert cells[4] == money(e[6]), cells
            assert cells[5] == e[7].replace("; Đợt", " Đợt"), cells
    total = [sum(h['HocPhiGoc'] for h in rows),
             sum(d['SoTien'] for h in rows for d in h['Dot']),
             sum(d['DaThu'] for h in rows for d in h['Dot'])]
    assert html.sections['tfoot'] == [["Tổng toàn bộ kết quả lọc", money(total[0]),
                                       "", money(total[1]), "Đã thu: " + money(total[2])]]
    _, data = session.get(path + "&xuat=excel")
    with zipfile.ZipFile(io.BytesIO(data)) as archive:
        root = ET.fromstring(archive.read("xl/worksheets/sheet1.xml"))
    ns = {'s': 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'}
    exported = []
    for row in root.findall('.//s:row', ns)[1:]:
        exported.append([(''.join(c.itertext()) if c.get('t') == 'inlineStr'
                          else Decimal(c.find('s:v', ns).text) if c.find('s:v', ns) is not None
                          else None) for c in row])
    assert exported == [expected_row(h) for h in rows] + [
        ["Tổng", None, None, total[0], None, None, total[1], None, total[2]]], path


def main():
    args = argparse.ArgumentParser(description=__doc__)
    args.add_argument('--url', default='http://localhost:5223')
    url = args.parse_args().url
    if not os.environ.get('NHOM10_SQL_CONNECTION'):
        raise SystemExit('Cần đặt NHOM10_SQL_CONNECTION trỏ tới CSDL demo của ứng dụng.')
    data = sql("""
SELECT h.MaHV, hv.HoTen, h.MaLop, h.HocPhiGoc, h.TyLeGiam,
 (SELECT u.TenUD, u.TyLeGiam FROM dbo.ApDungUuDai a JOIN dbo.UuDai u ON u.MaUD=a.MaUD
  WHERE a.MaHV=h.MaHV AND a.MaLop=h.MaLop ORDER BY u.MaUD FOR JSON PATH) UuDai,
 (SELECT d.Dot, d.SoTien, CONVERT(varchar(10),d.HanDong,103) Han,
  COALESCE((SELECT SUM(c.SoTien) FROM dbo.ChiTietPhieuThu c
    WHERE c.MaHV=d.MaHV AND c.MaLop=d.MaLop AND c.Dot=d.Dot),0) DaThu
  FROM dbo.DotHocPhi d WHERE d.MaHV=h.MaHV AND d.MaLop=h.MaLop ORDER BY d.Dot FOR JSON PATH) Dot
FROM dbo.HocPhi h JOIN dbo.HocVien hv ON hv.MaHV=h.MaHV ORDER BY h.MaLop,h.MaHV FOR JSON PATH
""")
    children = {r['MaHV'] for r in sql("SELECT MaHV FROM dbo.HocVienPhuHuynh WHERE MaPH='PH0301' FOR JSON PATH")}
    # Ngăn một CSDL trống làm các phép kiểm tra phạm vi đều đạt giả.
    assert len(data) > 20 and children and any(h['MaHV'] == 'HV0412' for h in data)
    for user in ('nv001', 'nv002', 'nv003', 'nv004', 'nv005', 'gv023', 'hv0412', 'ph0301'):
        session = Session(url, user)
        if user in ('nv002', 'nv003', 'gv023'):
            target, _ = session.get('/HocPhi/HocPhiPhaiThu')
            assert urllib.parse.urlparse(target).path == '/KhongCoQuyen', (user, target)
            target, _ = session.get('/HocPhi/HocPhiPhaiThu?xuat=excel')
            assert urllib.parse.urlparse(target).path == '/KhongCoQuyen', (user, target)
            print(f'TC37 {user}: ĐẠT (HTML và Excel bị chặn)')
            continue
        rows = [h for h in data if user not in ('hv0412', 'ph0301') or
                (h['MaHV'] == 'HV0412' if user == 'hv0412' else h['MaHV'] in children)]
        for page in (1, 2):
            check_report(session, rows, page=page)
        for filters in ({'lop': 'IEK-2609'}, {'maHV': 'HV0412'},
                        {'maHV': ' hv0412 '}, {'maHV': 'HV0480'},
                        {'lop': 'KHONG-CO'}, {'lop': 'IEK-2609', 'maHV': 'HV0412'}):
            selected = [h for h in rows if
                        (not filters.get('lop') or h['MaLop'] == filters['lop']) and
                        (not filters.get('maHV') or h['MaHV'] == filters['maHV'].strip().upper())]
            check_report(session, selected, filters)
        print(f'TC37 {user}: ĐẠT ({len(rows)} khoản; lọc, phân trang, tổng, Excel khớp SQL)')
    session = Session(url, 'nv003')
    path = '/HeThong/ThamSo'
    _, content = session.get(path)
    html = Html(content.decode())
    assert 'GiaTri[PHIEN_BAN_CSDL]' not in html.inputs
    version_query = "SELECT GiaTri FROM dbo.ThamSo WHERE MaThamSo='PHIEN_BAN_CSDL' FOR JSON PATH"
    version = sql(version_query)
    assert len(version) == 1
    for value in (version[0]['GiaTri'], 999):
        _, content = session.post(path, {'__RequestVerificationToken': html.token,
                                        'GiaTri[PHIEN_BAN_CSDL]': value})
        assert 'Phiên bản CSDL chỉ được cập nhật bằng tệp nâng cấp.' in content.decode()
        assert sql(version_query) == version
        html = Html(content.decode())
    policy_query = "SELECT GiaTri FROM dbo.ThamSo WHERE MaThamSo='SISO_TOI_DA' FOR JSON PATH"
    original = sql(policy_query)[0]['GiaTri']
    changed = 18 if original != 18 else 19
    try:
        _, content = session.post(path, {'__RequestVerificationToken': html.token,
                                        'GiaTri[SISO_TOI_DA]': changed})
        assert sql(policy_query)[0]['GiaTri'] == changed
        assert sql(version_query) == version
        assert sql("SELECT COUNT(*) SoDong FROM dbo.NhatKy WHERE DoiTuong='ThamSo' AND MaDoiTuong LIKE 'SISO_TOI_DA %' FOR JSON PATH")[0]['SoDong'] > 0
    finally:
        _, latest = session.get(path)
        session.post(path, {'__RequestVerificationToken': Html(latest.decode()).token,
                            'GiaTri[SISO_TOI_DA]': original})
        assert sql(policy_query)[0]['GiaTri'] == original
    print('TC38 HTTP: ĐẠT (phiên bản chỉ đọc, POST bị chặn; chính sách lưu và ghi nhật ký)')
    print('KẾT QUẢ: ĐẠT')


if __name__ == '__main__':
    if hasattr(sys.stdout, 'reconfigure'):
        sys.stdout.reconfigure(encoding='utf-8')
    main()
