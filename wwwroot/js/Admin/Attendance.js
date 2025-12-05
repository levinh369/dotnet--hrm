var currentPage = 1;
var Attendance = {
    loadData: function (pageIndex) {
        var data = $('#searchForm').serializeArray();
        if (pageIndex !== undefined) {
            // dùng pageIndex truyền vào
        } else {
            pageIndex = 1;
        }
        // Thêm pageIndex vào data (với tên "page" trùng tham số controller)
        data.push({ name: "page", value: pageIndex });
        currentPage = pageIndex;

        $.ajax({
            url: "/Admin/Attendance/ListData",
            type: "get",
            data: data,
            success: function (result) {
                $("#gridData").html(result); // Gắn HTML trả về vào div
                var statsJson = $("#statsData").val();

                if (statsJson) {
                    var stats = JSON.parse(statsJson);
                    $("#displayDate").text(new Date(stats.Date).toLocaleDateString());
                    $("#lblTotal").text(stats.TotalStaffCount);
                    $("#lblOnTime").text(stats.OnTimeCount);
                    $("#lblLate").text(stats.LateOrEarlyCount);
                    $("#lblAbsent").text(stats.AbsentCount);
                }
                var totalPages = $("#pagination").data("total-pages");
                if (!$('#paging-ul').data("twbs-pagination")) {
                    //Attendance.showPaging(totalPages, pageIndex);
                }
            },

            error: function () {
                alert("Lỗi tải dữ liệu");
            }
        });
    },
    detail: function (id) {
        $.ajax({
            url: '/Admin/Attendance/Detail',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#attendanceDetailModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }
        });

    },
    edit: function (id) {
        $.ajax({
            url: '/Admin/Attendance/Edit',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#attendanceEditModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }

        });
    },
    EditPost: function () {
        var data = $('#frmEditAttendance').serialize();
        $.ajax({
            url: "/Admin/Attendance/EditPost",
            type: "POST",
            data: data,
            success: function (res) {
                $('#attendanceEditModal').modal('hide');
                toastr.success(res.message);
                Attendance.loadData();
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }
        });
    },
}
Attendance.loadData();
$(document).on("submit", "#searchForm", function (e) {
    e.preventDefault(); // ❌ chặn reload trang
    Attendance.loadData();     // ✅ gọi Ajax load dữ liệu
});
$(document).on('submit', '#frmEditAttendance', function (e) {
    e.preventDefault(); // ngăn reload trang
    Attendance.EditPost();     // gọi AJAX
});