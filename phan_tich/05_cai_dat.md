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
| 3 | Máy chủ | ASP.NET Core 8 Hosting Bundle + IIS | Chạy ứng dụng web; bật HTTPS bằng chứng chỉ của tên miền nội bộ. .NET 8 hết hỗ trợ 10/11/2026: khi đưa vào sử dụng thật thì cài .NET 10 (06_bao_tri mục 4, B1) |
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

| Mức | Công cụ | Kết quả trên bản demo (11/10/2026) |
|---|---|---|
| Đơn vị | Dự án xUnit `demo/KiemThu`, chạy `dotnet test` | **98/98 đạt** (80 ca cũ + 18 ca H1/H2): bảng quyết định 3.1 R1–R8, ưu đãi hết hạn/trùng loại, chứng từ DK2026-0158, chia đợt làm tròn nghìn, ngày buổi thứ n, phiếu thu QT05, điểm danh QT11, số tiền bằng chữ, mật khẩu, khóa tạm; bảng quyết định 4.3, chứng nhận QT15, trùng lịch QT07, cây quyết định 2.4, điểm "7,5", tổng hợp báo cáo 5.4; R3.1: tổng tiền theo đợt, ưu đãi, nhiều phiếu thu, bộ lọc quyền và thực đơn 8 vai trò, phạm vi HV/PH, tổng/Excel qua phân trang; F5.1b: chặn POST phiên bản, vẫn lưu tham số chính sách (EF Core InMemory) |
| Tích hợp (CSDL) | `csdl/kiem_tra.sql` qua `csdl/chay.ps1` | **24/24 đạt** |
| Đồng bộ thiết kế – mã | `demo/sinh_ma.py`: mã nguồn dùng đúng module, mã lỗi, mã màn hình của 05_module, 06_giao_dien | Đạt (126 tệp, kiểm tra lại ngày 14/10/2026 sau khi cập nhật main) |
| Phân quyền | Đăng nhập 8 vai trò, mở từng trang, so mã HTTP với ma trận | Đạt – cho phép/chặn khớp ma trận |
| Chấp nhận | Nhập lại chứng từ mẫu qua giao diện, so bản in với chứng từ | PT2026-0731 khớp; luồng đăng ký → học phí → phiếu thu chạy thật đạt (mục 4.2) |

### 4.2 Ca kiểm thử theo tiến trình

Kết quả: **Đạt** = đã chạy trên bản demo và đúng. 40 ca trước đợt H1/H2 đã chạy và đạt (ca 1.1–5.4 và N4, N6 sau đợt code bổ sung; TC43 sau đợt sửa giao diện, chạy lại phần bề rộng 375/1280 px cho 8 vai trò sau khi đổi sang giao diện v2). Bổ sung TC37 và TC38, tổng 42 ca; hai ca mới chưa chạy tích hợp SQL Server/HTTP trên máy hiện tại.

