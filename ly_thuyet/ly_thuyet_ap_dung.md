# Lý thuyết áp dụng – Bài giảng "Hệ thống thông tin quản lý" (PTIT, 12/2024)

Nguồn: TS. Lê Thị Ngọc Diệp, TS. Đỗ Thị Lan Anh – https://drive.google.com/file/d/127o_w0aSwXn2bOAb-kvM23xHsfmVaDfk/view
Chỉ ghi phần dùng trực tiếp cho bài tập lớn (Phần B: Phân tích → Thiết kế → Cài đặt).

## 1. Khung chung (Ch.1)

- **HTTTQL**: thu thập – xử lý – lưu trữ – truyền đạt thông tin để hỗ trợ ra quyết định, phối hợp, điều khiển.
- **3 dạng thông tin/quyết định**: chiến lược (lãnh đạo cấp cao), chiến thuật (trưởng phòng), tác nghiệp (nhân viên, tổ trưởng).
- **Chất lượng thông tin**: chính xác, đầy đủ, tin cậy, phù hợp, kịp thời, được bảo vệ.
- **Phân loại**: theo cấp quản lý (ESS / MIS, DSS / TPS, ES, KMS); theo chức năng (tài chính–kế toán, marketing, nhân sự, SXKD – với trường học là *quản lý đào tạo*, văn phòng); theo tích hợp (ERP, SCM, CRM).
- **Vòng đời phát triển (SDLC) – 5 giai đoạn**:
  1. Xác định, lựa chọn, lập kế hoạch (phạm vi; khả thi kinh tế/kỹ thuật/tác nghiệp/pháp lý; lợi ích; chi phí hữu hình & vô hình)
  2. Phân tích hệ thống
  3. Thiết kế hệ thống (CSDL, xử lý, biểu mẫu & báo cáo, giao diện)
  4. Cài đặt và khai thác
  5. Bảo trì (hiệu chỉnh / thích nghi / phòng ngừa)

## 2. Phân tích hệ thống (Ch.3)

**Phương pháp luận**: tiếp cận hệ thống (Top-down), đi từ phân tích chức năng đến mô hình hóa, phân tích có cấu trúc (BFD, DFD, mô hình dữ liệu, ngôn ngữ có cấu trúc).

**Quy trình**: Thu thập thông tin → BFD → DFD → Báo cáo phân tích.

### 2.1 Thu thập thông tin
- Nội dung: môi trường (ngành, tổ chức, cơ cấu, khách hàng, nhân lực, tài chính, phần cứng/phần mềm hiện có) + thành phần HTTT hiện tại (hoạt động, thông tin vào, thông tin ra, CSDL, quá trình xử lý).
- 6 phương pháp: **nghiên cứu tài liệu** (có mẫu "Báo cáo kết quả nghiên cứu tài liệu hệ thống"), **quan sát**, **phỏng vấn** (cá nhân/nhóm; câu hỏi mở/đóng), **phiếu điều tra** (tiêu đề – định danh – câu hỏi – kết thúc), **JAD**, **làm mẫu (prototyping)**. Cần nêu ưu/nhược điểm từng cách.

### 2.2 Sơ đồ chức năng kinh doanh (BFD)
- 3 bước: khảo sát chức năng (tên, mô tả, đầu vào, đầu ra) → mô tả bằng văn bản → vẽ sơ đồ hình cây.
- Nguyên tắc phân rã: **thực chất** (chức năng con thực sự tham gia chức năng cha) và **đầy đủ** (các con cộng lại thực hiện trọn chức năng cha).
- Chức năng cùng cấp có độ phức tạp tương đương; tên = **động từ + bổ ngữ**; ký pháp: hình chữ nhật + đường gấp khúc hình cây.

### 2.3 Sơ đồ luồng dữ liệu (DFD) – ký pháp Gane & Sarson
| Ký hiệu | Đặt tên | Ghi chú |
|---|---|---|
| Chức năng (process) | động từ + bổ ngữ, trùng tên BFD | có số định danh 1.0, 1.1, 1.2.3… |
| Kho dữ liệu (data store) | danh từ | D1, D2… |
| Tác nhân ngoài | danh từ | nguồn/đích ngoài hệ thống |
| Luồng dữ liệu | danh từ ("Hóa đơn đã kiểm tra") | luồng vào/ra kho có thể không nhãn |

