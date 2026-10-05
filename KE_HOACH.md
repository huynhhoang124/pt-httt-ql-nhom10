# Plan xuyên suốt – Hệ thống quản lý trung tâm ngoại ngữ (Nhóm 10)

## Context
Dự án đang có: Bài tập 1 (6 góc nhìn), BFD_v2, DFD ngữ cảnh, DFD mức 0. Chưa có lộ trình chung, và DFD-0 đang vi phạm lý thuyết (thiếu luồng liên tiến trình, chưa cân bằng với ngữ cảnh, ký pháp lẫn elip/chữ nhật). Plan này là xương sống cho toàn bộ dự án: mọi phần làm theo đúng 5 giai đoạn SDLC và quy tắc trong `ly_thuyet/ly_thuyet_ap_dung.md`, mỗi giai đoạn ra một sản phẩm nộp được, cuối kỳ gộp thành một báo cáo tổng.

Mặc định đã chọn (đổi được): sản phẩm cuối = báo cáo + CSDL SQL Server + demo nhỏ; sơ đồ sinh bằng script Python (giống ảnh hiện tại); báo cáo Word + PDF, Times New Roman, đen trắng, bảng xám nhạt.


## Nguyên tắc xuyên suốt (kiểm tra ở mọi giai đoạn)
1. **Một nguồn sự thật**: danh sách chức năng (BFD) → tiến trình DFD → module phần mềm → menu giao diện dùng chung mã số 1.0, 1.1…
2. **Cân bằng DFD**: tác nhân + luồng ngữ cảnh = mức 0 = gộp các mức 1.
3. **Nhất quán DFD–ERD**: mỗi kho D1–D5 ↔ tập thực thể cụ thể; mỗi luồng vào/ra kho dùng thuộc tính có trong ERD.
4. **Form = luồng vào, Report = luồng ra** của DFD (không có form/report "mồ côi").
5. Tên chức năng: động từ + bổ ngữ; tên kho/luồng: danh từ; tên bảng/trường: không dấu.

## Cấu trúc repo (mở rộng từ hiện tại)
```
bai_tap_1/                 (giữ nguyên)
ly_thuyet/                 (giữ nguyên)
so_do/                     ảnh PNG cuối cùng
so_do/src/                 script Python vẽ sơ đồ (bfd.py, dfd.py, erd.py) – sửa ở đây rồi chạy lại
phan_tich/                 thu thập thông tin, mô tả chức năng, từ điển dữ liệu, đặc tả xử lý (.md)
thiet_ke/                  thực thể, chuẩn hóa, CSDL, module, form/report (.md)
csdl/                      schema.sql, seed.sql
demo/                      ứng dụng demo (giai đoạn 4)
bao_cao/                   create_report.py + Word/PDF từng giai đoạn và bản tổng
```
Tái dùng: `create_report.py` của Bài tập 1 (đường dẫn trong memory) làm khung sinh Word/PDF cho mọi báo cáo.

---

## Giai đoạn 0 – Sửa phần đã làm ✅ (xong 2026-10-02)
- **BFD_v3**: đổi 1.1 thành "Tiếp nhận hồ sơ, kiểm tra trình độ học viên" (bổ sung quy trình tư vấn/kiểm tra đầu vào của BT1 → thỏa nguyên tắc *đầy đủ*). Giữ 5×4 chức năng.
- **DFD ngữ cảnh**: giữ, chỉ rà lại nhãn luồng cho khớp BFD_v3.
- **DFD mức 0 (vẽ lại)**:
  - Tiến trình dạng chữ nhật bo góc Gane & Sarson, số 1.0…5.0, kho chữ nhật hở D1…D5.
  - Thêm luồng còn thiếu: HV/PH → 2.0 "Yêu cầu đăng ký, chuyển lớp/bảo lưu"; 3.0 → Kế toán "Phiếu thu".
  - Thêm luồng đọc kho chéo: 2.0 đọc D1 (HV, GV, khóa, phòng); 3.0 đọc D1 (học phí khóa) + D2 (đăng ký); 4.0 đọc D2 (danh sách lớp, buổi học); 5.0 đọc D1–D4; 5.0 → các tác nhân "Thông báo" đọc từ D5.
  - Luồng tiến trình ↔ kho vẽ 2 mũi tên riêng, có nhãn khi cần.
- Kiểm tra bằng bảng cân bằng ngữ cảnh ↔ mức 0 (mục Verification).

