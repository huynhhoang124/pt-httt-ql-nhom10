# 3.4 Thiết kế cơ sở dữ liệu vật lý – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn Thiết kế hệ thống (KE_HOACH.md, mục 3.4). Bài giảng mục 5.3.2 viết: thiết kế CSDL quan hệ là *xác định các tệp dữ liệu, mỗi tệp có những trường nào, trường nào là khóa chính, trường mô tả, trường quan hệ*, và việc này *xuất phát từ ERD với các thực thể đã được chuẩn hóa*. Bước này dùng đúng kết quả của các bước trước:
- 26 thực thể ở 3.1,
- 29 quan hệ ở 3.2,
- đã xác nhận đạt 3NF ở 3.3.

**Sản phẩm (thư mục `csdl/`):**

| Tệp | Nội dung |
|---|---|
| `schema.sql` | Tạo CSDL `TrungTamNgoaiNgu`, 26 bảng, khóa chính, khóa ngoại, CHECK, UNIQUE, giá trị mặc định, chỉ mục |
| `views.sql` | 9 view và 2 hàm tính các thuộc tính thứ sinh và các báo cáo MIS |
| `seed.sql` | Dữ liệu mẫu tại ngày chốt 31/12/2026, **sinh tự động** bằng `sinh_du_lieu.py` |
| `kiem_tra.sql` | 24 kiểm tra Đạt/LỖI, sau đó là 8 báo cáo MIS chạy trên dữ liệu mẫu |
| `mo_ta_bang.md` | Bảng mô tả từng tệp (Tên trường – Kiểu – Ràng buộc – Ý nghĩa), **sinh tự động** bằng `sinh_mo_ta.py` |
| `chay.ps1` | Chạy các tệp `.sql` lên SQL Server, không cần cài `sqlcmd` |

---

## 1. Hệ quản trị và quy ước

| Nội dung | Lựa chọn | Lý do |
|---|---|---|
| Hệ quản trị | Microsoft SQL Server (đã chạy trên bản 2022 Express) | Đúng công nghệ đã chọn ở BT1 (góc nhìn Technology) và GĐ1. Bản Express miễn phí, đủ cho 1 cơ sở, khoảng 500 học viên |
| Bảng mã đối chiếu | `Vietnamese_100_CI_AS` cho cả CSDL | Sắp xếp và so sánh tiếng Việt đúng (Ă, Â, Đ…), không phân biệt hoa thường |
| Tên bảng, tên trường | Tên tệp và tên trường của 3.1, viết không dấu dạng PascalCase | Bài giảng: tên tệp, tên trường viết không dấu. Giữ nguyên tên của 3.1 để đối chiếu tự động được |
| Tên ràng buộc | `PK_Bang`, `FK_Bang_BangDich`, `UQ_…`, `CK_Bang_Truong`, `DF_…`, chỉ mục `IX_…`, `UX_…` | Khi vi phạm, thông báo lỗi nêu đúng tên ràng buộc, nên tầng ứng dụng đổi được thành câu báo lỗi dễ hiểu (yêu cầu "trợ giúp" ở 3.6) |
| Xóa dữ liệu | Không xóa cứng dữ liệu nghiệp vụ; chuyển Trạng thái sang "Ngừng"/"Nghỉ việc"… Mọi khóa ngoại dùng NO ACTION | Theo 04_dac_ta mục 1.1. Dữ liệu cũ còn được phiếu thu, kết quả tham chiếu tới |

### 1.1 Quy tắc chọn kiểu dữ liệu