**Quy tắc vẽ**:
- Chức năng và kho phải có cả luồng vào và luồng ra; luồng ra của chức năng phải khác luồng vào.
- **Tác nhân ngoài ↔ kho, tác nhân ↔ tác nhân, kho ↔ kho không nối trực tiếp** – phải qua một chức năng.
- Không có luồng từ chức năng vào chính nó; nhánh rẽ/gộp phải cùng nội dung.
- Trao đổi hai chiều → vẽ 2 mũi tên ngược chiều; được vẽ lặp tác nhân/kho cho dễ đọc.

**Hệ thống DFD phân cấp** (bám theo BFD, mỗi chức năng BFD = một xử lý DFD):
- **Ngữ cảnh**: 1 xử lý số 0 + tác nhân ngoài + luồng vào/ra; không có kho.
- **Mức 0 (DFD-0)**: các xử lý 1.0, 2.0… ứng với cấp 1 BFD; giữ nguyên tác nhân + luồng ở ngữ cảnh, thêm kho và luồng nội bộ.
- **Mức 1 (DFD-1.0, 2.0…)**: thay xử lý cha bằng các xử lý con; **bảo toàn (cân bằng)** tác nhân, kho, luồng của mức trên.
- Dừng khi tới DFD cơ bản (không phân rã được nữa).
- Mô tả bổ sung cho xử lý mức cơ sở: ngôn ngữ cấu trúc giản lược, cây quyết định, bảng quyết định (điều kiện – quy tắc – hành động), **từ điển dữ liệu** (Tên gọi – Ý nghĩa – Cấu trúc dữ liệu – Nguồn gốc).
- 4 loại DFD: vật lý hiện tại, luận lý hiện tại, luận lý mới, vật lý mới.
- Hạn chế DFD: không có yếu tố thời gian, chỉ thể hiện một phần trật tự, không định lượng.

### 2.4 Báo cáo phân tích
Tiêu đề → Mục lục (phương pháp luận, kết quả thu thập, phân tích chức năng, BFD, dòng thông tin, DFD) → Lời giới thiệu → Nội dung → Kết luận → Phụ lục.

## 3. Thiết kế hệ thống (Ch.4)

**Quy trình**: Mô hình hóa thực thể → ERD + thiết kế CSDL → Chuẩn hóa → Thiết kế phần mềm → Thiết kế giao diện người–máy.

