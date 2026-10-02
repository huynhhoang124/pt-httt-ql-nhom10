# 2.1 Thu thập thông tin – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn Phân tích hệ thống (KE_HOACH.md, mục 2.1). Bám theo bài giảng HTTTQL PTIT, mục 3.3.1.

> **Lưu ý phạm vi:** nhóm phân tích một trung tâm ngoại ngữ *giả định* (1 cơ sở, khoảng 500 học viên, 30 giáo viên, như ở Bài tập 1). Vì vậy chứng từ mẫu, biên bản phỏng vấn, kết quả quan sát và kết quả phiếu điều tra dưới đây là **kịch bản giả định** để phục vụ phân tích. Nếu nhóm khảo sát được một trung tâm thật thì thay số liệu vào các bảng tương ứng, giữ nguyên cấu trúc.

---

## 1. Mục tiêu thu thập

Mục tiêu đặt ra cụ thể để biết khi nào thu thập đủ:

| # | Mục tiêu | Dùng cho bước |
|---|---|---|
| M1 | Xác định đủ các nghiệp vụ, người thực hiện, đầu vào và đầu ra của từng nghiệp vụ | BFD (2.2), DFD (2.3) |
| M2 | Thu đủ các chứng từ đang dùng (phiếu đăng ký, phiếu thu, sổ điểm danh, bảng điểm, chứng nhận) cùng các trường dữ liệu trên đó | Từ điển dữ liệu (2.4), chuẩn hóa (3.3) |
| M3 | Ghi lại các quy tắc nghiệp vụ: học phí, ưu đãi, sĩ số, chuyển lớp, bảo lưu, chuyên cần, xếp loại | Bảng/cây quyết định (2.4) |
| M4 | Xác định các báo cáo mà ban giám đốc và các bộ phận cần, kèm tần suất | Tiến trình 3.4, 5.3, 5.4; thiết kế report (3.6) |
| M5 | Xác định khó khăn của cách làm hiện tại và yêu cầu đối với hệ thống mới | Phạm vi, khả thi (GĐ1); yêu cầu phi chức năng |

## 2. Nội dung thông tin cần thu thập

Phân theo hai nhóm như bài giảng.

### 2.1 Môi trường của hệ thống hiện tại

| Nhóm | Thông tin cần thu | Kết quả (giả định) |
|---|---|---|
| Môi trường ngành | Mức độ cạnh tranh, xu hướng | Nhiều trung tâm ngoại ngữ cạnh tranh trong cùng khu vực; học viên và phụ huynh muốn theo dõi lịch học, học phí và kết quả qua điện thoại; chương trình luyện thi IELTS/TOEIC tăng mạnh. |
| Môi trường tổ chức | Lịch sử, mô hình tổ chức, quan hệ giữa các bộ phận, khối lượng công việc, khách hàng, chính sách | Trung tâm hoạt động 5 năm, có 1 cơ sở. Giám đốc quản lý 4 bộ phận: Đào tạo, Tuyển sinh – Chăm sóc học viên, Kế toán, CNTT. Mỗi kỳ khai giảng khoảng 25–30 lớp, mỗi lớp 8–20 học viên. Chính sách năm tới: mở thêm lớp thiếu nhi, giữ chân học viên cũ. |
| Môi trường vật lý | Quy trình xử lý số liệu, độ tin cậy | Hồ sơ giấy lưu trong tủ, lịch lớp ghi trên bảng trắng và Excel, học phí ghi vào sổ thu kèm Excel. Số liệu giữa các bộ phận thường lệch nhau. |
| Môi trường kỹ thuật | Phần cứng, phần mềm, CSDL, nhân sự tin học | Khoảng 12 máy tính văn phòng, 1 máy in hóa đơn, wifi. Phần mềm: Microsoft Excel, Google Sheets, Zalo. Chưa có CSDL tập trung. Có 1 nhân viên CNTT kiêm nhiệm. |

### 2.2 Các thành phần của hệ thống thông tin hiện tại

