using FUParkingModel.MailObject;
using FUParkingService.MailObject;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FUParkingService.MailService
{
    public class MailService : IMailService
    {
        private readonly MailSetting mailSetting;

        public MailService(IOptions<MailSetting> options)
        {
            this.mailSetting = options.Value;
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage
            {
                Sender = new MailboxAddress(mailSetting.DisplayName, mailSetting.Email)
            };
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = EmailTemplate(mailRequest.ToUsername, mailRequest.Body)
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(mailSetting.Host, mailSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(mailSetting.Email, mailSetting.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        #region Private Methods
        private static string EmailTemplate(string username, string body)
        {
            return $@"
                <!DOCTYPE html>
                <html lang=""vi"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Template Email</title>
                    <style>
                        body {{
                            margin: 0;
                            padding: 0;
                            font-family: Arial, sans-serif;
                            color: #333;
                        }}
                        .container {{width: 100%;
                            max-width: 800px; 
                            margin: 20px auto; 
                            border: 2px solid #ddd; 
                            border-radius: 10px; 
                            overflow: hidden;
                        }}
                        .header {{background - color: #f4f4f4;
                            text-align: center;
                            padding: 20px 0;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                        }}
                        .header img {{width: 100%;
                            max-width: 150px;
                            height: auto;
                            margin: 0 10px;
                        }}
                        .header .logo2 {{
                               max-height:65px;
                        }}
                        .body {{
                            padding: 50px 100px;
                        }}
                        .footer {{
                            background-color: #f4f4f4;
                            text-align: center;
                            padding: 10px 0;
                            font-size: 14px;
                            color: #777;
                        }}
                        .footer a {{
                            color: #007bff;
                            text-decoration: none;
                        }}
                        .promotion {{
                            border: 1px solid #ddd;
                            border-radius: 5px;
                            padding: 10px;
                            margin-top: 10px;
                        }}
                        .promotion-header {{
                            font - size: 16px;
                            font-weight: bold;
                            margin-bottom: 5px;
                        }}
                        .promotion-body {{
                            font - size: 14px;
                        }}
                        .details {{
                            border: 1px solid #ddd;
                            border-radius: 5px;
                            padding: 10px;
                            margin-top: 10px;
                        }}
                        .details-header {{
                            font - size: 16px;
                            font-weight: bold;
                            margin-bottom: 5px;
                        }}
                        .details-body {{
                            font - size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <!-- Header -->
                        <div class=""header"">
                            <img src=""https://williamkieu-devops.cloud/images/fpt-university.png"" alt=""FPTU Logo"">
                            <img src=""https://lh3.googleusercontent.com/a/ACg8ocIZzGAJC2gFR--_2Qk81gXCG_zXwzAVE9vlrbrZx0S_ZB9smoM=s288-c-no"" alt=""Bai Parking Logo"" >
                        </div>

                        <!-- Body -->
                        <div class=""body"">
                            <h2>Dear {username},</h2>
                            {body}
                            <p>If you have any questions or need further assistance, please do not hesitate to contact us.</p>
                            <p>Thank you for choosing Bai Parking! We look forward to serving you.</p>
                            <p>Best regards,<br/>The Bai Parking Team</p>
                        </div>

                        <!-- Footer -->
                        <div class=""footer"">
                            <p>A: Lot E2a-7, D1 Street, Hi-Tech Park, Long Thanh My Ward, Thu Duc City, Ho Chi Minh City</p>
                            <p>E: <a href=""mailto:baiparking.system@gmail.com"">baiparking.system@gmail.com</a> | T: (028) 7300 5588</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
        #endregion
    }
}