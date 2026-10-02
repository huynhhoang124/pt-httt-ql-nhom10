# 2.4 Đặc tả xử lý và từ điển dữ liệu – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn Phân tích hệ thống (KE_HOACH.md, mục 2.4). Bài giảng mục 3.3.3: DFD không diễn tả hết chi tiết xử lý, nên các xử lý ở mức cơ sở (ở đây là các chức năng x.y của DFD mức 1) được mô tả bổ sung bằng:
- **ngôn ngữ có cấu trúc giản lược**: nơi thực hiện, sự kiện kích hoạt, ai thực hiện, thực hiện thế nào;
- **cây quyết định**: các trường hợp rẽ nhánh;
- **bảng quyết định**: điều kiện – quy tắc – hành động, dùng cho xử lý phức tạp;
- **từ điển dữ liệu**: Tên gọi – Ý nghĩa – Cấu trúc dữ liệu – Nguồn gốc.

Mã quy tắc QTxx lấy từ `phan_tich/02_thu_thap.md` mục 9.3. Tên luồng và kho lấy đúng theo `so_do/src/dfd.py`.

---

## 1. Ngôn ngữ có cấu trúc giản lược

### 1.1 Các chức năng cập nhật danh mục (1.1–1.4, 2.1, 5.1)

Các chức năng này có cùng một khuôn xử lý, nên chỉ mô tả một lần:

```
Nơi thực hiện : màn hình danh mục tương ứng (học viên, giáo viên, khóa học, phòng học, lớp, tài khoản)
Kích hoạt     : người dùng chọn Thêm / Sửa / Ngừng sử dụng
Người thực hiện: 1.1 NV tuyển sinh; 1.2–1.4, 2.1 QL đào tạo; 5.1 Quản trị viên
Xử lý:
  NẾU Thêm THÌ
      kiểm tra các trường bắt buộc và định dạng (điện thoại, email, ngày)
      NẾU trùng bản ghi đã có THÌ báo lỗi và hiển thị bản ghi trùng
      NGƯỢC LẠI cấp mã mới, ghi vào kho
  NẾU Sửa THÌ ghi đè các trường được phép sửa, ghi nhật ký người sửa và thời điểm
  NẾU Ngừng sử dụng THÌ đổi Trạng thái thành "Ngừng" (không xóa, vì dữ liệu cũ còn tham chiếu tới)
```

Kiểm tra riêng của từng chức năng:

| Chức năng | Kiểm tra trùng / ràng buộc riêng |
|---|---|
| 1.1 | Trùng học viên nếu cùng *Điện thoại* và *Ngày sinh*. Phải có Trình độ đầu vào trước khi đăng ký học. Học viên dưới 18 tuổi bắt buộc có ít nhất 1 phụ huynh. |
| 1.2 | Trùng giáo viên nếu cùng *Điện thoại* hoặc *Email*. |
| 1.3 | Tổng ba trọng số = 100% (QT13). Học phí > 0, số buổi > 0. |
| 1.4 | Sức chứa > 0. |
| 2.1 | Sĩ số tối đa ≤ 20 và ≤ sức chứa phòng (QT06). Giáo viên phải dạy đúng ngôn ngữ của khóa học. Đến ngày khai giảng: nếu số đăng ký có hiệu lực ≥ 8 thì Trạng thái lớp = "Đang học", ngược lại = "Hủy" (QT06), rồi chuyển *Lớp đã mở* sang 2.3. |
| 5.1 | Tên đăng nhập là duy nhất. Tài khoản phụ huynh chỉ được gắn với học viên là con của mình. |

### 1.2 Chức năng 2.2 – Đăng ký học, xếp học viên vào lớp

```
Nơi thực hiện : quầy tuyển sinh
Kích hoạt     : nhận Yêu cầu đăng ký (HV/PH) hoặc Phiếu đăng ký (NV tuyển sinh)
Người thực hiện: NV tuyển sinh / CSHV
Xử lý:
  đọc Học viên (D1)
  NẾU học viên chưa có Trình độ đầu vào THÌ chuyển sang 1.1 để kiểm tra trình độ, dừng
  VỚI MỖI lớp trên phiếu:
      đọc Sĩ số lớp (D2)
      NẾU Trạng thái lớp ∉ {Dự kiến, Đang học} THÌ báo "Lớp không nhận đăng ký", bỏ qua lớp này
      NẾU số đăng ký còn giữ chỗ ≥ Sĩ số tối đa THÌ báo "Lớp đã đủ", gợi ý lớp cùng khóa còn chỗ
      NẾU học viên đã có đăng ký ở lớp này THÌ báo trùng
      NGƯỢC LẠI ghi Đăng ký (D2) với Trạng thái = "Đã đăng ký"
  đọc Tình trạng đóng học phí (D3)
  trả Kết quả đăng ký, danh sách lớp cho NV tuyển sinh, ghi rõ từng đăng ký:
      "Đã xác nhận" nếu đã thu ≥ 50% học phí phải nộp, ngược lại "Chờ đóng phí" (QT05)
```

Ghi chú: *hiệu lực của đăng ký* là thuộc tính thứ sinh (S), tính từ số đã thu trong D3, không lưu riêng. Số đăng ký còn giữ chỗ = số đăng ký của lớp có Trạng thái ∉ {Chuyển lớp, Nghỉ học}.

