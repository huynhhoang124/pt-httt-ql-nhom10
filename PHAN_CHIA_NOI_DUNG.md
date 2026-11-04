# Phân chia nội dung dự án theo thành viên – Nhóm 10

Lập ngày 04/11/2026. Mục đích: mỗi người **nắm chắc một phần của dự án** để trình bày và trả lời vấn đáp phần đó: lý thuyết theo bài giảng, nhóm đã làm gì, kết quả nằm ở tệp nào, mục nào trong thư mục. Hoàng Văn Huynh và Nguyễn Văn Luận nhận phần nhiều hơn một chút. Phần slide của mỗi người vẫn theo `PHAN_CONG_SUA.md` mục 7 (bộ slide: https://claude.ai/artifact/PBqgA262dz8XCm871dM9vB).

Cách dùng cho mỗi người:
1. Đọc **Lý thuyết cần nắm** (tóm tắt ở `ly_thuyet/ly_thuyet_ap_dung.md`, đầy đủ trong bài giảng "Hệ thống thông tin quản lý", PTIT 12/2024).
2. Đọc các tệp, các mục ở bảng **Đọc ở đâu**; mở các hình trong `so_do/`.
3. Chạy lệnh kiểm tra của phần mình (nếu có) để thấy kết quả "đạt".
4. Tự trả lời các **Câu hỏi vấn đáp có thể gặp**; câu nào chưa trả lời được thì hỏi Huynh hoặc người phụ trách phần liền kề.

## Tổng quan phân chia

| Người | Phần phụ trách | Giai đoạn | Slide trình bày | Khối lượng |
|---|---|---|---|---|
| Hoàng Văn Huynh | Tổng quan, phương pháp luận, định vị hệ thống; GĐ1 xác định, khả thi, kế hoạch; DFD ngữ cảnh và mức 0, nguyên tắc nhất quán; quản lý cấu hình Git; kết luận, báo cáo tổng | GĐ1, GĐ2, GĐ5 | 1, 2, 3, 4, 16 | Nhiều hơn |
| Nguyễn Văn Luận | DFD mức 1 của 4.0, 5.0 và cân bằng DFD; thực thể, thuộc tính; quan hệ và ERD; chuẩn hóa; đặc tả và demo phân hệ 4.0 | GĐ2, GĐ3, GĐ4 | 7, 9 | Nhiều hơn |
| Nguyễn Gia Ân | Thu thập thông tin; BFD và mô tả chức năng; DFD mức 1 của 1.0, 2.0; đặc tả và demo phân hệ 1.0, 2.0 | GĐ2, GĐ4 | 5, 6 | Vừa |
| Vũ Văn Hùng | DFD mức 1 của 3.0; bảng quyết định học phí; CSDL vật lý; nâng cấp CSDL; demo phân hệ 3.0 | GĐ2, GĐ3, GĐ4, GĐ5 | 8, 10 | Vừa |
| Trần Thu Thủy | Từ điển dữ liệu; thiết kế giao diện (form, báo cáo, thực đơn, thông báo lỗi); bộ mã thống nhất và sinh mã; kiểm thử; demo báo cáo 5.3, 5.4 | GĐ2, GĐ3, GĐ4 | 11, 13 | Vừa |
| Trần Minh Quân | Thiết kế phần mềm (module, phân quyền, kiến trúc); cài đặt, chuyển đổi, huấn luyện; bảo trì; kiến trúc bản demo | GĐ3, GĐ4, GĐ5 | 12, 14, 15 | Vừa |

Quy ước đọc đường dẫn: `phan_tich/` (GĐ1, GĐ2, GĐ4, GĐ5), `thiet_ke/` (GĐ3), `so_do/` (hình và bảng cân bằng DFD, script vẽ ở `so_do/src/`), `csdl/` (SQL), `demo/` (bản demo, kiểm thử), `bao_cao/` (Word, PDF). Mã màn hình F… là form, R… là báo cáo, FN… là chức năng nền; trang của màn hình nằm ở `demo/TrungTamNgoaiNgu/Pages/<Thư mục>/<Tên>.cshtml` (giao diện) và `.cshtml.cs` (xử lý).

---

## 1. Hoàng Văn Huynh – tổng quan, GĐ1, DFD mức 0, quản lý cấu hình, kết luận

### 1.1 Phần phụ trách
- Đề tài, bài toán, phương pháp luận và lộ trình năm giai đoạn của cả dự án.
- GĐ1: xác định hệ thống, so sánh ba phương án, khả thi năm mặt, định vị hệ thống, kế hoạch dự án và rủi ro.
- DFD mức ngữ cảnh và mức 0: 7 tác nhân, 5 kho, 5 tiến trình; cân bằng ngữ cảnh ↔ mức 0.
- Nguyên tắc xuyên suốt của nhóm (một bộ mã thống nhất, cân bằng DFD, nhất quán DFD–ERD, form = luồng vào, report = luồng ra).
- GĐ5: quản lý cấu hình bằng Git (nhánh, pull request, phiên bản, phân quyền trên kho mã).
- Kết luận, bài học; báo cáo tổng và cách dựng báo cáo tự động; điều phối phần hỏi đáp.

### 1.2 Lý thuyết cần nắm
- **HTTT quản lý**: thu thập, xử lý, lưu trữ, truyền đạt thông tin để hỗ trợ ra quyết định. Ba cấp thông tin, quyết định: chiến lược, chiến thuật, tác nghiệp. Chất lượng thông tin: chính xác, đầy đủ, tin cậy, phù hợp, kịp thời, được bảo vệ (bài giảng Ch.1).
- **Phân loại hệ thống**: theo cấp quản lý (ESS, MIS, DSS, TPS), theo chức năng, theo tích hợp (ERP, SCM, CRM). Hệ thống của nhóm: chủ yếu **TPS** (ghi danh, thu học phí, điểm danh, nhập điểm) + **MIS** (báo cáo định kỳ, so sánh kỳ này với kỳ trước), có yếu tố **CRM**.
- **Vòng đời phát triển (SDLC) 5 giai đoạn**: xác định, lựa chọn, lập kế hoạch → phân tích → thiết kế → cài đặt và khai thác → bảo trì.
- **GĐ1**: phạm vi; các phương án; đánh giá bằng điểm có trọng số; khả thi kinh tế (lợi ích, chi phí hữu hình và vô hình, thời gian hoàn vốn), kỹ thuật, tác nghiệp, pháp lý; nguyên tắc chỉ mua phần mềm sẵn khi đáp ứng khoảng 80% yêu cầu.
- **DFD (ký pháp Gane & Sarson)**: tiến trình (động từ + bổ ngữ, trùng tên BFD, có số hiệu), kho (danh từ, D1…), tác nhân ngoài, luồng dữ liệu (danh từ). Quy tắc vẽ: tiến trình và kho phải có cả luồng vào và ra; **tác nhân ↔ kho, tác nhân ↔ tác nhân, kho ↔ kho không nối trực tiếp**. Ngữ cảnh: 1 tiến trình số 0, không có kho. Mức 0: các tiến trình 1.0…5.0 ứng với cấp 1 của BFD, giữ nguyên tác nhân và luồng của ngữ cảnh. **Cân bằng** giữa các mức.
- **Quản lý cấu hình** (Ch.5): ghi vết phiên bản, phân quyền sửa đổi; bảo trì gồm hiệu chỉnh, thích nghi, phòng ngừa (và cải tiến).
- **Báo cáo phân tích** (Ch.3 mục 2.4): tiêu đề → mục lục → lời giới thiệu → nội dung → kết luận → phụ lục.

### 1.3 Đọc ở đâu

| Nội dung | Tệp | Mục |
|---|---|---|
| Lý thuyết tóm tắt | `ly_thuyet/ly_thuyet_ap_dung.md` | 1 Khung chung, 2.3 DFD, 4 Cài đặt, 5 Định vị |
| Bối cảnh, vấn đề, mục tiêu | `phan_tich/01_ke_hoach.md` | 1.1 – 1.3 |
| Phạm vi, ba phương án A/B/C, điểm có trọng số 2,90 / 3,20 / 4,05 | `phan_tich/01_ke_hoach.md` | 2.1 – 2.3 |
| Khả thi kinh tế (lợi ích 96 triệu/năm, hoàn vốn 1,3 – 3,7 năm), kỹ thuật, tác nghiệp, pháp lý, chính trị | `phan_tich/01_ke_hoach.md` | 3.1 – 3.6 |
| Định vị hệ thống TPS + MIS + CRM | `phan_tich/01_ke_hoach.md` | 4 |
| Lịch, nguồn lực, rủi ro | `phan_tich/01_ke_hoach.md` | 5.1 – 5.3 |
| Hình DFD ngữ cảnh, mức 0 | `so_do/DFD_muc_ngu_canh.png`, `so_do/DFD_muc_0.png` | – |
| Cân bằng ngữ cảnh ↔ mức 0, kho ↔ tiến trình | `so_do/can_bang_dfd.md` | "Tác nhân: mức ngữ cảnh ↔ mức 0", "Kho dữ liệu ↔ tiến trình mức 0" |
| Dữ liệu vẽ DFD và chương trình kiểm tra quy tắc, cân bằng | `so_do/src/dfd.py` | – |
| Nguyên tắc xuyên suốt, lộ trình, tiến độ | `KE_HOACH.md` | "Nguyên tắc xuyên suốt", các mục GĐ0–GĐ5, "Cập nhật 01/11", "Kiểm tra lại toàn bộ 02/11" |
| Quản lý cấu hình bằng Git | `phan_tich/06_bao_tri.md` | 3.1 Nhánh và pull request, 3.2 Phiên bản, 3.4 Phân quyền trên kho mã |
| Kết luận, cách dựng báo cáo tự động | `bao_cao/lam_bao_cao.py` (hàm `bao_cao_tong`), `bao_cao/Bao_cao_tong_Nhom_10.docx` | phần KẾT LUẬN |
| Bài tập 1 (6 góc nhìn) – điểm xuất phát | `bai_tap_1/Bao_cao_Bai_tap_1_Nhom_10.pdf` | – |

### 1.4 Số liệu phải nhớ
1 cơ sở, ~500 học viên, 30 giáo viên, 25–30 lớp mỗi kỳ · 8 vấn đề V1–V8 · phương án C 4,05 điểm (A 2,90; B 3,20; phần mềm có sẵn chỉ đáp ứng 65–70%) · lợi ích 96 triệu/năm, chi phí vận hành 42 triệu/năm, đầu tư 200 triệu, hoàn vốn 3,7 năm (1,3 năm nếu tính giữ chân học viên) · 7 tác nhân, 5 kho D1–D5, 5 tiến trình mức 0 · 37/37 màn hình, 98 + 24 + 46 ca kiểm thử, 10 lỗi đã sửa.

### 1.5 Câu hỏi vấn đáp có thể gặp
- Vì sao chọn tự xây dựng mà không thuê phần mềm có sẵn? (Không có đúng quy tắc ưu đãi cộng dồn, chia đợt; đáp ứng 65–70% < 80%; điểm 4,05.)
- Hệ thống thuộc loại nào trong phân loại HTTT? Vì sao có yếu tố MIS? (Báo cáo định kỳ, so sánh kỳ trước – R5.3a, R5.4a.)
- DFD mức 0 khác ngữ cảnh ở đâu? Thế nào là cân bằng? Lấy một tác nhân để minh họa.
- Vì sao tác nhân không được nối thẳng vào kho?
- Nhóm làm sao để sáu người làm sáu phần mà vẫn khớp? (Một bộ mã thống nhất; script kiểm tra tự động; pull request có duyệt.)

---

## 2. Nguyễn Văn Luận – DFD 4.0, 5.0, cân bằng DFD, thực thể, ERD, chuẩn hóa, phân hệ 4.0

### 2.1 Phần phụ trách
- DFD mức 1 của 4.0 Quản lý kết quả học tập và 5.0 Quản lý hệ thống và báo cáo; trả lời chung về **cân bằng DFD** cả năm sơ đồ mức 1.
- 3.1 Thực thể và thuộc tính: 26 thực thể, loại thực thể, loại thuộc tính, thuộc tính thứ sinh.
- 3.2 Quan hệ và ERD: 29 quan hệ (bậc, kiểu), cách biểu diễn 1–1, 1–N, N–N; hai ERD.
- 3.3 Chuẩn hóa 1NF → 2NF → 3NF trên 5 chứng từ, trộn bảng, đối chiếu với thiết kế.
- Đặc tả xử lý phân hệ 4.0 (điểm danh, tổng kết, xếp loại) và hai màn hình demo đã làm: R4.1 Chuyên cần, R4.3b Phiếu báo kết quả.

### 2.2 Lý thuyết cần nắm
- **DFD phân cấp**: mức 1 thay tiến trình cha bằng các tiến trình con, **bảo toàn tác nhân, kho, luồng** của mức trên; dừng ở DFD cơ bản. Bốn loại DFD (vật lý / luận lý, hiện tại / mới); hạn chế của DFD (không có thời gian, không định lượng).
- **Thực thể**: xác thực, chức năng, sự kiện, quan hệ. **Thuộc tính**: định danh (#), mô tả, quan hệ (khóa ngoại), lặp (R), thứ sinh (S). Điều kiện bảng thực thể: tên duy nhất, giá trị nguyên tố, dòng duy nhất, khóa không rỗng (bài giảng mục 5.2.1).
- **Quan hệ**: bậc 1, 2, ≥3 (đưa về bậc 2 bằng thực thể trung gian); kiểu 1–1, 1–N, N–N. Khi thiết kế tệp: 1–N đặt khóa đầu "1" vào tệp đầu "N"; **N–N tạo tệp thứ ba** có khóa là tổ hợp hai khóa. Ba bước dựng ERD: bảng thực thể → quan hệ → vẽ (bài giảng mục 5.2.2).
- **Nhất quán DFD – ERD**: dữ liệu đọc, ghi ở các kho phải có trong ERD.
- **Chuẩn hóa** (bài giảng mục 5.4): trước hết bỏ thuộc tính thứ sinh, ít ý nghĩa. 1NF: tách nhóm lặp; 2NF: bỏ phụ thuộc một phần khóa; 3NF: bỏ phụ thuộc bắc cầu; cuối cùng trộn các bảng cùng khóa. Dị thường khi không chuẩn hóa: thêm, sửa, xóa.
- **Cây quyết định, bảng quyết định** cho xử lý 4.3 (xếp loại, xét đạt).

### 2.3 Đọc ở đâu

| Nội dung | Tệp | Mục |
|---|---|---|
| Lý thuyết tóm tắt | `ly_thuyet/ly_thuyet_ap_dung.md` | 2.3 DFD, 3.1 – 3.3 |
| Hình DFD mức 1 của 4.0, 5.0 | `so_do/DFD_muc_1_4.png`, `so_do/DFD_muc_1_5.png` | – |
| Bảng cân bằng mức 0 ↔ mức 1 | `so_do/can_bang_dfd.md` | "DFD-4.0", "DFD-5.0" (các sơ đồ khác: DFD-1.0 … 3.0) |
| Kiểm tra quy tắc và cân bằng tự động | `so_do/src/dfd.py` (chạy `python so_do/src/dfd.py` → "đạt") | – |
| Loại thực thể, ký hiệu thuộc tính | `thiet_ke/01_thuc_the.md` | 1.1, 1.2 |
| Từ 20 đối tượng của Bài tập 1 → thực thể; đối tượng bị loại | `thiet_ke/01_thuc_the.md` | 2, 2.1 |
| Bảng thực thể chi tiết theo kho D1–D5 | `thiet_ke/01_thuc_the.md` | 4.1 – 4.5 |
| Thuộc tính thứ sinh, nguyên tắc "chốt" | `thiet_ke/01_thuc_the.md` | 5 |
| Kiểm tra: đối chiếu chứng từ, nhất quán DFD – thực thể | `thiet_ke/01_thuc_the.md` | 6.1 – 6.4 |
| Danh sách tổng hợp thực thể (mẫu Bảng 4.5) | `thiet_ke/01_thuc_the.md` | 8, 9 |
| Lý thuyết quan hệ, bảng 29 quan hệ, thống kê | `thiet_ke/02_quan_he.md` | 1, 2, 2.1 |
| Trường hợp đặc biệt (Điểm danh bậc 3 → bậc 2, Học qua Đăng ký) | `thiet_ke/02_quan_he.md` | 2.2 |
| ERD và kiểm tra kho ↔ thực thể | `thiet_ke/02_quan_he.md`; hình `so_do/ERD_1_nghiep_vu.png`, `so_do/ERD_2_he_thong.png`; `so_do/src/erd.py` | 3, 4, 4.1 |
| Lý thuyết chuẩn hóa, dị thường | `thiet_ke/03_chuan_hoa.md` | 1, 2 |
| Chuẩn hóa phiếu đăng ký, phiếu thu, bảng điểm (3 bước mỗi chứng từ) | `thiet_ke/03_chuan_hoa.md` | 3, 4, 5 |
| Sổ điểm danh, giấy chứng nhận (rút gọn) | `thiet_ke/03_chuan_hoa.md` | 6 |
| Trộn bảng, đối chiếu với thiết kế (22/26 thực thể, 86/164 thuộc tính) | `thiet_ke/03_chuan_hoa.md`; chạy `python thiet_ke/kiem_tra_chuan_hoa.py` | 7, 8, 8.1 |
| Đặc tả 4.1 điểm danh, 4.3 tổng kết; bảng quyết định 4.3 | `phan_tich/04_dac_ta.md` | 1.6, 1.7, 2.2, 3.2 |
| Quy tắc QT11 – QT15 (chuyên cần, điểm tổng kết, xếp loại, đạt) | `phan_tich/02_thu_thap.md` | 9.3 |
| Demo phân hệ 4.0 | `demo/TrungTamNgoaiNgu/Pages/HocTap/` – `ChuyenCan` (R4.1), `PhieuBaoKetQua` (R4.3b), `DiemDanh` (F4.1), `NhapDiem` (F4.2), `TongKet` (F4.3, R4.3a), `ChungNhan` (R4.4) | ca TC39, TC40 ở `phan_tich/05_cai_dat.md` 4.2 |

### 2.4 Số liệu phải nhớ
26 thực thể (1 xác thực, 9 chức năng, 10 sự kiện, 6 quan hệ), 164 thuộc tính · 29 quan hệ: 1 bậc 1, 28 bậc 2; 16 kiểu 1–N, 8 kiểu 1–1, 5 kiểu N–N · chuẩn hóa 5 chứng từ tìm lại 22/26 thực thể, 86/164 thuộc tính, không thừa không thiếu bảng · điểm tổng kết = 10% chuyên cần + 30% giữa kỳ + 60% cuối kỳ; đạt khi ≥ 5,0 và chuyên cần ≥ 70%; vắng > 20% thì cảnh báo.

### 2.5 Câu hỏi vấn đáp có thể gặp
- Cân bằng DFD là gì? Chỉ ra một luồng ở DFD-0 và các luồng tương ứng ở DFD-4.0.
- Vì sao "Đăng ký học" là thực thể riêng? Quan hệ N–N giữa Học viên và Lớp học biểu diễn thế nào?
- Điểm danh vốn là quan hệ bậc mấy? Nhóm đưa về bậc 2 ra sao?
- Lấy phiếu thu (hoặc phiếu đăng ký) giải thích từng bước 1NF, 2NF, 3NF. Phụ thuộc bắc cầu nào đã bị tách?
- Vì sao TaiKhoan, ThongBao, NhatKy, ThamSo không tìm ra được bằng chuẩn hóa chứng từ?
- Thuộc tính thứ sinh (tỷ lệ chuyên cần, điểm tổng kết) lưu hay tính? Vì sao "chốt" kết quả khi duyệt?

---

## 3. Nguyễn Gia Ân – thu thập thông tin, BFD, DFD 1.0, 2.0, phân hệ 1.0, 2.0

### 3.1 Phần phụ trách
- 2.1 Thu thập thông tin: nội dung cần thu thập, so sánh 6 phương pháp, kế hoạch; nghiên cứu tài liệu và 5 chứng từ mẫu; quan sát, phỏng vấn, phiếu điều tra, JAD; tổng hợp 8 vấn đề, yêu cầu, 16 quy tắc QT.
- 2.2 Phân tích chức năng: khảo sát, mô tả 5 chức năng cấp 1 và 20 chức năng lá; BFD và nguyên tắc phân rã; ma trận thực thể – chức năng.
- DFD mức 1 của 1.0 Quản lý danh mục và hồ sơ, 2.0 Quản lý lớp học và lịch học.
- Đặc tả xử lý 2.2 đăng ký, 2.3 xếp lịch, cây quyết định 2.4 chuyển lớp, bảo lưu, nghỉ học; demo R2.3a, R2.3b đã làm.

### 3.2 Lý thuyết cần nắm
- **Thu thập thông tin** (Ch.3 mục 2.1): nội dung (môi trường và các thành phần của HTTT hiện tại); 6 phương pháp: nghiên cứu tài liệu (mẫu "Báo cáo kết quả nghiên cứu tài liệu hệ thống"), quan sát, phỏng vấn (cá nhân, nhóm; câu hỏi đóng, mở), phiếu điều tra (tiêu đề – định danh – câu hỏi – kết thúc), JAD, làm mẫu; ưu, nhược điểm từng phương pháp.
- **BFD**: 3 bước (khảo sát chức năng → mô tả bằng văn bản → vẽ sơ đồ cây); nguyên tắc phân rã **thực chất** và **đầy đủ**; chức năng cùng cấp có độ phức tạp tương đương; tên **động từ + bổ ngữ**.
- **Ma trận thực thể – chức năng**: chức năng nào tạo (C), sửa (U), đọc (R) thực thể nào.
- **DFD mức 1** và cân bằng (như mục 2.2 phần của Luận).
- **Ngôn ngữ có cấu trúc giản lược, cây quyết định** để đặc tả xử lý mức cơ sở.

### 3.3 Đọc ở đâu

| Nội dung | Tệp | Mục |
|---|---|---|
| Lý thuyết tóm tắt | `ly_thuyet/ly_thuyet_ap_dung.md` | 2.1, 2.2, 2.3 |
| Mục tiêu, nội dung cần thu thập | `phan_tich/02_thu_thap.md` | 1, 2.1, 2.2 |
| So sánh 6 phương pháp, kế hoạch | `phan_tich/02_thu_thap.md` | 3.1, 3.2 |
| Báo cáo nghiên cứu tài liệu, 5 chứng từ mẫu (4.2.1 – 4.2.5) | `phan_tich/02_thu_thap.md` | 4.1, 4.2 |
| Quan sát, phỏng vấn (kế hoạch, bộ câu hỏi, kết quả), phỏng vấn nhóm | `phan_tich/02_thu_thap.md` | 5, 6.1 – 6.4 |
| Phiếu điều tra (71%, 85%, 93% – số liệu giả định), JAD học phí | `phan_tich/02_thu_thap.md` | 7, 8 |
| 8 vấn đề V1–V8, yêu cầu, 16 quy tắc QT01–QT16, độ phủ | `phan_tich/02_thu_thap.md` | 9.1 – 9.4 |
| Khảo sát, mô tả 5 chức năng cấp 1 | `phan_tich/03_chuc_nang.md` | 1, 2 |
| BFD và kiểm tra nguyên tắc phân rã | `phan_tich/03_chuc_nang.md` 3; hình `so_do/BFD_v3.png`; `so_do/src/bfd.py` | 3 |
| Ma trận thực thể – chức năng và kiểm tra | `phan_tich/03_chuc_nang.md` | 4 |
| Hình DFD mức 1 của 1.0, 2.0; bảng cân bằng | `so_do/DFD_muc_1_1.png`, `so_do/DFD_muc_1_2.png`; `so_do/can_bang_dfd.md` | "DFD-1.0", "DFD-2.0" |
| Đặc tả cập nhật danh mục, 2.2 đăng ký, 2.3 xếp lịch | `phan_tich/04_dac_ta.md` | 1.1, 1.2, 1.3 |
| Cây quyết định 2.4 (QT08, QT09, QT10) | `phan_tich/04_dac_ta.md` | 2.1 |
| Demo phân hệ 1.0 | `demo/TrungTamNgoaiNgu/Pages/DanhMuc/` – `HocVien` (F1.1), `GiaoVien` (F1.2), `KhoaHoc` (F1.3), `PhongHoc` (F1.4) | – |
| Demo phân hệ 2.0 | `demo/TrungTamNgoaiNgu/Pages/LopHoc/` – `MoLop` (F2.1), `DangKy` (F2.2), `KetQuaDangKy` (R2.2), `XepLich` (F2.3), `XuLyDon` (F2.4), `LichHoc` (R2.3a, R2.3b) | TC35, TC36 ở `phan_tich/05_cai_dat.md` 4.2 |

### 3.4 Số liệu phải nhớ
6 phương pháp, 5 chứng từ, 16 quy tắc, 8 vấn đề · phiếu điều tra 48/60 phiếu (giả định): 71% biết lịch qua giáo viên/Zalo, 85% muốn tra cứu học phí, 93% phụ huynh muốn được báo khi con vắng · BFD 5 chức năng cấp 1, 20 chức năng lá · QT06 lớp mở khi ≥ 8, tối đa 20 học viên; QT07 không trùng giáo viên hoặc phòng; QT08 chuyển lớp trong 3 buổi đầu; QT09 bảo lưu khi đã đóng đủ và học < 50%.

### 3.5 Câu hỏi vấn đáp có thể gặp
- So sánh phỏng vấn và phiếu điều tra; khi nào dùng JAD? Nhóm dùng JAD cho việc gì?
- Nguyên tắc "thực chất" và "đầy đủ" khi phân rã BFD là gì? Lấy 3.0 làm ví dụ.
- Ma trận thực thể – chức năng dùng để làm gì?
- Vẽ lại bằng lời DFD-2.0: tiến trình con, kho nào được đọc, ghi.
- Giải thích cây quyết định chuyển lớp, bảo lưu, nghỉ học.

---

## 4. Vũ Văn Hùng – DFD 3.0, bảng quyết định học phí, CSDL vật lý, phân hệ 3.0

### 4.1 Phần phụ trách
- DFD mức 1 của 3.0 Quản lý học phí.
- Đặc tả 3.1 tính học phí bằng **bảng quyết định R1–R8**; 3.2 lập phiếu thu, 3.3 theo dõi công nợ bằng ngôn ngữ có cấu trúc.
- 3.4 CSDL vật lý: chuyển ERD thành tệp, kiểu dữ liệu, ràng buộc toàn vẹn đặt ở đâu, chỉ mục, view và hàm, dữ liệu mẫu, kết quả chạy trên SQL Server.
- Nâng cấp CSDL và phiên bản CSDL (bảo trì); demo phân hệ 3.0, đặc biệt R3.1 đã làm.

### 4.2 Lý thuyết cần nắm
- **Bảng quyết định**: phần điều kiện – các quy tắc (tổ hợp điều kiện) – phần hành động; n điều kiện hai giá trị cho 2ⁿ quy tắc; dùng khi xử lý có nhiều tổ hợp điều kiện.
- **Ngôn ngữ có cấu trúc giản lược**: tuần tự, rẽ nhánh (NẾU … THÌ … NGƯỢC LẠI), lặp.
- **Thiết kế tệp từ ERD**: mỗi thực thể một tệp; quan hệ 1–N đặt khóa ngoại ở đầu N; N–N thành tệp thứ ba; tên tệp, trường không dấu (bài giảng Ch.4).
- **Ràng buộc toàn vẹn**: khóa chính, khóa ngoại, miền giá trị (CHECK), ràng buộc nghiệp vụ ở tầng ứng dụng; thuộc tính thứ sinh tính bằng view, hàm.
- **Bảo trì thích nghi**: thay đổi chính sách bằng dữ liệu (bảng tham số) thay vì sửa mã; thay đổi cấu trúc CSDL bằng tệp nâng cấp có đánh số.

### 4.3 Đọc ở đâu

| Nội dung | Tệp | Mục |
|---|---|---|
| Hình DFD mức 1 của 3.0; bảng cân bằng | `so_do/DFD_muc_1_3.png`; `so_do/can_bang_dfd.md` | "DFD-3.0" |
| Quy tắc học phí QT01 – QT05, QT10, QT16 | `phan_tich/02_thu_thap.md` | 9.3; JAD ở 8 |
| Đặc tả 3.2 lập phiếu thu, 3.3 công nợ | `phan_tich/04_dac_ta.md` | 1.4, 1.5 |
| Bảng quyết định 3.1 tính học phí R1 – R8 | `phan_tich/04_dac_ta.md` | 3.1 |
| Nội dung slide 8, 10 đã chuẩn bị | `bao_cao/noi_dung_slide_vu_van_hung.md` | – |
| Hệ quản trị, quy ước, kiểu dữ liệu | `thiet_ke/04_csdl.md` | 1, 1.1 |
| Chuyển ERD thành tệp, ràng buộc toàn vẹn đặt ở đâu | `thiet_ke/04_csdl.md` | 2, 3 |
| Mô tả từng tệp, chỉ mục, view và hàm | `thiet_ke/04_csdl.md`; chi tiết từng bảng `csdl/mo_ta_bang.md` | 4, 5, 6 |
| Dữ liệu mẫu (1.873 dòng), kết quả chạy SQL Server | `thiet_ke/04_csdl.md` | 7, 8 |
| Mã SQL | `csdl/schema.sql`, `csdl/views.sql`, `csdl/seed.sql` (sinh bằng `csdl/sinh_du_lieu.py`), `csdl/kiem_tra.sql` (24 kiểm tra), chạy `csdl/chay.ps1` | – |
| Nâng cấp CSDL, phiên bản | `csdl/nang_cap/001_tai_khoan_b2_b4.sql`, `csdl/nang_cap/002_phien_ban_csdl.sql`; `phan_tich/06_bao_tri.md` 3.3; `demo/KIEM_THU_HUNG.md` | – |
| Mã tính học phí trong demo | `demo/TrungTamNgoaiNgu/NghiepVu/QuyTac.cs`; test `demo/KiemThu/KiemThuQuyTac.cs`, `demo/KiemThu/KiemThuPhanViecHung.cs` | – |
| Demo phân hệ 3.0 | `demo/TrungTamNgoaiNgu/Pages/HocPhi/` – `KhoanPhaiThu` (F3.1), `HocPhiPhaiThu` (R3.1), `PhieuThu` (F3.2), `InPhieuThu` và `DanhSachPhieuThu` (R3.2), `CongNo` (R3.3); `Pages/BaoCao/DoanhThu` (R3.4) | TC37, TC38 |

### 4.4 Số liệu phải nhớ
Giảm học viên cũ 10%, nhóm ≥ 3 người 5%, đóng một lần 5%, tối đa 15% (QT04); chia 2 đợt 50% – 50% (QT05) · DK2026-0158 thuộc R4: 7.200.000 → 6.480.000 đ · 26 bảng, 164 trường, 35 khóa ngoại, 63 CHECK, 16 chỉ mục, 9 view, 2 hàm · 1.873 dòng mẫu · 24/24 kiểm tra · ThamSo 15 dòng (14 tham số + phiên bản CSDL).

### 4.5 Câu hỏi vấn đáp có thể gặp
- Bảng quyết định học phí có mấy điều kiện, mấy quy tắc? Vì sao R1 chỉ giảm 15% chứ không 20%?
- Vì sao con số chính sách (15%, sĩ số 20…) không viết vào CHECK hay vào mã?
- Quan hệ N–N nào trong ERD thành bảng riêng trong CSDL? Khóa của bảng đó là gì?
- View dùng để làm gì trong thiết kế của nhóm? Lấy `v_ChuyenCan` hoặc công nợ làm ví dụ.
- Khi đã có dữ liệu thật, muốn thêm một cột thì làm thế nào? (Tệp nâng cấp `nang_cap/NNN_…sql`, sao lưu trước, cập nhật PHIEN_BAN_CSDL.)

---

## 5. Trần Thu Thủy – từ điển dữ liệu, giao diện, bộ mã thống nhất, kiểm thử, báo cáo 5.3, 5.4

### 5.1 Phần phụ trách
- Từ điển dữ liệu: kho, luồng giữa tác nhân và hệ thống, luồng nội bộ, luồng xử lý ↔ kho, phần tử dữ liệu tính toán.
- 3.6 Thiết kế giao diện: 19 form ứng với luồng vào, 18 báo cáo ứng với luồng ra, thực đơn phân cấp, quy ước chung cho mọi màn hình, trợ giúp và 33 thông báo lỗi.
- Một bộ mã thống nhất và sinh mã tự động (`sinh_ma.py`).
- Kiểm thử: các mức kiểm thử, 46 ca theo tiến trình, lỗi phát hiện và cách sửa.
- Demo báo cáo R5.3a (tuyển sinh), R5.4a, R5.4b (học tập, giảng dạy) đã làm.

### 5.2 Lý thuyết cần nắm
- **Từ điển dữ liệu**: mỗi mục ghi Tên gọi – Ý nghĩa – Cấu trúc dữ liệu – Nguồn gốc; ký hiệu cấu trúc (+, [a | b], {lặp}, (tùy chọn)).
- **Thiết kế giao diện** (Ch.4 mục 3.5): yêu cầu dễ dùng, nhanh, chính xác, dễ kiểm soát, dễ phát triển. **Form = luồng dữ liệu vào** của DFD, **report = luồng ra**. Tài liệu nội bộ, bên ngoài, xoay vòng; xử lý trực tuyến và theo lô. Kiểu giao diện: đối thoại, thực đơn, biểu tượng, **điền mẫu**. Trợ giúp: sẵn sàng, nhất quán, chính xác – đầy đủ, linh hoạt, tin cậy (báo lỗi kèm gợi ý sửa).
- **Kiểm thử** (Ch.4 bước 5 thiết kế phần mềm, Ch.5): đơn vị, tích hợp, chấp nhận (nhập lại chứng từ mẫu); mỗi ca trỏ về một quy tắc hoặc chứng từ.
- **MIS**: báo cáo định kỳ, so sánh kỳ này với kỳ trước (R5.3a, R5.4a).

### 5.3 Đọc ở đâu

| Nội dung | Tệp | Mục |
|---|---|---|
| Lý thuyết tóm tắt | `ly_thuyet/ly_thuyet_ap_dung.md` | 2.3 (từ điển dữ liệu), 3.5 |
| Từ điển dữ liệu: kho, luồng, phần tử tính toán | `phan_tich/04_dac_ta.md` | 4.1 – 4.5 |
| Yêu cầu, người dùng và cách tương tác | `thiet_ke/06_giao_dien.md` | 1, 2 |
| 19 form (luồng vào), 18 báo cáo (luồng ra) | `thiet_ke/06_giao_dien.md` | 3, 4 |
| Thực đơn, quy ước chung, phác thảo màn hình | `thiet_ke/06_giao_dien.md`; hình `so_do/Thuc_don.png`, `so_do/Mau_F2_2.png`, `Mau_F3_2.png`, `Mau_F4_1.png`, `Mau_R4_3.png` | 5.1 – 5.3 |
| Trợ giúp và 33 thông báo lỗi | `thiet_ke/06_giao_dien.md` | 6 |
| Kiểm tra giao diện tự động (39 luồng, 106 trường) | `thiet_ke/06_giao_dien.md` 7; chạy `python so_do/src/giao_dien.py` | 7 |
| Sinh mã từ tài liệu thiết kế và kiểm tra ngược 131 tệp | `demo/sinh_ma.py` → `demo/TrungTamNgoaiNgu/Nen/ThucDon.g.cs`, `PhanQuyen.g.cs`, `ThongBaoLoi.g.cs` | – |
| Các mức kiểm thử (đơn vị, tích hợp, đồng bộ, phân quyền, độ bền, chấp nhận) | `phan_tich/05_cai_dat.md` | 4.1 |
| 46 ca theo tiến trình TC01 – TC43 | `phan_tich/05_cai_dat.md` | 4.2 |
| 10 lỗi phát hiện và cách sửa | `phan_tich/05_cai_dat.md` | 4.3 |
| Test đơn vị | `demo/KiemThu/` (chạy `dotnet test demo/KiemThu`, 98 ca) | – |
| Demo báo cáo | `demo/TrungTamNgoaiNgu/Pages/BaoCao/` – `TuyenSinh` (F5.3, R5.3a), `TinhTrangLop` (R5.3b), `HocTap` (R5.4a, R5.4b) | TC32, TC41 |

### 5.4 Số liệu phải nhớ
19 form, 106 trường nhập, 18 báo cáo, 37 màn hình, 33 thông báo lỗi · 98/98 test đơn vị, 24/24 kiểm tra CSDL, 46/46 ca theo tiến trình, 248 lượt kiểm tra phân quyền, hơn 2.600 lượt thử độ bền, 10 lỗi đã sửa · R5.3a năm 2026: 46 học viên mới (GT 14, IEK 13, IES 9, TOE 10).

### 5.5 Câu hỏi vấn đáp có thể gặp
- Vì sao nói form là luồng vào, report là luồng ra của DFD? Chỉ một form và luồng DFD tương ứng.
- Một thông báo lỗi tốt cần gì? Lấy ví dụ CK_Diem_Diem hoặc QT05.
- Từ điển dữ liệu khác bảng mô tả CSDL ở chỗ nào?
- `sinh_ma.py` làm gì? Nếu ai đó dùng một mã màn hình không có trong thiết kế thì sao?
- Kể một lỗi chỉ lộ ra khi chạy với dữ liệu thật và cách nhóm sửa.

---

## 6. Trần Minh Quân – thiết kế phần mềm, phân quyền, cài đặt, chuyển đổi, bảo trì

### 6.1 Phần phụ trách
- 3.5 Thiết kế phần mềm: mục đích và yêu cầu, giải thuật Top-down, 34 module, module và dữ liệu, ma trận phân quyền 8 vai trò, chọn ngôn ngữ và kiến trúc.
- Kiến trúc bản demo: phân quyền, thực đơn, nhật ký, sao lưu, lưu nháp; script kiểm tra phân quyền.
- GĐ4: kế hoạch cài đặt, nội dung chuyển đổi (phần cứng – phần mềm, quy trình, huấn luyện 3 mức, dữ liệu), phương pháp chuyển đổi theo giai đoạn, hỗ trợ sử dụng.
- GĐ5: các loại bảo trì, quy trình xử lý yêu cầu, danh sách bảo trì đã biết, 7 hướng phát triển.

### 6.2 Lý thuyết cần nắm
- **Thiết kế phần mềm 6 bước**: mục đích – yêu cầu → thiết kế giải thuật → chọn ngôn ngữ → viết chương trình → thử nghiệm → tài liệu hướng dẫn. **Top-down** (module chính → module con) và Bottom-up; số module ≥ số tiến trình DFD (thêm module nền: đăng nhập, phân quyền, nhật ký, sao lưu…).
- **Kế hoạch cài đặt**: cài phần mềm, thiết lập cấu hình, phân quyền người dùng, hồ sơ cấu hình.
- **Nội dung chuyển đổi**: phần cứng – phần mềm; quy trình, biểu mẫu; con người (huấn luyện: nhận thức máy tính → nhận thức hệ thống → kỹ xảo); dữ liệu.
- **4 phương pháp chuyển đổi**: trực tiếp, song song, theo giai đoạn, thăm dò (pilot) – ưu, nhược, khi nào dùng.
- **Bảo trì**: hiệu chỉnh, thích nghi, phòng ngừa, cải tiến; ưu tiên sửa lỗi > thích nghi > cải tiến; tài liệu hệ thống, quản lý cấu hình.

### 6.3 Đọc ở đâu

| Nội dung | Tệp | Mục |
|---|---|---|
| Lý thuyết tóm tắt | `ly_thuyet/ly_thuyet_ap_dung.md` | 3.4, 4 |
| Mục đích, yêu cầu; Top-down; 34 = 1 + 6 + 27 module | `thiet_ke/05_module.md` | 1, 2 |
| Danh sách module (27 module mức 2: 20 chức năng + N1–N7) | `thiet_ke/05_module.md`; hình `so_do/So_do_module.png`; `so_do/src/module.py` | 3 |
| Module và dữ liệu, liên kết giữa module | `thiet_ke/05_module.md` | 4, 4.1 |
| Ma trận phân quyền 22 module × 8 vai trò (T, S, D, I, X, x) | `thiet_ke/05_module.md` | 5 |
| Kiểm tra tự động, kiến trúc, kế hoạch bước 4–6 | `thiet_ke/05_module.md` | 6, 7, 8 |
| Cài phần mềm, cấu hình, phân quyền người dùng, hồ sơ cấu hình | `phan_tich/05_cai_dat.md` | 1.1 – 1.4 |
| Nội dung chuyển đổi, huấn luyện 3 mức, chuyển dữ liệu | `phan_tich/05_cai_dat.md` | 2.1 – 2.4 |
| Phương pháp chuyển đổi theo giai đoạn (4 giai đoạn, điều kiện chuyển) | `phan_tich/05_cai_dat.md` | 3 |
| Hỗ trợ sử dụng và tài liệu | `phan_tich/05_cai_dat.md` | 5 |
| Các loại bảo trì, quy trình, mức độ, ví dụ một vòng hiệu chỉnh | `phan_tich/06_bao_tri.md` | 1, 2, 2.1, 2.2 |
| Bảo trì đã biết (.NET 8 hết hỗ trợ 10/11/2026), hướng phát triển | `phan_tich/06_bao_tri.md` | 4, 5 |
| Mã phân quyền, thực đơn | `demo/TrungTamNgoaiNgu/Nen/PhanQuyen.cs` (thuộc tính `[Quyen]`, bộ lọc `KiemTraQuyen`), `Nen/ThucDon.cs`, `Program.cs` | – |
| Màn hình hệ thống | `demo/TrungTamNgoaiNgu/Pages/HeThong/` – `TaiKhoan` (F5.1a), `ThamSo` (F5.1b), `NhatKy` (R5.1), `ThongBao` (R5.2), `SaoLuu` (FN4); `Pages/DangNhap`, `DoiMatKhau` (FN1) | TC30, TC33, TC34 |
| Kiểm tra phân quyền 8 vai trò | `demo/kiem_tra_quyen.py` (khi ứng dụng đang chạy) | TC42 |
| Cách chạy demo | `README.md` | "Chạy demo và kiểm thử" |

### 6.4 Số liệu phải nhớ
34 module (1 + 6 + 27), 22 module có quyền × 8 vai trò · tài khoản mẫu: nv001 Giám đốc, nv002 QL đào tạo, nv003 Quản trị, nv004 Kế toán, nv005 Tuyển sinh, gv023, hv0412, ph0301 (mật khẩu = tên đăng nhập) · chuyển đổi 4 giai đoạn trong 10 tuần, học phí chạy song song tháng đầu · 248 lượt kiểm tra phân quyền · .NET 8 hết hỗ trợ 10/11/2026 → nâng .NET 10 · 7 hướng phát triển (ưu tiên 1: thông báo SMS/Zalo).

### 6.5 Câu hỏi vấn đáp có thể gặp
- Vì sao số module (34) nhiều hơn số tiến trình DFD (25)?
- Đọc ma trận phân quyền: kế toán có quyền gì ở module 3.2? Giáo viên ở 4.1? Chữ "x" thường khác "X" thế nào?
- Vì sao chọn chuyển đổi theo giai đoạn? Vì sao riêng học phí lại chạy song song?
- Ba mức huấn luyện là gì, dành cho ai?
- Phân biệt bảo trì thích nghi và cải tiến bằng ví dụ của dự án.
- Bản demo chặn người không có quyền như thế nào? (Thuộc tính `[Quyen("3.1")]` + bộ lọc kiểm tra theo ma trận; trang chủ chỉ hiện chức năng được phép.)

---

## 7. Phần chung mọi người phải nắm
- Đề tài, 5 giai đoạn, 5 chức năng cấp 1 (1.0 Danh mục và hồ sơ, 2.0 Lớp học và lịch học, 3.0 Học phí, 4.0 Kết quả học tập, 5.0 Hệ thống và báo cáo) và mã của chúng.
- Một bộ mã thống nhất: chức năng 3.1 trên BFD = tiến trình 3.1 trên DFD = module 3.1 = màn hình F3.1, R3.1 = `[Quyen("3.1")]` trong mã.
- Chạy được demo theo `README.md` và đăng nhập đúng vai trò của phần mình để minh họa khi được hỏi.
- Câu hỏi không thuộc phần mình: chuyển cho người phụ trách theo bảng tổng quan ở đầu tài liệu.
