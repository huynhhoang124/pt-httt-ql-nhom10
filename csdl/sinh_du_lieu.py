"""Sinh csdl/seed.sql – dữ liệu mẫu tại thời điểm chốt 31/12/2026.

Dữ liệu bám đúng 5 chứng từ mẫu (02_thu_thap.md mục 4.2: HV0412, DK2026-0158, PT2026-0731,
sổ điểm danh B1–B4, bảng điểm, CN2026-0089) và có đủ tình huống để chạy báo cáo, kiểm thử:
chuyển lớp, bảo lưu rồi học lại, nghỉ học, lớp hủy, dạy thay, buổi hủy + học bù, đăng ký nhóm,
ưu đãi cộng dồn bị chặn 15%, nợ quá hạn, một đợt nộp qua nhiều phiếu, một phiếu thu cho 2 học viên.
Học phí, ưu đãi, đợt, kết quả được TÍNH theo QT01–QT15 (không gõ tay).
Ngẫu nhiên có hạt giống cố định nên chạy lại ra đúng file cũ.
Chạy: python csdl/sinh_du_lieu.py  ->  csdl/seed.sql
"""
import hashlib
import random
from datetime import date, time, timedelta, datetime
from decimal import Decimal, ROUND_HALF_UP
from pathlib import Path

NGAY_CHOT = date(2026, 12, 31)
R = random.Random(2026)

# ------------------------------ danh mục ------------------------------
THAM_SO = [  # 3.1 E26
    ("SISO_TOI_THIEU", "Sĩ số tối thiểu để mở lớp", 8, "học viên", "QT06"),
    ("SISO_TOI_DA", "Sĩ số tối đa của lớp", 20, "học viên", "QT06"),
    ("TY_LE_GIAM_TOI_DA", "Tổng mức giảm tối đa", 15, "%", "QT04"),
    ("TY_LE_DOT_1", "Tỷ lệ tối thiểu của đợt 1", 50, "%", "QT05"),
    ("SO_BUOI_CHUYEN_LOP", "Số buổi tối đa được chuyển lớp", 3, "buổi", "QT08"),
    ("TY_LE_BUOI_BAO_LUU", "Tỷ lệ số buổi tối đa được bảo lưu", 50, "%", "QT09"),
    ("THANG_BAO_LUU", "Thời hạn bảo lưu", 6, "tháng", "QT09"),
    ("NGUONG_CANH_BAO_VANG", "Ngưỡng vắng để cảnh báo", 20, "%", "QT12"),
    ("DIEM_GIOI", "Điểm tổng kết tối thiểu loại Giỏi", 8.5, "điểm", "QT14"),
    ("DIEM_KHA", "Điểm tổng kết tối thiểu loại Khá", 7.0, "điểm", "QT14"),
    ("DIEM_DAT", "Điểm tổng kết tối thiểu để đạt", 5.0, "điểm", "QT14, QT15"),
    ("NGUONG_CHUYEN_CAN_DAT", "Tỷ lệ chuyên cần tối thiểu để đạt", 70, "%", "QT15"),
    ("SO_NGAY_NHAC_HOC_PHI", "Số ngày nhắc trước hạn học phí", 3, "ngày", None),
    ("GIO_SUA_DIEM_DANH", "Số giờ được sửa điểm danh", 24, "giờ", None),
]
TS = {m: Decimal(str(v)) for m, _, v, *_ in THAM_SO}

NHAN_VIEN = [  # MaNV, HoTen, BoPhan, ChucVu, vai trò tài khoản
    ("NV001", "Nguyễn Hoàng Long", "Ban giám đốc", "Giám đốc", "Giám đốc"),
    ("NV002", "Đỗ Thị Mai", "Đào tạo", "Trưởng phòng đào tạo", "QL đào tạo"),
    ("NV003", "Phan Minh Tuấn", "CNTT", "Quản trị hệ thống", "Quản trị viên"),
    ("NV004", "Lê Thu Trang", "Kế toán", "Kế toán viên", "NV kế toán"),
    ("NV005", "Vũ Ngọc Ánh", "Tuyển sinh – CSHV", "Nhân viên tư vấn", "NV tuyển sinh"),
    ("NV006", "Bùi Thanh Hà", "Kế toán", "Kế toán trưởng", "NV kế toán"),
    ("NV007", "Trần Thị Hoa", "Tuyển sinh – CSHV", "Nhân viên tư vấn", "NV tuyển sinh"),
]
GIAO_VIEN = [  # MaGV, HoTen, NgaySinh, ChuyenMon, BangCap
    ("GV008", "Ngô Thị Lan", date(1990, 4, 12), "Tiếng Anh thiếu nhi", "Cử nhân Sư phạm tiếng Anh; TESOL"),
    ("GV011", "Hoàng Đức Minh", date(1988, 9, 3), "Giao tiếp", "Thạc sĩ Ngôn ngữ Anh"),
    ("GV015", "Phạm Quốc Huy", date(1992, 1, 25), "IELTS; tiếng Anh thiếu nhi", "IELTS 8.0; CELTA"),
    ("GV020", "Đặng Thu Hương", date(1994, 6, 18), "IELTS", "IELTS 8.5"),
    ("GV023", "Lý Văn Khoa", date(1986, 11, 30), "TOEIC; tiếng Anh thương mại", "TOEIC 950; Thạc sĩ Quản trị kinh doanh"),
]
PHONG = [  # MaPhong, TenPhong, SucChua, ThietBi, TinhTrang
    ("P101", "Phòng 101", 20, "Máy chiếu, loa", "Sẵn sàng"),
    ("P102", "Phòng 102", 12, "Tivi", "Bảo trì"),
    ("P203", "Phòng 203", 16, "Máy chiếu, bảng tương tác", "Sẵn sàng"),
    ("P204", "Phòng 204", 16, "Máy chiếu", "Sẵn sàng"),
    ("P305", "Phòng 305", 25, "Máy chiếu, loa, micro", "Sẵn sàng"),
]
KHOA = {  # MaKH: TenKH, vào, ra, số buổi, phút/buổi, học phí, (TS cc, gk, ck)
    "IES": ("IELTS Kids Starter", "A1", "A2", 24, 90, 4_200_000, (10, 30, 60)),
    "IEK": ("IELTS Kids Foundation", "A2", "B1", 26, 90, 4_800_000, (10, 30, 60)),
    "GT": ("Giao tiếp cơ bản", "A1", "A2", 10, 120, 2_400_000, (10, 30, 60)),
    "TOE": ("TOEIC 550", "B1", "B2", 24, 120, 5_600_000, (10, 40, 50)),
}
UU_DAI = [  # MaUD, TenUD, LoaiUD, %, điều kiện
    ("UD-HVCU", "Ưu đãi học viên cũ", "Học viên cũ", 10, "Đã học hết ít nhất 1 khóa (QT01)"),
    ("UD-NHOM", "Ưu đãi đăng ký nhóm", "Đăng ký nhóm", 5, "Nhóm từ 3 người đăng ký cùng ngày (QT02)"),
    ("UD-1LAN", "Ưu đãi đóng một lần", "Đóng một lần", 5, "Đóng toàn bộ học phí một lần (QT03)"),
]
# Lớp: mã, khóa, GV, khai giảng, sĩ số tối đa, trạng thái tại ngày chốt, lịch tuần [(thứ, giờ, phòng)]
LOP = [
    ("IES-2603", "IES", "GV008", date(2026, 3, 3), 16, "Kết thúc", [(3, time(17, 30), "P203"), (5, time(17, 30), "P203")]),
    ("IEK-2609", "IEK", "GV015", date(2026, 9, 15), 16, "Kết thúc", [(3, time(18), "P203"), (5, time(18), "P203")]),
    ("IEK-2610", "IEK", "GV020", date(2026, 9, 16), 16, "Kết thúc", [(4, time(18), "P204"), (6, time(18), "P204")]),
    ("GT-2604", "GT", "GV011", date(2026, 9, 19), 20, "Kết thúc", [(7, time(8), "P101")]),
    ("TOE-2611", "TOE", "GV023", date(2026, 11, 2), 20, "Đang học", [(2, time(19), "P305"), (4, time(19), "P305")]),
    ("GT-2611", "GT", "GV011", date(2026, 11, 7), 20, "Hủy", [(7, time(14), "P101")]),
    ("GT-2701", "GT", "GV011", date(2027, 1, 9), 20, "Dự kiến", [(7, time(8), "P101")]),
]
LOP_MAP = {l[0]: l for l in LOP}
BUOI_HUY = {("IEK-2609", 13): "Giáo viên ốm"}                              # buổi hủy …
BUOI_BU = {"IEK-2609": [(date(2026, 12, 15), time(18), "P203")]}          # … và học bù
DAY_THAY = {("IEK-2609", 9): "GV020"}                                    # dạy thay