| Thành phần | Hiện trạng (giả định) |
|---|---|
| Hoạt động | Tư vấn và kiểm tra đầu vào, ghi danh, mở lớp, xếp lịch, thu học phí, điểm danh, nhập điểm, cấp chứng nhận, báo cáo cuối tháng. |
| Thông tin vào | Phiếu đăng ký học, bài kiểm tra đầu vào, tiền học phí, đơn chuyển lớp/bảo lưu, sổ điểm danh, điểm do giáo viên gửi. |
| Thông tin ra | Phiếu thu, lịch học, danh sách lớp, bảng điểm, chứng nhận, báo cáo doanh thu và tuyển sinh hằng tháng. |
| Cơ sở dữ liệu | Các file Excel riêng của từng bộ phận: danh sách học viên, lớp, sổ thu, bảng điểm. Không có khóa liên kết chung, một học viên có thể bị nhập 2 lần. |
| Xử lý và trao đổi | Giấy tờ chuyển tay giữa các bộ phận, nhắn qua Zalo, cuối tháng gom Excel bằng tay để lập báo cáo. |

## 3. Phương pháp thu thập và kế hoạch

### 3.1 So sánh 6 phương pháp (theo bài giảng)

| Phương pháp | Ưu điểm | Nhược điểm | Nhóm áp dụng |
|---|---|---|---|
| Nghiên cứu tài liệu | Cho cái nhìn tổng thể về tổ chức và quy trình; thu được chứng từ thật | Tài liệu có thể cũ, không phản ánh cách làm thực tế | **Có** – làm đầu tiên (mục 4) |
| Quan sát | Biết công việc thực tế và cường độ làm việc | Người bị quan sát thay đổi thói quen; tốn thời gian | **Có** – 3 điểm quan sát (mục 5) |
| Phỏng vấn cá nhân / nhóm | Hỏi thêm được ngay; biết thái độ và trách nhiệm của người trả lời. Phỏng vấn nhóm ít tốn thời gian hơn | Các ý kiến có thể mâu thuẫn; tốn thời gian. Trong phỏng vấn nhóm, cấp dưới ngại nói trái ý cấp trên | **Có** – 5 cá nhân + 1 buổi nhóm (mục 6) |
| Phiếu điều tra | Rẻ, khách quan khi có nhiều phiếu, dễ thống kê | Không hỏi thêm được; tỷ lệ thu hồi phiếu thấp | **Có** – phát cho học viên và phụ huynh (mục 7) |
| JAD (thảo luận chuyên đề) | Thảo luận có kiểm soát, ra được giải pháp tốt nhất | Tốn kém, nhiều người tham dự | **Có, rút gọn** – 1 buổi về học phí và ưu đãi (mục 8) |
| Làm mẫu (prototyping) | Người dùng hiểu hệ thống, góp ý sửa ngay | Khó thống nhất yêu cầu của nhiều người dùng | **Có** – ở giai đoạn thiết kế form (3.6) |

### 3.2 Kế hoạch thực hiện

| Bước | Phương pháp | Đối tượng / tài liệu | Phụ trách | Sản phẩm |
|---|---|---|---|---|
| 1 | Nghiên cứu tài liệu | Quy chế đào tạo, biểu phí, 5 chứng từ | Nguyễn Gia Ân | Báo cáo kết quả nghiên cứu tài liệu (mục 4) |
| 2 | Quan sát | Quầy tuyển sinh, quầy kế toán, một buổi học | Nguyễn Gia Ân, Trần Thu Thủy | Báo cáo kết quả quan sát (mục 5) |
| 3 | Phỏng vấn cá nhân | Giám đốc, QL đào tạo, NV tuyển sinh, Kế toán, Giáo viên | Cả nhóm, mỗi người 1 buổi | Biên bản phỏng vấn (mục 6) |
| 4 | Phỏng vấn nhóm | Trưởng 3 bộ phận | Hoàng Văn Huynh | Thống nhất quy trình liên bộ phận |
| 5 | Phiếu điều tra | 60 học viên và phụ huynh | Trần Minh Quân | Tổng hợp phiếu (mục 7) |
| 6 | JAD | Giám đốc, Kế toán, NV tuyển sinh, nhóm phân tích | Hoàng Văn Huynh chủ trì | Quy tắc học phí và ưu đãi (mục 8) |
| 7 | Tổng hợp | – | Hoàng Văn Huynh | Mục 9: vấn đề, yêu cầu, quy tắc nghiệp vụ |

