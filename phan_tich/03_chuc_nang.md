# 2.2 Phân tích chức năng – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn Phân tích hệ thống (KE_HOACH.md, mục 2.2). Làm theo quy trình xây dựng BFD trong bài giảng (mục 3.3.2.2):
**Bước 1** khảo sát chức năng (tên, mô tả, đầu vào, đầu ra) → **Bước 2** mô tả bằng văn bản → **Bước 3** vẽ sơ đồ (`so_do/BFD_v3.png`, sinh từ `so_do/src/bfd.py`).

Quy ước dùng trong tài liệu:
- Mã và tên chức năng lấy đúng theo `so_do/src/bfd.py`.
- Tên tác nhân và kho D1–D5 lấy đúng theo DFD mức 0 (`so_do/src/dfd.py`).
- Mã quy tắc QTxx lấy từ `phan_tich/02_thu_thap.md`, mục 9.3.
- Cột *Đầu vào / Đầu ra* liệt kê các luồng dữ liệu; đây là nguyên liệu để vẽ DFD mức 1 ở bước 2.3. Luồng nội bộ giữa hai chức năng con được ghi là "từ x.y" hoặc "sang x.y".

---

## 1. Bước 1 – Khảo sát chức năng

### Chức năng 1.0 – Quản lý danh mục và hồ sơ

| Mã | Tên chức năng | Mô tả | Đầu vào | Đầu ra | Người thực hiện | Quy tắc |
|---|---|---|---|---|---|---|
| 1.1 | Tiếp nhận hồ sơ, kiểm tra trình độ học viên | Tư vấn, tổ chức kiểm tra đầu vào và ghi trình độ (CEFR). Tạo mới hoặc cập nhật hồ sơ học viên và phụ huynh; kiểm tra trùng hồ sơ theo số điện thoại và ngày sinh. | Hồ sơ học viên, kết quả kiểm tra đầu vào (NV tuyển sinh); hồ sơ hiện có (D1) | Hồ sơ học viên cập nhật (D1) | NV tuyển sinh / CSHV | – |
| 1.2 | Quản lý hồ sơ giáo viên | Thêm và sửa hồ sơ giáo viên: chuyên môn, ngôn ngữ dạy, bằng cấp, tình trạng làm việc. | Thông tin giáo viên (QL đào tạo) | Hồ sơ giáo viên (D1) | QL đào tạo | – |
| 1.3 | Quản lý khóa học | Thêm và sửa khóa học: ngôn ngữ, trình độ đầu vào → đầu ra, số buổi, thời lượng, học phí, trọng số điểm. | Thông tin khóa học (QL đào tạo) | Khóa học (D1) | QL đào tạo | QT13 (trọng số) |
| 1.4 | Quản lý phòng học | Thêm và sửa phòng học: sức chứa, thiết bị, tình trạng sử dụng. | Thông tin phòng học (QL đào tạo) | Phòng học (D1) | QL đào tạo | QT06 (sức chứa) |

### Chức năng 2.0 – Quản lý lớp học và lịch học

| Mã | Tên chức năng | Mô tả | Đầu vào | Đầu ra | Người thực hiện | Quy tắc |
|---|---|---|---|---|---|---|
| 2.1 | Mở lớp, phân công giáo viên | Tạo lớp theo khóa học, đặt ngày khai giảng, sĩ số tối đa, phân công giáo viên. Chuyển lớp sang trạng thái *Đang học* khi đủ sĩ số tối thiểu, hoặc *Hủy* nếu không đủ. | Kế hoạch mở lớp, phân công giáo viên (QL đào tạo); khóa học, giáo viên (D1); sĩ số đăng ký (D2) | Lớp học (D2); lớp đã mở (sang 2.3) | QL đào tạo | QT06 |
| 2.2 | Đăng ký học, xếp học viên vào lớp | Nhận yêu cầu và phiếu đăng ký, kiểm tra lớp còn chỗ, ghi đăng ký (trạng thái *Chờ đóng phí*). Đăng ký có hiệu lực khi học viên đã đóng đợt 1. | Yêu cầu đăng ký (HV/PH); phiếu đăng ký (NV tuyển sinh); học viên (D1); sĩ số lớp (D2); tình trạng đóng học phí (D3) | Đăng ký (D2); kết quả đăng ký, danh sách lớp (NV tuyển sinh) | NV tuyển sinh / CSHV | QT05, QT06 |
| 2.3 | Xếp lịch, kiểm tra trùng lịch | Sinh các buổi học của lớp theo lịch tuần, gán phòng; từ chối nếu trùng giáo viên hoặc trùng phòng. Xử lý đổi lịch và học bù. | Lớp đã mở (từ 2.1); giáo viên, phòng học (D1); lịch đã xếp (D2) | Buổi học / lịch học (D2); lịch học (HV/PH); lịch dạy, danh sách lớp (Giáo viên) | QL đào tạo | QT07 |
| 2.4 | Xử lý chuyển lớp, bảo lưu, nghỉ học | Kiểm tra điều kiện rồi cập nhật trạng thái đăng ký: *Chuyển lớp* (tạo đăng ký mới ở lớp đích), *Bảo lưu*, *Nghỉ học*. | Yêu cầu chuyển lớp, bảo lưu (HV/PH); đơn chuyển lớp, bảo lưu (NV tuyển sinh); đăng ký, sĩ số, buổi học (D2); tình trạng đóng học phí (D3) | Đăng ký cập nhật (D2); kết quả xử lý (NV tuyển sinh) | NV tuyển sinh / CSHV | QT08, QT09, QT10 |

