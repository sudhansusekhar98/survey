using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models.Api
{
    #region Common Response Models
    
    /// <summary>
    /// Standard API response wrapper for consistent JSON structure
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
        }
    }

    /// <summary>
    /// Paginated response for list endpoints
    /// </summary>
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;
    }
    
    #endregion

    #region Authentication Models
    
    /// <summary>
    /// Login request model
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Login ID is required")]
        public string LoginId { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login response with JWT token
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } = new();
    }

    /// <summary>
    /// Refresh token request
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// User information DTO
    /// </summary>
    public class UserDto
    {
        public int UserId { get; set; }
        public string LoginId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int? EmpId { get; set; }
    }

    #endregion

    #region Dashboard Models

    /// <summary>
    /// Dashboard statistics response
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalSurveys { get; set; }
        public int CreatedSurveys { get; set; }
        public int AssignedSurveys { get; set; }
        public int InProgressSurveys { get; set; }
        public int SubmittedSurveys { get; set; }
        public int CompletedSurveys { get; set; }
        public int PendingSurveys { get; set; }
        public int OnHoldSurveys { get; set; }
        public int MissedDeadlineSurveys { get; set; }
        public decimal CompletionRate { get; set; }
        public Dictionary<string, int> SurveysByStatus { get; set; } = new();
        public Dictionary<string, int> SurveysByRegion { get; set; } = new();
        public Dictionary<string, int> SurveysByImplementationType { get; set; } = new();
    }

    #endregion

    #region Survey Models

    /// <summary>
    /// Survey list item (lightweight for listings)
    /// </summary>
    public class SurveyListDto
    {
        public long SurveyId { get; set; }
        public string SurveyName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? ClientName { get; set; }
        public string? RegionName { get; set; }
        public string? ImplementationType { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? SurveyDate { get; set; }
        public int LocationCount { get; set; }
        public bool IsRevised { get; set; }
    }

    /// <summary>
    /// Full survey details
    /// </summary>
    public class SurveyDto
    {
        public long SurveyId { get; set; }
        public string SurveyName { get; set; } = string.Empty;
        public string? ImplementationType { get; set; }
        public DateTime? SurveyDate { get; set; }
        public string? SurveyTeamName { get; set; }
        public string? SurveyTeamContact { get; set; }
        public string? AgencyName { get; set; }
        public string? LocationSiteName { get; set; }
        public int? StateId { get; set; }
        public string? StateName { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? ScopeOfWork { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int? RegionId { get; set; }
        public string? RegionName { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public bool IsRevised { get; set; }
        public int RevisionNumber { get; set; }
        public string? RevisionReason { get; set; }
        public int CreatedBy { get; set; }
        
        // Nested data
        public List<SurveyLocationDto> Locations { get; set; } = new();
        public List<SurveyAssignmentDto> Assignments { get; set; } = new();
        public SurveySubmissionDto? Submission { get; set; }
    }

    /// <summary>
    /// Create/Update survey request
    /// </summary>
    public class SurveyCreateRequest
    {
        [Required(ErrorMessage = "Survey name is required")]
        [StringLength(200)]
        public string SurveyName { get; set; } = string.Empty;
        
        public string? ImplementationType { get; set; }
        public DateTime? SurveyDate { get; set; }
        public string? SurveyTeamName { get; set; }
        public string? SurveyTeamContact { get; set; }
        public string? AgencyName { get; set; }
        public string? LocationSiteName { get; set; }
        public int? StateId { get; set; }
        public int? CityId { get; set; }
        public string? ScopeOfWork { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime? DueDate { get; set; }
        public int? RegionId { get; set; }
        public int? ClientId { get; set; }
    }

    public class SurveyUpdateRequest : SurveyCreateRequest
    {
        [Required]
        public long SurveyId { get; set; }
        public string? Status { get; set; }
    }

    #endregion

    #region Location Models

    /// <summary>
    /// Survey location DTO
    /// </summary>
    public class SurveyLocationDto
    {
        public int LocId { get; set; }
        public long SurveyId { get; set; }
        public string LocName { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? LocationType { get; set; }
        public string? WayType { get; set; }
        public bool IsGlobal { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public List<int> AssignedItemTypeIds { get; set; } = new();
    }

    /// <summary>
    /// Create/Update location request
    /// </summary>
    public class LocationCreateRequest
    {
        [Required(ErrorMessage = "Location name is required")]
        public string LocName { get; set; } = string.Empty;
        
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? LocationType { get; set; }
        public string? WayType { get; set; }
        public bool IsGlobal { get; set; }
        public List<int>? ItemTypeIds { get; set; }
    }

    public class LocationUpdateRequest : LocationCreateRequest
    {
        [Required]
        public int LocId { get; set; }
    }

    #endregion

    #region Survey Details Models

    /// <summary>
    /// Survey details (items at a location)
    /// </summary>
    public class SurveyDetailsDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemDesc { get; set; }
        public int ExistingQty { get; set; }
        public int RequiredQty { get; set; }
        public string? Remarks { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public List<SpecificationValueDto> Specifications { get; set; } = new();
    }

    /// <summary>
    /// Item type/module with items
    /// </summary>
    public class ItemTypeWithItemsDto
    {
        public int ItemTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string? TypeDesc { get; set; }
        public string? GroupName { get; set; }
        public List<SurveyDetailsDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Update survey item request
    /// </summary>
    public class SurveyItemUpdateRequest
    {
        [Required]
        public int ItemId { get; set; }
        public int ExistingQty { get; set; }
        public int RequiredQty { get; set; }
        public string? Remarks { get; set; }
        public List<SpecificationValueDto>? Specifications { get; set; }
    }

    /// <summary>
    /// Specification value
    /// </summary>
    public class SpecificationValueDto
    {
        public int SpecificationId { get; set; }
        public string SpecificationName { get; set; } = string.Empty;
        public string? Value { get; set; }
    }

    #endregion

    #region Assignment Models

    /// <summary>
    /// Survey assignment DTO
    /// </summary>
    public class SurveyAssignmentDto
    {
        public int TransId { get; set; }
        public long SurveyId { get; set; }
        public int EmpId { get; set; }
        public string? EmpName { get; set; }
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// Create assignment request
    /// </summary>
    public class AssignmentCreateRequest
    {
        [Required]
        public List<int> EmpIds { get; set; } = new();
        public DateTime? DueDate { get; set; }
    }

    #endregion

    #region Submission Models

    /// <summary>
    /// Survey submission DTO
    /// </summary>
    public class SurveySubmissionDto
    {
        public long SubmissionId { get; set; }
        public long SurveyId { get; set; }
        public string? SurveyName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? SubmittedBy { get; set; }
        public string? SubmittedByName { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ReviewComments { get; set; }
        public bool CanEdit { get; set; }
        public bool CanReview { get; set; }
    }

    /// <summary>
    /// Submit survey request
    /// </summary>
    public class SubmitSurveyRequest
    {
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Review (approve/reject) request
    /// </summary>
    public class ReviewRequest
    {
        [Required]
        public string Action { get; set; } = string.Empty; // "Approve" or "Reject"
        public string? Comments { get; set; }
    }

    #endregion

    #region Master Data Models

    /// <summary>
    /// Client DTO
    /// </summary>
    public class ClientDto
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientType { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Employee DTO
    /// </summary>
    public class EmployeeDto
    {
        public int EmpId { get; set; }
        public string EmpCode { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Device module/category DTO
    /// </summary>
    public class DeviceModuleDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string? TypeDesc { get; set; }
        public string? GroupName { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Device/item DTO
    /// </summary>
    public class DeviceDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public string? ItemDesc { get; set; }
        public string? UOM { get; set; }
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Region DTO
    /// </summary>
    public class RegionDto
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
    }

    #endregion

    #region Report Models

    /// <summary>
    /// Summary report DTO
    /// </summary>
    public class SummaryReportDto
    {
        public int TotalSurveys { get; set; }
        public int CompletedSurveys { get; set; }
        public int InProgressSurveys { get; set; }
        public int PendingSurveys { get; set; }
        public decimal CompletionRate { get; set; }
        public int TotalLocations { get; set; }
        public int CompletedLocations { get; set; }
        public Dictionary<string, int> SurveysByStatus { get; set; } = new();
        public Dictionary<string, int> SurveysByRegion { get; set; } = new();
        public Dictionary<string, int> SurveysByType { get; set; } = new();
    }

    /// <summary>
    /// Detailed survey report DTO
    /// </summary>
    public class DetailedReportDto
    {
        public SurveyDto Survey { get; set; } = new();
        public List<LocationReportDto> Locations { get; set; } = new();
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Location report data
    /// </summary>
    public class LocationReportDto
    {
        public int LocId { get; set; }
        public string LocName { get; set; } = string.Empty;
        public string? LocationType { get; set; }
        public string? Coordinates { get; set; }
        public string? Status { get; set; }
        public List<ItemTypeWithItemsDto> ItemTypes { get; set; } = new();
    }

    #endregion

    #region Image Upload Models

    /// <summary>
    /// Image upload response
    /// </summary>
    public class ImageUploadResponse
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
    }

    #endregion
}
