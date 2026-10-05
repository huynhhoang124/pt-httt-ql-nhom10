# GĐ5 Bảo trì – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn 5 của vòng đời phát triển hệ thống (KE_HOACH.md, mục Giai đoạn 5). Theo bài giảng (Ch.1, Ch.5), sau khi đưa vào khai thác hệ thống cần được **bảo trì** theo ba loại: **hiệu chỉnh**, **thích nghi**, **phòng ngừa**; yêu cầu thay đổi được xử lý theo thứ tự ưu tiên **sửa lỗi > thích nghi > cải tiến**; mọi thay đổi được ghi vết qua **quản lý cấu hình** (phiên bản, phân quyền).
Các ví dụ dưới đây lấy từ chính quá trình cài đặt bản demo (`phan_tich/05_cai_dat.md`) và các giới hạn đã biết của nó.

---

## 1. Các loại bảo trì

| Loại | Mục đích | Ví dụ trong hệ thống |
|---|---|---|
| **Hiệu chỉnh** (sửa lỗi) | Sửa chỗ hệ thống chạy sai so với đặc tả | 4 lỗi phát hiện khi kiểm thử GĐ4 (05_cai_dat mục 4.3): ưu đãi hết hạn trước ngày áp dụng, câu báo lỗi tiếng Anh, phiếu thu tự điền người nộp khi lưu, nhật ký mất dấu tiếng Việt |
| **Thích nghi** | Thay đổi để hệ thống theo kịp môi trường mới: chính sách, quy định, công nghệ | Chính sách ưu đãi năm mới (sửa bảng `UuDai`, không sửa mã); đổi sĩ số tối đa, tỷ lệ đợt 1, ngưỡng vắng (sửa bảng `ThamSo`); **.NET 8 hết hỗ trợ ngày 10/11/2026** → nâng lên .NET 10 (bản hỗ trợ dài hạn); trình duyệt, máy in mới |
| **Phòng ngừa** | Xử lý trước các nguy cơ chưa thành lỗi | Thử phục hồi bản sao lưu mỗi quý; theo dõi dung lượng CSDL (Express giới hạn 10 GB); chạy `csdl/kiem_tra.sql` định kỳ để phát hiện dữ liệu sai; chuyển đếm khóa đăng nhập từ bộ nhớ sang bảng CSDL; bắt đổi mật khẩu ở lần đăng nhập đầu |
| *Cải tiến* (hoàn thiện) | Thêm hoặc làm tốt hơn chức năng theo đề nghị người dùng | Nhập điểm chấp nhận cả dấu phẩy "7,5"; các hướng phát triển ở mục 5 |

Hai loại "thích nghi" thường gặp nhất (ưu đãi, tham số) **không cần lập trình viên**: thiết kế đã đưa mọi con số chính sách vào bảng `ThamSo`, `UuDai` (3.4), và mã nguồn luôn đọc từ đó (`NghiepVu/QuyTac.cs` không viết cứng 15%, 50%, 24 giờ…). Quản trị viên hoặc kế toán sửa qua giao diện; lập trình viên chỉ vào cuộc khi đổi *cách tính* chứ không phải *con số*.

---

## 2. Quy trình xử lý yêu cầu bảo trì

| Bước | Việc | Ai | Ghi ở |
|:-:|---|---|---|
| 1 | Tiếp nhận: mô tả hiện tượng, màn hình, tài khoản, thời điểm; đính kèm ảnh chụp | Người dùng → quản trị viên | GitHub Issues (mỗi yêu cầu một issue) |
| 2 | Phân loại (hiệu chỉnh / thích nghi / phòng ngừa / cải tiến) và mức độ (mục 2.1) | Quản trị viên, trưởng nhóm | Nhãn của issue |
| 3 | Phân tích: tìm nguyên nhân gốc, xác định module, bảng, tài liệu bị ảnh hưởng (dùng bảng module × dữ liệu ở 05_module mục 4) | Lập trình viên | Bình luận trong issue |
| 4 | Sửa trên nhánh riêng; thay đổi CSDL viết thành tệp nâng cấp (mục 3.3) | Lập trình viên | Nhánh `sua/…` hoặc `them/…` |
| 5 | Kiểm thử hồi quy: `dotnet test` (38 ca), `csdl/kiem_tra.sql` (24 kiểm tra), `demo/sinh_ma.py`, ca kiểm thử của tiến trình liên quan (05_cai_dat mục 4.2); thêm ca mới cho chính lỗi vừa sửa | Lập trình viên, người duyệt | Pull request |
| 6 | Duyệt và hợp nhất vào `main`; gắn thẻ phiên bản | Trưởng nhóm | Thẻ Git, ghi chú phát hành |
| 7 | Triển khai: sao lưu CSDL → chạy tệp nâng cấp → xuất bản ứng dụng → thử nhanh | Quản trị viên | Hồ sơ cấu hình (05_cai_dat mục 1.4) |
| 8 | Cập nhật tài liệu (đặc tả, thiết kế, hướng dẫn sử dụng) nếu thay đổi chạm tới; đóng issue | Người sửa | Tệp .md tương ứng |