### Chức năng 3.0 – Quản lý học phí

| Mã | Tên chức năng | Mô tả | Đầu vào | Đầu ra | Người thực hiện | Quy tắc |
|---|---|---|---|---|---|---|
| 3.1 | Tính học phí, áp dụng ưu đãi | Với mỗi đăng ký: lấy học phí của khóa, xét các ưu đãi được hưởng (học viên cũ, đăng ký nhóm, đóng một lần), giới hạn tổng mức giảm, chia đợt và đặt hạn đóng. Kế toán cập nhật danh mục ưu đãi. | Đăng ký học (D2); học phí khóa học (D1); ưu đãi (NV kế toán) | Ưu đãi, khoản học phí phải thu (D3); học phí phải thu (NV kế toán) | NV kế toán (hệ thống tự tính) | QT01–QT05 |
| 3.2 | Lập phiếu thu | Nhận tiền, đối chiếu với khoản phải thu của đợt, lập phiếu thu, cập nhật số đã thu. | Tiền học phí (HV/PH); thông tin thu tiền (NV kế toán); học phí phải thu, số đã thu (D3) | Phiếu thu (D3); phiếu thu (HV/PH, NV kế toán) | NV kế toán | QT05 |
| 3.3 | Theo dõi công nợ | Tính công nợ = phải thu − đã thu cho từng đăng ký còn hiệu lực; lọc các khoản quá hạn. Đăng ký nghỉ học không tính nợ. | Học phí, phiếu thu (D3); trạng thái đăng ký (D2) | Công nợ (NV kế toán) | NV kế toán | QT10, QT16 |
| 3.4 | Thống kê doanh thu | Tổng hợp phiếu thu theo kỳ, khóa học, hình thức thanh toán; kèm tổng công nợ. | Yêu cầu báo cáo doanh thu (Giám đốc); phiếu thu, công nợ (D3) | Báo cáo doanh thu, công nợ (Giám đốc) | NV kế toán, Giám đốc | – |

### Chức năng 4.0 – Quản lý học tập

| Mã | Tên chức năng | Mô tả | Đầu vào | Đầu ra | Người thực hiện | Quy tắc |
|---|---|---|---|---|---|---|
| 4.1 | Điểm danh, tính chuyên cần | Giáo viên chọn buổi học và ghi trạng thái từng học viên. Hệ thống tính tỷ lệ chuyên cần và đánh dấu học viên vượt ngưỡng vắng. | Điểm danh (Giáo viên); danh sách lớp, buổi học (D2) | Điểm danh (D4); chuyên cần (HV/PH) | Giáo viên | QT11, QT12 |
| 4.2 | Nhập điểm thành phần | Giáo viên nhập điểm giữa kỳ, cuối kỳ (thang 10) và nhận xét cho từng học viên. | Điểm số, nhận xét (Giáo viên); danh sách lớp (D2) | Điểm thành phần (D4) | Giáo viên | Điểm trong khoảng 0–10 |
| 4.3 | Tổng kết, xếp loại | Khi lớp kết thúc: tính điểm chuyên cần, điểm tổng kết theo trọng số, xếp loại và xác định Đạt/Không đạt. | Điểm danh, điểm thành phần (D4); trọng số điểm khóa học (D1) | Kết quả học tập (D4); kết quả học tập (HV/PH); danh sách học viên đạt (sang 4.4) | Hệ thống tự tính, QL đào tạo duyệt | QT13, QT14, QT15 |
| 4.4 | Cấp chứng nhận hoàn thành khóa học | Cấp số chứng nhận cho học viên đạt và in chứng nhận. | Danh sách học viên đạt (từ 4.3) | Chứng nhận (D4); chứng nhận (HV/PH) | QL đào tạo | QT15 |