| Loại dữ liệu | Kiểu | Ví dụ |
|---|---|---|
| Mã, số chứng từ (chỉ chữ cái Latin, chữ số, gạch ngang) | `VARCHAR(n)` | MaHV `VARCHAR(10)`, SoPhieuDK `VARCHAR(15)` |
| Tên, địa chỉ, ghi chú, miền giá trị tiếng Việt | `NVARCHAR(n)` | HoTen `NVARCHAR(100)`, TrangThai `NVARCHAR(20)` |
| Tiền (đồng, không có phần lẻ) | `DECIMAL(12,0)` | HocPhi, SoTien. Không dùng `FLOAT` vì tiền phải cộng trừ chính xác |
| Tỷ lệ phần trăm | `DECIMAL(5,2)`; trọng số điểm dùng `TINYINT` | TyLeGiam 10.00; TSCuoiKy 60 |
| Điểm thang 10 | `DECIMAL(3,1)` | 7.5 |
| Số đếm nhỏ | `TINYINT` / `SMALLINT` | Dot, Thu / SoBuoi, SucChua |
| Ngày / giờ / thời điểm | `DATE` / `TIME(0)` / `DATETIME2(0)` | NgayThu / GioBatDau / ThoiGianGui |
| Khóa do hệ thống tự cấp | `INT` / `BIGINT IDENTITY` | MaTB, MaNK (3.1 ghi "hệ thống cấp, tăng dần") |
| Mật khẩu | `VARCHAR(255)` chứa **chuỗi băm** | Không lưu mật khẩu gốc (3.1 E23) |

---

## 2. Chuyển ERD thành tệp dữ liệu

Làm đúng các quy tắc biểu diễn quan hệ của bài giảng (mục 5.2.2.3), đã kiểm tra ở 3.2:

| Kiểu quan hệ (3.2) | Cài đặt trong `schema.sql` | Ví dụ |
|---|---|---|
| 1–N (16 quan hệ) | Khóa của đầu 1 thành `FOREIGN KEY` ở bảng đầu N | `FK_LopHoc_KhoaHoc` (Q02) |
| N–N (5 quan hệ) | Bảng thực thể quan hệ có `PRIMARY KEY` gồm khóa của 2 bảng gốc (kèm thuộc tính riêng nếu cần), cùng 2 khóa ngoại | `DiemDanh` PK (MaHV, MaLop, SoBuoi), FK tới `DangKy` và `BuoiHoc` (Q18) |
| 1–1, khóa trùng nhau (Q13, Q20) | Khóa chính của bảng phụ đồng thời là khóa ngoại tới bảng chính | `HocPhi` PK = FK (MaHV, MaLop) → `DangKy` |
| 1–1, khóa khác nhau (Q22–Q26) | Khóa ngoại + `UNIQUE`. Với khóa ngoại cho phép rỗng thì dùng chỉ mục UNIQUE có lọc `WHERE … IS NOT NULL` | `UQ_ChungNhan_KetQua`; `UX_TaiKhoan_MaHV` |
| Bậc 1, kiểu 1–1 (Q12) | Khóa ngoại tự trỏ | `FK_DangKy_DangKyGoc` (MaHV, MaLopGoc) → `DangKy` (MaHV, MaLop) |

Có hai chỗ CSDL vật lý **chặt hơn** ERD:
- **`FK_ThongBao_GiamHo`** (MaHV, MaPH) → `HocVienPhuHuynh`. Khóa ngoại này bảo đảm thông báo chỉ gửi cho phụ huynh **của đúng học viên đó**, theo yêu cầu bảo vệ dữ liệu trẻ vị thành niên (2.1 mục 9.2). Đây là khóa ngoại thứ 35, ngoài 34 thuộc tính quan hệ của 3.1. `sinh_mo_ta.py` liệt kê nó trong danh sách bổ sung, kèm lý do.
- **`UQ_DangKy_PhieuLop`** (SoPhieuDK, MaLop). Ràng buộc này giữ cho cặp (Số phiếu ĐK, Mã lớp) cũng duy nhất, vì đó là khóa tương đương đã chứng minh ở 3.3 (mục 7.3, F11).

---

## 3. Ràng buộc toàn vẹn: đặt ở đâu

