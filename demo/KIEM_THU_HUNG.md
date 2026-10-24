# Kiểm thử phần việc Vũ Văn Hùng

H1 bổ sung R3.1 tại `/HocPhi/HocPhiPhaiThu`. H2 bổ sung tham số phiên bản và tệp nâng cấp 002. Các thay đổi tương ứng TC37 và TC38 trong `phan_tich/05_cai_dat.md`.

## Hoàn thành H3 ngày 24/10/2026

Đã sửa tổng dữ liệu mẫu trong `thiet_ke/04_csdl.md` thành **1.873 dòng**, đối chiếu bằng tổng 26 dòng của bảng tổng hợp. Đã chuyển TC37, TC38 lên trước TC39 và cập nhật kết quả sau kiểm thử tích hợp thực tế. Nội dung slide 8 và 10, kèm ghi chú trình bày 1–2 phút mỗi slide, nằm ở `bao_cao/noi_dung_slide_vu_van_hung.md` để ghép vào bộ slide chung.

Kết quả trên mã nguồn đã hợp nhất `origin/main` tại `dd5361f`:

| Kiểm tra | Kết quả |
|---|---|
| .NET SDK 8.0.425, build ứng dụng | 0 lỗi, 0 cảnh báo biên dịch |
| xUnit | 98/98 đạt |
| `demo/sinh_ma.py` | Đạt, 127 tệp C# và Razor; 37 màn hình trong danh mục thiết kế |
| Sinh lại seed và mô tả CSDL | Không thay đổi tệp sinh; 26 bảng, 164 trường, 35 khóa ngoại, 16 chỉ mục |
| `csdl/chay.ps1` trên SQL Server 2022 Docker riêng | 24/24 đạt |
| `csdl/kiem_tra_hung.ps1` | Đạt: thêm khi thiếu, chạy lại, nâng 1 lên 2, không hạ 3 xuống 2; hoàn tác giao dịch |
| TC37, HTTP và Excel | Đạt: từng dòng, ưu đãi, tiền từng đợt, hạn, đã thu và tổng khớp SQL; lọc lớp/mã HV, chuẩn hóa mã, kết hợp lọc, kết quả rỗng, trang 1/2; Excel giữ toàn bộ bộ lọc |
| TC38, HTTP và SQL | Đạt: phiên bản không có ô nhập; POST phiên bản hiện tại hoặc 999 bị từ chối, CSDL giữ nguyên; sửa chính sách vẫn lưu và ghi nhật ký, sau đó khôi phục. Chạy 002 thật rồi kiểm lại: giao diện hiện 2, POST 999 vẫn bị chặn và SQL giữ 2 |

Kết quả quyền truy cập R3.1 qua đăng nhập thật, có token chống giả mạo và cookie riêng:

| Tài khoản | Vai trò | HTML và Excel |
|---|---|---|
| nv001 | Giám đốc | 53 khoản |
| nv002 | Quản lý đào tạo | Chuyển về `/KhongCoQuyen` |
| nv003 | Quản trị | Chuyển về `/KhongCoQuyen` |
| nv004 | Kế toán | 53 khoản |
| nv005 | Tuyển sinh | 53 khoản |
| gv023 | Giáo viên | Chuyển về `/KhongCoQuyen` |
| hv0412 | Học viên | Chỉ 3 khoản của HV0412; lọc HV0480 không có dữ liệu |
| ph0301 | Phụ huynh | Chỉ 3 khoản của con; lọc HV0480 không có dữ liệu |

Tổng phải nộp toàn bộ là **213.880.000 đồng**, đối chiếu `SUM(DotHocPhi.SoTien)` nối `HocPhi`. Script HTTP đọc dữ liệu SQL độc lập, so từng dòng HTML và Excel, không chỉ so tổng. Kết quả này xác nhận giao diện HTML trả qua HTTP; chưa đánh giá thao tác trực quan hoặc bố cục in bằng trình duyệt.

### Chạy lại trên SQL Server riêng

Hai script PowerShell vẫn giữ mặc định Windows Authentication tới `.\SQLEXPRESS04`. Khi dùng SQL Server thử nghiệm với SQL Authentication, đặt biến môi trường `NHOM10_SQL_CONNECTION` chứa chuỗi kết nối; không ghi mật khẩu vào tệp theo dõi bởi Git hoặc nhật ký. Với `chay.ps1`, chuỗi kết nối có thể bắt đầu ở `master`; với script kiểm tra và ứng dụng, chọn `Database=TrungTamNgoaiNgu`.

```powershell
# Chỉ thực hiện trên CSDL demo riêng: chay.ps1 dựng lại cấu trúc và dữ liệu.
powershell -NoProfile -ExecutionPolicy Bypass -File csdl/chay.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File csdl/kiem_tra_hung.ps1

# Cửa sổ chạy ứng dụng: dùng cùng SQL Server và Database=TrungTamNgoaiNgu.
$env:ConnectionStrings__TrungTam = $env:NHOM10_SQL_CONNECTION
dotnet run --project demo/TrungTamNgoaiNgu --no-launch-profile --urls http://127.0.0.1:5223

# Cửa sổ kiểm thử: đặt cùng NHOM10_SQL_CONNECTION trước khi chạy.
python demo/kiem_tra_hung_http.py --url http://127.0.0.1:5223
```

`kiem_tra_hung_http.py` cần Python chuẩn và Windows PowerShell, không cần cài thêm thư viện. Script thử POST thay đổi `SISO_TOI_DA` rồi khôi phục bằng `finally`; nhật ký đăng nhập/sửa vẫn được giữ. Dừng ứng dụng rồi chạy lại `csdl/chay.ps1` sau khi kiểm thử để đưa CSDL demo về seed và phiên bản 1. Tệp `kiem_tra_hung.ps1` được lưu UTF-8 BOM để Windows PowerShell 5.1 đọc đúng tiếng Việt.