# ------------------------------ học viên ------------------------------
HO = ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Vũ", "Đặng", "Bùi", "Đỗ", "Ngô", "Dương", "Lý"]
DEM = {"Nam": ["Văn", "Minh", "Đức", "Quang", "Gia", "Hoàng", "Tuấn"],
       "Nữ": ["Thị", "Ngọc", "Thu", "Minh", "Bảo", "Khánh", "Phương"]}
TEN = {"Nam": ["Nam", "Huy", "Khang", "Phúc", "Long", "Duy", "Bảo", "Minh", "Đạt", "Hiếu", "Sơn", "Tùng"],
       "Nữ": ["Anh", "Linh", "Hà", "Trang", "Vy", "Ngân", "Chi", "My", "Thảo", "Hương", "Lan", "Nhi"]}
DUONG = ["Trần Phú", "Quang Trung", "Nguyễn Trãi", "Lê Lợi", "Tô Hiệu", "Văn Quán", "Phùng Hưng"]

hoc_vien, phu_huynh, giam_ho = {}, {}, []
_dt_da_dung = set()


def so_dt():
    while True:
        s = "09" + "".join(R.choice("0123456789") for _ in range(8))
        if s not in _dt_da_dung:
            _dt_da_dung.add(s)
            return s


def ten_ngau_nhien(gt, ho=None):
    return f"{ho or R.choice(HO)} {R.choice(DEM[gt])} {R.choice(TEN[gt])}"


def them_hv(ma, loai, tiep_nhan, ten=None, gt=None, sinh=None, ph=None, **kw):
    """loai: 'tre' (dưới 18, có phụ huynh) hoặc 'lon'."""
    gt = gt or R.choice(["Nam", "Nữ"])
    ten = ten or ten_ngau_nhien(gt)
    if sinh is None:
        nam = R.randint(2011, 2014) if loai == "tre" else R.randint(1988, 2006)
        sinh = date(nam, R.randint(1, 12), R.randint(1, 28))
    hoc_vien[ma] = dict(MaHV=ma, HoTen=ten, NgaySinh=sinh, GioiTinh=gt,
                        DienThoai=kw.get("dt") or so_dt(), Email=kw.get("email"),
                        DiaChi=kw.get("dia_chi") or f"{R.randint(1, 120)} {R.choice(DUONG)}, Hà Đông",
                        TrinhDoDauVao=kw.get("trinh_do"), NgayKiemTra=kw.get("ngay_kt"),
                        NgayTiepNhan=tiep_nhan, TrangThai="Hoạt động")
    if loai == "tre":
        if ph is None:  # tự tạo một phụ huynh (bố hoặc mẹ), cùng họ với con
            ph = next(f"PH{i:04d}" for i in range(330, 10000) if f"PH{i:04d}" not in phu_huynh)
            vai = R.choice(["Bố", "Mẹ"])
            phu_huynh[ph] = dict(MaPH=ph, HoTen=ten_ngau_nhien("Nam" if vai == "Bố" else "Nữ", ten.split()[0]),
                                 DienThoai=so_dt(), Email=None)
            giam_ho.append((ma, ph, vai))
        else:
            for p, vai in ph:
                giam_ho.append((ma, p, vai))


# Các học viên, phụ huynh trên chứng từ mẫu
phu_huynh["PH0301"] = dict(MaPH="PH0301", HoTen="Nguyễn Văn Bình", DienThoai="0983111222", Email=None)
phu_huynh["PH0302"] = dict(MaPH="PH0302", HoTen="Lê Thị Thu", DienThoai="0983111333", Email="lethu.ph@gmail.com")
phu_huynh["PH0320"] = dict(MaPH="PH0320", HoTen="Đỗ Văn Quân", DienThoai="0912777888", Email="doquan@gmail.com")
_dt_da_dung |= {"0983111222", "0983111333", "0912777888", "0912345678"}

