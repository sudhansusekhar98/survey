using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using System.Text.Json;

namespace SurveyApp.Controllers
{
    /// <summary>
    /// Controller for Report OTP Access Control
    /// Handles OTP generation, validation, and audit logging for report downloads
    /// </summary>
    public class ReportOTPController : Controller
    {
        private readonly IReportOTP _otpRepo;
        private readonly IAdmin _adminRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReportOTPController> _logger;
        private const int SUPER_ADMIN_ROLE_ID = 101;

        public ReportOTPController(IReportOTP otpRepo, IAdmin adminRepo, IEmailService emailService, ILogger<ReportOTPController> logger)
        {
            _otpRepo = otpRepo;
            _adminRepo = adminRepo;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Check if current user requires OTP for report download
        /// </summary>
        [HttpGet]
        public IActionResult CheckOTPRequired()
        {
            int roleId = GetCurrentRoleId();
            bool isSuperAdmin = _otpRepo.IsSuperAdmin(roleId);

            return Json(new
            {
                requiresOTP = !isSuperAdmin,
                isSuperAdmin = isSuperAdmin,
                message = isSuperAdmin ? "Super Admin - no OTP required" : "OTP verification required for report download"
            });
        }

        /// <summary>
        /// Request OTP for report download
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RequestOTP([FromBody] OTPRequestModel? request)
        {
            try
            {
                int userId = GetCurrentUserId();
                int roleId = GetCurrentRoleId();
                string userName = GetCurrentUserName();

                // Super Admins don't need OTP
                if (_otpRepo.IsSuperAdmin(roleId))
                {
                    return Json(new OTPResponseModel
                    {
                        Success = true,
                        RequiresOTP = false,
                        Message = "Super Admin - no OTP required"
                    });
                }

                // Handle null request - create default object
                if (request == null)
                {
                    request = new OTPRequestModel { ReportType = "Unknown" };
                }

                // Get client info for logging
                string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string? userAgent = Request.Headers["User-Agent"].ToString();

                // Serialize report parameters
                string? reportParameters = JsonSerializer.Serialize(new
                {
                    surveyId = request.SurveyId,
                    fromDate = request.FromDate,
                    toDate = request.ToDate,
                    status = request.Status,
                    region = request.Region,
                    type = request.Type
                });

                // Ensure report type is not null
                string reportType = request.ReportType ?? "Report";

                // Generate OTP
                var result = _otpRepo.GenerateOTP(
                    userId,
                    userName,
                    reportType,
                    reportParameters,
                    ipAddress,
                    userAgent
                );

                if (result.Success)
                {
                    // Send OTP notification to Super Admins via email
                    await NotifySuperAdminsAsync(userId, userName, reportType, result.OTP!, result.ExpiresAt!.Value);
                }

                // Don't send the actual OTP in the response - it should only go to Super Admins
                return Json(new OTPResponseModel
                {
                    Success = result.Success,
                    LogId = result.LogId,
                    ExpiresAt = result.ExpiresAt,
                    RequiresOTP = true,
                    Message = result.Success
                        ? "OTP has been sent to the Super Admin via email. Please contact them to get the OTP."
                        : result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting OTP");
                return Json(new OTPResponseModel
                {
                    Success = false,
                    Message = $"Error requesting OTP: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Validate OTP entered by user
        /// </summary>
        [HttpPost]
        public IActionResult ValidateOTP([FromBody] OTPValidationModel request)
        {
            try
            {
                int userId = GetCurrentUserId();
                int roleId = GetCurrentRoleId();

                // Super Admins don't need OTP validation
                if (_otpRepo.IsSuperAdmin(roleId))
                {
                    return Json(new OTPResponseModel
                    {
                        Success = true,
                        RequiresOTP = false,
                        Message = "Super Admin - no OTP required"
                    });
                }

                if (string.IsNullOrEmpty(request.OTP) || request.OTP.Length != 6)
                {
                    return Json(new OTPResponseModel
                    {
                        Success = false,
                        Message = "Please enter a valid 6-digit OTP"
                    });
                }

                var result = _otpRepo.ValidateOTP(userId, request.OTP);

                return Json(new OTPResponseModel
                {
                    Success = result.Success,
                    LogId = result.LogId,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP");
                return Json(new OTPResponseModel
                {
                    Success = false,
                    Message = $"Error validating OTP: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Check if user has valid OTP (already validated)
        /// </summary>
        [HttpGet]
        public IActionResult HasValidOTP()
        {
            int userId = GetCurrentUserId();
            int roleId = GetCurrentRoleId();

            // Super Admins always have access
            if (_otpRepo.IsSuperAdmin(roleId))
            {
                return Json(new { hasValidOTP = true, isSuperAdmin = true });
            }

            bool hasValid = _otpRepo.HasValidOTP(userId);
            return Json(new { hasValidOTP = hasValid, isSuperAdmin = false });
        }

        /// <summary>
        /// Get pending OTPs - Only for Super Admins
        /// </summary>
        [HttpGet]
        public IActionResult GetPendingOTPs()
        {
            int roleId = GetCurrentRoleId();

            if (!_otpRepo.IsSuperAdmin(roleId))
            {
                return Forbid();
            }

            var pendingOTPs = _otpRepo.GetPendingOTPs();
            return Json(pendingOTPs);
        }

        /// <summary>
        /// Get OTP audit log - Only for Super Admins
        /// </summary>
        [HttpGet]
        public IActionResult GetOTPHistory(int? userId = null)
        {
            int roleId = GetCurrentRoleId();

            if (!_otpRepo.IsSuperAdmin(roleId))
            {
                return Forbid();
            }

            var history = _otpRepo.GetOTPHistory(userId);
            return Json(history);
        }

        /// <summary>
        /// OTP Audit Log View - Only for Super Admins
        /// </summary>
        public IActionResult AuditLog()
        {
            int roleId = GetCurrentRoleId();

            if (!_otpRepo.IsSuperAdmin(roleId))
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> Access denied. Super Admin privileges required.";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index", "Dashboard");
            }

            var history = _otpRepo.GetOTPHistory();
            return View(history);
        }

        /// <summary>
        /// Pending OTPs Dashboard - Only for Super Admins
        /// </summary>
        public IActionResult PendingRequests()
        {
            int roleId = GetCurrentRoleId();

            if (!_otpRepo.IsSuperAdmin(roleId))
            {
                TempData["ResultMessage"] = "<strong>Error!</strong> Access denied. Super Admin privileges required.";
                TempData["ResultType"] = "danger";
                return RedirectToAction("Index", "Dashboard");
            }

            var pendingOTPs = _otpRepo.GetPendingOTPs();
            return View(pendingOTPs);
        }

        #region Private Helper Methods

        private int GetCurrentUserId()
        {
            return Convert.ToInt32(HttpContext.Session.GetString("UserID") ?? "0");
        }

        private int GetCurrentRoleId()
        {
            return Convert.ToInt32(HttpContext.Session.GetString("RoleId") ?? "102");
        }

        private string GetCurrentUserName()
        {
            return HttpContext.Session.GetString("UserName") ?? "Unknown";
        }

        /// <summary>
        /// Send OTP notification to all Super Admins via email
        /// </summary>
        private async Task NotifySuperAdminsAsync(int requestingUserId, string requestingUserName, string reportType, string otp, DateTime expiresAt)
        {
            try
            {
                // Get all Super Admins
                var superAdmins = _adminRepo.GetSuperUsers();

                if (superAdmins == null || !superAdmins.Any())
                {
                    _logger.LogWarning("No Super Admins found to send OTP notification");
                    return;
                }

                // Send email to each Super Admin
                var emailTasks = new List<Task<bool>>();
                foreach (var admin in superAdmins)
                {
                    if (!string.IsNullOrEmpty(admin.EmailID))
                    {
                        _logger.LogInformation(
                            "Sending OTP notification to {AdminName} ({AdminEmail}) for user {UserName} (ID: {UserId})",
                            admin.LoginName, admin.EmailID, requestingUserName, requestingUserId
                        );

                        var task = _emailService.SendReportOTPNotificationAsync(
                            admin.LoginName ?? "Administrator",
                            admin.EmailID,
                            requestingUserName,
                            requestingUserId,
                            reportType,
                            otp,
                            expiresAt
                        );
                        emailTasks.Add(task);
                    }
                    else
                    {
                        _logger.LogWarning("Super Admin {AdminName} (ID: {AdminId}) has no email configured", admin.LoginName, admin.UserId);
                    }
                }

                // Wait for all emails to be sent
                if (emailTasks.Any())
                {
                    var results = await Task.WhenAll(emailTasks);
                    int successCount = results.Count(r => r);
                    int failCount = results.Count(r => !r);
                    
                    _logger.LogInformation("OTP notification sent: {SuccessCount} success, {FailCount} failed", successCount, failCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP notification to Super Admins");
            }
        }

        #endregion
    }
}

