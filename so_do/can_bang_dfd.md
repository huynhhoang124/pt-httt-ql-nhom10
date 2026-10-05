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
| Quản lý đào tạo | Vào hệ thống | Yêu cầu đổi lịch, học bù, dạy thay | 2.0 |
| Quản lý đào tạo | Vào hệ thống | Phê duyệt kết quả học tập | 4.0 |
| Quản lý đào tạo | Ra khỏi hệ thống | Bảng điểm chờ duyệt | 4.0 |
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
| D1 Hồ sơ | 4.0 | Đọc từ kho | Trọng số điểm khóa học |
| D1 Hồ sơ | 5.0 | Đọc từ kho | Hồ sơ học viên, giáo viên |
| D2 Lớp và lịch học | 2.0 | Đọc từ kho | Đăng ký, sĩ số, lịch đã xếp |
| D2 Lớp và lịch học | 2.0 | Ghi vào kho | Lớp, đăng ký, lịch học |
| D2 Lớp và lịch học | 3.0 | Đọc từ kho | Đăng ký học |
| D2 Lớp và lịch học | 4.0 | Đọc từ kho | Danh sách lớp, buổi học |
| D2 Lớp và lịch học | 5.0 | Đọc từ kho | Lớp, đăng ký, buổi học |
| D3 Học phí | 2.0 | Đọc từ kho | Tình trạng đóng học phí |
| D3 Học phí | 3.0 | Đọc từ kho | Học phí phải thu, số đã thu |
| D3 Học phí | 3.0 | Ghi vào kho | Ưu đãi, học phí, phiếu thu |
| D3 Học phí | 5.0 | Đọc từ kho | Công nợ |
| D4 Học tập | 4.0 | Đọc từ kho | Điểm danh, điểm thành phần |
| D4 Học tập | 4.0 | Ghi vào kho | Điểm danh, điểm, kết quả, chứng nhận |
| D4 Học tập | 5.0 | Đọc từ kho | Chuyên cần, kết quả |
| D5 Tài khoản và thông báo | 5.0 | Đọc từ kho | Quyền truy cập, nhật ký |
| D5 Tài khoản và thông báo | 5.0 | Ghi vào kho | Tài khoản, thông báo, nhật ký |

## DFD-1.0 (Quản lý danh mục và hồ sơ): mức 0 ↔ mức 1

| Tác nhân / kho | Hướng | Luồng mức 0 | Phân rã ở mức 1 |
|---|---|---|---|
| Nhân viên tuyển sinh/Chăm sóc học viên | Vào | Hồ sơ học viên, kết quả kiểm tra đầu vào | 1.1: Hồ sơ học viên, kết quả kiểm tra đầu vào |
| Quản lý đào tạo | Vào | Thông tin khóa học, giáo viên, phòng học | 1.2: Thông tin giáo viên; 1.3: Thông tin khóa học; 1.4: Thông tin phòng học |
| D1 Hồ sơ | Ghi vào kho | Hồ sơ cập nhật | 1.1: Hồ sơ học viên; 1.2: Hồ sơ giáo viên; 1.3: Khóa học; 1.4: Phòng học |
| D1 Hồ sơ | Đọc từ kho | Hồ sơ hiện có | 1.1: Hồ sơ hiện có |

## DFD-2.0 (Quản lý lớp học và lịch học): mức 0 ↔ mức 1

| Tác nhân / kho | Hướng | Luồng mức 0 | Phân rã ở mức 1 |
|---|---|---|---|
| Học viên/Phụ huynh | Vào | Yêu cầu đăng ký, chuyển lớp, bảo lưu | 2.2: Yêu cầu đăng ký; 2.4: Yêu cầu chuyển lớp, bảo lưu |
| Nhân viên tuyển sinh/Chăm sóc học viên | Vào | Phiếu đăng ký, đơn chuyển lớp, bảo lưu | 2.2: Phiếu đăng ký; 2.4: Đơn chuyển lớp, bảo lưu |
| Quản lý đào tạo | Vào | Kế hoạch mở lớp, phân công giáo viên | 2.1: Kế hoạch mở lớp, phân công giáo viên |
| Quản lý đào tạo | Vào | Yêu cầu đổi lịch, học bù, dạy thay | 2.3: Yêu cầu đổi lịch, học bù, dạy thay |
| Nhân viên tuyển sinh/Chăm sóc học viên | Ra | Kết quả đăng ký, danh sách lớp | 2.2: Kết quả đăng ký, danh sách lớp; 2.4: Kết quả xử lý đăng ký |
| Học viên/Phụ huynh | Ra | Lịch học | 2.3: Lịch học |
| Giáo viên | Ra | Lịch dạy, danh sách lớp | 2.3: Lịch dạy, danh sách lớp |
| D1 Hồ sơ | Đọc từ kho | Học viên, giáo viên, khóa học, phòng học | 2.1: Khóa học, giáo viên; 2.3: Giáo viên, phòng học; 2.2: Học viên |
| D2 Lớp và lịch học | Ghi vào kho | Lớp, đăng ký, lịch học | 2.1: Lớp học; 2.3: Lịch học (buổi học); 2.2: Đăng ký; 2.4: Đăng ký cập nhật |
| D2 Lớp và lịch học | Đọc từ kho | Đăng ký, sĩ số, lịch đã xếp | 2.1: Sĩ số đăng ký; 2.3: Lịch đã xếp; 2.2: Sĩ số lớp; 2.4: Đăng ký, sĩ số, buổi đã học |
| D3 Học phí | Đọc từ kho | Tình trạng đóng học phí | 2.2: Tình trạng đóng học phí; 2.4: Tình trạng đóng học phí |
| (nội bộ) | 2.1 → 2.3 | – | Lớp đã mở |

