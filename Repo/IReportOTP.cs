using SurveyApp.Models;

namespace SurveyApp.Repo
{
    /// <summary>
    /// Interface for Report OTP operations
    /// </summary>
    public interface IReportOTP
    {
        /// <summary>
        /// Generate a new OTP for report download
        /// </summary>
        OTPResponseModel GenerateOTP(int userId, string userName, string reportType, string? reportParameters, string? ipAddress, string? userAgent);

        /// <summary>
        /// Validate an OTP entered by the user
        /// </summary>
        OTPResponseModel ValidateOTP(int userId, string otp);

        /// <summary>
        /// Check if user has a valid (validated) OTP for download
        /// </summary>
        bool HasValidOTP(int userId);

        /// <summary>
        /// Mark download as completed
        /// </summary>
        bool MarkDownloadCompleted(long logId);

        /// <summary>
        /// Get OTP log history for audit purposes
        /// </summary>
        List<ReportOTPModel> GetOTPHistory(int? userId = null);

        /// <summary>
        /// Get all pending OTPs (for Super Admin view)
        /// </summary>
        List<ReportOTPModel> GetPendingOTPs();

        /// <summary>
        /// Expire old pending OTPs (cleanup job)
        /// </summary>
        int ExpireOldOTPs();

        /// <summary>
        /// Check if user is a Super Admin (RoleId = 101)
        /// </summary>
        bool IsSuperAdmin(int roleId);
    }
}
