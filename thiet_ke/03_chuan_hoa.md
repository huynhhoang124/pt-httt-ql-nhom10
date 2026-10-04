# 3.3 Chuẩn hóa dữ liệu – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn Thiết kế hệ thống (KE_HOACH.md, mục 3.3). Làm theo bài giảng mục 5.4. Mỗi chứng từ được chuẩn hóa theo đúng 3 bước của *Ví dụ 3 – Hóa đơn bán hàng*:
1. Liệt kê thuộc tính, đánh dấu thuộc tính lặp (R) và thứ sinh (S).
2. Loại thuộc tính thứ sinh và thuộc tính ít ý nghĩa.
3. Chuẩn hóa: 1NF → 2NF → 3NF.

Sau đó **trộn các bảng thực thể** mô tả cùng một đối tượng.

Chứng từ chuẩn hóa: 3 chứng từ chính theo KE_HOACH là **Phiếu đăng ký**, **Phiếu thu**, **Bảng điểm lớp** (mục 3–5). Hai chứng từ còn lại là Sổ điểm danh và Chứng nhận, làm rút gọn ở mục 6 để phủ hết kho D2, D4. Mẫu chứng từ ở `phan_tich/02_thu_thap.md` mục 4.2.

**Yêu cầu của bước này** (KE_HOACH): kết quả 3NF phải trùng với tập bảng đã thiết kế ở 3.1–3.2. Việc đối chiếu ở mục 8 được kiểm tra tự động bằng `thiet_ke/kiem_tra_chuan_hoa.py`.

---

## 1. Cơ sở lý thuyết (bài giảng mục 5.4)

**Phụ thuộc hàm.** *X → Y* nếu mỗi giá trị của X xác định duy nhất một giá trị của Y. Bài giảng dùng 3 dạng:

| Dạng | Định nghĩa | Ví dụ ở hệ thống này |
|---|---|---|
| Phụ thuộc hàm | X → Y | Mã HV → Họ tên HV |
| Phụ thuộc hàm toàn bộ | (X1, X2) → Y, và không một phần nào của (X1, X2) đủ để xác định Y | (Mã HV, Mã lớp, Loại điểm) → Điểm |
| Phụ thuộc hàm bắc cầu | X → Y và Y → Z | Số phiếu ĐK → Mã HV → Họ tên HV |

**Các dạng chuẩn:**

| Dạng chuẩn | Điều kiện | Cách tách |
|---|---|---|
| 1NF | Không có thuộc tính lặp; mọi giá trị đều sơ cấp | Tách nhóm lặp thành thực thể mới. Khóa = định danh của thực thể gốc + một thuộc tính phù hợp với nhóm lặp |
| 2NF | Đã 1NF, và mọi thuộc tính không khóa phụ thuộc **toàn bộ** vào khóa | Tách các thuộc tính chỉ phụ thuộc **một phần** khóa. Khóa của bảng mới là phần khóa đó |
| 3NF | Đã 2NF, và không có phụ thuộc **bắc cầu** từ khóa tới thuộc tính khác | Khóa → B → C thì tách C sang thực thể mới có khóa là B |

**Quy ước trình bày** (giống Ví dụ 3 của bài giảng):
- Thực thể trung gian được đánh số theo bước, vd "Phiếu đăng ký (1)" → "Phiếu đăng ký (2)". Thực thể đã đạt 3NF thì không có số.
- `#` đánh dấu thuộc tính định danh; `(R)` thuộc tính lặp; `(S)` thuộc tính thứ sinh; `{ }` nhóm lặp.
- Mã KH, Mã PH, Mã NV không in trên chứng từ (chứng từ chỉ in tên). Mình thêm các mã này làm định danh, giống bài giảng thêm "Mã hàng hóa" khi tách nhóm lặp: họ tên có thể trùng nên không dùng làm khóa được.

---

## 2. Vì sao phải chuẩn hóa – dị thường khi lưu nguyên chứng từ

Giả sử phiếu đăng ký được lưu nguyên dạng như trên giấy vào một bảng duy nhất "Phiếu đăng ký (1)" (khóa: Số phiếu ĐK). Tương tự Ví dụ 2 của bài giảng, bảng này gặp các lỗi sau:

| Lỗi | Biểu hiện với dữ liệu mẫu |
|---|---|
| Dư thừa | HV0412 học 3 khóa trong 2 năm thì họ tên, ngày sinh, điện thoại, thông tin phụ huynh bị chép lại 3 lần. Tên khóa "IELTS Kids Foundation" và lịch học của lớp IEK-2609 bị chép lại trên mọi phiếu có lớp này. |
| Dị thường khi sửa | Phụ huynh Nguyễn Văn Bình đổi số điện thoại thì phải sửa mọi phiếu của con. Sót một phiếu là dữ liệu mâu thuẫn. Đây chính là vấn đề V1 "dữ liệu nhập lặp, không khớp" ở 2.1. |
| Dị thường khi thêm | Lớp vừa mở mà chưa có ai đăng ký thì không có chỗ ghi lịch học và ngày khai giảng, vì khóa Số phiếu ĐK không được rỗng. |
| Dị thường khi xóa | Hủy phiếu duy nhất của một học viên thì mất luôn hồ sơ và kết quả kiểm tra đầu vào của học viên đó. |

Phiếu thu và bảng điểm gặp các lỗi tương tự: tên khóa, tên học viên, trọng số điểm bị chép lặp trên từng phiếu.

---

## 3. Chứng từ 1 – Phiếu đăng ký (DK2026-0158)

### Bước 1 – Liệt kê thuộc tính, xác định R và S

Thực thể ban đầu là **"Phiếu đăng ký (1)"**:

Số phiếu ĐK, Ngày ĐK, Mã HV, Họ tên HV, Ngày sinh, Giới tính, Điện thoại HV, Email HV, Địa chỉ HV, Trình độ đầu vào, Ngày kiểm tra, Mã PH (R), Họ tên PH (R), Quan hệ (R), Điện thoại PH (R), Stt (R), Mã lớp (R), Mã KH (R), Tên khóa học (R), Thứ (R), Giờ bắt đầu (R), Giờ kết thúc (R), Ngày khai giảng (R), Học phí (R), Mã UĐ (R), Tên UĐ (R), Tỷ lệ giảm UĐ (R), Tổng học phí (S), Giảm (S), Phải nộp (S), Mã NV, Họ tên NV, Chữ ký.