Theo 3.1 (E26), CSDL chỉ giữ các **bất biến** (không đổi theo chính sách). Các con số chính sách đọc từ bảng `ThamSo` và được kiểm tra ở tầng ứng dụng. Như vậy, khi Giám đốc đổi một con số chính sách thì không phải sửa cấu trúc CSDL.

| Loại ràng buộc | Cách cài đặt | Số lượng |
|---|---|---|
| Ràng buộc thực thể (khóa chính) | `PRIMARY KEY`, gồm 11 khóa kép | 26 |
| Ràng buộc tham chiếu | `FOREIGN KEY` (34 theo 3.1 + 1 bổ sung) | 35 |
| Ràng buộc miền giá trị | `CHECK … IN (…)` cho mọi miền `[a \| b]` ở 3.1; `CHECK > 0`; điểm 0–10; định dạng điện thoại, email | 63 CHECK (gồm cả CHECK nhiều trường bên dưới) |
| Ràng buộc nhiều trường trong một bảng | `CHECK`, ví dụ: tổng 3 trọng số = 100; giờ kết thúc > giờ bắt đầu; Bảo lưu thì phải có hạn bảo lưu; Đạt thì xếp loại khác Không đạt; vai trò khớp với đúng một chủ tài khoản | (trong số 63) |
| Duy nhất | `UNIQUE` / chỉ mục UNIQUE có lọc | 4 + 4 |
| Giá trị mặc định | `DEFAULT` (trạng thái ban đầu, ngày lập, thời điểm gửi) | 16 |

**Quy tắc nghiệp vụ QT01–QT16 được bảo đảm ở đâu:**

| Quy tắc | Trong CSDL | Ở tầng ứng dụng (GĐ4) | Kiểm tra trong `kiem_tra.sql` |
|---|---|---|---|
| QT01–QT04 ưu đãi, chặn 15% | Miền `LoaiUD`, `TyLeGiam` 0–100 | Bảng quyết định 3.1, đọc `TY_LE_GIAM_TOI_DA` | #8, #9 |
| QT05 đóng tối đa 2 đợt, đợt 1 ≥ 50% | `SoDot`, `Dot` ∈ {1, 2} | Chia đợt; kiểm tra số tiền khi lập phiếu thu | #10, #11; view `v_HocPhiDangKy.HieuLuc` |
| QT06 sĩ số 8–20, ≤ sức chứa | `SiSoToiDa > 0`, `SucChua > 0` | Đọc `SISO_TOI_THIEU`, `SISO_TOI_DA` | #13 |
| QT07 không trùng lịch | Chỉ mục `IX_BuoiHoc_GiaoVien_Ngay`, `IX_BuoiHoc_Phong_Ngay` giúp tra nhanh | Kiểm tra khoảng giờ giao nhau khi xếp lịch (04_dac_ta mục 1.3) | #14 |
| QT08 chuyển lớp, QT09 bảo lưu | `FK_DangKy_DangKyGoc`; `CK_DangKy_HanBaoLuu`; `CK_DangKy_LopGoc` | Cây quyết định 2.4 | #16, #17 |
| QT10 nghỉ học không tính nợ, QT16 quá hạn | – | – | Hàm `fn_CongNo(@Ngay)`; #12 |
| QT11 trạng thái điểm danh | `CK_DiemDanh_TrangThai`; khóa ngoại kép bảo đảm đúng buổi của đúng lớp | Chỉ sửa trong 24 giờ (`GIO_SUA_DIEM_DANH`) | #18, #23 |
| QT12 cảnh báo vắng | – | Gửi thông báo (5.2) | view `v_ChuyenCan`; #20 |
| QT13–QT15 tổng kết, xếp loại, đạt | `CK_KhoaHoc_TrongSo`; `CK_Diem_Diem`; `CK_KetQua_Dat` | Tính rồi lưu giá trị chốt khi duyệt | view `v_KetQuaTinh`; #5, #7, #19 |

