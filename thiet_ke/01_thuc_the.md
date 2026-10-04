# 3.1 Thực thể và thuộc tính – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn Thiết kế hệ thống (KE_HOACH.md, mục 3.1). Đây là bước đầu tiên của quy trình thiết kế trong bài giảng (mục 5.2 – Mô hình hóa thực thể):
**xác định thực thể và thuộc tính (lập bảng)** → xác định quan hệ (3.2) → vẽ ERD (3.2) → chuẩn hóa (3.3) → CSDL vật lý (3.4).

Nguồn đầu vào:
- 20 đối tượng dữ liệu của Bài tập 1, góc nhìn Data (`bai_tap_1/`).
- 5 chứng từ mẫu: `phan_tich/02_thu_thap.md`, mục 4.2.
- Từ điển dữ liệu, cấu trúc các kho D1–D5: `phan_tich/04_dac_ta.md`, mục 4.1. Các thuộc tính thứ sinh lấy ở mục 4.5 của cùng tài liệu.
- Ma trận thực thể – chức năng: `phan_tich/03_chuc_nang.md`, mục 4.
- Quy tắc nghiệp vụ QT01–QT16: `phan_tich/02_thu_thap.md`, mục 9.3.

---

## 1. Cơ sở lý thuyết và quy ước

### 1.1 Các loại thực thể (theo bài giảng)

| Loại | Định nghĩa trong bài giảng | Ví dụ bài giảng | Ở hệ thống này |
|---|---|---|---|
| Xác thực | Đối tượng hữu hình | xe đạp, nhà, máy tính | Phòng học |
| Chức năng | Mục đích, chức năng, nhiệm vụ của con người, thiết bị trong tổ chức | sinh viên, nhân viên, khách hàng | Học viên, Phụ huynh, Giáo viên, Nhân viên, Tài khoản; Khóa học, Lớp học, Ưu đãi, Tham số (nhiệm vụ, chính sách do trung tâm đặt ra) |
| Sự kiện | Sự kiện, biến cố, thường có chứng từ | biên bản, biên lai, hóa đơn | Phiếu đăng ký, Buổi học, Học phí, Đợt học phí, Phiếu thu, Điểm thành phần, Kết quả học tập, Chứng nhận, Thông báo, Nhật ký |
| Quan hệ | Quan hệ giữa các đối tượng, thường có thêm dữ liệu riêng | kết hôn, hợp đồng | Học viên – Phụ huynh, Lịch tuần, Đăng ký học, Áp dụng ưu đãi, Chi tiết phiếu thu, Điểm danh |

### 1.2 Các loại thuộc tính và ký hiệu dùng trong tài liệu

| Ký hiệu | Loại thuộc tính | Ý nghĩa |
|---|---|---|
| `#` | Định danh (khóa) | Xác định duy nhất một cá thể. Một thuộc tính là **khóa đơn**, nhiều thuộc tính là **khóa kép**. Không được rỗng. |
| (để trống) | Mô tả | Làm rõ tính chất của cá thể; giá trị có thể trùng giữa các cá thể. |
| `QH → X` | Quan hệ | Giá trị là định danh của một cá thể trong thực thể X, dùng để liên kết. Sau này thành khóa ngoại. |
| `R` | Lặp | Nhận nhiều giá trị cho một cá thể. Chỉ xuất hiện ở bảng **thuộc tính ban đầu** (mục 3). Bảng thực thể cuối cùng không còn R. |
| `S` | Thứ sinh | Tính hoặc suy ra được từ thuộc tính khác. Mặc định **không lưu**. Riêng loại *S – chốt* được giữ lại theo nguyên tắc ở mục 5. |

Quy ước đặt tên: tên thực thể và tên thuộc tính viết có dấu. **Tên tệp và tên trường viết không dấu, dạng PascalCase**, và sẽ dùng nguyên văn ở `csdl/schema.sql` (3.4). Kiểu dữ liệu vật lý chưa xét ở bước này, đúng như bài giảng: *"không quan tâm đến cấu trúc lưu trữ vật lý (kiểu, loại) của bảng thực thể"*.

---

## 2. Bước 1 – Rà soát 20 đối tượng dữ liệu của Bài tập 1

Mỗi đối tượng BT1 được xử lý theo một trong 4 cách: **giữ** nguyên, **tách** (vì có thuộc tính lặp hoặc chứa hai đối tượng khác nhau), **gộp**, hoặc **loại** (vì là thứ sinh hoặc ngoài phạm vi).

| STT | Đối tượng BT1 | Xử lý | Thực thể thiết kế | Lý do |
|---|---|---|---|---|
| 1 | Tài khoản | Giữ | TaiKhoan | – |
| 2 | Học viên | Giữ | HocVien | Thông tin phụ huynh trong hồ sơ học viên là nhóm lặp, được tách sang dòng 3. |
| 3 | Phụ huynh | Tách | PhuHuynh + HocVienPhuHuynh | Một phụ huynh theo dõi nhiều học viên (BT1), và một học viên có thể có cả bố lẫn mẹ. Đây là quan hệ N–N, có thuộc tính riêng là *Quan hệ*. |
| 4 | Giáo viên | Giữ | GiaoVien | – |
| 5 | Nhân viên | Giữ | NhanVien | – |
| 6 | Khóa học | Giữ | KhoaHoc | Mục "chuẩn đầu ra" của BT1 tương ứng với thuộc tính *Trình độ đầu ra*. |
| 7 | Lớp học | Tách | LopHoc + LichTuan | *Lịch tuần* ("T3-T5 18:00-19:30") là thuộc tính lặp và không nguyên tố. |
| 8 | Đăng ký học | Tách | PhieuDangKy + DangKy | Một phiếu đăng ký có thể ghi nhiều lớp (chứng từ DK2026-0158 có 2 lớp). Mỗi lớp lại có trạng thái riêng: chuyển lớp, bảo lưu, nghỉ học. |
| 9 | Phòng học | Giữ | PhongHoc | Bỏ thuộc tính *Cơ sở*, vì phạm vi chỉ có 1 cơ sở (`phan_tich/01_ke_hoach.md`). |
| 10 | Lịch học | Gộp | BuoiHoc (lịch theo ngày), LichTuan (mẫu lịch trong tuần) | Theo BT1, "Lịch học" gồm lớp, giáo viên, ngày, giờ, phòng, tức là chính các buổi học đã xếp ngày. Phần lịch lặp lại trong tuần thuộc về LichTuan. |
| 11 | Buổi học | Giữ | BuoiHoc | Nhận thêm các thuộc tính của dòng 10. |
| 12 | Điểm danh | Giữ | DiemDanh | – |
| 13 | Học phí | Tách | HocPhi + DotHocPhi | Một khoản học phí chia tối đa 2 đợt, mỗi đợt có số tiền và hạn đóng riêng (QT05), nên các đợt là nhóm lặp. |
| 14 | Ưu đãi | Tách | UuDai + ApDungUuDai | Một khoản học phí được hưởng nhiều ưu đãi cộng dồn (QT04), và một ưu đãi áp cho nhiều khoản: quan hệ N–N. |
| 15 | Phiếu thu | Tách | PhieuThu + ChiTietPhieuThu | Một phiếu thu gồm nhiều dòng (lớp, đợt). Xem chứng từ PT2026-0731. |
| 16 | Công nợ | **Loại** | – | Thứ sinh: Công nợ = Phải nộp − Đã thu (QT10). Tính khi cần, không lưu. |
| 17 | Bảng điểm | Đổi | Diem | Bảng điểm là một **báo cáo**. Dữ liệu gốc của nó là từng cột điểm của từng học viên (Diem). Trọng số thuộc về KhoaHoc. |
| 18 | Kết quả học tập | Giữ | KetQua | – |
| 19 | Chứng nhận | Giữ | ChungNhan | – |
| 20 | Thông báo | Giữ | ThongBao | – |
| + | (không có ở BT1) | Thêm | NhatKy | DFD có luồng "Nhật ký hệ thống" (5.1) và kho D5 có {Nhật ký}. |
| + | (không có ở BT1) | Thêm | ThamSo | Luồng "Tài khoản, phân quyền, cấu hình" vào 5.1 có phần *Tham số cấu hình*. Nếu không có thực thể chứa phần này, DFD và ERD sẽ không nhất quán. |

Kết quả: 20 đối tượng → **26 thực thể**.

### 2.1 Những đối tượng đã cân nhắc nhưng không lập thành thực thể