## Giai đoạn 1 – Xác định, lựa chọn, lập kế hoạch ✅ (xong 2026-10-02)
Sản phẩm: `phan_tich/01_ke_hoach.md` → chương 1 báo cáo tổng.
- Phạm vi: 1 cơ sở, ~500 HV, 30 GV (từ BT1); ngoài phạm vi: thanh toán online, QR, đa cơ sở.
- Khả thi: kinh tế (chi phí hữu hình: máy chủ, phát triển; vô hình: đào tạo, thay đổi thói quen; lợi ích: giảm sai sót học phí, giảm thời gian báo cáo), kỹ thuật (stack BT1), tác nghiệp (nhân viên dùng được), pháp lý (bảo vệ dữ liệu cá nhân trẻ vị thành niên).
- Định vị hệ thống: TPS + MIS + yếu tố CRM.
- Lịch dự án (Gantt đơn giản theo các giai đoạn dưới).

## Giai đoạn 2 – Phân tích hệ thống
### 2.1 Thu thập thông tin – `phan_tich/02_thu_thap.md` ✅ (xong 2026-10-02)
- Báo cáo nghiên cứu tài liệu theo mẫu bài giảng, dựa trên chứng từ mẫu tự xây: **phiếu đăng ký học, phiếu thu học phí, sổ điểm danh, bảng điểm, chứng nhận**. Các chứng từ này là đầu vào cho chuẩn hóa ở 3.3.
- Kế hoạch phỏng vấn (Giám đốc, QL đào tạo, NV tuyển sinh, Kế toán, GV): câu hỏi mở + đóng; 1 phiếu điều tra học viên (tiêu đề – định danh – câu hỏi – kết thúc).
- Bảng so sánh ưu/nhược 6 phương pháp, nêu phương pháp nhóm dùng.

### 2.2 Phân tích chức năng – `phan_tich/03_chuc_nang.md` ✅ (xong 2026-10-02)
- Bảng mô tả từng chức năng lá BFD: Mã – Tên – Mô tả – Đầu vào – Đầu ra – Người thực hiện (20 dòng).
- Ma trận thực thể–chức năng (chức năng nào tạo/đọc/sửa dữ liệu gì) → làm cầu nối sang DFD và ERD.

### 2.3 DFD phân cấp – `so_do/` ✅ (xong 2026-10-02)
- Ngữ cảnh, mức 0 (giai đoạn 0).
- **5 sơ đồ mức 1**: DFD-1.0 … DFD-5.0, mỗi sơ đồ có 4 tiến trình con x.1–x.4, giữ nguyên tác nhân/kho/luồng của mức 0 (cân bằng). Có thể tách kho con (vd D2 → D2.1 Lớp, D2.2 Đăng ký, D2.3 Lịch học) nếu cần nhưng phải ghi rõ thuộc D2.
- Dừng ở mức 1 (các tiến trình x.y đã là cơ bản).

### 2.4 Đặc tả xử lý + từ điển dữ liệu – `phan_tich/04_dac_ta.md` ✅ (xong 2026-10-02)
- Ngôn ngữ cấu trúc cho các tiến trình cơ sở chính (2.2 đăng ký/xếp lớp, 3.2 lập phiếu thu, 4.1 điểm danh, 4.3 tổng kết).
- **Bảng quyết định**: 3.1 tính học phí/ưu đãi (điều kiện: HV cũ, đăng ký nhóm, đóng 1 lần…); 4.3 xếp loại.
- **Cây quyết định**: 2.4 chuyển lớp/bảo lưu/nghỉ; cảnh báo chuyên cần.
- **Từ điển dữ liệu** cho mọi luồng và kho: Tên – Ý nghĩa – Cấu trúc – Nguồn gốc.

### 2.5 Báo cáo phân tích (nộp) ✅ (xong 2026-10-02 – `bao_cao/Bao_cao_GD1_GD2_Nhom_10.docx/.pdf`)
Tiêu đề → Mục lục → Giới thiệu → Phương pháp luận → Kết quả thu thập → Phân tích chức năng + BFD → DFD các mức → Đặc tả → Kết luận → Phụ lục (chứng từ, phiếu phỏng vấn).

## Giai đoạn 3 – Thiết kế hệ thống
### 3.1 Thực thể & thuộc tính – `thiet_ke/01_thuc_the.md` ✅ (xong 2026-10-04)
- 20 đối tượng BT1 → **26 thực thể** (1 xác thực, 9 chức năng, 10 sự kiện, 6 quan hệ), chia theo kho D1–D5; thuộc tính # / QH / R / S, tên trường không dấu.
- Loại Công nợ (thứ sinh); bỏ DauDiem (Loại điểm là miền giá trị); thêm NhatKy, ThamSo. Có 8 thuộc tính thứ sinh "chốt" được lưu, có lý do.
- Khóa của đăng ký là (MaHV, MaLop); 34 thuộc tính quan hệ ở mục 9 là đầu vào cho 3.2. Đã đối chiếu từng trường của 5 chứng từ mẫu.

