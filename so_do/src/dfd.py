"""DFD mức ngữ cảnh và mức 0, sinh từ cùng một bộ luồng nên luôn cân bằng.

LUONG_TN và LUONG_KHO là nguồn duy nhất; sơ đồ ngữ cảnh gộp LUONG_TN theo tác nhân.
Chạy: python so_do/src/dfd.py  ->  kiểm tra quy tắc và cân bằng, ghi DFD_muc_ngu_canh.png,
DFD_muc_0.png, DFD_muc_1_1.png … DFD_muc_1_5.png và can_bang_dfd.md trong so_do/.
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
    ("D2", "2.0", "doc", "Đăng ký, sĩ số, lịch đã xếp"),
    ("D3", "2.0", "doc", "Tình trạng đóng học phí"),
    ("D1", "3.0", "doc", "Học phí khóa học"),
    ("D2", "3.0", "doc", "Đăng ký học"),
    ("D3", "3.0", "ghi", "Ưu đãi, học phí, phiếu thu"),
    ("D3", "3.0", "doc", "Học phí phải thu, số đã thu"),
    ("D1", "4.0", "doc", "Trọng số điểm khóa học"),
    ("D2", "4.0", "doc", "Danh sách lớp, buổi học"),
    ("D4", "4.0", "ghi", "Điểm danh, điểm, kết quả, chứng nhận"),
    ("D4", "4.0", "doc", "Điểm danh, điểm thành phần"),
    ("D1", "5.0", "doc", "Hồ sơ học viên, giáo viên"),
    ("D2", "5.0", "doc", "Lớp, đăng ký, buổi học"),
    ("D3", "5.0", "doc", "Công nợ"),
    ("D4", "5.0", "doc", "Chuyên cần, kết quả"),
    ("D5", "5.0", "ghi", "Tài khoản, thông báo, nhật ký"),
    ("D5", "5.0", "doc", "Quyền truy cập, nhật ký"),
]

# DFD mức 1: phân rã từng tiến trình x.0 thành các chức năng con x.1–x.4 của BFD.
#   tn / kho: như mức 0 nhưng nối với chức năng con; phần tử thứ 5 (nếu có) là tên luồng
#             mức 0 mà luồng này tách ra từ đó – thiếu thì hiểu là trùng tên (dùng để kiểm tra cân bằng).
#   noi_bo:   (từ, đến, tên luồng) giữa hai chức năng con.
#   thu_tu:   thứ tự vẽ từ trên xuống; luồng nội bộ phải nối hai chức năng liền kề, đi xuống.
_TT1, _TT2 = "Thông tin khóa học, giáo viên, phòng học", "Học viên, giáo viên, khóa học, phòng học"
MUC_1 = {
    "1.0": dict(
        tn=[
            ("TS", "1.1", "vao", "Hồ sơ học viên, kết quả kiểm tra đầu vào"),
            ("DT", "1.2", "vao", "Thông tin giáo viên", _TT1),
            ("DT", "1.3", "vao", "Thông tin khóa học", _TT1),
            ("DT", "1.4", "vao", "Thông tin phòng học", _TT1),
        ],
        kho=[
            ("D1", "1.1", "doc", "Hồ sơ hiện có"),
            ("D1", "1.1", "ghi", "Hồ sơ học viên", "Hồ sơ cập nhật"),
            ("D1", "1.2", "ghi", "Hồ sơ giáo viên", "Hồ sơ cập nhật"),
            ("D1", "1.3", "ghi", "Khóa học", "Hồ sơ cập nhật"),
            ("D1", "1.4", "ghi", "Phòng học", "Hồ sơ cập nhật"),
        ],
        noi_bo=[],
        thu_tu=["1.1", "1.2", "1.3", "1.4"],
    ),
    "2.0": dict(
        tn=[
            ("DT", "2.1", "vao", "Kế hoạch mở lớp, phân công giáo viên"),
            ("HV", "2.2", "vao", "Yêu cầu đăng ký", "Yêu cầu đăng ký, chuyển lớp, bảo lưu"),
            ("TS", "2.2", "vao", "Phiếu đăng ký", "Phiếu đăng ký, đơn chuyển lớp, bảo lưu"),
            ("TS", "2.2", "ra", "Kết quả đăng ký, danh sách lớp"),
            ("HV", "2.3", "ra", "Lịch học"),
            ("GV", "2.3", "ra", "Lịch dạy, danh sách lớp"),
            ("HV", "2.4", "vao", "Yêu cầu chuyển lớp, bảo lưu", "Yêu cầu đăng ký, chuyển lớp, bảo lưu"),
            ("TS", "2.4", "vao", "Đơn chuyển lớp, bảo lưu", "Phiếu đăng ký, đơn chuyển lớp, bảo lưu"),
            ("TS", "2.4", "ra", "Kết quả xử lý đăng ký", "Kết quả đăng ký, danh sách lớp"),
        ],
        kho=[
            ("D1", "2.1", "doc", "Khóa học, giáo viên", _TT2),
            ("D2", "2.1", "doc", "Sĩ số đăng ký", "Đăng ký, sĩ số, lịch đã xếp"),
            ("D2", "2.1", "ghi", "Lớp học", "Lớp, đăng ký, lịch học"),
            ("D1", "2.3", "doc", "Giáo viên, phòng học", _TT2),
            ("D2", "2.3", "doc", "Lịch đã xếp", "Đăng ký, sĩ số, lịch đã xếp"),
            ("D2", "2.3", "ghi", "Lịch học (buổi học)", "Lớp, đăng ký, lịch học"),
            ("D1", "2.2", "doc", "Học viên", _TT2),
            ("D2", "2.2", "doc", "Sĩ số lớp", "Đăng ký, sĩ số, lịch đã xếp"),
            ("D3", "2.2", "doc", "Tình trạng đóng học phí"),
            ("D2", "2.2", "ghi", "Đăng ký", "Lớp, đăng ký, lịch học"),
            ("D2", "2.4", "doc", "Đăng ký, sĩ số, buổi đã học", "Đăng ký, sĩ số, lịch đã xếp"),
            ("D3", "2.4", "doc", "Tình trạng đóng học phí"),
            ("D2", "2.4", "ghi", "Đăng ký cập nhật", "Lớp, đăng ký, lịch học"),
        ],
        noi_bo=[("2.1", "2.3", "Lớp đã mở")],
        thu_tu=["2.1", "2.3", "2.2", "2.4"],
    ),
    "3.0": dict(
        tn=[
            ("KT", "3.1", "vao", "Ưu đãi", "Thông tin thu tiền, ưu đãi"),
            ("KT", "3.1", "ra", "Học phí phải thu", "Học phí phải thu, phiếu thu, công nợ"),
            ("HV", "3.2", "vao", "Tiền học phí"),
            ("KT", "3.2", "vao", "Thông tin thu tiền", "Thông tin thu tiền, ưu đãi"),
            ("HV", "3.2", "ra", "Phiếu thu"),
            ("KT", "3.2", "ra", "Phiếu thu", "Học phí phải thu, phiếu thu, công nợ"),
            ("KT", "3.3", "ra", "Công nợ", "Học phí phải thu, phiếu thu, công nợ"),
            ("GD", "3.4", "vao", "Yêu cầu báo cáo doanh thu"),
            ("GD", "3.4", "ra", "Báo cáo doanh thu, công nợ"),
        ],
        kho=[
            ("D1", "3.1", "doc", "Học phí khóa học"),
            ("D2", "3.1", "doc", "Đăng ký học"),
            ("D3", "3.1", "ghi", "Ưu đãi, học phí phải thu", "Ưu đãi, học phí, phiếu thu"),
            ("D3", "3.2", "doc", "Học phí phải thu, số đã thu"),
            ("D3", "3.2", "ghi", "Phiếu thu mới", "Ưu đãi, học phí, phiếu thu"),
            ("D2", "3.3", "doc", "Trạng thái đăng ký", "Đăng ký học"),
            ("D3", "3.3", "doc", "Học phí phải thu, số đã thu"),
            ("D3", "3.4", "doc", "Số đã thu theo kỳ", "Học phí phải thu, số đã thu"),
        ],
        noi_bo=[("3.3", "3.4", "Tổng công nợ")],
        thu_tu=["3.1", "3.2", "3.3", "3.4"],
    ),
    "4.0": dict(
        tn=[
            ("GV", "4.1", "vao", "Phiếu điểm danh buổi học", "Điểm danh, điểm số, nhận xét"),
            ("HV", "4.1", "ra", "Chuyên cần", "Chuyên cần, kết quả học tập, chứng nhận"),
            ("GV", "4.2", "vao", "Điểm số, nhận xét", "Điểm danh, điểm số, nhận xét"),
            ("HV", "4.3", "ra", "Kết quả học tập", "Chuyên cần, kết quả học tập, chứng nhận"),
            ("HV", "4.4", "ra", "Chứng nhận", "Chuyên cần, kết quả học tập, chứng nhận"),
        ],
        kho=[
            ("D2", "4.1", "doc", "Danh sách lớp, buổi học"),
            ("D4", "4.1", "ghi", "Điểm danh đã ghi", "Điểm danh, điểm, kết quả, chứng nhận"),
            ("D2", "4.2", "doc", "Danh sách lớp", "Danh sách lớp, buổi học"),
            ("D4", "4.2", "ghi", "Điểm thành phần", "Điểm danh, điểm, kết quả, chứng nhận"),
            ("D1", "4.3", "doc", "Trọng số điểm khóa học"),
            ("D4", "4.3", "doc", "Điểm danh, điểm thành phần"),
            ("D4", "4.3", "ghi", "Kết quả học tập", "Điểm danh, điểm, kết quả, chứng nhận"),
            ("D4", "4.4", "ghi", "Chứng nhận", "Điểm danh, điểm, kết quả, chứng nhận"),
        ],
        noi_bo=[("4.3", "4.4", "Danh sách học viên đạt")],
        thu_tu=["4.1", "4.2", "4.3", "4.4"],
    ),
    "5.0": dict(
        tn=[
            ("QT", "5.1", "vao", "Tài khoản, phân quyền, cấu hình"),
            ("QT", "5.1", "ra", "Nhật ký hệ thống"),
            ("HV", "5.2", "ra", "Thông báo"),
            ("GD", "5.3", "vao", "Yêu cầu báo cáo tổng hợp"),
            ("GD", "5.3", "ra", "Báo cáo tuyển sinh", "Báo cáo tuyển sinh, kết quả học tập"),
            ("DT", "5.3", "ra", "Báo cáo lớp học", "Báo cáo lớp học, chuyên cần, kết quả, giảng dạy"),
            ("GD", "5.4", "vao", "Yêu cầu báo cáo tổng hợp"),
            ("GD", "5.4", "ra", "Báo cáo kết quả học tập", "Báo cáo tuyển sinh, kết quả học tập"),
            ("DT", "5.4", "ra", "Báo cáo chuyên cần, kết quả, giảng dạy",
             "Báo cáo lớp học, chuyên cần, kết quả, giảng dạy"),
        ],
        kho=[
            ("D1", "5.1", "doc", "Hồ sơ học viên, giáo viên"),
            ("D5", "5.1", "doc", "Quyền truy cập, nhật ký"),
            ("D5", "5.1", "ghi", "Tài khoản, nhật ký", "Tài khoản, thông báo, nhật ký"),
            ("D1", "5.2", "doc", "Liên hệ học viên, phụ huynh", "Hồ sơ học viên, giáo viên"),
            ("D2", "5.2", "doc", "Lịch học thay đổi", "Lớp, đăng ký, buổi học"),
            ("D3", "5.2", "doc", "Công nợ"),
            ("D4", "5.2", "doc", "Chuyên cần, kết quả"),
            ("D5", "5.2", "ghi", "Thông báo đã gửi", "Tài khoản, thông báo, nhật ký"),
            ("D1", "5.3", "doc", "Hồ sơ học viên", "Hồ sơ học viên, giáo viên"),
            ("D2", "5.3", "doc", "Lớp, đăng ký", "Lớp, đăng ký, buổi học"),
            ("D1", "5.4", "doc", "Giáo viên", "Hồ sơ học viên, giáo viên"),
            ("D2", "5.4", "doc", "Lớp, buổi học", "Lớp, đăng ký, buổi học"),
            ("D4", "5.4", "doc", "Chuyên cần, kết quả"),
        ],
        noi_bo=[],
        thu_tu=["5.1", "5.2", "5.3", "5.4"],
    ),
}


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


def con_cua(p):
    ma = p.split(".")[0]
    return {f"{ma}.{i}": ten for i, ten in enumerate(CHUC_NANG[p][1], 1)}


def cha(luong):
    """Tên luồng mức 0 mà luồng mức 1 tách ra từ đó."""
    return luong[4] if len(luong) > 4 else luong[3]


def kiem_tra_muc_1():
    """Cân bằng mức 1 với mức 0 + quy tắc vẽ cho từng sơ đồ DFD-x.0."""
    assert set(MUC_1) == set(CHUC_NANG), "Mỗi tiến trình mức 0 cần đúng một sơ đồ mức 1"
    for p, d in MUC_1.items():
        cons = con_cua(p)
        assert sorted(d["thu_tu"]) == sorted(cons), f"DFD-{p}: thu_tu phải gồm đủ {sorted(cons)}"
        for tn, q, h, *_ in d["tn"]:
            assert q in cons, (p, q)
        for k, q, h, *_ in d["kho"]:
            assert q in cons, (p, q)
        # cân bằng: mỗi luồng mức 1 tách từ một luồng mức 0 cùng tác nhân/kho, cùng hướng, và ngược lại
        tren_tn = {(tn, h, t) for tn, q, h, t in LUONG_TN if q == p}
        duoi_tn = {(l[0], l[2], cha(l)) for l in d["tn"]}
        tren_kho = {(k, h, t) for k, q, h, t in LUONG_KHO if q == p}
        duoi_kho = {(l[0], l[2], cha(l)) for l in d["kho"]}
        assert duoi_tn == tren_tn, f"DFD-{p} không cân bằng tác nhân: thừa {duoi_tn - tren_tn}, thiếu {tren_tn - duoi_tn}"
        assert duoi_kho == tren_kho, f"DFD-{p} không cân bằng kho: thừa {duoi_kho - tren_kho}, thiếu {tren_kho - duoi_kho}"
        for a, b, _ in d["noi_bo"]:
            assert a in cons and b in cons and a != b, (p, a, b)
            assert d["thu_tu"].index(b) == d["thu_tu"].index(a) + 1, f"DFD-{p}: đặt {a} ngay trên {b}"
        for c in cons:
            vao = {l[3] for l in d["tn"] if l[1] == c and l[2] == "vao"} |                   {l[3] for l in d["kho"] if l[1] == c and l[2] == "doc"} | {t for _, b, t in d["noi_bo"] if b == c}
            ra = {l[3] for l in d["tn"] if l[1] == c and l[2] == "ra"} |                  {l[3] for l in d["kho"] if l[1] == c and l[2] == "ghi"} | {t for a, _, t in d["noi_bo"] if a == c}
            assert vao and ra, f"Chức năng {c} thiếu luồng vào hoặc ra"
            assert not vao & ra, f"Chức năng {c} có luồng ra trùng luồng vào: {vao & ra}"
    print("Kiểm tra cân bằng DFD mức 1: đạt")


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


def ve_theo_dai(tieu_de, tien_trinh_ds, luong_tn, luong_kho, noi_bo, ten_file, n_min=1):
    """Mỗi tiến trình một dải ngang: tác nhân bên trái, kho bên phải (vẽ lặp nếu cần)."""
    HANG, KHOANG = 1.9, 1.0
    X_TN, W_TN, X_P, W_P, X_K, W_K = 0, 4.2, 9.2, 3.8, 18.0, 4.2
    dai = []
    for p, ten in tien_trinh_ds:
        tns = list(dict.fromkeys(l[0] for l in luong_tn if l[1] == p))
        khos = sorted({l[0] for l in luong_kho if l[1] == p})
        dai.append((p, ten, tns, khos, max(len(tns), len(khos), n_min)))
    cao = sum(n * HANG + KHOANG for *_, n in dai)
    fig, ax = plt.subplots(figsize=(22.5 * 0.62, (cao + 1.5) * 0.62))

    da_ve_tn, da_ve_kho, hop, yt = set(), set(), {}, 0.0
    for p, ten, tns, khos, n in dai:
        cy, h = yt - n * HANG / 2, n * HANG - 0.6
        tien_trinh(ax, X_P + W_P / 2, cy, W_P, h, p, fill(ten, 18))
        hop[p] = (cy + h / 2, cy - h / 2)
        for i, tn in enumerate(tns):
            y = yt - HANG * (i + 0.5 + (n - len(tns)) / 2)
            tac_nhan(ax, X_TN, y, W_TN, 1.0, tn, lap=tn in da_ve_tn)
            da_ve_tn.add(tn)
            vao = "\n".join(l[3] for l in luong_tn if l[0] == tn and l[1] == p and l[2] == "vao")
            ra = "\n".join(l[3] for l in luong_tn if l[0] == tn and l[1] == p and l[2] == "ra")
            cap_luong(ax, X_TN + W_TN, X_P, y, vao, ra, 42, 8)
        for i, k in enumerate(khos):
            y = yt - HANG * (i + 0.5 + (n - len(khos)) / 2)
            kho(ax, X_K, y, W_K, 0.8, k, lap=k in da_ve_kho)
            da_ve_kho.add(k)
            ghi = "\n".join(l[3] for l in luong_kho if l[0] == k and l[1] == p and l[2] == "ghi")
            doc = "\n".join(l[3] for l in luong_kho if l[0] == k and l[1] == p and l[2] == "doc")
            cap_luong(ax, X_P + W_P, X_K, y, ghi, doc, 42, 8)
        yt -= n * HANG + KHOANG
    for a, b, t in noi_bo:  # luồng giữa hai chức năng con liền kề, đi xuống
        x = X_P + W_P / 2
        mui_ten(ax, x, hop[a][1], x, hop[b][0])
        ax.text(x + 0.15, (hop[a][1] + hop[b][0]) / 2, t, ha="left", va="center", fontsize=8)

    ax.text((X_TN + X_K + W_K) / 2, 1.0, tieu_de, ha="center", fontsize=13, weight="bold")
    ax.text(X_TN, yt + 0.3, "Ghi chú: tác nhân có gạch chéo góc và kho có thêm vạch đứng là hình vẽ lặp.",
            fontsize=8.5, style="italic")
    ax.set_xlim(-0.3, X_K + W_K + 0.3)
    ax.set_ylim(yt - 0.2, 1.5)
    ax.set_aspect("equal")
    ax.axis("off")
    luu(fig, ten_file)


def ve_muc_0():
    ve_theo_dai("SƠ ĐỒ DFD MỨC 0 – HỆ THỐNG QUẢN LÝ TRUNG TÂM NGOẠI NGỮ",
                [(p, ten) for p, (ten, _) in CHUC_NANG.items()], LUONG_TN, LUONG_KHO, [], "DFD_muc_0.png")


def ve_muc_1():
    for p, d in MUC_1.items():
        cons = con_cua(p)
        ve_theo_dai(f"SƠ ĐỒ DFD MỨC 1 – TIẾN TRÌNH {p} {CHUC_NANG[p][0].upper()}",
                    [(c, cons[c]) for c in d["thu_tu"]], d["tn"], d["kho"], d["noi_bo"],
                    f"DFD_muc_1_{p.split('.')[0]}.png", n_min=2)


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
    ten_ngoai = lambda x: f"{x} {KHO[x]}" if x in KHO else ten_tn(x)
    huong = {"vao": "Vào", "ra": "Ra", "ghi": "Ghi vào kho", "doc": "Đọc từ kho"}
    for p, d in MUC_1.items():
        dong += ["", f"## DFD-{p} ({CHUC_NANG[p][0]}): mức 0 ↔ mức 1", "",
                 "| Tác nhân / kho | Hướng | Luồng mức 0 | Phân rã ở mức 1 |", "|---|---|---|---|"]
        for x, q, h, t in LUONG_TN + LUONG_KHO:
            if q == p:
                con = "; ".join(f"{l[1]}: {l[3]}" for l in d["tn"] + d["kho"]
                                if l[0] == x and l[2] == h and cha(l) == t)
                dong.append(f"| {ten_ngoai(x)} | {huong[h]} | {t} | {con} |")
        for a_, b_, t in d["noi_bo"]:
            dong.append(f"| (nội bộ) | {a_} → {b_} | – | {t} |")
    (THU_MUC / "can_bang_dfd.md").write_text("\n".join(dong) + "\n", encoding="utf-8")
    print("Đã ghi", THU_MUC / "can_bang_dfd.md")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # console Windows mặc định cp1252
    kiem_tra()
    kiem_tra_muc_1()
    ve_ngu_canh()
    ve_muc_0()
    ve_muc_1()
    ghi_bang_can_bang()
