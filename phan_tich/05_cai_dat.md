# GĐ4 Cài đặt và khai thác – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn 4 của vòng đời phát triển hệ thống (KE_HOACH.md, mục Giai đoạn 4). Theo bài giảng (Ch.5), giai đoạn này gồm: **kế hoạch cài đặt** (cài phần mềm, thiết lập cấu hình, phân quyền người dùng, lập hồ sơ cấu hình); **chuyển đổi** phần cứng – phần mềm, quy trình và biểu mẫu, con người (huấn luyện 3 mức), dữ liệu; chọn **phương pháp chuyển đổi**; sau đó là hỗ trợ sử dụng, tài liệu hệ thống và quản lý cấu hình.
Số liệu trung tâm (1 cơ sở, khoảng 500 học viên, 30 giáo viên, 12 máy tính, 4 bộ phận) lấy từ `phan_tich/02_thu_thap.md`, là **số liệu giả định**. Phần kiểm thử ghi kết quả chạy thật trên bản demo `demo/TrungTamNgoaiNgu` và CSDL `csdl/`.

---

## 1. Kế hoạch cài đặt

### 1.1 Cài phần mềm

Hệ thống chạy trên **một máy chủ** đặt tại trung tâm; người dùng chỉ cần trình duyệt (máy tính văn phòng hoặc điện thoại), không cài gì thêm (GĐ1 mục 3.2).