| Đối tượng | Lý do không lập |
|---|---|
| Công nợ | Thứ sinh (xem trên). |
| Cơ sở | Ngoài phạm vi (1 cơ sở). Hướng phát triển đa cơ sở ở GĐ5 sẽ cần thêm. |
| Đầu điểm (cột điểm) | KE_HOACH dự kiến có thực thể này. Khi thiết kế, ta thấy chỉ có 2 loại điểm cố định là Giữa kỳ và Cuối kỳ, còn trọng số đi theo từng khóa học (ba trường TS ở KhoaHoc). Vì vậy *Loại điểm* chỉ là một miền giá trị trong Diem, không cần thực thể riêng. |
| Vai trò, Quyền | Có 8 vai trò cố định, và quyền của mỗi vai trò được quy định bằng ma trận phân quyền ở 3.5. Vì vậy *Vai trò* là miền giá trị trong TaiKhoan. Nhóm lặp {Quyền} trong từ điển dữ liệu được xác định qua vai trò, không lưu theo từng tài khoản. Nếu sau này cần quyền tùy biến cho từng người, sẽ thêm VaiTro và Quyen (GĐ5). |
| Đơn chuyển lớp, bảo lưu | Là chứng từ vào của 2.4. Nội dung cần giữ lại (loại thay đổi, ngày, lý do, lớp đích) được ghi vào DangKy (TrangThai, NgayThayDoi, LyDo, MaLopGoc) và NhatKy. |
| Kết quả kiểm tra đầu vào | Chỉ cần trình độ và ngày kiểm tra **gần nhất**, nên lưu trong HocVien. Nếu cần lưu lịch sử nhiều lần kiểm tra thì tách thành thực thể riêng. |
| Ngôn ngữ, Bộ phận, Trình độ | Danh sách ngắn và cố định, nên dùng miền giá trị (ràng buộc CHECK ở 3.4). |
| Các báo cáo MIS | Báo cáo doanh thu, tuyển sinh, chuyên cần, kết quả đều tính ra từ dữ liệu gốc, nên là thứ sinh, không lưu. |

---

## 3. Bước 2 – Thuộc tính ban đầu: đánh dấu R và S

Làm theo cách của bài giảng (Ví dụ 3 – Hóa đơn): liệt kê mọi thuộc tính lấy từ từ điển dữ liệu và chứng từ, đánh dấu (R) và (S). Dưới đây chỉ ghi các đối tượng có thuộc tính lặp hoặc thứ sinh. Các đối tượng còn lại chuyển thẳng sang mục 4.

| Đối tượng ban đầu | Thuộc tính ban đầu (lấy từ từ điển dữ liệu và chứng từ) | Xử lý |
|---|---|---|
| Học viên | #Mã HV, Họ tên, Ngày sinh, Giới tính, Điện thoại, Email, Địa chỉ, Trình độ đầu vào, Ngày kiểm tra, Họ tên PH (R), Điện thoại PH (R), Quan hệ (R), Trạng thái, Tuổi (S) | Nhóm lặp {PH} → PhuHuynh + HocVienPhuHuynh. Bỏ Tuổi. |
| Lớp | #Mã lớp, Mã KH, Mã GV, Ngày khai giảng, Thứ (R), Giờ bắt đầu (R), Giờ kết thúc (R) & (S), Phòng (R), Sĩ số tối đa, Trạng thái, Số đăng ký (S), Ngày kết thúc (S) | Nhóm lặp {Thứ, Giờ, Phòng} → LichTuan. Bỏ Số đăng ký, Ngày kết thúc, Giờ kết thúc (= Giờ bắt đầu + Thời lượng buổi của khóa). |
| Phiếu đăng ký | #Số phiếu ĐK, Ngày ĐK, Mã HV, (thông tin HV, PH), Stt (R), Mã lớp (R), Tên KH (R), Lịch học (R), Khai giảng (R), Học phí (R), Trạng thái (R), Hạn bảo lưu (R), Đăng ký gốc (R), Mã UĐ, Tổng học phí (S), Giảm (S), Phải nộp (S), Mã NV tiếp nhận | Nhóm lặp {dòng lớp} → DangKy. Bỏ Stt (ít ý nghĩa, giống bài giảng bỏ "Stt") và 3 thuộc tính S. Tên KH, Lịch, Khai giảng đã có ở KhoaHoc, LopHoc. |
| Học phí phải thu | Số phiếu ĐK, Mã lớp, Học phí gốc, Mã UĐ (R), Tỷ lệ giảm (S), Phải nộp (S), Đợt (R), Số tiền đợt (R), Hạn đóng (R) | {Mã UĐ} → ApDungUuDai; {Đợt} → DotHocPhi. Bỏ Phải nộp. Tỷ lệ giảm giữ dạng *chốt* (mục 5). |
| Phiếu thu | #Số PT, Ngày thu, Người nộp, Mã HV, Số phiếu ĐK, Stt (R), Mã lớp (R), Học phí (R), Giảm (R), Phải nộp (R) & (S), Nộp lần này (R), Đợt, Tổng nộp (S), Viết bằng chữ (S), Còn nợ (S), Hạn đợt 2, Hình thức, Người thu | Nhóm lặp {dòng} → ChiTietPhieuThu. Bỏ các thuộc tính S. Học phí, Giảm đã có ở HocPhi. Hạn đợt 2 đã có ở DotHocPhi. |
| Bảng điểm (gắn với khóa) | Mã lớp, Trọng số CC, Trọng số GK, Trọng số CK, Mã HV (R), Điểm CC (R) & (S), Điểm GK (R), Điểm CK (R), Tổng kết (R) & (S), Xếp loại (R) & (S), Kết quả (R) & (S), Nhận xét | Trọng số → KhoaHoc. {Mã HV, GK, CK} → Diem. Tổng kết, Xếp loại, Kết quả → KetQua (S – chốt). Bỏ Điểm CC. |
| Tài khoản | #Tên đăng nhập, Mật khẩu, Vai trò, Quyền (R), Mã người dùng, Trạng thái | {Quyền} được xác định qua Vai trò (mục 2.1). |
| Giáo viên | #Mã GV, …, Ngôn ngữ dạy, Chuyên môn, Bằng cấp | Bằng cấp và Chuyên môn là **văn bản mô tả**: hệ thống không lọc hay tính toán theo từng bằng, nên không coi là R. Ngôn ngữ dạy là đơn trị, theo giả định G1 (mục 7). |

> Tách nhóm lặp ở bước này để có danh sách thực thể cho ERD. Mục 3.3 (chuẩn hóa) sẽ làm lại một cách hình thức trên 3 chứng từ *Phiếu đăng ký*, *Phiếu thu*, *Bảng điểm lớp*. Kết quả 3NF ở đó phải trùng với các bảng ở mục 4 dưới đây.

---

## 4. Bước 3 – Bảng thực thể chi tiết

Cột *Bắt buộc*: **Có** = không được rỗng. Mọi thuộc tính `#` đều bắt buộc.

### 4.1 Kho D1 – Hồ sơ

#### E01. Học viên (`HocVien`) – thực thể chức năng

Người học tại trung tâm. Được tạo và cập nhật ở 1.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | # | Có | Hệ thống cấp, dạng HVxxxx (vd HV0412) |
| Họ và tên | HoTen | | Có | |
| Ngày sinh | NgaySinh | | Có | Dùng để kiểm tra trùng và tính tuổi (S) |
| Giới tính | GioiTinh | | Có | [Nam \| Nữ] |
| Điện thoại | DienThoai | | Có | 10 chữ số. Bộ (Điện thoại, Ngày sinh) dùng để kiểm tra trùng hồ sơ (1.1) |
| Email | Email | | Không | |
| Địa chỉ | DiaChi | | Không | Để một trường, vì không cần thống kê theo phường, quận |
| Trình độ đầu vào | TrinhDoDauVao | | Không | [A1 \| A2 \| B1 \| B2 \| C1 \| C2] (CEFR). Phải có trước khi đăng ký học (2.2) |
| Ngày kiểm tra | NgayKiemTra | | Không | Ngày kiểm tra đầu vào gần nhất. Bắt buộc khi có Trình độ |
| Ngày tiếp nhận | NgayTiepNhan | | Có | Ngày lập hồ sơ |
| Trạng thái | TrangThai | | Có | [Hoạt động \| Ngừng]. Hồ sơ "Ngừng" không bị xóa (04_dac_ta mục 1.1) |

Thứ sinh, không lưu: *Tuổi* (từ Ngày sinh); *Học viên cũ* (QT01: có đăng ký ở một lớp đã Kết thúc và không ở trạng thái Nghỉ học).

#### E02. Phụ huynh (`PhuHuynh`) – thực thể chức năng

Người giám hộ, bắt buộc với học viên dưới 18 tuổi. Được tạo ở 1.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã phụ huynh | MaPH | # | Có | Dạng PHxxxx |
| Họ và tên | HoTen | | Có | |
| Điện thoại | DienThoai | | Có | Dùng để nhận thông báo (5.2) |
| Email | Email | | Không | |

#### E03. Học viên – Phụ huynh (`HocVienPhuHuynh`) – thực thể quan hệ

Liên kết phụ huynh với học viên mà họ giám hộ (quan hệ N–N). Được tạo ở 1.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → HocVien | Có | |
| Mã phụ huynh | MaPH | #, QH → PhuHuynh | Có | |
| Quan hệ | QuanHe | | Có | [Bố \| Mẹ \| Người giám hộ] |

