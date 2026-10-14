# Kiểm thử phần việc Vũ Văn Hùng

H1 bổ sung R3.1 tại `/HocPhi/HocPhiPhaiThu`. H2 bổ sung tham số phiên bản và tệp nâng cấp 002. Các thay đổi tương ứng TC37 và TC38 trong `phan_tich/05_cai_dat.md`.

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
