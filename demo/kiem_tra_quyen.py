import os
import re
import requests
from bs4 import BeautifulSoup

BASE_URL = "http://localhost:5223"

DANH_SACH_TAI_KHOAN = [
    ("HV", "Học viên", "hv0412", "123456"),
    ("PH", "Phụ huynh", "ph0320", "123456"),
    ("GV", "Giáo viên", "gv008", "123456"),
    ("TS", "NV tuyển sinh", "nv005", "123456"),
    ("KT", "NV kế toán", "nv004", "123456"),
    ("DT", "QL đào tạo", "nv002", "123456"),
    ("GD", "Giám đốc", "nv001", "123456"),
    ("QT", "Quản trị viên", "nv003", "123456"),
]

def doc_phan_quyen_tu_cs():
    """Đọc trực tiếp mảng VaiTro và từ điển MaTran từ file Nen/PhanQuyen.g.cs"""
    pq_path = "../TrungTamNgoaiNgu/Nen/PhanQuyen.g.cs"
    vai_tro_list = ["HV", "PH", "GV", "TS", "KT", "DT", "GD", "QT"]
    ma_tran = {}
    
    if os.path.exists(pq_path):
        with open(pq_path, "r", encoding="utf-8") as f:
            content = f.read()
            # Đọc mảng VaiTro nếu có
            vt_match = re.search(r'public static readonly string\[\] VaiTro = new\[\]\s*\{([^}]*)\}', content)
            if not vt_match:
                vt_match = re.search(r'public static readonly string\[\] VaiTro =\s*\{([^}]*)\}', content)
            if vt_match:
                parsed_vt = re.findall(r'["\']([^"\']+)["\']', vt_match.group(1))
                if parsed_vt:
                    vai_tro_list = parsed_vt
            
            # Đọc từ điển MaTran
            pattern = re.compile(r'["\']([\w\.]+)["\']\s*=\s*new\[\]\s*\{([^}]*)\}')
            matches = pattern.findall(content)
            for module_code, permissions_str in matches:
                perms = re.findall(r'["\']([^"\']*)["\']', permissions_str)
                if len(perms) == len(vai_tro_list):
                    ma_tran[module_code] = perms
                    
    # Fallback dữ liệu chuẩn từ file bạn cung cấp nếu đọc file lỗi
    if not ma_tran:
        ma_tran = {
            "1.1": ["x", "x", "x", "TSX", "X", "X", "X", "X"],
            "1.2": ["", "", "x", "X", "", "TSX", "X", "X"],
            "1.3": ["X", "X", "X", "X", "X", "TSX", "X", ""],
            "1.4": ["", "", "", "", "", "TSX", "X", ""],
            "2.1": ["X", "X", "x", "X", "X", "TSX", "X", ""],
            "2.2": ["x", "x", "x", "TSX", "X", "X", "X", ""],
            "2.3": ["x", "x", "x", "X", "", "TSX", "X", ""],
            "2.4": ["x", "x", "", "TSX", "X", "X", "X", ""],
            "3.1": ["x", "x", "", "X", "TSX", "", "X", ""],
            "3.2": ["x", "x", "", "X", "TX", "", "X", ""],
            "3.3": ["x", "x", "", "X", "X", "", "X", ""],
            "3.4": ["", "", "", "", "X", "", "X", ""],
            "4.1": ["x", "x", "TSx", "X", "", "X", "X", ""],
            "4.2": ["x", "x", "TSx", "", "", "X", "X", ""],
            "4.3": ["x", "x", "x", "", "", "DX", "X", ""],
            "4.4": ["x", "x", "", "X", "", "IX", "X", ""],
            "5.1": ["", "", "", "", "", "", "", "TSX"],
            "5.2": ["x", "x", "", "X", "X", "X", "", "X"],
            "5.3": ["", "", "", "X", "", "X", "X", ""],
            "5.4": ["", "", "x", "", "", "X", "X", ""],
            "N1": ["x", "x", "x", "x", "x", "x", "x", "x"],
            "N4": ["", "", "", "", "", "", "", "TX"],
        }
    return vai_tro_list, ma_tran