---

## 4. Nghiên cứu tài liệu

### 4.1 Báo cáo kết quả nghiên cứu tài liệu hệ thống (mẫu Hình 3.11)

> **Đề án:** Hệ thống quản lý trung tâm ngoại ngữ – Nhóm 10
>
> **BÁO CÁO KẾT QUẢ NGHIÊN CỨU TÀI LIỆU HỆ THỐNG**
>
> **Người thực hiện:** Nguyễn Gia Ân
> **Chủ đề nghiên cứu:** Quy trình ghi danh, thu học phí, tổ chức lớp và đánh giá học viên
> **Thời gian:** tuần 1 của giai đoạn phân tích  **Địa điểm:** văn phòng trung tâm (giả định)
> **Mục tiêu nghiên cứu:** M1, M2, M3 (mục 1)
>
> **Nội dung nghiên cứu:**
> - *Hoạt động của hệ thống:* tư vấn và kiểm tra đầu vào → ghi danh → mở lớp khi đủ sĩ số → xếp lịch, phân công giáo viên và phòng → thu học phí (1 hoặc 2 đợt) → học, điểm danh → nhập điểm giữa kỳ và cuối kỳ → tổng kết, cấp chứng nhận. Có các nghiệp vụ phát sinh: chuyển lớp, bảo lưu, nghỉ học.
> - *Thông tin vào:* phiếu đăng ký học (4.2.1), bài kiểm tra đầu vào, đơn chuyển lớp/bảo lưu, tiền học phí, sổ điểm danh (4.2.3), điểm thành phần.
> - *Thông tin ra:* phiếu thu (4.2.2), lịch học, danh sách lớp, bảng điểm lớp (4.2.4), giấy chứng nhận (4.2.5), báo cáo tháng.
> - *Quá trình xử lý thông tin:* phần lớn làm tay. Học phí và ưu đãi tính bằng máy tính bỏ túi theo biểu phí. Kiểm tra trùng lịch bằng cách nhìn bảng lịch. Điểm tổng kết do giáo viên tự tính trên Excel.
> - *Cơ sở dữ liệu:* sổ ghi danh, sổ thu, các file Excel riêng của từng bộ phận.
>
> **Tóm tắt chung:** quy trình đủ và rõ ràng, nhưng dữ liệu bị phân tán và nhập lặp lại ở nhiều nơi.
> **Đánh giá tổng quát:** cần một CSDL dùng chung, tự động tính học phí, ưu đãi, chuyên cần và điểm tổng kết, đồng thời tự lập báo cáo.
>
> *Ngày … tháng … năm 2026*

### 4.2 Chứng từ mẫu thu được

Đây là nguồn dữ liệu cho từ điển dữ liệu (2.4) và cho chuẩn hóa (3.3). Các trường được giữ nguyên như trên giấy, chưa chuẩn hóa.

#### 4.2.1 Phiếu đăng ký học (luồng "Phiếu đăng ký" vào tiến trình 2.0)

