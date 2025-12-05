var notif = {
    loadNotif: function (page = 1, pageSize = 6) {
        $.ajax({
            url: '/Employee/Notification/loadNotification',
            type: 'POST',
            data: { page, pageSize },
            success: function (result) {
                const list = document.getElementById('notificationList');

                if (page === 1) list.innerHTML = "";

                if (result.length === 0 && page === 1) {
                    list.innerHTML = `
                        <div class="text-center py-5">
                            <i class="fas fa-bell-slash text-muted fa-3x mb-3 opacity-25"></i>
                            <p class="text-muted">Không có thông báo nào.</p>
                        </div>
                    `;
                    const btnMore = document.getElementById('btnLoadMoreNoti');
                    if (btnMore) btnMore.style.display = 'none';
                    return;
                }

                result.forEach(n => {
                    let iconClass = 'fa-bell';
                    let bgClass = 'bg-light text-secondary';

                    switch (n.type) {
                        case 0:
                            iconClass = 'fa-file-signature';
                            bgClass = 'bg-primary bg-opacity-10 text-primary';
                            break;
                        case 1:
                            iconClass = 'fa-user-pen';
                            bgClass = 'bg-info bg-opacity-10 text-info';
                            break;
                        case 2:
                            iconClass = 'fa-clock';
                            bgClass = 'bg-warning bg-opacity-10 text-warning';
                            break;
                        case 3:
                            iconClass = 'fa-check-circle';
                            bgClass = 'bg-success bg-opacity-10 text-success';
                            break;
                        case 4:
                            iconClass = 'fa-times-circle';
                            bgClass = 'bg-danger bg-opacity-10 text-danger';
                            break;
                    }

                    const unreadClass = !n.isRead ? 'bg-light fw-semibold' : 'bg-white';
                    const dotHtml = !n.isRead ? '<span class="badge bg-danger rounded-pill p-1 ms-2" style="width:8px; height:8px;"> </span>' : '';

                    const li = document.createElement('li');
                    li.className = `list-group-item list-group-item-action p-0 border-bottom ${unreadClass}`;
                    li.id = `notif-${n.notificationId}`;

                    li.innerHTML = `
                        <a href="${n.url || '#'}" 
                           class="text-decoration-none text-dark d-flex align-items-start p-3 w-100 h-100">
                            <div class="flex-shrink-0 me-3">
                                <div class="rounded-circle d-flex align-items-center justify-content-center ${bgClass}" style="width: 48px; height: 48px;">
                                    <i class="fas ${iconClass} fs-5"></i>
                                </div>
                            </div>
                            <div class="flex-grow-1" style="min-width: 0;">
                                <div class="d-flex justify-content-between align-items-center mb-1">
                                    <h6 class="mb-0 text-truncate pe-2" style="font-size: 0.95rem;">${n.title}</h6>
                                    <small class="text-muted" style="font-size: 0.75rem; white-space: nowrap;">
                                        ${notif.timeAgo(new Date(n.createdAt))}
                                    </small>
                                </div>
                                <p class="mb-0 text-secondary text-truncate small" style="max-width: 90%;">
                                    ${n.content ?? "Không có nội dung"}
                                </p>
                            </div>
                            <div class="flex-shrink-0 d-flex align-items-center mt-1">
                                ${dotHtml}
                            </div>
                        </a>
                    `;
                    list.appendChild(li);
                });

                const btnLoadMore = document.getElementById('btnLoadMoreNoti');
                if (btnLoadMore) {
                    if (result.length < pageSize) {
                        btnLoadMore.style.display = 'none';
                    } else {
                        btnLoadMore.style.display = 'block';
                    }
                }
            },
            error: function () {
                toastr.error('Không thể tải thông báo');
            }
        });
    },

    timeAgo: function (date) {
        const seconds = Math.floor((new Date() - date) / 1000);
        let interval = seconds / 31536000;
        if (interval > 1) return Math.floor(interval) + " năm trước";
        interval = seconds / 2592000;
        if (interval > 1) return Math.floor(interval) + " tháng trước";
        interval = seconds / 86400;
        if (interval > 1) return Math.floor(interval) + " ngày trước";
        interval = seconds / 3600;
        if (interval > 1) return Math.floor(interval) + " giờ trước";
        interval = seconds / 60;
        if (interval > 1) return Math.floor(interval) + " phút trước";
        return "Vừa xong";
    }
};