---

## 4. Mô tả từng tệp dữ liệu

Bảng mô tả chi tiết của 26 tệp (Tên trường – Kiểu dữ liệu – Ràng buộc – Ý nghĩa) nằm ở **`csdl/mo_ta_bang.md`**. Tệp này do `python csdl/sinh_mo_ta.py` sinh ra từ chính `schema.sql`, nên luôn khớp với CSDL thật. Trước khi sinh, script **đối chiếu với 3.1** và dừng nếu sai một trong các điều sau:
- thiếu hoặc thừa bảng, thiếu hoặc thừa trường, sai thứ tự trường;
- khóa chính khác thuộc tính `#`;
- `NOT NULL` không khớp cột "Bắt buộc";
- khóa ngoại không khớp 34 thuộc tính quan hệ.

Kết quả chạy: *26 bảng, 161 trường, 35 khóa ngoại, 16 chỉ mục: đạt*. Script đã được thử với dữ liệu sai: đổi một cột sang NULL, hoặc bỏ một khóa ngoại, thì script đều báo lỗi.

Tổng hợp:

| # | Tệp | Thực thể | Kho | Số trường | Khóa chính | Số dòng mẫu |
|---|---|---|---|---|---|---|
| 1 | HocVien | Học viên | D1 | 11 | MaHV | 46 |
| 2 | PhuHuynh | Phụ huynh | D1 | 4 | MaPH | 25 |
| 3 | HocVienPhuHuynh | Học viên – Phụ huynh | D1 | 3 | MaHV, MaPH | 26 |
| 4 | GiaoVien | Giáo viên | D1 | 9 | MaGV | 5 |
| 5 | KhoaHoc | Khóa học | D1 | 13 | MaKH | 4 |
| 6 | PhongHoc | Phòng học | D1 | 5 | MaPhong | 5 |
| 7 | LopHoc | Lớp học | D2 | 6 | MaLop | 7 |
| 8 | LichTuan | Lịch tuần | D2 | 4 | MaLop, Thu | 11 |
| 9 | BuoiHoc | Buổi học | D2 | 10 | MaLop, SoBuoi | 111 |
| 10 | PhieuDangKy | Phiếu đăng ký | D2 | 3 | SoPhieuDK | 53 |
| 11 | DangKy | Đăng ký học | D2 | 8 | MaHV, MaLop | 58 |
| 12 | UuDai | Ưu đãi | D3 | 7 | MaUD | 3 |
| 13 | HocPhi | Học phí | D3 | 6 | MaHV, MaLop | 53 |
| 14 | DotHocPhi | Đợt học phí | D3 | 5 | MaHV, MaLop, Dot | 101 |
| 15 | ApDungUuDai | Áp dụng ưu đãi | D3 | 3 | MaHV, MaLop, MaUD | 18 |
| 16 | PhieuThu | Phiếu thu | D3 | 5 | SoPT | 92 |
| 17 | ChiTietPhieuThu | Chi tiết phiếu thu | D3 | 5 | SoPT, MaHV, MaLop, Dot | 96 |
| 18 | DiemDanh | Điểm danh | D4 | 5 | MaHV, MaLop, SoBuoi | 940 |
| 19 | Diem | Điểm thành phần | D4 | 5 | MaHV, MaLop, LoaiDiem | 80 |
| 20 | KetQua | Kết quả học tập | D4 | 7 | MaHV, MaLop | 35 |
| 21 | ChungNhan | Chứng nhận | D4 | 4 | SoCN | 31 |
| 22 | NhanVien | Nhân viên | D5 | 7 | MaNV | 7 |
| 23 | TaiKhoan | Tài khoản | D5 | 8 | TenDangNhap | 17 |
| 24 | ThongBao | Thông báo | D5 | 7 | MaTB | 27 |
| 25 | NhatKy | Nhật ký | D5 | 6 | MaNK | 7 |
| 26 | ThamSo | Tham số | D5 | 5 | MaThamSo | 14 |
| | **Tổng** | | | **161** | | **1.872** |

