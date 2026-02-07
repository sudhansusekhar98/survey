using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Models.Api;
using SurveyApp.Repo;
using System.Security.Claims;

namespace SurveyApp.Controllers.Api
{
    /// <summary>
    /// Survey Locations API Controller
    /// </summary>
    [Route("api/v1/surveys/{surveyId}/locations")]
    [ApiController]
    [Authorize]
    public class LocationApiController : ControllerBase
    {
        private readonly ISurvey _surveyRepo;
        private readonly ISurveyLocationStatus _statusRepo;
        private readonly ILogger<LocationApiController> _logger;

        public LocationApiController(
            ISurvey surveyRepo,
            ISurveyLocationStatus statusRepo,
            ILogger<LocationApiController> logger)
        {
            _surveyRepo = surveyRepo;
            _statusRepo = statusRepo;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        #region Location CRUD

        /// <summary>
        /// Get all locations for a survey
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<SurveyLocationDto>>), 200)]
        public IActionResult GetLocations(long surveyId)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var locations = _surveyRepo.GetSurveyLocationById(surveyId) ?? new List<SurveyLocationModel>();
                var statuses = _statusRepo.GetSurveyLocationStatuses(surveyId);

                var dtos = locations.Select(l =>
                {
                    var status = statuses.FirstOrDefault(s => s.LocID == l.LocID);
                    var assignedTypes = _surveyRepo.GetSelectedItemTypesForLocation(l.LocID)?
                        .Select(t => t.Id).ToList() ?? new List<int>();

                    return new SurveyLocationDto
                    {
                        LocId = l.LocID,
                        SurveyId = l.SurveyID,
                        LocName = l.LocName,
                        Latitude = l.LocLat,
                        Longitude = l.LocLog,
                        LocationType = l.LocationType,
                        WayType = l.WayType,
                        IsGlobal = l.IsGlobal,
                        Status = status?.Status,
                        CreatedOn = l.CreateOn,
                        AssignedItemTypeIds = assignedTypes
                    };
                }).ToList();

                return Ok(ApiResponse<List<SurveyLocationDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations for survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get a specific location
        /// </summary>
        [HttpGet("{locId}")]
        [ProducesResponseType(typeof(ApiResponse<SurveyLocationDto>), 200)]
        public IActionResult GetLocation(long surveyId, int locId)
        {
            try
            {
                var location = _surveyRepo.GetSurveyLocationByLocId(locId);
                if (location == null || location.SurveyID != surveyId)
                {
                    return NotFound(ApiResponse<object>.Fail("Location not found"));
                }

                var status = _statusRepo.GetLocationStatus(surveyId, locId);
                var assignedTypes = _surveyRepo.GetSelectedItemTypesForLocation(locId)?
                    .Select(t => t.Id).ToList() ?? new List<int>();

                var dto = new SurveyLocationDto
                {
                    LocId = location.LocID,
                    SurveyId = location.SurveyID,
                    LocName = location.LocName,
                    Latitude = location.LocLat,
                    Longitude = location.LocLog,
                    LocationType = location.LocationType,
                    WayType = location.WayType,
                    IsGlobal = location.IsGlobal,
                    Status = status?.Status,
                    CreatedOn = location.CreateOn,
                    AssignedItemTypeIds = assignedTypes
                };

                return Ok(ApiResponse<SurveyLocationDto>.Ok(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location {LocId} for survey {SurveyId}", locId, surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Create a new location
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SurveyLocationDto>), 201)]
        public IActionResult CreateLocation(long surveyId, [FromBody] LocationCreateRequest request)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));
                }

                var userId = GetCurrentUserId();

                var location = new SurveyLocationModel
                {
                    SurveyID = surveyId,
                    LocName = request.LocName,
                    LocLat = request.Latitude,
                    LocLog = request.Longitude,
                    LocationType = request.LocationType,
                    WayType = request.WayType,
                    IsGlobal = request.IsGlobal,
                    CreateBy = userId,
                    CreateOn = DateTime.Now,
                    Isactive = true
                };

                var result = _surveyRepo.AddSurveyLocation(location);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to create location"));
                }

                // Get the created location
                var locations = _surveyRepo.GetSurveyLocationById(surveyId);
                var created = locations?.OrderByDescending(l => l.LocID).FirstOrDefault();

                if (created != null && request.ItemTypeIds != null && request.ItemTypeIds.Any())
                {
                    // Assign item types to location
                    _surveyRepo.SaveItemTypesForLocation(surveyId, survey.SurveyName, created.LocID, request.ItemTypeIds, userId);
                }

                // Update survey status to In Progress if first location
                if (survey.SurveyStatus == "Created" || survey.SurveyStatus == "Assigned")
                {
                    _surveyRepo.UpdateSurveyStatus(surveyId, "In Progress");
                }

                _logger.LogInformation("Location created for survey {SurveyId} by user {UserId}", surveyId, userId);

                var dto = new SurveyLocationDto
                {
                    LocId = created?.LocID ?? 0,
                    SurveyId = surveyId,
                    LocName = request.LocName,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    LocationType = request.LocationType,
                    WayType = request.WayType,
                    IsGlobal = request.IsGlobal,
                    AssignedItemTypeIds = request.ItemTypeIds ?? new List<int>()
                };

                return CreatedAtAction(nameof(GetLocation), new { surveyId, locId = dto.LocId }, 
                    ApiResponse<SurveyLocationDto>.Ok(dto, "Location created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating location for survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Update a location
        /// </summary>
        [HttpPut("{locId}")]
        [ProducesResponseType(typeof(ApiResponse<SurveyLocationDto>), 200)]
        public IActionResult UpdateLocation(long surveyId, int locId, [FromBody] LocationUpdateRequest request)
        {
            try
            {
                if (locId != request.LocId)
                {
                    return BadRequest(ApiResponse<object>.Fail("Location ID mismatch"));
                }

                var existing = _surveyRepo.GetSurveyLocationByLocId(locId);
                if (existing == null || existing.SurveyID != surveyId)
                {
                    return NotFound(ApiResponse<object>.Fail("Location not found"));
                }

                var survey = _surveyRepo.GetSurveyById(surveyId);
                var userId = GetCurrentUserId();

                var location = new SurveyLocationModel
                {
                    LocID = locId,
                    SurveyID = surveyId,
                    LocName = request.LocName,
                    LocLat = request.Latitude,
                    LocLog = request.Longitude,
                    LocationType = request.LocationType,
                    WayType = request.WayType,
                    IsGlobal = request.IsGlobal,
                    Isactive = true
                };

                var result = _surveyRepo.UpdateSurveyLocation(location);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to update location"));
                }

                // Update item types if provided
                if (request.ItemTypeIds != null)
                {
                    _surveyRepo.SaveItemTypesForLocation(surveyId, survey?.SurveyName ?? "", locId, request.ItemTypeIds, userId);
                }

                _logger.LogInformation("Location {LocId} updated for survey {SurveyId}", locId, surveyId);

                var dto = new SurveyLocationDto
                {
                    LocId = locId,
                    SurveyId = surveyId,
                    LocName = request.LocName,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    LocationType = request.LocationType,
                    WayType = request.WayType,
                    IsGlobal = request.IsGlobal,
                    AssignedItemTypeIds = request.ItemTypeIds ?? new List<int>()
                };

                return Ok(ApiResponse<SurveyLocationDto>.Ok(dto, "Location updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location {LocId} for survey {SurveyId}", locId, surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Delete a location
        /// </summary>
        [HttpDelete("{locId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult DeleteLocation(long surveyId, int locId)
        {
            try
            {
                var existing = _surveyRepo.GetSurveyLocationByLocId(locId);
                if (existing == null || existing.SurveyID != surveyId)
                {
                    return NotFound(ApiResponse<object>.Fail("Location not found"));
                }

                var result = _surveyRepo.DeleteSurveyLocation(locId);
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to delete location"));
                }

                _logger.LogInformation("Location {LocId} deleted from survey {SurveyId}", locId, surveyId);
                return Ok(ApiResponse<object>.Ok(null, "Location deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting location {LocId} from survey {SurveyId}", locId, surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion

        #region Location Status

        /// <summary>
        /// Mark location as completed
        /// </summary>
        [HttpPost("{locId}/complete")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult MarkAsCompleted(long surveyId, int locId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = _statusRepo.MarkLocationAsCompleted(surveyId, locId, userId);
                
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to mark location as completed"));
                }

                return Ok(ApiResponse<object>.Ok(null, "Location marked as completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking location {LocId} as completed", locId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Mark location as in progress (unlock)
        /// </summary>
        [HttpPost("{locId}/unlock")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult MarkAsInProgress(long surveyId, int locId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = _statusRepo.MarkLocationAsInProgress(surveyId, locId, userId);
                
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to unlock location"));
                }

                return Ok(ApiResponse<object>.Ok(null, "Location unlocked for editing"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking location {LocId}", locId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get available item types for a location
        /// </summary>
        [HttpGet("{locId}/item-types")]
        [ProducesResponseType(typeof(ApiResponse<List<DeviceModuleDto>>), 200)]
        public IActionResult GetItemTypes(long surveyId, int locId)
        {
            try
            {
                var itemTypes = _surveyRepo.GetItemTypeMaster(locId);
                var selectedTypes = _surveyRepo.GetSelectedItemTypesForLocation(locId);
                var selectedIds = selectedTypes?.Select(t => t.Id).ToHashSet() ?? new HashSet<int>();

                var dtos = itemTypes.Select(t => new DeviceModuleDto
                {
                    Id = t.Id,
                    TypeName = t.TypeName,
                    TypeDesc = t.TypeDesc,
                    GroupName = t.GroupName,
                    IsActive = selectedIds.Contains(t.Id)
                }).ToList();

                return Ok(ApiResponse<List<DeviceModuleDto>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item types for location {LocId}", locId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Assign item types to a location
        /// </summary>
        [HttpPost("{locId}/item-types")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public IActionResult AssignItemTypes(long surveyId, int locId, [FromBody] AssignItemTypesRequest request)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var userId = GetCurrentUserId();
                var result = _surveyRepo.SaveItemTypesForLocation(surveyId, survey.SurveyName, locId, request.ItemTypeIds, userId);
                
                if (!result)
                {
                    return BadRequest(ApiResponse<object>.Fail("Failed to assign item types"));
                }

                return Ok(ApiResponse<object>.Ok(null, "Item types assigned successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning item types to location {LocId}", locId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        #endregion
    }

    public class AssignItemTypesRequest
    {
        public List<int> ItemTypeIds { get; set; } = new();
    }
}
