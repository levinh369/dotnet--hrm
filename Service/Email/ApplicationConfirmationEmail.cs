using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace DACN.Service.Email
{
    public class ApplicationConfirmationEmail
    {
        private readonly IConfiguration _config;

        public ApplicationConfirmationEmail(IConfiguration config)
        {
            _config = config;
        }

        // Gửi email xác nhận 
        public async Task SendOrderConfirmationAsync(string toEmail, string employeeName, string title)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Vertex HR", _config["EmailSettings:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = $"Công ty Vertex HR - Xác nhận ứng tuyển thành công";

            // Build body HTML
            var body = $@"
                <html>
                <body>
                    <p>Chào <strong>{employeeName}</strong>,</p>
                    <p>Hệ thống Vertex HR đã nhận được đơn ứng tuyển của bạn cho vị trí <strong>{title}</strong>.</p>
                    <p>Bộ phận tuyển dụng của chúng tôi sẽ xem xét hồ sơ và phản hồi trong thời gian sớm nhất.</p>
                    <br/>
                    <p>Cảm ơn bạn đã quan tâm đến Vertex HR!</p>
                    <p>Trân trọng,<br/>Bộ phận Tuyển dụng</p>
                </body>
                </html>
                ";

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