### 2.1 Mức độ và thời gian xử lý

| Mức | Dấu hiệu | Ví dụ | Xử lý |
|---|---|---|---|
| Khẩn | Sai tiền, mất dữ liệu, lộ dữ liệu, cả trung tâm không làm việc được | Phiếu thu tính sai số còn nợ; phụ huynh xem được dữ liệu học viên khác | Trong ngày; tạm khóa chức năng lỗi, làm tay theo quy trình cũ |
| Cao | Một bộ phận bị chặn công việc | Giáo viên không điểm danh được | 1–2 ngày |
| Thường | Có cách làm tạm | Báo cáo hiển thị sai định dạng | Gộp vào bản phát hành kế tiếp |
| Thấp | Góp ý, cải tiến | Thêm cột vào báo cáo | Xếp hàng theo ưu tiên của giám đốc |

Thứ tự ưu tiên khi nhiều yêu cầu cùng lúc: **mức độ** trước, rồi tới **loại** theo bài giảng (sửa lỗi > thích nghi > cải tiến).

### 2.2 Ví dụ một vòng bảo trì hiệu chỉnh

Lỗi "nhật ký mất dấu" phát hiện khi kiểm thử TC21 (05_cai_dat):
1. *Hiện tượng:* nhật ký ghi `TOE-2611/Cu?i k?` thay vì `TOE-2611/Cuối kỳ`.
2. *Phân loại:* hiệu chỉnh, mức thường (nhật ký vẫn ghi, chỉ khó đọc).
3. *Nguyên nhân gốc:* cột `NhatKy.MaDoiTuong` kiểu `VARCHAR` không chứa được tiếng Việt, trong khi khóa của bảng `Diem` có `LoaiDiem` tiếng Việt. Sửa ở cột (một chỗ) thay vì đổi cách ghi ở từng trang.
4. *Sửa:* `csdl/schema.sql` đổi thành `NVARCHAR(100)`; ánh xạ EF Core bỏ `IsUnicode(false)`; sinh lại `csdl/mo_ta_bang.md`. Với CSDL đang chạy thật, tệp nâng cấp là:
   ```sql
   ALTER TABLE dbo.NhatKy ALTER COLUMN MaDoiTuong NVARCHAR(100) NOT NULL;
   ```
5. *Kiểm thử hồi quy:* `kiem_tra.sql` 24/24, nhập lại điểm → nhật ký đúng dấu.
6. *Phiên bản:* tăng số cuối (vd `v1.0.0` → `v1.0.1`).

---

## 3. Quản lý cấu hình bằng Git

### 3.1 Nhánh và pull request

- `main`: bản đang chạy hoặc sắp phát hành; **được bảo vệ** – chỉ hợp nhất qua pull request có ít nhất 1 người duyệt (đã cấu hình trên GitHub).
- Nhánh làm việc: `sua/<mô-tả>` cho hiệu chỉnh, `them/<mô-tả>` cho cải tiến, `nang-cap/<mô-tả>` cho thích nghi công nghệ. Mỗi nhánh gắn với một issue.
- Pull request phải ghi: issue liên quan, loại bảo trì, tài liệu đã cập nhật, kết quả các bước kiểm thử ở mục 2 bước 5.

### 3.2 Phiên bản

Đánh số `vA.B.C`, gắn thẻ Git trên `main` mỗi lần triển khai:

| Thay đổi | Tăng | Ví dụ |
|---|---|---|
| Hiệu chỉnh, không đổi cấu trúc CSDL | C | `v1.0.0` → `v1.0.1` |
| Thích nghi hoặc cải tiến; có tệp nâng cấp CSDL | B | `v1.0.1` → `v1.1.0` |
| Thay đổi lớn về phạm vi (vd đa cơ sở) | A | `v1.4.2` → `v2.0.0` |

Mỗi thẻ có ghi chú phát hành (GitHub Release): danh sách issue đã xử lý, tệp nâng cấp CSDL cần chạy, việc người dùng cần biết.

### 3.3 Thay đổi CSDL

Ở giai đoạn thiết kế, CSDL được **tạo lại từ đầu** bằng `csdl/chay.ps1` (xóa và chạy lại `schema.sql`, `views.sql`, `seed.sql`). Khi đã có dữ liệu thật thì **không được tạo lại**. Vì vậy từ bản `v1.0.0`:
- `schema.sql` vẫn là mô tả đầy đủ của cấu trúc mới nhất (dùng cài mới, dùng cho bản thử);
- mỗi thay đổi thêm một tệp `csdl/nang_cap/NNN_mo_ta.sql` (đánh số tăng dần) chỉ gồm `ALTER`/`CREATE`, chạy được trên CSDL đang có dữ liệu;
- bảng `ThamSo` lưu thêm số của tệp nâng cấp cuối cùng đã chạy, để biết CSDL đang ở phiên bản nào;
- luôn sao lưu trước khi chạy tệp nâng cấp; thử trên bản sao của CSDL thật trước.

### 3.4 Phân quyền trên kho mã