### Chức năng 5.0 – Quản lý hệ thống và báo cáo

| Mã | Tên chức năng | Mô tả | Đầu vào | Đầu ra | Người thực hiện | Quy tắc |
|---|---|---|---|---|---|---|
| 5.1 | Quản lý tài khoản, phân quyền | Tạo và khóa tài khoản cho nhân viên, giáo viên, học viên, phụ huynh; gán vai trò; cấu hình tham số; ghi và xem nhật ký. | Tài khoản, phân quyền, cấu hình (Quản trị viên); hồ sơ học viên, giáo viên (D1); quyền truy cập (D5) | Tài khoản, nhật ký (D5); nhật ký hệ thống (Quản trị viên) | Quản trị viên | Phụ huynh chỉ xem được thông tin của con mình |
| 5.2 | Gửi thông báo | Sinh và gửi thông báo: lịch học thay đổi, nhắc học phí sắp hoặc đã quá hạn, cảnh báo vắng học, kết quả học tập. | Lịch học thay đổi (D2); công nợ (D3); chuyên cần, kết quả (D4); liên hệ học viên, phụ huynh (D1) | Thông báo (D5); thông báo (HV/PH) | Hệ thống tự động, NV CSHV | QT12, QT16 |
| 5.3 | Lập báo cáo tuyển sinh, tình trạng lớp | Số học viên mới theo tháng và khóa học; sĩ số và tỷ lệ lấp đầy từng lớp; số lớp mở và hủy. | Yêu cầu báo cáo tổng hợp (Giám đốc); hồ sơ học viên (D1); lớp, đăng ký (D2) | Báo cáo tuyển sinh (Giám đốc); báo cáo lớp học (QL đào tạo) | QL đào tạo, Giám đốc | – |
| 5.4 | Lập báo cáo chuyên cần, kết quả, giảng dạy | Tỷ lệ chuyên cần theo lớp, danh sách học viên nghỉ nhiều; tỷ lệ đạt và phân bố xếp loại; số buổi dạy của từng giáo viên. | Yêu cầu báo cáo tổng hợp (Giám đốc); giáo viên (D1); lớp, buổi học (D2); chuyên cần, kết quả (D4) | Báo cáo chuyên cần, kết quả, giảng dạy (QL đào tạo); báo cáo kết quả học tập (Giám đốc) | QL đào tạo, Giám đốc | – |

---

## 2. Bước 2 – Mô tả hoạt động và mối quan hệ giữa các chức năng

Viết theo dạng văn bản như ví dụ trong bài giảng (Ví dụ 2 – Ngân hàng X).

"Trung tâm ngoại ngữ có 5 nhiệm vụ chính: **quản lý danh mục và hồ sơ**, **quản lý lớp học và lịch học**, **quản lý học phí**, **quản lý học tập**, và **quản lý hệ thống và báo cáo**.

Khi học viên đến trung tâm, nhân viên tuyển sinh **tiếp nhận hồ sơ và kiểm tra trình độ** để xác định học viên phù hợp với khóa nào. Từ trước đó, quản lý đào tạo đã cập nhật **hồ sơ giáo viên**, **khóa học** và **phòng học** để làm danh mục dùng chung.

Dựa trên danh mục, quản lý đào tạo **mở lớp và phân công giáo viên**. Nhân viên tuyển sinh **đăng ký học và xếp học viên vào lớp** còn chỗ. Lớp đủ sĩ số tối thiểu thì được **xếp lịch**, đồng thời hệ thống **kiểm tra trùng lịch** giáo viên và phòng học; lịch học được gửi cho học viên, lịch dạy được gửi cho giáo viên. Trong quá trình học, các yêu cầu **chuyển lớp, bảo lưu, nghỉ học** được xét theo quy định rồi cập nhật vào đăng ký.