### 1.3 Chức năng 2.3 – Xếp lịch, kiểm tra trùng lịch

```
Nơi thực hiện : phòng đào tạo
Kích hoạt     : nhận Lớp đã mở (từ 2.1), hoặc QL đào tạo yêu cầu đổi lịch / thêm buổi học bù
Người thực hiện: QL đào tạo (hệ thống kiểm tra trùng)
Xử lý:
  sinh danh sách buổi học từ Ngày khai giảng, Lịch tuần của lớp và Số buổi của khóa
  VỚI MỖI buổi học B định xếp (ngày, giờ bắt đầu, giờ kết thúc, phòng, giáo viên):
      đọc Lịch đã xếp (D2) của cùng ngày
      buổi X bị TRÙNG với B NẾU  X.giờ bắt đầu < B.giờ kết thúc  VÀ  B.giờ bắt đầu < X.giờ kết thúc
                               VÀ  (X.giáo viên = B.giáo viên  HOẶC  X.phòng = B.phòng)        (QT07)
      NẾU có buổi trùng THÌ báo trùng (nêu rõ lớp, giáo viên hoặc phòng bị trùng), không ghi
      NGƯỢC LẠI ghi Lịch học (buổi học) vào D2
  gửi Lịch học cho HV/PH; gửi Lịch dạy, danh sách lớp cho Giáo viên
```

### 1.4 Chức năng 3.2 – Lập phiếu thu

```
Nơi thực hiện : quầy kế toán
Kích hoạt     : HV/PH nộp Tiền học phí
Người thực hiện: NV kế toán
Xử lý:
  đọc Học phí phải thu, số đã thu (D3) của các đăng ký được nộp
  VỚI MỖI dòng (đăng ký, đợt) trên phiếu:
      Còn nợ = Phải nộp − Đã thu
      NẾU số tiền nộp > Còn nợ THÌ báo lỗi "Nộp vượt số còn nợ"
      NẾU đợt = 1 VÀ (Đã thu + số tiền nộp) < 50% × Phải nộp THÌ báo lỗi "Đợt 1 tối thiểu 50%" (QT05)
  NẾU không có lỗi THÌ
      cấp Số phiếu thu (tăng dần), Ngày thu = hôm nay, NV thu = người đang đăng nhập
      ghi Phiếu thu mới (D3)
      in Phiếu thu: 1 liên cho HV/PH, 1 liên cho NV kế toán
```

### 1.5 Chức năng 3.3 – Theo dõi công nợ

```
Nơi thực hiện : phòng kế toán
Kích hoạt     : hằng ngày lúc đầu ngày, hoặc khi kế toán mở màn hình công nợ
Người thực hiện: hệ thống tính, NV kế toán xem
Xử lý:
  đọc Trạng thái đăng ký (D2); đọc Học phí phải thu, số đã thu (D3)
  VỚI MỖI đăng ký có Trạng thái ∉ {Nghỉ học, Chuyển lớp}:                     (QT10)
      Công nợ = Phải nộp − Đã thu
      NẾU Công nợ > 0 VÀ đã qua Hạn đóng của đợt chưa đóng đủ THÌ đánh dấu "Quá hạn"   (QT16)
  gửi Công nợ cho NV kế toán (lọc được theo lớp, theo tình trạng quá hạn)
  gửi Tổng công nợ sang 3.4
```

Đăng ký ở trạng thái *Chuyển lớp* không tính nợ vì số đã thu được chuyển sang đăng ký mới ở lớp đích (xem cây quyết định 2.4).

### 1.6 Chức năng 4.1 – Điểm danh, tính chuyên cần

```
Nơi thực hiện : lớp học (giáo viên dùng điện thoại hoặc máy tính)
Kích hoạt     : giáo viên mở buổi học trong ngày dạy
Người thực hiện: Giáo viên
Xử lý:
  đọc Danh sách lớp, buổi học (D2): chỉ lấy học viên có đăng ký Trạng thái = "Đã đăng ký"
  giáo viên chọn trạng thái cho từng học viên: x (có mặt), M (đi muộn), P (vắng có phép), K (vắng không phép)   (QT11)
  ghi Điểm danh đã ghi (D4); chỉ được sửa trong vòng 24 giờ sau buổi học
  VỚI MỖI học viên:
      Tỷ lệ chuyên cần = số buổi (x hoặc M) / số buổi đã học
      gửi Chuyên cần cho HV/PH
      áp dụng cây quyết định cảnh báo chuyên cần (mục 2.2)
```

### 1.7 Chức năng 4.3 – Tổng kết, xếp loại

```
Nơi thực hiện : phòng đào tạo
Kích hoạt     : lớp đã học hết số buổi và giáo viên đã nhập đủ điểm giữa kỳ, cuối kỳ
Người thực hiện: hệ thống tính, QL đào tạo duyệt
Xử lý:
  đọc Trọng số điểm khóa học (D1); đọc Điểm danh, điểm thành phần (D4)
  VỚI MỖI học viên của lớp (không tính đăng ký Chuyển lớp, Nghỉ học, Bảo lưu):
      Điểm chuyên cần = Tỷ lệ chuyên cần × 10
      Điểm tổng kết  = TS_cc × Điểm chuyên cần + TS_gk × Điểm giữa kỳ + TS_ck × Điểm cuối kỳ,
                       làm tròn 1 chữ số thập phân                                   (QT13)
      xác định Xếp loại và Kết quả theo bảng quyết định 4.3 (mục 3.2)
      ghi Kết quả học tập (D4); gửi Kết quả học tập cho HV/PH
  gửi Danh sách học viên đạt sang 4.4
```

