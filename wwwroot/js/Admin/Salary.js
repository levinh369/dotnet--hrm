var Salary = {
    // Hàm khởi tạo (Chạy khi trang vừa load xong)
    init: function () {
        // 1. Load dữ liệu mặc định (Tháng hiện tại)
        Salary.loadData();

        // 2. BẮT SỰ KIỆN ONCHANGE CỦA Ô THÁNG
        $('#txtMonth').on('change', function () {
            // Lấy giá trị vừa chọn để hiển thị lên Label cho đẹp
            var val = $(this).val(); // "2025-11"
            if (val) {
                var parts = val.split('-');
                $('#lblCurrentPeriod').text(parts[1] + '/' + parts[0]);
            }
            Salary.loadData(1);
        });
    },

    // Hàm tải dữ liệu (như chúng ta đã viết)
    loadData: function (pageIndex) {
        if (pageIndex === undefined) pageIndex = 1;

        // Lấy tháng/năm TỪ CHÍNH Ô INPUT ĐÓ
        var monthStr = $('#txtMonth').val();
        var month = 0;
        var year = 0;

        if (monthStr) {
            var parts = monthStr.split('-');
            year = parseInt(parts[0]);
            month = parseInt(parts[1]);
        }

        // Gọi AJAX
        $.ajax({
            url: "/Admin/Salary/ListData",
            type: "GET",
            data: {
                page: pageIndex,
                pageSize: 5,
                month: month,
                year: year
            },
            success: function (result) {
                $("#gridData").html(result);

                // Cập nhật Card thống kê...
                if ($('#hid-total').length) {
                    $('#lblTotalSalary').text($('#hid-total').val());
                    $('#lblNetSalary').text($('#hid-net').val());
                    $('#lblDeduction').text($('#hid-deduct').val());
                } else {
                    $('#lblTotalSalary').text('0 VNĐ');
                    // ... reset các số khác
                }
            }
        });
    },

    calculate: function () {
        var inputDate = $('#txtMonth').val();
        if (!inputDate) {
            Swal.fire({
                icon: 'warning',
                title: 'Chưa chọn tháng!',
                text: 'Vui lòng chọn tháng năm để tính lương.',
            });
            return;
        }

        var parts = inputDate.split("-");
        var year = parts[0];
        var month = parts[1];
        Swal.fire({
            title: `Tính lương tháng ${month}/${year}?`,
            text: "Hệ thống sẽ tính toán lại dữ liệu (nếu chưa chốt). Bạn có chắc chắn không?",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#ffc107', 
            cancelButtonColor: '#6c757d',
            confirmButtonText: '<i class="fas fa-calculator"></i> Tính ngay',
            cancelButtonText: 'Hủy bỏ'
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire({
                    title: 'Đang xử lý...',
                    text: 'Vui lòng không tắt trình duyệt khi hệ thống đang tính toán.',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });
                $.ajax({
                    url: '/Admin/Salary/CalculatePayroll',
                    type: 'POST',
                    data: {
                        month: month,
                        year: year
                    },
                    success: function (res) {
                        if (res.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Hoàn tất!',
                                text: res.message,
                                timer: 2000, 
                                showConfirmButton: false
                            });
                            Salary.loadData();
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Không thể tính lương',
                                text: res.message
                            });
                        }
                    },
                    error: function () {
                        // 5c. Lỗi Server/Mạng
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi hệ thống',
                            text: 'Không thể kết nối đến máy chủ. Vui lòng thử lại sau.'
                        });
                    }
                });
            }
        });
    },
    edit: function (id) {
        $.ajax({
            url: '/Admin/Salary/Edit',
            type: 'GET',
            data: { id: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#salaryEditModal').modal('show');
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }

        });
    },
    EditPost: function () {
        var form = $('#frmEditSalary')[0]; // get the raw DOM element
        var formData = new FormData(form);
        $.ajax({
            url: "/Admin/Salary/EditPost",
            type: "POST",
            data: formData,
            processData: false, // important for FormData
            contentType: false,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                $('#salaryEditModal').modal('hide');
                toastr.success(res.message);
                Salary.loadData();
            },
            error: function () {
                toastr.error("Lỗi tải trang");
            }
        });
    },
}
$(function () {
    Salary.init();
});