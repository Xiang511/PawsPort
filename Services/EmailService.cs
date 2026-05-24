using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Serilog;
namespace PawsPort.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 發送驗證碼郵件
        /// </summary>
        /// <param name="toEmail">收件人郵箱</param>
        /// <param name="verificationCode">驗證碼</param>
        /// <param name="userName">用戶名稱</param>
        /// <returns>是否發送成功</returns>
        public async Task<bool> SendVerificationCodeAsync(string toEmail, string verificationCode, string userName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]));
                message.To.Add(new MailboxAddress(userName, toEmail));
                message.Subject = "PETMILY - 登入驗證碼";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                                <h2 style='color: #4CAF50; text-align: center;'>PETMILY 登入驗證</h2>
                                <p>親愛的 <strong>{userName}</strong>，您好！</p>
                                <p>您正在嘗試登入 PETMILY 寵物媒合平台。</p>
                                <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px;'>
                                    <p style='margin: 0; font-size: 14px; color: #666;'>您的驗證碼是：</p>
                                    <h1 style='margin: 10px 0; color: #4CAF50; font-size: 36px; letter-spacing: 5px;'>{verificationCode}</h1>
                                    <p style='margin: 0; font-size: 12px; color: #999;'>此驗證碼將在 5 分鐘後失效</p>
                                </div>
                                <p style='color: #666; font-size: 14px;'>
                                    <strong>注意事項：</strong><br>
                                    • 請勿將此驗證碼分享給任何人<br>
                                    • 如果這不是您本人的操作，請忽略此郵件
                                </p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                                <p style='font-size: 12px; color: #999; text-align: center;'>
                                    © 2026 PETMILY. All rights reserved.
                                </p>
                            </div>
                        </body>
                        </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // 根據配置選擇連接方式
                    var smtpServer = emailSettings["SmtpServer"];
                    var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                    var useSsl = bool.Parse(emailSettings["UseSsl"] ?? "true");

                    await client.ConnectAsync(smtpServer, smtpPort,
                        useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                    // 如果需要認證
                    var username = emailSettings["Username"];
                    var password = emailSettings["Password"];

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        await client.AuthenticateAsync(username, password);
                    }

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    Log.Information("[EmailService] 驗證碼郵件已發送至: {Email}", toEmail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[EmailService] 發送驗證碼郵件失敗: {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// 發送註冊 Email 驗證碼郵件
        /// </summary>
        /// <param name="toEmail">收件人郵箱</param>
        /// <param name="verificationCode">驗證碼</param>
        /// <param name="userName">用戶名稱</param>
        /// <returns>是否發送成功</returns>
        public async Task<bool> SendRegisterVerificationCodeAsync(string toEmail, string verificationCode, string userName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]));
                message.To.Add(new MailboxAddress(userName, toEmail));
                message.Subject = "PawsPort - 註冊 Email 驗證碼";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                                <h2 style='color: #4CAF50; text-align: center;'>🐾 PawsPort 註冊驗證</h2>
                                <p>親愛的 <strong>{userName}</strong>，歡迎加入 PawsPort！</p>
                                <p>請使用以下驗證碼完成您的帳號註冊：</p>
                                <div style='background-color: #f4f4f4; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px;'>
                                    <p style='margin: 0; font-size: 14px; color: #666;'>您的驗證碼是：</p>
                                    <h1 style='margin: 10px 0; color: #4CAF50; font-size: 36px; letter-spacing: 5px;'>{verificationCode}</h1>
                                    <p style='margin: 0; font-size: 12px; color: #999;'>此驗證碼將在 10 分鐘後失效</p>
                                </div>
                                <p style='color: #666; font-size: 14px;'>
                                    <strong>注意事項：</strong><br>
                                    • 請勿將此驗證碼分享給任何人<br>
                                    • 如果這不是您本人的操作，請忽略此郵件
                                </p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                                <p style='font-size: 12px; color: #999; text-align: center;'>
                                    © 2026 PawsPort. All rights reserved.
                                </p>
                            </div>
                        </body>
                        </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    var smtpServer = emailSettings["SmtpServer"];
                    var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                    var useSsl = bool.Parse(emailSettings["UseSsl"] ?? "true");

                    await client.ConnectAsync(smtpServer, smtpPort,
                        useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                    var username = emailSettings["Username"];
                    var password = emailSettings["Password"];

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        await client.AuthenticateAsync(username, password);
                    }

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    Log.Information("[EmailService] 註冊驗證碼郵件已發送至: {Email}", toEmail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[EmailService] 發送註冊驗證碼郵件失敗: {Email}", toEmail);
                return false;
            }
        }

        /// <summary>
        /// 發送密碼重置郵件
        /// </summary>
        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var resetLink = $"{emailSettings["WebsiteUrl"]}/reset-password?token={resetToken}";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    emailSettings["SenderName"],
                    emailSettings["SenderEmail"]));
                message.To.Add(new MailboxAddress(userName, toEmail));
                message.Subject = "PETMILY - 密碼重置請求";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                                <h2 style='color: #4CAF50; text-align: center;'>密碼重置請求</h2>
                                <p>親愛的 <strong>{userName}</strong>，您好！</p>
                                <p>我們收到了您的密碼重置請求。</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                        重置密碼
                                    </a>
                                </div>
                                <p style='color: #666; font-size: 14px;'>
                                    此連結將在 24 小時後失效。如果這不是您本人的操作，請忽略此郵件。
                                </p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                                <p style='font-size: 12px; color: #999; text-align: center;'>
                                    © 2026 PETMILY. All rights reserved.
                                </p>
                            </div>
                        </body>
                        </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    var smtpServer = emailSettings["SmtpServer"];
                    var smtpPort = int.Parse(emailSettings["SmtpPort"]);
                    var useSsl = bool.Parse(emailSettings["UseSsl"] ?? "true");

                    await client.ConnectAsync(smtpServer, smtpPort,
                        useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                    var username = emailSettings["Username"];
                    var password = emailSettings["Password"];

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        await client.AuthenticateAsync(username, password);
                    }

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    Log.Information("[EmailService] 密碼重置郵件已發送至: {Email}", toEmail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[EmailService] 發送密碼重置郵件失敗: {Email}", toEmail);
                return false;
            }
        }
    }
}