Kiểm tra lại với bảng điểm mẫu (02_thu_thap.md, mục 4.2.4): HV0412 có 0,1×9,0 + 0,3×7,5 + 0,6×8,0 = 7,95 → **8,0 – Khá**; HV0398 có 0,1×6,0 + 0,3×5,0 + 0,6×4,0 = **4,5 – Không đạt**. Khớp với chứng từ.

### 1.8 Các chức năng còn lại

| Chức năng | Mô tả ngắn |
|---|---|
| 3.1 | Kích hoạt khi có đăng ký mới. Phải nộp = Học phí khóa × (1 − Tỷ lệ giảm), với tỷ lệ giảm lấy theo **bảng quyết định 3.1** (mục 3.1). Nếu học viên chọn đóng một lần: 1 đợt, hạn đóng là ngày khai giảng. Ngược lại: 2 đợt, đợt 1 = 50% hạn ngày khai giảng, đợt 2 = 50% hạn ngày của buổi học giữa khóa (QT05). Ghi vào D3 và gửi Học phí phải thu cho kế toán. |
| 3.4 | Kích hoạt khi Giám đốc yêu cầu (theo tháng, quý hoặc khoảng ngày). Doanh thu = tổng tiền các phiếu thu trong kỳ, nhóm theo khóa học và hình thức thanh toán; kèm Tổng công nợ từ 3.3; có so sánh với kỳ trước. |
| 4.2 | Kiểm tra 0 ≤ Điểm ≤ 10. Chỉ giáo viên được phân công dạy lớp mới được nhập điểm. Được sửa điểm cho đến khi lớp đã được tổng kết. |
| 4.4 | Với mỗi học viên trong Danh sách học viên đạt: cấp Số chứng nhận (tăng dần theo năm, dạng CN2026-xxxx), Ngày cấp = hôm nay; ghi D4 và in chứng nhận (QT15). |
| 5.2 | Chạy tự động hằng ngày, gửi thông báo khi có: lịch học thay đổi; khoản học phí còn 3 ngày đến hạn hoặc đã quá hạn (QT16); cảnh báo vắng (QT12); kết quả học tập. Người nhận là học viên, cùng với phụ huynh nếu học viên dưới 18 tuổi. Ghi Thông báo đã gửi vào D5. |
| 5.3, 5.4 | Kích hoạt theo yêu cầu của Giám đốc hoặc định kỳ cuối tháng. Chỉ đọc dữ liệu, tổng hợp theo kỳ, có so sánh với kỳ trước (đặc trưng của MIS). Danh sách chỉ tiêu: xem các dòng báo cáo ở mục 4.2. |

---

## 2. Cây quyết định

### 2.1 Chức năng 2.4 – Xử lý chuyển lớp, bảo lưu, nghỉ học (QT08, QT09, QT10)

```
Yêu cầu của học viên
├── Chuyển lớp
│   ├── Số buổi đã học của lớp hiện tại > 3 ──────────────► TỪ CHỐI: "Quá thời hạn chuyển lớp"
│   └── Số buổi đã học ≤ 3
│       ├── Lớp đích khác khóa học ───────────────────────► TỪ CHỐI: "Chỉ chuyển sang lớp cùng khóa"
│       └── Lớp đích cùng khóa học
│           ├── Lớp đích đã đủ sĩ số ─────────────────────► TỪ CHỐI: "Lớp đích hết chỗ"
│           └── Lớp đích còn chỗ ─────────────────────────► CHẤP NHẬN: đăng ký cũ = "Chuyển lớp";
│                                                           tạo đăng ký mới ở lớp đích (giữ số đã thu);
│                                                           không thu thêm phí
├── Bảo lưu
│   ├── Chưa đóng đủ học phí ─────────────────────────────► TỪ CHỐI: "Cần đóng đủ học phí"
│   └── Đã đóng đủ học phí
│       ├── Đã học ≥ 50% số buổi ─────────────────────────► TỪ CHỐI: "Đã học quá nửa khóa"
│       └── Đã học < 50% số buổi
│           ├── Đã bảo lưu ở khóa này rồi ────────────────► TỪ CHỐI: "Mỗi khóa chỉ bảo lưu 1 lần"
│           └── Chưa bảo lưu lần nào ─────────────────────► CHẤP NHẬN: đăng ký = "Bảo lưu";
│                                                           Hạn bảo lưu = hôm nay + 6 tháng
└── Nghỉ học ─────────────────────────────────────────────► CHẤP NHẬN: đăng ký = "Nghỉ học";
                                                            không hoàn tiền; phần còn nợ bị hủy
```

Học viên đang bảo lưu muốn học lại thì đi lại chức năng 2.2 với một lớp cùng khóa, mở trước Hạn bảo lưu. Quá Hạn bảo lưu thì đăng ký tự chuyển sang "Nghỉ học".