---

## 5. Chỉ mục

Mỗi khóa chính đã có sẵn chỉ mục cụm. Ngoài ra có 12 chỉ mục thường và 4 chỉ mục UNIQUE có lọc, đều phục vụ một xử lý cụ thể trên DFD:
- tra cứu học viên và kiểm tra trùng hồ sơ (1.1);
- kiểm tra trùng giáo viên, trùng phòng (2.3, QT07);
- tính sĩ số (2.1, 2.2, 5.3);
- doanh thu theo kỳ (3.4);
- số đã thu theo đợt (3.2, 3.3);
- điểm danh theo buổi (4.1);
- thông báo của học viên (5.2);
- nhật ký theo thời gian (5.1).

Danh sách đầy đủ ở cuối `csdl/mo_ta_bang.md`.

Hệ thống có khoảng 500 học viên, mỗi kỳ 25–30 lớp. Lượng dữ liệu một năm vào cỡ vài chục nghìn dòng điểm danh, nên không cần phân vùng bảng hay chỉ mục phức tạp hơn.

---

## 6. View và hàm: tính thuộc tính thứ sinh

Các thuộc tính thứ sinh ở 3.1 mục 5 **không lưu**, mà được tính bằng view hoặc hàm. Con số chính sách đọc qua `fn_ThamSo`, không viết cứng:

| View / hàm | Tính gì | Dùng cho |
|---|---|---|
| `fn_ThamSo(@Ma)` | Giá trị một tham số | Mọi view bên dưới |
| `v_ChuoiDangKy` | Đăng ký đầu chuỗi của mỗi đăng ký (truy vấn đệ quy theo quan hệ bậc 1 Q12) | Học phí của đăng ký chuyển lớp, học lại |
| `v_HocPhiDangKy` | Phải nộp, Đã thu, Còn nợ, Hiệu lực (QT05) | 2.2, 2.4, 3.2 |
| `fn_CongNo(@Ngay)` | Công nợ và quá hạn **tại một ngày**; bỏ đăng ký nghỉ học (QT10) | 3.3, 5.2 (nhắc học phí) |
| `v_DoanhThu` | Từng khoản đã thu, kèm kỳ, khóa, hình thức | 3.4 |
| `v_SiSoLop` | Số đăng ký giữ chỗ, tỷ lệ lấp đầy | 2.1, 2.2, 5.3 |
| `v_ChuyenCan` | Số buổi có mặt, tỷ lệ chuyên cần, cảnh báo vắng, nguy cơ không đạt (QT11, QT12) | 4.1, 5.2, 5.4 |
| `v_KetQuaTinh` | Điểm chuyên cần, điểm tổng kết, xếp loại, kết quả tính theo QT13–QT15 | 4.3; đối chiếu với giá trị chốt |
| `v_HocVienCu` | Học viên cũ và ngày kết thúc lớp đã học (QT01) | 3.1 xét ưu đãi |
| `v_GiangDay` | Số buổi đã dạy thực tế của giáo viên, theo tháng (tính cả dạy thay) | 5.4 |
| `v_TuyenSinh` | Học viên mới, tính theo đăng ký đầu tiên | 5.3 |

`fn_CongNo` là hàm có tham số ngày, không phải view. Như vậy báo cáo công nợ "tại ngày 31/12/2026" luôn cho cùng một kết quả, không phụ thuộc ngày máy chạy.

---

## 7. Dữ liệu mẫu

`seed.sql` do `csdl/sinh_du_lieu.py` sinh ra. Hạt giống ngẫu nhiên cố định, nên chạy lại cho ra đúng tệp cũ. Học phí, ưu đãi, đợt và kết quả được **tính** bằng chính các quy tắc QT, không gõ tay. Ngày chốt dữ liệu là **31/12/2026**: lúc đó lớp IEK-2609 của chứng từ mẫu đã kết thúc và đã cấp chứng nhận.

