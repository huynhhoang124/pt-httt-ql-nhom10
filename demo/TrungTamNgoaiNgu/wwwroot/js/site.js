// Quy ước chung của mọi màn hình (06_giao_dien mục 5.2, 6) – giao diện v3. Viết thuần, không thư viện ngoài.
// Giữ nguyên: Ctrl+S = Lưu, Esc = Thoát, F1 = Trợ giúp, câu báo lỗi tiếng Việt, "Có mặt hết", xóa nháp khi đăng xuất.
// Thêm: thanh bên thu gọn / ngăn kéo, Ctrl+K tìm nhanh chức năng, toast, hộp xác nhận, ô tìm học viên có gợi ý, chế độ tối.
(() => {
    const $ = (s, g = document) => g.querySelector(s);
    const $$ = (s, g = document) => [...g.querySelectorAll(s)];
    const thay = el => el && el.offsetParent !== null && !el.disabled;
    const luuMay = (k, v) => { try { v == null ? localStorage.removeItem(k) : localStorage.setItem(k, v); } catch { } };
    const docMay = k => { try { return localStorage.getItem(k); } catch { return null; } };
    const boDau = s => (s || '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D').toLowerCase();
    // dữ liệu (tên học viên, tên màn hình) đưa vào innerHTML phải thoát ký tự HTML
    const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]);
    const iconUrl = () => '';   // bộ biểu tượng nhúng sẵn trong trang (_Layout), gọi bằng #ten
    const ic = (ten, lop = '') => `<svg class="ic ${lop}" aria-hidden="true"><use href="${iconUrl()}#${ten}"/></svg>`;
    const coHopMo = () => $('dialog[open]') || $('.app[data-ngan-keo-mo]');

    /* ---------- Chế độ sáng / tối ---------- */
    document.addEventListener('click', e => {
        const b = e.target.closest('[data-doi-giao-dien]');
        if (!b) return;
        const toi = document.documentElement.getAttribute('data-bs-theme') !== 'dark';
        document.documentElement.setAttribute('data-bs-theme', toi ? 'dark' : 'light');
        luuMay('ttnn:giao-dien', toi ? 'dark' : 'light');
    });

    /* ---------- Thanh bên: thu gọn (máy tính), ngăn kéo (điện thoại) ---------- */
    const app = () => $('.app');
    if (docMay('ttnn:thu-gon') === '1') app()?.setAttribute('data-thu-gon', '');
    function dongNganKeo() { const a = app(); if (a && a.hasAttribute('data-ngan-keo-mo')) { a.removeAttribute('data-ngan-keo-mo'); $('[data-mo-ngan-keo]')?.focus(); } }
    document.addEventListener('click', e => {
        const t = e.target;
        if (t.closest('[data-thu-gon-ben]')) {
            const a = app(), gon = !a.hasAttribute('data-thu-gon');
            a.toggleAttribute('data-thu-gon', gon);
            luuMay('ttnn:thu-gon', gon ? '1' : null);
        } else if (t.closest('[data-mo-ngan-keo]')) {
            app().setAttribute('data-ngan-keo-mo', '');
            $('.thanh-ben .td-muc, .thanh-ben summary')?.focus();
        } else if (t.closest('.man-che, [data-dong-ngan-keo]')) {
            dongNganKeo();
        } else if (t.closest('.app[data-thu-gon] .thanh-ben .td-nhom > summary')) {
            // đang thu gọn mà bấm biểu tượng phân hệ: mở rộng thanh bên để thấy danh sách màn hình
            app().removeAttribute('data-thu-gon');
            luuMay('ttnn:thu-gon', null);
        }
        // đóng menu người dùng khi bấm ra ngoài
        $$('details.nguoi-dung[open]').forEach(d => { if (!d.contains(t)) d.open = false; });
    });

    /* ---------- Trợ giúp (nút "?" và F1): mở/đóng hộp trợ giúp của màn hình ---------- */
    function moTroGiup() {
        const tg = document.getElementById('tro-giup');
        if (!tg) return;
        tg.hidden = !tg.hidden;
        $$('[data-tro-giup]').forEach(b => b.setAttribute('aria-expanded', String(!tg.hidden)));
        if (tg.hidden) return;
        const y = tg.getBoundingClientRect().top + window.scrollY - 80;
        if (y < window.scrollY) window.scrollTo({ top: y, behavior: 'smooth' });
        tg.classList.remove('noi-bat'); void tg.offsetWidth; tg.classList.add('noi-bat');
        tg.focus({ preventScroll: true });
    }
    document.addEventListener('click', e => { if (e.target.closest('[data-tro-giup]')) moTroGiup(); });

    /* ---------- Phím tắt ---------- */
    document.addEventListener('keydown', e => {
        const k = e.key.toLowerCase();
        if ((e.ctrlKey || e.metaKey) && k === 's') {
            e.preventDefault();
            if (coHopMo()) return;
            const nut = $$('[data-phim="luu"]').find(thay);
            if (nut) nut.click();
        } else if ((e.ctrlKey || e.metaKey) && k === 'k') {
            e.preventDefault();
            moTimNhanh();
        } else if (e.key === 'Escape' && !e.defaultPrevented) {
            if ($('dialog[open]')) return;                 // hộp thoại tự đóng bằng Esc
            if ($('.app[data-ngan-keo-mo]')) { dongNganKeo(); return; }
            const goiY = $('.goi-y:not([hidden])');
            if (goiY) { goiY.hidden = true; return; }
            const thoat = $$('[data-phim="thoat"]').find(thay);
            if (thoat) location.href = thoat.href;
        } else if (e.key === 'F1') {
            e.preventDefault();
            moTroGiup();
        }
    });

    /* ---------- Ctrl+K: tìm nhanh chức năng (lọc danh sách màn hình trên trình duyệt) ---------- */
    let tn = null;
    function dungTimNhanh() {
        const muc = $$('.thanh-ben a.td-con').map(a => ({
            ma: a.querySelector('.ma')?.textContent.trim() || '',
            ten: a.querySelector('.ma + span')?.textContent.trim() || a.textContent.trim(),
            nhom: a.closest('.td-nhom')?.querySelector('.td-nhan')?.textContent.trim() || '',
            url: a.getAttribute('href'),
        }));
        const d = document.createElement('dialog');
        d.className = 'tim-nhanh';
        d.setAttribute('aria-label', 'Tìm nhanh chức năng');
        d.innerHTML = `<div class="tn-o">${ic('search', 'ic-lon')}<input type="search" placeholder="Gõ mã hoặc tên màn hình, vd F3.2, phiếu thu…" aria-label="Tìm chức năng" role="combobox" aria-expanded="true" aria-controls="tn-ds" autocomplete="off"><span class="kbd">Esc</span></div>
            <ul class="tn-ds" id="tn-ds" role="listbox"></ul>
            <div class="tn-chan"><span><span class="kbd">↑</span><span class="kbd">↓</span> chọn</span><span><span class="kbd">Enter</span> mở</span><span><span class="kbd">Esc</span> đóng</span></div>`;
        document.body.append(d);
        const o = $('input', d), ds = $('.tn-ds', d);
        let chon = 0, kq = [];
        const ve = () => {
            const q = boDau(o.value.trim());
            kq = muc.filter(m => !q || boDau(m.ma + ' ' + m.ten + ' ' + m.nhom).includes(q));
            chon = Math.min(chon, Math.max(kq.length - 1, 0));
            if (!kq.length) { ds.innerHTML = `<li class="tn-trong">Không có chức năng nào khớp “${esc(o.value)}”.</li>`; return; }
            let nhom = '', html = '';
            kq.forEach((m, i) => {
                if (m.nhom !== nhom) { nhom = m.nhom; html += `<li class="tn-nhom" role="presentation">${esc(nhom)}</li>`; }
                html += `<li class="tn-muc" role="option" id="tn-${i}" aria-selected="${i === chon}"><a href="${esc(m.url)}" tabindex="-1"><span class="chip-ma">${esc(m.ma)}</span><span>${esc(m.ten)}</span>${ic('arrow-right', 'ic-nho mui')}</a></li>`;
            });
            ds.innerHTML = html;
            o.setAttribute('aria-activedescendant', 'tn-' + chon);
            const el = $('#tn-' + chon, ds);
            if (el) { const r = el.offsetTop - ds.clientHeight + el.offsetHeight + 8; if (el.offsetTop < ds.scrollTop) ds.scrollTop = el.offsetTop - 30; else if (r > ds.scrollTop) ds.scrollTop = r; }
        };
        o.addEventListener('input', () => { chon = 0; ve(); });
        o.addEventListener('keydown', e => {
            if (e.key === 'ArrowDown') { e.preventDefault(); chon = (chon + 1) % Math.max(kq.length, 1); ve(); }
            else if (e.key === 'ArrowUp') { e.preventDefault(); chon = (chon - 1 + kq.length) % Math.max(kq.length, 1); ve(); }
            else if (e.key === 'Enter' && kq[chon]) { e.preventDefault(); location.href = kq[chon].url; }
        });
        d.addEventListener('click', e => { if (e.target === d) d.close(); });
        d.ve = ve; d.o = o;
        return d;
    }
    function moTimNhanh() {
        if ($('dialog[open]')) return;
        tn = tn || dungTimNhanh();
        tn.o.value = ''; tn.ve(); tn.showModal(); tn.o.focus();
    }
    document.addEventListener('click', e => { if (e.target.closest('[data-tim-nhanh]')) moTimNhanh(); });

    /* ---------- Toast: thongBao('Đã lưu phiếu', 'PT2027-0001', 'thanh-cong' | 'loi' | 'cb' | 'tt') ---------- */
    const BT = { 'thanh-cong': 'circle-check', loi: 'circle-alert', cb: 'triangle-alert', tt: 'info' };
    window.thongBao = (tieuDe, noiDung = '', loai = 'thanh-cong', ms = 5000) => {
        let khay = $('.khay-toast');
        if (!khay) { khay = document.createElement('div'); khay.className = 'khay-toast'; khay.setAttribute('aria-live', 'polite'); document.body.append(khay); }
        const t = document.createElement('div');
        t.className = 'toast-tb ' + loai;
        t.setAttribute('role', loai === 'loi' ? 'alert' : 'status');
        t.innerHTML = `${ic(BT[loai] || 'info', 'ic-lon')}<div class="noi"><b></b>${noiDung ? '<p></p>' : ''}</div><button type="button" class="btn btn-nhe btn-sm btn-bieu-tuong" aria-label="Đóng thông báo">${ic('x', 'ic-nho')}</button>`;
        t.querySelector('b').textContent = tieuDe;
        if (noiDung) t.querySelector('p').textContent = noiDung;
        const bo = () => { t.classList.add('ra'); setTimeout(() => t.remove(), 200); };
        t.querySelector('button').onclick = bo;
        khay.append(t);
        if (ms) setTimeout(bo, ms);
        return t;
    };
    // máy chủ đặt <div hidden data-toast="thanh-cong" data-tieu-de="…">nội dung</div> sau khi lưu
    const toastTuMayChu = () => $$('[data-toast]').forEach(el => { thongBao(el.dataset.tieuDe || el.textContent.trim(), el.dataset.tieuDe ? el.textContent.trim() : '', el.dataset.toast || 'thanh-cong'); el.remove(); });

    /* ---------- Hộp xác nhận thay confirm(): data-xac-nhan="Nội dung" trên <form> hoặc nút ----------
       Tùy chọn: data-xac-nhan-tieu-de, data-xac-nhan-nut (chữ nút đồng ý), data-xac-nhan-loai="nguy" | "chinh" */
    window.xacNhan = ({ tieuDe = 'Xác nhận', noiDung = '', nut = 'Đồng ý', loai = '' } = {}) => new Promise(xong => {
        const d = document.createElement('dialog');
        d.className = 'hop-thoai ' + loai;
        d.setAttribute('aria-labelledby', 'ht-td'); d.setAttribute('aria-describedby', 'ht-nd');
        d.innerHTML = `<div class="ht-than"><div class="ht-bieu-tuong">${ic(loai === 'chinh' ? 'info' : 'triangle-alert', 'ic-lon')}</div><div><h2 class="ht-tieu-de" id="ht-td"></h2><p class="ht-noi-dung" id="ht-nd"></p></div></div>
            <div class="ht-lenh"><button type="button" class="btn btn-secondary" value="khong">Hủy</button><button type="button" class="btn ${loai === 'nguy' ? 'btn-danger' : 'btn-primary'}" value="co"></button></div>`;
        $('#ht-td', d).textContent = tieuDe; $('#ht-nd', d).textContent = noiDung; $('[value=co]', d).textContent = nut;
        document.body.append(d);
        let kq = false;
        d.addEventListener('click', e => { const b = e.target.closest('button'); if (b) { kq = b.value === 'co'; d.close(); } else if (e.target === d) d.close(); });
        d.addEventListener('close', () => { d.remove(); xong(kq); });
        d.showModal();
        $('[value=co]', d).focus();
    });
    const hoi = el => xacNhan({ tieuDe: el.dataset.xacNhanTieuDe || 'Xác nhận', noiDung: el.dataset.xacNhan, nut: el.dataset.xacNhanNut || 'Đồng ý', loai: el.dataset.xacNhanLoai || '' });
    document.addEventListener('submit', e => {
        const f = e.target, nut = e.submitter;
        // data-xac-nhan rỗng (nút Lưu chung của _NutLenh khi trang không cần hỏi) = không hỏi
        const nguon = nut?.dataset.xacNhan ? nut : f.dataset.xacNhan ? f : null;
        if (!nguon || f.dataset.daXacNhan) { delete f.dataset.daXacNhan; return; }
        e.preventDefault();
        e.stopImmediatePropagation();
        hoi(nguon).then(co => { if (co) { f.dataset.daXacNhan = '1'; f.requestSubmit(nut || undefined); } });
    }, true);
    document.addEventListener('click', e => {   // liên kết <a data-xac-nhan>
        const a = e.target.closest('a[data-xac-nhan]');
        if (!a) return;
        e.preventDefault();
        hoi(a).then(co => { if (co) location.href = a.href; });
    });

    /* ---------- Trường có dấu * (nhãn .bat-buoc) là bắt buộc ---------- */
    const batBuoc = () => $$('label.bat-buoc').forEach(l => { if (l.control) l.control.required = true; });

    /* ---------- Câu báo lỗi nhập liệu bằng tiếng Việt ---------- */
    const tenTruong = o => ((o.labels && o.labels[0] && o.labels[0].innerText) || o.getAttribute('aria-label') || 'trường này').replace(/\s*\*\s*$/, '').trim();
    document.addEventListener('invalid', e => {
        const o = e.target, v = o.validity, ten = tenTruong(o);
        let cau = '';
        if (v.valueMissing) cau = (o.tagName === 'SELECT' || o.type === 'radio' || o.type === 'checkbox' ? 'Chọn ' : 'Nhập ') + ten.charAt(0).toLowerCase() + ten.slice(1) + '.';
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

    /* ---------- F4.1: "Có mặt hết (người chưa chọn)" ---------- */
    document.addEventListener('click', e => {
        const b = e.target.closest('[data-co-mat-het]');
        if (!b) return;
        const nhom = new Set($$('input[type=radio][name^="TrangThai["]', b.form).map(r => r.name));
        let n = 0;
        nhom.forEach(ten => {
            const ds = $$(`input[type=radio][name="${CSS.escape(ten)}"]`, b.form);
            if (!ds.some(r => r.checked)) { const x = ds.find(r => r.value === 'x'); if (x) { x.checked = true; n++; } }
        });
        b.form.dispatchEvent(new Event('change', { bubbles: true }));
        thongBao(n ? `Đã đánh có mặt cho ${n} học viên` : 'Mọi học viên đều đã có trạng thái', n ? 'Bấm Lưu (Ctrl+S) để ghi điểm danh.' : '', n ? 'thanh-cong' : 'tt', 3500);
    });

    /* ---------- Ô tìm học viên: thay <datalist id="ds-hoc-vien"> bằng danh sách gợi ý theo mã / tên / điện thoại ---------- */
    function oTimHocVien(o) {
        const dl = document.getElementById(o.getAttribute('list'));
        if (!dl || o.dataset.daDung) return;
        o.dataset.daDung = '1';
        const ds = $$('option', dl).map(op => { const [ten, dt] = (op.textContent || '').split(' · '); return { ma: op.value, ten: (ten || '').trim(), dt: (dt || '').trim() }; });
        o.removeAttribute('list');
        o.setAttribute('role', 'combobox'); o.setAttribute('aria-autocomplete', 'list'); o.setAttribute('aria-expanded', 'false');
        const ul = document.createElement('ul');
        ul.className = 'goi-y'; ul.id = o.id + '-goi-y'; ul.hidden = true; ul.setAttribute('role', 'listbox');
        o.setAttribute('aria-controls', ul.id);
        o.parentElement.classList.add('tim-hv');
        o.after(ul);
        let kq = [], chon = 0;
        const to = (s, q) => { const i = q ? boDau(s).indexOf(q) : -1; return i < 0 ? esc(s) : esc(s.slice(0, i)) + '<mark>' + esc(s.slice(i, i + q.length)) + '</mark>' + esc(s.slice(i + q.length)); };
        const ve = () => {
            const q = boDau(o.value.trim());
            if (q.length < 2) { ul.hidden = true; o.setAttribute('aria-expanded', 'false'); return; }
            kq = ds.filter(h => boDau(h.ma + ' ' + h.ten + ' ' + h.dt).includes(q)).slice(0, 8);
            chon = 0;
            ul.innerHTML = kq.length ? kq.map((h, i) => `<li role="option" id="${ul.id}-${i}" aria-selected="${i === chon}" data-ma="${esc(h.ma)}"><span class="ma">${to(h.ma, q)}</span><span>${to(h.ten, q)}</span><span class="phu">${to(h.dt, q)}</span></li>`).join('')
                : '<li class="trong-goi-y">Không tìm thấy học viên. Thử mã HV, họ tên hoặc số điện thoại.</li>';
            ul.hidden = false; o.setAttribute('aria-expanded', 'true');
        };
        const lay = ma => { o.value = ma; ul.hidden = true; o.setAttribute('aria-expanded', 'false'); if (o.form && o.form.method.toLowerCase() === 'get') o.form.requestSubmit(); };
        const doi = i => { chon = (i + kq.length) % kq.length; $$('li', ul).forEach((li, j) => li.setAttribute('aria-selected', String(j === chon))); o.setAttribute('aria-activedescendant', `${ul.id}-${chon}`); };
        o.addEventListener('input', ve);
        o.addEventListener('keydown', e => {
            if (ul.hidden || !kq.length) return;
            if (e.key === 'ArrowDown') { e.preventDefault(); doi(chon + 1); }
            else if (e.key === 'ArrowUp') { e.preventDefault(); doi(chon - 1); }
            else if (e.key === 'Enter') { e.preventDefault(); lay(kq[chon].ma); }
        });
        ul.addEventListener('mousedown', e => { const li = e.target.closest('li[data-ma]'); if (li) { e.preventDefault(); lay(li.dataset.ma); } });
        o.addEventListener('blur', () => setTimeout(() => { ul.hidden = true; }, 120));
    }

    /* ---------- Đăng xuất: xóa nháp (N6) của người dùng này trên máy ---------- */
    document.addEventListener('submit', e => {
        const dx = e.target.closest('form[data-dang-xuat]');
        if (!dx) return;
        try { const tien = 'nhap:' + dx.dataset.dangXuat + ':'; Object.keys(localStorage).filter(k => k.startsWith(tien)).forEach(k => localStorage.removeItem(k)); } catch { }
    });

    /* ---------- Hiện mật khẩu (trang đăng nhập) ---------- */
    document.addEventListener('click', e => {
        const b = e.target.closest('[data-hien-mat-khau]');
        if (!b) return;
        const o = b.parentElement.querySelector('input');
        const hien = o.type === 'password';
        o.type = hien ? 'text' : 'password';
        b.setAttribute('aria-pressed', String(hien));
        b.setAttribute('aria-label', hien ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
    });

    /* ---------- Bảng → thẻ trên điện thoại (site.css: .bang-the, ≤ 640px): gắn data-nhan theo tiêu đề cột,
       ô chữ đầu tiên làm đầu thẻ. Bỏ qua chứng từ in (.lien) và bảng có class khong-the. ---------- */
    const bangThe = () => $$('table.table').forEach(t => {
        if (t.closest('.lien') || t.classList.contains('khong-the') || !t.tHead) return;
        const hang = t.tHead.rows[t.tHead.rows.length - 1];
        const ten = [...hang.cells].flatMap(c => Array(c.colSpan).fill(c.innerText.trim()));
        t.classList.add('bang-the');
        [...t.tBodies].flatMap(b => [...b.rows]).forEach(r => {
            let i = 0, coDau = false;
            [...r.cells].forEach(c => {
                const n = ten[i]; i += c.colSpan;
                if (c.colSpan > 1 || c.hasAttribute('data-nhan')) return;
                if (!coDau && n && c.innerText.trim() && !c.querySelector('input, select, textarea')) { c.classList.add('dau-the'); coDau = true; }
                else if (n) c.dataset.nhan = n;
            });
        });
    });

    const khoiDong = () => { batBuoc(); bangThe(); $$('input[list="ds-hoc-vien"]').forEach(oTimHocVien); toastTuMayChu(); };
    document.readyState === 'loading' ? document.addEventListener('DOMContentLoaded', khoiDong) : khoiDong();
})();
