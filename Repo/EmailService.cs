using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using AnalyticaDocs.Repository;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _emailSettings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendCanteenEmailAsync(string employeeName, string epin, string recipients)
    {
        var subject = "Canteen PIN";
        var body = $@"
            <table>
                <tr><td>Dear {employeeName},</td></tr>
                <tr><td>Your Canteen Token PIN code is <strong>{epin}</strong>. Please keep this confidential.</td></tr>
                <tr><td><br/><strong>Instructions:</strong><br/>
                    1. Redeem at <a href='http://34.131.176.131:8080/Source/TokenRedeem'>Token Page</a><br/>
                    2. Don’t share your PIN.<br/>
                    3. Redemption is final.<br/>
                    4. Report any misuse to 
                    <a href='mailto:it@aetherbreweries.com'>it@aetherbreweries.com</a>.
                </td></tr>
                <tr><td><br/>Thank you,<br/>Token Management Team</td></tr>
                <tr><td><br/><strong>Disclaimer:</strong> This is a system-generated email. Please do not reply. For queries, contact 
                    <a href='mailto:HR@aetherbreweries.com'>HR</a>.
                </td></tr>
            </table>";

        return await SendEmailAsync(recipients, subject, body);
    }

    private async Task<bool> SendEmailAsync(string toEmails, string subject, string htmlBody)
    {
        try
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "Canteen"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            foreach (var addr in toEmails.Split(','))
            {
                if (!string.IsNullOrWhiteSpace(addr))
                    mail.To.Add(new MailAddress(addr.Trim()));
            }

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}", toEmails);
            return false;
        }
    }
}