```
TRUNG TÂM NGOẠI NGỮ ...                         Số phiếu: DK2026-0158
                    PHIẾU ĐĂNG KÝ HỌC           Ngày: 05/09/2026
Học viên: Mã HV: HV0412   Họ tên: Nguyễn Minh Anh   Ngày sinh: 14/03/2012   Giới tính: Nữ
          Điện thoại: 0912 345 678   Email: minhanh@gmail.com   Địa chỉ: 12 Trần Phú, Hà Đông
Phụ huynh: Họ tên: Nguyễn Văn Bình   Quan hệ: Bố   Điện thoại: 0983 111 222
Kết quả kiểm tra đầu vào: Trình độ A2 (ngày 02/09/2026)
Lớp đăng ký:
 STT | Mã lớp   | Khóa học              | Lịch học           | Khai giảng | Học phí
  1  | IEK-2609 | IELTS Kids Foundation | T3-T5 18:00-19:30  | 15/09/2026 | 4.800.000
  2  | GT-2604  | Giao tiếp cơ bản      | T7 08:00-10:00     | 19/09/2026 | 2.400.000
Ưu đãi áp dụng: UD-HVCU (học viên cũ, giảm 10%)
Tổng học phí: 7.200.000   Giảm: 720.000   Phải nộp: 6.480.000
Nhân viên tiếp nhận: Trần Thị Hoa          Chữ ký học viên/phụ huynh
```

#### 4.2.2 Phiếu thu học phí (luồng "Phiếu thu" ra từ tiến trình 3.0)

```
                    PHIẾU THU                    Số: PT2026-0731   Ngày: 06/09/2026
Họ tên người nộp: Nguyễn Văn Bình (phụ huynh)
Học viên: HV0412 – Nguyễn Minh Anh      Theo phiếu đăng ký: DK2026-0158
 STT | Mã lớp   | Khóa học              | Học phí   | Giảm    | Phải nộp  | Nộp lần này
  1  | IEK-2609 | IELTS Kids Foundation | 4.800.000 | 480.000 | 4.320.000 | 4.320.000
  2  | GT-2604  | Giao tiếp cơ bản      | 2.400.000 | 240.000 | 2.160.000 | 1.080.000
Tổng nộp lần này: 5.400.000   (Năm triệu bốn trăm nghìn đồng)
Đợt: 1   Còn nợ sau phiếu này: 1.080.000   Hạn đợt 2: 17/10/2026
Hình thức: Chuyển khoản        Người thu: Lê Thu Trang (kế toán)
```

#### 4.2.3 Sổ điểm danh (luồng "Điểm danh" vào tiến trình 4.0)

```
SỔ ĐIỂM DANH   Lớp: IEK-2609 – IELTS Kids Foundation   GV: Phạm Quốc Huy   Phòng: P203
Ký hiệu: x = có mặt, M = đi muộn, P = vắng có phép, K = vắng không phép
 Mã HV  | Họ tên            | B1 15/09 | B2 17/09 | B3 22/09 | B4 24/09 | ... | Ghi chú
 HV0412 | Nguyễn Minh Anh   |    x     |    M     |    x     |    P     |
 HV0398 | Trần Gia Bảo      |    x     |    K     |    K     |    x     |     | Nhắc phụ huynh
```

#### 4.2.4 Bảng điểm lớp (luồng "Điểm số" vào tiến trình 4.0, "Kết quả học tập" ra)

```
BẢNG ĐIỂM   Lớp: IEK-2609   Khóa: IELTS Kids Foundation (trình độ A2 → B1)   GV: Phạm Quốc Huy
Trọng số: Chuyên cần 10% – Giữa kỳ 30% – Cuối kỳ 60%
 Mã HV  | Họ tên          | Chuyên cần | Giữa kỳ | Cuối kỳ | Tổng kết | Xếp loại | Kết quả
 HV0412 | Nguyễn Minh Anh |    9.0     |   7.5   |   8.0   |   8.0    |   Khá    |  Đạt
 HV0398 | Trần Gia Bảo    |    6.0     |   5.0   |   4.0   |   4.5    |    –     | Không đạt
Nhận xét của GV: ...
```

#### 4.2.5 Giấy chứng nhận hoàn thành khóa học (luồng "Chứng nhận" ra từ tiến trình 4.0)

```
Số: CN2026-0089
Chứng nhận học viên: Nguyễn Minh Anh   Ngày sinh: 14/03/2012
Đã hoàn thành khóa học: IELTS Kids Foundation (lớp IEK-2609), từ 15/09/2026 đến 15/12/2026
Kết quả: Khá (8.0)      Ngày cấp: 20/12/2026      Giám đốc trung tâm
```

