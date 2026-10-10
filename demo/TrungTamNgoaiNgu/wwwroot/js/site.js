// Quy ước chung của mọi màn hình (06_giao_dien mục 5.2, 6). Không thư viện ngoài.
(() => {
    const thay = el => el && el.offsetParent !== null && !el.disabled;

    // Phím tắt: Ctrl+S = Lưu, Esc = Thoát, F1 = Trợ giúp (Ctrl+P = In là phím của trình duyệt)
    document.addEventListener('keydown', e => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
            e.preventDefault();
            const nut = [...document.querySelectorAll('[data-phim="luu"]')].find(thay);
            if (nut) nut.click();
        } else if (e.key === 'Escape' && !e.defaultPrevented) {
            const thoat = [...document.querySelectorAll('[data-phim="thoat"]')].find(thay);
            if (thoat) location.href = thoat.href;
        } else if (e.key === 'F1') {
            e.preventDefault();
            moTroGiup();
        }
    });

    // Nút "?" và F1: đưa dòng trợ giúp của màn hình vào tầm mắt
    function moTroGiup() {
        const tg = document.getElementById('tro-giup');
        if (!tg) return;
        tg.scrollIntoView({ behavior: 'smooth', block: 'center' });
        tg.classList.remove('noi-bat');
        void tg.offsetWidth;            // chạy lại hiệu ứng khi bấm nhiều lần
        tg.classList.add('noi-bat');
        tg.focus({ preventScroll: true });
    }
    document.querySelectorAll('[data-tro-giup]').forEach(b => b.addEventListener('click', moTroGiup));

    // Trường có dấu * (nhãn .bat-buoc) là bắt buộc: trình duyệt chặn ngay khi bấm Lưu, máy chủ vẫn kiểm tra lại
    document.querySelectorAll('label.bat-buoc').forEach(l => { if (l.control) l.control.required = true; });

    // Câu báo lỗi nhập liệu bằng tiếng Việt, không phụ thuộc ngôn ngữ của trình duyệt
    const tenTruong = o => ((o.labels && o.labels[0] && o.labels[0].innerText) || o.getAttribute('aria-label') || 'trường này')
        .replace(/\s*\*\s*$/, '').trim();
    document.addEventListener('invalid', e => {
        const o = e.target, v = o.validity, ten = tenTruong(o);
        let cau = '';
        if (v.valueMissing) cau = (o.tagName === 'SELECT' || o.type === 'radio' || o.type === 'checkbox' ? 'Chọn ' : 'Nhập ') + ten.toLowerCase() + '.';
        else if (v.patternMismatch) cau = o.title || ten + ' chưa đúng dạng.';
        else if (v.typeMismatch) cau = ten + (o.type === 'email' ? ' chưa đúng dạng, vd ten@gmail.com.' : ' chưa đúng dạng.');
        else if (v.badInput) cau = ten + ': chỉ nhập số.';
        else if (v.rangeUnderflow) cau = ten + ' không được nhỏ hơn ' + o.min + '.';
        else if (v.rangeOverflow) cau = ten + ' không được lớn hơn ' + o.max + '.';
        else if (v.stepMismatch) cau = ten + ': nhập bội số của ' + o.step + '.';
        else if (v.tooLong) cau = ten + ' dài quá ' + o.maxLength + ' ký tự.';
        if (cau) o.setCustomValidity(cau);
    }, true);
    const xoaLoi = e => { if (e.target.setCustomValidity) e.target.setCustomValidity(''); };
    document.addEventListener('input', xoaLoi, true);
    document.addEventListener('change', xoaLoi, true);

    // Đăng xuất: xóa nháp (N6) của người dùng này trên máy
    const dx = document.querySelector('form[data-dang-xuat]');
    if (dx) dx.addEventListener('submit', () => {
        try {
            const tien = 'nhap:' + dx.dataset.dangXuat + ':';
            Object.keys(localStorage).filter(k => k.startsWith(tien)).forEach(k => localStorage.removeItem(k));
        } catch { }
    });
})();
