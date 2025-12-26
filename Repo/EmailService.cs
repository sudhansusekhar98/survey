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

    public async Task<bool> SendSurveySubmissionNotificationAsync(string supervisorName, string supervisorEmail, 
        string surveyName, string submittedBy, DateTime submissionDate)
    {
        var subject = $"Survey Submitted for Review - {surveyName}";
        var body = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h3 style='color: #6f42c1;'>Survey Submission Notification</h3>
                <p>Dear {supervisorName},</p>
                <p>A new survey has been submitted for your review:</p>
                <table style='border: 1px solid #ddd; border-collapse: collapse; width: 100%; max-width: 600px;'>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Survey Name:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{surveyName}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Submitted By:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{submittedBy}</td>
                    </tr>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Submission Date:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{submissionDate:dd-MMM-yyyy hh:mm tt}</td>
                    </tr>
                </table>
                <p style='margin-top: 20px;'>Please review and approve/reject the survey at your earliest convenience.</p>
                <p style='margin-top: 20px;'>Best Regards,<br/>Survey Management System</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #666;'>
                    <strong>Note:</strong> This is an automated email. Please do not reply to this message.
                </p>
            </div>";

        return await SendEmailAsync(supervisorEmail, subject, body);
    }

    public async Task<bool> SendSurveyApprovalNotificationAsync(string submitterName, string submitterEmail, 
        string surveyName, string reviewedBy, string reviewComments)
    {
        var subject = $"Survey Approved - {surveyName}";
        var body = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h3 style='color: #28a745;'>Survey Approved ✓</h3>
                <p>Dear {submitterName},</p>
                <p>Your survey has been <strong style='color: #28a745;'>approved</strong>:</p>
                <table style='border: 1px solid #ddd; border-collapse: collapse; width: 100%; max-width: 600px;'>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Survey Name:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{surveyName}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Reviewed By:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{reviewedBy}</td>
                    </tr>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Review Date:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{DateTime.Now:dd-MMM-yyyy hh:mm tt}</td>
                    </tr>
                    {(!string.IsNullOrEmpty(reviewComments) ? $@"
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; vertical-align: top;'><strong>Comments:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{reviewComments}</td>
                    </tr>" : "")}
                </table>
                <p style='margin-top: 20px;'>Congratulations! Your survey has been completed successfully.</p>
                <p style='margin-top: 20px;'>Best Regards,<br/>Survey Management System</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #666;'>
                    <strong>Note:</strong> This is an automated email. Please do not reply to this message.
                </p>
            </div>";
        return await SendEmailAsync(submitterEmail, subject, body);
    }

    public async Task<bool> SendSurveyRejectionNotificationAsync(string submitterName, string submitterEmail, 
        string surveyName, string reviewedBy, string reviewComments)
    {
        var subject = $"Survey Requires Revision - {surveyName}";
        var body = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h3 style='color: #dc3545;'>Survey Requires Revision</h3>
                <p>Dear {submitterName},</p>
                <p>Your survey has been <strong style='color: #dc3545;'>rejected</strong> and requires revisions:</p>
                <table style='border: 1px solid #ddd; border-collapse: collapse; width: 100%; max-width: 600px;'>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Survey Name:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{surveyName}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Reviewed By:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{reviewedBy}</td>
                    </tr>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Review Date:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{DateTime.Now:dd-MMM-yyyy hh:mm tt}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; vertical-align: top;'><strong>Rejection Reason:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd; color: #dc3545;'>{reviewComments ?? "No comments provided"}</td>
                    </tr>
                </table>
                <p style='margin-top: 20px; padding: 15px; background-color: #fff3cd; border-left: 4px solid #ffc107;'>
                    <strong>Action Required:</strong> Please review the comments above and make necessary corrections to the survey. 
                    The survey has been unlocked for editing.
                </p>
                <p style='margin-top: 20px;'>Best Regards,<br/>Survey Management System</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #666;'>
                    <strong>Note:</strong> This is an automated email. Please do not reply to this message.
                </p>
            </div>";

        return await SendEmailAsync(submitterEmail, subject, body);
    }

    public async Task<bool> SendSurveyAssignmentNotificationAsync(string employeeName, string employeeEmail, 
        string surveyName, DateTime? dueDate)
    {
        var subject = $"New Survey Assigned: {surveyName}";
        var body = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h3 style='color: #007bff;'>New Survey Assignment</h3>
                <p>Dear {employeeName},</p>
                <p>A new survey has been assigned to you:</p>
                <table style='border: 1px solid #ddd; border-collapse: collapse; width: 100%; max-width: 600px;'>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Survey Name:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{surveyName}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Due Date:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{dueDate:dd-MMM-yyyy}</td>
                    </tr>
                </table>
                <p style='margin-top: 20px;'>Please complete the survey by the due date.</p>
                <p style='margin-top: 20px;'>Best Regards,<br/>Survey Management System</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #666;'>
                    <strong>Note:</strong> This is an automated email. Please do not reply to this message.
                </p>
            </div>";

        return await SendEmailAsync(employeeEmail, subject, body);
    }

    public async Task<bool> SendNewUserAccountNotificationAsync(string userName, string userEmail, 
        string loginId, string temporaryPassword)
    {
        var subject = "Your New Account Has Been Created - Survey Management System";
        var body = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h3 style='color: #007bff;'>Welcome to Survey Management System!</h3>
                <p>Dear {userName},</p>
                <p>Your account has been created. Please use the following credentials to login:</p>
                <table style='border: 1px solid #ddd; border-collapse: collapse; width: 100%; max-width: 600px;'>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Login ID:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{loginId}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Temporary Password:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd; font-family: monospace; font-size: 14px;'>{temporaryPassword}</td>
                    </tr>
                </table>
                <p style='margin-top: 20px; padding: 15px; background-color: #fff3cd; border-left: 4px solid #ffc107;'>
                    <strong>Important:</strong> You will be required to change your password on first login. 
                    Please keep your credentials secure and do not share them with anyone.
                </p>
                <p style='margin-top: 20px;'>Best Regards,<br/>Survey Management System</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #666;'>
                    <strong>Note:</strong> This is an automated email. Please do not reply to this message.
                </p>
            </div>";

        return await SendEmailAsync(userEmail, subject, body);
    }

    public async Task<bool> SendPasswordResetNotificationAsync(string userName, string userEmail, 
        string loginId, string temporaryPassword, string resetByName)
    {
        var subject = "Password Reset - Survey Management System";
        var body = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h3 style='color: #dc3545;'>Password Reset Notification</h3>
                <p>Dear {userName},</p>
                <p>Your password has been reset by <strong>{resetByName}</strong>. Please use the following credentials to login:</p>
                <table style='border: 1px solid #ddd; border-collapse: collapse; width: 100%; max-width: 600px;'>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Login ID:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{loginId}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Temporary Password:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd; font-family: monospace; font-size: 14px;'>{temporaryPassword}</td>
                    </tr>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>Reset Date:</strong></td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{DateTime.Now:dd-MMM-yyyy hh:mm tt}</td>
                    </tr>
                </table>
                <p style='margin-top: 20px; padding: 15px; background-color: #f8d7da; border-left: 4px solid #dc3545;'>
                    <strong>Action Required:</strong> You will be required to change your password on your next login. 
                    If you did not request this password reset, please contact your administrator immediately.
                </p>
                <p style='margin-top: 20px;'>Best Regards,<br/>Survey Management System</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #666;'>
                    <strong>Note:</strong> This is an automated email. Please do not reply to this message.
                </p>
            </div>";

        return await SendEmailAsync(userEmail, subject, body);
    }

    public async Task<bool> SendReportOTPNotificationAsync(string adminName, string adminEmail,
        string requestingUserName, int requestingUserId, string reportType, 
        string otp, DateTime expiresAt)
    {
        var subject = $"Report Download OTP Request - {requestingUserName}";
        var body = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h3 style='color: #6f42c1;'>🔐 Report Download OTP Request</h3>
                <p>Dear {adminName},</p>
                <p>A user has requested authorization to download a report and requires your approval:</p>
                
                <table style='border: 1px solid #ddd; border-collapse: collapse; width: 100%; max-width: 600px; margin: 20px 0;'>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 12px; border: 1px solid #ddd; width: 40%;'><strong>Requesting User:</strong></td>
                        <td style='padding: 12px; border: 1px solid #ddd;'>{requestingUserName} (ID: {requestingUserId})</td>
                    </tr>
                    <tr>
                        <td style='padding: 12px; border: 1px solid #ddd;'><strong>Report Type:</strong></td>
                        <td style='padding: 12px; border: 1px solid #ddd;'><span style='background-color: #e7e9fd; color: #6f42c1; padding: 4px 12px; border-radius: 4px;'>{reportType}</span></td>
                    </tr>
                    <tr style='background-color: #f8f9fa;'>
                        <td style='padding: 12px; border: 1px solid #ddd;'><strong>Request Time:</strong></td>
                        <td style='padding: 12px; border: 1px solid #ddd;'>{DateTime.Now:dd-MMM-yyyy hh:mm:ss tt}</td>
                    </tr>
                </table>

                <div style='background-color: #fff3cd; border: 2px solid #ffc107; border-radius: 8px; padding: 20px; margin: 20px 0; text-align: center;'>
                    <p style='margin: 0 0 10px 0; font-size: 14px; color: #856404;'><strong>One-Time Password (OTP)</strong></p>
                    <h1 style='margin: 0; font-size: 48px; letter-spacing: 12px; color: #6f42c1; font-family: monospace;'>{otp}</h1>
                    <p style='margin: 15px 0 0 0; font-size: 12px; color: #856404;'>
                        <i>⏱️ Expires at: <strong>{expiresAt:hh:mm:ss tt}</strong> (in 10 minutes)</i>
                    </p>
                </div>

                <p style='padding: 15px; background-color: #e8f4fd; border-left: 4px solid #007bff; margin: 20px 0;'>
                    <strong>Instructions:</strong><br/>
                    Share this OTP with <strong>{requestingUserName}</strong> if you approve their request to download the report. 
                    The OTP will expire in 10 minutes and can only be used once.
                </p>

                <p style='margin-top: 20px;'>Best Regards,<br/>Survey Management System</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                <p style='font-size: 12px; color: #666;'>
                    <strong>Note:</strong> This is an automated security notification. Please do not reply to this message.
                    If you did not expect this request, please contact IT immediately.
                </p>
            </div>";

        return await SendEmailAsync(adminEmail, subject, body);
    }

    private async Task<bool> SendEmailAsync(string toEmails, string subject, string htmlBody)
    {
        try
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, "Support"),
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