**Quy mô:** 46 học viên, 25 phụ huynh, 5 giáo viên, 7 nhân viên, 4 khóa, 7 lớp, 58 đăng ký, 111 buổi học, 940 lượt điểm danh, 92 phiếu thu, 35 kết quả, 31 chứng nhận. KE_HOACH dự kiến khoảng 30 học viên và 6 lớp; dữ liệu mẫu tăng lên để có đủ các tình huống dưới đây.

| Tình huống | Dữ liệu mẫu |
|---|---|
| 5 chứng từ mẫu (2.1 mục 4.2) | HV0412, phiếu đăng ký DK2026-0158 (2 lớp), phiếu thu PT2026-0731 (3 dòng, 5.400.000), sổ điểm danh buổi 1–4 lớp IEK-2609, bảng điểm (8,0 Khá; 4,5 Không đạt), chứng nhận CN2026-0089 |
| Học viên cũ (QT01) | HV0412 và 4 bạn học xong IES-2603 (kết thúc 21/05/2026), sau đó đăng ký các lớp IEK |
| Ưu đãi cộng dồn bị chặn (QT04, quy tắc R1) | HV0445: học viên cũ + nhóm + đóng một lần = 20%, bị chặn còn 15% |
| Đăng ký nhóm (QT02) | 3 học viên TOE-2611 cùng ngày 20/10; nhóm 3 người của GT-2701 |
| Chuyển lớp (QT08) | HV0425 chuyển từ IEK-2609 sang IEK-2610 sau 2 buổi; 3 học viên của lớp GT-2611 bị hủy chuyển sang GT-2701 |
| Bảo lưu rồi học lại (QT09) | HV0440 đóng đủ, bảo lưu sau 3/10 buổi đến 05/04/2027, rồi đăng ký học lại GT-2701 bằng phiếu mới, có đăng ký gốc |
| Nghỉ học (QT10) | HV0460 nghỉ ngày 10/10, còn nợ đợt 2 nhưng không bị tính công nợ |
| Lớp hủy, lớp dự kiến (QT06) | GT-2611 hủy; GT-2701 dự kiến (7/8 đăng ký, chưa có buổi học) |
| Buổi hủy, học bù, dạy thay | IEK-2609 nghỉ buổi 13 (giáo viên ốm), học bù 15/12; buổi 9 do GV020 dạy thay |
| Nợ quá hạn (QT16) | 3 học viên TOE-2611 chưa đóng đợt 2, hạn 09/12/2026 |
| Một đợt nộp qua nhiều phiếu (Q16 N–N) | HV0488 nộp đợt 2 qua 2 phiếu, phiếu sau bị trễ hạn |
| Một phiếu thu cho 2 học viên (3.1 E17) | Hai anh em HV0451, HV0452 do bố (PH0320) nộp chung |
| Học viên có cả bố lẫn mẹ (Q01 N–N) | HV0412: PH0301 (bố), PH0302 (mẹ) |
| Cảnh báo chuyên cần (QT12) | 10 đăng ký bị cảnh báo, trong đó có HV0398 (61,5%) |

**Khác biệt nhỏ so với chứng từ mẫu:** bảng điểm mẫu ghi HV0412 có điểm chuyên cần 9,0. Với 26 buổi thực dạy và 24 buổi có mặt, điểm đúng là 24/26 × 10 = 9,2. Điểm tổng kết vẫn là 0,1 × 9,2 + 0,3 × 7,5 + 0,6 × 8,0 = 7,97 → **8,0 – Khá**, khớp chứng từ. Con số 9,0 trên bảng điểm mẫu là số minh họa ở bước thu thập.

---

## 8. Kết quả chạy trên SQL Server