## DFD-3.0 (Quản lý học phí): mức 0 ↔ mức 1

| Tác nhân / kho | Hướng | Luồng mức 0 | Phân rã ở mức 1 |
|---|---|---|---|
| Học viên/Phụ huynh | Vào | Tiền học phí | 3.2: Tiền học phí |
| Nhân viên kế toán | Vào | Thông tin thu tiền, ưu đãi | 3.1: Ưu đãi; 3.2: Thông tin thu tiền |
| Giám đốc | Vào | Yêu cầu báo cáo doanh thu | 3.4: Yêu cầu báo cáo doanh thu |
| Học viên/Phụ huynh | Ra | Phiếu thu | 3.2: Phiếu thu |
| Nhân viên kế toán | Ra | Học phí phải thu, phiếu thu, công nợ | 3.1: Học phí phải thu; 3.2: Phiếu thu; 3.3: Công nợ |
| Giám đốc | Ra | Báo cáo doanh thu, công nợ | 3.4: Báo cáo doanh thu, công nợ |
| D1 Hồ sơ | Đọc từ kho | Học phí khóa học | 3.1: Học phí khóa học |
| D2 Lớp và lịch học | Đọc từ kho | Đăng ký học | 3.1: Đăng ký học; 3.3: Trạng thái đăng ký |
| D3 Học phí | Ghi vào kho | Ưu đãi, học phí, phiếu thu | 3.1: Ưu đãi, học phí phải thu; 3.2: Phiếu thu mới |
| D3 Học phí | Đọc từ kho | Học phí phải thu, số đã thu | 3.2: Học phí phải thu, số đã thu; 3.3: Học phí phải thu, số đã thu; 3.4: Số đã thu theo kỳ |
| (nội bộ) | 3.3 → 3.4 | – | Tổng công nợ |

## DFD-4.0 (Quản lý học tập): mức 0 ↔ mức 1

| Tác nhân / kho | Hướng | Luồng mức 0 | Phân rã ở mức 1 |
|---|---|---|---|
| Giáo viên | Vào | Điểm danh, điểm số, nhận xét | 4.1: Phiếu điểm danh buổi học; 4.2: Điểm số, nhận xét |
| Quản lý đào tạo | Vào | Phê duyệt kết quả học tập | 4.3: Phê duyệt kết quả học tập |
| Học viên/Phụ huynh | Ra | Chuyên cần, kết quả học tập, chứng nhận | 4.1: Chuyên cần; 4.3: Kết quả học tập; 4.4: Chứng nhận |
| Quản lý đào tạo | Ra | Bảng điểm chờ duyệt | 4.3: Bảng điểm chờ duyệt |
| D1 Hồ sơ | Đọc từ kho | Trọng số điểm khóa học | 4.3: Trọng số điểm khóa học |
| D2 Lớp và lịch học | Đọc từ kho | Danh sách lớp, buổi học | 4.1: Danh sách lớp, buổi học; 4.2: Danh sách lớp |
| D4 Học tập | Ghi vào kho | Điểm danh, điểm, kết quả, chứng nhận | 4.1: Điểm danh đã ghi; 4.2: Điểm thành phần; 4.3: Kết quả học tập; 4.4: Chứng nhận |
| D4 Học tập | Đọc từ kho | Điểm danh, điểm thành phần | 4.3: Điểm danh, điểm thành phần |
| (nội bộ) | 4.3 → 4.4 | – | Danh sách học viên đạt |

## DFD-5.0 (Quản lý hệ thống và báo cáo): mức 0 ↔ mức 1

| Tác nhân / kho | Hướng | Luồng mức 0 | Phân rã ở mức 1 |
|---|---|---|---|
| Quản trị viên | Vào | Tài khoản, phân quyền, cấu hình | 5.1: Tài khoản, phân quyền, cấu hình |
| Giám đốc | Vào | Yêu cầu báo cáo tổng hợp | 5.3: Yêu cầu báo cáo tổng hợp; 5.4: Yêu cầu báo cáo tổng hợp |
| Quản lý đào tạo | Ra | Báo cáo lớp học, chuyên cần, kết quả, giảng dạy | 5.3: Báo cáo lớp học; 5.4: Báo cáo chuyên cần, kết quả, giảng dạy |
| Học viên/Phụ huynh | Ra | Thông báo | 5.2: Thông báo |
| Giám đốc | Ra | Báo cáo tuyển sinh, kết quả học tập | 5.3: Báo cáo tuyển sinh; 5.4: Báo cáo kết quả học tập |
| Quản trị viên | Ra | Nhật ký hệ thống | 5.1: Nhật ký hệ thống |
| D1 Hồ sơ | Đọc từ kho | Hồ sơ học viên, giáo viên | 5.1: Hồ sơ học viên, giáo viên; 5.2: Liên hệ học viên, phụ huynh; 5.3: Hồ sơ học viên; 5.4: Giáo viên |
| D2 Lớp và lịch học | Đọc từ kho | Lớp, đăng ký, buổi học | 5.2: Lịch học thay đổi; 5.3: Lớp, đăng ký; 5.4: Lớp, buổi học |
| D3 Học phí | Đọc từ kho | Công nợ | 5.2: Công nợ |
| D4 Học tập | Đọc từ kho | Chuyên cần, kết quả | 5.2: Chuyên cần, kết quả; 5.4: Chuyên cần, kết quả |
| D5 Tài khoản và thông báo | Ghi vào kho | Tài khoản, thông báo, nhật ký | 5.1: Tài khoản, nhật ký; 5.2: Thông báo đã gửi |
| D5 Tài khoản và thông báo | Đọc từ kho | Quyền truy cập, nhật ký | 5.1: Quyền truy cập, nhật ký |
