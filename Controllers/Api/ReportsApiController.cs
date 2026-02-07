using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyApp.Models;
using SurveyApp.Models.Api;
using SurveyApp.Repo;
using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SurveyApp.Controllers.Api
{
    /// <summary>
    /// Reports API Controller - Summary and detailed reports
    /// </summary>
    [Route("api/v1/reports")]
    [ApiController]
    [Authorize]
    public class ReportsApiController : ControllerBase
    {
        private readonly ISurvey _surveyRepo;
        private readonly ISurveySubmission _submissionRepo;
        private readonly ISurveyLocationStatus _statusRepo;
        private readonly ILogger<ReportsApiController> _logger;

        public ReportsApiController(
            ISurvey surveyRepo,
            ISurveySubmission submissionRepo,
            ISurveyLocationStatus statusRepo,
            ILogger<ReportsApiController> logger)
        {
            _surveyRepo = surveyRepo;
            _submissionRepo = submissionRepo;
            _statusRepo = statusRepo;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        /// <summary>
        /// Get summary report with filters
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<SummaryReportDto>), 200)]
        public IActionResult GetSummaryReport(
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null,
            [FromQuery] string? status = null,
            [FromQuery] string? region = null,
            [FromQuery] string? implementationType = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var allSurveys = _surveyRepo.GetAllSurveys(userId);

                // Apply date filters
                if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var startDate))
                {
                    allSurveys = allSurveys.Where(s => s.SurveyDate >= startDate).ToList();
                }
                if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var endDate))
                {
                    allSurveys = allSurveys.Where(s => s.SurveyDate <= endDate.AddDays(1)).ToList();
                }

                // Apply other filters
                if (!string.IsNullOrEmpty(status))
                    allSurveys = allSurveys.Where(s => s.SurveyStatus == status).ToList();
                
                if (!string.IsNullOrEmpty(region))
                    allSurveys = allSurveys.Where(s => s.RegionName == region).ToList();
                
                if (!string.IsNullOrEmpty(implementationType))
                    allSurveys = allSurveys.Where(s => s.ImplementationType == implementationType).ToList();

                // Calculate totals
                var totalLocations = allSurveys.Sum(s => _surveyRepo.GetSurveyLocationById(s.SurveyId)?.Count ?? 0);
                var completedLocations = 0;
                
                foreach (var survey in allSurveys)
                {
                    var statuses = _statusRepo.GetSurveyLocationStatuses(survey.SurveyId);
                    completedLocations += statuses.Count(s => s.Status == "Completed");
                }

                var report = new SummaryReportDto
                {
                    TotalSurveys = allSurveys.Count,
                    CompletedSurveys = allSurveys.Count(s => s.SurveyStatus == "Completed"),
                    InProgressSurveys = allSurveys.Count(s => s.SurveyStatus == "In Progress"),
                    PendingSurveys = allSurveys.Count(s => s.SurveyStatus == "Pending" || s.SurveyStatus == "Created"),
                    CompletionRate = allSurveys.Count > 0
                        ? Math.Round((decimal)allSurveys.Count(s => s.SurveyStatus == "Completed") / allSurveys.Count * 100, 1)
                        : 0,
                    TotalLocations = totalLocations,
                    CompletedLocations = completedLocations,
                    SurveysByStatus = allSurveys
                        .GroupBy(s => s.SurveyStatus ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count()),
                    SurveysByRegion = allSurveys
                        .Where(s => !string.IsNullOrEmpty(s.RegionName))
                        .GroupBy(s => s.RegionName!)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    SurveysByType = allSurveys
                        .Where(s => !string.IsNullOrEmpty(s.ImplementationType))
                        .GroupBy(s => s.ImplementationType!)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return Ok(ApiResponse<SummaryReportDto>.Ok(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary report");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get detailed report for a specific survey
        /// </summary>
        [HttpGet("{surveyId}/detailed")]
        [ProducesResponseType(typeof(ApiResponse<DetailedReportDto>), 200)]
        public IActionResult GetDetailedReport(long surveyId)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var locations = _surveyRepo.GetSurveyLocationById(surveyId) ?? new List<SurveyLocationModel>();
                var assignments = _surveyRepo.GetSurveyAssignments(surveyId) ?? new List<SurveyAssignmentModel>();
                var submission = _submissionRepo.GetSubmissionBySurveyId(surveyId);
                var statuses = _statusRepo.GetSurveyLocationStatuses(surveyId);

                var surveyDto = new SurveyDto
                {
                    SurveyId = survey.SurveyId,
                    SurveyName = survey.SurveyName,
                    ImplementationType = survey.ImplementationType,
                    SurveyDate = survey.SurveyDate,
                    SurveyTeamName = survey.SurveyTeamName,
                    SurveyTeamContact = survey.SurveyTeamContact,
                    AgencyName = survey.AgencyName,
                    LocationSiteName = survey.LocationSiteName,
                    ScopeOfWork = survey.ScopeOfWork,
                    Latitude = survey.Latitude,
                    Longitude = survey.Longitude,
                    Status = survey.SurveyStatus,
                    DueDate = survey.DueDate,
                    RegionName = survey.RegionName,
                    ClientName = survey.ClientName,
                    SubmittedDate = survey.SubmittedDate,
                    IsRevised = survey.IsRevised,
                    RevisionNumber = survey.RevisionNumber,
                    RevisionReason = survey.RevisionReason
                };

                var locationReports = locations.Select(loc =>
                {
                    var status = statuses.FirstOrDefault(s => s.LocID == loc.LocID);
                    var itemTypes = _surveyRepo.GetAssignedTypeList(surveyId, loc.LocID);

                    var itemTypeWithItems = itemTypes.Select(t =>
                    {
                        var items = _surveyRepo.GetAssignedItemList(surveyId, loc.LocID, t.ItemTypeID);
                        return new ItemTypeWithItemsDto
                        {
                            ItemTypeId = t.ItemTypeID,
                            TypeName = t.TypeName,
                            TypeDesc = t.TypeDesc,
                            GroupName = t.GroupName,
                            Items = items.Select(i => new SurveyDetailsDto
                            {
                                ItemId = i.ItemID,
                                ItemName = i.ItemName,
                                ItemDesc = i.ItemDesc,
                                ExistingQty = i.ItemQtyExist,
                                RequiredQty = i.ItemQtyReq,
                                Remarks = i.Remarks,
                                ImageUrls = !string.IsNullOrEmpty(i.ImgPath)
                                    ? i.ImgPath.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                                    : new List<string>()
                            }).ToList()
                        };
                    }).ToList();

                    return new LocationReportDto
                    {
                        LocId = loc.LocID,
                        LocName = loc.LocName,
                        LocationType = loc.LocationType,
                        Coordinates = loc.LocLat.HasValue && loc.LocLog.HasValue
                            ? $"{loc.LocLat}, {loc.LocLog}"
                            : null,
                        Status = status?.Status ?? "Pending",
                        ItemTypes = itemTypeWithItems
                    };
                }).ToList();

                var report = new DetailedReportDto
                {
                    Survey = surveyDto,
                    Locations = locationReports,
                    GeneratedDate = DateTime.UtcNow
                };

                return Ok(ApiResponse<DetailedReportDto>.Ok(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating detailed report for survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }

        /// <summary>
        /// Get requirement summary (grouped by item type)
        /// </summary>
        [HttpGet("{surveyId}/requirements")]
        [ProducesResponseType(typeof(ApiResponse<List<RequirementSummaryItemDto>>), 200)]
        public IActionResult GetRequirementSummary(long surveyId)
        {
            try
            {
                var survey = _surveyRepo.GetSurveyById(surveyId);
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                var locations = _surveyRepo.GetSurveyLocationById(surveyId) ?? new List<SurveyLocationModel>();
                var summary = new Dictionary<int, RequirementSummaryItemDto>();

                foreach (var loc in locations)
                {
                    var itemTypes = _surveyRepo.GetAssignedTypeList(surveyId, loc.LocID);
                    foreach (var type in itemTypes)
                    {
                        var items = _surveyRepo.GetAssignedItemList(surveyId, loc.LocID, type.ItemTypeID);
                        foreach (var item in items)
                        {
                            if (!summary.ContainsKey(item.ItemID))
                            {
                                summary[item.ItemID] = new RequirementSummaryItemDto
                                {
                                    ItemId = item.ItemID,
                                    ItemName = item.ItemName,
                                    ItemTypeId = type.ItemTypeID,
                                    ItemTypeName = type.TypeName,
                                    TotalExisting = 0,
                                    TotalRequired = 0
                                };
                            }
                            summary[item.ItemID].TotalExisting += item.ItemQtyExist;
                            summary[item.ItemID].TotalRequired += item.ItemQtyReq;
                        }
                    }
                }

                var result = summary.Values
                    .OrderBy(s => s.ItemTypeName)
                    .ThenBy(s => s.ItemName)
                    .ToList();

                return Ok(ApiResponse<List<RequirementSummaryItemDto>>.Ok(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating requirement summary for survey {SurveyId}", surveyId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred"));
            }
        }
    }

    public class RequirementSummaryItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int ItemTypeId { get; set; }
        public string ItemTypeName { get; set; } = string.Empty;
        public int TotalExisting { get; set; }
        public int TotalRequired { get; set; }
    }

    /// <summary>
    /// Image Upload API Controller - Handle image uploads to Cloudinary
    /// </summary>
    [Route("api/v1/images")]
    [ApiController]
    [Authorize]
    public class ImageUploadApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISurvey _surveyRepo;
        private readonly ILogger<ImageUploadApiController> _logger;

        public ImageUploadApiController(
            IConfiguration configuration,
            ISurvey surveyRepo,
            ILogger<ImageUploadApiController> logger)
        {
            _configuration = configuration;
            _surveyRepo = surveyRepo;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        private Cloudinary GetCloudinary()
        {
            var cloudName = _configuration["Cloudinary:CloudName"];
            var apiKey = _configuration["Cloudinary:ApiKey"];
            var apiSecret = _configuration["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            return new Cloudinary(account);
        }

        /// <summary>
        /// Upload an image for a survey item
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(ApiResponse<ImageUploadResponse>), 200)]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<IActionResult> UploadImage(
            [FromQuery] long surveyId,
            [FromQuery] int locId,
            [FromQuery] int itemId,
            IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.Fail("No file uploaded"));
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(ApiResponse<object>.Fail("Invalid file type. Allowed: JPEG, PNG, GIF, WebP"));
                }

                var cloudinary = GetCloudinary();
                var survey = _surveyRepo.GetSurveyById(surveyId);
                
                if (survey == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Survey not found"));
                }

                // Generate unique public ID
                var publicId = $"survey_{surveyId}/loc_{locId}/item_{itemId}_{DateTime.UtcNow.Ticks}";

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = publicId,
                    Folder = "surveys",
                    Transformation = new Transformation()
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    return StatusCode(500, ApiResponse<object>.Fail($"Upload failed: {uploadResult.Error.Message}"));
                }

                // Note: Image reference saving to database would require adding SaveItemImage method to ISurvey
                // For now, we just return the uploaded image URL
                var response = new ImageUploadResponse
                {
                    ImageUrl = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId
                };

                _logger.LogInformation("Image uploaded for survey {SurveyId} location {LocId} item {ItemId}: {Url}", 
                    surveyId, locId, itemId, response.ImageUrl);
                return Ok(ApiResponse<ImageUploadResponse>.Ok(response, "Image uploaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while uploading"));
            }
        }

        /// <summary>
        /// Delete an image from Cloudinary
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return BadRequest(ApiResponse<object>.Fail("Public ID is required"));
                }

                var cloudinary = GetCloudinary();
                
                var deleteParams = new DeletionParams(publicId);
                var result = await cloudinary.DestroyAsync(deleteParams);

                if (result.Result == "ok" || result.Result == "not found")
                {
                    return Ok(ApiResponse<object>.Ok(null, "Image deleted successfully"));
                }

                return BadRequest(ApiResponse<object>.Fail($"Failed to delete image: {result.Result}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {PublicId}", publicId);
                return StatusCode(500, ApiResponse<object>.Fail("An error occurred while deleting"));
            }
        }
    }
}
