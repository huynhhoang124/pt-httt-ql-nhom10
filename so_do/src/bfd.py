"""BFD hệ thống quản lý trung tâm ngoại ngữ.

CHUC_NANG là nguồn duy nhất cho mã và tên chức năng: DFD, module, menu đều lấy từ đây.
Chạy: python so_do/src/bfd.py  ->  so_do/BFD_v3.png
"""
import sys
from pathlib import Path
from textwrap import fill

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle

GOC = "0. HỆ THỐNG QUẢN LÝ\nTRUNG TÂM NGOẠI NGỮ"
CHUC_NANG = {
    "1.0": ("Quản lý danh mục và hồ sơ", [
        "Tiếp nhận hồ sơ, kiểm tra trình độ học viên",
        "Quản lý hồ sơ giáo viên",
        "Quản lý khóa học",
        "Quản lý phòng học",
    ]),
    "2.0": ("Quản lý lớp học và lịch học", [
        "Mở lớp, phân công giáo viên",
        "Đăng ký học, xếp học viên vào lớp",
        "Xếp lịch, kiểm tra trùng lịch",
        "Xử lý chuyển lớp, bảo lưu, nghỉ học",
    ]),
    "3.0": ("Quản lý học phí", [
        "Tính học phí, áp dụng ưu đãi",
        "Lập phiếu thu",
        "Theo dõi công nợ",
        "Thống kê doanh thu",
    ]),
    "4.0": ("Quản lý học tập", [
        "Điểm danh, tính chuyên cần",
        "Nhập điểm thành phần",
        "Tổng kết, xếp loại",
        "Cấp chứng nhận hoàn thành khóa học",
    ]),
    "5.0": ("Quản lý hệ thống và báo cáo", [
        "Quản lý tài khoản, phân quyền",
        "Gửi thông báo",
        "Lập báo cáo tuyển sinh, tình trạng lớp",
        "Lập báo cáo chuyên cần, kết quả, giảng dạy",
    ]),
}
RA = Path(__file__).resolve().parents[1] / "BFD_v3.png"


def hop(ax, x, y, w, h, chu, dam=False, co=10):
    ax.add_patch(Rectangle((x, y), w, h, fill=False, lw=1.4))
    ax.text(x + w / 2, y + h / 2, chu, ha="center", va="center", fontsize=co,
            weight="bold" if dam else "normal", linespacing=1.3)


def ve():
    W, GAP, H, BUOC = 3.9, 0.45, 1.05, 1.4
    tong = len(CHUC_NANG) * W + (len(CHUC_NANG) - 1) * GAP
    fig, ax = plt.subplots(figsize=(tong * 0.75, 9.6 * 0.75))

    hop(ax, tong / 2 - 2.6, 8.2, 5.2, 1.1, GOC, dam=True, co=12)
    ax.plot([tong / 2] * 2, [8.2, 7.7], color="black", lw=1.2)
    ax.plot([W / 2, tong - W / 2], [7.7] * 2, color="black", lw=1.2)

    for i, (ma, (ten, cons)) in enumerate(CHUC_NANG.items()):
        x = i * (W + GAP)
        ax.plot([x + W / 2] * 2, [7.7, 7.2], color="black", lw=1.2)
        hop(ax, x, 6.2, W, H, fill(f"{ma}. {ten}", 22), dam=True, co=10.5)
        y_cuoi = 0
        for j, con in enumerate(cons, 1):
            y = 4.85 - (j - 1) * BUOC
            hop(ax, x + 0.55, y, W - 0.55, H, fill(f"{ma[0]}.{j}. {con}", 25), co=9.5)
            ax.plot([x + 0.25, x + 0.55], [y + H / 2] * 2, color="black", lw=1.2)
            y_cuoi = y + H / 2
        ax.plot([x + 0.25] * 2, [6.2, y_cuoi], color="black", lw=1.2)

    ax.set_xlim(-0.2, tong + 0.2)
    ax.set_ylim(0.5, 9.5)
    ax.set_aspect("equal")
    ax.axis("off")
    fig.savefig(RA, dpi=160, bbox_inches="tight", facecolor="white")
    print("Đã ghi", RA)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # console Windows mặc định cp1252
    ve()