Giải thích các chỗ đánh dấu:
- **Phụ huynh là nhóm lặp.** Mẫu chỉ có bố, nhưng một học viên có thể có cả bố lẫn mẹ.
- **Lịch học "T3-T5 18:00-19:30" không nguyên tố.** Một ô chứa nhiều thứ và một khung giờ, nên được tách thành Thứ (R), Giờ bắt đầu, Giờ kết thúc. Đây là nhóm lặp **lồng** trong dòng lớp.
- **Ưu đãi in một lần ở cuối phiếu, nhưng về ý nghĩa thuộc từng dòng lớp.** Ưu đãi "đóng một lần" (QT03) xét theo từng lớp, và một dòng có thể hưởng nhiều ưu đãi cộng dồn (QT04). Vì vậy {Mã UĐ, Tên UĐ, Tỷ lệ giảm} cũng là nhóm lặp lồng trong dòng lớp. Bài giảng (mục 5.4.1) nhấn mạnh việc xem xét kỹ ý nghĩa từng thuộc tính; đây là một trường hợp như vậy.

Cấu trúc nhóm lặp của phiếu:

```
Phiếu đăng ký (1) = #Số phiếu ĐK + … + {Phụ huynh} + {Dòng lớp + {Lịch} + {Ưu đãi}} + …
```

Khóa: **Số phiếu ĐK**, vì nó đủ để phân biệt các phiếu với nhau.

### Bước 2 – Loại thuộc tính thứ sinh và thuộc tính ít ý nghĩa

- Loại **Tổng học phí, Giảm, Phải nộp** (S): cộng hoặc nhân ra được từ Học phí và Tỷ lệ giảm.
- Loại **Stt**: ít ý nghĩa, giống bài giảng bỏ "Stt" và "Liên số".
- Loại **Chữ ký**: chỉ có trên bản giấy.

Còn lại:

**Phiếu đăng ký (1)**: #Số phiếu ĐK, Ngày ĐK, Mã HV, Họ tên HV, Ngày sinh, Giới tính, Điện thoại HV, Email HV, Địa chỉ HV, Trình độ đầu vào, Ngày kiểm tra, {Mã PH, Họ tên PH, Quan hệ, Điện thoại PH}, {Mã lớp, Mã KH, Tên khóa học, Ngày khai giảng, Học phí, {Thứ, Giờ bắt đầu, Giờ kết thúc}, {Mã UĐ, Tên UĐ, Tỷ lệ giảm UĐ}}, Mã NV, Họ tên NV.

**Phụ thuộc hàm** (dùng cho các bước sau):

| # | Phụ thuộc hàm | Căn cứ |
|---|---|---|
| F1 | Số phiếu ĐK → Ngày ĐK, Mã HV, Mã NV | Mỗi phiếu lập một ngày, cho một học viên, do một nhân viên tiếp nhận |
| F2 | Mã HV → Họ tên HV, Ngày sinh, Giới tính, Điện thoại HV, Email HV, Địa chỉ HV, Trình độ đầu vào, Ngày kiểm tra | Hồ sơ học viên; trình độ là của lần kiểm tra gần nhất |
| F3 | Mã NV → Họ tên NV | |
| F4 | Mã PH → Họ tên PH, Điện thoại PH | |
| F5 | (Mã HV, Mã PH) → Quan hệ | Quan hệ là của cặp học viên – phụ huynh |
| F6 | Mã lớp → Mã KH, Ngày khai giảng | Mỗi lớp mở theo một khóa |
| F7 | Mã KH → Tên khóa học | |
| F8 | (Mã lớp, Thứ) → Giờ bắt đầu, Giờ kết thúc | Mỗi thứ, một lớp học một ca (giả định G2 ở 3.1) |
| F9 | (Số phiếu ĐK, Mã lớp) → Học phí | Học phí in trên phiếu là học phí **tại ngày đăng ký**, khác học phí hiện hành của khóa (3.1 mục 5) |
| F10 | Mã UĐ → Tên UĐ, Tỷ lệ giảm UĐ | |
| F11 | (Mã HV, Mã lớp) → Số phiếu ĐK | Mỗi học viên chỉ có một đăng ký ở mỗi lớp (2.2). Dùng ở bước trộn (mục 7) |

### Bước 3 – Chuẩn hóa

**a) Chuẩn hóa mức 1 (1NF).** "Phiếu đăng ký (1)" chứa 2 nhóm lặp trực tiếp, và nhóm dòng lớp lại chứa 2 nhóm lồng. Tách lần lượt từng nhóm. Khóa của mỗi bảng mới = khóa của bảng gốc + một thuộc tính phân biệt các phần tử trong nhóm lặp.

| Thực thể sau 1NF | Thuộc tính |
|---|---|
| Phiếu đăng ký (2) | #Số phiếu ĐK, Ngày ĐK, Mã HV, Họ tên HV, Ngày sinh, Giới tính, Điện thoại HV, Email HV, Địa chỉ HV, Trình độ đầu vào, Ngày kiểm tra, Mã NV, Họ tên NV |
| Phụ huynh (1) | #Số phiếu ĐK, #Mã PH, Họ tên PH, Điện thoại PH, Quan hệ |
| Dòng đăng ký (1) | #Số phiếu ĐK, #Mã lớp, Mã KH, Tên khóa học, Ngày khai giảng, Học phí |
| Lịch của dòng (1) | #Số phiếu ĐK, #Mã lớp, #Thứ, Giờ bắt đầu, Giờ kết thúc |
| Ưu đãi của dòng (1) | #Số phiếu ĐK, #Mã lớp, #Mã UĐ, Tên UĐ, Tỷ lệ giảm UĐ |

**b) Chuẩn hóa mức 2 (2NF).** "Phiếu đăng ký (2)" có khóa đơn nên đã đạt 2NF. Bốn bảng còn lại có khóa kép, cần tìm các thuộc tính chỉ phụ thuộc **một phần** khóa:

| Bảng | Phụ thuộc một phần khóa | Tách thành |
|---|---|---|
| Phụ huynh (1) | Họ tên PH, Điện thoại PH chỉ phụ thuộc Mã PH (F4) | **Phụ huynh** (#Mã PH, Họ tên PH, Điện thoại PH) và Giám hộ (1) (#Số phiếu ĐK, #Mã PH, Quan hệ) |
| Dòng đăng ký (1) | Mã KH, Tên khóa học, Ngày khai giảng chỉ phụ thuộc Mã lớp (F6, F7) | Lớp học (1) (#Mã lớp, Mã KH, Tên khóa học, Ngày khai giảng) và **Dòng đăng ký** (#Số phiếu ĐK, #Mã lớp, Học phí). Học phí ở lại vì phụ thuộc toàn bộ khóa (F9) |
| Lịch của dòng (1) | Giờ bắt đầu, Giờ kết thúc chỉ phụ thuộc (Mã lớp, Thứ) (F8) | Lịch tuần (1) (#Mã lớp, #Thứ, Giờ bắt đầu, Giờ kết thúc). Phần còn lại (Số phiếu ĐK, Mã lớp, Thứ) không còn thuộc tính không khóa và chỉ là phép ghép của Dòng đăng ký với Lịch tuần, nên **bỏ** vì thừa |
| Ưu đãi của dòng (1) | Tên UĐ, Tỷ lệ giảm UĐ chỉ phụ thuộc Mã UĐ (F10) | **Ưu đãi** (#Mã UĐ, Tên UĐ, Tỷ lệ giảm) và **Áp dụng ưu đãi** (1) (#Số phiếu ĐK, #Mã lớp, #Mã UĐ) |

**c) Chuẩn hóa mức 3 (3NF).** Tìm các phụ thuộc bắc cầu:

| Bảng | Phụ thuộc bắc cầu | Tách thành |
|---|---|---|
| Phiếu đăng ký (2) | Số phiếu ĐK → Mã HV → Họ tên HV, Ngày sinh, … (F1, F2); Số phiếu ĐK → Mã NV → Họ tên NV (F1, F3) | **Học viên** (#Mã HV, Họ tên HV, Ngày sinh, Giới tính, Điện thoại HV, Email HV, Địa chỉ HV, Trình độ đầu vào, Ngày kiểm tra); **Nhân viên** (#Mã NV, Họ tên NV); **Phiếu đăng ký** (#Số phiếu ĐK, Ngày ĐK, Mã HV, Mã NV) |
| Lớp học (1) | Mã lớp → Mã KH → Tên khóa học (F6, F7) | **Khóa học** (#Mã KH, Tên khóa học); **Lớp học** (#Mã lớp, Mã KH, Ngày khai giảng) |
| Giám hộ (1) | Quan hệ thật ra phụ thuộc vào cặp (học viên, phụ huynh) (F5). Số phiếu ĐK chỉ có mặt vì Số phiếu ĐK → Mã HV (F1), nên có bắc cầu (Số phiếu ĐK, Mã PH) → (Mã HV, Mã PH) → Quan hệ. Nếu giữ nguyên, cặp HV–PH sẽ lặp lại trên mọi phiếu của học viên | **Học viên – Phụ huynh** (#Mã HV, #Mã PH, Quan hệ) |
| Lịch tuần (1) | Không có bắc cầu trong phạm vi phiếu (chưa biết thời lượng buổi) | Giữ. Ở bước trộn, Giờ kết thúc sẽ thành thứ sinh (mục 7) |

**Kết quả chuẩn hóa Phiếu đăng ký (11 thực thể):**

| Thực thể | Thuộc tính |
|---|---|
| Phiếu đăng ký | #Số phiếu ĐK, Ngày ĐK, Mã HV, Mã NV |
| Học viên | #Mã HV, Họ tên HV, Ngày sinh, Giới tính, Điện thoại HV, Email HV, Địa chỉ HV, Trình độ đầu vào, Ngày kiểm tra |
| Nhân viên | #Mã NV, Họ tên NV |
| Phụ huynh | #Mã PH, Họ tên PH, Điện thoại PH |
| Học viên – Phụ huynh | #Mã HV, #Mã PH, Quan hệ |
| Dòng đăng ký | #Số phiếu ĐK, #Mã lớp, Học phí |
| Lớp học | #Mã lớp, Mã KH, Ngày khai giảng |
| Khóa học | #Mã KH, Tên khóa học |
| Lịch tuần | #Mã lớp, #Thứ, Giờ bắt đầu, Giờ kết thúc |
| Ưu đãi | #Mã UĐ, Tên UĐ, Tỷ lệ giảm |
| Áp dụng ưu đãi | #Số phiếu ĐK, #Mã lớp, #Mã UĐ |

---

## 4. Chứng từ 2 – Phiếu thu (PT2026-0731)

### Bước 1 – Liệt kê thuộc tính, xác định R và S

**"Phiếu thu (1)"**: Số PT, Ngày thu, Họ tên người nộp, Mã HV, Họ tên HV, Số phiếu ĐK, Stt (R), Mã lớp (R), Mã KH (R), Tên khóa học (R), Học phí (R), Giảm (R) & (S), Phải nộp (R) & (S), Nộp lần này (R), Tổng nộp (S), Viết bằng chữ (S), Đợt, Còn nợ (S), Hạn đợt 2, Hình thức, Mã NV, Họ tên NV.

Khóa: **Số PT**.

### Bước 2 – Loại thuộc tính thứ sinh, ít ý nghĩa; xem lại ý nghĩa thuộc tính

Loại **Giảm, Phải nộp, Tổng nộp, Viết bằng chữ, Còn nợ** (S) và **Stt**.

Khi xem lại **ý nghĩa** từng thuộc tính còn lại trên phiếu mẫu, ta thấy 3 thuộc tính in ở đầu phiếu thật ra thuộc về từng dòng:

| Thuộc tính | Trên chứng từ | Ý nghĩa thật | Xử lý |
|---|---|---|---|
| Đợt, Nộp lần này | "Đợt: 1" ghi ở đầu phiếu | Dòng IEK-2609 nộp 4.320.000 = **cả đợt 1 lẫn đợt 2**, còn dòng GT-2604 chỉ nộp đợt 1. Một dòng lớp có thể nộp nhiều đợt | {Đợt, Số tiền} là nhóm lặp **lồng** trong dòng lớp. "Nộp lần này" được tách theo đợt, đổi tên thành Số tiền |
| Hạn đợt 2 | Một giá trị ở đầu phiếu | Là hạn của **một đợt cụ thể của một lớp cụ thể** (GT-2604, đợt 2) | Đổi tên thành Hạn đóng, thuộc nhóm {Đợt} |
| Mã HV, Họ tên HV, Số phiếu ĐK | Đầu phiếu, vì mẫu chỉ có 1 học viên | Một phiếu thu có thể thu cho nhiều học viên, vd phụ huynh nộp cho 2 con (3.1, E17) | Thuộc nhóm {Dòng lớp} |

Còn lại:

**Phiếu thu (1)**: #Số PT, Ngày thu, Người nộp, Hình thức, Mã NV, Họ tên NV, {Mã HV, Họ tên HV, Số phiếu ĐK, Mã lớp, Mã KH, Tên khóa học, Học phí, {Đợt, Số tiền, Hạn đóng}}.

**Phụ thuộc hàm:**

| # | Phụ thuộc hàm | Căn cứ |
|---|---|---|
| G1 | Số PT → Ngày thu, Người nộp, Hình thức, Mã NV | |
| G2 | Mã NV → Họ tên NV; Mã HV → Họ tên HV | |
| G3 | Mã lớp → Mã KH → Tên khóa học | Như F6, F7 |
| G4 | (Mã HV, Mã lớp) → Số phiếu ĐK, Học phí | Một đăng ký (F11) có một học phí đã chốt |
| G5 | (Mã HV, Mã lớp, Đợt) → Hạn đóng | Hạn là của từng đợt (QT05) |
| G6 | (Số PT, Mã HV, Mã lớp, Đợt) → Số tiền | Số tiền nộp **trên phiếu này** cho đợt đó; một đợt có thể nộp qua nhiều phiếu |

### Bước 3 – Chuẩn hóa

**a) 1NF.** Tách nhóm {Dòng lớp}, với khóa = Số PT + (Mã HV, Mã lớp). Tách tiếp nhóm lồng {Đợt}:

| Thực thể sau 1NF | Thuộc tính |
|---|---|
| Phiếu thu (2) | #Số PT, Ngày thu, Người nộp, Hình thức, Mã NV, Họ tên NV |
| Dòng phiếu thu (1) | #Số PT, #Mã HV, #Mã lớp, Họ tên HV, Số phiếu ĐK, Mã KH, Tên khóa học, Học phí |
| Đợt nộp (1) | #Số PT, #Mã HV, #Mã lớp, #Đợt, Số tiền, Hạn đóng |

**b) 2NF.**

| Bảng | Phụ thuộc một phần khóa | Tách thành |
|---|---|---|
| Dòng phiếu thu (1) | Họ tên HV chỉ phụ thuộc Mã HV (G2); Mã KH, Tên khóa học chỉ phụ thuộc Mã lớp (G3); Số phiếu ĐK, Học phí chỉ phụ thuộc (Mã HV, Mã lớp) (G4) | **Học viên** (#Mã HV, Họ tên HV); Lớp học (1) (#Mã lớp, Mã KH, Tên khóa học); **Đăng ký – học phí** (#Mã HV, #Mã lớp, Số phiếu ĐK, Học phí). Phần còn lại (Số PT, Mã HV, Mã lớp) không còn thuộc tính không khóa và đã nằm trong khóa của Đợt nộp, nên **bỏ** |
| Đợt nộp (1) | Hạn đóng chỉ phụ thuộc (Mã HV, Mã lớp, Đợt), không phụ thuộc Số PT (G5) | **Đợt học phí** (#Mã HV, #Mã lớp, #Đợt, Hạn đóng) và **Chi tiết phiếu thu** (#Số PT, #Mã HV, #Mã lớp, #Đợt, Số tiền) (G6) |

**c) 3NF.**

| Bảng | Phụ thuộc bắc cầu | Tách thành |
|---|---|---|
| Phiếu thu (2) | Số PT → Mã NV → Họ tên NV | **Nhân viên** (#Mã NV, Họ tên NV); **Phiếu thu** (#Số PT, Ngày thu, Người nộp, Hình thức, Mã NV) |
| Lớp học (1) | Mã lớp → Mã KH → Tên khóa học | **Lớp học** (#Mã lớp, Mã KH); **Khóa học** (#Mã KH, Tên khóa học) |
| Đăng ký – học phí | Không có (Số phiếu ĐK và Học phí đều phụ thuộc trực tiếp vào khóa) | Giữ |

**Kết quả chuẩn hóa Phiếu thu (8 thực thể):**

| Thực thể | Thuộc tính |
|---|---|
| Phiếu thu | #Số PT, Ngày thu, Người nộp, Hình thức, Mã NV |
| Chi tiết phiếu thu | #Số PT, #Mã HV, #Mã lớp, #Đợt, Số tiền |
| Đợt học phí | #Mã HV, #Mã lớp, #Đợt, Hạn đóng |
| Đăng ký – học phí | #Mã HV, #Mã lớp, Số phiếu ĐK, Học phí |
| Học viên | #Mã HV, Họ tên HV |
| Nhân viên | #Mã NV, Họ tên NV |
| Lớp học | #Mã lớp, Mã KH |
| Khóa học | #Mã KH, Tên khóa học |

Kiểm tra lại bằng số liệu mẫu: "Chi tiết phiếu thu" của PT2026-0731 có 3 dòng (IEK-2609 đợt 1, IEK-2609 đợt 2, GT-2604 đợt 1), tổng 5.400.000. "Đợt học phí" (HV0412, GT-2604, 2) giữ Hạn đóng 17/10/2026. Hai số này khớp với chứng từ (3.1 mục 6.2).

---

## 5. Chứng từ 3 – Bảng điểm lớp (IEK-2609)

### Bước 1 – Liệt kê thuộc tính, xác định R và S

**"Bảng điểm (1)"**: Mã lớp, Mã KH, Tên khóa học, Trình độ đầu vào, Trình độ đầu ra, Mã GV, Họ tên GV, TS chuyên cần, TS giữa kỳ, TS cuối kỳ, Mã HV (R), Họ tên HV (R), Điểm chuyên cần (R) & (S), Điểm giữa kỳ (R), Điểm cuối kỳ (R), Điểm tổng kết (R) & (S), Xếp loại (R) & (S), Kết quả (R) & (S), Nhận xét.

Khóa: **Mã lớp** (mỗi lớp có một bảng điểm).

### Bước 2 – Loại thuộc tính thứ sinh; xem lại ý nghĩa thuộc tính

- Loại **Điểm chuyên cần** (S): tính được từ sổ điểm danh (QT11).
- **Điểm tổng kết, Xếp loại, Kết quả** là S tại lúc tính (QT13–QT15). Nhưng khi QL đào tạo đã duyệt, chúng là **kết quả chính thức** và được in lên chứng nhận. Nhóm **giữ** 3 thuộc tính này theo nguyên tắc "chốt" ở 3.1 mục 5, đánh dấu *(chốt)*. Đây là ngoại lệ duy nhất của chứng từ này so với bước 2 của bài giảng, và có lý do.
- **Điểm giữa kỳ, Điểm cuối kỳ là một nhóm lặp theo Loại điểm**, giống "Kỹ năng 1, Kỹ năng 2" trong Bảng 4.4 của bài giảng. Lý do: chúng cùng là "điểm của một cột kiểm tra", được nhập ở 2 thời điểm khác nhau (chức năng 4.2), và mỗi cột có nhận xét riêng (từ điển dữ liệu: luồng "Điểm số, nhận xét"). Nhận xét in một chỗ ở cuối bảng, nhưng thực chất là nhận xét của giáo viên cho từng học viên ở từng cột điểm.

Còn lại:

**Bảng điểm (1)**: #Mã lớp, Mã KH, Tên khóa học, Trình độ đầu vào, Trình độ đầu ra, Mã GV, Họ tên GV, TS chuyên cần, TS giữa kỳ, TS cuối kỳ, {Mã HV, Họ tên HV, Điểm tổng kết (chốt), Xếp loại (chốt), Kết quả (chốt), {Loại điểm, Điểm, Nhận xét}}.

**Phụ thuộc hàm:**

| # | Phụ thuộc hàm | Căn cứ |
|---|---|---|
| H1 | Mã lớp → Mã KH, Mã GV | Lớp mở theo một khóa, có một giáo viên phụ trách |
| H2 | Mã KH → Tên khóa học, Trình độ đầu vào, Trình độ đầu ra, TS chuyên cần, TS giữa kỳ, TS cuối kỳ | Trọng số đặt theo từng khóa học (QT13, chức năng 1.3) |
| H3 | Mã GV → Họ tên GV; Mã HV → Họ tên HV | |
| H4 | (Mã lớp, Mã HV) → Điểm tổng kết, Xếp loại, Kết quả | Kết quả của một học viên trong một lớp |
| H5 | (Mã lớp, Mã HV, Loại điểm) → Điểm, Nhận xét | |
| H6 | Điểm tổng kết, Tỷ lệ chuyên cần → Xếp loại, Kết quả | Đây là **quy tắc tính** (QT14, QT15), không phải dữ liệu |

### Bước 3 – Chuẩn hóa

**a) 1NF.** Tách nhóm {học viên} với khóa (Mã lớp, Mã HV), rồi tách nhóm lồng {điểm} với khóa (Mã lớp, Mã HV, Loại điểm):

| Thực thể sau 1NF | Thuộc tính |
|---|---|
| Bảng điểm (2) | #Mã lớp, Mã KH, Tên khóa học, Trình độ đầu vào, Trình độ đầu ra, Mã GV, Họ tên GV, TS chuyên cần, TS giữa kỳ, TS cuối kỳ |
| Kết quả (1) | #Mã lớp, #Mã HV, Họ tên HV, Điểm tổng kết, Xếp loại, Kết quả |
| Điểm (1) | #Mã lớp, #Mã HV, #Loại điểm, Điểm, Nhận xét |

**b) 2NF.**

| Bảng | Phụ thuộc một phần khóa | Tách thành |
|---|---|---|
| Kết quả (1) | Họ tên HV chỉ phụ thuộc Mã HV (H3) | **Học viên** (#Mã HV, Họ tên HV); **Kết quả học tập** (#Mã lớp, #Mã HV, Điểm tổng kết, Xếp loại, Kết quả) |
| Điểm (1) | Không có: Điểm và Nhận xét phụ thuộc toàn bộ khóa (H5) | Giữ, đặt tên **Điểm thành phần** |

**c) 3NF.**

| Bảng | Phụ thuộc bắc cầu | Tách thành |
|---|---|---|
| Bảng điểm (2) | Mã lớp → Mã KH → Tên khóa học, trình độ, trọng số (H1, H2); Mã lớp → Mã GV → Họ tên GV (H1, H3) | **Khóa học** (#Mã KH, Tên khóa học, Trình độ đầu vào, Trình độ đầu ra, TS chuyên cần, TS giữa kỳ, TS cuối kỳ); **Giáo viên** (#Mã GV, Họ tên GV); **Lớp học** (#Mã lớp, Mã KH, Mã GV) |
| Kết quả học tập | Xếp loại, Kết quả suy ra từ Điểm tổng kết theo H6. Đây là bắc cầu dạng **công thức**, tức là thuộc tính thứ sinh. | Không tách bảng mới, vì "quy tắc xếp loại" không phải dữ liệu cần lưu. Đúng ra phải bỏ hai thuộc tính này; nhóm giữ có chủ đích vì là giá trị *chốt*: khi trọng số hay ngưỡng xếp loại thay đổi, kết quả đã duyệt và đã in trên chứng nhận không được đổi theo (3.1 mục 5) |

**Kết quả chuẩn hóa Bảng điểm (6 thực thể):**

| Thực thể | Thuộc tính |
|---|---|
| Lớp học | #Mã lớp, Mã KH, Mã GV |
| Khóa học | #Mã KH, Tên khóa học, Trình độ đầu vào, Trình độ đầu ra, TS chuyên cần, TS giữa kỳ, TS cuối kỳ |
| Giáo viên | #Mã GV, Họ tên GV |
| Học viên | #Mã HV, Họ tên HV |
| Kết quả học tập | #Mã lớp, #Mã HV, Điểm tổng kết, Xếp loại, Kết quả |
| Điểm thành phần | #Mã lớp, #Mã HV, #Loại điểm, Điểm, Nhận xét |

Kiểm tra lại bằng số liệu mẫu: Điểm thành phần của HV0412 có 2 dòng (Giữa kỳ 7,5 và Cuối kỳ 8,0). Kết quả học tập (IEK-2609, HV0412) = 8,0 – Khá – Đạt. Tính lại theo QT13 với trọng số của khóa IEK: 0,1 × 9,0 + 0,3 × 7,5 + 0,6 × 8,0 = 7,95 → 8,0 ✓.

---

## 6. Hai chứng từ còn lại (rút gọn)

### 6.1 Sổ điểm danh (lớp IEK-2609)

| Bước | Nội dung |
|---|---|
| Bước 1 | **Sổ điểm danh (1)**: Mã lớp, Mã KH, Tên khóa học, Mã GV, Họ tên GV, Phòng, Số buổi (R), Ngày buổi (R), Mã HV (R), Họ tên HV (R), Trạng thái (R), Ghi chú (R). Khóa: Mã lớp. Có 2 nhóm lặp: hàng tiêu đề {buổi} và các dòng {học viên}. Ô trạng thái nằm ở giao của buổi và học viên, nên thuộc cả hai nhóm |
| Bước 2 | Không có thuộc tính thứ sinh (số buổi vắng là S nhưng không in). Xem lại ý nghĩa: **Phòng** ghi ở đầu sổ vì lớp thường học một phòng, nhưng thực chất là phòng của **từng buổi** (học bù có thể đổi phòng) → thuộc nhóm {buổi}. **Ghi chú** ("Nhắc phụ huynh") ghi ở cuối dòng học viên, nhưng nói về các buổi vắng cụ thể; giáo viên ghi chú khi điểm danh từng buổi → thuộc ô (học viên, buổi) |
| 1NF | Sổ điểm danh (2) (#Mã lớp, Mã KH, Tên khóa học, Mã GV, Họ tên GV); Buổi (1) (#Mã lớp, #Số buổi, Ngày, Phòng); Dòng sổ (1) (#Mã lớp, #Mã HV, Họ tên HV); Ô điểm danh (1) (#Mã lớp, #Mã HV, #Số buổi, Trạng thái, Ghi chú) |
| 2NF | Dòng sổ (1): Họ tên HV chỉ phụ thuộc Mã HV → **Học viên** (#Mã HV, Họ tên HV), phần còn lại là **Học viên của lớp** (#Mã lớp, #Mã HV). Buổi (1) → **Buổi học** (#Mã lớp, #Số buổi, Ngày, Mã phòng). Ô điểm danh (1) → **Điểm danh** (#Mã lớp, #Mã HV, #Số buổi, Trạng thái, Ghi chú) |
| 3NF | Sổ điểm danh (2): Mã lớp → Mã KH → Tên khóa học; Mã lớp → Mã GV → Họ tên GV → **Khóa học** (#Mã KH, Tên khóa học), **Giáo viên** (#Mã GV, Họ tên GV), **Lớp học** (#Mã lớp, Mã KH, Mã GV) |

Kết quả: Lớp học, Khóa học, Giáo viên, Học viên, Học viên của lớp, Buổi học, Điểm danh (7 thực thể).

### 6.2 Giấy chứng nhận (CN2026-0089)

| Bước | Nội dung |
|---|---|
| Bước 1 | **Chứng nhận (1)**: Số CN, Mã HV, Họ tên HV, Ngày sinh, Mã lớp, Mã KH, Tên khóa học, Từ ngày (S), Đến ngày (S), Xếp loại, Điểm tổng kết, Ngày cấp, Chữ ký giám đốc. Khóa: Số CN. Mã HV không in nhưng được thêm làm định danh (họ tên có thể trùng) |
| Bước 2 | Loại Từ ngày, Đến ngày (S, lấy từ buổi đầu và buổi cuối của lớp) và Chữ ký (chỉ có trên bản giấy) |
| 1NF, 2NF | Không có nhóm lặp; khóa đơn → đã 2NF |
| 3NF | Số CN → Mã HV → Họ tên HV, Ngày sinh; Số CN → Mã lớp → Mã KH → Tên khóa học; Số CN → (Mã HV, Mã lớp) → Xếp loại, Điểm tổng kết → tách **Học viên** (#Mã HV, Họ tên HV, Ngày sinh), **Lớp học** (#Mã lớp, Mã KH), **Khóa học** (#Mã KH, Tên khóa học), **Kết quả học tập** (#Mã lớp, #Mã HV, Điểm tổng kết, Xếp loại); còn lại **Chứng nhận** (#Số CN, Mã HV, Mã lớp, Ngày cấp) |

---

## 7. Trộn các bảng thực thể

Bài giảng nêu: sau khi chuẩn hóa, một số thực thể có thể thừa vì cùng mô tả một đối tượng, nên cần **trộn** các bảng lại. Khi trộn phải bảo toàn ý nghĩa: tránh đồng nghĩa khác tên, đồng âm khác nghĩa, và loại bỏ phụ thuộc bắc cầu nếu xuất hiện.

Ký hiệu chứng từ nguồn: **A** = Phiếu đăng ký, **B** = Phiếu thu, **C** = Bảng điểm, **D** = Sổ điểm danh, **E** = Chứng nhận.

### 7.1 Thống nhất tên gọi trước khi trộn

| Tên trên các chứng từ | Tên thống nhất | Lý do |
|---|---|---|
| Họ tên HV, Họ tên NV, Họ tên GV, Họ tên PH | Họ và tên (`HoTen`) trong bảng của từng đối tượng | Cùng một khái niệm, khác bảng |
| Điện thoại HV / PH | Điện thoại (`DienThoai`) | |
| "Học phí" trên Phiếu đăng ký và "Học phí" trên Phiếu thu | Học phí gốc (`HocPhiGoc`), thuộc khoản học phí của đăng ký | Cùng nghĩa (F9, G4): học phí đã chốt tại ngày đăng ký |
| "Học phí" của khóa (bảng giá hiện hành) | Học phí (`KhoaHoc.HocPhi`) | **Đồng âm khác nghĩa** với dòng trên. Hai giá trị khác nhau khi trung tâm đổi giá, nên phải là hai thuộc tính |
| "Nộp lần này" | Số tiền (`ChiTietPhieuThu.SoTien`) | Đã tách theo đợt (mục 4) |
| "Hạn đợt 2" | Hạn đóng (`DotHocPhi.HanDong`) | Áp dụng cho mọi đợt |
| "Phòng" (sổ điểm danh) | Mã phòng (`MaPhong`) | |
| "Tỷ lệ giảm UĐ" | Tỷ lệ giảm (`UuDai.TyLeGiam`) | Mức giảm của một ưu đãi. Khác `HocPhi.TyLeGiam` là tổng mức giảm đã chốt, có giới hạn 15% (QT04) |

### 7.2 Gom các bảng theo khóa

| Khóa | Các bảng từ chứng từ | Trộn thành |
|---|---|---|
| Mã HV | Học viên (A, B, C, D, E) | **HocVien**: hợp các thuộc tính của các bảng |
| Mã PH | Phụ huynh (A) | **PhuHuynh** |
| (Mã HV, Mã PH) | Học viên – Phụ huynh (A) | **HocVienPhuHuynh** |
| Mã NV | Nhân viên (A, B) | **NhanVien** |
| Mã GV | Giáo viên (C, D) | **GiaoVien** |
| Mã KH | Khóa học (A, B, C, D, E) | **KhoaHoc** |
| Mã lớp | Lớp học (A, B, C, D, E) | **LopHoc** (#Mã lớp, Mã KH, Mã GV, Ngày khai giảng) |
| (Mã lớp, Thứ) | Lịch tuần (A) | **LichTuan**. Sau khi trộn, Khóa học đã có Thời lượng buổi (3.1), nên Giờ kết thúc = Giờ bắt đầu + Thời lượng buổi trở thành **thứ sinh** → bỏ |
| (Mã lớp, Số buổi) | Buổi học (D) | **BuoiHoc** |
| Số phiếu ĐK | Phiếu đăng ký (A) | **PhieuDangKy** (bỏ Mã HV, xem 7.3) |
| (Số phiếu ĐK, Mã lớp) ≡ (Mã HV, Mã lớp) | Dòng đăng ký (A); Đăng ký – học phí (B); Học viên của lớp (D) | **DangKy** + **HocPhi** (xem 7.3) |
| (…, Mã UĐ) | Áp dụng ưu đãi (A) | **ApDungUuDai**, đổi khóa sang (Mã HV, Mã lớp, Mã UĐ) |
| Mã UĐ | Ưu đãi (A) | **UuDai** |
| (Mã HV, Mã lớp, Đợt) | Đợt học phí (B) | **DotHocPhi** |
| Số PT | Phiếu thu (B) | **PhieuThu** |
| (Số PT, Mã HV, Mã lớp, Đợt) | Chi tiết phiếu thu (B) | **ChiTietPhieuThu** |
| (Mã lớp, Mã HV) | Kết quả học tập (C, E) | **KetQua** (xem 7.3) |
| (Mã lớp, Mã HV, Loại điểm) | Điểm thành phần (C) | **Diem** |
| (Mã lớp, Mã HV, Số buổi) | Điểm danh (D) | **DiemDanh** |
| Số CN | Chứng nhận (E) | **ChungNhan** |

### 7.3 Các quyết định trộn và không trộn

**(1) Hai khóa tương đương của đăng ký.** Phiếu đăng ký cho khóa (Số phiếu ĐK, Mã lớp). Phiếu thu, bảng điểm và sổ điểm danh cho khóa (Mã HV, Mã lớp). Hai khóa này xác định cùng một đối tượng, vì:
- Số phiếu ĐK → Mã HV (F1), nên (Số phiếu ĐK, Mã lớp) → (Mã HV, Mã lớp).
- Mỗi học viên chỉ có một đăng ký ở mỗi lớp (F11), nên (Mã HV, Mã lớp) → Số phiếu ĐK.

Ba bảng *Dòng đăng ký* (A), *Đăng ký – học phí* (B) và *Học viên của lớp* (D) vì vậy mô tả **cùng một việc học**. Nhóm chọn **(Mã HV, Mã lớp)** làm khóa, vì đây là khóa mà Điểm danh, Điểm, Kết quả trỏ tới (3 chứng từ C, D, E dùng nó). Số phiếu ĐK trở thành thuộc tính quan hệ trỏ về PhieuDangKy. Áp dụng ưu đãi cũng đổi khóa tương ứng.

**(2) Bỏ Mã HV khỏi Phiếu đăng ký.** Sau khi trộn, học viên của phiếu đã có ở các dòng DangKy. Nếu giữ Mã HV ở cả hai nơi thì một dữ liệu bị lưu hai lần, trái với mục đích chuẩn hóa (bài giảng 5.4.1: một thuộc tính chỉ xuất hiện ở nhiều bảng khi là định danh kết nối). Kết quả: **PhieuDangKy (#SoPhieuDK, NgayDK, MaNV)**.

Kiểm tra lại 3NF của DangKy: vẫn còn phụ thuộc Số phiếu ĐK → Mã HV. Nhưng Mã HV là **thuộc tính khóa**, mà 3NF chỉ cấm phụ thuộc bắc cầu từ khóa tới các thuộc tính **không khóa** (bài giảng 5.4.2). Vì vậy DangKy vẫn đạt 3NF.

**(3) Không trộn ba nhóm cùng khóa (Mã HV, Mã lớp).** Sau bước (1), có ba nhóm thuộc tính cùng khóa:
- việc học: Số phiếu ĐK, trạng thái…;
- khoản phải thu: Học phí gốc, tỷ lệ giảm, số đợt;
- kết quả cuối khóa: Điểm tổng kết, Xếp loại, Kết quả.

Bài giảng chỉ trộn khi các bảng *mô tả cho cùng một đối tượng*. Ở đây là ba đối tượng khác nhau, nên giữ **DangKy**, **HocPhi**, **KetQua** riêng, đúng như 3.1 (E13, E20):
- Chúng phát sinh ở ba thời điểm, do ba xử lý khác nhau: 2.2, 3.1, 4.3.
- Chúng thuộc hai kho khác nhau trên DFD: D2 cho đăng ký; D3 và D4 cho học phí và kết quả.
- Không phải đăng ký nào cũng có học phí riêng (đăng ký chuyển lớp không có), hay có kết quả (đăng ký nghỉ học không có). Nếu trộn, các dòng đó sẽ có nhiều trường rỗng.

Vì vậy bảng *Đăng ký – học phí* (B) được chia theo hai đối tượng: Số phiếu ĐK sang DangKy, Học phí gốc sang HocPhi.

**(4) Kiểm tra sau khi trộn.** Bài giảng lưu ý: khi trộn phải loại bỏ phụ thuộc bắc cầu nếu nó xuất hiện. Kết quả rà lại:
- **Giờ kết thúc (LichTuan)**: thành thứ sinh khi gặp Thời lượng buổi của khóa → đã bỏ.
- **Học phí gốc và Học phí của khóa**: là hai thuộc tính khác nghĩa (mục 7.1). Học phí gốc không phụ thuộc bắc cầu Mã lớp → Mã KH → Học phí, vì giá trị của nó cố định từ ngày đăng ký.
- Không phát sinh phụ thuộc bắc cầu nào khác.

---

## 8. Đối chiếu kết quả chuẩn hóa với thiết kế 3.1–3.2

Cột 2 là thuộc tính thu được từ chuẩn hóa các chứng từ, viết theo tên trường. Cột 4 là các thuộc tính 3.1 có thêm, cùng nguồn của chúng. Nguồn là chức năng hay từ điển dữ liệu nơi dữ liệu được nhập, vì không chứng từ nào in các thuộc tính đó.

| Tên tệp | Khóa và thuộc tính sau chuẩn hóa | Chứng từ | Thuộc tính 3.1 bổ sung từ nguồn khác |
|---|---|---|---|
| HocVien | #MaHV, HoTen, NgaySinh, GioiTinh, DienThoai, Email, DiaChi, TrinhDoDauVao, NgayKiemTra | A, B, C, D, E | NgayTiepNhan, TrangThai (1.1, quản lý hồ sơ) |
| PhuHuynh | #MaPH, HoTen, DienThoai | A | Email (từ điển dữ liệu D1) |
| HocVienPhuHuynh | #MaHV, #MaPH, QuanHe | A | – |
| GiaoVien | #MaGV, HoTen | C, D | NgaySinh, DienThoai, Email, NgonNguDay, ChuyenMon, BangCap, TinhTrang (1.2) |
| KhoaHoc | #MaKH, TenKH, TrinhDoDauVao, TrinhDoDauRa, TSChuyenCan, TSGiuaKy, TSCuoiKy | A, B, C, D, E | NgonNgu, SoBuoi, ThoiLuongBuoi, HocPhi, MoTa, TrangThai (1.3) |
| PhongHoc | #MaPhong | D | TenPhong, SucChua, ThietBi, TinhTrang (1.4; chứng từ chỉ có mã phòng) |
| LopHoc | #MaLop, MaKH, MaGV, NgayKhaiGiang | A, B, C, D, E | SiSoToiDa, TrangThai (2.1) |
| LichTuan | #MaLop, #Thu, GioBatDau | A | MaPhong (2.1, kế hoạch mở lớp gán phòng cho khung giờ) |
| BuoiHoc | #MaLop, #SoBuoi, Ngay, MaPhong | D | GioBatDau, GioKetThuc, MaGV, Loai, NoiDung, TrangThai (2.3) |
| PhieuDangKy | #SoPhieuDK, NgayDK, MaNV | A | – |
| DangKy | #MaHV, #MaLop, SoPhieuDK | A, B, D | TrangThai, NgayThayDoi, LyDo, HanBaoLuu, MaLopGoc (2.4, đơn chuyển lớp/bảo lưu) |
| UuDai | #MaUD, TenUD, TyLeGiam | A | LoaiUD, NgayBatDau, NgayKetThuc, DieuKien (3.1) |
| HocPhi | #MaHV, #MaLop, HocPhiGoc | A, B | TyLeGiam, SoDot, NgayLap (3.1, tính học phí) |
| DotHocPhi | #MaHV, #MaLop, #Dot, HanDong | B | SoTien (3.1: số tiền phải nộp của đợt; chứng từ chỉ in số đã nộp) |
| ApDungUuDai | #MaHV, #MaLop, #MaUD | A | – |
| PhieuThu | #SoPT, NgayThu, NguoiNop, HinhThuc, MaNV | B | – |
| ChiTietPhieuThu | #SoPT, #MaHV, #MaLop, #Dot, SoTien | B | – |
| DiemDanh | #MaHV, #MaLop, #SoBuoi, TrangThai, GhiChu | D | – |
| Diem | #MaHV, #MaLop, #LoaiDiem, Diem, NhanXet | C | – |
| KetQua | #MaHV, #MaLop, DiemTongKet, XepLoai, KetQua | C, E | MaNVDuyet, NgayDuyet (4.3, QL đào tạo duyệt) |
| ChungNhan | #SoCN, MaHV, MaLop, NgayCap | E | – |
| NhanVien | #MaNV, HoTen | A, B | BoPhan, ChucVu, DienThoai, Email, TrangThai (5.1) |
| TaiKhoan | – | – | TenDangNhap, MatKhauBam, VaiTro, MaNV, MaGV, MaHV, MaPH, TrangThai (5.1; không có chứng từ) |
| ThongBao | – | – | MaTB, Loai, MaHV, MaPH, NoiDung, ThoiGianGui, TrangThaiGui (5.2; không có chứng từ) |
| NhatKy | – | – | MaNK, ThoiDiem, TenDangNhap, ThaoTac, DoiTuong, MaDoiTuong (dữ liệu hệ thống) |
| ThamSo | – | – | MaThamSo, TenThamSo, GiaTri, DonVi, QuyTac (dữ liệu cấu hình) |

### 8.1 Kết luận đối chiếu

Chạy `python thiet_ke/kiem_tra_chuan_hoa.py`: **đạt**.

| Kết quả | Số liệu |
|---|---|
| Thực thể tìm lại được bằng chuẩn hóa 5 chứng từ | **22/26**, khóa trùng đúng khóa đã thiết kế ở 3.1 |
| Thuộc tính lấy từ chứng từ | 86/161. 75 thuộc tính còn lại là dữ liệu chỉ nhập trên màn hình danh mục hoặc do hệ thống sinh, không in trên chứng từ nào; nguồn của chúng ghi ở cột 4 |
| Bảng hoặc thuộc tính chuẩn hóa ra mà 3.1 không có | **0** |
| Thực thể không có chứng từ | 4: TaiKhoan, ThongBao (D5, nhập qua 5.1 và 5.2), NhatKy (dữ liệu hệ thống), ThamSo (dữ liệu cấu hình) |

Kết luận:
- **Chuẩn hóa không phát hiện bảng nào sai, cũng không thiếu bảng nào.** 26 bảng của 3.1 đều đạt 3NF, nên đáp ứng yêu cầu của KE_HOACH: kết quả 3NF trùng với tập bảng đã thiết kế.
- **Các quyết định ở 3.1 được xác nhận lại bằng phụ thuộc hàm:** tách PhieuDangKy và DangKy, khóa (MaHV, MaLop), Đợt và Mã HV nằm ở dòng phiếu thu, tách LichTuan, HocVienPhuHuynh.
- **Script đã được thử với dữ liệu sai** để chắc nó bắt được lỗi. Đổi khóa DangKy thành (SoPhieuDK, MaLop) thì báo lỗi khóa. Thêm MaHV vào PhieuThu thì báo thuộc tính thừa.

---

## 9. Đầu ra cho các bước sau

| Bước | Dùng gì từ tài liệu này |
|---|---|
| 3.4 CSDL vật lý | 26 bảng ở 3.1 đã được xác nhận là đạt 3NF. Các phụ thuộc hàm F1–F11, G1–G6, H1–H6 là căn cứ đặt PRIMARY KEY và UNIQUE. Riêng F11 cho thấy cần UNIQUE (SoPhieuDK, MaLop) trên DangKy, để khóa tương đương (Số phiếu ĐK, Mã lớp) cũng được bảo đảm. Có 8 thuộc tính "chốt" được lưu có chủ đích (3.1 mục 5), cần ghi chú trong bảng mô tả tệp |
| 3.6 Giao diện | Form nhập theo dạng **đã chuẩn hóa**: phiếu đăng ký chọn học viên, lớp, ưu đãi từ danh mục chứ không gõ lại tên; phiếu thu nhập theo từng đợt |
