# GĐ1 Xác định, lựa chọn và lập kế hoạch hệ thống – Hệ thống quản lý trung tâm ngoại ngữ

Thuộc giai đoạn 1 của vòng đời phát triển hệ thống (KE_HOACH.md, mục Giai đoạn 1). Theo bài giảng (Ch.1), giai đoạn này gồm ba hoạt động: **xác định** hệ thống, **lựa chọn** hệ thống và **lập kế hoạch**; trong đó phải xác định tính khả thi (kinh tế, kỹ thuật, tác nghiệp, pháp lý, chính trị), lợi ích và chi phí (hữu hình, vô hình) của dự án.
Tài liệu được viết sau giai đoạn phân tích nên dùng lại số liệu của Bài tập 1 và của `phan_tich/02_thu_thap.md`. Các con số tiền là **ước tính giả định** cho một trung tâm quy mô vừa.

---

## 1. Xác định hệ thống

### 1.1 Bối cảnh và vấn đề

Trung tâm có một cơ sở, khoảng 500 học viên đang học và 30 giáo viên; mỗi kỳ khai giảng 25–30 lớp. Dữ liệu đang nằm rải rác trên giấy, Excel và Zalo, gây ra 8 vấn đề V1–V8 (mục 9.1 của thu thập thông tin). Ba vấn đề nặng nhất:
- tính sai ưu đãi và khó theo dõi công nợ → thất thu học phí (V4, V5);
- không biết lớp còn chỗ, trùng phòng và trùng giáo viên khi xếp lịch (V2, V3);
- báo cáo tháng phải gom Excel bằng tay, chậm và không tin cậy (V8).

### 1.2 Người đề xuất và mức độ ưu tiên

Dự án do **Giám đốc** đề xuất. Theo bài giảng, dự án do lãnh đạo cấp cao đề xuất thường gắn với mục tiêu chiến lược và liên quan nhiều bộ phận. Ở đây dự án liên quan đủ 4 bộ phận (đào tạo, tuyển sinh – CSHV, kế toán, CNTT) và gắn với chính sách năm tới của trung tâm: giữ chân học viên cũ, mở thêm lớp thiếu nhi.

### 1.3 Mục tiêu của hệ thống

| # | Mục tiêu | Chỉ tiêu đo được (sau 6 tháng vận hành) |
|---|---|---|
| MT1 | Tập trung dữ liệu học viên, lớp, học phí, học tập vào một CSDL | 100% học viên đang học có hồ sơ duy nhất, không trùng |
| MT2 | Rút ngắn thời gian ghi danh | Từ 10–15 phút xuống ≤ 5 phút một phiếu |
| MT3 | Tính học phí, ưu đãi và công nợ chính xác | Không còn phiếu áp sai ưu đãi; công nợ xem được ngay |
| MT4 | Không trùng lịch | 0 lần trùng phòng hoặc trùng giáo viên trong một kỳ (hiện 2–3 lần) |
| MT5 | Thông tin kịp thời cho học viên, phụ huynh | Phụ huynh nhận thông báo vắng học trong ngày |
| MT6 | Báo cáo quản lý tự động | Báo cáo tháng có trong ngày đầu tháng sau, không phải gom tay |

## 2. Lựa chọn hệ thống

### 2.1 Phạm vi

| Trong phạm vi | Ngoài phạm vi (giai đoạn sau) |
|---|---|
| 20 chức năng của BFD: hồ sơ, danh mục, lớp, lịch, đăng ký, chuyển lớp/bảo lưu, học phí, phiếu thu, công nợ, điểm danh, điểm, tổng kết, chứng nhận, tài khoản, thông báo, báo cáo | Thanh toán trực tuyến, điểm danh bằng mã QR, quản lý nhiều cơ sở, ứng dụng di động riêng, gửi SMS/Zalo tự động, xuất hóa đơn điện tử, tính lương giáo viên |

### 2.2 Các phương án

| Phương án | Mô tả |
|---|---|
| A. Cải tiến cách làm hiện tại | Chuẩn hóa các file Excel/Google Sheets dùng chung, thêm công thức tính học phí |
| B. Mua phần mềm có sẵn | Thuê phần mềm quản lý trung tâm đào tạo dạng dịch vụ đám mây (SaaS), trả phí theo tháng |
| C. Tự xây dựng | Xây hệ thống web riêng theo phương án công nghệ ở Bài tập 1 (ASP.NET Core, SQL Server) |

### 2.3 Đánh giá và lựa chọn

Bài giảng (mục thiết kế phần mềm) nêu: chỉ nên mua phần mềm có sẵn khi nó đáp ứng khoảng **80%** yêu cầu, và đánh giá theo các tiêu chí chức năng, linh hoạt, dễ dùng, CSDL, công sức triển khai, bảo trì, tài liệu, nhà cung cấp, chi phí. Thang điểm 1 (kém) – 5 (tốt):

