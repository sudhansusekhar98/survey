using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;

namespace SurveyApp.Controllers
{
    public class SurveySubmissionController : Controller
    {
        private readonly ISurveySubmission _submissionRepo;
        private readonly ISurvey _surveyRepo;
        private readonly IAdmin _adminRepo;
        private readonly IEmpMaster _empRepo;
        private readonly IEmailService _emailService;

        public SurveySubmissionController(
            ISurveySubmission submissionRepo,
            ISurvey surveyRepo,
            IAdmin adminRepo,
            IEmpMaster empRepo,
            IEmailService emailService)
        {
            _submissionRepo = submissionRepo;
            _surveyRepo = surveyRepo;
            _adminRepo = adminRepo;
            _empRepo = empRepo;
            _emailService = emailService;
        }

        /// <summary>
        /// Display all submissions for review (Supervisor view)
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var roleId = HttpContext.Session.GetString("RoleId");
                
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Index", "UserLogin");
                }

                int currentUserId = int.Parse(userId);
                int currentRoleId = int.Parse(roleId ?? "101");
                
                // Only allow super admin (RoleId 100) or survey creators to access this page
                // Get pending submissions for surveys created by this user
                var submissions = currentRoleId == 100 
                    ? _submissionRepo.GetPendingSubmissionsForReview(null) // Super admin sees all
                    : _submissionRepo.GetPendingSubmissionsForReview(currentUserId); // Creator sees only their surveys