Cả khi chuyển lớp và khi học lại sau bảo lưu, đăng ký mới đều lưu *Đăng ký gốc*. Vì vậy học phí **không phải ghi lại vào D3**: 3.1 không lập khoản phải thu mới cho đăng ký có Đăng ký gốc, và Đã thu của đăng ký mới được tính gộp cả các phiếu thu của đăng ký gốc (mục 4.5). Nhờ vậy tiến trình 2.0 chỉ cần đọc D3, đúng như DFD.

### 2.2 Chức năng 4.1 / 5.2 – Cảnh báo chuyên cần (QT11, QT12, QT15)

```
Sau mỗi buổi điểm danh, với mỗi học viên: Tỷ lệ vắng = 1 − Tỷ lệ chuyên cần (tính trên số buổi đã học)
├── Tỷ lệ vắng ≤ 20% ───────────────► Không làm gì
├── 20% < Tỷ lệ vắng ≤ 30% ─────────► Gửi "Cảnh báo vắng học" cho học viên (và phụ huynh nếu dưới 18 tuổi)
└── Tỷ lệ vắng > 30% ───────────────► Gửi cảnh báo + đánh dấu "Nguy cơ không đạt"
                                      (chuyên cần sẽ < 70%, không đủ điều kiện đạt theo QT15)
```

---

## 3. Bảng quyết định

### 3.1 Chức năng 3.1 – Tính học phí, áp dụng ưu đãi (QT01–QT05)

Một điều kiện chỉ được tính là "Có" khi ưu đãi tương ứng đang trong thời gian hiệu lực (QT04).

| | R1 | R2 | R3 | R4 | R5 | R6 | R7 | R8 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| **Điều kiện** | | | | | | | | |
| C1. Học viên cũ (QT01) | C | C | C | C | K | K | K | K |
| C2. Đăng ký nhóm ≥ 3 người (QT02) | C | C | K | K | C | C | K | K |
| C3. Đóng một lần (QT03) | C | K | C | K | C | K | C | K |
| **Hành động** | | | | | | | | |
| Giảm 0% | | | | | | | | X |
| Giảm 5% | | | | | | X | X | |
| Giảm 10% | | | | X | X | | | |
| Giảm 15% | X¹ | X | X | | | | | |
| Thu 1 đợt (hạn: ngày khai giảng) | X | | X | | X | | X | |
| Thu 2 đợt (50% + 50%) (QT05) | | X | | X | | X | | X |

C = Có, K = Không. ¹ R1 cộng dồn được 20%, nhưng bị giới hạn ở mức tối đa 15% (QT04).

Ví dụ với phiếu đăng ký mẫu DK2026-0158: học viên cũ, đăng ký một mình, chia 2 đợt → **R4**: giảm 10%, thu 2 đợt. Phải nộp 6.480.000 đ, khớp với chứng từ.

### 3.2 Chức năng 4.3 – Xếp loại và xét đạt (QT13–QT15)

| | R1 | R2 | R3 | R4 | R5 |
|---|:-:|:-:|:-:|:-:|:-:|
| **Điều kiện** | | | | | |
| C1. Điểm tổng kết (TK) | ≥ 8,5 | 7,0 ≤ TK < 8,5 | 5,0 ≤ TK < 7,0 | ≥ 5,0 | < 5,0 |
| C2. Tỷ lệ chuyên cần ≥ 70% | C | C | C | K | – |
| **Hành động** | | | | | |
| Xếp loại | Giỏi | Khá | Trung bình | Theo điểm (như R1–R3) | Không đạt |
| Kết quả = Đạt | X | X | X | | |
| Kết quả = Không đạt | | | | X | X |
| Đưa vào Danh sách học viên đạt (sang 4.4) | X | X | X | | |

"–" = không xét điều kiện này.

---

## 4. Từ điển dữ liệu

**Ký hiệu cấu trúc:** `+` kết hợp; `{ }` lặp nhiều lần; `( )` có thể có hoặc không; `[ a | b ]` chọn một; `#` thuộc tính định danh; `(S)` thuộc tính thứ sinh (tính ra được, không lưu khi thiết kế CSDL).
**Nguồn gốc:** nơi sinh ra dữ liệu – một tác nhân ngoài hoặc một xử lý x.y.

### 4.1 Kho dữ liệu