Ràng buộc: học viên dưới 18 tuổi phải có ít nhất 1 dòng (04_dac_ta mục 1.1). Dòng này cũng là căn cứ phân quyền: *phụ huynh chỉ xem được thông tin của con mình*.

#### E04. Giáo viên (`GiaoVien`) – thực thể chức năng

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã giáo viên | MaGV | # | Có | Dạng GVxxx |
| Họ và tên | HoTen | | Có | |
| Ngày sinh | NgaySinh | | Có | |
| Điện thoại | DienThoai | | Có | Trùng Điện thoại hoặc Email thì coi là trùng giáo viên (1.2) |
| Email | Email | | Có | |
| Ngôn ngữ dạy | NgonNguDay | | Có | [Anh \| Trung \| Nhật \| Hàn]. Dùng ở 2.1: giáo viên phải dạy đúng ngôn ngữ của khóa |
| Chuyên môn | ChuyenMon | | Không | Văn bản mô tả, vd "IELTS; tiếng Anh thiếu nhi" |
| Bằng cấp | BangCap | | Không | Văn bản mô tả |
| Tình trạng làm việc | TinhTrang | | Có | [Đang dạy \| Tạm nghỉ \| Nghỉ việc] |

#### E05. Khóa học (`KhoaHoc`) – thực thể chức năng

Chương trình đào tạo, từ đó trung tâm mở nhiều lớp. Được tạo ở 1.3.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã khóa học | MaKH | # | Có | Viết tắt tên khóa, vd IEK, GT |
| Tên khóa học | TenKH | | Có | vd "IELTS Kids Foundation" |
| Ngôn ngữ | NgonNgu | | Có | Cùng miền với GiaoVien.NgonNguDay |
| Trình độ đầu vào | TrinhDoDauVao | | Có | CEFR |
| Trình độ đầu ra | TrinhDoDauRa | | Có | CEFR, cao hơn trình độ đầu vào |
| Số buổi | SoBuoi | | Có | > 0 |
| Thời lượng buổi | ThoiLuongBuoi | | Có | Số phút, > 0 (vd 90) |
| Học phí | HocPhi | | Có | Đồng, > 0. Là giá **hiện hành**, mỗi đăng ký sẽ chốt giá riêng ở HocPhi.HocPhiGoc |
| Trọng số chuyên cần | TSChuyenCan | | Có | %, |
| Trọng số giữa kỳ | TSGiuaKy | | Có | %, |
| Trọng số cuối kỳ | TSCuoiKy | | Có | %. Tổng 3 trọng số = 100 (QT13) |
| Mô tả | MoTa | | Không | |
| Trạng thái | TrangThai | | Có | [Đang mở \| Ngừng] |

Ba trọng số là ba thuộc tính khác nghĩa nhau và luôn đủ ba, nên **không** phải nhóm lặp.

#### E06. Phòng học (`PhongHoc`) – thực thể xác thực

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã phòng | MaPhong | # | Có | vd P203 |
| Tên phòng | TenPhong | | Có | |
| Sức chứa | SucChua | | Có | > 0 (QT06: sĩ số lớp không vượt sức chứa) |
| Thiết bị | ThietBi | | Không | Văn bản mô tả |
| Tình trạng | TinhTrang | | Có | [Sẵn sàng \| Bảo trì \| Ngừng]. Chỉ phòng "Sẵn sàng" mới được xếp lịch |

### 4.2 Kho D2 – Lớp và lịch học

#### E07. Lớp học (`LopHoc`) – thực thể chức năng

Một lần tổ chức một khóa học. Được tạo ở 2.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã lớp | MaLop | # | Có | vd IEK-2609 |
| Mã khóa học | MaKH | QH → KhoaHoc | Có | |
| Mã giáo viên phụ trách | MaGV | QH → GiaoVien | Có | Phân công ở 2.1 |
| Ngày khai giảng | NgayKhaiGiang | | Có | |
| Sĩ số tối đa | SiSoToiDa | | Có | ≤ tham số SISO_TOI_DA (20) và ≤ sức chứa phòng ở LichTuan (QT06) |
| Trạng thái | TrangThai | | Có | [Dự kiến \| Đang học \| Kết thúc \| Hủy] |

Thứ sinh, không lưu: *Số đăng ký* (số đăng ký giữ chỗ, 04_dac_ta mục 4.5); *Tỷ lệ lấp đầy*; *Ngày kết thúc* (= ngày của buổi học cuối).

#### E08. Lịch tuần (`LichTuan`) – thực thể quan hệ (Lớp học – Phòng học)

Mẫu lịch lặp lại hằng tuần của lớp. 2.3 dùng mẫu này để sinh các buổi học. Tách ra từ thuộc tính lặp *Lịch tuần* của lớp.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã lớp | MaLop | #, QH → LopHoc | Có | |
| Thứ | Thu | # | Có | [2 … 7 \| 8] (8 = Chủ nhật) |
| Giờ bắt đầu | GioBatDau | | Có | |
| Mã phòng | MaPhong | QH → PhongHoc | Có | Phòng mặc định của khung giờ này |

Giả định: mỗi thứ trong tuần, một lớp học tối đa một ca. *Giờ kết thúc* = Giờ bắt đầu + KhoaHoc.ThoiLuongBuoi (S, không lưu). Nhờ tách bảng này, chuỗi "T3-T5 18:00-19:30" trên phiếu đăng ký trở thành các giá trị **nguyên tố**, đúng điều kiện bảng thực thể của bài giảng.

#### E09. Buổi học (`BuoiHoc`) – thực thể sự kiện

Một buổi học cụ thể đã xếp ngày. Được tạo ở 2.3, đổi trạng thái khi giáo viên dạy (4.1).

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã lớp | MaLop | #, QH → LopHoc | Có | |
| Số thứ tự buổi | SoBuoi | # | Có | 1, 2, 3… Buổi học bù lấy số tiếp theo |
| Ngày | Ngay | | Có | |
| Giờ bắt đầu | GioBatDau | | Có | |
| Giờ kết thúc | GioKetThuc | | Có | > Giờ bắt đầu. **Lưu** riêng, vì buổi thi hoặc học bù có thể dài ngắn khác thời lượng chuẩn. Kiểm tra trùng lịch (QT07) dựa trên giờ này |
| Mã phòng | MaPhong | QH → PhongHoc | Có | Mặc định lấy từ LichTuan, có thể đổi |
| Mã giáo viên dạy | MaGV | QH → GiaoVien | Có | Mặc định là giáo viên phụ trách lớp, khác đi khi có người **dạy thay**. Vì vậy không phải thứ sinh |
| Loại buổi | Loai | | Có | [Thường \| Học bù] |
| Nội dung | NoiDung | | Không | Nội dung bài học (BT1) |
| Trạng thái | TrangThai | | Có | [Kế hoạch \| Đã dạy \| Hủy] |

#### E10. Phiếu đăng ký (`PhieuDangKy`) – thực thể sự kiện

Chứng từ ghi danh (02_thu_thap mục 4.2.1). Được tạo ở 2.2.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Số phiếu đăng ký | SoPhieuDK | # | Có | Dạng DKyyyy-xxxx (vd DK2026-0158) |
| Ngày đăng ký | NgayDK | | Có | |
| Mã nhân viên tiếp nhận | MaNV | QH → NhanVien | Có | Lấy từ phiên đăng nhập |

Học viên của phiếu được xác định qua các dòng DangKy, không lưu lặp lại ở phiếu. Thứ sinh, không lưu: *Tổng học phí*, *Giảm*, *Phải nộp* (cộng từ HocPhi của các dòng).

#### E11. Đăng ký học (`DangKy`) – thực thể quan hệ (Học viên – Lớp học: "Học")

Một học viên học một lớp. Đây là quan hệ N–N, giống *Sinh viên – Môn học* trong bài giảng. Được tạo ở 2.2, cập nhật ở 2.4.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → HocVien | Có | |
| Mã lớp | MaLop | #, QH → LopHoc | Có | Mỗi học viên chỉ có 1 đăng ký ở mỗi lớp (2.2 từ chối đăng ký trùng) |
| Số phiếu đăng ký | SoPhieuDK | QH → PhieuDangKy | Có | Đăng ký sinh ra do chuyển lớp thì giữ số phiếu của đăng ký gốc |
| Trạng thái | TrangThai | | Có | [Đã đăng ký \| Chuyển lớp \| Bảo lưu \| Nghỉ học] |
| Ngày thay đổi | NgayThayDoi | | Không | Ngày chấp nhận chuyển lớp, bảo lưu hoặc nghỉ học (2.4) |
| Lý do | LyDo | | Không | Lý do ghi trên đơn chuyển lớp, bảo lưu |
| Hạn bảo lưu | HanBaoLuu | S – chốt | Không | = Ngày thay đổi + 6 tháng (QT09). Chỉ có giá trị khi Trạng thái = Bảo lưu |
| Mã lớp gốc | MaLopGoc | QH → DangKy (bậc 1) | Không | Cặp (MaHV, MaLopGoc) trỏ tới **đăng ký gốc** của cùng học viên, khi đăng ký này sinh ra do chuyển lớp hoặc học lại sau bảo lưu. Dùng để cộng *Đã thu* (04_dac_ta mục 4.5) |