Mỗi đăng ký phát sinh học phí. Kế toán **tính học phí và áp dụng ưu đãi**, sau đó **lập phiếu thu** mỗi lần học viên đóng tiền. Đăng ký chỉ có hiệu lực khi học viên đã đóng đợt 1. Kế toán **theo dõi công nợ** của các khoản chưa đóng hoặc đã quá hạn, và định kỳ **thống kê doanh thu** gửi giám đốc.

Ở mỗi buổi học, giáo viên **điểm danh**, hệ thống tính chuyên cần. Giáo viên **nhập điểm thành phần**. Khi lớp kết thúc, hệ thống **tổng kết và xếp loại**; học viên đạt yêu cầu được **cấp chứng nhận hoàn thành khóa học**.

Xuyên suốt các hoạt động trên, quản trị viên **quản lý tài khoản và phân quyền**. Hệ thống **gửi thông báo** về lịch học, học phí, chuyên cần và kết quả cho học viên và phụ huynh, đồng thời **lập báo cáo tuyển sinh, tình trạng lớp** và **báo cáo chuyên cần, kết quả, giảng dạy** cho quản lý đào tạo và giám đốc."

Các phần chữ in đậm khớp 1–1 với 5 chức năng cấp 1 và 20 chức năng lá trong BFD_v3. Riêng các cụm "kiểm tra trùng lịch" và "điểm danh" là một phần trong tên của chức năng 2.3 và 4.1.

---

## 3. Bước 3 – Sơ đồ BFD và kiểm tra nguyên tắc phân rã

Sơ đồ: `so_do/BFD_v3.png`.

| Chức năng cha | Nguyên tắc *thực chất* (mỗi chức năng con thực sự là một phần của chức năng cha) | Nguyên tắc *đầy đủ* (các chức năng con cộng lại làm trọn chức năng cha) |
|---|---|---|
| 1.0 | Cả 4 chức năng con đều quản lý một loại hồ sơ hoặc danh mục | Bao phủ đủ 4 nhóm đối tượng gốc: học viên (kèm phụ huynh), giáo viên, khóa học, phòng học |
| 2.0 | Cả 4 chức năng con đều thao tác trên lớp, đăng ký hoặc lịch | Đủ vòng đời: mở lớp → ghi danh → xếp lịch → thay đổi đăng ký |
| 3.0 | Cả 4 chức năng con đều thao tác trên khoản thu | Đủ chu trình: tính phí → thu → theo dõi nợ → thống kê |
| 4.0 | Cả 4 chức năng con đều thao tác trên dữ liệu học tập | Đủ chu trình: chuyên cần → điểm → tổng kết → chứng nhận |
| 5.0 | Cả 4 chức năng con đều là chức năng hỗ trợ dùng chung hoặc chức năng tổng hợp | Đủ: quản trị truy cập, truyền thông tin ra ngoài, 2 nhóm báo cáo MIS |

Các chức năng cùng cấp có độ phức tạp tương đương (cấp 1 đều có 4 chức năng con; mỗi chức năng lá là một nghiệp vụ trọn vẹn do một vai trò thực hiện). Tên chức năng đều theo dạng động từ + bổ ngữ.

---

## 4. Ma trận thực thể – chức năng

Mục đích: làm cầu nối từ chức năng sang DFD (luồng vào/ra kho) và sang ERD (bước 3.1–3.2). Danh sách thực thể ở đây là **dự kiến**, lấy từ 20 đối tượng dữ liệu của BT1 và 5 chứng từ ở bước 2.1; tên có thể chỉnh lại khi chuẩn hóa.

**Ký hiệu:** C = tạo, U = sửa, R = đọc.

| Viết tắt | Thực thể | Kho |
|---|---|---|
| HV | Học viên | D1 |
| PH | Phụ huynh | D1 |
| GV | Giáo viên | D1 |
| KH | Khóa học | D1 |
| PHG | Phòng học | D1 |
| LOP | Lớp học | D2 |
| DK | Đăng ký học | D2 |
| BH | Buổi học (lịch học) | D2 |
| UD | Ưu đãi | D3 |
| HP | Học phí phải thu (theo đăng ký, theo đợt) | D3 |
| PT | Phiếu thu | D3 |
| DD | Điểm danh | D4 |
| DIEM | Điểm thành phần | D4 |
| KQ | Kết quả học tập | D4 |
| CN | Chứng nhận | D4 |
| NV | Nhân viên | D5 |
| TK | Tài khoản | D5 |
| TB | Thông báo | D5 |
| NK | Nhật ký hệ thống | D5 |