| Mã | Tiến trình | Dữ liệu vào / thao tác | Kết quả mong đợi | Kết quả |
|---|---|---|---|---|
| TC01 | 1.1 | Thêm HV trùng ĐT + ngày sinh với hồ sơ có sẵn | Báo trùng, gợi ý mở hồ sơ cũ | Đạt |
| TC02 | 1.1 | Thêm HV dưới 18 tuổi không có phụ huynh; có phụ huynh nhưng thiếu quan hệ | Chặn, yêu cầu nhập đủ phụ huynh | Đạt |
| TC02b | 1.1 | Điện thoại "09ab"; có trình độ nhưng thiếu ngày kiểm tra | Câu lỗi tiếng Việt từ ràng buộc CSDL (N7) | Đạt |
| TC02c | 1.1 | Thêm HV dưới 18 có phụ huynh; sửa hồ sơ HV0412; học viên tự gửi form | Lưu HV mới (HV0490); sửa được; học viên bị chặn | Đạt |
| TC03 | 2.2 | HV0394 đăng ký GT-2701 | Lập phiếu DK2027-0001, trạng thái "Chờ lập học phí" | Đạt |
| TC04 | 2.2 | Đăng ký lại cùng lớp | Báo "Học viên đã có đăng ký ở lớp này" | Đạt |
| TC05 | 2.2 | Đăng ký lớp đã đủ sĩ số (tạm hạ tham số SISO_TOI_DA = 7) | Báo QT06, gợi ý lớp cùng khóa còn chỗ | Đạt |
| TC06 | 2.3 | Sinh lịch GT-2701; thêm buổi học bù 06/01/2027 trùng giáo viên GV023, trùng phòng P305 với TOE-2611; đổi buổi 2 sang giờ, phòng của TOE-2611; NV tuyển sinh gửi xếp lịch | Sinh 10 buổi 09/01–13/03/2027; từ chối, nêu rõ lớp, buổi, giáo viên hoặc phòng bị trùng (QT07); buổi không trùng thì lưu; TS bị chặn | Đạt |
| TC07 | 2.4 | Mọi lá của cây quyết định 2.4: HV0480 chuyển lớp khi TOE-2611 đã học 18 buổi; chuyển sang lớp khác khóa, sang lớp đã kết thúc; HV0470 bảo lưu còn nợ; HV0480 bảo lưu khi đã học 18/24 buổi; HV0440 bảo lưu lần 2; HV0445 bảo lưu; HV0461 chuyển sang GT-2702 (lớp thêm tạm); HV0471 nghỉ học còn nợ | Từ chối đúng lá (QT08, QT09); bảo lưu đến 04/07/2027; đăng ký mới giữ số đã thu qua đăng ký gốc; khoản nợ của đăng ký nghỉ học không còn trong công nợ (QT10) | Đạt |
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
| TC20 | 4.2 | Nhập điểm "abc", 11, "8.25"; nhập "7,5" (sau bảo trì B7) | Báo lỗi, gợi ý cách nhập; "7,5" lưu thành 7.5 | Đạt |
| TC21 | 4.2 | Nhập rồi sửa điểm cuối kỳ | Lưu, ghi nhật ký "Sửa Diem TOE-2611/Cuối kỳ" | Đạt |
| TC22 | 4.3 | Tổng kết lớp: các trường hợp R1–R5 của bảng quyết định 4.3; duyệt lại IEK-2609 (đã xóa kết quả cũ); duyệt lớp TOE-2611 mới dạy 18/24 buổi | Xếp loại, Đạt/Không đạt đúng bảng; 9/9 kết quả trùng bảng điểm mẫu; lớp chưa đủ buổi bị chặn (QT13); giáo viên gửi duyệt bị chặn | Đạt |
| TC23 | 4.4 | Cấp chứng nhận cho HV0398 (không đạt); NV tuyển sinh gửi cấp; cấp cho HV0412, rồi chọn lại HV0412 cùng 2 HV khác | Từ chối (QT15); chặn ở bộ lọc phân quyền; cấp CN2027-0001, lần sau chỉ cấp 2 số mới, bản in trùng mẫu chứng nhận | Đạt |
| TC24 | 5.1 | Sai mật khẩu 5 lần, khởi động lại máy chủ rồi nhập đúng (sau bảo trì B4) | Khóa 15 phút; vẫn khóa sau khi khởi động lại (đếm lưu ở TaiKhoan) | Đạt |
| TC24b | 5.1 | hv0480 (tài khoản mới cấp) đăng nhập lần đầu, mở trang khác; đổi mật khẩu trùng mật khẩu cũ (bảo trì B2) | Mọi trang chuyển về đổi mật khẩu; mật khẩu mới phải khác mật khẩu cũ; đổi xong dùng bình thường | Đạt |
| TC25 | 5.1 | Mọi thao tác ghi | Có dòng nhật ký cùng giao dịch | Đạt |
| TC26 | 5.3, 3.4 | Mở báo cáo tình trạng lớp, doanh thu | Số liệu = truy vấn trên view | Đạt |
| TC27 | 1.2–1.4 | Thêm GV trùng email, điện thoại "09ab"; khóa học trọng số 10 – 30 – 50, trình độ A2 → A1, trùng mã; phòng sức chứa 0; Giám đốc (chỉ xem) gửi form | Câu lỗi tiếng Việt cho từng ràng buộc (N7); thêm GV024, khóa JP1, sửa P102; Giám đốc bị chặn | Đạt |
| TC28 | 2.1 | Mở lớp sĩ số 21; dùng phòng 16 chỗ cho sĩ số 20; phòng đang bảo trì; GV tiếng Anh dạy khóa tiếng Nhật; trùng mã lớp; sửa lớp đang học; chốt lớp chưa tới ngày khai giảng; chốt GT-2700 (khai giảng 02/01/2027, 0 đăng ký) | Từ chối (QT06, ngôn ngữ, trùng mã, trạng thái); lưu GT-2703 rồi đổi lịch tuần (bỏ T7, thêm T4); GT-2700 bị Hủy | Đạt |
| TC29 | 3.3, N5 | Kế toán mở công nợ ngày 04/01/2027, lọc quá hạn, xuất Excel; học viên, phụ huynh mở; QL đào tạo mở | 5 khoản 10.680.000 đ, 3 quá hạn = fn_CongNo; tệp .xlsx mở được bằng Excel, cột tiền là số; HV/PH chỉ thấy khoản của mình; QL đào tạo bị chặn | Đạt |
| TC31 | 5.2 | Khởi động máy chủ ngày 04/01/2027 (lô đầu ngày); thêm buổi học bù TOE-2611 ngày 09/01, duyệt lại kết quả HV0412; chạy lô lần 2, lần 3; học viên, phụ huynh, NV tuyển sinh, giáo viên mở R5.2 | Lần 1: 3 nhắc nợ quá hạn (QT16) + 1 cảnh báo vắng, nguy cơ không đạt (HV0486); lần 2: 10 thông báo lịch học bù, kết quả HV0412 không gửi lại vì đã có; lần 3: 0; HV/PH chỉ thấy thông báo của mình; giáo viên bị chặn | Đạt |
| TC30 | 5.1 | Quản trị viên cấp tài khoản hv0413; cấp trùng chủ (HV0412), sai vai trò, trùng tên; khóa nv005, tự khóa mình; đặt lại mật khẩu nv004 đang bị khóa tạm; tham số TY_LE_DOT_1 = 150, SISO_TOI_DA = 18; xem nhật ký; QL đào tạo mở trang | Cấp được, mật khẩu tạm 10 ký tự hiện một lần, đăng nhập phải đổi mật khẩu; các trường hợp sai bị từ chối; nv005 không vào được; nv004 hết khóa; tham số sai bị chặn, tham số đúng lưu và ghi nhật ký "20→18"; QL đào tạo bị chặn | Đạt |
| TC32 | 5.4, N5 | QL đào tạo mở R5.4a/R5.4b kỳ 04/01/2026 – 04/01/2027, kỳ 01/07 – 31/12/2026, nhập ngày đảo ngược, xuất Excel; giáo viên GV023 mở; kế toán, NV tuyển sinh, học viên mở | 5 lớp, tỷ lệ đạt 88,6%, chuyên cần 85,9%; số đạt, điểm TB, chuyên cần, buổi dạy = truy vấn KetQua, v_ChuyenCan, v_GiangDay; kỳ 6 tháng có so sánh kỳ trước; ngày đảo tự đổi lại; GV chỉ thấy TOE-2611 và dòng của mình; vai trò khác bị chặn | Đạt |
| TC33 | N4 | Quản trị viên bấm "Sao lưu ngay"; 7 vai trò khác mở trang; Giám đốc gửi form sao lưu; mở địa chỉ không có, gây lỗi máy chủ | Tạo tệp .bak (9 MB) trong thư mục sao lưu của SQL Server, hiện trong danh sách (lịch sử msdb), ghi nhật ký "Thêm SaoLuu"; vai trò khác bị chặn; sao lưu lỗi và mọi lỗi chưa lường trước hiện trang lỗi tiếng Việt kèm mã lỗi (404, 500), chi tiết chỉ ghi ở máy chủ. Phục hồi (RESTORE … WITH REPLACE) làm tay trên máy chủ, chưa chạy thử trong demo | Đạt |
| TC34 | N6, 3.2, 2.2 | Kế toán nhập dở phiếu thu HV0485 (người nộp, chuyển khoản, 500.000) rồi mở lại trang; bấm Lưu rồi chọn Hủy ở hộp xác nhận; gửi với số tiền vượt nợ; bấm nút Hủy của form; đăng xuất; NV tuyển sinh chọn lớp GT-2701 cho HV0412, bấm Lưu nháp, mở lại | Mở lại điền đủ các ô, báo "Đã mở lại bản nháp lưu lúc …"; chọn Hủy ở hộp xác nhận thì nháp còn; gửi đi thì nháp bị xóa, form lỗi vẫn giữ số đã nhập; nút Hủy đưa form về giá trị ban đầu và xóa nháp; nháp gắn tên đăng nhập, đăng xuất thì xóa; F2.2 mở lại đã chọn GT-2701 | Đạt |
| TC39 | 4.1 | Tra cứu chuyên cần R4.1: hv0480 (lớp TOE-2611 18 buổi), ph0320 (các con), hv0412 (3 lớp kết thúc); nv002/nv005 nhập mã HV; kế toán nv004 mở trang | Tỷ lệ chuyên cần, số buổi có mặt / vắng phép / vắng không phép khớp v_ChuyenCan; bấm xem từng buổi có ngày, trạng thái, ghi chú; cảnh báo vắng nếu > 20% (QT12); HV/PH chỉ thấy của mình; nv004 bị chuyển về /KhongCoQuyen | Đạt |
| TC40 | 4.3 | Phiếu báo kết quả học tập R4.3b: hv0412 (3 lớp Đạt), hv0398 (Không đạt), ph0301, gv023; lớp TOE-2611 chưa duyệt | Hiện đầy đủ điểm chuyên cần, giữa kỳ, cuối kỳ, tổng kết, xếp loại, Đạt/Không đạt, nhận xét, người duyệt, ngày duyệt; lớp chưa duyệt báo "chưa có kết quả"; bản in trùng chứng từ mẫu 4.2.4; HV/PH/GV chỉ thấy phạm vi của mình; vai trò không quyền bị chặn | Đạt |
| TC43 | Giao diện chung (06_giao_dien 5.2, 6) | Đăng nhập 8 vai trò, mở mọi mục thực đơn và các trang ngoài quyền; mở mọi trang ở bề rộng 375 và 1280 px; Ctrl+S, Esc, F1, nút Hủy trên F3.2; điện thoại "09ab", email "abc", điểm "11", "8.25", mã HV "Minh"; F4.1 bấm "Có mặt hết"; đăng xuất | Mọi mục thực đơn mở được, trang ngoài quyền bị chặn; không trang nào tràn ngang, mọi ô nhập có nhãn, chỉ một màu nhấn; phím tắt đúng; trình duyệt chặn và báo bằng tiếng Việt; học viên chưa chọn được đánh x; đăng xuất về trang đăng nhập | Đạt |
| TC37 | 3.1, N5 – R3.1 | nv004 xem toàn bộ, lọc lớp/mã HV, đổi số dòng/trang, xuất Excel; hv0412 và ph0301 lọc cả mã ngoài phạm vi; mở bằng 8 vai trò | Học phí gốc, tên và % ưu đãi, tổng giảm thực tế, số phải nộp và từng đợt (tiền, hạn, đã thu) đúng; tổng và Excel theo toàn bộ bộ lọc; không lộ khoản ngoài phạm vi; GV/ĐT/QT bị chặn, HV/PH/TS/KT/GĐ được xem; tổng phải nộp = SUM(DotHocPhi.SoTien) nối HocPhi | Chưa chạy tích hợp SQL Server/HTTP trên máy hiện tại; đã thêm kiểm thử tự động |
| TC38 | 5.1 – F5.1b, nâng cấp CSDL | Chạy `powershell -ExecutionPolicy Bypass -File csdl/kiem_tra_hung.ps1`; chạy 002 trên CSDL mẫu; nv003 mở F5.1b và gửi giả mạo `GiaTri[PHIEN_BAN_CSDL]=999`; sửa tham số chính sách bình thường | 002 thêm đúng một dòng phiên bản 2 khi thiếu; chạy lần hai không lỗi/trùng; nâng 1 lên 2 và không hạ phiên bản cao hơn; F5.1b chỉ hiện phiên bản, POST giả mạo bị từ chối; tham số khác vẫn lưu và ghi nhật ký | Chưa chạy tích hợp SQL Server/HTTP trên máy hiện tại; đã thêm kiểm thử tự động |