Thứ sinh, không lưu: *Hiệu lực* (= Đã đăng ký và Đã thu ≥ 50% Phải nộp); *Đã thu*, *Còn nợ* (từ D3).

Khóa là **(MaHV, MaLop)**, không phải Số phiếu ĐK như từ điển dữ liệu 2.4 đã ghi. Lý do: khi mô hình hóa ta thấy một phiếu có nhiều lớp, nên Số phiếu ĐK chỉ định danh được *phiếu*, còn mỗi dòng đăng ký được định danh bằng (học viên, lớp). Sổ điểm danh và bảng điểm cũng tra theo đúng cặp này.

Vì quan hệ với đăng ký gốc luôn là của *cùng một học viên*, nên chỉ cần thêm một thuộc tính MaLopGoc.

### 4.3 Kho D3 – Học phí

#### E12. Ưu đãi (`UuDai`) – thực thể chức năng

Chính sách giảm học phí. Kế toán nhập qua 3.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã ưu đãi | MaUD | # | Có | vd UD-HVCU |
| Tên ưu đãi | TenUD | | Có | |
| Loại ưu đãi | LoaiUD | | Có | [Học viên cũ \| Đăng ký nhóm \| Đóng một lần]. Ứng với điều kiện C1, C2, C3 trong bảng quyết định 3.1 |
| Tỷ lệ giảm | TyLeGiam | | Có | %, trong khoảng 0–100 |
| Ngày bắt đầu | NgayBatDau | | Có | |
| Ngày kết thúc | NgayKetThuc | | Có | ≥ Ngày bắt đầu. Chỉ áp dụng ưu đãi còn hiệu lực (QT04) |
| Điều kiện áp dụng | DieuKien | | Không | Văn bản mô tả |

#### E13. Học phí (`HocPhi`) – thực thể sự kiện (khoản phải thu của một đăng ký)

Được tạo ở 3.1 khi có đăng ký mới. Đăng ký có MaLopGoc thì **không** có dòng HocPhi riêng (04_dac_ta mục 2.1).

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → DangKy | Có | Quan hệ 1–1 với DangKy |
| Mã lớp | MaLop | #, QH → DangKy | Có | |
| Học phí gốc | HocPhiGoc | S – chốt | Có | = KhoaHoc.HocPhi tại ngày lập |
| Tỷ lệ giảm | TyLeGiam | S – chốt | Có | Tính theo bảng quyết định 3.1, tối đa 15% (QT04) |
| Số đợt | SoDot | | Có | [1 \| 2]. Do học viên chọn (QT03, QT05) |
| Ngày lập | NgayLap | | Có | |

Thứ sinh, không lưu: *Phải nộp* = Học phí gốc × (1 − Tỷ lệ giảm) = tổng Số tiền các đợt.

Có cùng khóa với DangKy nhưng vẫn để **riêng**. Bài giảng có bước "trộn bảng cùng khóa *mô tả cùng đối tượng*", mà ở đây là hai đối tượng khác nhau:
- DangKy là *việc học*, thuộc D2, do 2.2 tạo.
- HocPhi là *nghĩa vụ tài chính*, thuộc D3, do 3.1 tạo vào lúc khác.
- Đăng ký sinh ra do chuyển lớp không có khoản phải thu. Nếu trộn hai bảng, các trường học phí của những dòng này sẽ để rỗng.

#### E14. Đợt học phí (`DotHocPhi`) – thực thể sự kiện

Các đợt đóng của một khoản học phí. Tách ra từ nhóm lặp {Đợt + Số tiền + Hạn đóng}.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → HocPhi | Có | |
| Mã lớp | MaLop | #, QH → HocPhi | Có | |
| Đợt | Dot | # | Có | [1 \| 2] |
| Số tiền | SoTien | S – chốt | Có | Đợt 1 ≥ 50% Phải nộp (QT05) |
| Hạn đóng | HanDong | S – chốt | Có | Đợt 1: ngày khai giảng; đợt 2: ngày của buổi học giữa khóa |

#### E15. Áp dụng ưu đãi (`ApDungUuDai`) – thực thể quan hệ (Học phí – Ưu đãi: "Được hưởng")

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → HocPhi | Có | |
| Mã lớp | MaLop | #, QH → HocPhi | Có | |
| Mã ưu đãi | MaUD | #, QH → UuDai | Có | |

#### E16. Phiếu thu (`PhieuThu`) – thực thể sự kiện

Chứng từ một lần nộp tiền (02_thu_thap mục 4.2.2). Được tạo ở 3.2.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Số phiếu thu | SoPT | # | Có | Dạng PTyyyy-xxxx, tăng dần |
| Ngày thu | NgayThu | | Có | |
| Người nộp | NguoiNop | | Có | Họ tên người trực tiếp nộp: học viên hoặc phụ huynh |
| Hình thức | HinhThuc | | Có | [Tiền mặt \| Chuyển khoản] |
| Mã nhân viên thu | MaNV | QH → NhanVien | Có | Lấy từ phiên đăng nhập |

Thứ sinh, không lưu: *Tổng tiền*, *Viết bằng chữ*, *Còn nợ sau phiếu*.

#### E17. Chi tiết phiếu thu (`ChiTietPhieuThu`) – thực thể quan hệ (Phiếu thu – Đợt học phí)

Một dòng trên phiếu thu: số tiền nộp cho một đợt của một đăng ký.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Số phiếu thu | SoPT | #, QH → PhieuThu | Có | |
| Mã học viên | MaHV | #, QH → DotHocPhi | Có | |
| Mã lớp | MaLop | #, QH → DotHocPhi | Có | |
| Đợt | Dot | #, QH → DotHocPhi | Có | |
| Số tiền | SoTien | | Có | > 0 và ≤ số còn nợ của đợt (04_dac_ta mục 1.4) |

Mã HV nằm ở từng dòng chứ không ở đầu phiếu, vì một phiếu thu có thể thu cho nhiều học viên (một phụ huynh nộp tiền cho hai con). Khi đó khóa (SoPT, MaHV, MaLop, Dot) là tối thiểu. Đợt cũng nằm ở từng dòng chứ không ở đầu phiếu. Mục 6.2 cho thấy chính chứng từ mẫu cần như vậy.

### 4.4 Kho D4 – Học tập

#### E18. Điểm danh (`DiemDanh`) – thực thể quan hệ (Đăng ký – Buổi học: "Tham dự")

Được tạo ở 4.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → DangKy | Có | |
| Mã lớp | MaLop | #, QH → DangKy, BuoiHoc | Có | Dùng chung cho cả hai quan hệ. Vì vậy học viên chỉ được điểm danh ở buổi học của **chính lớp mình** |
| Số thứ tự buổi | SoBuoi | #, QH → BuoiHoc | Có | |
| Trạng thái | TrangThai | | Có | [x \| M \| P \| K] = có mặt, đi muộn, vắng có phép, vắng không phép (QT11) |
| Ghi chú | GhiChu | | Không | |

Thứ sinh, không lưu: *Tỷ lệ chuyên cần* (QT11), *Cảnh báo vắng* (QT12).

#### E19. Điểm thành phần (`Diem`) – thực thể sự kiện

Một cột điểm của một học viên trong một lớp. Được tạo ở 4.2.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → DangKy | Có | |
| Mã lớp | MaLop | #, QH → DangKy | Có | |
| Loại điểm | LoaiDiem | # | Có | [Giữa kỳ \| Cuối kỳ] |
| Điểm | Diem | | Có | 0 ≤ Điểm ≤ 10 |
| Nhận xét | NhanXet | | Không | |

#### E20. Kết quả học tập (`KetQua`) – thực thể sự kiện

Kết quả cuối khóa đã được duyệt. Được tạo ở 4.3.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã học viên | MaHV | #, QH → DangKy | Có | Quan hệ 1–1 với DangKy (không bắt buộc phải có) |
| Mã lớp | MaLop | #, QH → DangKy | Có | |
| Điểm tổng kết | DiemTongKet | S – chốt | Có | 0–10, làm tròn 1 chữ số thập phân (QT13) |
| Xếp loại | XepLoai | S – chốt | Có | [Giỏi \| Khá \| Trung bình \| Không đạt] (QT14) |
| Kết quả | KetQua | S – chốt | Có | [Đạt \| Không đạt] (QT15) |
| Người duyệt | MaNVDuyet | QH → NhanVien | Không | QL đào tạo duyệt. Rỗng = chưa duyệt |
| Ngày duyệt | NgayDuyet | | Không | |

Thứ sinh, không lưu: *Tỷ lệ chuyên cần*, *Điểm chuyên cần* (= tỷ lệ × 10).