| Ai | Quyền |
|---|---|
| Trưởng nhóm (admin) | Duyệt, hợp nhất, gắn thẻ, cấu hình bảo vệ nhánh |
| Thành viên phát triển | Tạo nhánh, mở pull request; không đẩy thẳng vào `main` |
| Quản trị viên của trung tâm | Đọc mã, tạo issue; giữ cấu hình máy chủ (chuỗi kết nối, mật khẩu) **ngoài** kho mã |

Kho mã không chứa dữ liệu thật hay mật khẩu thật: `seed.sql` là dữ liệu giả định; chuỗi kết nối bản chính thức đặt ở biến môi trường hoặc tệp cấu hình riêng trên máy chủ.

---

## 4. Danh sách bảo trì đã biết khi đưa vào sử dụng

| # | Việc | Loại | Mức | Ghi chú |
|:-:|---|---|---|---|
| B1 | Nâng .NET 8 / EF Core 8 lên .NET 10 / EF Core 10 | Thích nghi | Cao | .NET 8 hết hỗ trợ 10/11/2026, sau đó không còn bản vá bảo mật |
| B2 | Bắt đổi mật khẩu ở lần đăng nhập đầu | Phòng ngừa | Cao | Tài khoản cấp hàng loạt cho HV/PH (05_cai_dat mục 1.3) |
| B3 | Lịch sao lưu tự động + thử phục hồi (module N4) | Phòng ngừa | Cao | Chưa có trong bản demo |
| B4 | Đếm số lần đăng nhập sai lưu trong CSDL thay vì bộ nhớ | Phòng ngừa | Thường | Hiện mất khi khởi động lại máy chủ (`Nen/DangNhap.cs`) |
| B5 | Cài nốt các module chưa có trong demo: 1.2–1.4, 2.1, 2.3, 2.4, 3.3, 4.3, 4.4, 5.1, 5.2, 5.4, N4–N6 | Hoàn thiện | Cao | Các ca kiểm thử "–" ở 05_cai_dat mục 4.2 |
| B6 | Cập nhật danh mục ưu đãi theo chính sách năm 2028 trước 31/12/2027 | Thích nghi | Thường | Ưu đãi mẫu hết hạn 31/12/2027 |
| B7 | Nhập điểm chấp nhận dấu phẩy thập phân ("7,5") | Cải tiến | Thấp | Hiện báo lỗi kèm gợi ý dùng dấu chấm |
| B8 | Số tiền bằng chữ chỉ đọc tới hàng tỷ | – | Thấp | Đủ cho học phí; không cần làm |

---

## 5. Hướng phát triển

Các chức năng đã để **ngoài phạm vi** ở GĐ1 (01_ke_hoach mục 2.1), sắp theo lợi ích so với công sức. Với mỗi hướng, phần đã thiết kế được giữ nguyên, chỉ thêm vào:

| Hướng | Lợi ích | Thay đổi so với thiết kế hiện tại | Ưu tiên |
|---|---|---|:-:|
| Thông báo qua SMS/Zalo, email tự động | 93% phụ huynh muốn được báo khi con vắng (02_thu_thap mục 7); giảm việc gọi điện của CSHV | Module 5.2 thêm kênh gửi; bảng `ThongBao` thêm cột kênh, trạng thái gửi. DFD không đổi (vẫn là luồng "Thông báo" tới HV/PH) | 1 |
| Thanh toán trực tuyến (mã QR chuyển khoản, cổng thanh toán) | 85% muốn tra cứu học phí trực tuyến; bớt tiền mặt | Phiếu thu sinh tự động khi ngân hàng báo đã nhận (vẫn qua quy tắc QT05 của 3.2); thêm tác nhân "Ngân hàng / cổng thanh toán" vào DFD | 2 |
| Điểm danh bằng mã QR | Nhanh hơn với lớp đông; học viên tự quét | Màn hình F4.1 hiện mã QR của buổi; quy tắc QT11 giữ nguyên | 3 |
| Ứng dụng di động | Học viên, phụ huynh xem lịch, học phí, kết quả | Thêm lớp Web API trên các module xử lý có sẵn (05_module mục 7); không đổi CSDL | 4 |
| Tính lương giáo viên | Bỏ bảng tính tay | Dựa vào view `v_GiangDay` (số buổi dạy thực tế, kể cả dạy thay); thêm bảng đơn giá, bảng lương | 5 |
| Hóa đơn điện tử | Theo quy định về hóa đơn điện tử khi học viên cần hóa đơn | Kết nối nhà cung cấp hóa đơn điện tử từ phiếu thu | 6 |
| Quản lý nhiều cơ sở | Khi trung tâm mở thêm cơ sở (hiện chưa có dự định trong 2 năm – 02_thu_thap mục 6.3) | Thêm thực thể **CoSo**; `LopHoc`, `PhongHoc`, `NhanVien` có mã cơ sở; phân quyền thêm phạm vi "cơ sở của mình"; báo cáo lọc theo cơ sở. Thay đổi lớn → phiên bản `v2.0.0` | 7 |

Mỗi hướng khi làm đều đi lại vòng đời thu nhỏ: thu thập yêu cầu → sửa DFD/ERD/module/giao diện tương ứng → cài đặt → kiểm thử → phát hành theo mục 2–3.