| Tên gọi | Ý nghĩa | Cấu trúc dữ liệu | Nguồn gốc |
|---|---|---|---|
| D1 Hồ sơ | Danh mục và hồ sơ gốc | {Học viên} + {Phụ huynh} + {Giáo viên} + {Khóa học} + {Phòng học} | 1.1–1.4 |
| Học viên | Hồ sơ một học viên | #Mã HV + Họ tên + Ngày sinh + Giới tính + Điện thoại + (Email) + (Địa chỉ) + Trình độ đầu vào + Ngày kiểm tra + {Mã PH + Quan hệ} + Trạng thái | 1.1 |
| Phụ huynh | Người giám hộ của học viên dưới 18 tuổi | #Mã PH + Họ tên + Điện thoại + (Email) | 1.1 |
| Giáo viên | Hồ sơ giáo viên | #Mã GV + Họ tên + Ngày sinh + Điện thoại + Email + Ngôn ngữ dạy + Chuyên môn + Bằng cấp + Tình trạng làm việc | 1.2 |
| Khóa học | Chương trình đào tạo | #Mã KH + Tên KH + Ngôn ngữ + Trình độ đầu vào + Trình độ đầu ra + Số buổi + Thời lượng buổi + Học phí + TS chuyên cần + TS giữa kỳ + TS cuối kỳ + Trạng thái | 1.3 |
| Phòng học | Phòng dùng để xếp lịch | #Mã phòng + Tên phòng + Sức chứa + (Thiết bị) + Tình trạng | 1.4 |
| D2 Lớp và lịch học | Lớp, đăng ký và lịch học | {Lớp} + {Đăng ký} + {Buổi học} | 2.1–2.4 |
| Lớp | Một lần tổ chức một khóa học | #Mã lớp + Mã KH + Mã GV + Ngày khai giảng + Lịch tuần + Sĩ số tối đa + Trạng thái [Dự kiến \| Đang học \| Kết thúc \| Hủy] + Số đăng ký (S) | 2.1 |
| Đăng ký | Một học viên học một lớp | #Số phiếu ĐK + Ngày ĐK + Mã HV + {Mã lớp + Trạng thái [Đã đăng ký \| Chuyển lớp \| Bảo lưu \| Nghỉ học] + (Hạn bảo lưu) + (Đăng ký gốc)} + Mã NV tiếp nhận + Hiệu lực (S) | 2.2, 2.4 |
| Buổi học | Một buổi trong lịch của lớp | Mã lớp + #Số thứ tự buổi + Ngày + Giờ bắt đầu + Giờ kết thúc + Mã phòng + Mã GV + Loại [Thường \| Học bù] + Trạng thái [Kế hoạch \| Đã dạy \| Hủy] | 2.3 |
| D3 Học phí | Ưu đãi, khoản phải thu và các lần thu | {Ưu đãi} + {Học phí phải thu} + {Phiếu thu} | 3.1, 3.2 |
| Ưu đãi | Chính sách giảm học phí | #Mã UĐ + Tên UĐ + Tỷ lệ giảm + Ngày bắt đầu + Ngày kết thúc + Điều kiện áp dụng | NV kế toán qua 3.1 |
| Học phí phải thu | Khoản phải nộp của một đăng ký | Số phiếu ĐK + Mã lớp + Học phí gốc + {Mã UĐ} + Tỷ lệ giảm + Phải nộp (S) + {Đợt + Số tiền đợt + Hạn đóng} | 3.1 |
| Phiếu thu | Chứng từ một lần nộp tiền | #Số PT + Ngày thu + Người nộp + Mã HV + {Số phiếu ĐK + Mã lớp + Đợt + Số tiền} + Tổng tiền (S) + Hình thức [Tiền mặt \| Chuyển khoản] + Mã NV thu | 3.2 |
| D4 Học tập | Dữ liệu quá trình và kết quả học | {Điểm danh} + {Điểm thành phần} + {Kết quả học tập} + {Chứng nhận} | 4.1–4.4 |
| Điểm danh | Trạng thái của một học viên trong một buổi | Mã lớp + Số thứ tự buổi + Mã HV + Trạng thái [x \| M \| P \| K] + (Ghi chú) | 4.1 |
| Điểm thành phần | Một cột điểm của một học viên | Mã lớp + Mã HV + Loại điểm [Giữa kỳ \| Cuối kỳ] + Điểm + (Nhận xét) | 4.2 |
| Kết quả học tập | Kết quả cuối khóa | Mã lớp + Mã HV + Tỷ lệ chuyên cần (S) + Điểm chuyên cần (S) + Điểm tổng kết (S) + Xếp loại + Kết quả [Đạt \| Không đạt] | 4.3 |
| Chứng nhận | Giấy chứng nhận hoàn thành | #Số CN + Mã HV + Mã lớp + Ngày cấp | 4.4 |
| D5 Tài khoản và thông báo | Dữ liệu quản trị hệ thống | {Nhân viên} + {Tài khoản} + {Thông báo} + {Nhật ký} | 5.1, 5.2 |
| Nhân viên | Nhân viên văn phòng của trung tâm | #Mã NV + Họ tên + Bộ phận + Chức vụ + Điện thoại + Email | 5.1 |
| Tài khoản | Quyền đăng nhập vào hệ thống | #Tên đăng nhập + Mật khẩu (mã hóa) + Vai trò + [Mã NV \| Mã GV \| Mã HV \| Mã PH] + Trạng thái | 5.1 |
| Thông báo | Một thông báo đã gửi | #Mã TB + Loại [Lịch học \| Học phí \| Chuyên cần \| Kết quả] + Người nhận + Nội dung + Thời gian gửi + Trạng thái gửi | 5.2 |
| Nhật ký | Vết thao tác của người dùng | Thời điểm + Tên đăng nhập + Thao tác + Đối tượng | Mọi xử lý; 5.1 cho xem |

Thuộc tính *Hiệu lực (S)* của Đăng ký = Trạng thái "Đã đăng ký" và số đã thu ≥ 50% Phải nộp. *Số đăng ký (S)* của Lớp = số đăng ký đang giữ chỗ (mục 1.2).

### 4.2 Luồng dữ liệu giữa tác nhân và hệ thống

