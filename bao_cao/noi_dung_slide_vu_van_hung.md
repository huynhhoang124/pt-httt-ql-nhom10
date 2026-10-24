# Nội dung thuyết trình của Vũ Văn Hùng

Phần chuẩn bị để ghép vào bộ slide chung của Nhóm 10 theo mục 7 tài liệu phân công ngày 16/10/2026. Gồm slide 8 về quyết định học phí và slide 10 về CSDL, mỗi slide có ghi chú trình bày khoảng 1–2 phút. Số liệu dưới đây đối chiếu mã nguồn ngày 24/10/2026; kiểm tra lại nếu nhóm thay đổi thiết kế sau đợt tổng hợp ngày 25/10.

## Slide 8 Bảng quyết định tính học phí

- Ba điều kiện tạo thành tám quy tắc R1–R8: học viên cũ, nhóm từ ba người và đóng một lần.
- Mức giảm tương ứng là 10%, 5% và 5%; chỉ áp dụng ưu đãi còn hiệu lực, tổng giảm tối đa 15% theo QT04.
- R1 đủ ba điều kiện: cộng 20%, thực tế giảm 15%; R8 không có điều kiện nào: giảm 0%.
- Đóng một lần thu một đợt; còn lại chia hai đợt theo tỷ lệ 50% và 50% của QT05.
- DK2026-0158 thuộc R4: học viên cũ, đăng ký riêng, chia hai đợt; giảm 10%, tổng phải nộp 6.480.000 đồng.

### Bảng để đưa lên slide

| Quy tắc | Học viên cũ | Nhóm từ 3 người | Đóng một lần | Giảm thực tế | Số đợt |
|---|---|---|---|---|---|
| R1 | Có | Có | Có | 15% | 1 |
| R2 | Có | Có | Không | 15% | 2 |
| R3 | Có | Không | Có | 15% | 1 |
| R4 | Có | Không | Không | 10% | 2 |
| R5 | Không | Có | Có | 10% | 1 |
| R6 | Không | Có | Không | 5% | 2 |
| R7 | Không | Không | Có | 5% | 1 |
| R8 | Không | Không | Không | 0% | 2 |

### Ghi chú người trình bày

Phần tính học phí có nhiều tổ hợp ưu đãi nên nhóm dùng bảng quyết định để tránh bỏ sót trường hợp. Ba câu hỏi là học viên đã học tại trung tâm hay chưa, có đăng ký theo nhóm từ ba người hay không và có đóng học phí một lần hay không. Mỗi câu trả lời có hai khả năng, tạo ra tám cột quy tắc từ R1 đến R8. Điều kiện ưu đãi chỉ được tính khi ưu đãi còn hiệu lực.

Mức giảm cho học viên cũ là 10%, nhóm là 5% và đóng một lần là 5%. Trường hợp R1 có đủ ba ưu đãi, tổng cộng là 20%, nhưng quy tắc QT04 giới hạn mức giảm thực tế còn 15%. Nếu đóng một lần thì thu một đợt; nếu chia đợt thì áp dụng tỷ lệ 50% và 50% theo QT05. Các con số chính sách được đọc từ CSDL để có thể điều chỉnh mà không sửa thuật toán.

Ví dụ phiếu DK2026-0158 là học viên cũ, đăng ký một mình và chia hai đợt nên thuộc R4. Học phí gốc của hai lớp là 7.200.000 đồng, giảm 10% còn 6.480.000 đồng. Bảng quyết định giúp đối chiếu được cả chứng từ mẫu và các ca kiểm thử. Khi xem R3.1, số phải nộp lấy từ tổng các đợt đã lập để giữ đúng kết quả làm tròn.

Nguồn: `phan_tich/04_dac_ta.md` mục 3.1; `demo/TrungTamNgoaiNgu/NghiepVu/QuyTac.cs`; `demo/TrungTamNgoaiNgu/Pages/HocPhi/HocPhiPhaiThu.cshtml.cs`.

## Slide 10 Cơ sở dữ liệu vật lý

- SQL Server với bảng mã `Vietnamese_100_CI_AS`; thiết kế xuất phát từ 26 thực thể đã chuẩn hóa.
- 26 bảng, 164 trường, 35 khóa ngoại, 63 CHECK và 16 chỉ mục.
- CHECK giữ các bất biến; tham số chính sách lưu trong `ThamSo`, gồm 14 tham số nghiệp vụ và một dòng phiên bản CSDL.
- Bộ dữ liệu mẫu có 1.873 dòng; gồm 46 học viên, 58 đăng ký, 111 buổi học và 940 lượt điểm danh.
- Quy trình dựng: `schema.sql` → `views.sql` → `seed.sql` → `kiem_tra.sql`; phiên bản seed là 1, nâng cấp 002 đưa lên 2 và chạy lại an toàn.

### Ghi chú người trình bày

Sau khi chuẩn hóa mô hình dữ liệu, nhóm chuyển 26 thực thể thành 26 bảng trên SQL Server. Thiết kế hiện có 164 trường và 35 khóa ngoại để bảo đảm liên kết giữa các bảng. Các ràng buộc CHECK dùng cho điều kiện luôn đúng, chẳng hạn miền giá trị của điểm; còn các con số có thể đổi theo chính sách, như tỷ lệ giảm tối đa, được lưu trong bảng ThamSo.

Bộ dữ liệu mẫu không mô phỏng toàn bộ quy mô hoạt động của trung tâm. Nhóm tạo một bộ nhỏ gồm 46 học viên, 58 đăng ký và 111 buổi học, nhưng có đủ các tình huống cần kiểm tra như học viên cũ, ưu đãi cộng dồn, nợ quá hạn, chuyển lớp và bảo lưu. Tổng số dòng ở 26 bảng là 1.873, trong đó bảng ThamSo có 15 dòng: 14 tham số nghiệp vụ và một dòng ghi phiên bản CSDL.

Để tái lập kết quả, nhóm chạy lần lượt tệp cấu trúc, các view và hàm, dữ liệu mẫu, rồi bộ 24 truy vấn kiểm tra. Tệp seed đã gồm thay đổi của nâng cấp 001 nên mang phiên bản 1. Nâng cấp 002 bổ sung hoặc cập nhật phiên bản lên 2; chạy lại không tạo dòng trùng và không hạ phiên bản cao hơn. Trên giao diện quản trị, phiên bản chỉ được xem, còn thay đổi phiên bản phải đi qua tệp nâng cấp. Cách này giúp biết CSDL đã áp dụng tới tệp nào khi bảo trì.

Nguồn: `thiet_ke/04_csdl.md` mục 2–7; `csdl/schema.sql`; `csdl/sinh_du_lieu.py`; `csdl/nang_cap/002_phien_ban_csdl.sql`.
