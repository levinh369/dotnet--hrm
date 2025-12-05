var job = {
    openApplyModal: function (id) {
        $.ajax({
            url: '/Employee/Job/ApplyJob',
            type: 'GET',
            data: { jobId: id },
            success: function (result) {
                $('#modal-placeholder').html(result);
                $('#applyJobModal').modal('show');
            },
            error: function () {
                alert("Đã có lỗi xảy ra khi tải form.");
            }
        });
    },
    apply: function () {
        var form = $('#applyForm')[0]; // get the raw DOM element
        var formData = new FormData(form);
        $.ajax({
            url: '/Employee/Job/Apply',
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
$(document).on('submit', '#applyForm', function (e) {
    e.preventDefault(); // ❗ Chặn submit mặc định
    job.apply();   // Gọi hàm xử lý AJAX
});