Mỗi dòng là một luồng trên DFD mức 1. Luồng mức 0 tương ứng xem ở `so_do/can_bang_dfd.md`.

| Tên gọi | Ý nghĩa | Cấu trúc dữ liệu | Nguồn gốc |
|---|---|---|---|
| Hồ sơ học viên, kết quả kiểm tra đầu vào | Thông tin để tạo hoặc cập nhật học viên | Học viên (trừ #Mã HV) + {Phụ huynh + Quan hệ} + Trình độ đầu vào + Ngày kiểm tra | NV tuyển sinh |
| Thông tin giáo viên | Hồ sơ giáo viên mới hoặc cần sửa | Giáo viên | QL đào tạo |
| Thông tin khóa học | Khóa học mới hoặc cần sửa | Khóa học | QL đào tạo |
| Thông tin phòng học | Phòng mới hoặc cần sửa | Phòng học | QL đào tạo |
| Kế hoạch mở lớp, phân công giáo viên | Đề xuất mở lớp | Mã KH + Mã GV + Ngày khai giảng + Lịch tuần + Sĩ số tối đa | QL đào tạo |
| Yêu cầu đăng ký | Mong muốn học của học viên | Họ tên / Mã HV + Khóa học mong muốn + (Khung giờ mong muốn) | HV/PH |
| Phiếu đăng ký | Chứng từ đăng ký (02_thu_thap.md, mục 4.2.1) | Số phiếu ĐK + Ngày + Mã HV + {Mã lớp} + (Mã UĐ) + Mã NV tiếp nhận | NV tuyển sinh |
| Kết quả đăng ký, danh sách lớp | Kết quả xếp lớp | {Mã lớp + Tên KH + Lịch tuần + Ngày khai giảng + Trạng thái [Đã xác nhận \| Chờ đóng phí \| Lớp đã đủ]} + Danh sách học viên của lớp | 2.2 |
| Yêu cầu chuyển lớp, bảo lưu | Mong muốn thay đổi việc học | Mã HV + Mã lớp + Loại [Chuyển lớp \| Bảo lưu \| Nghỉ học] + (Lớp đích) + Lý do | HV/PH |
| Đơn chuyển lớp, bảo lưu | Đơn chính thức do nhân viên lập | Số phiếu ĐK + Mã lớp + Loại + (Lớp đích) + Lý do + Ngày lập | NV tuyển sinh |
| Kết quả xử lý đăng ký | Trả lời cho đơn | Số phiếu ĐK + Loại + [Chấp nhận \| Từ chối] + (Lý do từ chối) + (Hạn bảo lưu) + (Đăng ký mới) | 2.4 |
| Lịch học | Lịch các buổi học gửi học viên | Mã lớp + {Ngày + Giờ bắt đầu + Giờ kết thúc + Phòng} | 2.3 |
| Lịch dạy, danh sách lớp | Lịch gửi giáo viên | {Mã lớp + {Ngày + Giờ + Phòng}} + {Mã HV + Họ tên} | 2.3 |
| Ưu đãi | Thông tin ưu đãi do kế toán nhập | Ưu đãi | NV kế toán |
| Học phí phải thu | Khoản học viên phải nộp | Số phiếu ĐK + Học phí gốc + Tỷ lệ giảm + Phải nộp + {Đợt + Số tiền đợt + Hạn đóng} | 3.1 |
| Tiền học phí | Số tiền học viên nộp | Người nộp + Số tiền + Hình thức | HV/PH |
| Thông tin thu tiền | Thông tin kế toán ghi khi thu tiền | Mã HV + {Số phiếu ĐK + Mã lớp + Đợt + Số tiền} + Hình thức | NV kế toán |
| Phiếu thu | Chứng từ thu (02_thu_thap.md, mục 4.2.2) | Phiếu thu + Còn nợ (S) | 3.2 |
| Công nợ | Danh sách nợ học phí | {Mã HV + Họ tên + Mã lớp + Phải nộp + Đã thu + Còn nợ (S) + Hạn đóng + Quá hạn (S)} | 3.3 |
| Yêu cầu báo cáo doanh thu | Tham số của báo cáo | Từ ngày + Đến ngày + (Khóa học) | Giám đốc |
| Báo cáo doanh thu, công nợ | Báo cáo tài chính định kỳ | Kỳ báo cáo + {Khóa học + Doanh thu} + {Hình thức + Doanh thu} + Tổng doanh thu + Tổng công nợ + So sánh kỳ trước (S) | 3.4 |
| Phiếu điểm danh buổi học | Trạng thái học viên trong một buổi | Mã lớp + Số thứ tự buổi + {Mã HV + Trạng thái [x \| M \| P \| K] + (Ghi chú)} | Giáo viên |
| Chuyên cần | Tình hình đi học gửi học viên | Mã lớp + Số buổi đã học + Số buổi có mặt + Tỷ lệ chuyên cần (S) + (Cảnh báo) | 4.1 |
| Điểm số, nhận xét | Điểm giáo viên nhập | Mã lớp + Loại điểm + {Mã HV + Điểm + (Nhận xét)} | Giáo viên |
| Kết quả học tập | Kết quả cuối khóa (02_thu_thap.md, mục 4.2.4) | Kết quả học tập | 4.3 |
| Chứng nhận | Giấy chứng nhận (02_thu_thap.md, mục 4.2.5) | Chứng nhận + Họ tên + Ngày sinh + Tên KH + Xếp loại + Điểm tổng kết | 4.4 |
| Tài khoản, phân quyền, cấu hình | Thao tác quản trị | Tài khoản + Vai trò + {Quyền} + (Tham số cấu hình) | Quản trị viên |
| Nhật ký hệ thống | Vết thao tác để kiểm tra | {Nhật ký} | 5.1 |
| Thông báo | Tin gửi học viên, phụ huynh | Loại + Nội dung + Thời gian gửi | 5.2 |
| Yêu cầu báo cáo tổng hợp | Tham số của báo cáo MIS | Loại báo cáo + Từ ngày + Đến ngày + (Khóa học / Lớp / Giáo viên) | Giám đốc |
| Báo cáo tuyển sinh | Số học viên mới | Kỳ + {Khóa học + Số học viên mới} + Tổng + So sánh kỳ trước (S) | 5.3 |
| Báo cáo lớp học | Tình trạng lớp | {Mã lớp + Sĩ số tối đa + Số đăng ký + Tỷ lệ lấp đầy (S) + Trạng thái} + Số lớp mở + Số lớp hủy | 5.3 |
| Báo cáo kết quả học tập | Kết quả đào tạo cho giám đốc | Kỳ + {Khóa học + Tỷ lệ đạt (S) + Phân bố xếp loại (S)} | 5.4 |
| Báo cáo chuyên cần, kết quả, giảng dạy | Báo cáo cho QL đào tạo | {Mã lớp + Tỷ lệ chuyên cần TB (S) + Tỷ lệ đạt (S)} + {Học viên nghỉ nhiều} + {Mã GV + Số buổi đã dạy (S)} | 5.4 |

### 4.3 Luồng nội bộ giữa các chức năng con

| Tên gọi | Ý nghĩa | Cấu trúc dữ liệu | Nguồn gốc |
|---|---|---|---|
| Lớp đã mở | Lớp đủ điều kiện để xếp lịch | Mã lớp + Mã GV + Ngày khai giảng + Lịch tuần + Số buổi | 2.1 |
| Tổng công nợ | Số liệu công nợ cho báo cáo doanh thu | Tổng còn nợ + Tổng quá hạn + Số học viên nợ | 3.3 |
| Danh sách học viên đạt | Học viên được cấp chứng nhận | Mã lớp + {Mã HV + Xếp loại + Điểm tổng kết} | 4.3 |

### 4.4 Luồng giữa xử lý và kho

Các luồng này là tập con của cấu trúc kho ở mục 4.1. Chiều *Ghi* = xử lý → kho; *Đọc* = kho → xử lý.

| Tên gọi | Kho | Chiều | Xử lý | Cấu trúc (lấy từ mục 4.1) |
|---|---|---|---|---|
| Hồ sơ hiện có | D1 | Đọc | 1.1 | Học viên, Phụ huynh (để kiểm tra trùng) |
| Hồ sơ học viên / Hồ sơ giáo viên / Khóa học / Phòng học | D1 | Ghi | 1.1 / 1.2 / 1.3 / 1.4 | Bản ghi tương ứng |
| Khóa học, giáo viên | D1 | Đọc | 2.1 | Khóa học, Giáo viên |
| Giáo viên, phòng học | D1 | Đọc | 2.3 | Giáo viên, Phòng học |
| Học viên | D1 | Đọc | 2.2 | Học viên |
| Sĩ số đăng ký / Sĩ số lớp | D2 | Đọc | 2.1 / 2.2 | Mã lớp + Sĩ số tối đa + Số đăng ký (S) |
| Lớp học | D2 | Ghi | 2.1 | Lớp |
| Lịch đã xếp | D2 | Đọc | 2.3 | {Buổi học} trong cùng ngày |
| Lịch học (buổi học) | D2 | Ghi | 2.3 | {Buổi học} |
| Đăng ký / Đăng ký cập nhật | D2 | Ghi | 2.2 / 2.4 | Đăng ký |
| Đăng ký, sĩ số, buổi đã học | D2 | Đọc | 2.4 | Đăng ký + Sĩ số lớp đích + số Buổi học đã dạy |
| Tình trạng đóng học phí | D3 | Đọc | 2.2, 2.4 | Số phiếu ĐK + Phải nộp + Đã thu (S) |
| Học phí khóa học | D1 | Đọc | 3.1 | Mã KH + Học phí |
| Đăng ký học / Trạng thái đăng ký | D2 | Đọc | 3.1 / 3.3 | Đăng ký (kèm lịch sử để xét QT01) |
| Ưu đãi, học phí phải thu | D3 | Ghi | 3.1 | Ưu đãi + Học phí phải thu |
| Học phí phải thu, số đã thu | D3 | Đọc | 3.2, 3.3 | Học phí phải thu + tổng tiền các Phiếu thu theo đăng ký |
| Phiếu thu mới | D3 | Ghi | 3.2 | Phiếu thu |
| Số đã thu theo kỳ | D3 | Đọc | 3.4 | {Phiếu thu} trong kỳ |
| Danh sách lớp, buổi học / Danh sách lớp | D2 | Đọc | 4.1 / 4.2 | Đăng ký của lớp + Buổi học |
| Điểm danh đã ghi | D4 | Ghi | 4.1 | {Điểm danh} |
| Điểm thành phần | D4 | Ghi | 4.2 | {Điểm thành phần} |
| Trọng số điểm khóa học | D1 | Đọc | 4.3 | TS chuyên cần + TS giữa kỳ + TS cuối kỳ |
| Điểm danh, điểm thành phần | D4 | Đọc | 4.3 | {Điểm danh} + {Điểm thành phần} của lớp |
| Kết quả học tập / Chứng nhận | D4 | Ghi | 4.3 / 4.4 | Kết quả học tập / Chứng nhận |
| Hồ sơ học viên, giáo viên | D1 | Đọc | 5.1 | Học viên, Phụ huynh, Giáo viên (để tạo tài khoản) |
| Quyền truy cập, nhật ký | D5 | Đọc | 5.1 | Tài khoản + Nhật ký |
| Tài khoản, nhật ký | D5 | Ghi | 5.1 | Tài khoản + Nhân viên + Nhật ký |
| Liên hệ học viên, phụ huynh | D1 | Đọc | 5.2 | Mã HV + Điện thoại + Email + {Phụ huynh} |
| Lịch học thay đổi | D2 | Đọc | 5.2 | {Buổi học} có thay đổi |
| Công nợ | D3 | Đọc | 5.2 | Khoản sắp đến hạn hoặc quá hạn |
| Chuyên cần, kết quả | D4 | Đọc | 5.2, 5.4 | Tỷ lệ chuyên cần + Kết quả học tập |
| Thông báo đã gửi | D5 | Ghi | 5.2 | Thông báo |
| Hồ sơ học viên / Giáo viên | D1 | Đọc | 5.3 / 5.4 | Học viên / Giáo viên |
| Lớp, đăng ký / Lớp, buổi học | D2 | Đọc | 5.3 / 5.4 | Lớp + Đăng ký / Lớp + Buổi học |

### 4.5 Phần tử dữ liệu tính toán (thuộc tính thứ sinh)

| Tên gọi | Ý nghĩa | Công thức | Nguồn gốc |
|---|---|---|---|
| Phải nộp | Học phí sau ưu đãi | Học phí gốc × (1 − Tỷ lệ giảm) | 3.1 |
| Tỷ lệ giảm | Mức giảm được hưởng | Theo bảng quyết định 3.1, tối đa 15% | 3.1 |
| Đã thu | Tổng đã nộp cho một đăng ký | Tổng Số tiền trên các phiếu thu của đăng ký, cộng cả phiếu thu của Đăng ký gốc (nếu có) | 3.2 |
| Còn nợ / Công nợ | Số còn phải nộp | Phải nộp − Đã thu | 3.2, 3.3 |
| Hiệu lực đăng ký | Đăng ký đã được xác nhận | Trạng thái = Đã đăng ký và Đã thu ≥ 50% × Phải nộp | 2.2 |
| Số đăng ký của lớp | Số chỗ đã có người | Số Đăng ký của lớp có Trạng thái ∉ {Chuyển lớp, Nghỉ học} | 2.1, 2.2 |
| Tỷ lệ lấp đầy | Mức sử dụng lớp | Số đăng ký / Sĩ số tối đa | 5.3 |
| Tỷ lệ chuyên cần | Mức đi học đều | Số buổi (x hoặc M) / Số buổi đã học | 4.1 |
| Điểm chuyên cần | Điểm cho chuyên cần | Tỷ lệ chuyên cần × 10 | 4.3 |
| Điểm tổng kết | Điểm cuối khóa | TS_cc × Điểm chuyên cần + TS_gk × Giữa kỳ + TS_ck × Cuối kỳ, làm tròn 1 chữ số thập phân | 4.3 |
| Doanh thu kỳ | Tiền thu được trong kỳ | Tổng Tổng tiền của các phiếu thu có Ngày thu trong kỳ | 3.4 |
| Số buổi đã dạy | Khối lượng giảng dạy | Số Buổi học có Trạng thái = Đã dạy, theo Mã GV | 5.4 |

---

## 5. Đầu ra cho các bước sau

| Bước | Dùng gì từ tài liệu này |
|---|---|
| 2.5 Báo cáo phân tích | Toàn bộ tài liệu này là chương "Đặc tả xử lý" |
| 3.1 Thực thể, thuộc tính | Mục 4.1 (cấu trúc kho): thuộc tính #, nhóm lặp `{ }`, thuộc tính thứ sinh (S) cần loại bỏ trước khi chuẩn hóa |
| 3.3 Chuẩn hóa | Nhóm lặp trong *Đăng ký*, *Phiếu thu*, *Học phí phải thu* là ví dụ 1NF; *Học viên* chứa {Mã PH + Quan hệ} là quan hệ N–N |
| 3.4 CSDL | Các miền giá trị `[ a \| b ]` → ràng buộc CHECK; mục 1 và 3 → ràng buộc nghiệp vụ, trigger hoặc xử lý ở tầng ứng dụng |
| 4 Demo / kiểm thử | Mỗi quy tắc (R1–R8, R1–R5) là một test case; các ví dụ đã đối chiếu với chứng từ mẫu |