| Tiêu chí | Trọng số | A | B | C |
|---|:-:|:-:|:-:|:-:|
| Đáp ứng chức năng (20 chức năng, 16 quy tắc QT01–QT16) | 30% | 2 | 3 | 5 |
| Linh hoạt khi quy tắc thay đổi | 15% | 3 | 2 | 5 |
| Dễ dùng | 10% | 3 | 4 | 4 |
| Toàn vẹn và bảo mật dữ liệu | 15% | 1 | 4 | 4 |
| Công sức và thời gian triển khai | 10% | 5 | 4 | 2 |
| Chi phí ban đầu | 10% | 5 | 4 | 2 |
| Chi phí vận hành lâu dài, phụ thuộc nhà cung cấp | 10% | 4 | 2 | 4 |
| **Điểm có trọng số** | | **2,90** | **3,20** | **4,05** |

Ước tính phương án B đáp ứng khoảng 65–70% yêu cầu: các sản phẩm phổ biến có sẵn ghi danh, thu phí, điểm danh, nhưng không có đúng các quy tắc ưu đãi cộng dồn có trần, chuyển lớp/bảo lưu, xét đạt theo chuyên cần (QT01–QT04, QT08–QT10, QT15). Mức này dưới ngưỡng 80% nên **chọn phương án C – tự xây dựng**. Phương án B vẫn là phương án dự phòng nếu dự án C chậm tiến độ quá nhiều.

## 3. Tính khả thi

### 3.1 Khả thi kinh tế

**Chi phí hữu hình (ước tính):**

| Khoản | Một lần (triệu đồng) | Hằng năm (triệu đồng) |
|---|:-:|:-:|
| Phát triển phần mềm (tương đương thuê 2 lập trình viên × 3 tháng) | 180 | – |
| Chuyển đổi dữ liệu từ Excel sang CSDL | 10 | – |
| Huấn luyện người sử dụng (3 mức: nhận thức máy tính, nhận thức hệ thống, kỹ xảo) | 10 | – |
| Máy chủ đám mây (4 vCPU, 8 GB RAM, như BT1) | – | 12 |
| Tên miền, chứng chỉ SSL, dịch vụ email | – | 3 |
| Bảo trì, hỗ trợ (khoảng 15% chi phí phát triển) | – | 27 |
| **Cộng** | **200** | **42** |

Phần cứng người dùng (12 máy tính, máy in, wifi) đã có sẵn, không tính thêm. SQL Server bản Express và ASP.NET Core miễn phí.

**Chi phí vô hình:** năng suất giảm trong khoảng 1 tháng chuyển đổi (nhân viên vừa học vừa làm); rủi ro sai dữ liệu khi nhập lại từ Excel; thời gian của các trưởng bộ phận cho phỏng vấn, JAD và nghiệm thu.

**Lợi ích đo đếm được (ước tính):**

| Lợi ích | Cách tính | Triệu đồng/năm |
|---|---|:-:|
| Giảm thất thu học phí | Doanh thu khoảng 9.600 (500 HV × 4,8 triệu × 4 kỳ) × 0,5% thất thu do áp sai ưu đãi, quên nhắc nợ | 48 |
| Tiết kiệm công sức văn phòng | Khoảng 2 nhân viên × 25% thời gian ghi danh, đối chiếu, gom báo cáo × 8 triệu/tháng × 12 | 48 |
| **Cộng** | | **96** |

**Lợi ích không đo đếm được:** phụ huynh được thông báo kịp thời nên hài lòng hơn; hình ảnh chuyên nghiệp; giám đốc ra quyết định dựa trên số liệu đúng hạn (đặc trưng MIS); dữ liệu tập trung làm nền cho các tính năng sau (thanh toán trực tuyến, CRM).

**Đánh giá:** lợi ích ròng = 96 − 42 = 54 triệu/năm, thời gian hoàn vốn khoảng 200 / 54 ≈ **3,7 năm**. Nếu tính thêm việc tăng tỷ lệ học viên học tiếp chỉ 1% doanh thu (≈ 96 triệu/năm, nhờ chăm sóc tốt hơn) thì thời gian hoàn vốn còn khoảng **1,3 năm**. Dự án **khả thi về kinh tế**, nhất là khi tính lợi ích giữ chân học viên – cũng chính là mục tiêu chiến lược của trung tâm.

### 3.2 Khả thi kỹ thuật

