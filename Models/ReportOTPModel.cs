using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SurveyApp.Models
{
    /// <summary>
    /// Model for Report OTP Log entries
    /// </summary>
    public class ReportOTPModel
    {
        public long LogId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string? ReportParameters { get; set; }
        public string OTP { get; set; } = string.Empty;
        public DateTime OTPGeneratedAt { get; set; }
        public DateTime OTPExpiresAt { get; set; }
        public string OTPStatus { get; set; } = "Pending"; // Pending, Validated, Expired, Cancelled
        public DateTime? ValidatedAt { get; set; }
        public int? ValidatedBy { get; set; }
        public DateTime? DownloadedAt { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    /// <summary>
    /// ViewModel for OTP Request
    /// </summary>
    public class OTPRequestModel
    {
        [JsonPropertyName("reportType")]
        public string ReportType { get; set; } = string.Empty;
        
        [JsonPropertyName("reportParameters")]
        public string? ReportParameters { get; set; }
        
        [JsonPropertyName("surveyId")]
        public long? SurveyId { get; set; }
        
        [JsonPropertyName("fromDate")]
        public DateTime? FromDate { get; set; }
        
        [JsonPropertyName("toDate")]
        public DateTime? ToDate { get; set; }
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
        [JsonPropertyName("region")]
        public string? Region { get; set; }
        
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    /// <summary>
    /// ViewModel for OTP Validation
    /// </summary>
    public class OTPValidationModel
    {
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [JsonPropertyName("otp")]
        public string OTP { get; set; } = string.Empty;
        
        [JsonPropertyName("reportType")]
        public string ReportType { get; set; } = string.Empty;
        
        [JsonPropertyName("reportParameters")]
        public string? ReportParameters { get; set; }
    }

    /// <summary>
    /// Response model for OTP operations
    /// </summary>
    public class OTPResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long? LogId { get; set; }
        public string? OTP { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool RequiresOTP { get; set; }
    }

    /// <summary>
    /// Enum for Report Types
    /// </summary>
    public static class ReportTypes
    {
        public const string Summary = "SummaryReport";
        public const string Detailed = "DetailedReport";
        public const string DetailedNew = "DetailedReportNew";
        public const string Excel = "ExcelExport";
    }
}
