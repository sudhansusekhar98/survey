using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Models.Api;
using SurveyApp.Repo;
using AnalyticaDocs.Repo;
using System.Security.Claims;

namespace SurveyApp.Controllers.Api
{
    /// <summary>
    /// Survey API Controller - Full CRUD operations
    /// </summary>
    [Route("api/v1/surveys")]
    [ApiController]
    [Authorize]
    public class SurveyApiController : ControllerBase
    {
        private readonly ISurvey _surveyRepo;
        private readonly ISurveySubmission _submissionRepo;
        private readonly ISurveyLocationStatus _statusRepo;
        private readonly IAdmin _adminRepo;
        private readonly ILogger<SurveyApiController> _logger;

        public SurveyApiController(
            ISurvey surveyRepo,
            ISurveySubmission submissionRepo,
            ISurveyLocationStatus statusRepo,
            IAdmin adminRepo,
            ILogger<SurveyApiController> logger)
        {
            _surveyRepo = surveyRepo;
            _submissionRepo = submissionRepo;
            _statusRepo = statusRepo;
            _adminRepo = adminRepo;
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

        #region Survey CRUD

        /// <summary>
        /// Get all surveys with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<SurveyListDto>>), 200)]
        public IActionResult GetSurveys(
            [FromQuery] string? status = null,
            [FromQuery] string? region = null,
            [FromQuery] string? implementationType = null,
            [FromQuery] int? clientId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var allSurveys = _surveyRepo.GetAllSurveys(userId);

                // Apply filters
                var filtered = allSurveys.AsQueryable();
                
                if (!string.IsNullOrEmpty(status))
                    filtered = filtered.Where(s => s.SurveyStatus == status);
                    
                if (!string.IsNullOrEmpty(region))
                    filtered = filtered.Where(s => s.RegionName == region);
                    
                if (!string.IsNullOrEmpty(implementationType))
                    filtered = filtered.Where(s => s.ImplementationType == implementationType);
                    
                if (clientId.HasValue)
                    filtered = filtered.Where(s => s.ClientID == clientId);

                var totalCount = filtered.Count();
                
                var surveyList = filtered
                    .OrderByDescending(s => s.SurveyId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var surveys = surveyList.Select(s => new SurveyListDto
                    {
                        SurveyId = s.SurveyId,
                        SurveyName = s.SurveyName,
                        Status = s.SurveyStatus,
                        ClientName = s.ClientName,
                        RegionName = s.RegionName,
                        ImplementationType = s.ImplementationType,
                        DueDate = s.DueDate,
                        SurveyDate = s.SurveyDate,
                        IsRevised = s.IsRevised,
                        LocationCount = _surveyRepo.GetSurveyLocationById(s.SurveyId)?.Count ?? 0
                    })
                    .ToList();

                var response = new PaginatedResponse<SurveyListDto>
                {
                    Items = surveys,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };

                return Ok(ApiResponse<PaginatedResponse<SurveyListDto>>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting surveys");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while fetching surveys"));
            }
        }

        /// <summary>
        /// Get survey by ID with full details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<SurveyDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public IActionResult GetSurvey(long id)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(id);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var locations = _surveyRepo.GetSurveyLocationById(id) ?? new List<SurveyLocationModel>();
                var assignments = _surveyRepo.GetSurveyAssignments(id) ?? new List<SurveyAssignmentModel>();
                var submission = _submissionRepo.GetSubmissionBySurveyId(id);

                var dto = MapToSurveyDto(survey, locations, assignments, submission);

                return Ok(ApiResponse<SurveyDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting survey {SurveyId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while fetching survey"));
            }
        }