## Lịch sử kiểm thử trước H3

Kết quả ngày 12/10/2026: build không lỗi hoặc cảnh báo; xUnit 98/98 đạt (80 ca cũ và 18 ca mới); `demo/sinh_ma.py` đạt với 122 tệp; sinh lại seed và mô tả CSDL đạt; `git diff --check` đạt. SDK 8.0.425 được dùng từ thư mục tạm của máy. Chưa kiểm thử tích hợp SQL Server/HTTP thật: instance `.\SQLEXPRESS04` không có trên máy hiện tại, `csdl/chay.ps1` lỗi kết nối. Các ca InMemory không xác nhận được thực thi SQL hoặc giao diện trình duyệt.

## Kiểm tra mã nguồn

Kiểm tra lại ngày 14/10/2026 trên nhánh `vu-van-hung` sau khi cập nhật từ `main`: build 0 lỗi/0 cảnh báo, xUnit 98/98 đạt, kiểm tra mã đạt với 126 tệp. Bảng ca kiểm thử hiện có 42 ca (đã gồm TC39/TC40 từ các phần khác và TC37/TC38 của Hùng).

```powershell
dotnet build demo/TrungTamNgoaiNgu
dotnet test demo/KiemThu
python demo/sinh_ma.py
python csdl/sinh_du_lieu.py
python csdl/sinh_mo_ta.py
```

Kiểm thử xUnit dùng EF Core InMemory cho phạm vi dữ liệu và xử lý POST; kết quả này không thay thế kiểm thử SQL Server hoặc HTTP thật. Có kiểm tra bộ lọc quyền và thực đơn R3.1 cho đủ 8 vai trò; học viên/phụ huynh không thể mở rộng phạm vi bằng bộ lọc mã HV hoặc xuất Excel; khoản của đăng ký nghỉ học vẫn được đưa vào R3.1. Kiểm tra POST phiên bản bằng cả giá trị hiện tại và 999; tham số chính sách khác vẫn lưu được.

## Kiểm tra SQL Server

Máy cần SQL Server với instance được cấu hình trong `appsettings.json`. Tạo lại dữ liệu mẫu bằng `csdl/chay.ps1` chỉ trên CSDL demo, vì lệnh này xóa và dựng lại dữ liệu.

```powershell
powershell -ExecutionPolicy Bypass -File csdl/chay.ps1
powershell -ExecutionPolicy Bypass -File csdl/kiem_tra_hung.ps1
# Nâng bản mẫu từ 1 lên 2 khi cần thử F5.1b với phiên bản 2
powershell -ExecutionPolicy Bypass -File csdl/chay.ps1 -Tep nang_cap/002_phien_ban_csdl.sql
```

`kiem_tra_hung.ps1` thử thiếu dòng, chạy lại, nâng phiên bản 1 và giữ phiên bản cao hơn; mọi thay đổi thử nghiệm nằm trong giao dịch và được hoàn tác. Sau khi tạo lại seed, phiên bản là 1 và bảng ThamSo có 15 dòng. Schema không có CHECK giới hạn giá trị hoặc đơn vị ThamSo, nên giá trị 1/2 và đơn vị `phiên bản` phù hợp schema hiện tại.

Đối chiếu tổng trên R3.1 bằng SQL sau. Thêm cùng điều kiện lớp/mã HV vào cả hai truy vấn khi kiểm tra bộ lọc. Không có cột `SoTienPhaiNop` trong HocPhi; số phải nộp là tổng tiền các đợt đã lập, giữ kết quả làm tròn của quy tắc chia đợt.

```sql
SELECT SUM(h.HocPhiGoc) AS TongGoc FROM dbo.HocPhi h;
SELECT SUM(d.SoTien) AS TongPhaiNop
FROM dbo.DotHocPhi d
JOIN dbo.HocPhi h ON h.MaHV = d.MaHV AND h.MaLop = d.MaLop;
SELECT h.MaHV, h.MaLop, d.Dot, d.SoTien, d.HanDong,
       COALESCE((SELECT SUM(c.SoTien) FROM dbo.ChiTietPhieuThu c
                 WHERE c.MaHV = d.MaHV AND c.MaLop = d.MaLop AND c.Dot = d.Dot), 0) AS DaThu
FROM dbo.HocPhi h
JOIN dbo.DotHocPhi d ON d.MaHV = h.MaHV AND d.MaLop = h.MaLop
ORDER BY h.MaLop, h.MaHV, d.Dot;
```

## Kiểm tra giao diện và HTTP thật

Chạy demo rồi mở R3.1 bằng nv001, nv004, nv005, hv0412, ph0301 (được xem), nv002, nv003, gv023 (bị chặn). Đổi lớp, mã HV, phân trang và xuất Excel; kiểm tra tổng vẫn bao gồm mọi trang trong bộ lọc. Với HV/PH, nhập mã của học viên khác phải không có dữ liệu cả trên trang và trong Excel. Mở bằng nv003: F5.1b không có ô nhập phiên bản. Gửi POST có mã chống giả mạo hợp lệ kèm `GiaTri[PHIEN_BAN_CSDL]=999` phải báo lỗi tiếng Việt và không đổi phiên bản; gửi tham số chính sách hợp lệ vẫn lưu được. Sau đợt thử có thay đổi dữ liệu demo, chạy lại `csdl/chay.ps1`.
