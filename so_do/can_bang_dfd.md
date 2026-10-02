# Bảng cân bằng DFD (sinh tự động từ so_do/src/dfd.py – không sửa tay)

## Tác nhân: mức ngữ cảnh ↔ mức 0

| Tác nhân | Hướng | Luồng ở ngữ cảnh | Tiến trình mức 0 |
|---|---|---|---|
| Học viên/Phụ huynh | Vào hệ thống | Yêu cầu đăng ký, chuyển lớp, bảo lưu | 2.0 |
| Học viên/Phụ huynh | Vào hệ thống | Tiền học phí | 3.0 |
| Học viên/Phụ huynh | Ra khỏi hệ thống | Lịch học | 2.0 |
| Học viên/Phụ huynh | Ra khỏi hệ thống | Phiếu thu | 3.0 |
| Học viên/Phụ huynh | Ra khỏi hệ thống | Chuyên cần, kết quả học tập, chứng nhận | 4.0 |
| Học viên/Phụ huynh | Ra khỏi hệ thống | Thông báo | 5.0 |
| Nhân viên tuyển sinh/Chăm sóc học viên | Vào hệ thống | Hồ sơ học viên, kết quả kiểm tra đầu vào | 1.0 |
| Nhân viên tuyển sinh/Chăm sóc học viên | Vào hệ thống | Phiếu đăng ký, đơn chuyển lớp, bảo lưu | 2.0 |
| Nhân viên tuyển sinh/Chăm sóc học viên | Ra khỏi hệ thống | Kết quả đăng ký, danh sách lớp | 2.0 |
| Nhân viên kế toán | Vào hệ thống | Thông tin thu tiền, ưu đãi | 3.0 |
| Nhân viên kế toán | Ra khỏi hệ thống | Học phí phải thu, phiếu thu, công nợ | 3.0 |
| Giáo viên | Vào hệ thống | Điểm danh, điểm số, nhận xét | 4.0 |
| Giáo viên | Ra khỏi hệ thống | Lịch dạy, danh sách lớp | 2.0 |
| Quản lý đào tạo | Vào hệ thống | Thông tin khóa học, giáo viên, phòng học | 1.0 |
| Quản lý đào tạo | Vào hệ thống | Kế hoạch mở lớp, phân công giáo viên | 2.0 |
| Quản lý đào tạo | Ra khỏi hệ thống | Báo cáo lớp học, chuyên cần, kết quả, giảng dạy | 5.0 |
| Quản trị viên | Vào hệ thống | Tài khoản, phân quyền, cấu hình | 5.0 |
| Quản trị viên | Ra khỏi hệ thống | Nhật ký hệ thống | 5.0 |
| Giám đốc | Vào hệ thống | Yêu cầu báo cáo doanh thu | 3.0 |
| Giám đốc | Vào hệ thống | Yêu cầu báo cáo tổng hợp | 5.0 |
| Giám đốc | Ra khỏi hệ thống | Báo cáo doanh thu, công nợ | 3.0 |
| Giám đốc | Ra khỏi hệ thống | Báo cáo tuyển sinh, kết quả học tập | 5.0 |

## Kho dữ liệu ↔ tiến trình mức 0

| Kho | Tiến trình | Hướng | Luồng |
|---|---|---|---|
| D1 Hồ sơ | 1.0 | Đọc từ kho | Hồ sơ hiện có |
| D1 Hồ sơ | 1.0 | Ghi vào kho | Hồ sơ cập nhật |
| D1 Hồ sơ | 2.0 | Đọc từ kho | Học viên, giáo viên, khóa học, phòng học |
| D1 Hồ sơ | 3.0 | Đọc từ kho | Học phí khóa học |
| D1 Hồ sơ | 5.0 | Đọc từ kho | Hồ sơ học viên, giáo viên |
| D2 Lớp và lịch học | 2.0 | Đọc từ kho | Sĩ số, lịch đã xếp |
| D2 Lớp và lịch học | 2.0 | Ghi vào kho | Lớp, đăng ký, lịch học |
| D2 Lớp và lịch học | 3.0 | Đọc từ kho | Đăng ký học |
| D2 Lớp và lịch học | 4.0 | Đọc từ kho | Danh sách lớp, buổi học |
| D2 Lớp và lịch học | 5.0 | Đọc từ kho | Lớp, đăng ký |
| D3 Học phí | 3.0 | Đọc từ kho | Số đã thu, công nợ |
| D3 Học phí | 3.0 | Ghi vào kho | Học phí, phiếu thu |
| D3 Học phí | 5.0 | Đọc từ kho | Công nợ |
| D4 Học tập | 4.0 | Đọc từ kho | Điểm thành phần |
| D4 Học tập | 4.0 | Ghi vào kho | Điểm danh, điểm, kết quả |
| D4 Học tập | 5.0 | Đọc từ kho | Chuyên cần, kết quả |
| D5 Tài khoản và thông báo | 5.0 | Đọc từ kho | Quyền truy cập |
| D5 Tài khoản và thông báo | 5.0 | Ghi vào kho | Tài khoản, thông báo, nhật ký |