| Chức năng | HV | PH | GV | KH | PHG | LOP | DK | BH | UD | HP | PT | DD | DIEM | KQ | CN | NV | TK | TB | NK |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1.1 | CRU | CU | | | | | | | | | | | | | | | | | |
| 1.2 | | | CU | | | | | | | | | | | | | | | | |
| 1.3 | | | | CU | | | | | | | | | | | | | | | |
| 1.4 | | | | | CU | | | | | | | | | | | | | | |
| 2.1 | | | R | R | | CU | R | | | | | | | | | | | | |
| 2.2 | R | | | | | R | C | | | R | R | | | | | | | | |
| 2.3 | | | R | | R | R | | CU | | | | | | | | | | | |
| 2.4 | | | | | | R | CU | R | | R | R | | | | | | | | |
| 3.1 | | | | R | | R | R | | CU | C | | | | | | | | | |
| 3.2 | | | | | | | | | | R | C | | | | | | | | |
| 3.3 | | | | | | | R | | | R | R | | | | | | | | |
| 3.4 | | | | | | | | | | R | R | | | | | | | | |
| 4.1 | | | | | | | R | R | | | | CU | | | | | | | |
| 4.2 | | | | | | | R | | | | | | CU | | | | | | |
| 4.3 | | | | R | | | R | | | | | R | R | CU | | | | | |
| 4.4 | | | | | | | | | | | | | | R | C | | | | |
| 5.1 | R | R | R | | | | | | | | | | | | | CU | CU | | CR |
| 5.2 | R | R | | | | R | R | R | | R | R | R | | R | | | | C | |
| 5.3 | R | | | R | | R | R | | | | | | | | | | | | |
| 5.4 | | | R | | | R | R | R | | | | R | | R | | | | | |

### Kiểm tra ma trận

- **Mỗi thực thể đều có chức năng tạo (C):** đạt, cả 19 thực thể.
- **Mỗi chức năng đều đụng tới dữ liệu:** đạt, cả 20 chức năng.
- **Khớp với DFD mức 0:** mọi ô C/U của một chức năng x.y đều nằm ở kho mà tiến trình x.0 có luồng *ghi*, và mọi ô R đều nằm ở kho mà x.0 có luồng *đọc*, hoặc ở kho mà chính x.0 ghi vào (`so_do/can_bang_dfd.md`).
- Những điểm đã sửa trên DFD mức 0 nhờ rà ma trận và vẽ mức 1:
  - Thêm luồng **"Tình trạng đóng học phí" (D3 → 2.0)**: 2.2 và 2.4 cần biết tình trạng đóng học phí theo QT05 và QT09.
  - Thêm luồng **"Trọng số điểm khóa học" (D1 → 4.0)**: 4.3 cần trọng số để tính điểm tổng kết (QT13).
  - Đổi tên một số luồng kho cho đủ nội dung mà mức 1 cần: D2 → 2.0 "Đăng ký, sĩ số, lịch đã xếp"; D3 → 3.0 "Học phí phải thu, số đã thu"; 4.0 → D4 "Điểm danh, điểm, kết quả, chứng nhận"; D5 → 5.0 "Quyền truy cập, nhật ký".
- Mã nhân viên lập phiếu (NV) lấy từ phiên đăng nhập nên không vẽ thành luồng đọc D5 trên DFD.

## 5. Đầu ra cho các bước sau

| Bước | Dùng gì từ tài liệu này |
|---|---|
| 2.3 DFD mức 1 | Cột Đầu vào / Đầu ra ở mục 1 → các luồng của DFD-1.0 … DFD-5.0 (phải cân bằng với `so_do/can_bang_dfd.md`) |
| 2.4 Đặc tả xử lý | Cột Quy tắc → bảng/cây quyết định cho 2.4, 3.1, 4.1, 4.3 |
| 3.1–3.2 Thực thể, ERD | Danh sách thực thể và cột Kho ở mục 4 |
| 3.5 Module, phân quyền | Cột Người thực hiện → ma trận vai trò × chức năng |