Để riêng, không trộn vào DangKy, với lý do giống HocPhi: thuộc D4, do 4.3 tạo khi lớp kết thúc, và đăng ký đã chuyển lớp hoặc nghỉ học thì không có kết quả.

#### E21. Chứng nhận (`ChungNhan`) – thực thể sự kiện

Được tạo ở 4.4, chỉ cho học viên có Kết quả = Đạt (QT15).

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Số chứng nhận | SoCN | # | Có | Dạng CNyyyy-xxxx, tăng dần theo năm |
| Mã học viên | MaHV | QH → KetQua | Có | Cặp (MaHV, MaLop) là duy nhất: mỗi kết quả có tối đa 1 chứng nhận |
| Mã lớp | MaLop | QH → KetQua | Có | |
| Ngày cấp | NgayCap | | Có | |

Thứ sinh, không lưu: *Từ ngày – đến ngày* của khóa in trên chứng nhận (lấy từ buổi đầu và buổi cuối của lớp).

### 4.5 Kho D5 – Tài khoản và thông báo

#### E22. Nhân viên (`NhanVien`) – thực thể chức năng

Nhân viên văn phòng, kể cả Giám đốc, QL đào tạo và Quản trị viên. Được tạo ở 5.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã nhân viên | MaNV | # | Có | Dạng NVxxx |
| Họ và tên | HoTen | | Có | |
| Bộ phận | BoPhan | | Có | [Ban giám đốc \| Đào tạo \| Tuyển sinh – CSHV \| Kế toán \| CNTT] (02_thu_thap mục 2.1) |
| Chức vụ | ChucVu | | Có | |
| Điện thoại | DienThoai | | Có | |
| Email | Email | | Có | |
| Trạng thái | TrangThai | | Có | [Đang làm \| Nghỉ việc] |

#### E23. Tài khoản (`TaiKhoan`) – thực thể chức năng

Quyền đăng nhập vào hệ thống. Được tạo ở 5.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Tên đăng nhập | TenDangNhap | # | Có | Duy nhất |
| Mật khẩu (đã băm) | MatKhauBam | | Có | Chỉ lưu chuỗi băm, không lưu mật khẩu gốc |
| Vai trò | VaiTro | | Có | [Học viên \| Phụ huynh \| Giáo viên \| NV tuyển sinh \| NV kế toán \| QL đào tạo \| Giám đốc \| Quản trị viên]. Quyền của từng vai trò xem ở 3.5 |
| Mã nhân viên | MaNV | QH → NhanVien | Không | |
| Mã giáo viên | MaGV | QH → GiaoVien | Không | |
| Mã học viên | MaHV | QH → HocVien | Không | |
| Mã phụ huynh | MaPH | QH → PhuHuynh | Không | |
| Trạng thái | TrangThai | | Có | [Hoạt động \| Khóa] |

Ràng buộc: trong 4 thuộc tính quan hệ, **đúng một** có giá trị và phải khớp với Vai trò (Giáo viên → MaGV; Học viên → MaHV; Phụ huynh → MaPH; các vai trò còn lại → MaNV). Bốn thuộc tính được để riêng, không gộp thành một "Mã người dùng", vì nếu gộp thì không đặt được khóa ngoại.

#### E24. Thông báo (`ThongBao`) – thực thể sự kiện

Một thông báo gửi tới một người nhận. Được tạo ở 5.2.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã thông báo | MaTB | # | Có | Hệ thống cấp, tăng dần |
| Loại | Loai | | Có | [Lịch học \| Học phí \| Chuyên cần \| Kết quả] |
| Mã học viên | MaHV | QH → HocVien | Có | Học viên được nói tới trong thông báo |
| Mã phụ huynh | MaPH | QH → PhuHuynh | Không | Có giá trị khi người nhận là phụ huynh. Rỗng nghĩa là gửi cho chính học viên |
| Nội dung | NoiDung | | Có | |
| Thời gian gửi | ThoiGianGui | | Có | |
| Trạng thái gửi | TrangThaiGui | | Có | [Chờ gửi \| Đã gửi \| Lỗi] |

Học viên dưới 18 tuổi sẽ nhận 2 dòng cho cùng một sự việc, một cho học viên và một cho phụ huynh (04_dac_ta mục 1.8, chức năng 5.2).

#### E25. Nhật ký (`NhatKy`) – thực thể sự kiện

Vết thao tác của người dùng. Mọi xử lý đều ghi vào đây, 5.1 dùng để xem.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã nhật ký | MaNK | # | Có | Hệ thống cấp, tăng dần. Bộ (Thời điểm, Tên đăng nhập) có thể trùng nên không dùng làm khóa |
| Thời điểm | ThoiDiem | | Có | |
| Tên đăng nhập | TenDangNhap | QH → TaiKhoan | Có | |
| Thao tác | ThaoTac | | Có | [Đăng nhập \| Thêm \| Sửa \| Ngừng \| Duyệt \| In] |
| Đối tượng | DoiTuong | | Có | Tên thực thể bị tác động, vd "DangKy" |
| Mã đối tượng | MaDoiTuong | | Có | Giá trị khóa của cá thể, vd "HV0412/IEK-2609" |

#### E26. Tham số (`ThamSo`) – thực thể chức năng

Các con số trong quy tắc nghiệp vụ mà Giám đốc có thể thay đổi. Quản trị viên cập nhật qua 5.1. Đây là nơi lưu phần *Tham số cấu hình* của luồng vào 5.1.

| Thuộc tính | Tên trường | Loại | Bắt buộc | Miền giá trị / ghi chú |
|---|---|---|---|---|
| Mã tham số | MaThamSo | # | Có | |
| Tên tham số | TenThamSo | | Có | |
| Giá trị | GiaTri | | Có | Số |
| Đơn vị | DonVi | | Có | |
| Quy tắc | QuyTac | | Không | Mã QTxx của quy tắc dùng tham số này |

Giá trị ban đầu:

| MaThamSo | Giá trị | Đơn vị | Quy tắc |
|---|---|---|---|
| SISO_TOI_THIEU | 8 | học viên | QT06 |
| SISO_TOI_DA | 20 | học viên | QT06 |
| TY_LE_GIAM_TOI_DA | 15 | % | QT04 |
| TY_LE_DOT_1 | 50 | % | QT05 |
| SO_BUOI_CHUYEN_LOP | 3 | buổi | QT08 |
| TY_LE_BUOI_BAO_LUU | 50 | % | QT09 |
| THANG_BAO_LUU | 6 | tháng | QT09 |
| NGUONG_CANH_BAO_VANG | 20 | % | QT12 |
| DIEM_GIOI | 8.5 | điểm | QT14 |
| DIEM_KHA | 7.0 | điểm | QT14 |
| DIEM_DAT | 5.0 | điểm | QT14, QT15 |
| NGUONG_CHUYEN_CAN_DAT | 70 | % | QT15 |
| SO_NGAY_NHAC_HOC_PHI | 3 | ngày | 5.2 |
| GIO_SUA_DIEM_DANH | 24 | giờ | 4.1 |

Quy ước trên DFD: các xử lý đọc tham số **khi chương trình khởi động** nên không vẽ luồng đọc D5 cho từng xử lý. Cách làm này giống với mã nhân viên lấy từ phiên đăng nhập (03_chuc_nang mục 4). Ở 3.4, ràng buộc CHECK chỉ đặt cho những bất biến **không đổi** (điểm trong khoảng 0–10, sĩ số > 0). Các con số chính sách ở trên không đưa vào CHECK, để đổi tham số mà không phải sửa CSDL.

---

## 5. Thuộc tính thứ sinh và nguyên tắc "chốt"

Bài giảng yêu cầu loại thuộc tính thứ sinh trước khi chuẩn hóa. Hệ thống tuân theo yêu cầu đó, chỉ có **một ngoại lệ có điều kiện**. Một thuộc tính thứ sinh được **lưu** (ký hiệu *S – chốt*) khi thỏa **cả hai** điều kiện sau:
1. Giá trị đã được công bố cho học viên (in lên chứng từ) hoặc đã được duyệt.
2. Dữ liệu dùng để tính có thể thay đổi về sau: học phí khóa, tỷ lệ ưu đãi, trọng số, tham số, lịch học.

Khi đó giá trị đã lưu là một **sự kiện lịch sử**, không còn phụ thuộc hàm vào dữ liệu hiện tại. Nếu tính lại thì sẽ ra số khác với số đã in trên phiếu hoặc chứng nhận. Ví dụ: kỳ sau trung tâm tăng học phí khóa IEK, nhưng phiếu thu cũ của HV0412 vẫn phải ghi đúng 4.800.000 đ.

