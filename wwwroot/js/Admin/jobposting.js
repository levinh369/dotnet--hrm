var currentPage = 1;
var Job = {
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
            url: "/Admin/JobPosting/ListData",
            type: "get",
            data: data,
            success: function (result) {
                $("#gridData").html(result); // Gắn HTML trả về vào div
                var totalPages = $("#pagination").data("total-pages");
                if (!$('#paging-ul').data("twbs-pagination")) {
                    Job.showPaging(totalPages, pageIndex);
                }
            },

            error: function () {
                alert("Lỗi tải dữ liệu");
            }
        });
    },
    openCreateModal: function () {
        $.ajax({
            url: '/Admin/JobPosting/Create',
            type: 'GET',
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#addJobModal').modal('show');
            },
            error: function () {
                alert("Đã có lỗi xảy ra khi tải form.");
            }
        });
    },
    create: function () {
        var form = $('#addCVForm');
        var formData = form.serialize();

        $.ajax({
            url: '/Admin/JobPosting/Create',
            type: 'POST',
            data: formData,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    $('#addJobModal').modal('hide');
                    toastr.success(res.message);
                    Job.loadData(currentPage); // đảm bảo currentPage đã khai báo
                } else {
                    toastr.error(res.message);
                }
            },
            error: function () {
                toastr.error("Đã xảy ra lỗi khi gửi dữ liệu.");
            }
        });
    },
    detail: function (id) {
        $.ajax({
            url: '/Admin/JobPosting/Detail',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#viewJobModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }
        });

    },
    edit: function (id) {
        $.ajax({
            url: '/Admin/JobPosting/Edit',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#editJobModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }

        });
    },
    EditPost: function () {
        var data = $('#editJobForm').serialize();
        $.ajax({
            url: "/Admin/JobPosting/EditPost",
            type: "POST",
            data: data,
            success: function (res) {
                $('#editJobModal').modal('hide');
                toastr.success(res.message);
                Job.loadData();
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
                    url: "/Admin/JobPosting/Delete",
                    type: "GET",
                    data: { id: id },
                    success: function (res) {
                        toastr.success(res.message);
                        Job.loadData();
                    },
                    error: function () {
                        toastr.error("Lỗi tải trang");
                    }
                });
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
                        Job.loadData(page);
                    }
                }
            });
            $('#paging-ul').data('init-complete', true); // đánh dấu đã init
        }
    },
}
Job.loadData();
$(document).on('submit', '#addCVForm', function (e) {
    e.preventDefault(); // ❗ Chặn submit mặc định
    Job.create();   // Gọi hàm xử lý AJAX
});
$(document).on('submit', '#editJobForm', function (e) {
    e.preventDefault(); // ngăn reload trang
    Job.EditPost();     // gọi AJAX
});
$(document).on("submit", "#searchForm", function (e) {
    e.preventDefault(); // ❌ chặn reload trang
    Job.loadData();     // ✅ gọi Ajax load dữ liệu
});