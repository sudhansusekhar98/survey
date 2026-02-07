using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Models.Api;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;
using AnalyticaDocs.Repository;
using System.Security.Claims;

namespace SurveyApp.Controllers.Api
{
    /// <summary>
    /// Survey Submission API Controller - Submit, approve, reject surveys
    /// </summary>
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    public class SubmissionApiController : ControllerBase
    {
        private readonly ISurvey _surveyRepo;
        private readonly ISurveySubmission _submissionRepo;
        private readonly IAdmin _adminRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<SubmissionApiController> _logger;

        public SubmissionApiController(
            ISurvey surveyRepo,
            ISurveySubmission submissionRepo,
            IAdmin adminRepo,
            IEmailService emailService,
            ILogger<SubmissionApiController> logger)
        {
            _surveyRepo = surveyRepo;
            _submissionRepo = submissionRepo;
            _adminRepo = adminRepo;
            _emailService = emailService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        private int GetCurrentRoleId()
        {
            var claim = User.FindFirst("RoleId");
            return claim != null && int.TryParse(claim.Value, out int roleId) ? roleId : 0;
        }

        #region Submissions

        /// <summary>
        /// Get all pending submissions for review (supervisors)
        /// </summary>
        [HttpGet("submissions/pending")]
        [ProducesResponseType(typeof(ApiResponse<List<SurveySubmissionDto>>), 200)]
        public IActionResult GetPendingSubmissions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var roleId = GetCurrentRoleId();
                var empId = _adminRepo.GetEmpIdByUserId(userId);

                var submissions = _submissionRepo.GetPendingSubmissionsForUser(userId, roleId, empId);

                var dtos = submissions.Select(s => new SurveySubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    SurveyId = s.SurveyId,
                    SurveyName = s.SurveyName,
                    Status = s.SubmissionStatus,
                    SubmittedBy = s.SubmittedBy,
                    SubmittedByName = s.SubmittedByName,
                    SubmissionDate = s.SubmissionDate,
                    ReviewedBy = s.ReviewedBy,
                    ReviewedByName = s.ReviewedByName,
                    ReviewDate = s.ReviewDate,
                    ReviewComments = s.ReviewComments,
                    CanEdit = s.CanEdit,
                    CanReview = _submissionRepo.CanUserReviewSubmission(s.SurveyId, userId, roleId)
                }).ToList();

                return Ok(ApiResponse<List<SurveySubmissionDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending submissions");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get my submissions (status tracking)
        /// </summary>
        [HttpGet("submissions/my")]
        [ProducesResponseType(typeof(ApiResponse<List<SurveySubmissionDto>>), 200)]
        public IActionResult GetMySubmissions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var submissions = _submissionRepo.GetAllSubmissions(userId);

                var dtos = submissions.Select(s => new SurveySubmissionDto
                {
                    SubmissionId = s.SubmissionId,
                    SurveyId = s.SurveyId,
                    SurveyName = s.SurveyName,
                    Status = s.SubmissionStatus,
                    SubmittedBy = s.SubmittedBy,
                    SubmittedByName = s.SubmittedByName,
                    SubmissionDate = s.SubmissionDate,
                    ReviewedBy = s.ReviewedBy,
                    ReviewedByName = s.ReviewedByName,
                    ReviewDate = s.ReviewDate,
                    ReviewComments = s.ReviewComments,
                    CanEdit = s.CanEdit,
                    CanReview = false
                }).ToList();

                return Ok(ApiResponse<List<SurveySubmissionDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting my submissions");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Submit a survey for approval
        /// </summary>
        [HttpPost("surveys/{surveyId}/submit")]
        [ProducesResponseType(typeof(ApiResponse<SurveySubmissionDto>), 200)]
        public async Task<IActionResult> SubmitSurvey(long surveyId, [FromBody] SubmitSurveyRequest? request)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                // Check if survey is complete
                var completionStatus = _surveyRepo.CheckSurveyCompletionStatus(surveyId);
                if (!completionStatus.IsComplete)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        "Survey is not complete",
                        new List<string> { completionStatus.Message }));
                }

                // Check if already submitted
                var existingSubmission = _submissionRepo.GetSubmissionBySurveyId(surveyId);
                if (existingSubmission != null && existingSubmission.SubmissionStatus == "Submitted")
                {
                    return BadRequest(ApiResponse<object>.Fail("Survey is already submitted and pending review"));
                }

                var userId = GetCurrentUserId();
                var currentUser = _adminRepo.GetUserById(userId);
                var submittedByName = currentUser?.LoginName ?? "Unknown";