### 3.1 Thực thể – thuộc tính
- Loại thực thể: xác thực, chức năng, sự kiện (hóa đơn, biên lai), quan hệ (hợp đồng, đăng ký).
- Loại thuộc tính: **định danh** (#khóa đơn/khóa kép), **mô tả**, **quan hệ** (khóa ngoại), **lặp** (R), **thứ sinh** (S – tính ra được, ví dụ Thành tiền).
- Bảng thực thể: tên duy nhất, giá trị nguyên tố, dòng duy nhất, khóa không rỗng.

### 3.2 Quan hệ
- **Bậc**: 1 (trong cùng thực thể), 2 (giữa 2 thực thể), ≥3 (luôn đưa được về bậc 2 bằng thực thể trung gian).
- **Kiểu**: 1-1, 1-N, N-N. Ký pháp: hình thoi ghi tên quan hệ (động từ: "Có", "Học", "Thi").
- Cách biểu diễn khi thiết kế tệp:
  - 1-1 bậc 2: đặt khóa của tệp này vào tệp kia (bên nào cũng được).
  - **1-N bậc 2: khóa của đầu "1" thành trường quan hệ ở tệp đầu "N"**.
  - **N-N bậc 2: tạo tệp thứ 3**, khóa = tổ hợp 2 khóa gốc + thuộc tính riêng (vd SVMH: MaSV, MaMH, Lanthi, Diemthi).
  - Bậc 1: 1-1/1-N → 1 tệp có trường quan hệ tự tham chiếu; N-N → thêm tệp quan hệ.
- **3 bước dựng ERD**: xác định thực thể + thuộc tính (lập bảng) → xác định quan hệ (bậc, kiểu) → vẽ ERD.
- Nhất quán DFD–ERD: dữ liệu ghi vào/đọc ra từ các kho phải nằm trong ERD.
- Tên tệp/trường viết **không dấu** hoặc tiếng Anh (thực thể/thuộc tính được viết có dấu).

### 3.3 Chuẩn hóa (1NF → 3NF)
- Trước khi chuẩn hóa: bỏ thuộc tính thứ sinh và thuộc tính không quan trọng.
- **1NF**: không có thuộc tính lặp → tách nhóm lặp thành thực thể mới, khóa = khóa gốc + khóa phù hợp.
- **2NF**: thuộc tính không khóa phụ thuộc **toàn bộ** khóa → tách phần chỉ phụ thuộc một phần khóa.
- **3NF**: không có phụ thuộc **bắc cầu** (X → Y → Z) → tách Z sang thực thể mới khóa Y.
- Sau cùng: trộn các bảng cùng khóa mô tả cùng đối tượng.
- Ví dụ mẫu trong bài giảng: Hóa đơn → Hóa đơn / Khách hàng / Hàng mua / Hàng hóa.

### 3.4 Thiết kế phần mềm
- 6 bước: mục đích–yêu cầu → thiết kế giải thuật → chọn ngôn ngữ → viết chương trình → thử nghiệm → tài liệu hướng dẫn.
- Thiết kế giải thuật: **Top-down** (module chính → module con) hoặc **Bottom-up** (gộp chương trình rời thành phân hệ). Số module ≥ số process DFD (thêm phân quyền, sửa lỗi…).
- Mua phần mềm sẵn: chấp nhận khi đáp ứng ~80% yêu cầu; tiêu chí: chức năng, linh hoạt, dễ dùng, CSDL, công sức triển khai, bảo trì, tài liệu, nhà cung cấp, chi phí.

### 3.5 Thiết kế giao diện
- Yêu cầu: dễ dùng, nhanh, chính xác, dễ kiểm soát, dễ phát triển.
- **Form** = luồng dữ liệu **vào** xử lý trong DFD; **Report** = luồng dữ liệu **ra**. Tài liệu nội bộ / bên ngoài / xoay vòng; xử lý trực tuyến vs theo lô.
- Trợ giúp: sẵn sàng, nhất quán, chính xác–đầy đủ, linh hoạt, tin cậy (báo lỗi + gợi ý sửa).
- Kiểu giao diện: đối thoại, thực đơn (phân cấp), biểu tượng, **điền mẫu** (phổ biến nhất), màn hình nhập liệu/đối thoại/thực đơn.

## 4. Cài đặt (Ch.5)

- **Kế hoạch cài đặt**: cài phần mềm, thiết lập cấu hình, **phân quyền người dùng**, lập hồ sơ cấu hình.
- **Nội dung chuyển đổi**: phần cứng–phần mềm; quy trình nghiệp vụ, công nghệ quản lý, biểu mẫu; con người (huấn luyện: nhận thức máy tính → nhận thức hệ thống → kỹ xảo); **dữ liệu** (danh mục, phân công, khối lượng–chất lượng, lịch, kiểm tra cuối).
- **4 phương pháp chuyển đổi**: trực tiếp (nhanh, rẻ, rủi ro cao) · song song (an toàn, tốn gấp đôi) · theo giai đoạn (từng bộ phận) · thăm dò/pilot (một chi nhánh trước).
- Hỗ trợ sử dụng, cải tiến (ưu tiên: sửa lỗi > thích nghi > cải tiến), tài liệu hệ thống (mô tả + sử dụng), quản lý cấu hình (ghi vết phiên bản, phân quyền).

## 5. Phần C – dùng để định vị hệ thống
- Hệ thống quản lý trung tâm ngoại ngữ chủ yếu là **TPS** (ghi danh, thu học phí, điểm danh, nhập điểm) + **MIS** (báo cáo định kỳ: tuyển sinh, doanh thu, chuyên cần) phục vụ giám đốc; có yếu tố **CRM** (chăm sóc học viên/phụ huynh, giữ chân học viên).
- TPS: thu thập dữ liệu → xử lý & cập nhật CSDL (thời gian thực / theo lô) → lập chứng từ, báo cáo.
- MIS: báo cáo định kỳ / đột xuất, tính so sánh (thực tế vs kế hoạch, kỳ này vs kỳ trước).

## 6. Bài tập mẫu trong bài giảng (để luyện)
- Ch.3: cửa hàng quần áo; công ty điện tử X (BFD, ngữ cảnh, DFD-0); kho siêu thị (BFD, ngữ cảnh, DFD-0, DFD-1).
- Ch.4: công ty xây dựng ABC (ngày công – ERD + tệp); thư viện mượn/trả sách (ERD + tệp).