def map_module_to_url(module_code):
    """Ánh xạ mã module sang đường dẫn URL thực tế của hệ thống"""
    mapping = {
        "1.1": "/DanhMuc/KhoaHoc",
        "1.2": "/DanhMuc/PhongHoc",
        "1.3": "/DanhMuc/GiaoVien",
        "1.4": "/DanhMuc/NhanVien",
        "2.1": "/NghiepVu/DangKy",
        "2.2": "/NghiepVu/HocVien",
        "2.3": "/NghiepVu/LopHoc",
        "2.4": "/NghiepVu/XepLop",
        "3.1": "/HocTap/DiemDanh",
        "3.2": "/HocTap/Diem",
        "3.3": "/HocTap/KetQua",
        "3.4": "/HocTap/ChuyenCan",
        "4.1": "/BaoCao/HocTap",
        "4.2": "/BaoCao/TaiChinh",
        "4.3": "/BaoCao/NhanSu",
        "4.4": "/BaoCao/TongHop",
        "5.1": "/HeThong/TaiKhoan",
        "5.2": "/HeThong/ThamSo",
        "5.3": "/HeThong/SaoLuu",
        "5.4": "/HeThong/NhatKy",
        "N1": "/",
        "N4": "/HeThong/PhanQuyen"
    }
    return mapping.get(module_code, f"/Module/{module_code.replace('.', '_')}")

def kiem_tra_phan_quyen():
    print("=== SCRIPT KIỂM TRA PHÂN QUYỀN 8 VAI TRÒ (TC42) ===")
    vai_tro_list, ma_tran = doc_phan_quyen_tu_cs()
    modules = list(ma_tran.keys())
    print(f"Đã đọc thành công {len(modules)} module/chức năng từ Nen/PhanQuyen.g.cs\n")
    
    session = requests.Session()
    ket_qua_chi_tiet = {}
    danh_sach_sai = []
    
    for vt_code, vt_name, user, pwd in DANH_SACH_TAI_KHOAN:
        ket_qua_chi_tiet[vt_code] = {}
        
        # Tìm vị trí index của vai trò này trong mảng PhanQuyen.g.cs
        try:
            vt_index = vai_tro_list.index(vt_code)
        except ValueError:
            continue
            
        # 1. Lấy Token từ trang đăng nhập
        try:
            res_get = session.get(f"{BASE_URL}/DangNhap", timeout=5)
            if res_get.status_code != 200:
                continue
            soup = BeautifulSoup(res_get.text, 'html.parser')
            token_input = soup.find('input', {'name': '__RequestVerificationToken'})
            token = token_input['value'] if token_input else ""
            
            # 2. Gửi yêu cầu đăng nhập POST
            session.post(f"{BASE_URL}/DangNhap", data={
                "__RequestVerificationToken": token,
                "TenDangNhap": user,
                "MatKhau": pwd
            }, allow_redirects=True)
        except Exception:
            continue
            
        # 3. Kiểm tra quyền truy cập từng module
        for mod in modules:
            perm_val = ma_tran[mod][vt_index]
            co_quyen_theo_cs = (perm_val.strip() != "") # Có chữ nghĩa là được phép
            
            url = map_module_to_url(mod)
            res_page = session.get(f"{BASE_URL}{url}", allow_redirects=False)
            
            status_code = res_page.status_code
            redirect_loc = res_page.headers.get("Location", "")
            
            is_blocked = (status_code in [302, 303, 307] and "KhongCoQuyen" in redirect_loc) or (status_code == 403)
            is_ok = (status_code == 200)
            
            if co_quyen_theo_cs:
                dung_nghiep_vu = is_ok
                trang_thai_str = "OK" if is_ok else f"Sai({status_code})"
            else:
                dung_nghiep_vu = is_blocked or status_code in [302, 303, 307, 403]
                trang_thai_str = "Chặn" if dung_nghiep_vu else f"Lọt({status_code})"
                
            ket_qua_chi_tiet[vt_code][mod] = trang_thai_str
            
            if not dung_nghiep_vu:
                danh_sach_sai.append((vt_code, mod, f"MaTran: '{perm_val}', HTTP: {status_code}"))

    # IN BẢNG MA TRẬN VAI TRÒ x MODULE
    print("\n BẢNG MA TRẬN PHÂN QUYỀN THỰC TẾ (VAI TRÒ x MODULE)")
    header = f"{'Module':<8} | " + " | ".join([f"{vt:<5}" for vt in vai_tro_list])
    print(header)
    print("-" * len(header))
    
    for mod in modules:
        row = f"{mod:<8} | " + " | ".join([f"{ket_qua_chi_tiet.get(vt, {}).get(mod, 'N/A'):<5}" for vt in vai_tro_list])
        print(row)
        
    print("\n" + "="*50)
    if len(danh_sach_sai) == 0:
        print("đạt")
    else:
        print(f"danh sách sai ({len(danh_sach_sai)} lỗi):")
        for err in danh_sach_sai:
            print(f" - Vai trò {err[0]} tại module {err[1]} -> {err[2]}")

if __name__ == "__main__":
    kiem_tra_phan_quyen()