## 5. Quan sát hệ thống – báo cáo kết quả quan sát (giả định)

| Điểm quan sát | Thời gian | Điều quan sát được | Hệ quả cho thiết kế |
|---|---|---|---|
| Quầy tuyển sinh | 17:00–19:00, ngày khai giảng | Ghi một phiếu đăng ký mất 10–15 phút. Phải gọi sang phòng đào tạo để hỏi lớp còn chỗ không. Hai lần tra nhầm lớp đã đủ sĩ số. | Hiển thị sĩ số và số chỗ trống theo thời gian thực (2.2); tự điền thông tin của học viên cũ (1.1). |
| Quầy kế toán | Đầu tháng | Tính ưu đãi bằng tay, có 1 phiếu áp sai mức giảm. Phải tra sổ để biết học viên còn nợ bao nhiêu. | Tính học phí và ưu đãi tự động (3.1); xem công nợ ngay khi lập phiếu thu (3.2, 3.3). |
| Một buổi học | 18:00–19:30 | Giáo viên điểm danh trên giấy, cuối tuần mới gửi lại văn phòng. Phụ huynh không biết con vắng học. | Giáo viên điểm danh trực tiếp trên hệ thống (4.1); gửi thông báo vắng học cho phụ huynh (5.2). |

Nhận xét chung: công việc ở văn phòng bị ngắt quãng liên tục vì vừa phải tiếp khách vừa nghe điện thoại, nên hệ thống cần **lưu nháp phiếu** để nhân viên làm tiếp sau khi bị gián đoạn (đúng như bài giảng lưu ý khi nói về phương pháp quan sát).

## 6. Phỏng vấn

### 6.1 Kế hoạch phỏng vấn cá nhân

| Người được phỏng vấn | Mục tiêu | Thời lượng |
|---|---|---|
| Giám đốc | Mục tiêu, báo cáo cần có, chính sách học phí | 45 phút |
| Quản lý đào tạo | Khóa học, mở lớp, xếp lịch, phân công giáo viên, đánh giá | 45 phút |
| Nhân viên tuyển sinh / CSHV | Tư vấn, kiểm tra đầu vào, ghi danh, chuyển lớp, bảo lưu | 30 phút |
| Nhân viên kế toán | Học phí, ưu đãi, phiếu thu, công nợ, doanh thu | 30 phút |
| Giáo viên | Điểm danh, nhập điểm, nhận xét, lịch dạy | 30 phút |

### 6.2 Bộ câu hỏi (Đ = câu hỏi đóng, M = câu hỏi mở)

**Giám đốc**
1. (M) Những vấn đề nào trong vận hành hiện nay khiến anh/chị lo ngại nhất?
2. (M) Anh/chị cần xem những báo cáo nào, với tần suất bao lâu?
3. (Đ) Báo cáo doanh thu cần lập theo: ☐ ngày ☐ tuần ☐ tháng ☐ quý
4. (M) Trung tâm đang có những chính sách ưu đãi học phí nào? Chính sách nào sắp thay đổi?
5. (Đ) Có dự định mở thêm cơ sở trong 2 năm tới không? ☐ Có ☐ Không

**Quản lý đào tạo**
1. (M) Hãy mô tả các bước từ khi lên kế hoạch khóa học đến khi khai giảng một lớp.
2. (Đ) Sĩ số tối thiểu để mở lớp là bao nhiêu? Sĩ số tối đa là bao nhiêu?
3. (M) Khi xếp lịch, việc trùng giáo viên hoặc trùng phòng xảy ra như thế nào?
4. (Đ) Trọng số điểm hiện tại: chuyên cần … % – giữa kỳ … % – cuối kỳ … %
5. (M) Điều kiện để một học viên được cấp chứng nhận là gì?