| Thực thể | Thuộc tính thứ sinh | Cách tính | Lưu? | Lý do |
|---|---|---|---|---|
| HocVien | Tuổi | Từ Ngày sinh | Không | |
| HocVien | Học viên cũ | Có đăng ký ở lớp đã Kết thúc, không Nghỉ học (QT01) | Không | |
| LopHoc | Số đăng ký, Tỷ lệ lấp đầy | Đếm DangKy giữ chỗ; / Sĩ số tối đa | Không | |
| LopHoc | Ngày kết thúc | Ngày của buổi học cuối | Không | |
| LichTuan | Giờ kết thúc | Giờ bắt đầu + Thời lượng buổi | Không | |
| PhieuDangKy | Tổng học phí, Giảm, Phải nộp | Cộng từ HocPhi | Không | |
| DangKy | Hiệu lực | Đã đăng ký và Đã thu ≥ 50% Phải nộp (QT05) | Không | |
| DangKy | Đã thu, Còn nợ, Quá hạn | Từ ChiTietPhieuThu, DotHocPhi | Không | Đây chính là "Công nợ" của BT1 |
| DangKy | Hạn bảo lưu | Ngày thay đổi + THANG_BAO_LUU | **Chốt** | Đã báo cho học viên; tham số có thể đổi |
| HocPhi | Học phí gốc | = KhoaHoc.HocPhi tại ngày lập | **Chốt** | In trên phiếu; học phí khóa có thể đổi |
| HocPhi | Tỷ lệ giảm | Bảng quyết định 3.1 | **Chốt** | In trên phiếu; ưu đãi có thể hết hạn hoặc đổi tỷ lệ |
| HocPhi | Phải nộp | Học phí gốc × (1 − Tỷ lệ giảm) | Không | Tính lại được từ hai giá trị đã chốt |
| DotHocPhi | Số tiền, Hạn đóng | Theo QT05 | **Chốt** | Hạn đợt 2 in trên phiếu thu; lịch học có thể dời |
| PhieuThu | Tổng tiền, Viết bằng chữ, Còn nợ | Cộng các dòng | Không | |
| DiemDanh | Tỷ lệ chuyên cần | (x + M) / số buổi đã học (QT11) | Không | |
| KetQua | Điểm chuyên cần | Tỷ lệ chuyên cần × 10 | Không | |
| KetQua | Điểm tổng kết, Xếp loại, Kết quả | QT13–QT15 | **Chốt** | Đã duyệt, in trên chứng nhận; trọng số và ngưỡng có thể đổi |
| ChungNhan | Từ ngày, đến ngày | Buổi đầu, buổi cuối | Không | |
| (báo cáo) | Doanh thu, số học viên mới, tỷ lệ đạt… | 3.4, 5.3, 5.4 | Không | Báo cáo MIS |

---

## 6. Kiểm tra

### 6.1 Điều kiện của bảng thực thể (bài giảng mục 5.2.1.2)

| Điều kiện | Kết quả |
|---|---|
| Mỗi bảng có tên duy nhất trong toàn hệ thống | Đạt: 26 tên khác nhau (mục 8). |
| Giá trị các cột là nguyên tố | Đạt. "Lịch học T3-T5 18:00-19:30" đã tách thành LichTuan (Thứ, Giờ bắt đầu). "Đợt" và "Nộp lần này" đã tách theo từng dòng. Địa chỉ và Họ tên để một trường vì hệ thống không xử lý trên từng thành phần. |
| Mỗi dòng là duy nhất; khóa không rỗng và duy nhất | Đạt: cả 26 bảng đều có `#`, thuộc tính `#` đều bắt buộc. |
| Tên cột duy nhất trong một bảng | Đạt. Trong bảng Diem, thuộc tính *Điểm* có tên trường trùng tên bảng (Diem). Như vậy vẫn hợp lệ, nhưng ở 3.4 nếu công cụ báo xung đột thì đổi thành `GiaTri`. |
| Không còn thuộc tính lặp | Đạt: mọi R ở mục 3 đã được tách. |
| Không lưu thuộc tính thứ sinh | Đạt, trừ 8 thuộc tính *chốt* có lý do ở mục 5. |

### 6.2 Đối chiếu chứng từ mẫu → cá thể

Mỗi trường trên 5 chứng từ ở 02_thu_thap mục 4.2 phải có chỗ lưu, hoặc là thứ sinh, hoặc được bỏ có lý do. Các mã HV0412, IEK-2609… lấy đúng từ chứng từ. Các mã chứng từ không ghi (mã phụ huynh, giáo viên, nhân viên) là mã giả định.

**Phiếu đăng ký DK2026-0158**

| Trường trên chứng từ | Lưu ở |
|---|---|
| Số phiếu, Ngày | PhieuDangKy (DK2026-0158, 05/09/2026) |
| Mã HV, Họ tên, Ngày sinh, Giới tính, Điện thoại, Email, Địa chỉ | HocVien (HV0412, Nguyễn Minh Anh, 14/03/2012, Nữ, 0912345678, minhanh@gmail.com, 12 Trần Phú, Hà Đông) |
| Phụ huynh: Họ tên, Điện thoại | PhuHuynh (PH0301, Nguyễn Văn Bình, 0983111222) |
| Quan hệ: Bố | HocVienPhuHuynh (HV0412, PH0301, Bố) |
| Trình độ A2, ngày 02/09/2026 | HocVien.TrinhDoDauVao, NgayKiemTra |
| Dòng 1: IEK-2609 | DangKy (HV0412, IEK-2609, DK2026-0158, Đã đăng ký) |
| Dòng 2: GT-2604 | DangKy (HV0412, GT-2604, DK2026-0158, Đã đăng ký) |
| Khóa học | KhoaHoc.TenKH, tìm qua LopHoc.MaKH |
| Lịch học "T3-T5 18:00-19:30" | LichTuan (IEK-2609, 3, 18:00, P203), (IEK-2609, 5, 18:00, P203). Giờ kết thúc 19:30 = 18:00 + 90 phút (S) |
| Khai giảng | LopHoc.NgayKhaiGiang |
| Học phí 4.800.000 / 2.400.000 | HocPhi.HocPhiGoc (chốt từ KhoaHoc.HocPhi) |
| Ưu đãi UD-HVCU (giảm 10%) | ApDungUuDai (HV0412, IEK-2609, UD-HVCU), (HV0412, GT-2604, UD-HVCU); HocPhi.TyLeGiam = 10 |
| Tổng 7.200.000, Giảm 720.000, Phải nộp 6.480.000 | S. Kiểm tra: 4.800.000 × 0,9 + 2.400.000 × 0,9 = 4.320.000 + 2.160.000 = **6.480.000** ✓ |
| Nhân viên tiếp nhận: Trần Thị Hoa | PhieuDangKy.MaNV = NV007 |
| STT, chữ ký | Bỏ: STT ít ý nghĩa; chữ ký nằm trên bản giấy |

**Phiếu thu PT2026-0731.** Đây là chứng từ cho thấy *Đợt* phải nằm ở từng dòng. Đầu phiếu ghi "Đợt: 1". Nhưng dòng IEK-2609 nộp đủ 4.320.000, tức nộp **cả đợt 1 lẫn đợt 2** (mỗi đợt 2.160.000), còn dòng GT-2604 chỉ nộp đợt 1 (1.080.000). Nếu chỉ ghi một đợt cho cả phiếu thì không lưu đúng được phiếu này.

| Trường trên chứng từ | Lưu ở |
|---|---|
| Số, Ngày, Người nộp, Hình thức, Người thu | PhieuThu (PT2026-0731, 06/09/2026, Nguyễn Văn Bình, Chuyển khoản, NV004 – Lê Thu Trang) |
| Học viên HV0412, theo phiếu ĐK DK2026-0158 | ChiTietPhieuThu.MaHV; số phiếu ĐK lấy qua DangKy.SoPhieuDK (S) |
| Dòng IEK-2609, nộp 4.320.000 | ChiTietPhieuThu (PT2026-0731, HV0412, IEK-2609, 1, 2.160.000) và (PT2026-0731, HV0412, IEK-2609, 2, 2.160.000) |
| Dòng GT-2604, nộp 1.080.000 | ChiTietPhieuThu (PT2026-0731, HV0412, GT-2604, 1, 1.080.000) |
| Học phí, Giảm, Phải nộp của từng dòng | HocPhi (gốc, tỷ lệ) và S |
| Tổng nộp 5.400.000, bằng chữ | S. Kiểm tra: 2.160.000 + 2.160.000 + 1.080.000 = **5.400.000** ✓ |
| Còn nợ 1.080.000 | S. Kiểm tra: 6.480.000 − 5.400.000 = **1.080.000** ✓, bằng đúng đợt 2 của GT-2604 |
| Hạn đợt 2: 17/10/2026 | DotHocPhi (HV0412, GT-2604, 2, 1.080.000, 17/10/2026). 17/10/2026 là thứ Bảy thứ 5 kể từ ngày khai giảng 19/09, tức buổi giữa khóa ✓ |

**Sổ điểm danh lớp IEK-2609**

