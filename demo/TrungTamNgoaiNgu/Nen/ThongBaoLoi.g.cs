// Sinh tự động bởi demo/sinh_ma.py từ thiet_ke/06_giao_dien.md mục 6 – không sửa tay; sửa bảng trong .md rồi chạy lại.

namespace TrungTamNgoaiNgu.Nen;

public static partial class ThongBaoLoi
{
    static readonly Dictionary<string, (string ManHinh, string ThongBao, string GoiY)> Bang = new()
    {
        ["CK_HocVien_DienThoai"] = ("F1.1", "Số điện thoại phải gồm đúng 10 chữ số.", "Bỏ khoảng trắng, dấu chấm; vd 0912345678"),
        ["CK_HocVien_KiemTra"] = ("F1.1", "Đã nhập trình độ nhưng thiếu ngày kiểm tra (hoặc ngược lại).", "Nhập đủ cả hai, hoặc để trống cả hai"),
        ["UQ_GiaoVien_Email"] = ("F1.2", "Email này đã thuộc một giáo viên khác.", "Kiểm tra trùng hồ sơ trước khi thêm mới"),
        ["CK_KhoaHoc_TrongSo"] = ("F1.3", "Tổng ba trọng số phải bằng 100%.", "Vd 10 – 30 – 60"),
        ["CK_LopHoc_SiSoToiDa"] = ("F2.1", "Sĩ số tối đa phải lớn hơn 0.", "Theo quy định, không quá 20 (QT06)"),
        ["PK_DangKy"] = ("F2.2", "Học viên đã có đăng ký ở lớp này.", "Chọn lớp khác cùng khóa còn chỗ"),
        ["CK_BuoiHoc_Gio"] = ("F2.3", "Giờ kết thúc phải sau giờ bắt đầu.", "Kiểm tra lại khung giờ"),
        ["CK_DangKy_HanBaoLuu"] = ("F2.4", "Đăng ký bảo lưu phải có hạn bảo lưu.", "Hệ thống tự điền ngày bảo lưu + 6 tháng; không xóa ô này"),
        ["CK_UuDai_Ngay"] = ("F3.1", "Ngày kết thúc ưu đãi phải sau ngày bắt đầu.", "Kiểm tra lại thời gian hiệu lực"),
        ["CK_ChiTietPhieuThu_SoTien"] = ("F3.2", "Số tiền nộp phải lớn hơn 0.", "Bỏ dòng đợt không nộp"),
        ["CK_Diem_Diem"] = ("F4.2", "Điểm phải từ 0 đến 10.", "Dùng dấu chấm cho phần lẻ, vd 7.5"),
        ["FK_DiemDanh_BuoiHoc"] = ("F4.1", "Buổi học này không thuộc lớp đang chọn.", "Chọn lại buổi trong lịch của lớp"),
        ["CK_KetQua_Dat"] = ("F4.3", "Kết quả \"Đạt\" không đi với xếp loại \"Không đạt\".", "Chạy lại tổng kết; không sửa tay kết quả"),
        ["CK_TaiKhoan_ChuSoHuu"] = ("F5.1a", "Vai trò không khớp với chủ tài khoản.", "Vai trò Giáo viên chọn mã giáo viên; Học viên chọn mã học viên…"),
        ["FK_ThongBao_GiamHo"] = ("R5.2", "Người nhận không phải phụ huynh của học viên này.", "Kiểm tra quan hệ giám hộ ở F1.1"),
        ["QT05"] = ("F3.2", "Đợt 1 phải nộp tối thiểu 50% số phải nộp.", "Nộp thêm, hoặc chọn đóng hai đợt"),
        ["QT05#2"] = ("F3.2", "Số tiền vượt số còn nợ của đợt.", "Kiểm tra lại đợt và số còn nợ"),
        ["QT06"] = ("F2.2", "Lớp đã đủ sĩ số.", "Hệ thống gợi ý lớp cùng khóa còn chỗ"),
        ["QT07"] = ("F2.3", "Trùng lịch với lớp khác (cùng giáo viên hoặc cùng phòng).", "Đổi giờ, đổi phòng hoặc đổi giáo viên"),
        ["QT08"] = ("F2.4", "Đã quá 3 buổi đầu, không chuyển lớp được.", "Xét bảo lưu hoặc nghỉ học"),
        ["QT09"] = ("F2.4", "Chưa đóng đủ học phí, không bảo lưu được.", "Đóng đủ trước khi bảo lưu"),
        ["QT11"] = ("F4.1", "Đã quá 24 giờ sau buổi học, không sửa điểm danh được.", "Liên hệ QL đào tạo"),
    };
}
