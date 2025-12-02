using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using static DACN.Enums.StatusEnums;

namespace DACN.Service.Email
{
    public class HrConfirmEmail
    {
        private readonly IConfiguration _config;

        public HrConfirmEmail(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Gửi email thông báo dựa trên trạng thái ứng tuyển (Không cần ghi chú)
        /// </summary>
        /// <param name="toEmail">Email ứng viên</param>
        /// <param name="employeeName">Tên ứng viên</param>
        /// <param name="title">Vị trí ứng tuyển</param>
        /// <param name="status">Trạng thái (Pending, Approved, Rejected)</param>
        public async Task SendApplicationStatusEmailAsync(string toEmail, string employeeName, string title, ApplicationStatus status)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Vertex HR", _config["EmailSettings:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            string subject = "";
            string body = "";
            // Phân loại email dựa trên trạng thái
            switch (status)
            {
                case ApplicationStatus.Pending:
                    return; // Không gửi email cho trạng thái Pending

                case ApplicationStatus.Approved:
                    subject = $"Chúc mừng! Bạn đã trúng tuyển vị trí {title} tại Vertex HR";
                    body = $@"
                        <html>
                        <body>
                            <p>Chào <strong>{employeeName}</strong>,</p>
                            <p>Chúng tôi vui mừng thông báo bạn đã <strong>trúng tuyển</strong> vị trí <strong>{title}</strong> tại Vertex HR.</p>
                            <p>Bộ phận nhân sự sẽ liên hệ với bạn trong thời gian sớm nhất để trao đổi về các bước tiếp theo.</p>
                            <br/>
                            <p>Chào mừng bạn đến với gia đình Vertex HR!</p>
                            <p>Trân trọng,<br/>Bộ phận Tuyển dụng</p>
                        </body>
                        </html>";
                    break;

                case ApplicationStatus.Rejected:
                    subject = $"Phản hồi hồ sơ ứng tuyển vị trí {title} tại Vertex HR";
                    body = $@"
                        <html>
                        <body>
                            <p>Chào <strong>{employeeName}</strong>,</p>
                            <p>Cảm ơn bạn đã quan tâm và ứng tuyển vị trí <strong>{title}</strong> tại Vertex HR.</p>
                            <p>Sau quá trình xem xét, chúng tôi rất tiếc phải thông báo rằng hồ sơ của bạn chưa phù hợp với các tiêu chí của vị trí này ở thời điểm hiện tại.</p>
                            <p>Chúng tôi sẽ lưu lại hồ sơ của bạn và sẽ liên hệ khi có vị trí khác phù hợp hơn trong tương lai.</p>
                            <br/>
                            <p>Chúc bạn sớm tìm được công việc như ý.</p>
                            <p>Trân trọng,<br/>Bộ phận Tuyển dụng</p>
                        </body>
                        </html>";
                    break;
            }

            if (string.IsNullOrEmpty(subject))
            {
                return;
            }

            email.Subject = subject;
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