namespace Core.ThirdParty.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    Task<bool> SendCredentialsEmailAsync(string toEmail, string userName, string password, CancellationToken cancellationToken = default);
    Task<bool> SendPasswordChangedEmailAsync(string toEmail, string newPassword, CancellationToken cancellationToken = default);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_emailSettings.SmtpUsername) || string.IsNullOrEmpty(_emailSettings.SmtpPassword))
            {
                // In development, log instead of sending
                Console.WriteLine($"Email would be sent to: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Body: {body}");
                return true;
            }

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendCredentialsEmailAsync(string toEmail, string userName, string password, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to VitaGym Portal - Your Account Credentials";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #4CAF50;'>Welcome to VitaGym Portal!</h2>
                    <p>Your account has been created successfully. Please find your login credentials below:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Username:</strong> {userName}</p>
                        <p><strong>Password:</strong> {password}</p>
                    </div>
                    <p>Please keep these credentials secure and change your password after your first login.</p>
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>This is an automated message. Please do not reply to this email.</p>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task<bool> SendPasswordChangedEmailAsync(string toEmail, string newPassword, CancellationToken cancellationToken = default)
    {
        var subject = "VitaGym Portal - Your Password Has Been Changed";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #2196F3;'>Password Changed</h2>
                    <p>Your password has been changed successfully. Your new password is:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>New Password:</strong> {newPassword}</p>
                    </div>
                    <p>Please keep this password secure and consider changing it after your next login.</p>
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>This is an automated message. Please do not reply to this email.</p>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }
}