- Công nghệ đã chọn ở BT1 (ASP.NET Core Web API, SQL Server, HTML/Bootstrap hoặc React) là công nghệ phổ biến, có nhiều tài liệu; nhóm đã học C# và SQL.
- Quy mô nhỏ: khoảng 750 người dùng, 50–100 người truy cập đồng thời giờ cao điểm, khoảng 20 bảng dữ liệu; một máy chủ 4 vCPU/8 GB là đủ.
- Hạ tầng hiện có (máy tính, mạng) dùng được ngay; giáo viên và phụ huynh dùng trình duyệt trên điện thoại, không cần cài ứng dụng.
- Điểm khó về kỹ thuật: kiểm tra trùng lịch (QT07) và tính học phí nhiều đợt, nhiều ưu đãi. Cả hai đã được đặc tả rõ bằng ngôn ngữ cấu trúc và bảng quyết định ở giai đoạn phân tích.
- Gửi SMS/Zalo cần tích hợp dịch vụ bên ngoài nên bản đầu chỉ dùng thông báo trong hệ thống và email.

→ **Khả thi về kỹ thuật.**

### 3.3 Khả thi tác nghiệp

- Người dùng muốn có hệ thống: theo phiếu điều tra giả định, 85% muốn tra cứu học phí trực tuyến và 93% phụ huynh muốn nhận thông báo khi con vắng học; giáo viên muốn điểm danh bằng điện thoại.
- Có thể gặp phản kháng từ kế toán (đã quen Excel) và nhân viên lớn tuổi. Biện pháp: hệ thống xuất được Excel; huấn luyện theo 3 mức; chuyển đổi **theo giai đoạn** (danh mục → lớp/lịch → học phí → học tập) để từng bộ phận làm quen dần.
- Quy trình nghiệp vụ hầu như giữ nguyên; chỉ thêm 2 quy định đã thống nhất trong buổi phỏng vấn nhóm (đăng ký có hiệu lực sau khi đóng đợt 1; nhắc nợ quá hạn qua thông báo).

→ **Khả thi về tác nghiệp**, với điều kiện có huấn luyện và chuyển đổi dần.

### 3.4 Khả thi pháp lý

- **Dữ liệu cá nhân, nhất là của trẻ vị thành niên:** hệ thống lưu họ tên, ngày sinh, liên hệ, kết quả học tập của học viên dưới 18 tuổi. Phải tuân thủ quy định hiện hành về bảo vệ dữ liệu cá nhân (Nghị định 13/2023/NĐ-CP và các văn bản thay thế, bổ sung). Biện pháp:
  - phiếu đăng ký có mục phụ huynh đồng ý cho xử lý dữ liệu của con;
  - phân quyền theo vai trò, phụ huynh chỉ xem được thông tin của con mình;
  - mật khẩu được mã hóa, kết nối HTTPS, có nhật ký truy cập;
  - học viên nghỉ quá thời hạn lưu trữ thì xóa hoặc ẩn danh dữ liệu.
- **Chứng từ thu tiền:** phiếu thu là chứng từ nội bộ, phải có đủ nội dung của chứng từ kế toán (số, ngày, người nộp, số tiền bằng số và bằng chữ, người thu). Việc xuất hóa đơn điện tử theo quy định thuế nằm ngoài phạm vi và tiếp tục làm trên phần mềm hóa đơn hiện có của kế toán.
- **Bản quyền phần mềm:** dùng công cụ miễn phí hoặc mã nguồn mở (ASP.NET Core, SQL Server Express, Bootstrap); mã nguồn hệ thống thuộc về trung tâm.

→ **Khả thi về pháp lý** nếu thực hiện đủ các biện pháp trên; chúng được đưa vào yêu cầu phi chức năng và thiết kế phân quyền (3.5).

### 3.5 Khả thi chính trị (về mặt tổ chức)

- Giám đốc là người đề xuất và bảo trợ dự án; các trưởng bộ phận đã tham gia phỏng vấn nhóm và buổi JAD về học phí.
- Hệ thống làm thay đổi quyền nắm thông tin: số liệu học phí không còn chỉ nằm ở kế toán, lịch lớp không còn chỉ nằm ở phòng đào tạo. Cần phân quyền rõ ràng (ai được xem, ai được sửa) và để giám đốc chốt các quy tắc nghiệp vụ, tránh tranh chấp giữa các bộ phận.

→ **Khả thi**, với điều kiện giám đốc tiếp tục bảo trợ dự án.

### 3.6 Kết luận khả thi

| Khía cạnh | Kết luận | Điều kiện |
|---|---|---|
| Kinh tế | Khả thi | Hoàn vốn 1,3–3,7 năm |
| Kỹ thuật | Khả thi | Bản đầu không tích hợp SMS/Zalo |
| Tác nghiệp | Khả thi | Huấn luyện 3 mức, chuyển đổi theo giai đoạn |
| Pháp lý | Khả thi | Có đồng ý của phụ huynh, phân quyền, mã hóa, HTTPS |
| Chính trị | Khả thi | Giám đốc bảo trợ, phân quyền rõ |