**Nhân viên tuyển sinh / CSHV**
1. (M) Hãy mô tả một lượt tiếp nhận học viên mới từ đầu đến cuối.
2. (Đ) Bài kiểm tra đầu vào xếp học viên theo thang trình độ nào? ☐ CEFR (A1–C1) ☐ Thang riêng
3. (M) Quy định chuyển lớp và bảo lưu hiện nay ra sao? Những trường hợp nào gây tranh cãi?
4. (Đ) Trung bình mỗi tháng có bao nhiêu yêu cầu chuyển lớp hoặc bảo lưu?

**Nhân viên kế toán**
1. (M) Học phí và ưu đãi đang được tính như thế nào? Các ưu đãi có được cộng dồn không?
2. (Đ) Học phí được đóng tối đa mấy đợt? Hạn của từng đợt là khi nào?
3. (M) Anh/chị theo dõi công nợ và nhắc học viên nợ học phí bằng cách nào?
4. (Đ) Các hình thức thanh toán đang nhận: ☐ Tiền mặt ☐ Chuyển khoản ☐ Thẻ

**Giáo viên**
1. (M) Thầy/cô điểm danh và gửi điểm cho văn phòng bằng cách nào?
2. (Đ) Nếu có hệ thống, thầy/cô muốn điểm danh trên: ☐ Điện thoại ☐ Máy tính
3. (M) Thông tin nào về học viên mà thầy/cô muốn xem trước khi vào lớp?

### 6.3 Tóm tắt kết quả phỏng vấn (giả định)

| Người được phỏng vấn | Ý chính |
|---|---|
| Giám đốc | Cần báo cáo doanh thu và công nợ hằng tháng, báo cáo tuyển sinh theo khóa, tỷ lệ lấp đầy lớp. Muốn giữ chân học viên cũ bằng ưu đãi. Chưa mở thêm cơ sở trong 2 năm tới. |
| Quản lý đào tạo | Lớp mở khi có tối thiểu 8 học viên, tối đa 20. Trùng phòng xảy ra khoảng 2–3 lần mỗi kỳ. Trọng số điểm: 10–30–60. Học viên đạt khi điểm tổng kết ≥ 5 và chuyên cần ≥ 70%. |
| NV tuyển sinh / CSHV | Xếp trình độ theo CEFR. Chuyển lớp chỉ được làm trong 3 buổi đầu. Bảo lưu tối đa 6 tháng. Mỗi tháng có khoảng 10–15 yêu cầu chuyển lớp hoặc bảo lưu. |
| Nhân viên kế toán | Học phí đóng tối đa 2 đợt. Ưu đãi được cộng dồn nhưng tổng giảm không quá 15%. Đang nhận tiền mặt và chuyển khoản. Công nợ đang theo dõi bằng Excel. |
| Giáo viên | Muốn điểm danh bằng điện thoại. Muốn xem danh sách lớp kèm số buổi vắng của từng học viên. |

### 6.4 Phỏng vấn nhóm (trưởng 3 bộ phận)

Thống nhất được 2 điểm: (1) phiếu đăng ký chỉ có hiệu lực sau khi học viên đóng đợt 1; (2) học viên nợ quá hạn đợt 2 sẽ được nhắc qua thông báo, chưa cho nghỉ học ngay.

## 7. Phiếu điều tra học viên / phụ huynh

