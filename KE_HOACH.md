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

## Giai đoạn 1 – Xác định, lựa chọn, lập kế hoạch
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

### 2.3 DFD phân cấp – `so_do/`
- Ngữ cảnh, mức 0 (giai đoạn 0).
- **5 sơ đồ mức 1**: DFD-1.0 … DFD-5.0, mỗi sơ đồ có 4 tiến trình con x.1–x.4, giữ nguyên tác nhân/kho/luồng của mức 0 (cân bằng). Có thể tách kho con (vd D2 → D2.1 Lớp, D2.2 Đăng ký, D2.3 Lịch học) nếu cần nhưng phải ghi rõ thuộc D2.
- Dừng ở mức 1 (các tiến trình x.y đã là cơ bản).

### 2.4 Đặc tả xử lý + từ điển dữ liệu – `phan_tich/04_dac_ta.md`
- Ngôn ngữ cấu trúc cho các tiến trình cơ sở chính (2.2 đăng ký/xếp lớp, 3.2 lập phiếu thu, 4.1 điểm danh, 4.3 tổng kết).
- **Bảng quyết định**: 3.1 tính học phí/ưu đãi (điều kiện: HV cũ, đăng ký nhóm, đóng 1 lần…); 4.3 xếp loại.
- **Cây quyết định**: 2.4 chuyển lớp/bảo lưu/nghỉ; cảnh báo chuyên cần.
- **Từ điển dữ liệu** cho mọi luồng và kho: Tên – Ý nghĩa – Cấu trúc – Nguồn gốc.

### 2.5 Báo cáo phân tích (nộp)
Tiêu đề → Mục lục → Giới thiệu → Phương pháp luận → Kết quả thu thập → Phân tích chức năng + BFD → DFD các mức → Đặc tả → Kết luận → Phụ lục (chứng từ, phiếu phỏng vấn).

## Giai đoạn 3 – Thiết kế hệ thống
### 3.1 Thực thể & thuộc tính – `thiet_ke/01_thuc_the.md`
- Lấy 20 đối tượng BT1, phân loại (xác thực / sự kiện / quan hệ), đánh dấu thuộc tính # / R / S. Loại thứ sinh: Công nợ (= học phí − đã thu), tỷ lệ chuyên cần, điểm tổng kết.
- Dự kiến thực thể chính: HocVien, PhuHuynh, GiaoVien, NhanVien, KhoaHoc, LopHoc, PhongHoc, DangKy, BuoiHoc (lịch), DiemDanh, UuDai, PhieuThu, DauDiem (cột điểm), Diem, KetQua, ChungNhan, TaiKhoan, ThongBao.

### 3.2 Quan hệ + ERD – `so_do/ERD.png`
- Bảng quan hệ (bậc, kiểu). N-N quan trọng: HocVien–LopHoc qua DangKy; HocVien–BuoiHoc qua DiemDanh; DangKy–DauDiem qua Diem; PhuHuynh–HocVien.
- Vẽ ERD ký pháp bài giảng (hình thoi, động từ).
- Đối chiếu kho D1–D5 ↔ thực thể.

### 3.3 Chuẩn hóa – `thiet_ke/02_chuan_hoa.md`
- Làm mẫu theo bài giảng (Hóa đơn → …) trên 3 chứng từ: **Phiếu đăng ký**, **Phiếu thu**, **Bảng điểm lớp**: dạng chưa chuẩn → 1NF → 2NF → 3NF → trộn bảng cùng khóa. Kết quả phải trùng tập bảng ở 3.1–3.2.

### 3.4 CSDL vật lý – `csdl/schema.sql`, `csdl/seed.sql`
- SQL Server: bảng, PK, FK, CHECK (trạng thái, điểm 0–10, sĩ số), UNIQUE, index trường tra cứu. Bảng mô tả từng tệp (Tên trường – Kiểu – Ràng buộc – Ý nghĩa).
- Dữ liệu mẫu đủ để chạy báo cáo (≈30 HV, 5 GV, 4 khóa, 6 lớp).
- Một số view/truy vấn cho báo cáo MIS (doanh thu, công nợ, chuyên cần, tỷ lệ lấp đầy).

### 3.5 Thiết kế phần mềm – `thiet_ke/03_module.md`
- Sơ đồ module Top-down: module chính → 5 phân hệ ↔ 1.0–5.0 → module con ↔ x.y; thêm đăng nhập/phân quyền, sao lưu (số module ≥ số tiến trình).
- Ma trận phân quyền: vai trò (7 tác nhân + Quản trị) × chức năng.

### 3.6 Thiết kế giao diện – `thiet_ke/04_giao_dien.md`
- Bảng ánh xạ: luồng vào DFD → Form (điền mẫu), luồng ra DFD → Report.
- Form chính: hồ sơ HV, đăng ký lớp, xếp lịch, phiếu thu, điểm danh, nhập điểm. Report: phiếu thu, danh sách lớp, lịch dạy, bảng điểm, công nợ, BC tuyển sinh/doanh thu/chuyên cần.
- Sơ đồ thực đơn phân cấp theo BFD; mockup đen trắng; quy tắc trợ giúp và thông báo lỗi.

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