        /// <summary>
        /// Create a new survey
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SurveyDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public IActionResult CreateSurvey([FromBody] SurveyCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));
                }

                var userId = GetCurrentUserId();

                var survey = new SurveyModel
                {
                    SurveyName = request.SurveyName,
                    ImplementationType = request.ImplementationType,
                    SurveyDate = request.SurveyDate ?? DateTime.Now,
                    SurveyTeamName = request.SurveyTeamName,
                    SurveyTeamContact = request.SurveyTeamContact,
                    AgencyName = request.AgencyName,
                    LocationSiteName = request.LocationSiteName,
                    StateId = request.StateId,
                    CityId = request.CityId,
                    ScopeOfWork = request.ScopeOfWork,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    DueDate = request.DueDate,
                    RegionID = request.RegionId,
                    ClientID = request.ClientId,
                    SurveyStatus = "Created",
                    CreatedBy = userId
                };

                var result = _surveyRepo.AddSurvey(survey);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to create survey"));
                }

                // Get the created survey (assuming it's the latest one)
                var created = _surveyRepo.GetAllSurveys(userId)
                    .OrderByDescending(s => s.SurveyId)
                    .FirstOrDefault();

                if (created == null)
                {
                    return StatusCode(500, ApiResponse<object>.Fail("Survey created but could not retrieve"));
                }

                _logger.LogInformation("Survey created: {SurveyId} by user {UserId}", created.SurveyId, userId);

                var dto = MapToSurveyDto(created, new List<SurveyLocationModel>(), new List<SurveyAssignmentModel>(), null);
                return CreatedAtAction(nameof(GetSurvey), new { id = created.SurveyId }, ApiResponse<SurveyDto>.Ok(dto, "Survey created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating survey");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while creating survey"));
            }
        }

        /// <summary>
        /// Update an existing survey
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<SurveyDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public IActionResult UpdateSurvey(long id, [FromBody] SurveyUpdateRequest request)
        {
            try
            {
                if (id != request.SurveyId)
                {
                    return BadRequest(ApiResponse<object>.Fail("Survey ID mismatch"));
                }

                var existing = _surveyRepo.GetSurveyById(id);
                if (existing == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                // Check if survey can be edited
                if (!_submissionRepo.CanEditSurvey(id))
                {
                    return BadRequest(ApiResponse<object>.Fail("Survey is locked and cannot be edited"));
                }

                var survey = new SurveyModel
                {
                    SurveyId = id,
                    SurveyName = request.SurveyName,
                    ImplementationType = request.ImplementationType,
                    SurveyDate = request.SurveyDate,
                    SurveyTeamName = request.SurveyTeamName,
                    SurveyTeamContact = request.SurveyTeamContact,
                    AgencyName = request.AgencyName,
                    LocationSiteName = request.LocationSiteName,
                    StateId = request.StateId,
                    CityId = request.CityId,
                    MapMarking = existing.MapMarking,
                    CityDistrict = existing.CityDistrict,
                    ScopeOfWork = request.ScopeOfWork,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    DueDate = request.DueDate,
                    RegionID = request.RegionId,
                    ClientID = request.ClientId,
                    SurveyStatus = request.Status ?? existing.SurveyStatus,
                    CreatedBy = existing.CreatedBy
                };

                var result = _surveyRepo.UpdateSurvey(survey);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to update survey"));
                }

                var updated = _surveyRepo.GetSurveyById(id);
                var locations = _surveyRepo.GetSurveyLocationById(id) ?? new List<SurveyLocationModel>();
                var assignments = _surveyRepo.GetSurveyAssignments(id) ?? new List<SurveyAssignmentModel>();
                var submission = _submissionRepo.GetSubmissionBySurveyId(id);

                var dto = MapToSurveyDto(updated!, locations, assignments, submission);

                _logger.LogInformation("Survey updated: {SurveyId}", id);
                return Ok(ApiResponse<SurveyDto>.Ok(dto, "Survey updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating survey {SurveyId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while updating survey"));
            }
        }

        /// <summary>
        /// Delete a survey
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public IActionResult DeleteSurvey(long id)
        {
            try
            {
                var existing = _surveyRepo.GetSurveyById(id);
                if (existing == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                // Check if survey can be deleted (not submitted/approved)
                var submission = _submissionRepo.GetSubmissionBySurveyId(id);
                if (submission != null && submission.SubmissionStatus == "Approved")
                {
                    return BadRequest(ApiResponse<object>.Fail("Cannot delete an approved survey"));
                }

                var result = _surveyRepo.DeleteSurvey(id);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to delete survey"));
                }

                _logger.LogInformation("Survey deleted: {SurveyId}", id);
                return Ok(ApiResponse<object>.Ok(null, "Survey deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting survey {SurveyId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while deleting survey"));
            }
        }

        #endregion

        #region Survey Status

        /// <summary>
        /// Update survey status
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult UpdateStatus(long id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var existing = _surveyRepo.GetSurveyById(id);
                if (existing == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var result = _surveyRepo.UpdateSurveyStatus(id, request.Status);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to update status"));
                }

                return Ok(ApiResponse<object>.Ok(null, $"Status updated to {request.Status}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for survey {SurveyId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Check survey completion status
        /// </summary>
        [HttpGet("{id}/completion-status")]
        [ProducesResponseType(typeof(ApiResponse<SurveyCompletionStatus>), 200)]
        public IActionResult GetCompletionStatus(long id)
        {
            try
            {
                var status = _surveyRepo.CheckSurveyCompletionStatus(id);
                return Ok(ApiResponse<SurveyCompletionStatus>.Ok(status));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking completion status for survey {SurveyId}", id);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Helper Methods

        private SurveyDto MapToSurveyDto(
            SurveyModel survey, 
            List<SurveyLocationModel> locations,
            List<SurveyAssignmentModel> assignments,
            SurveySubmissionModel? submission)
        {
            return new SurveyDto
            {
                SurveyId = survey.SurveyId,
                SurveyName = survey.SurveyName,
                ImplementationType = survey.ImplementationType,
                SurveyDate = survey.SurveyDate,
                SurveyTeamName = survey.SurveyTeamName,
                SurveyTeamContact = survey.SurveyTeamContact,
                AgencyName = survey.AgencyName,
                LocationSiteName = survey.LocationSiteName,
                StateId = survey.StateId,
                StateName = survey.MapMarking,
                CityId = survey.CityId,
                CityName = survey.CityDistrict,
                ScopeOfWork = survey.ScopeOfWork,
                Latitude = survey.Latitude,
                Longitude = survey.Longitude,
                Status = survey.SurveyStatus,
                DueDate = survey.DueDate,
                RegionId = survey.RegionID,
                RegionName = survey.RegionName,
                ClientId = survey.ClientID,
                ClientName = survey.ClientName,
                SubmittedDate = survey.SubmittedDate,
                IsRevised = survey.IsRevised,
                RevisionNumber = survey.RevisionNumber,
                RevisionReason = survey.RevisionReason,
                CreatedBy = survey.CreatedBy,
                Locations = locations.Select(l => new SurveyLocationDto
                {
                    LocId = l.LocID,
                    SurveyId = l.SurveyID,
                    LocName = l.LocName,
                    Latitude = l.LocLat,
                    Longitude = l.LocLog,
                    LocationType = l.LocationType,
                    WayType = l.WayType,
                    IsGlobal = l.IsGlobal,
                    CreatedOn = l.CreateOn
                }).ToList(),
                Assignments = assignments.Select(a => new SurveyAssignmentDto
                {
                    TransId = a.TransID,
                    SurveyId = a.SurveyID,
                    EmpId = a.EmpID,
                    EmpName = a.EmpName,
                    DueDate = a.DueDate
                }).ToList(),
                Submission = submission != null ? new SurveySubmissionDto
                {
                    SubmissionId = submission.SubmissionId,
                    SurveyId = submission.SurveyId,
                    SurveyName = submission.SurveyName,
                    Status = submission.SubmissionStatus,
                    SubmittedBy = submission.SubmittedBy,
                    SubmittedByName = submission.SubmittedByName,
                    SubmissionDate = submission.SubmissionDate,
                    ReviewedBy = submission.ReviewedBy,
                    ReviewedByName = submission.ReviewedByName,
                    ReviewDate = submission.ReviewDate,
                    ReviewComments = submission.ReviewComments,
                    CanEdit = submission.CanEdit,
                    CanReview = submission.CanReview
                } : null
            };
        }

        #endregion
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