## 4. Định vị hệ thống

Theo cách phân loại trong bài giảng: hệ thống chủ yếu là **TPS** (ghi danh, thu học phí, điểm danh, nhập điểm – xử lý trực tuyến, cập nhật CSDL ngay) kết hợp **MIS** (báo cáo định kỳ và đột xuất về tuyển sinh, doanh thu, công nợ, chuyên cần, kết quả, có so sánh với kỳ trước). Hệ thống phục vụ cấp **tác nghiệp** (nhân viên, giáo viên) và cấp **chiến thuật** (giám đốc, quản lý đào tạo). Chức năng gửi thông báo và chăm sóc học viên, phụ huynh là yếu tố **CRM**.

## 5. Lập kế hoạch dự án

### 5.1 Lịch thực hiện

Tính theo tuần của học kỳ, tuần 1 bắt đầu 07/09/2026. Mốc nộp bài sẽ điều chỉnh theo lịch của giảng viên. ■ = thời gian thực hiện, ✓ = đã xong.

| Công việc | T1 | T2 | T3 | T4 | T5 | T6 | T7 | T8 | T9 | T10 | T11 | T12 | T13 | T14 | T15 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Bài tập 1 – 6 góc nhìn | ✓ | | | | | | | | | | | | | | |
| GĐ1 Xác định, lập kế hoạch | | | | ✓ | | | | | | | | | | | |
| GĐ2 Phân tích (2.1–2.5) | | ✓ | ✓ | ✓ | | | | | | | | | | | |
| 3.1–3.2 Thực thể, ERD | | | | | ■ | ■ | | | | | | | | | |
| 3.3 Chuẩn hóa | | | | | | ■ | | | | | | | | | |
| 3.4 CSDL vật lý (SQL Server) | | | | | | | ■ | | | | | | | | |
| 3.5 Module, phân quyền | | | | | | | ■ | ■ | | | | | | | |
| 3.6 Giao diện, form, report | | | | | | | | ■ | ■ | | | | | | |
| Báo cáo thiết kế | | | | | | | | | ■ | | | | | | |
| GĐ4 Demo, cài đặt, kiểm thử | | | | | | | | | | ■ | ■ | ■ | ■ | | |
| GĐ5 Bảo trì | | | | | | | | | | | | | | ■ | |
| Báo cáo tổng, thuyết trình | | | | | | | | | | | | | | ■ | ■ |

### 5.2 Nguồn lực và phân công

| Thành viên | Vai trò trong dự án | Phần chính (theo KE_HOACH.md) |
|---|---|---|
| Hoàng Văn Huynh | Trưởng nhóm, phân tích viên chính | Điều phối, sơ đồ, ERD, duyệt PR, báo cáo tổng |
| Nguyễn Gia Ân | Phân tích viên | Thu thập thông tin, DFD-1.0, DFD-2.0 |
| Vũ Văn Hùng | Phân tích viên, thiết kế CSDL | DFD-3.0, bảng quyết định học phí, CSDL vật lý |
| Nguyễn Văn Luận | Thiết kế dữ liệu | DFD-4.0, DFD-5.0, chuẩn hóa |
| Trần Thu Thủy | Thiết kế giao diện, tài liệu | Mô tả chức năng, từ điển dữ liệu, form/report |
| Trần Minh Quân | Lập trình viên | Module, phân quyền, demo, cài đặt/bảo trì |

Công cụ: GitHub (mỗi người một nhánh, mở PR vào `main`), Python để sinh sơ đồ và báo cáo, Visual Studio / VS Code, SQL Server.

### 5.3 Rủi ro và biện pháp

| Rủi ro | Khả năng | Ảnh hưởng | Biện pháp |
|---|:-:|:-:|---|
| Thiếu thời gian làm demo | Trung bình | Cao | Demo chỉ gồm các chức năng cốt lõi (đăng ký, phiếu thu, điểm danh, nhập điểm, 2–3 báo cáo); nếu cần thì chỉ làm CSDL + truy vấn |
| Các phần làm riêng không khớp nhau (DFD, ERD, CSDL) | Trung bình | Cao | Dùng chung mã chức năng, mã quy tắc; script tự kiểm tra cân bằng DFD; đối chiếu DFD ↔ ERD ↔ CSDL sau mỗi giai đoạn |
| Yêu cầu của giảng viên thay đổi | Trung bình | Trung bình | Báo cáo dựng tự động từ .md nên sửa nhanh |
| Thành viên không tham gia đều | Thấp | Trung bình | Phân công rõ ràng, theo dõi qua PR trên GitHub |
| Dữ liệu giả định không thực tế | Thấp | Thấp | Ghi rõ "giả định"; có thể thay bằng số liệu khảo sát thật mà không đổi cấu trúc |