| Trường trên chứng từ | Lưu ở |
|---|---|
| Lớp, khóa, GV Phạm Quốc Huy | LopHoc (IEK-2609, IEK, GV015) |
| Phòng P203 | BuoiHoc.MaPhong |
| B1 15/09, B2 17/09, B3 22/09, B4 24/09 | BuoiHoc (IEK-2609, 1…4, các ngày tương ứng). Các ngày này đều rơi vào thứ Ba và thứ Năm, khớp với LichTuan ✓ |
| Ô x / M / P / K | DiemDanh.TrangThai, ví dụ (HV0412, IEK-2609, 2, M), (HV0398, IEK-2609, 3, K) |
| Ghi chú "Nhắc phụ huynh" | DiemDanh.GhiChu |
| Ký hiệu | Miền giá trị của DiemDanh.TrangThai |

**Bảng điểm lớp IEK-2609**

| Trường trên chứng từ | Lưu ở |
|---|---|
| Khóa, trình độ A2 → B1 | KhoaHoc (TrinhDoDauVao, TrinhDoDauRa) |
| Trọng số 10% – 30% – 60% | KhoaHoc (TSChuyenCan, TSGiuaKy, TSCuoiKy) |
| Chuyên cần 9.0 / 6.0 | S, tính từ DiemDanh |
| Giữa kỳ, Cuối kỳ | Diem (HV0412, IEK-2609, Giữa kỳ, 7.5), (HV0412, IEK-2609, Cuối kỳ, 8.0); tương tự HV0398 với 5.0 và 4.0 |
| Tổng kết, Xếp loại, Kết quả | KetQua (HV0412, IEK-2609, 8.0, Khá, Đạt); (HV0398, IEK-2609, 4.5, Không đạt, Không đạt). Ô xếp loại "–" trên chứng từ được ghi là *Không đạt* (QT14) |
| Nhận xét của GV | Diem.NhanXet |

**Chứng nhận CN2026-0089**

| Trường trên chứng từ | Lưu ở |
|---|---|
| Số, Ngày cấp | ChungNhan (CN2026-0089, HV0412, IEK-2609, 20/12/2026) |
| Họ tên, Ngày sinh | HocVien |
| Khóa học, lớp | KhoaHoc, LopHoc |
| Từ 15/09/2026 đến 15/12/2026 | S, tính từ BuoiHoc đầu và cuối |
| Kết quả Khá (8.0) | KetQua |
| Giám đốc trung tâm (chữ ký) | Bỏ: nằm trên bản giấy |

Kết luận: mọi trường trên 5 chứng từ đều có chỗ lưu, hoặc là thứ sinh, hoặc được bỏ có lý do. Các phép tính trên chứng từ đều khớp với dữ liệu gốc.

### 6.3 Nhất quán DFD – thực thể

Mỗi kho trên DFD ứng với một tập thực thể xác định. Mỗi thực thể chỉ thuộc **một** kho:

| Kho | Thực thể | Xử lý ghi (theo DFD) |
|---|---|---|
| D1 Hồ sơ | HocVien, PhuHuynh, HocVienPhuHuynh, GiaoVien, KhoaHoc, PhongHoc | 1.1–1.4 |
| D2 Lớp và lịch học | LopHoc, LichTuan, BuoiHoc, PhieuDangKy, DangKy | 2.1–2.4 |
| D3 Học phí | UuDai, HocPhi, DotHocPhi, ApDungUuDai, PhieuThu, ChiTietPhieuThu | 3.1, 3.2 |
| D4 Học tập | DiemDanh, Diem, KetQua, ChungNhan | 4.1–4.4 |
| D5 Tài khoản và thông báo | NhanVien, TaiKhoan, ThongBao, NhatKy, ThamSo | 5.1, 5.2 |

Ánh xạ 19 thực thể dự kiến trong ma trận thực thể – chức năng (03_chuc_nang mục 4) sang 26 thực thể thiết kế. Các chữ C/R/U của ma trận giữ nguyên cho thực thể con:

| Ma trận (2.2) | Thực thể thiết kế |
|---|---|
| HV, GV, KH, PHG, BH, DD, DIEM, KQ, CN, NV, TK, TB, NK | Giữ nguyên tên: HocVien, GiaoVien, KhoaHoc, PhongHoc, BuoiHoc, DiemDanh, Diem, KetQua, ChungNhan, NhanVien, TaiKhoan, ThongBao, NhatKy |
| PH | PhuHuynh + HocVienPhuHuynh |
| LOP | LopHoc + LichTuan |
| DK | PhieuDangKy + DangKy |
| UD | UuDai + ApDungUuDai |
| HP | HocPhi + DotHocPhi |
| PT | PhieuThu + ChiTietPhieuThu |
| (mới) | ThamSo: 5.1 tạo và sửa (C, U); các xử lý khác đọc khi khởi động (mục 4.5, E26) |

Các luồng ghi vào kho ở 04_dac_ta mục 4.4 đều mang thuộc tính có trong bảng của thực thể tương ứng. Các luồng mang thuộc tính thứ sinh (Phải nộp, Đã thu, Còn nợ, Tỷ lệ chuyên cần, Số đăng ký) đều tính được từ thuộc tính gốc, theo mục 5.

### 6.4 Thuộc tính quan hệ đều trỏ tới định danh có thật

Mỗi `QH → X` phải trỏ đúng tới các thuộc tính `#` của X, đủ số thành phần. Danh sách đầy đủ ở mục 9. Đã kiểm tra cả 34 dòng.

---

## 7. Giả định khi thiết kế

| # | Giả định | Nếu thay đổi thì |
|---|---|---|
| G1 | Mỗi giáo viên dạy **một** ngôn ngữ chính | Tách thực thể GiaoVienNgonNgu (MaGV, NgonNgu) |
| G2 | Mỗi lớp học tối đa một ca trong một ngày của tuần | Thêm Giờ bắt đầu vào khóa của LichTuan |
| G3 | Học viên không chuyển ngược về lớp mình đã rời. Khóa (MaHV, MaLop) không cho phép tạo đăng ký thứ hai ở cùng lớp | 2.4 khôi phục trạng thái của đăng ký cũ thay vì tạo mới |
| G4 | Chỉ lưu trình độ của lần kiểm tra đầu vào gần nhất | Tách thực thể KiemTraDauVao |
| G5 | Các con số chính sách nằm ở ThamSo. Riêng tỷ lệ giảm nằm ở UuDai | – |

---

## 8. Tổng hợp – Danh sách thực thể và thuộc tính (mẫu Bảng 4.5 của bài giảng)

| # | Thực thể | Tên tệp | Loại | Kho | Thuộc tính |
|---|---|---|---|---|---|
| 1 | Học viên | HocVien | Chức năng | D1 | #MaHV, HoTen, NgaySinh, GioiTinh, DienThoai, Email, DiaChi, TrinhDoDauVao, NgayKiemTra, NgayTiepNhan, TrangThai |
| 2 | Phụ huynh | PhuHuynh | Chức năng | D1 | #MaPH, HoTen, DienThoai, Email |
| 3 | Học viên – Phụ huynh | HocVienPhuHuynh | Quan hệ | D1 | #MaHV, #MaPH, QuanHe |
| 4 | Giáo viên | GiaoVien | Chức năng | D1 | #MaGV, HoTen, NgaySinh, DienThoai, Email, NgonNguDay, ChuyenMon, BangCap, TinhTrang |
| 5 | Khóa học | KhoaHoc | Chức năng | D1 | #MaKH, TenKH, NgonNgu, TrinhDoDauVao, TrinhDoDauRa, SoBuoi, ThoiLuongBuoi, HocPhi, TSChuyenCan, TSGiuaKy, TSCuoiKy, MoTa, TrangThai |
| 6 | Phòng học | PhongHoc | Xác thực | D1 | #MaPhong, TenPhong, SucChua, ThietBi, TinhTrang |
| 7 | Lớp học | LopHoc | Chức năng | D2 | #MaLop, MaKH, MaGV, NgayKhaiGiang, SiSoToiDa, TrangThai |
| 8 | Lịch tuần | LichTuan | Quan hệ | D2 | #MaLop, #Thu, GioBatDau, MaPhong |
| 9 | Buổi học | BuoiHoc | Sự kiện | D2 | #MaLop, #SoBuoi, Ngay, GioBatDau, GioKetThuc, MaPhong, MaGV, Loai, NoiDung, TrangThai |
| 10 | Phiếu đăng ký | PhieuDangKy | Sự kiện | D2 | #SoPhieuDK, NgayDK, MaNV |
| 11 | Đăng ký học | DangKy | Quan hệ | D2 | #MaHV, #MaLop, SoPhieuDK, TrangThai, NgayThayDoi, LyDo, HanBaoLuu, MaLopGoc |
| 12 | Ưu đãi | UuDai | Chức năng | D3 | #MaUD, TenUD, LoaiUD, TyLeGiam, NgayBatDau, NgayKetThuc, DieuKien |
| 13 | Học phí | HocPhi | Sự kiện | D3 | #MaHV, #MaLop, HocPhiGoc, TyLeGiam, SoDot, NgayLap |
| 14 | Đợt học phí | DotHocPhi | Sự kiện | D3 | #MaHV, #MaLop, #Dot, SoTien, HanDong |
| 15 | Áp dụng ưu đãi | ApDungUuDai | Quan hệ | D3 | #MaHV, #MaLop, #MaUD |
| 16 | Phiếu thu | PhieuThu | Sự kiện | D3 | #SoPT, NgayThu, NguoiNop, HinhThuc, MaNV |
| 17 | Chi tiết phiếu thu | ChiTietPhieuThu | Quan hệ | D3 | #SoPT, #MaHV, #MaLop, #Dot, SoTien |
| 18 | Điểm danh | DiemDanh | Quan hệ | D4 | #MaHV, #MaLop, #SoBuoi, TrangThai, GhiChu |
| 19 | Điểm thành phần | Diem | Sự kiện | D4 | #MaHV, #MaLop, #LoaiDiem, Diem, NhanXet |
| 20 | Kết quả học tập | KetQua | Sự kiện | D4 | #MaHV, #MaLop, DiemTongKet, XepLoai, KetQua, MaNVDuyet, NgayDuyet |
| 21 | Chứng nhận | ChungNhan | Sự kiện | D4 | #SoCN, MaHV, MaLop, NgayCap |
| 22 | Nhân viên | NhanVien | Chức năng | D5 | #MaNV, HoTen, BoPhan, ChucVu, DienThoai, Email, TrangThai |
| 23 | Tài khoản | TaiKhoan | Chức năng | D5 | #TenDangNhap, MatKhauBam, VaiTro, MaNV, MaGV, MaHV, MaPH, TrangThai |
| 24 | Thông báo | ThongBao | Sự kiện | D5 | #MaTB, Loai, MaHV, MaPH, NoiDung, ThoiGianGui, TrangThaiGui |
| 25 | Nhật ký | NhatKy | Sự kiện | D5 | #MaNK, ThoiDiem, TenDangNhap, ThaoTac, DoiTuong, MaDoiTuong |
| 26 | Tham số | ThamSo | Chức năng | D5 | #MaThamSo, TenThamSo, GiaTri, DonVi, QuyTac |