| Bước | Máy | Phần mềm | Ghi chú |
|---|---|---|---|
| 1 | Máy chủ | Windows 10/11 Pro hoặc Windows Server | Dùng 1 trong 12 máy hiện có nếu đủ cấu hình (≥ 8 GB RAM, ổ SSD) |
| 2 | Máy chủ | SQL Server 2022 **Express** | Miễn phí, giới hạn 10 GB/CSDL – đủ cho nhiều năm dữ liệu của 1 cơ sở |
| 3 | Máy chủ | ASP.NET Core 8 Hosting Bundle + IIS | Chạy ứng dụng web; bật HTTPS bằng chứng chỉ của tên miền nội bộ |
| 4 | Máy chủ | Tạo CSDL: `csdl/chay.ps1` chạy `schema.sql` → `views.sql` → (`seed.sql` chỉ ở môi trường thử) → `kiem_tra.sql` | Bản chính thức thay `seed.sql` bằng dữ liệu chuyển đổi (mục 2.4) |
| 5 | Máy chủ | Xuất bản ứng dụng: `dotnet publish -c Release`, chép vào thư mục site IIS | Bản demo chạy thử bằng `dotnet run --launch-profile http` (http://localhost:5223) |
| 6 | Máy chủ | Lịch sao lưu (module N4): `BACKUP DATABASE` hằng ngày lúc 23:00 bằng Task Scheduler, giữ 30 bản; chép bản tuần ra ổ ngoài | Express không có SQL Agent nên dùng Task Scheduler |
| 7 | Máy người dùng | Trình duyệt Chrome/Edge bản mới; đặt lối tắt tới địa chỉ hệ thống | Giáo viên dùng điện thoại qua wifi của trung tâm |
| 8 | Máy in hóa đơn | Đặt khổ giấy A5 cho phiếu thu (R3.2 in 2 liên) | Thử in PT mẫu, đối chiếu với chứng từ cũ |

### 1.2 Thiết lập cấu hình

| Cấu hình | Nơi đặt | Giá trị khi đưa vào sử dụng |
|---|---|---|
| Chuỗi kết nối CSDL | `appsettings.json` → `ConnectionStrings:TrungTam` | Trỏ tới instance SQL Server của máy chủ, xác thực Windows |
| Giờ hệ thống | `appsettings.json` → `HeThong:ThoiDiem` | **Để trống** (dùng giờ máy). Bản demo đặt cố định 04/01/2027 19:30 cho khớp dữ liệu mẫu |
| Tham số nghiệp vụ | Bảng `ThamSo` (14 tham số) | Sĩ số tối đa 20, tối thiểu 8; tỷ lệ đợt 1 50%; trần giảm 15%; sửa điểm danh trong 24 giờ; ngưỡng cảnh báo vắng 20%; điểm đạt 5, khá 7, giỏi 8,5; chuyên cần đạt 70%… Quản trị viên sửa ở 5.1, không cần sửa mã |
| Danh mục ưu đãi | Bảng `UuDai` | Nhập theo chính sách năm; **ngày kết thúc phải bao trùm thời gian áp dụng** (lỗi đã gặp khi thử, mục 4.3) |
| Khóa tạm đăng nhập | Mã nguồn (`KhoaDangNhap`) | Sai 5 lần → khóa 15 phút |
| Chế độ chạy | Biến môi trường `ASPNETCORE_ENVIRONMENT` | `Production`: ẩn chi tiết lỗi, chuyển về trang /Error |

### 1.3 Phân quyền người dùng

Tài khoản và quyền theo **ma trận phân quyền** ở `thiet_ke/05_module.md` mục 5; bản demo sinh mã kiểm tra quyền trực tiếp từ bảng đó (`demo/sinh_ma.py`), nên tài liệu và chương trình luôn trùng nhau.

| Vai trò | Ai | Số tài khoản (dự kiến) | Phạm vi |
|---|---|:-:|---|
| GD – Giám đốc | Giám đốc | 1 | Xem mọi báo cáo |
| DT – QL đào tạo | Trưởng bộ phận đào tạo | 2 | Danh mục, lớp, lịch, duyệt kết quả |
| TS – NV tuyển sinh/CSHV | Bộ phận tuyển sinh | 3 | Hồ sơ HV, đăng ký, chuyển lớp |
| KT – NV kế toán | Bộ phận kế toán | 2 | Học phí, phiếu thu, công nợ |
| QT – Quản trị viên | Nhân viên CNTT kiêm nhiệm | 1 | Tài khoản, tham số, nhật ký, sao lưu |
| GV – Giáo viên | Giáo viên | 30 | Chỉ lớp mình phụ trách, buổi mình dạy |
| HV – Học viên | Học viên đang học | ~500 | Chỉ dữ liệu của mình |
| PH – Phụ huynh | Phụ huynh học viên dưới 18 tuổi | ~200 | Chỉ dữ liệu của con mình |

Quy trình cấp tài khoản:
1. Quản trị viên tạo tài khoản nhân viên, giáo viên trước buổi huấn luyện mức 2; tên đăng nhập theo mã (nv…, gv…), mật khẩu ban đầu ngẫu nhiên, giao trực tiếp.
2. Tài khoản học viên, phụ huynh tạo hàng loạt sau khi chuyển đổi hồ sơ (mục 2.4), gửi qua email/tin nhắn; chỉ tạo cho học viên đang học.
3. Người dùng đổi mật khẩu ở lần đăng nhập đầu (trang Đổi mật khẩu; mật khẩu lưu dạng PBKDF2). Bản demo mới hướng dẫn chứ chưa bắt buộc; bản chính thức cần chặn các trang khác cho đến khi đổi xong.
4. Nhân viên nghỉ việc: quản trị viên chuyển tài khoản sang "Khóa" ngay trong ngày; không xóa để giữ nhật ký.

### 1.4 Hồ sơ cấu hình

Hồ sơ cấu hình ghi lại "hệ thống đang chạy gồm những gì", dùng khi sửa lỗi, nâng cấp hoặc cài lại.

| Mục | Nội dung | Lưu ở |
|---|---|---|
| Phiên bản phần mềm | Thẻ Git (vd `v1.0`) của mã nguồn đã xuất bản | GitHub, nhánh `main` |
| Phiên bản CSDL | Mã commit của `csdl/schema.sql`, `views.sql`; mọi thay đổi cấu trúc là một tệp SQL mới có số thứ tự | GitHub |
| Môi trường | Bản Windows, SQL Server, .NET; tên máy chủ, instance, cổng | Sổ cấu hình (tệp riêng, không đưa lên GitHub) |
| Tham số | Giá trị bảng `ThamSo`, `UuDai` tại ngày đưa vào sử dụng | Xuất từ CSDL, kèm ngày |
| Tài khoản quản trị | Ai giữ, ngày đổi mật khẩu gần nhất | Sổ cấu hình |
| Sao lưu | Lịch, nơi lưu, lần thử phục hồi gần nhất | Sổ cấu hình |

---

## 2. Nội dung chuyển đổi

### 2.1 Phần cứng – phần mềm

Không mua thêm phần cứng (GĐ1 mục 3.1): dùng 12 máy tính, máy in hóa đơn và wifi hiện có; chọn 1 máy làm máy chủ (hoặc thuê máy chủ ảo nếu máy hiện có không đủ). Phần mềm cũ (Excel, Google Sheets, Zalo) **không gỡ bỏ**: Excel vẫn dùng để mở các báo cáo xuất ra (N5); Zalo vẫn là kênh liên lạc, nhưng không còn là nơi lưu dữ liệu.

### 2.2 Quy trình nghiệp vụ và biểu mẫu

| Cách làm cũ | Cách làm mới | Form / báo cáo |
|---|---|---|
| Phiếu đăng ký giấy, sổ ghi danh | Nhập phiếu trên máy, hệ thống kiểm tra sĩ số (QT06) và trùng đăng ký | F2.2, R2.2 |
| Tính học phí, ưu đãi bằng máy tính bỏ túi | Hệ thống xét ưu đãi theo bảng quyết định 3.1, kế toán xác nhận | F3.1 |
| Viết phiếu thu tay, ghi sổ thu và Excel | Lập phiếu trên máy, in 2 liên, số tiền bằng chữ tự sinh; không sửa phiếu đã lập | F3.2, R3.2 |
| Sổ điểm danh giấy | Giáo viên điểm danh trên điện thoại trong giờ học đến 24 giờ sau (QT11) | F4.1 |
| Giáo viên tính điểm trên Excel riêng | Nhập điểm thành phần, hệ thống tính tổng kết (QT13–QT15) | F4.2, 4.3 |
| Gom Excel cuối tháng làm báo cáo | Báo cáo lấy trực tiếp từ CSDL, xem lúc nào cũng được | R3.4, R5.3… |
| Lịch lớp trên bảng trắng | Lịch sinh theo lịch tuần, kiểm tra trùng giáo viên/phòng (QT07) | 2.3 |

Biểu mẫu giấy mới in từ hệ thống giữ **cùng bố cục** chứng từ cũ (đã đối chiếu: PT2026-0731 in ra khớp chứng từ mẫu) để học viên và phụ huynh không bị bỡ ngỡ.

### 2.3 Con người – huấn luyện 3 mức

| Mức | Đối tượng | Nội dung | Hình thức, thời lượng |
|---|---|---|---|
| 1. Nhận thức máy tính | Nhân viên lớn tuổi, giáo viên ít dùng máy | Dùng trình duyệt, đăng nhập, đổi mật khẩu, in; giữ bí mật mật khẩu | 1 buổi (2 giờ), nhóm nhỏ |
| 2. Nhận thức hệ thống | Mọi nhân viên, giáo viên | Hệ thống làm gì, ai làm phần nào (BFD, luồng chứng từ giữa các bộ phận), dữ liệu nhập một lần dùng chung | 1 buổi (2 giờ), toàn trung tâm |
| 3. Kỹ xảo | Theo vai trò | TS: F1.1, F2.2, chuyển lớp/bảo lưu. KT: F3.1, F3.2, công nợ, doanh thu. GV: điểm danh trên điện thoại, nhập điểm. DT: mở lớp, xếp lịch, duyệt kết quả. QT: tài khoản, tham số, sao lưu – phục hồi | 1–2 buổi mỗi vai trò, thực hành trên **bản thử** có dữ liệu mẫu (`seed.sql`) |

Học viên, phụ huynh không cần huấn luyện: nhận hướng dẫn 1 trang (cách đăng nhập, xem lịch, học phí, chuyên cần, kết quả) kèm tài khoản.
Mỗi giai đoạn chuyển đổi (mục 3) huấn luyện mức 3 **ngay trước** khi bộ phận đó bắt đầu dùng, để không quên.

### 2.4 Dữ liệu

**Danh mục dữ liệu chuyển đổi.** Thứ tự chuyển theo khóa ngoại: bảng được tham chiếu chuyển trước.

| Thứ tự | Nguồn cũ | Bảng đích | Phạm vi |
|:-:|---|---|---|
| 1 | Biểu phí, danh sách khóa học | KhoaHoc | Mọi khóa đang mở |
| 2 | Danh sách phòng | PhongHoc | Toàn bộ |
| 3 | Excel giáo viên, nhân viên | GiaoVien, NhanVien | Đang làm việc |
| 4 | Excel học viên (tuyển sinh), phiếu đăng ký giấy | HocVien, PhuHuynh, HocVienPhuHuynh | HV đang học và HV đã học trong 2 năm gần đây |
| 5 | Excel lịch lớp, bảng trắng | LopHoc, LichTuan, BuoiHoc | Lớp đang học, lớp dự kiến; **lớp đã kết thúc trong 2 năm** |
| 6 | Sổ ghi danh | PhieuDangKy, DangKy | Theo các lớp ở bước 5 |
| 7 | Sổ thu, Excel kế toán | HocPhi, DotHocPhi, ApDungUuDai, PhieuThu, ChiTietPhieuThu | Chỉ đăng ký của lớp đang học/dự kiến; số đã thu nhập thành 1 phiếu thu "chuyển số dư" cho mỗi đợt |
| 8 | Sổ điểm danh, bảng điểm | DiemDanh, Diem | Lớp đang học |
| 9 | Danh sách chứng nhận đã cấp | KetQua, ChungNhan | Lớp đã kết thúc trong 2 năm |
| 10 | – | TaiKhoan, ThamSo, UuDai | Tạo mới (mục 1.2, 1.3) |

Lớp đã kết thúc phải chuyển **kèm ít nhất buổi học cuối ở trạng thái "Đã dạy"**: view `v_HocVienCu` dựa vào lớp *Kết thúc* và buổi *Đã dạy* để xét ưu đãi học viên cũ (QT01). Thiếu dữ liệu này thì học viên cũ không được giảm 10%.

**Phân công.**

| Việc | Người làm |
|---|---|
| Viết script chuyển Excel → SQL (đọc tệp Excel, chuẩn hóa, sinh lệnh INSERT như `csdl/sinh_du_lieu.py`) | Trần Minh Quân, Vũ Văn Hùng |
| Làm sạch hồ sơ học viên, giáo viên, khóa học | NV tuyển sinh, QL đào tạo (dưới sự hướng dẫn của Nguyễn Gia Ân) |
| Đối chiếu học phí, số dư công nợ | NV kế toán, Vũ Văn Hùng |
| Đối chiếu điểm danh, điểm lớp đang học | Giáo viên chủ nhiệm lớp, Nguyễn Văn Luận |
| Nghiệm thu dữ liệu | Hoàng Văn Huynh, giám đốc ký biên bản |

**Khối lượng và chất lượng.** Khoảng 500 học viên đang học + học viên cũ, 30 giáo viên, 25–30 lớp mỗi kỳ, vài nghìn dòng điểm danh. Các lỗi đã biết của dữ liệu cũ (02_thu_thap mục 9.1) và cách xử lý:

| Lỗi | Xử lý khi chuyển |
|---|---|
| Một học viên bị nhập 2 lần ở các file khác nhau (V1) | Gộp theo **số điện thoại + ngày sinh** (quy tắc kiểm tra trùng của F1.1); trường hợp nghi ngờ đưa ra danh sách để TS xác nhận |
| Số điện thoại nhiều định dạng | Chuẩn hóa về 10 chữ số, bỏ khoảng trắng, dấu chấm |
| Thiếu trình độ đầu vào | Để trống; F2.2 sẽ yêu cầu ghi ở F1.1 trước khi đăng ký mới |
| Học viên dưới 18 tuổi thiếu phụ huynh | Bổ sung từ phiếu đăng ký giấy trước khi chuyển; script chuyển đổi liệt kê các trường hợp còn thiếu (F1.1 chặn khi nhập tay, nhưng nhập thẳng vào CSDL thì không qua F1.1) |
| Số liệu học phí lệch giữa sổ thu và Excel | Lấy **sổ thu** (chứng từ gốc) làm chuẩn; chênh lệch ghi biên bản |

**Lịch chuyển đổi.** Đi cùng các giai đoạn ở mục 3: dữ liệu bước 1–4 trước giai đoạn 1; bước 5–6 trước giai đoạn 2; bước 7 trước giai đoạn 3 (chốt số dư vào ngày cuối tháng); bước 8–9 trước giai đoạn 4.

**Kiểm tra cuối.**
- Chạy `csdl/kiem_tra.sql`: 24 kiểm tra toàn vẹn và quy tắc phải **ĐẠT** (đã đạt với dữ liệu mẫu).
- Đếm số bản ghi từng bảng, so với số dòng của file nguồn.
- Tổng công nợ từ hệ thống (R3.3) = tổng còn nợ theo sổ thu tại ngày chốt; kiểm tra ngẫu nhiên 20 học viên.
- Sĩ số từng lớp (R2.2) = danh sách lớp giáo viên đang giữ.
- Biên bản nghiệm thu dữ liệu có chữ ký của trưởng bộ phận liên quan.

---

## 3. Phương pháp chuyển đổi

| Phương pháp | Ưu điểm | Nhược điểm | Phù hợp với trung tâm? |
|---|---|---|---|
| Trực tiếp | Nhanh, rẻ | Rủi ro cao: lỗi là ngừng thu tiền, ngừng điểm danh | Không – kế toán và giáo viên chưa quen |
| Song song (toàn bộ) | An toàn nhất | Mọi bộ phận làm 2 lần trong nhiều tuần, nhân sự ít không kham nổi | Không cho toàn hệ thống |
| **Theo giai đoạn** | Từng bộ phận làm quen dần; lỗi chỉ ảnh hưởng một phần; thứ tự khớp với phụ thuộc dữ liệu | Thời gian chuyển đổi dài hơn; giai đoạn chuyển tiếp phải nối dữ liệu cũ – mới | **Chọn** |
| Thăm dò (pilot) | Thử ở một chi nhánh trước | Trung tâm chỉ có **1 cơ sở** | Không |

**Đề xuất: theo giai đoạn**, đi theo 4 phân hệ của BFD, đúng thứ tự phụ thuộc dữ liệu (danh mục → lớp/lịch → học phí → học tập). Riêng giai đoạn học phí (rủi ro tiền bạc cao nhất, kế toán quen Excel – GĐ1 mục 3.3) chạy **song song 1 tháng** với sổ thu cũ.

| Giai đoạn | Phân hệ đưa vào dùng | Người dùng | Thời gian | Điều kiện sang giai đoạn sau |
|:-:|---|---|---|---|
| 1 | 1.0 Danh mục & hồ sơ, 5.1 tài khoản | TS, DT, QT | Tuần 1–2 | Hồ sơ HV/GV đã làm sạch, không còn trùng; mọi nhân viên đăng nhập được |
| 2 | 2.0 Lớp & lịch, đăng ký | TS, DT | Tuần 3–4, trước một kỳ khai giảng | Lịch kỳ mới xếp trên hệ thống không trùng; danh sách lớp khớp với giáo viên |
| 3 | 3.0 Học phí | KT | Tuần 5–8 (song song sổ thu trong tháng đầu) | Cuối tháng: tổng thu và công nợ trên hệ thống = sổ thu; kế toán tự lập phiếu không cần hỗ trợ |
| 4 | 4.0 Học tập, 5.2–5.4 thông báo, báo cáo; mở tài khoản HV/PH | GV, DT, GD, HV, PH | Tuần 9–10 | Giáo viên điểm danh đủ 2 tuần liền; giám đốc nhận báo cáo tháng từ hệ thống |

Sau giai đoạn 4, ngừng ghi sổ giấy và Excel; lưu trữ bản cuối của các file cũ (chỉ đọc) ít nhất 1 năm.

---

## 4. Kế hoạch kiểm thử

### 4.1 Các mức kiểm thử

Theo `thiet_ke/05_module.md` mục 8, bước 5. Mọi ca kiểm thử đều trỏ về một quy tắc hoặc một chứng từ đã có.

| Mức | Công cụ | Kết quả trên bản demo (05/10/2026) |
|---|---|---|
| Đơn vị | Dự án xUnit `demo/KiemThu`, chạy `dotnet test` | **38/38 đạt**: bảng quyết định 3.1 R1–R8, ưu đãi hết hạn/trùng loại, chứng từ DK2026-0158, chia đợt làm tròn nghìn, ngày buổi thứ n, phiếu thu QT05, điểm danh QT11, số tiền bằng chữ, mật khẩu, khóa tạm |
| Tích hợp (CSDL) | `csdl/kiem_tra.sql` qua `csdl/chay.ps1` | **24/24 đạt** |
| Đồng bộ thiết kế – mã | `demo/sinh_ma.py`: mã nguồn dùng đúng module, mã lỗi, mã màn hình của 05_module, 06_giao_dien | Đạt (81 tệp) |
| Phân quyền | Đăng nhập 8 vai trò, mở từng trang, so mã HTTP với ma trận | Đạt – cho phép/chặn khớp ma trận |
| Chấp nhận | Nhập lại chứng từ mẫu qua giao diện, so bản in với chứng từ | PT2026-0731 khớp; luồng đăng ký → học phí → phiếu thu chạy thật đạt (mục 4.2) |

### 4.2 Ca kiểm thử theo tiến trình

Kết quả: **Đạt** = đã chạy trên bản demo và đúng; **–** = module chưa có trong bản demo, sẽ chạy khi cài đặt đầy đủ.

| Mã | Tiến trình | Dữ liệu vào / thao tác | Kết quả mong đợi | Kết quả |
|---|---|---|---|---|
| TC01 | 1.1 | Thêm HV trùng ĐT + ngày sinh với hồ sơ có sẵn | Báo trùng, gợi ý mở hồ sơ cũ | Đạt |
| TC02 | 1.1 | Thêm HV dưới 18 tuổi không có phụ huynh; có phụ huynh nhưng thiếu quan hệ | Chặn, yêu cầu nhập đủ phụ huynh | Đạt |
| TC02b | 1.1 | Điện thoại "09ab"; có trình độ nhưng thiếu ngày kiểm tra | Câu lỗi tiếng Việt từ ràng buộc CSDL (N7) | Đạt |
| TC02c | 1.1 | Thêm HV dưới 18 có phụ huynh; sửa hồ sơ HV0412; học viên tự gửi form | Lưu HV mới (HV0490); sửa được; học viên bị chặn | Đạt |
| TC03 | 2.2 | HV0394 đăng ký GT-2701 | Lập phiếu DK2027-0001, trạng thái "Chờ lập học phí" | Đạt |
| TC04 | 2.2 | Đăng ký lại cùng lớp | Báo "Học viên đã có đăng ký ở lớp này" | Đạt |
| TC05 | 2.2 | Đăng ký lớp đã đủ sĩ số (tạm hạ tham số SISO_TOI_DA = 7) | Báo QT06, gợi ý lớp cùng khóa còn chỗ | Đạt |
| TC06 | 2.3 | Xếp buổi trùng giáo viên hoặc phòng | Từ chối (QT07) | – |
| TC07 | 2.4 | Chuyển lớp sau quá 3 buổi; bảo lưu quá 50% số buổi | Từ chối (QT08, QT09) | – |
| TC08 | 3.1 | HV cũ, một mình, 2 đợt (R4) | Giảm 10%: 2.400.000 → 2 × 1.080.000 | Đạt |
| TC09 | 3.1 | HV cũ, đóng một lần (R3) | Giảm 15% (trần QT04): 2.040.000, 1 đợt | Đạt |
| TC10 | 3.1 | Ưu đãi đã hết hạn tại ngày đăng ký | Không giảm | Đạt (đơn vị) |
| TC11 | 3.2 | Nộp đợt 1 = 500.000 (< 50%) | Báo QT05, nêu số tối thiểu 1.080.000 | Đạt |
| TC12 | 3.2 | Nộp vượt số còn nợ của đợt | Báo lỗi, nêu số còn nợ | Đạt |
| TC13 | 3.2 | Bỏ trống người nộp | Báo "Nhập họ tên người nộp." | Đạt |
| TC14 | 3.2 | Nộp đủ đợt 1 | Lập PT2027-0001, in 2 liên, còn nợ 1.080.000, số tiền bằng chữ đúng | Đạt |
| TC15 | 3.3 | Đăng ký Nghỉ học còn nợ | Không tính vào công nợ (QT10) | Đạt (CSDL) |
| TC16 | 4.1 | GV023 điểm danh buổi 19 TOE-2611 (đang trong giờ) | Lưu, cập nhật tỷ lệ chuyên cần, cảnh báo HV vắng > 20% (QT12) | Đạt |
| TC17 | 4.1 | Thiếu trạng thái một học viên / trạng thái lạ | Chặn, nêu mã học viên | Đạt |
| TC18 | 4.1 | Sửa điểm danh buổi đã qua 24 giờ | Chặn (QT11) | Đạt |
| TC19 | 4.1 | QL đào tạo (chỉ có quyền xem) gửi điểm danh | Chặn ở bộ lọc phân quyền | Đạt |
| TC20 | 4.2 | Nhập điểm "abc", 11, "7,5" | Báo lỗi, gợi ý dùng dấu chấm | Đạt |
| TC21 | 4.2 | Nhập rồi sửa điểm cuối kỳ | Lưu, ghi nhật ký "Sửa Diem TOE-2611/Cuối kỳ" | Đạt |
| TC22 | 4.3 | Tổng kết lớp: các trường hợp R1–R5 của bảng quyết định 4.3 | Xếp loại, Đạt/Không đạt đúng bảng | – |
| TC23 | 4.4 | Cấp chứng nhận cho HV không đạt | Từ chối (QT15) | – |
| TC24 | 5.1 | Sai mật khẩu 5 lần | Khóa 15 phút | Đạt |
| TC25 | 5.1 | Mọi thao tác ghi | Có dòng nhật ký cùng giao dịch | Đạt |
| TC26 | 5.3, 3.4 | Mở báo cáo tình trạng lớp, doanh thu | Số liệu = truy vấn trên view | Đạt |

### 4.3 Lỗi phát hiện khi kiểm thử và cách sửa

| Lỗi | Phát hiện ở | Nguyên nhân | Sửa |
|---|---|---|---|
| HV cũ không được giảm | TC08 | Ưu đãi mẫu hết hạn 31/12/2026, trước ngày demo | Kéo hạn đến 31/12/2027 (`csdl/sinh_du_lieu.py`); thêm ca đơn vị TC10 |
| Câu lỗi tiếng Anh "The NguoiNop field is required" | TC13 | ASP.NET tự thêm kiểm tra bắt buộc cho chuỗi không-null | Tắt kiểm tra ngầm (Program.cs); trang tự báo bằng tiếng Việt |
| Bỏ trống người nộp vẫn lưu được | TC13 | Trang tự điền người nộp cả khi lưu | Chỉ gợi ý người nộp khi mở form |
| Nhật ký ghi "Cu?i k?" | TC21 | Cột `NhatKy.MaDoiTuong` kiểu VARCHAR | Đổi thành NVARCHAR(100) (`csdl/schema.sql`) |

Sau mỗi lần sửa: chạy lại `csdl/chay.ps1` (24/24), `dotnet test` (38/38) và ca kiểm thử liên quan.

---

## 5. Hỗ trợ sử dụng và tài liệu

- **Hỗ trợ:** quản trị viên là đầu mối trong giờ hành chính; tháng đầu mỗi giai đoạn có thành viên nhóm trực tại trung tâm vào giờ cao điểm (khai giảng, hạn đóng học phí). Lỗi ghi vào sổ yêu cầu, xử lý theo thứ tự ưu tiên: **sửa lỗi > thích nghi > cải tiến** (chi tiết ở GĐ5).
- **Tài liệu hệ thống** (theo `thiet_ke/05_module.md` mục 8, bước 6): tài liệu mô tả (BFD, DFD, ERD, từ điển dữ liệu, sơ đồ module – đã có ở GĐ2, GĐ3) và tài liệu sử dụng – hướng dẫn riêng từng vai trò theo ma trận phân quyền, kèm hình chụp màn hình.
- **Quản lý cấu hình:** mã nguồn và CSDL trên GitHub (`main` được bảo vệ, sửa qua nhánh + PR); mỗi bản đưa vào sử dụng gắn thẻ phiên bản; hồ sơ cấu hình mục 1.4 cập nhật mỗi lần nâng cấp.