them_hv("HV0412", "tre", date(2026, 2, 20), "Nguyễn Minh Anh", "Nữ", date(2012, 3, 14),
        ph=[("PH0301", "Bố"), ("PH0302", "Mẹ")], dt="0912345678", email="minhanh@gmail.com",
        dia_chi="12 Trần Phú, Hà Đông", trinh_do="A2", ngay_kt=date(2026, 9, 2))
for i in range(390, 398):  # bạn cùng lớp IES-2603
    them_hv(f"HV{i:04d}", "tre", date(2026, 2, R.randint(10, 25)), trinh_do="A1", ngay_kt=date(2026, 2, 26))
them_hv("HV0398", "tre", date(2026, 8, 28), "Trần Gia Bảo", "Nam", date(2013, 7, 2), trinh_do="A2", ngay_kt=date(2026, 8, 28))
them_hv("HV0425", "tre", date(2026, 8, 25), trinh_do="A2", ngay_kt=date(2026, 8, 25))
for i in range(413, 418):
    them_hv(f"HV{i:04d}", "tre", date(2026, 8, R.randint(18, 31)), trinh_do="A2", ngay_kt=date(2026, 8, 31))
them_hv("HV0451", "tre", date(2026, 9, 3), "Đỗ Minh Khang", "Nam", date(2012, 5, 9), ph=[("PH0320", "Bố")], trinh_do="A2", ngay_kt=date(2026, 9, 3))
them_hv("HV0452", "tre", date(2026, 9, 3), "Đỗ Ngọc Hân", "Nữ", date(2014, 1, 21), ph=[("PH0320", "Bố")], trinh_do="A2", ngay_kt=date(2026, 9, 3))
for i in (453, 454, 455, 460):
    them_hv(f"HV{i:04d}", "tre", date(2026, 9, R.randint(1, 10)), trinh_do="A2", ngay_kt=date(2026, 9, 10))
them_hv("HV0440", "lon", date(2026, 9, 8), trinh_do="A1", ngay_kt=date(2026, 9, 8))
for i in range(441, 444):  # thiếu niên học giao tiếp
    them_hv(f"HV{i:04d}", "tre", date(2026, 9, R.randint(5, 15)), sinh=date(R.randint(2009, 2010), R.randint(1, 12), R.randint(1, 28)),
            trinh_do="A1", ngay_kt=date(2026, 9, 15))
for i in range(444, 449):
    them_hv(f"HV{i:04d}", "lon", date(2026, 9, R.randint(5, 16)), trinh_do="A1", ngay_kt=date(2026, 9, 16))
for i in range(480, 490):
    them_hv(f"HV{i:04d}", "lon", date(2026, 10, R.randint(10, 28)), trinh_do="B1", ngay_kt=date(2026, 10, 28))
for i in (461, 462, 463):
    them_hv(f"HV{i:04d}", "lon", date(2026, 10, R.randint(15, 30)), trinh_do="A1", ngay_kt=date(2026, 10, 30))
for i in (470, 471):
    them_hv(f"HV{i:04d}", "lon", date(2026, 12, 10), trinh_do="A1", ngay_kt=date(2026, 12, 10))

# ------------------------------ đăng ký ------------------------------
# (MaHV, MaLop, NgayDK, NV tiếp nhận, số đợt, nhóm?) – đăng ký cùng HV, cùng ngày thì chung một phiếu
DK = []


def dk(hv, lop, ngay, nv="NV007", so_dot=2, nhom=False):
    DK.append(dict(MaHV=hv, MaLop=lop, NgayDK=ngay, MaNV=nv, SoDot=so_dot, Nhom=nhom, MaLopGoc=None,
                   TrangThai="Đã đăng ký", NgayThayDoi=None, LyDo=None, HanBaoLuu=None))


for hv in ["HV0412"] + [f"HV{i:04d}" for i in range(390, 398)]:
    dk(hv, "IES-2603", date(2026, 2, 26) if hv != "HV0412" else date(2026, 2, 24), "NV005")
dk("HV0412", "IEK-2609", date(2026, 9, 5))                 # phiếu DK2026-0158: 2 lớp, học viên cũ
dk("HV0412", "GT-2604", date(2026, 9, 5))
for hv, d in [("HV0398", date(2026, 8, 29)), ("HV0425", date(2026, 8, 26)), ("HV0390", date(2026, 9, 1)),
              ("HV0391", date(2026, 9, 2))] + [(f"HV{i:04d}", date(2026, 9, R.randint(1, 12))) for i in range(413, 418)]:
    dk(hv, "IEK-2609", d, R.choice(["NV005", "NV007"]))
for hv in ["HV0451", "HV0452", "HV0460", "HV0392", "HV0393", "HV0453", "HV0454", "HV0455"]:
    dk(hv, "IEK-2610", date(2026, 9, R.randint(4, 14)) if hv not in ("HV0451", "HV0452") else date(2026, 9, 4),
       R.choice(["NV005", "NV007"]))
for hv in ["HV0440"] + [f"HV{i:04d}" for i in range(441, 449)]:
    dk(hv, "GT-2604", date(2026, 9, R.randint(8, 17)), R.choice(["NV005", "NV007"]))
dk("HV0480", "TOE-2611", date(2026, 10, 20), "NV005", nhom=True)    # nhóm 3 người cùng ngày (QT02)
dk("HV0481", "TOE-2611", date(2026, 10, 20), "NV005", so_dot=1, nhom=True)
dk("HV0482", "TOE-2611", date(2026, 10, 20), "NV005", nhom=True)
for i in range(483, 490):
    dk(f"HV{i:04d}", "TOE-2611", date(2026, 10, R.randint(12, 31)), R.choice(["NV005", "NV007"]))
for hv in ("HV0461", "HV0462", "HV0463"):
    dk(hv, "GT-2611", date(2026, 11, R.randint(1, 4)), "NV007", so_dot=1)
# GT-2701: nhóm 3 người ngày 10/12, trong đó HV0445 là học viên cũ của GT-2604 và đóng một lần:
# 10% + 5% + 5% = 20% bị chặn ở 15% (QT04, quy tắc R1 của bảng quyết định 3.1)
dk("HV0445", "GT-2701", date(2026, 12, 10), "NV005", so_dot=1, nhom=True)
dk("HV0470", "GT-2701", date(2026, 12, 10), "NV005", nhom=True)
dk("HV0471", "GT-2701", date(2026, 12, 10), "NV005", nhom=True)