### 4.3 Lỗi phát hiện khi kiểm thử và cách sửa

| Lỗi | Phát hiện ở | Nguyên nhân | Sửa |
|---|---|---|---|
| HV cũ không được giảm | TC08 | Ưu đãi mẫu hết hạn 31/12/2026, trước ngày demo | Kéo hạn đến 31/12/2027 (`csdl/sinh_du_lieu.py`); thêm ca đơn vị TC10 |
| Câu lỗi tiếng Anh "The NguoiNop field is required" | TC13 | ASP.NET tự thêm kiểm tra bắt buộc cho chuỗi không-null | Tắt kiểm tra ngầm (Program.cs); trang tự báo bằng tiếng Việt |
| Bỏ trống người nộp vẫn lưu được | TC13 | Trang tự điền người nộp cả khi lưu | Chỉ gợi ý người nộp khi mở form |
| Nhật ký ghi "Cu?i k?" | TC21 | Cột `NhatKy.MaDoiTuong` kiểu VARCHAR | Đổi thành NVARCHAR(100) (`csdl/schema.sql`) |

Sau mỗi lần sửa: chạy lại `csdl/chay.ps1` (24/24), `dotnet test` (98/98 sau H1/H2) và ca kiểm thử liên quan. Đợt H1/H2 ngày 12/10/2026 đã build không lỗi/cảnh báo, xUnit 98/98 đạt và kiểm tra mã đạt với 122 tệp. Kiểm thử SQL Server/HTTP thật cho TC37, TC38 chưa chạy vì máy hiện tại không có instance SQL Server mẫu; xem `demo/KIEM_THU_HUNG.md` và `csdl/kiem_tra_hung.ps1` để chạy tiếp.

---

## 5. Hỗ trợ sử dụng và tài liệu

- **Hỗ trợ:** quản trị viên là đầu mối trong giờ hành chính; tháng đầu mỗi giai đoạn có thành viên nhóm trực tại trung tâm vào giờ cao điểm (khai giảng, hạn đóng học phí). Lỗi ghi vào sổ yêu cầu, xử lý theo thứ tự ưu tiên: **sửa lỗi > thích nghi > cải tiến** (chi tiết ở GĐ5).
- **Tài liệu hệ thống** (theo `thiet_ke/05_module.md` mục 8, bước 6): tài liệu mô tả (BFD, DFD, ERD, từ điển dữ liệu, sơ đồ module – đã có ở GĐ2, GĐ3) và tài liệu sử dụng – hướng dẫn riêng từng vai trò theo ma trận phân quyền, kèm hình chụp màn hình.
- **Quản lý cấu hình:** mã nguồn và CSDL trên GitHub (`main` được bảo vệ, sửa qua nhánh + PR); mỗi bản đưa vào sử dụng gắn thẻ phiên bản; hồ sơ cấu hình mục 1.4 cập nhật mỗi lần nâng cấp.