### 3.2 Quan hệ + ERD – `thiet_ke/02_quan_he.md`, `so_do/ERD_1_nghiep_vu.png`, `so_do/ERD_2_he_thong.png` ✅ (xong 2026-10-05)
- 29 quan hệ, gồm 16 quan hệ 1–N, 8 quan hệ 1–1, 5 quan hệ N–N (cộng quan hệ N–N "Học" giữa HV và lớp qua DangKy) và 1 quan hệ bậc 1 (đăng ký gốc). Dùng đủ 34 thuộc tính quan hệ của 3.1.
- ERD vẽ theo ký pháp bài giảng (hình thoi chứa động từ, 1/N, thực thể quan hệ chữ hoa kèm elip như Hình 4.45), chia 2 hình. Sơ đồ sinh bằng `so_do/src/erd.py`: script đọc bảng quan hệ trong .md, kiểm tra 8 quy tắc rồi mới vẽ.
- Đối chiếu DFD–ERD theo 2 ràng buộc của bài giảng: mọi luồng ghi và đọc kho, mọi báo cáo đều có đường đi trên ERD.

### 3.3 Chuẩn hóa – `thiet_ke/03_chuan_hoa.md` ✅ (xong 2026-10-05)
- Chuẩn hóa đủ 3 bước của Ví dụ 3 (Hóa đơn) cho **Phiếu đăng ký**, **Phiếu thu**, **Bảng điểm lớp**; làm rút gọn cho Sổ điểm danh và Chứng nhận. Liệt kê đủ phụ thuộc hàm (F1–F11, G1–G6, H1–H6), kèm bảng dị thường khi lưu nguyên chứng từ.
- Trộn bảng: thống nhất tên (học phí gốc khác học phí khóa); hai khóa tương đương của đăng ký → chọn (MaHV, MaLop); không trộn DangKy, HocPhi, KetQua vì là ba đối tượng khác nhau.
- `thiet_ke/kiem_tra_chuan_hoa.py`: 22/26 thực thể tìm lại được từ chứng từ, khóa trùng thiết kế, 0 thuộc tính thừa. Kết quả 3NF trùng với 3.1.

### 3.4 CSDL vật lý – `thiet_ke/04_csdl.md`, `csdl/` ✅ (xong 2026-10-05)
- SQL Server (chạy trên 2022 Express, bảng mã `Vietnamese_100_CI_AS`). Có 26 bảng, 161 trường, 35 khóa ngoại (34 theo 3.1 + `FK_ThongBao_GiamHo`), 63 CHECK, 16 chỉ mục. CHECK chỉ dùng cho bất biến; con số chính sách đọc từ bảng ThamSo.
- `views.sql`: 9 view và 2 hàm tính thuộc tính thứ sinh và báo cáo MIS (doanh thu, `fn_CongNo(@Ngay)`, sĩ số – lấp đầy, chuyên cần, kết quả, giảng dạy, tuyển sinh).
- `seed.sql` do `sinh_du_lieu.py` sinh, chốt ngày 31/12/2026, bám đúng 5 chứng từ mẫu. Có 46 học viên, 7 lớp, 940 lượt điểm danh, 92 phiếu thu, đủ các tình huống: chuyển lớp, bảo lưu, nghỉ học, lớp hủy, nợ quá hạn…
- Kiểm tra: `chay.ps1` chạy schema → views → seed → `kiem_tra.sql`, kết quả 24/24 đạt (có thử chèn 5 bản ghi sai đều bị chặn). `sinh_mo_ta.py` đối chiếu schema với 3.1 rồi sinh `csdl/mo_ta_bang.md`.

### 3.5 Thiết kế phần mềm – `thiet_ke/05_module.md` ✅ (xong 2026-10-05; `so_do/So_do_module.png`, `so_do/src/module.py`). Đã bổ sung 3 luồng QL đào tạo ↔ 2.3, 4.3 vào DFD
- Sơ đồ module Top-down: module chính → 5 phân hệ ↔ 1.0–5.0 → module con ↔ x.y; thêm đăng nhập/phân quyền, sao lưu (số module ≥ số tiến trình).
- Ma trận phân quyền: vai trò (7 tác nhân + Quản trị) × chức năng.

### 3.6 Thiết kế giao diện – `thiet_ke/06_giao_dien.md` ✅ (xong 2026-10-05; 18 form, 18 báo cáo, thực đơn `so_do/Thuc_don.png`, 4 phác thảo `so_do/Mau_*.png`, kiểm tra `so_do/src/giao_dien.py`)
- Bảng ánh xạ: luồng vào DFD → Form (điền mẫu), luồng ra DFD → Report.
- Form chính: hồ sơ HV, đăng ký lớp, xếp lịch, phiếu thu, điểm danh, nhập điểm. Report: phiếu thu, danh sách lớp, lịch dạy, bảng điểm, công nợ, BC tuyển sinh/doanh thu/chuyên cần.
- Sơ đồ thực đơn phân cấp theo BFD; mockup đen trắng; quy tắc trợ giúp và thông báo lỗi.