```
PHIẾU KHẢO SÁT Ý KIẾN HỌC VIÊN VÀ PHỤ HUYNH
[Tiêu đề] Nhằm nâng cao chất lượng phục vụ, trung tâm mong anh/chị dành 5 phút trả lời các câu hỏi dưới đây.
          Mọi thông tin chỉ dùng cho mục đích cải tiến dịch vụ.
[Định danh] Anh/chị là: ☐ Học viên ☐ Phụ huynh   Độ tuổi học viên: ☐ <12 ☐ 12–17 ☐ ≥18
            Khóa đang học: ……………   Đã học ở trung tâm: ☐ <3 tháng ☐ 3–12 tháng ☐ >12 tháng
[Câu hỏi]
 1. Anh/chị thường biết lịch học, lịch nghỉ qua: ☐ Giáo viên báo ☐ Zalo ☐ Gọi điện văn phòng ☐ Khác
 2. Mức hài lòng với thủ tục đăng ký và đóng học phí (1–5): ☐1 ☐2 ☐3 ☐4 ☐5
 3. Anh/chị có muốn tra cứu học phí, công nợ trực tuyến không? ☐ Có ☐ Không
 4. (Phụ huynh) Anh/chị có muốn nhận thông báo khi con vắng học không? ☐ Có ☐ Không
 5. Anh/chị muốn xem kết quả học tập theo: ☐ Từng bài kiểm tra ☐ Cuối khóa ☐ Không cần
 6. Góp ý khác: …………………………………………
[Kết thúc] Xin chân thành cảm ơn! – Người chủ trì: Trần Minh Quân, Nhóm 10
```

**Tổng hợp (giả định, thu về 48/60 phiếu):** 71% biết lịch học qua giáo viên hoặc Zalo; mức hài lòng trung bình với thủ tục là 3,1/5; 85% muốn tra cứu học phí trực tuyến; 93% phụ huynh muốn nhận thông báo khi con vắng học.

## 8. Thảo luận chuyên đề (JAD rút gọn): học phí và ưu đãi

- **Thành phần:** Giám đốc (người quản lý, đánh giá khả thi), Kế toán và NV tuyển sinh (người sử dụng), nhóm phân tích (người phát triển). Chủ trì: Hoàng Văn Huynh. Thư ký: Trần Thu Thủy.
- **Trình tự:** đặt vấn đề (tính ưu đãi đang sai, không thống nhất) → thảo luận → chọn giải pháp → kết luận.
- **Kết luận:** chốt các quy tắc QT01–QT05 ở mục 9.3.

## 9. Tổng hợp kết quả thu thập

### 9.1 Vấn đề của hệ thống hiện tại

| # | Vấn đề | Nguồn phát hiện | Chức năng giải quyết (BFD_v3) |
|---|---|---|---|
| V1 | Dữ liệu học viên bị nhập lặp lại ở nhiều nơi, thông tin không khớp nhau | Tài liệu, phỏng vấn | 1.1 |
| V2 | Không biết số chỗ trống của lớp ngay lúc ghi danh | Quan sát | 2.1, 2.2 |
| V3 | Trùng giáo viên hoặc trùng phòng khi xếp lịch | Phỏng vấn QL đào tạo | 2.3 |
| V4 | Tính ưu đãi sai, không thống nhất | Quan sát, JAD | 3.1 |
| V5 | Khó theo dõi công nợ và hạn đóng đợt 2 | Phỏng vấn kế toán | 3.3 |
| V6 | Điểm danh trên giấy, cập nhật chậm; phụ huynh không được báo khi con vắng | Quan sát, phiếu điều tra | 4.1, 5.2 |
| V7 | Giáo viên tự tính điểm tổng kết nên dễ sai | Tài liệu | 4.3 |
| V8 | Báo cáo tháng phải gom Excel bằng tay | Phỏng vấn giám đốc | 3.4, 5.3, 5.4 |

### 9.2 Yêu cầu đối với hệ thống mới

**Yêu cầu chức năng:** 20 chức năng lá trong BFD_v3 (`so_do/BFD_v3.png`). Mô tả chi tiết ở bước 2.2.

**Yêu cầu phi chức năng:**
- Dùng được trên trình duyệt máy tính và điện thoại (giáo viên điểm danh bằng điện thoại).
- Phục vụ được 50–100 người dùng đồng thời vào giờ cao điểm (số liệu từ BT1).
- Phân quyền theo vai trò; phụ huynh chỉ xem được thông tin của con mình (bảo vệ dữ liệu cá nhân của trẻ vị thành niên).
- Cho phép lưu nháp phiếu đăng ký và phiếu thu.
- Sao lưu dữ liệu hằng ngày.

### 9.3 Quy tắc nghiệp vụ thu được