def tim_dk(hv, lop):
    return next(d for d in DK if d["MaHV"] == hv and d["MaLop"] == lop)


def doi_trang_thai(hv, lop, tt, ngay, ly_do, lop_moi=None, ngay_dk_moi=None, nv="NV007"):
    """Xử lý 2.4 (cây quyết định 04_dac_ta mục 2.1)."""
    d = tim_dk(hv, lop)
    d.update(TrangThai=tt, NgayThayDoi=ngay, LyDo=ly_do)
    if tt == "Bảo lưu":
        d["HanBaoLuu"] = date(ngay.year + (ngay.month + 5) // 12, (ngay.month + 5) % 12 + 1, ngay.day)
    if lop_moi:  # đăng ký mới mang đăng ký gốc; chuyển lớp giữ số phiếu cũ, học lại lập phiếu mới
        DK.append(dict(MaHV=hv, MaLop=lop_moi, NgayDK=ngay_dk_moi or d["NgayDK"], MaNV=nv, SoDot=None, Nhom=False,
                       MaLopGoc=lop, TrangThai="Đã đăng ký", NgayThayDoi=None, LyDo=None, HanBaoLuu=None,
                       PhieuMoi=ngay_dk_moi is not None))


doi_trang_thai("HV0425", "IEK-2609", "Chuyển lớp", date(2026, 9, 18), "Trùng lịch học ở trường", "IEK-2610")
doi_trang_thai("HV0440", "GT-2604", "Bảo lưu", date(2026, 10, 5), "Đi công tác 3 tháng", "GT-2701", date(2026, 12, 15), "NV005")
doi_trang_thai("HV0460", "IEK-2610", "Nghỉ học", date(2026, 10, 10), "Gia đình chuyển nhà")
for hv in ("HV0461", "HV0462", "HV0463"):
    doi_trang_thai(hv, "GT-2611", "Chuyển lớp", date(2026, 11, 6), "Lớp hủy do không đủ sĩ số", "GT-2701")

# ------------------------------ lịch, buổi học ------------------------------


def ngay_theo_lich(lop, so):
    """so buổi đầu tiên theo lịch tuần (dự kiến, chưa xét hủy)."""
    _, _, _, kg, _, _, lich = LOP_MAP[lop]
    thu = {t - 2: (g, p) for t, g, p in lich}  # Thứ 2..8 -> weekday 0..6
    ds, d = [], kg
    while len(ds) < so:
        if d.weekday() in thu:
            ds.append((d,) + thu[d.weekday()])
        d += timedelta(days=1)
    return ds


def cong_phut(t, phut):
    return (datetime.combine(date(2000, 1, 1), t) + timedelta(minutes=phut)).time()


buoi = []  # dict MaLop, SoBuoi, Ngay, GioBatDau, GioKetThuc, MaPhong, MaGV, Loai, NoiDung, TrangThai
for ma, kh, gv, kg, _, tt, lich in LOP:
    if tt not in ("Đang học", "Kết thúc"):
        continue
    so_buoi, phut = KHOA[kh][3], KHOA[kh][4]
    ds = [(d, g, p, "Thường") for d, g, p in ngay_theo_lich(ma, so_buoi)] + \
         [(d, g, p, "Học bù") for d, g, p in BUOI_BU.get(ma, [])]
    lop_buoi = []
    for i, (d, g, p, loai) in enumerate(ds, 1):
        if (ma, i) in BUOI_HUY:
            noi_dung, trang_thai = f"Hủy: {BUOI_HUY[ma, i]}", "Hủy"
        else:
            noi_dung, trang_thai = f"Bài {i}", "Đã dạy" if d <= NGAY_CHOT else "Kế hoạch"
        lop_buoi.append(dict(MaLop=ma, SoBuoi=i, Ngay=d, GioBatDau=g, GioKetThuc=cong_phut(g, phut), MaPhong=p,
                             MaGV=DAY_THAY.get((ma, i), gv), Loai=loai, NoiDung=noi_dung, TrangThai=trang_thai))
    # kiểm tra giữa kỳ ở buổi học THỰC TẾ thứ (số buổi + 1) / 2, thi cuối kỳ ở buổi cuối (bỏ qua buổi hủy)
    thuc = [b for b in lop_buoi if b["TrangThai"] != "Hủy"]
    thuc[(so_buoi + 1) // 2 - 1]["NoiDung"] = "Kiểm tra giữa kỳ"
    thuc[-1]["NoiDung"] = "Học bù buổi 13; thi cuối kỳ" if thuc[-1]["Loai"] == "Học bù" else "Thi cuối kỳ"
    buoi += lop_buoi

# ------------------------------ học phí (3.1, bảng quyết định 3.1) ------------------------------


def lam_tron(x, buoc="1000"):
    return (Decimal(x) / Decimal(buoc)).quantize(Decimal("1"), ROUND_HALF_UP) * Decimal(buoc)


def ngay_ket_thuc(lop):
    ds = [b["Ngay"] for b in buoi if b["MaLop"] == lop and b["TrangThai"] == "Đã dạy"]
    return max(ds) if ds and LOP_MAP[lop][5] == "Kết thúc" else None


def la_hoc_vien_cu(hv, ngay):  # QT01
    return any(d["MaHV"] == hv and d["TrangThai"] == "Đã đăng ký" and ngay_ket_thuc(d["MaLop"])
               and ngay_ket_thuc(d["MaLop"]) < ngay for d in DK)


hoc_phi, dot_hp, ap_dung = [], [], []
for d in DK:
    if d["MaLopGoc"]:  # đăng ký có đăng ký gốc: không lập khoản phải thu mới (04_dac_ta mục 2.1)
        continue
    kh = LOP_MAP[d["MaLop"]][1]
    goc = Decimal(KHOA[kh][5])
    uds = (["UD-HVCU"] if la_hoc_vien_cu(d["MaHV"], d["NgayDK"]) else []) + \
          (["UD-NHOM"] if d["Nhom"] else []) + (["UD-1LAN"] if d["SoDot"] == 1 else [])
    ty_le = min(sum(Decimal(u[3]) for u in UU_DAI if u[0] in uds), TS["TY_LE_GIAM_TOI_DA"])
    phai_nop = goc * (100 - ty_le) / 100
    hoc_phi.append(dict(MaHV=d["MaHV"], MaLop=d["MaLop"], HocPhiGoc=goc, TyLeGiam=ty_le, SoDot=d["SoDot"], NgayLap=d["NgayDK"]))
    ap_dung += [(d["MaHV"], d["MaLop"], u) for u in uds]
    kg = LOP_MAP[d["MaLop"]][3]
    if d["SoDot"] == 1:
        dot_hp.append(dict(MaHV=d["MaHV"], MaLop=d["MaLop"], Dot=1, SoTien=phai_nop, HanDong=kg))
    else:
        dot1 = lam_tron(phai_nop * TS["TY_LE_DOT_1"] / 100)
        giua = ngay_theo_lich(d["MaLop"], (KHOA[kh][3] + 1) // 2)[-1][0]  # buổi giữa khóa theo lịch dự kiến
        dot_hp += [dict(MaHV=d["MaHV"], MaLop=d["MaLop"], Dot=1, SoTien=dot1, HanDong=kg),
                   dict(MaHV=d["MaHV"], MaLop=d["MaLop"], Dot=2, SoTien=phai_nop - dot1, HanDong=giua)]

# ------------------------------ phiếu thu ------------------------------
# (ngày thu, [(MaHV, MaLop, đợt, số tiền)], người nộp, hình thức, NV thu) – mặc định: đợt 1 trước khai giảng,
# đợt 2 trước hạn; các ngoại lệ ghi rõ bên dưới.
KHONG_NOP_DOT_2 = {"HV0485", "HV0486", "HV0487", "HV0460"}   # nợ quá hạn (QT16) / nghỉ học (QT10)
thu = []


def nguoi_nop(hv):
    ph = [p for h, p, _ in giam_ho if h == hv]
    return phu_huynh[ph[0]]["HoTen"] if ph else hoc_vien[hv]["HoTen"]


def can_thu(hv, lop, dot):
    return next(x["SoTien"] for x in dot_hp if (x["MaHV"], x["MaLop"], x["Dot"]) == (hv, lop, dot))


dac_biet = {("HV0412", "IEK-2609"), ("HV0412", "GT-2604"), ("HV0451", "IEK-2610"), ("HV0452", "IEK-2610"),
            ("HV0440", "GT-2604"), ("HV0488", "TOE-2611")}
# HV0412: phiếu PT2026-0731 ngày 06/09 = IEK đợt 1 + đợt 2, GT đợt 1 (đúng chứng từ); GT đợt 2 nộp 16/10
thu.append((date(2026, 9, 6), [("HV0412", "IEK-2609", 1, can_thu("HV0412", "IEK-2609", 1)),
                                ("HV0412", "IEK-2609", 2, can_thu("HV0412", "IEK-2609", 2)),
                                ("HV0412", "GT-2604", 1, can_thu("HV0412", "GT-2604", 1))],
            "Nguyễn Văn Bình", "Chuyển khoản", "NV004", "PT2026-0731"))
thu.append((date(2026, 10, 16), [("HV0412", "GT-2604", 2, can_thu("HV0412", "GT-2604", 2))],
            "Lê Thị Thu", "Tiền mặt", "NV004", None))
# Hai anh em HV0451, HV0452: một phiếu thu chung do bố nộp (3.1 E17)
for dot, ngay in ((1, date(2026, 9, 10)), (2, date(2026, 10, 25))):
    thu.append((ngay, [(hv, "IEK-2610", dot, can_thu(hv, "IEK-2610", dot)) for hv in ("HV0451", "HV0452")],
                "Đỗ Văn Quân", "Chuyển khoản", "NV006", None))
# HV0440 đóng đủ trước khi bảo lưu (QT09)
thu.append((date(2026, 9, 18), [("HV0440", "GT-2604", 1, can_thu("HV0440", "GT-2604", 1))],
            hoc_vien["HV0440"]["HoTen"], "Tiền mặt", "NV004", None))
thu.append((date(2026, 10, 1), [("HV0440", "GT-2604", 2, can_thu("HV0440", "GT-2604", 2))],
            hoc_vien["HV0440"]["HoTen"], "Tiền mặt", "NV004", None))
# HV0488: đợt 2 nộp qua 2 phiếu, phiếu sau trễ hạn (một đợt – nhiều phiếu, quan hệ N–N Q16)
thu.append((date(2026, 10, 30), [("HV0488", "TOE-2611", 1, can_thu("HV0488", "TOE-2611", 1))],
            hoc_vien["HV0488"]["HoTen"], "Chuyển khoản", "NV006", None))
thu.append((date(2026, 12, 5), [("HV0488", "TOE-2611", 2, Decimal(1_000_000))], hoc_vien["HV0488"]["HoTen"],
            "Tiền mặt", "NV004", None))
thu.append((date(2026, 12, 20), [("HV0488", "TOE-2611", 2, can_thu("HV0488", "TOE-2611", 2) - 1_000_000)],
            hoc_vien["HV0488"]["HoTen"], "Tiền mặt", "NV004", None))
for h in hoc_phi:
    if (h["MaHV"], h["MaLop"]) in dac_biet:
        continue
    kg = LOP_MAP[h["MaLop"]][3]
    for x in [x for x in dot_hp if (x["MaHV"], x["MaLop"]) == (h["MaHV"], h["MaLop"])]:
        if x["Dot"] == 2 and h["MaHV"] in KHONG_NOP_DOT_2:
            continue
        if x["Dot"] == 1:
            ngay = min(h["NgayLap"] + timedelta(days=R.randint(0, 4)), kg)
        else:
            ngay = x["HanDong"] - timedelta(days=R.randint(0, 6))
        thu.append((ngay, [(h["MaHV"], h["MaLop"], x["Dot"], x["SoTien"])], nguoi_nop(h["MaHV"]),
                    R.choice(["Tiền mặt", "Chuyển khoản"]), R.choice(["NV004", "NV006"]), None))
thu = [t for t in thu if t[0] <= NGAY_CHOT]

# ------------------------------ đánh số chứng từ ------------------------------


def danh_so(ds, tien_to, co_dinh, dau_nam, khoa_sap_xep):
    """Đánh số tăng dần theo ngày; số cố định của chứng từ mẫu giữ nguyên, các số khác tránh nó.
    Đợt đầu năm (trước tháng 7) bắt đầu từ dau_nam; nửa sau năm đặt sao cho chứng từ mẫu đúng số của nó."""
    ds = sorted(ds, key=khoa_sap_xep)
    truoc = [x for x in ds if x[0].month < 7]
    sau = [x for x in ds if x[0].month >= 7]
    so = {}
    for i, x in enumerate(truoc):
        so[id(x)] = dau_nam + i
    vt = next(i for i, x in enumerate(sau) if co_dinh(x))
    goc = co_dinh(sau[vt]) - vt
    assert goc > dau_nam + len(truoc), "số chứng từ nửa sau năm chồng lên nửa đầu năm"
    for i, x in enumerate(sau):
        so[id(x)] = goc + i
    return [(f"{tien_to}{x[0].year}-{so[id(x)]:04d}", x) for x in ds]


# Phiếu đăng ký: các đăng ký cùng học viên, cùng ngày, không phải chuyển lớp -> chung một phiếu
nhom_phieu = {}
for d in DK:
    if d["MaLopGoc"] and not d.get("PhieuMoi"):
        continue
    nhom_phieu.setdefault((d["NgayDK"], d["MaHV"]), []).append(d)
phieu_dk = danh_so([(k[0], k[1], v) for k, v in nhom_phieu.items()], "DK",
                   lambda x: 158 if (x[1], x[0]) == ("HV0412", date(2026, 9, 5)) else None, 31,
                   lambda x: (x[0], x[1]))
for so, (_, _, ds) in phieu_dk:
    for d in ds:
        d["SoPhieuDK"] = so
for d in DK:  # chuyển lớp: giữ số phiếu của đăng ký gốc
    if d["MaLopGoc"] and not d.get("PhieuMoi"):
        d["SoPhieuDK"] = tim_dk(d["MaHV"], d["MaLopGoc"])["SoPhieuDK"]
phieu_thu = danh_so(thu, "PT", lambda x: 731 if x[5] == "PT2026-0731" else None, 101,
                    lambda x: (x[0], x[1][0][0], x[1][0][1], x[1][0][2]))

# ------------------------------ điểm danh, điểm, kết quả ------------------------------


def dang_hoc(d, ngay):
    """Đăng ký d có trong lớp vào ngày đó không (vào lớp từ ngày đăng ký/chuyển, ra khi đổi trạng thái)."""
    vao = d["NgayDK"] if not d["MaLopGoc"] else tim_dk(d["MaHV"], d["MaLopGoc"])["NgayThayDoi"]
    ra = d["NgayThayDoi"] if d["TrangThai"] != "Đã đăng ký" else None
    return (d["MaLopGoc"] is None or ngay >= vao) and (ra is None or ngay < ra)


CO_DINH = {  # sổ điểm danh mẫu (02_thu_thap mục 4.2.3) và bảng điểm mẫu (4.2.4)
    ("HV0412", "IEK-2609"): dict(dd={1: "x", 2: "M", 3: "x", 4: "P", 17: "P"}, vang=[4, 17], gk=7.5, ck=8.0),
    ("HV0398", "IEK-2609"): dict(dd={1: "x", 2: "K", 3: "K", 4: "x"}, vang=[2, 3, 6, 8, 11, 15, 19, 22, 24, 26], gk=5.0, ck=4.0),
    ("HV0412", "IES-2603"): dict(dd={}, vang=[5, 20], gk=7.0, ck=7.5),
}
TY_LE_CO_MAT = {"HV0447": 0.6, "HV0397": 0.66, "HV0455": 0.74, "HV0486": 0.7}   # một số học viên hay vắng
NANG_LUC = {"HV0397": 4.0, "HV0443": 4.6}                                           # học yếu

diem_danh, diem, ket_qua = [], [], []
for d in DK:
    buoi_lop = [b for b in buoi if b["MaLop"] == d["MaLop"] and b["TrangThai"] == "Đã dạy" and dang_hoc(d, b["Ngay"])]
    if not buoi_lop:
        continue
    cd = CO_DINH.get((d["MaHV"], d["MaLop"]), {})
    p = TY_LE_CO_MAT.get(d["MaHV"], R.uniform(0.82, 0.99))
    for b in buoi_lop:
        n = b["SoBuoi"]
        if n in cd.get("dd", {}):
            tt = cd["dd"][n]
        elif "vang" in cd:
            tt = R.choice("PK") if n in cd["vang"] else ("M" if R.random() < 0.08 else "x")
        else:
            tt = ("M" if R.random() < 0.1 else "x") if R.random() < p else R.choice("PPK")
        ghi_chu = "Nhắc phụ huynh" if (d["MaHV"], d["MaLop"], n) == ("HV0398", "IEK-2609", 3) else None
        diem_danh.append((d["MaHV"], d["MaLop"], n, tt, ghi_chu))
    giua_ky = next(b for b in buoi if b["MaLop"] == d["MaLop"] and b["NoiDung"] == "Kiểm tra giữa kỳ")
    da_giua_ky = giua_ky["TrangThai"] == "Đã dạy" and dang_hoc(d, giua_ky["Ngay"])
    ket_thuc = LOP_MAP[d["MaLop"]][5] == "Kết thúc" and d["TrangThai"] == "Đã đăng ký"
    nl = NANG_LUC.get(d["MaHV"], R.uniform(5.5, 9.2))
    cham = lambda: float(min(Decimal(10), max(Decimal(0), (Decimal(str(nl + R.uniform(-1, 1))) * 2)
                                              .quantize(Decimal("1"), ROUND_HALF_UP) / 2)))
    gk = cd.get("gk", cham()) if da_giua_ky else None
    ck = cd.get("ck", cham()) if ket_thuc else None
    nx = lambda v: "Tiến bộ tốt" if v >= 8 else "Cần cố gắng thêm" if v < 5 else "Đạt yêu cầu"
    if gk is not None:
        diem.append((d["MaHV"], d["MaLop"], "Giữa kỳ", gk, nx(gk)))
    if ck is not None:
        diem.append((d["MaHV"], d["MaLop"], "Cuối kỳ", ck, nx(ck)))
        # Tổng kết theo QT13–QT15 – cùng công thức với view v_KetQuaTinh
        dds = [x for x in diem_danh if (x[0], x[1]) == (d["MaHV"], d["MaLop"])]
        co_mat = sum(1 for x in dds if x[3] in "xM")
        cc = Decimal(10) * co_mat / len(dds)
        tsc, tsg, tsk = KHOA[LOP_MAP[d["MaLop"]][1]][6]
        tk = ((tsc * cc + tsg * Decimal(str(gk)) + tsk * Decimal(str(ck))) / 100).quantize(Decimal("0.1"), ROUND_HALF_UP)
        xl = ("Giỏi" if tk >= TS["DIEM_GIOI"] else "Khá" if tk >= TS["DIEM_KHA"] else
              "Trung bình" if tk >= TS["DIEM_DAT"] else "Không đạt")
        kq = "Đạt" if tk >= TS["DIEM_DAT"] and Decimal(100) * co_mat / len(dds) >= TS["NGUONG_CHUYEN_CAN_DAT"] else "Không đạt"
        ket_qua.append(dict(MaHV=d["MaHV"], MaLop=d["MaLop"], DiemTongKet=tk, XepLoai=xl, KetQua=kq,
                            MaNVDuyet="NV002", NgayDuyet=ngay_ket_thuc(d["MaLop"]) + timedelta(days=3)))

# Chứng nhận cho kết quả Đạt (QT15), ngày cấp = 5 ngày sau buổi cuối; CN2026-0089 là của HV0412 lớp IEK-2609
cap = [(ngay_ket_thuc(k["MaLop"]) + timedelta(days=5), k["MaLop"], k["MaHV"] != "HV0412", k["MaHV"], k)
       for k in ket_qua if k["KetQua"] == "Đạt"]
chung_nhan = danh_so(cap, "CN", lambda x: 89 if (x[3], x[1]) == ("HV0412", "IEK-2609") else None, 31,
                     lambda x: x[:4])

# ------------------------------ tài khoản, thông báo, nhật ký ------------------------------


def bam(u):  # mật khẩu băm giả định cho dữ liệu mẫu
    return "sha256$" + hashlib.sha256(f"nhom10:{u}".encode()).hexdigest()


tai_khoan = [(m.lower(), bam(m.lower()), vt, m, None, None, None) for m, _, _, _, vt in NHAN_VIEN] + \
            [(g[0].lower(), bam(g[0].lower()), "Giáo viên", None, g[0], None, None) for g in GIAO_VIEN] + \
            [(h.lower(), bam(h.lower()), "Học viên", None, None, h, None) for h in ("HV0412", "HV0398", "HV0480")] + \
            [(p.lower(), bam(p.lower()), "Phụ huynh", None, None, None, p) for p in ("PH0301", "PH0320")]

thong_bao = []


def bao(loai, hv, noi_dung, luc):
    """Gửi cho học viên; nếu học viên dưới 18 tuổi gửi thêm cho từng phụ huynh (04_dac_ta mục 1.8, 5.2)."""
    thong_bao.append((loai, hv, None, noi_dung, luc, "Đã gửi"))
    tuoi = (luc.date() - hoc_vien[hv]["NgaySinh"]).days // 365
    if tuoi < 18:
        for h, p, _ in giam_ho:
            if h == hv:
                thong_bao.append((loai, hv, p, noi_dung, luc, "Đã gửi"))


huy = next(b for b in buoi if (b["MaLop"], b["SoBuoi"]) == ("IEK-2609", 13))
for d in DK:
    if d["MaLop"] == "IEK-2609" and dang_hoc(d, huy["Ngay"]):
        bao("Lịch học", d["MaHV"], f"Lớp IEK-2609 nghỉ buổi 13 ngày {huy['Ngay']:%d/%m/%Y} (giáo viên ốm); học bù ngày 15/12/2026.",
            datetime(2026, 10, 26, 20, 0))
bao("Chuyên cần", "HV0398", "Cảnh báo vắng học: Trần Gia Bảo đã vắng quá 20% số buổi lớp IEK-2609.", datetime(2026, 10, 1, 21, 0))
for hv in sorted(KHONG_NOP_DOT_2 - {"HV0460"}):
    bao("Học phí", hv, "Học phí đợt 2 lớp TOE-2611 đã quá hạn ngày 09/12/2026, đề nghị hoàn thành sớm.",
        datetime(2026, 12, 12, 9, 0))
bao("Kết quả", "HV0412", "Kết quả lớp IEK-2609: điểm tổng kết 8,0 – Khá – Đạt.", datetime(2026, 12, 18, 9, 0))

nhat_ky = [
    (datetime(2026, 9, 5, 17, 40), "nv007", "Thêm", "PhieuDangKy", "DK2026-0158"),
    (datetime(2026, 9, 6, 9, 15), "nv004", "Thêm", "PhieuThu", "PT2026-0731"),
    (datetime(2026, 9, 18, 10, 0), "nv007", "Sửa", "DangKy", "HV0425/IEK-2609"),
    (datetime(2026, 9, 22, 19, 35), "gv015", "Thêm", "DiemDanh", "IEK-2609/3"),
    (datetime(2026, 10, 26, 19, 0), "nv002", "Sửa", "BuoiHoc", "IEK-2609/13"),
    (datetime(2026, 12, 18, 8, 30), "nv002", "Duyệt", "KetQua", "IEK-2609"),
    (datetime(2026, 12, 20, 10, 0), "nv002", "In", "ChungNhan", "CN2026-0089"),
]

# ------------------------------ ghi SQL ------------------------------


def sql(v):
    if v is None:
        return "NULL"
    if isinstance(v, (int, float, Decimal)) and not isinstance(v, bool):
        return str(v)
    if isinstance(v, datetime):
        return f"'{v:%Y-%m-%d %H:%M:%S}'"
    if isinstance(v, date):
        return f"'{v:%Y-%m-%d}'"
    if isinstance(v, time):
        return f"'{v:%H:%M}'"
    return "N'" + str(v).replace("'", "''") + "'"


def insert(bang, cot, dong):
    out = [f"-- {bang}: {len(dong)} dòng"]
    for i in range(0, len(dong), 500):
        out.append(f"INSERT INTO {bang} ({', '.join(cot)}) VALUES\n" +
                   ",\n".join("    (" + ", ".join(sql(v) for v in r) + ")" for r in dong[i:i + 500]) + ";")
    return "\n".join(out)


phan = [
    insert("NhanVien", ["MaNV", "HoTen", "BoPhan", "ChucVu", "DienThoai", "Email", "TrangThai"],
           [(m, t, b, c, so_dt(), f"{m.lower()}@ttnn.edu.vn", "Đang làm") for m, t, b, c, _ in NHAN_VIEN]),
    insert("HocVien", list(hoc_vien["HV0412"]), [tuple(h.values()) for h in hoc_vien.values()]),
    insert("PhuHuynh", ["MaPH", "HoTen", "DienThoai", "Email"], [tuple(p.values()) for p in phu_huynh.values()]),
    insert("HocVienPhuHuynh", ["MaHV", "MaPH", "QuanHe"], giam_ho),
    insert("GiaoVien", ["MaGV", "HoTen", "NgaySinh", "DienThoai", "Email", "NgonNguDay", "ChuyenMon", "BangCap", "TinhTrang"],
           [(m, t, s, so_dt(), f"{m.lower()}@ttnn.edu.vn", "Anh", cm, bc, "Đang dạy") for m, t, s, cm, bc in GIAO_VIEN]),
    insert("KhoaHoc", ["MaKH", "TenKH", "NgonNgu", "TrinhDoDauVao", "TrinhDoDauRa", "SoBuoi", "ThoiLuongBuoi", "HocPhi",
                       "TSChuyenCan", "TSGiuaKy", "TSCuoiKy", "MoTa", "TrangThai"],
           [(m, t, "Anh", v, r, sb, ph, hp, *ts, None, "Đang mở") for m, (t, v, r, sb, ph, hp, ts) in KHOA.items()]),
    insert("PhongHoc", ["MaPhong", "TenPhong", "SucChua", "ThietBi", "TinhTrang"], PHONG),
    insert("LopHoc", ["MaLop", "MaKH", "MaGV", "NgayKhaiGiang", "SiSoToiDa", "TrangThai"], [l[:6] for l in LOP]),
    insert("LichTuan", ["MaLop", "Thu", "GioBatDau", "MaPhong"], [(l[0], t, g, p) for l in LOP for t, g, p in l[6]]),
    insert("BuoiHoc", list(buoi[0]), [tuple(b.values()) for b in buoi]),
    insert("PhieuDangKy", ["SoPhieuDK", "NgayDK", "MaNV"], [(so, x[0], x[2][0]["MaNV"]) for so, x in phieu_dk]),
    insert("DangKy", ["MaHV", "MaLop", "SoPhieuDK", "TrangThai", "NgayThayDoi", "LyDo", "HanBaoLuu", "MaLopGoc"],
           [(d["MaHV"], d["MaLop"], d["SoPhieuDK"], d["TrangThai"], d["NgayThayDoi"], d["LyDo"], d["HanBaoLuu"], d["MaLopGoc"])
            for d in sorted(DK, key=lambda d: d["MaLopGoc"] is not None)]),   # đăng ký gốc chèn trước
    insert("UuDai", ["MaUD", "TenUD", "LoaiUD", "TyLeGiam", "NgayBatDau", "NgayKetThuc", "DieuKien"],
           [(m, t, l, p, date(2026, 1, 1), date(2027, 12, 31), dk_) for m, t, l, p, dk_ in UU_DAI]),  # còn hiệu lực ở ngày demo 04/01/2027
    insert("HocPhi", list(hoc_phi[0]), [tuple(h.values()) for h in hoc_phi]),
    insert("DotHocPhi", list(dot_hp[0]), [tuple(x.values()) for x in dot_hp]),
    insert("ApDungUuDai", ["MaHV", "MaLop", "MaUD"], ap_dung),
    insert("PhieuThu", ["SoPT", "NgayThu", "NguoiNop", "HinhThuc", "MaNV"], [(so, t[0], t[2], t[3], t[4]) for so, t in phieu_thu]),
    insert("ChiTietPhieuThu", ["SoPT", "MaHV", "MaLop", "Dot", "SoTien"], [(so, *l) for so, t in phieu_thu for l in t[1]]),
    insert("DiemDanh", ["MaHV", "MaLop", "SoBuoi", "TrangThai", "GhiChu"], diem_danh),
    insert("Diem", ["MaHV", "MaLop", "LoaiDiem", "Diem", "NhanXet"], diem),
    insert("KetQua", list(ket_qua[0]), [tuple(k.values()) for k in ket_qua]),
    insert("ChungNhan", ["SoCN", "MaHV", "MaLop", "NgayCap"], [(so, x[3], x[1], x[0]) for so, x in chung_nhan]),
    insert("TaiKhoan", ["TenDangNhap", "MatKhauBam", "VaiTro", "MaNV", "MaGV", "MaHV", "MaPH"], tai_khoan),
    insert("ThongBao", ["Loai", "MaHV", "MaPH", "NoiDung", "ThoiGianGui", "TrangThaiGui"], thong_bao),
    insert("NhatKy", ["ThoiDiem", "TenDangNhap", "ThaoTac", "DoiTuong", "MaDoiTuong"], nhat_ky),
    insert("ThamSo", ["MaThamSo", "TenThamSo", "GiaTri", "DonVi", "QuyTac"], THAM_SO),
]
BANG_XOA = ["ThamSo", "NhatKy", "ThongBao", "TaiKhoan", "ChungNhan", "KetQua", "Diem", "DiemDanh", "ChiTietPhieuThu",
            "PhieuThu", "ApDungUuDai", "DotHocPhi", "HocPhi", "UuDai", "DangKy", "PhieuDangKy", "BuoiHoc", "LichTuan",
            "LopHoc", "PhongHoc", "KhoaHoc", "GiaoVien", "HocVienPhuHuynh", "PhuHuynh", "HocVien", "NhanVien"]
dau = ["/* Dữ liệu mẫu tại ngày chốt 31/12/2026 – SINH TỰ ĐỘNG bằng csdl/sinh_du_lieu.py, không sửa tay. */",
       "USE TrungTamNgoaiNgu;", "GO", "SET NOCOUNT ON;",
       "/* Chạy lại được: xóa dữ liệu cũ theo thứ tự ngược khóa ngoại */",
       "UPDATE DangKy SET MaLopGoc = NULL;"] + [f"DELETE FROM {b};" for b in BANG_XOA] + \
      ["DBCC CHECKIDENT ('ThongBao', RESEED, 0) WITH NO_INFOMSGS;", "DBCC CHECKIDENT ('NhatKy', RESEED, 0) WITH NO_INFOMSGS;", ""]
Path(__file__).with_name("seed.sql").write_text("\n".join(dau) + "\n\n".join(phan) + "\nGO\n", encoding="utf-8")
print(f"Đã ghi seed.sql: {len(hoc_vien)} học viên, {len(DK)} đăng ký, {len(buoi)} buổi, {len(diem_danh)} điểm danh, "
      f"{len(phieu_thu)} phiếu thu, {len(ket_qua)} kết quả, {len(chung_nhan)} chứng nhận")