                return View(submissions);
            }
            catch (Exception ex)
            {
                TempData["ResultType"] = "error";
                TempData["ResultMessage"] = $"<div class='alert alert-danger'>Error loading submissions: {ex.Message}</div>";
                return View(new List<SurveySubmissionModel>());
            }
        }

        /// <summary>
        /// Display all submitted surveys by current user
        /// </summary>
        public IActionResult MySubmissions()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Index", "UserLogin");
                }

                int currentUserId = int.Parse(userId);
                var submissions = _submissionRepo.GetAllSubmissions(currentUserId);

                return View(submissions);
            }
            catch (Exception ex)
            {
                TempData["ResultType"] = "error";
                TempData["ResultMessage"] = $"<div class='alert alert-danger'>Error loading submissions: {ex.Message}</div>";
                return View(new List<SurveySubmissionModel>());
            }
        }

        /// <summary>
        /// Submit survey for approval (called from SurveyDetails)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitForApproval(Int64 surveyId)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userName = HttpContext.Session.GetString("UserName");
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not logged in" });
                }

                int currentUserId = int.Parse(userId);

                // Submit survey
                bool result = _submissionRepo.SubmitSurvey(surveyId, currentUserId, "Submitted");

                if (result)
                {
                    // Get survey details
                    var survey = _surveyRepo.GetSurveyById(surveyId);
                    
                    if (survey != null)
                    {
                        // Get supervisor (survey creator) details
                        var supervisorId = _submissionRepo.GetSurveyCreatorId(surveyId);
                        
                        if (supervisorId.HasValue)
                        {
                            var supervisorUser = _adminRepo.GetUserById(supervisorId.Value);
                            
                            if (supervisorUser != null && !string.IsNullOrEmpty(supervisorUser.EmailID))
                            {
                                // Send email notification to supervisor
                                await _emailService.SendSurveySubmissionNotificationAsync(
                                    supervisorUser.LoginName ?? "Supervisor",
                                    supervisorUser.EmailID,
                                    survey.SurveyName ?? "Survey",
                                    userName ?? "User",
                                    DateTime.Now
                                );
                            }
                        }
                    }

                    return Json(new { success = true, message = "Survey submitted successfully for approval" });
                }

                return Json(new { success = false, message = "Failed to submit survey" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Approve survey submission
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Approve(Int64 submissionId, string? reviewComments)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userName = HttpContext.Session.GetString("UserName");
                var roleId = HttpContext.Session.GetString("RoleId");
                
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ResultType"] = "error";
                    TempData["ResultMessage"] = "<div class='alert alert-danger'>User not logged in</div>";
                    return RedirectToAction("Index");
                }

                int currentUserId = int.Parse(userId);
                int currentRoleId = int.Parse(roleId ?? "101");
                
                // Verify authorization: get submission and check if user is survey creator or super admin
                var submission = _submissionRepo.GetAllSubmissions()
                    .FirstOrDefault(s => s.SubmissionId == submissionId);
                    
                if (submission != null)
                {
                    var surveyCreatorId = _submissionRepo.GetSurveyCreatorId(submission.SurveyId);
                    
                    // Check if user is authorized (super admin or survey creator)
                    if (currentRoleId != 100 && (!surveyCreatorId.HasValue || surveyCreatorId.Value != currentUserId))
                    {
                        TempData["ResultType"] = "error";
                        TempData["ResultMessage"] = "<div class='alert alert-danger'><i class='bi bi-shield-exclamation'></i> <strong>Unauthorized!</strong> You are not authorized to approve this survey.</div>";
                        return RedirectToAction("Index");
                    }
                }

                // Update review status to Approved
                bool result = _submissionRepo.UpdateReviewStatus(submissionId, "Approved", currentUserId, reviewComments);

                if (result)
                {
                    // Get submission details for email
                    var approvedSubmission = _submissionRepo.GetAllSubmissions()
                        .FirstOrDefault(s => s.SubmissionId == submissionId);

                    if (approvedSubmission != null && approvedSubmission.SubmittedBy.HasValue)
                    {
                        var submitterUser = _adminRepo.GetUserById(approvedSubmission.SubmittedBy.Value);
                        
                        if (submitterUser != null && !string.IsNullOrEmpty(submitterUser.EmailID))
                        {
                            // Send approval email to submitter
                            await _emailService.SendSurveyApprovalNotificationAsync(
                                submitterUser.LoginName ?? "User",
                                submitterUser.EmailID,
                                approvedSubmission.SurveyName ?? "Survey",
                                userName ?? "Supervisor",
                                reviewComments ?? ""
                            );
                        }
                    }

                    TempData["ResultType"] = "success";
                    TempData["ResultMessage"] = "<div class='alert alert-success'>Survey approved successfully and notification sent</div>";
                }
                else
                {
                    TempData["ResultType"] = "error";
                    TempData["ResultMessage"] = "<div class='alert alert-danger'>Failed to approve survey</div>";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ResultType"] = "error";
                TempData["ResultMessage"] = $"<div class='alert alert-danger'>Error: {ex.Message}</div>";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Reject survey submission
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Reject(Int64 submissionId, string? rejectionReason)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserID");
                var userName = HttpContext.Session.GetString("UserName");
                var roleId = HttpContext.Session.GetString("RoleId");
                
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ResultType"] = "error";
                    TempData["ResultMessage"] = "<div class='alert alert-danger'>User not logged in</div>";
                    return RedirectToAction("Index");
                }
                
                int currentUserId = int.Parse(userId);
                int currentRoleId = int.Parse(roleId ?? "101");
                
                // Verify authorization: get submission and check if user is survey creator or super admin
                var submission = _submissionRepo.GetAllSubmissions()
                    .FirstOrDefault(s => s.SubmissionId == submissionId);
                    
                if (submission != null)
                {
                    var surveyCreatorId = _submissionRepo.GetSurveyCreatorId(submission.SurveyId);
                    
                    // Check if user is authorized (super admin or survey creator)
                    if (currentRoleId != 100 && (!surveyCreatorId.HasValue || surveyCreatorId.Value != currentUserId))
                    {
                        TempData["ResultType"] = "error";
                        TempData["ResultMessage"] = "<div class='alert alert-danger'><i class='bi bi-shield-exclamation'></i> <strong>Unauthorized!</strong> You are not authorized to reject this survey.</div>";
                        return RedirectToAction("Index");
                    }
                }

                if (string.IsNullOrEmpty(rejectionReason))
                {
                    TempData["ResultType"] = "error";
                    TempData["ResultMessage"] = "<div class='alert alert-danger'>Rejection reason is required</div>";
                    return RedirectToAction("Index");
                }

                // Update review status to Rejected
                bool result = _submissionRepo.UpdateReviewStatus(submissionId, "Rejected", currentUserId, rejectionReason);

                if (result)
                {
                    // Get submission details for email
                    var rejectedSubmission = _submissionRepo.GetAllSubmissions()
                        .FirstOrDefault(s => s.SubmissionId == submissionId);

                    if (rejectedSubmission != null && rejectedSubmission.SubmittedBy.HasValue)
                    {
                        var submitterUser = _adminRepo.GetUserById(rejectedSubmission.SubmittedBy.Value);
                        
                        if (submitterUser != null && !string.IsNullOrEmpty(submitterUser.EmailID))
                        {
                            // Send rejection email to submitter
                            await _emailService.SendSurveyRejectionNotificationAsync(
                                submitterUser.LoginName ?? "User",
                                submitterUser.EmailID,
                                rejectedSubmission.SurveyName ?? "Survey",
                                userName ?? "Supervisor",
                                rejectionReason
                            );
                        }
                    }

                    TempData["ResultType"] = "success";
                    TempData["ResultMessage"] = "<div class='alert alert-success'>Survey rejected and notification sent to submitter for corrections</div>";
                }
                else
                {
                    TempData["ResultType"] = "error";
                    TempData["ResultMessage"] = "<div class='alert alert-danger'>Failed to reject survey</div>";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ResultType"] = "error";
                TempData["ResultMessage"] = $"<div class='alert alert-danger'>Error: {ex.Message}</div>";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Get submission details (for modal/AJAX)
        /// </summary>
        [HttpGet]
        public IActionResult GetSubmissionDetails(Int64 surveyId)
        {
            try
            {
                var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);
                
                if (submission == null)
                {
                    return Json(new { success = false, message = "Submission not found" });
                }

                return Json(new 
                { 
                    success = true,
                    data = new
                    {
                        submissionId = submission.SubmissionId,
                        surveyId = submission.SurveyId,
                        surveyName = submission.SurveyName,
                        submittedBy = submission.SubmittedByName,
                        submissionDate = submission.SubmissionDate?.ToString("dd-MMM-yyyy hh:mm tt"),
                        status = submission.SubmissionStatus,
                        reviewComments = submission.ReviewComments
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
