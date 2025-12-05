var currentPage = 1;
var JobApp = {
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
            url: "/Admin/JobApplication/ListData",
            type: "get",
            data: data,
            success: function (result) {
                $("#gridData").html(result); // Gắn HTML trả về vào div
                var totalPages = $("#pagination").data("total-pages");
                if (!$('#paging-ul').data("twbs-pagination")) {
                    JobApp.showPaging(totalPages, pageIndex);
                }
            },

            error: function () {
                alert("Lỗi tải dữ liệu");
            }
        });
    },
    showPaging: function (totalPages, currentPage) {
        if (totalPages > 1) {
            $('#paging-ul').twbsPagination({
                startPage: currentPage,
                totalPages: totalPages,
                visiblePages: 5,
                first: '<i class="fa fa-fast-backward"></i>',
                prev: '<i class="fa fa-step-backward"></i>',
                next: '<i class="fa fa-step-forward"></i>',
                last: '<i class="fa fa-fast-forward"></i>',
                onPageClick: function (event, page) {
                    // chỉ load khi KHÔNG phải lần khởi tạo
                    if ($('#paging-ul').data('init-complete')) {
                        JobApp.loadData(page);
                    }
                }
            });
            $('#paging-ul').data('init-complete', true); // đánh dấu đã init
        }
    },
    detail: function (id) {
        $.ajax({
            url: '/Admin/JobApplication/Detail',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#jobApplicationDetailModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }
        });

    },
    edit: function (id) {
        $.ajax({
            url: '/Admin/JobApplication/Edit',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#jobApplicationEditModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }

        });
    },
    EditPost: function () {
        var form = $('#jobApplicationEditForm')[0]; // get the raw DOM element
        var formData = new FormData(form);
        $.ajax({
            url: "/Admin/JobApplication/EditPost",
            type: "POST",
            data: formData,
            processData: false, // important for FormData
            contentType: false,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                $('#jobApplicationEditModal').modal('hide');
                toastr.success(res.message);
                JobApp.loadData();
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }
        });
    },
    remove: function (id) {
        Swal.fire({
            title: "Bạn có chắc muốn xóa?",
            text: "Thao tác này sẽ không thể hoàn tác!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Xóa",
            cancelButtonText: "Hủy"
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/Admin/Position/Delete",
                    type: "GET",
                    data: { id: id },
                    success: function (res) {
                        toastr.success(res.message);
                        Pos.loadData();
                    },
                    error: function () {
                        toastr.error("Lỗi tải trang");
                    }
                });
            }
        });
    },
    openCreateModal: function () {
        $.ajax({
            url: '/Admin/JobApplication/Create',
            type: 'GET',
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#applyJobModal').modal('show');
            },
            error: function () {
                alert("Đã có lỗi xảy ra khi tải form.");
            }
        });
    },
    create: function () {
        var form = $('#applyForm')[0]; // get the raw DOM element
        var formData = new FormData(form);
        $.ajax({
            url: '/Admin/JobApplication/Create',
            type: 'POST',
            data: formData,
            processData: false, // important for FormData
            contentType: false,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    $('#applyJobModal').modal('hide');
                    toastr.success(res.message);
                    JobApp.loadData(currentPage); // đảm bảo currentPage đã khai báo
                } else {
                    toastr.error(res.message);
                }
            },
            error: function () {
                toastr.error("Đã xảy ra lỗi khi gửi dữ liệu.");
            }
        });
    },
    changeStatus: function (jobId, selectElement) {
        var status = selectElement.value; 
        $.ajax({
            url: "/Admin/JobApplication/Status",
            type: "get",
            data: {
                jobId: jobId,
                status: status
            },
            success: function (res) {
                toastr.success(res.message);
                JobApp.loadData(currentPage);
            },
            error: function () {
                alert("Lỗi tải dữ liệu");
            }
        });
    },
}
JobApp.loadData();
$(document).on('submit', '#jobApplicationEditForm', function (e) {
    e.preventDefault(); // ngăn reload trang
    JobApp.EditPost();     // gọi AJAX
});
$(document).on('submit', '#applyForm', function (e) {
    e.preventDefault(); // ❗ Chặn submit mặc định
    JobApp.create();   // Gọi hàm xử lý AJAX
});