// Khởi tạo connection tới hub SignalR
const NotificationEmployee = new signalR.HubConnectionBuilder()
    .withUrl("/Notifications") // phải trùng với route MapHub trên server
    .build();

NotificationEmployee.on("NotificationEmployee", function (notif) {
    console.log("Thông báo mới:", notif);
    const notification = document.getElementById('notifDropdown');
    const notifBell = document.getElementById('notificationBell');

    if (!notification || !notifBell) return;

    // Bật container (nếu đang ẩn)
    notification.style.display = "block";

    // --- 1. CẤU HÌNH ICON & MÀU SẮC (Dùng FontAwesome cho đẹp) ---
    let iconClass = 'fa-bell';
    let bgClass = 'bg-secondary';
    let textClass = 'text-secondary';

    switch (notif.type) {
        case 0: // Task/Note
            iconClass = 'fa-file-signature';
            bgClass = 'bg-primary';
            textClass = 'text-primary';
            break;
        case 1: // Edit
            iconClass = 'fa-pen-to-square';
            bgClass = 'bg-info';
            textClass = 'text-info';
            break;
        case 2: // Warning
            iconClass = 'fa-clock';
            bgClass = 'bg-warning';
            textClass = 'text-warning';
            break;
        case 3: // Success
            iconClass = 'fa-check-circle';
            bgClass = 'bg-success';
            textClass = 'text-success';
            break;
        default: // Default
            iconClass = 'fa-bell';
            bgClass = 'bg-secondary';
            textClass = 'text-secondary';
            break;
    }

    // --- 2. TẠO HTML MỚI (ĐẸP HƠN) ---
    const notifContainer = document.createElement('div');
    notifContainer.className = 'notif-temp-item border-bottom'; // Thêm class để CSS animation

    notifContainer.innerHTML = `
        <div class="notif-item-modern d-flex p-3 align-items-start" style="cursor: default;">
            <div class="flex-shrink-0 me-3">
                <div class="rounded-circle d-flex align-items-center justify-content-center ${bgClass} bg-opacity-10" 
                     style="width: 42px; height: 42px;">
                    <i class="fas ${iconClass} ${textClass} fs-5"></i>
                </div>
            </div>

            <div class="flex-grow-1" style="min-width: 0;">
                <div class="d-flex justify-content-between align-items-center mb-1">
                    <strong class="text-dark text-truncate me-2" style="font-size: 0.95rem;">${notif.title}</strong>
                    <small class="text-muted" style="font-size: 0.7rem; white-space: nowrap;">Vừa xong</small>
                </div>
                
                <p class="mb-2 text-secondary small text-break" style="line-height: 1.4;">
                    ${notif.content}
                </p>

                <div class="text-end">
                    <button onclick="window.location.href='${notif.url || '#'}'" 
                            class="btn btn-sm btn-outline-primary btn-view-notif py-1 px-3">
                        Xem chi tiết <i class="fas fa-arrow-right ms-1"></i>
                    </button>
                </div>
            </div>
        </div>
    `;

    // Thêm vào đầu danh sách
    notification.prepend(notifContainer);

    // --- 3. CẬP NHẬT BADGE (Giữ nguyên logic của bạn) ---
    const badge = document.getElementById("notifCount");
    if (badge) {
        let current = parseInt(badge.innerText || "0");
        badge.innerText = current + 1;
        badge.style.display = "inline-block";

        // Thêm hiệu ứng rung chuông nhẹ
        notifBell.classList.add('fa-shake');
        setTimeout(() => notifBell.classList.remove('fa-shake'), 1000);
    }

    // --- 4. TỰ ĐỘNG ẨN SAU 6 GIÂY (Giữ nguyên logic của bạn) ---
    setTimeout(() => {
        // Thêm hiệu ứng mờ dần trước khi xóa
        notifContainer.style.transition = "opacity 0.5s ease";
        notifContainer.style.opacity = "0";

        setTimeout(() => {
            notifContainer.remove();

            if (badge) {
                let updated = parseInt(badge.innerText || "0") - 1;
                badge.innerText = updated > 0 ? updated : 0;
                if (updated <= 0) badge.style.display = "none";
            }
        }, 500); // Đợi 0.5s cho hiệu ứng mờ chạy xong
    }, 6000);
});
NotificationEmployee.start()
    .then(() => console.log("✅ Kết nối SignalR thành công"))
    .catch(err => console.error("❌ Kết nối lỗi:", err));
$('#notificationModal').on('shown.bs.modal', function () {
    notif.loadNotif(1);
});