```
powershell -ExecutionPolicy Bypass -File csdl/chay.ps1
```

Đã chạy trên SQL Server 2022 Express (`.\SQLEXPRESS04`). Muốn chạy trên máy khác thì thêm `-May <tên máy chủ>`. Bốn tệp `.sql` chạy không lỗi, chạy lại nhiều lần được, và **24/24 kiểm tra đạt**:

| # | Kiểm tra | Kết quả |
|---|---|---|
| 1–6 | Đối chiếu 5 chứng từ mẫu: phải nộp 6.480.000; phiếu thu 5.400.000; còn nợ 1.080.000, hạn 17/10; sổ điểm danh xMxP / xKKx; bảng điểm; chứng nhận | Đạt |
| 7 | 35 kết quả đã chốt = kết quả tính lại (lệch 0) | Đạt |
| 8–12 | QT01, QT04 (1 khoản bị chặn 15%), QT05 (53 khoản), không nộp vượt, công nợ / quá hạn (QT10, QT16) | Đạt |
| 13–17 | QT06 sĩ số, QT07 không trùng lịch (111 buổi), buổi đúng lịch tuần, QT08 chuyển lớp (4 lần), QT09 bảo lưu | Đạt |
| 18–20 | QT11 điểm danh (940 lượt), QT15 chứng nhận (31), QT12 cảnh báo vắng | Đạt |
| 21–22 | Học viên dưới 18 tuổi đều có phụ huynh; phiếu thu chung của 2 anh em | Đạt |
| 23 | **Ràng buộc chặn dữ liệu sai**: chèn thử 5 bản ghi sai trong một giao dịch rồi hủy. Đó là điểm 11, điểm danh buổi không thuộc lớp, "Đạt" với "Không đạt", vai trò Giáo viên gắn mã học viên, thông báo gửi nhầm phụ huynh. Cả 5 đều bị đúng ràng buộc tương ứng chặn | Đạt |
| 24 | 26 bảng, 35 khóa ngoại, bảng mã `Vietnamese_100_CI_AS` | Đạt |

Một số kết quả báo cáo MIS trên dữ liệu mẫu (phần 2 của `kiem_tra.sql`):
- **Doanh thu:** tháng 9/2026 là 53,64 triệu, tháng 10/2026 là 76,48 triệu. Khóa IELTS Kids Foundation dẫn đầu với 81,6 triệu.
- **Công nợ tại 31/12/2026:** 5 khoản, trong đó 3 khoản quá hạn, mỗi khoản 2,8 triệu.
- **Lấp đầy lớp:** từ 50% đến 56%. GT-2701 mới đạt 35%, chưa đủ sĩ số để mở.
- **Tỷ lệ đạt:** GT-2604 77,8%; IEK-2609 88,9%; IEK-2610 100%; IES-2603 88,9%.
- **Số buổi đã dạy:** GV020 dạy 27 buổi, tức 26 buổi của lớp mình và 1 buổi dạy thay; GV015 dạy 25 buổi.

---

## 9. Đầu ra cho các bước sau

| Bước | Dùng gì từ bước này |
|---|---|
| 3.5 Module, phân quyền | `TaiKhoan.VaiTro` (8 vai trò) và ràng buộc `CK_TaiKhoan_ChuSoHuu` |
| 3.6 Giao diện | Danh sách miền giá trị (thành danh sách chọn trên form). Tên ràng buộc → câu báo lỗi. Các view → báo cáo |
| GĐ4 Demo | Chạy thẳng trên CSDL này: EF Core ánh xạ 26 bảng; các quy tắc "ở tầng ứng dụng" của mục 3 là phần cần lập trình; `kiem_tra.sql` dùng làm bộ kiểm thử hồi quy |
| GĐ4 Chuyển đổi dữ liệu | `sinh_du_lieu.py` là mẫu cho việc nhập dữ liệu Excel cũ theo đúng thứ tự khóa ngoại |