Thống kê theo loại: **1 xác thực** (PhongHoc), **9 chức năng**, **10 sự kiện**, **6 quan hệ**, tổng cộng 26 thực thể. Có 11 thực thể dùng khóa kép.

---

## 9. Danh sách thuộc tính quan hệ (đầu vào cho 3.2)

Mỗi dòng là một mối liên kết giữa hai thực thể. Ở 3.2 sẽ xác định bậc, kiểu (1–1, 1–N, N–N) và đặt tên quan hệ bằng động từ.

| # | Thực thể chứa | Thuộc tính quan hệ | Trỏ tới | Ý nghĩa |
|---|---|---|---|---|
| 1 | HocVienPhuHuynh | MaHV | HocVien | Học viên được giám hộ |
| 2 | HocVienPhuHuynh | MaPH | PhuHuynh | Người giám hộ |
| 3 | LopHoc | MaKH | KhoaHoc | Lớp mở theo khóa |
| 4 | LopHoc | MaGV | GiaoVien | Giáo viên phụ trách |
| 5 | LichTuan | MaLop | LopHoc | Lịch của lớp |
| 6 | LichTuan | MaPhong | PhongHoc | Phòng mặc định |
| 7 | BuoiHoc | MaLop | LopHoc | Buổi của lớp |
| 8 | BuoiHoc | MaPhong | PhongHoc | Phòng thực tế |
| 9 | BuoiHoc | MaGV | GiaoVien | Giáo viên dạy thực tế |
| 10 | PhieuDangKy | MaNV | NhanVien | Người tiếp nhận |
| 11 | DangKy | MaHV | HocVien | Người học |
| 12 | DangKy | MaLop | LopHoc | Lớp học |
| 13 | DangKy | SoPhieuDK | PhieuDangKy | Chứng từ ghi danh |
| 14 | DangKy | (MaHV, MaLopGoc) | DangKy | Đăng ký gốc (quan hệ bậc 1) |
| 15 | HocPhi | (MaHV, MaLop) | DangKy | Khoản phải thu của đăng ký |
| 16 | DotHocPhi | (MaHV, MaLop) | HocPhi | Đợt của khoản phải thu |
| 17 | ApDungUuDai | (MaHV, MaLop) | HocPhi | Khoản được giảm |
| 18 | ApDungUuDai | MaUD | UuDai | Ưu đãi được hưởng |
| 19 | PhieuThu | MaNV | NhanVien | Người thu |
| 20 | ChiTietPhieuThu | SoPT | PhieuThu | Thuộc phiếu thu |
| 21 | ChiTietPhieuThu | (MaHV, MaLop, Dot) | DotHocPhi | Đợt được nộp |
| 22 | DiemDanh | (MaHV, MaLop) | DangKy | Học viên của lớp |
| 23 | DiemDanh | (MaLop, SoBuoi) | BuoiHoc | Buổi được điểm danh |
| 24 | Diem | (MaHV, MaLop) | DangKy | Điểm của đăng ký |
| 25 | KetQua | (MaHV, MaLop) | DangKy | Kết quả của đăng ký |
| 26 | KetQua | MaNVDuyet | NhanVien | Người duyệt |
| 27 | ChungNhan | (MaHV, MaLop) | KetQua | Chứng nhận cho kết quả đạt |
| 28 | TaiKhoan | MaNV | NhanVien | Chủ tài khoản là nhân viên |
| 29 | TaiKhoan | MaGV | GiaoVien | Chủ tài khoản là giáo viên |
| 30 | TaiKhoan | MaHV | HocVien | Chủ tài khoản là học viên |
| 31 | TaiKhoan | MaPH | PhuHuynh | Chủ tài khoản là phụ huynh |
| 32 | ThongBao | MaHV | HocVien | Học viên được thông báo |
| 33 | ThongBao | MaPH | PhuHuynh | Phụ huynh nhận thông báo |
| 34 | NhatKy | TenDangNhap | TaiKhoan | Người thao tác |

---

## 10. Điều chỉnh so với giai đoạn phân tích

Các tài liệu phân tích (2.x) đã nộp nên giữ nguyên. Những điểm thiết kế làm chi tiết hơn được ghi lại dưới đây để truy vết:

| Ở tài liệu phân tích | Ở thiết kế | Lý do |
|---|---|---|
| Đăng ký: #Số phiếu ĐK + {Mã lớp + …} (04_dac_ta mục 4.1) | PhieuDangKy (#SoPhieuDK) + DangKy (#MaHV, #MaLop) | Một phiếu có nhiều lớp; mỗi dòng có vòng đời riêng |
| Lớp: Lịch tuần là một thuộc tính | LichTuan | Thuộc tính lặp và không nguyên tố |
| Học phí phải thu: {Mã UĐ}, {Đợt…} | ApDungUuDai, DotHocPhi | Nhóm lặp |
| Phiếu thu: {Số phiếu ĐK + Mã lớp + Đợt + Số tiền} | ChiTietPhieuThu (#SoPT, #MaHV, #MaLop, #Dot) | Dòng phiếu thu trỏ tới đợt học phí qua khóa của đăng ký |
| Điểm danh, Điểm thành phần, Kết quả: Mã lớp + Mã HV | Giữ, đặt thành khóa kép và trỏ tới DangKy | – |
| Tài khoản: {Quyền} | Quyền xác định qua Vai trò (3.5) | Vai trò cố định |
| Thông báo: Người nhận | MaHV + MaPH | Để đặt được khóa ngoại |
| Nhật ký: không có khóa | #MaNK | (Thời điểm, Tên đăng nhập) có thể trùng |
| (không có) | ThamSo | Lưu phần "Tham số cấu hình" của luồng vào 5.1 |
| KE_HOACH dự kiến DauDiem | Bỏ, Loại điểm là miền giá trị | Mục 2.1 |

---

## 11. Đầu ra cho các bước sau

| Bước | Dùng gì từ tài liệu này |
|---|---|
| 3.2 Quan hệ + ERD | Mục 8 (thực thể), mục 9 (34 thuộc tính quan hệ → bậc, kiểu, tên quan hệ) |
| 3.3 Chuẩn hóa | Mục 3 (thuộc tính ban đầu có R, S) cho 3 chứng từ. Kết quả 3NF phải trùng mục 8. Mục 5 là căn cứ giữ thuộc tính *chốt* |
| 3.4 CSDL vật lý | Tên tệp, tên trường, cột *Bắt buộc* (NOT NULL), *Miền giá trị* (CHECK), `#` (PRIMARY KEY), QH (FOREIGN KEY); mục 6.2 làm dữ liệu mẫu cho seed.sql; bảng ThamSo |
| 3.5 Module, phân quyền | TaiKhoan.VaiTro (8 vai trò) |
| 3.6 Giao diện | Mỗi thực thể chức năng có một form danh mục; mỗi thực thể sự kiện có một form nhập chứng từ |