                // Submit the survey
                var result = _submissionRepo.SubmitSurvey(surveyId, userId, "Submitted");
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to submit survey"));
                }

                // Update survey status
                _surveyRepo.UpdateSurveyStatus(surveyId, "Submitted");

                // Send notification emails to super admins
                try
                {
                    var superAdmins = _adminRepo.GetSuperUsers();
                    foreach (var admin in superAdmins.Where(a => !string.IsNullOrEmpty(a.EmailID)))
                    {
                        await _emailService.SendSurveySubmissionNotificationAsync(
                            admin.LoginName,
                            admin.EmailID!,
                            survey.SurveyName,
                            submittedByName,
                            DateTime.Now);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send submission notification emails");
                }

                // Get updated submission
                var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);

                _logger.LogInformation("Survey {SurveyId} submitted by user {UserId}", surveyId, userId);

                var dto = new SurveySubmissionDto
                {
                    SubmissionId = submission?.SubmissionId ?? 0,
                    SurveyId = surveyId,
                    SurveyName = survey.SurveyName,
                    Status = "Submitted",
                    SubmittedBy = userId,
                    SubmittedByName = submittedByName,
                    SubmissionDate = DateTime.Now,
                    CanEdit = false,
                    CanReview = false
                };

                return Ok(ApiResponse<SurveySubmissionDto>.Ok(dto, "Survey submitted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Approve or reject a submission
        /// </summary>
        [HttpPost("submissions/{submissionId}/review")]
        [ProducesResponseType(typeof(ApiResponse<SurveySubmissionDto>), 200)]
        public async Task<IActionResult> ReviewSubmission(long submissionId, [FromBody] ReviewRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail("Validation failed"));
                }

                // Get submission
                var submissions = _submissionRepo.GetAllSubmissions();
                var submission = submissions.FirstOrDefault(s => s.SubmissionId == submissionId);
                
                if (submission == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Submission not found"));
                }

                var userId = GetCurrentUserId();
                var roleId = GetCurrentRoleId();
                var currentUser = _adminRepo.GetUserById(userId);
                var reviewerName = currentUser?.LoginName ?? "Unknown";

                // Check if user can review
                if (!_submissionRepo.CanUserReviewSubmission(submission.SurveyId, userId, roleId))
                {
                    return Forbid();
                }

                var action = request.Action.ToLower();
                string newStatus;
                string surveyStatus;

                if (action == "approve")
                {
                    newStatus = "Approved";
                    surveyStatus = "Completed";
                }
                else if (action == "reject")
                {
                    newStatus = "Rejected";
                    surveyStatus = "In Progress";
                }
                else
                {
                    return BadRequest(ApiResponse<object>.Fail("Invalid action. Use 'Approve' or 'Reject'"));
                }

                // Update submission status
                var result = _submissionRepo.UpdateReviewStatus(submissionId, newStatus, userId, request.Comments);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail($"Failed to {action.ToLower()} submission"));
                }

                // Update survey status
                _surveyRepo.UpdateSurveyStatus(submission.SurveyId, surveyStatus);

                // Send notification email
                try
                {
                    if (submission.SubmittedBy.HasValue)
                    {
                        var submitter = _adminRepo.GetUserById(submission.SubmittedBy.Value);
                        var survey = _surveyRepo.GetSurveyById(submission.SurveyId);
                        
                        if (submitter != null && !string.IsNullOrEmpty(submitter.EmailID) && survey != null)
                        {
                            if (action == "approve")
                            {
                                await _emailService.SendSurveyApprovalNotificationAsync(
                                    submitter.LoginName,
                                    submitter.EmailID,
                                    survey.SurveyName,
                                    reviewerName,
                                    request.Comments ?? "");
                            }
                            else
                            {
                                await _emailService.SendSurveyRejectionNotificationAsync(
                                    submitter.LoginName,
                                    submitter.EmailID,
                                    survey.SurveyName,
                                    reviewerName,
                                    request.Comments ?? "No reason provided");
                            }
                        }
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send review notification email");
                }

                _logger.LogInformation("Submission {SubmissionId} {Action}ed by user {UserId}", 
                    submissionId, action, userId);

                // Get updated submission
                var updated = _submissionRepo.GetSubmissionBySurveyId(submission.SurveyId);

                var dto = new SurveySubmissionDto
                {
                    SubmissionId = updated?.SubmissionId ?? submissionId,
                    SurveyId = submission.SurveyId,
                    SurveyName = submission.SurveyName,
                    Status = newStatus,
                    SubmittedBy = submission.SubmittedBy,
                    SubmittedByName = submission.SubmittedByName,
                    SubmissionDate = submission.SubmissionDate,
                    ReviewedBy = userId,
                    ReviewedByName = reviewerName,
                    ReviewDate = DateTime.Now,
                    ReviewComments = request.Comments,
                    CanEdit = action == "reject",
                    CanReview = false
                };

                return Ok(ApiResponse<SurveySubmissionDto>.Ok(dto, $"Survey {action}ed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing submission {SubmissionId}", submissionId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Withdraw/unlock a submitted survey
        /// </summary>
        [HttpPost("surveys/{surveyId}/withdraw")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult WithdrawSubmission(long surveyId)
        {
            try
            {
                var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);
                if (submission == null)
                {
                    return NotFound(ApiResponse<object>.Fail("No submission found"));
                }

                if (submission.SubmissionStatus != "Submitted")
                {
                    return BadRequest(ApiResponse<object>.Fail("Can only withdraw pending submissions"));
                }

                var userId = GetCurrentUserId();

                // Check if user can withdraw (must be the submitter)
                if (submission.SubmittedBy != userId)
                {
                    return Forbid();
                }

                // Delete submission and update survey status
                var result = _submissionRepo.DeleteSubmission(submission.SubmissionId);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to withdraw submission"));
                }

                _surveyRepo.UpdateSurveyStatus(surveyId, "In Progress");

                _logger.LogInformation("Submission withdrawn for survey {SurveyId} by user {UserId}", surveyId, userId);
                return Ok(ApiResponse<object>.Ok(null, "Submission withdrawn successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing submission for survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Assignments

        /// <summary>
        /// Get assignments for a survey
        /// </summary>
        [HttpGet("surveys/{surveyId}/assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<SurveyAssignmentDto>>), 200)]
        public IActionResult GetAssignments(long surveyId)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var assignments = _surveyRepo.GetSurveyAssignments(surveyId) ?? new List<SurveyAssignmentModel>();

                var dtos = assignments.Select(a => new SurveyAssignmentDto
                {
                    TransId = a.TransID,
                    SurveyId = a.SurveyID,
                    EmpId = a.EmpID,
                    EmpName = a.EmpName,
                    DueDate = a.DueDate
                }).ToList();

                return Ok(ApiResponse<List<SurveyAssignmentDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assignments for survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Create assignments for a survey
        /// </summary>
        [HttpPost("surveys/{surveyId}/assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<SurveyAssignmentDto>>), 200)]
        public IActionResult CreateAssignments(long surveyId, [FromBody] AssignmentCreateRequest request)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var userId = GetCurrentUserId();
                var created = new List<SurveyAssignmentDto>();

                foreach (var empId in request.EmpIds)
                {
                    var assignment = new SurveyAssignmentModel
                    {
                        SurveyID = surveyId,
                        EmpID = empId,
                        DueDate = request.DueDate,
                        CreateBy = userId
                    };

                    var result = _surveyRepo.AddSurveyAssignment(assignment);
                    if (result)
                    {
                        var emp = _adminRepo.GetEmployeeById(empId);
                        created.Add(new SurveyAssignmentDto
                        {
                            SurveyId = surveyId,
                            EmpId = empId,
                            EmpName = emp?.EmpName,
                            DueDate = request.DueDate
                        });
                    }
                }

                // Update survey status to Assigned if it was Created
                if (survey.SurveyStatus == "Created" && created.Any())
                {
                    _surveyRepo.UpdateSurveyStatus(surveyId, "Assigned");
                }

                _logger.LogInformation("{Count} assignments created for survey {SurveyId}", created.Count, surveyId);
                return Ok(ApiResponse<List<SurveyAssignmentDto>>.Ok(created, $"{created.Count} assignments created"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignments for survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Delete an assignment
        /// </summary>
        [HttpDelete("surveys/{surveyId}/assignments/{transId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult DeleteAssignment(long surveyId, int transId)
        {
            try
            {
                var result = _surveyRepo.DeleteSurveyAssignment(transId);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to delete assignment"));
                }

                return Ok(ApiResponse<object>.Ok(null, "Assignment deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assignment {TransId}", transId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get surveys assigned to current user
        /// </summary>
        [HttpGet("my-assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<SurveyListDto>>), 200)]
        public IActionResult GetMyAssignedSurveys()
        {
            try
            {
                var userId = GetCurrentUserId();
                var assignments = _surveyRepo.GetAllSurveyAssignments(userId);

                var surveyIds = assignments.Select(a => a.SurveyID).Distinct();
                var surveys = new List<SurveyListDto>();

                foreach (var surveyId in surveyIds)
                {
                    var survey = _surveyRepo.GetSurveyById(surveyId);
                    if (survey != null)
                    {
                        surveys.Add(new SurveyListDto
                        {
                            SurveyId = survey.SurveyId,
                            SurveyName = survey.SurveyName,
                            Status = survey.SurveyStatus,
                            ClientName = survey.ClientName,
                            RegionName = survey.RegionName,
                            ImplementationType = survey.ImplementationType,
                            DueDate = survey.DueDate,
                            SurveyDate = survey.SurveyDate,
                            IsRevised = survey.IsRevised
                        });
                    }
                }

                return Ok(ApiResponse<List<SurveyListDto>>.Ok(surveys));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting assigned surveys");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion
    }
}
