using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace DACN.Service.Email
{
    public class SendWelcomeEmail
    {
        private readonly IConfiguration _config;

        public SendWelcomeEmail(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendWelcomeEmailAsync(string toEmail, string employeeName, string positionName, string departmentName, DateTime? startDate, string activationLink)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Vertex HR", _config["EmailSettings:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));

            email.Subject = "Chào mừng bạn đến với Vertex HR - Kích hoạt tài khoản nhân viên";

            string body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                    .container {{ width: 90%; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                    .button {{ background-color: #007bff; color: #ffffff; padding: 12px 25px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold; }}
                    .button:hover {{ background-color: #0056b3; }}
                    .info-box {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <p>Chào <strong>{employeeName}</strong>,</p>
                    <p>Chào mừng bạn đã chính thức trở thành thành viên của gia đình <strong>Vertex HR</strong>!</p>
                    <p>Bộ phận Nhân sự đã hoàn tất hồ sơ và tạo tài khoản nhân viên cho bạn trên hệ thống nội bộ của chúng tôi.</p>
                    
                    <hr style='border:0; border-top: 1px solid #eee;'>

                    <h3 style='color: #007bff;'>HÀNH ĐỘNG QUAN TRỌNG</h3>
                    <p>Vui lòng kích hoạt tài khoản của bạn bằng cách thiết lập mật khẩu cá nhân. Tên đăng nhập của bạn chính là email này.</p>
                    <p style='text-align: center; margin: 25px 0;'>
                        <a href='{activationLink}' class='button'>NHẤN VÀO ĐÂY ĐỂ ĐẶT MẬT KHẨU</a>
                    </p>
                    <p><small><i>Lưu ý: Vì lý do bảo mật, liên kết này sẽ chỉ có hiệu lực trong 24 giờ.</i></small></p>

                    <div class='info-box'>
                        <strong>Chúng tôi xin xác nhận thông tin công việc của bạn:</strong>
                        <ul>
                            <li><strong>Tên đăng nhập:</strong> {toEmail}</li>
                            <li><strong>Vị trí:</strong> {positionName}</li>
                            <li><strong>Phòng ban:</strong> {departmentName}</li>
                            <li><strong>Ngày bắt đầu:</strong> {startDate?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật"}</li>
                        </ul>
                    </div>
                    
                    <p>Nếu bạn gặp bất kỳ vấn đề gì, vui lòng liên hệ với Bộ phận IT (it.support@vertex.com).</p>
                    <br/>
                    <p>Trân trọng,<br/>Bộ phận Nhân sự Vertex HR</p>
                </div>
            </body>
            </html>";

            email.Body = new TextPart(TextFormat.Html) { Text = body };

            // Gửi email qua SMTP
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["EmailSettings:Host"],
                int.Parse(_config["EmailSettings:Port"]),
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _config["EmailSettings:From"],
                _config["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
    }

