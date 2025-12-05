var currentPage = 1;
var conTract = {
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
            url: "/Admin/Contract/ListData",
            type: "get",
            data: data,
            success: function (result) {
                $("#gridData").html(result); // Gắn HTML trả về vào div
                var totalPages = $("#pagination").data("total-pages");
                if (!$('#paging-ul').data("twbs-pagination")) {
                    conTract.showPaging(totalPages, pageIndex);
                }
            },

            error: function () {
                alert("Lỗi tải dữ liệu");
            }
        });
    },
    detail: function (id) {
        $.ajax({
            url: '/Admin/Contract/Detail',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#detailContractModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
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
                        conTract.loadData(page);
                    }
                }
            });
            $('#paging-ul').data('init-complete', true); // đánh dấu đã init
        }
    },
    edit: function (id) {
        $.ajax({
            url: '/Admin/Contract/Edit',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#editContractModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }

        });
    },
    renew: function (id) {
        $.ajax({
            url: '/Admin/Contract/renew',
            type: 'GET',
            data: { contractId: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#renewContractModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }

        });
    },
    RenewPost: function () {
        var form = $('#formRenewContract')[0]; // get the raw DOM element
        var formData = new FormData(form);
        $.ajax({
            url: "/Admin/Contract/RenewPost",
            type: "POST",
            data: formData,
            processData: false, // important for FormData
            contentType: false,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                $('#renewContractModal').modal('hide');
                toastr.success(res.message);
                conTract.loadData();
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }
        });
    },
    EditPost: function () {
        var form = $('#formEditContract')[0]; // get the raw DOM element
        var formData = new FormData(form);
        $.ajax({
            url: "/Admin/Contract/EditPost",
            type: "POST",
            data: formData,
            processData: false, // important for FormData
            contentType: false,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                $('#editContractModal').modal('hide');
                toastr.success(res.message);
                conTract.loadData();
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
                    url: "/Admin/Contract/Delete",
                    type: "GET",
                    data: { id: id },
                    success: function (res) {
                        toastr.success(res.message);
                        Dep.loadData();
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
            url: '/Admin/Contract/Create',
            type: 'GET',
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#addContractModal').modal('show');
            },
            error: function () {
                alert("Đã có lỗi xảy ra khi tải form.");
            }
        });
    },
    create: function () {
        var form = $('#formAddContract')[0]; // get the raw DOM element
        var formData = new FormData(form);
        $.ajax({
            url: '/Admin/Contract/Create',
            type: 'POST',
            data: formData,
            processData: false, // important for FormData
            contentType: false,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    $('#addContractModal').modal('hide');
                    toastr.success(res.message);
                    conTract.loadData(currentPage); // đảm bảo currentPage đã khai báo
                } else {
                    toastr.error(res.message);
                }
            },
            error: function () {
                toastr.error("Đã xảy ra lỗi khi gửi dữ liệu.");
            }
        });
    },
}
conTract.loadData();
$(document).on('submit', '#formEditContract', function (e) {
    e.preventDefault(); // ngăn reload trang
    conTract.EditPost();     // gọi AJAX
});
$(document).on("submit", "#searchForm", function (e) {
    e.preventDefault(); // ❌ chặn reload trang
    Dep.loadData();     // ✅ gọi Ajax load dữ liệu
});
$(document).on('submit', '#formAddContract', function (e) {
    e.preventDefault(); // ❗ Chặn submit mặc định
    conTract.create();   // Gọi hàm xử lý AJAX
});
$(document).on('submit', '#formRenewContract', function (e) {
    e.preventDefault(); // ngăn reload trang
    conTract.RenewPost();     // gọi AJAX
});