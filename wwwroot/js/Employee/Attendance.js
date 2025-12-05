// wwwroot/js/attendance.js

const AttendanceManager = {
    elements: {},

    // 1. Init
    init: function () {
        this.cacheDom();
        this.bindEvents();
        this.startClock();
        this.checkInitialState();
    },

    // 2. Cache DOM (Dùng jQuery chọn cho lẹ)
    cacheDom: function () {
        this.elements = {
            clock: $('#live-clock'),
            date: $('#live-date'),
            status: $('#attendanceStatus'),
            btnCheckIn: $('#btnCheckIn'),
            btnCheckOut: $('#btnCheckOut'),
            note: $('#attendanceNote'),
            historyList: $('#attendanceHistory'),
            emptyHistory: $('#emptyHistory'),
            wrapper: $('.card-body') // Wrapper chứa data-attribute
        };
    },

    // 3. Gán sự kiện
    bindEvents: function () {
        this.elements.btnCheckIn.on('click', () => this.submitAttendance('in'));
        this.elements.btnCheckOut.on('click', () => this.submitAttendance('out'));
    },

    // 4. Hàm xử lý chung cho cả In và Out (Code gọn hơn 50%)
    submitAttendance: function (type) {
        const noteVal = this.elements.note.val();
        const btn = type === 'in' ? this.elements.btnCheckIn : this.elements.btnCheckOut;
        const url = type === 'in' ? '/Employee/Employee/CheckIn' : '/Employee/Employee/CheckOut';
        const originalText = btn.html();

        // Loading
        btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang xử lý...');

        $.ajax({
            url: url,
            type: 'POST',
            data: { note: noteVal },
            success: (result) => {
                btn.prop('disabled', false).html(originalText); // Reset nút

                if (result.success) {
                    toastr.success(result.message);
                    // Tái sử dụng hàm updateUI -> Code sạch
                    this.updateUI(type, result.time, noteVal);
                } else {
                    toastr.warning(result.message);
                }
            },
            error: () => {
                btn.prop('disabled', false).html(originalText);
                toastr.error("Lỗi kết nối Server");
            }
        });
    },

    // 5. Cập nhật giao diện (Dùng chung)
    updateUI: function (type, time, note) {
        if (type === 'in') {
            this.elements.status.html(
                `<span class="badge bg-success"><i class="fas fa-check-circle me-1"></i> Đã check-in lúc ${time}</span>`
            );
            this.elements.btnCheckIn.hide();
            this.elements.btnCheckOut.fadeIn();
        } else {
            this.elements.status.html(
                `<span class="badge bg-primary"><i class="fas fa-flag-checkered me-1"></i> Đã hoàn thành lúc ${time}</span>`
            );
            this.elements.btnCheckOut.hide();
            this.elements.note.prop('disabled', true).attr('placeholder', 'Đã kết thúc ngày làm việc.');
        }

        this.elements.note.val(''); // Xóa ghi chú cũ
        this.addHistoryItem(type, time, note);
    },

    // 6. Thêm lịch sử
    addHistoryItem: function (type, time, note) {
        this.elements.emptyHistory.hide();

        const iconClass = type === 'in' ? 'fa-sign-in-alt text-success' : 'fa-sign-out-alt text-warning';
        const title = type === 'in' ? 'Check-in (Vào)' : 'Check-out (Ra)';

        const itemHtml = `
            <li class="list-group-item d-flex justify-content-between align-items-start animate__animated animate__fadeInDown">
                <div>
                    <i class="fas ${iconClass} me-2"></i>
                    <strong>${title}</strong>
                    ${note ? `<div class="small text-muted fst-italic">Ghi chú: ${note}</div>` : ''}
                </div>
                <span class="small text-muted">${time}</span>
            </li>`;

        this.elements.historyList.prepend(itemHtml); // Chèn lên đầu
    },

    // 7. Đồng hồ
    startClock: function () {
        const update = () => {
            const now = new Date();
            this.elements.clock.text(now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false }));
            this.elements.date.text(now.toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' }));
        };
        update();
        setInterval(update, 1000);
    },

    // 8. Check trạng thái ban đầu
    checkInitialState: function () {
        const hasCheckIn = this.elements.wrapper.data('hasCheckIn') === 'True'; // jQuery .data() tự parse kiểu
        const hasCheckOut = this.elements.wrapper.data('hasCheckOut') === 'True';

        if (hasCheckOut) {
            this.elements.status.html('<span class="badge bg-primary">Đã hoàn thành công việc</span>');
            this.elements.btnCheckIn.hide();
            this.elements.btnCheckOut.hide();
            this.elements.note.prop('disabled', true);
        } else if (hasCheckIn) {
            this.elements.status.html('<span class="badge bg-success">Đang làm việc</span>');
            this.elements.btnCheckIn.hide();
            this.elements.btnCheckOut.show();
        }
    }
};

// Run
$(document).ready(function () {
    AttendanceManager.init();
});