Các bước 2.4 (bảng/cây quyết định), 3.4 (ràng buộc CHECK) và demo đều dùng **đúng mã quy tắc** dưới đây.

| Mã | Quy tắc | Áp dụng ở |
|---|---|---|
| QT01 | Ưu đãi **học viên cũ** (đã học hết ít nhất 1 khóa trước đó: lớp đã kết thúc, đăng ký không ở trạng thái nghỉ học): giảm 10% | 3.1 |
| QT02 | Ưu đãi **đăng ký nhóm** (từ 3 người trở lên, đăng ký cùng ngày): giảm 5% | 3.1 |
| QT03 | Ưu đãi **đóng một lần** toàn bộ học phí: giảm 5% | 3.1 |
| QT04 | Các ưu đãi được cộng dồn nhưng tổng mức giảm **tối đa 15%**; chỉ áp dụng ưu đãi còn trong thời gian hiệu lực | 3.1 |
| QT05 | Học phí đóng tối đa **2 đợt**: đợt 1 ≥ 50% trước ngày khai giảng; đợt 2 trước buổi học giữa khóa. Phiếu đăng ký chỉ có hiệu lực sau khi đóng đợt 1 | 2.2, 3.2, 3.3 |
| QT06 | Lớp được mở khi có **tối thiểu 8** học viên; sĩ số **tối đa 20** và không vượt quá sức chứa phòng | 2.1, 2.2 |
| QT07 | Không có 2 buổi học trùng thời gian khi dùng **cùng giáo viên** hoặc **cùng phòng** | 2.3 |
| QT08 | **Chuyển lớp** chỉ được thực hiện trong 3 buổi đầu, sang lớp cùng khóa học và còn chỗ; không tính phí | 2.4 |
| QT09 | **Bảo lưu** khi đã đóng đủ học phí và đã học dưới 50% số buổi; tối đa 6 tháng; mỗi khóa được bảo lưu 1 lần | 2.4 |
| QT10 | **Nghỉ học**: không hoàn học phí; phần học phí còn nợ được hủy | 2.4, 3.3 |
| QT11 | Trạng thái điểm danh: Có mặt, Đi muộn, Vắng có phép, Vắng không phép. **Tỷ lệ chuyên cần** = (có mặt + đi muộn) / số buổi đã học | 4.1 |
| QT12 | Vắng quá **20%** số buổi thì gửi cảnh báo cho học viên và phụ huynh | 4.1, 5.2 |
| QT13 | **Điểm tổng kết** = 10% chuyên cần + 30% giữa kỳ + 60% cuối kỳ (thang 10, làm tròn 1 chữ số thập phân) | 4.3 |
| QT14 | **Xếp loại** theo điểm tổng kết: ≥ 8,5 Giỏi; ≥ 7,0 Khá; ≥ 5,0 Trung bình; < 5,0 Không đạt | 4.3 |
| QT15 | **Đạt** khi điểm tổng kết ≥ 5,0 và tỷ lệ chuyên cần ≥ 70%; chỉ học viên đạt mới được cấp chứng nhận | 4.3, 4.4 |
| QT16 | Học viên nợ quá hạn đợt 2 được nhắc qua thông báo, chưa bị cho nghỉ học | 3.3, 5.2 |

### 9.4 Kiểm tra độ phủ

- Mọi chức năng lá trong BFD_v3 đều có ít nhất một nguồn thông tin: tài liệu, quan sát, phỏng vấn hoặc điều tra (xem cột "Chức năng giải quyết" ở mục 9.1 và cột "Áp dụng ở" ở mục 9.3). Riêng các chức năng 1.2, 1.3, 1.4 (danh mục giáo viên, khóa học, phòng học) có nguồn là nghiên cứu tài liệu và phỏng vấn QL đào tạo; chức năng 5.1 (tài khoản, phân quyền) có nguồn là yêu cầu phi chức năng ở mục 9.2.
- Mỗi chứng từ ở mục 4.2 ứng với một luồng dữ liệu trong DFD mức 0 (`so_do/can_bang_dfd.md`).