### 3.7 Báo cáo thiết kế (nộp) ✅ (xong 2026-10-05 – `bao_cao/Bao_cao_GD3_Nhom_10.docx/.pdf`, 105 trang)
Mở đầu → 3.1–3.6 (gộp từ `thiet_ke/01–06`, giữ đánh số 3.k để mã bước = số chương) → Kết luận → Phụ lục A mô tả tệp dữ liệu (`csdl/mo_ta_bang.md`). Tham chiếu chéo đổi tự động ("04_dac_ta mục 1.1" → "báo cáo phân tích, mục 6.1.1"); hình quá ngang (module, thực đơn) xoay dọc trang. Chạy `python bao_cao/lam_bao_cao.py thiet_ke`.

## Giai đoạn 4 – Cài đặt và khai thác
- **Demo** (`demo/`, ASP.NET Core + EF Core theo BT1; nếu thiếu thời gian thì chỉ CSDL + truy vấn): đăng nhập theo vai trò, hồ sơ HV, đăng ký lớp, lập phiếu thu, điểm danh, nhập điểm, 2–3 báo cáo. Chỉ làm chức năng đã có trong DFD.
- `phan_tich/05_cai_dat.md`: kế hoạch cài đặt (cài phần mềm, cấu hình, phân quyền, hồ sơ cấu hình); chuyển đổi dữ liệu từ Excel cũ; huấn luyện 3 mức; phương pháp chuyển đổi đề xuất **theo giai đoạn** (danh mục → lớp/lịch → học phí → học tập) có lý do; kế hoạch kiểm thử (test case theo tiến trình).

## Giai đoạn 5 – Bảo trì
- Phân loại hiệu chỉnh / thích nghi / phòng ngừa; thứ tự ưu tiên; quản lý cấu hình bằng Git (nhánh, PR, tag phiên bản); hướng phát triển (thanh toán online, QR, đa cơ sở).

## Báo cáo tổng cuối kỳ
`bao_cao/Bao_cao_tong_Nhom_10.docx/.pdf` = BT1 + GĐ1 → GĐ5 theo khung báo cáo phân tích trong lý thuyết.

## Phân công gợi ý (sửa theo nhóm)
| Thành viên | Phần chính |
|---|---|
| Hoàng Văn Huynh | Điều phối, GĐ0, DFD mức 0, ERD, review PR, báo cáo tổng |
| Nguyễn Gia Ân | Thu thập thông tin, DFD-1.0 + 2.0 |
| Vũ Văn Hùng | DFD-3.0, bảng quyết định học phí, CSDL vật lý |
| Nguyễn Văn Luận | DFD-4.0 + 5.0, chuẩn hóa |
| Trần Thu Thủy | Mô tả chức năng, từ điển dữ liệu, form/report |
| Trần Minh Quân | Module + phân quyền, demo, cài đặt/bảo trì |

Mỗi người làm nhánh riêng → PR vào main → Huynh duyệt.

## Thứ tự thực hiện
GĐ0 → 2.1 → 2.2 → 2.3 → 2.4 → (nộp báo cáo phân tích) → GĐ1 bổ sung → 3.1 → 3.2 → 3.3 → 3.4 → 3.5 → 3.6 → (nộp báo cáo thiết kế) → GĐ4 → GĐ5 → báo cáo tổng.

## Verification (chạy sau mỗi giai đoạn)
- **Cân bằng DFD**: bảng tác nhân × luồng ở ngữ cảnh, mức 0, mức 1 – mọi dòng khớp.
- **Quy tắc vẽ**: không tác nhân↔kho, kho↔kho, tác nhân↔tác nhân; mọi tiến trình/kho có vào và ra.
- **BFD ↔ DFD ↔ module ↔ menu**: cùng danh sách mã, không thiếu/thừa.
- **DFD ↔ ERD**: mỗi kho ánh xạ thực thể; mỗi thuộc tính trên luồng có trong bảng.
- **Chuẩn hóa**: kết quả 3NF trùng schema.sql.
- **CSDL**: chạy schema.sql + seed.sql trên SQL Server không lỗi; các truy vấn báo cáo trả số liệu đúng với dữ liệu mẫu.
- **Form/Report**: mỗi form/report trỏ về một luồng DFD.
- Báo cáo xuất Word + PDF, xem lại PDF từng trang.
