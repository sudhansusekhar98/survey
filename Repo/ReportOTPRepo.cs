using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using AnalyticaDocs.Util;
using System.Data;

namespace SurveyApp.Repo
{
    /// <summary>
    /// Repository implementation for Report OTP operations
    /// </summary>
    public class ReportOTPRepo : IReportOTP
    {
        private const int SUPER_ADMIN_ROLE_ID = 101;
        private const int OTP_EXPIRY_MINUTES = 10;

        // Generate a new OTP for report download
        public OTPResponseModel GenerateOTP(int userId, string userName, string reportType, string? reportParameters, string? ipAddress, string? userAgent)
        {
            try
            {
                // Generate 6-digit OTP
                string otp = GenerateRandomOTP();

                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("sp_ReportOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Type", 1); // Generate OTP
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@UserName", userName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ReportType", reportType);
                cmd.Parameters.AddWithValue("@ReportParameters", reportParameters ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@OTP", otp);
                cmd.Parameters.AddWithValue("@OTPExpiryMinutes", OTP_EXPIRY_MINUTES);
                cmd.Parameters.AddWithValue("@IPAddress", ipAddress ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@UserAgent", userAgent ?? (object)DBNull.Value);

                con.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    long logId = Convert.ToInt64(reader["LogId"]);
                    return new OTPResponseModel
                    {
                        Success = true,
                        LogId = logId,
                        OTP = otp,
                        ExpiresAt = DateTime.Now.AddMinutes(OTP_EXPIRY_MINUTES),
                        Message = "OTP generated successfully. Please contact a Super Admin to get the OTP.",
                        RequiresOTP = true
                    };
                }

                return new OTPResponseModel
                {
                    Success = false,
                    Message = "Failed to generate OTP. Please try again."
                };
            }
            catch (Exception ex)
            {
                return new OTPResponseModel
                {
                    Success = false,
                    Message = $"Error generating OTP: {ex.Message}"
                };
            }
        }

        // Validate an OTP entered by the user
        public OTPResponseModel ValidateOTP(int userId, string otp)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("sp_ReportOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Type", 2); // Validate OTP
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@OTP", otp);

                con.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    bool isValid = Convert.ToBoolean(reader["IsValid"]);
                    string message = reader["Message"]?.ToString() ?? "";
                    long? logId = reader["LogId"] != DBNull.Value ? Convert.ToInt64(reader["LogId"]) : null;

                    return new OTPResponseModel
                    {
                        Success = isValid,
                        LogId = logId,
                        Message = message
                    };
                }

                return new OTPResponseModel
                {
                    Success = false,
                    Message = "OTP validation failed. Please try again."
                };
            }
            catch (Exception ex)
            {
                return new OTPResponseModel
                {
                    Success = false,
                    Message = $"Error validating OTP: {ex.Message}"
                };
            }
        }

        // Check if user has a valid (validated) OTP for download
        public bool HasValidOTP(int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("sp_ReportOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Type", 5); // Get validated OTP
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using var reader = cmd.ExecuteReader();

                return reader.Read(); // If a record is returned, user has valid OTP
            }
            catch
            {
                return false;
            }
        }

        /// Mark download as completed
        public bool MarkDownloadCompleted(long logId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("sp_ReportOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Type", 3); // Mark completed
                cmd.Parameters.AddWithValue("@LogId", logId);

                con.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return Convert.ToBoolean(reader["Success"]);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Get OTP log history for audit purposes
        public List<ReportOTPModel> GetOTPHistory(int? userId = null)
        {
            var history = new List<ReportOTPModel>();

            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("sp_ReportOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Type", 4); // Get history
                cmd.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);

                con.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    history.Add(new ReportOTPModel
                    {
                        LogId = Convert.ToInt64(reader["LogId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        UserName = reader["UserName"]?.ToString(),
                        ReportType = reader["ReportType"]?.ToString() ?? "",
                        ReportParameters = reader["ReportParameters"]?.ToString(),
                        OTPGeneratedAt = Convert.ToDateTime(reader["OTPGeneratedAt"]),
                        OTPExpiresAt = Convert.ToDateTime(reader["OTPExpiresAt"]),
                        OTPStatus = reader["OTPStatus"]?.ToString() ?? "",
                        ValidatedAt = reader["ValidatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["ValidatedAt"]) : null,
                        DownloadedAt = reader["DownloadedAt"] != DBNull.Value ? Convert.ToDateTime(reader["DownloadedAt"]) : null,
                        IPAddress = reader["IPAddress"]?.ToString(),
                        CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                    });
                }
            }
            catch
            {
                // Return empty list on error
            }

            return history;
        }

        /// Get all pending OTPs (for Super Admin view)
        public List<ReportOTPModel> GetPendingOTPs()
        {
            var pending = new List<ReportOTPModel>();

            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("sp_ReportOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Type", 7); // Get pending OTPs

                con.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    pending.Add(new ReportOTPModel
                    {
                        LogId = Convert.ToInt64(reader["LogId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        UserName = reader["UserName"]?.ToString(),
                        ReportType = reader["ReportType"]?.ToString() ?? "",
                        ReportParameters = reader["ReportParameters"]?.ToString(),
                        OTP = reader["OTP"]?.ToString() ?? "",
                        OTPGeneratedAt = Convert.ToDateTime(reader["OTPGeneratedAt"]),
                        OTPExpiresAt = Convert.ToDateTime(reader["OTPExpiresAt"]),
                        OTPStatus = reader["OTPStatus"]?.ToString() ?? "",
                        IPAddress = reader["IPAddress"]?.ToString()
                    });
                }
            }
            catch
            {
                // Return empty list on error
            }

            return pending;
        }

        // Expire old pending OTPs (cleanup job)
        public int ExpireOldOTPs()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("sp_ReportOTP", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Type", 6); // Expire old OTPs

                con.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return Convert.ToInt32(reader["ExpiredCount"]);
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        // Check if user is a Super Admin (RoleId = 101)
        public bool IsSuperAdmin(int roleId)
        {
            return roleId == SUPER_ADMIN_ROLE_ID;
        }

        // Generate a random 6-digit OTP
        private string GenerateRandomOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
