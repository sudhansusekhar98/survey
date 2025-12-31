using System;
using System.Collections.Generic;

namespace SurveyApp.Models
{
    public class SurveyReportViewModel
    {
        public string ReportTitle { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public string GeneratedBy { get; set; } = string.Empty;
        
        // Filter parameters
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public string? Region { get; set; }
        public string? ImplementationType { get; set; }
        
        // Summary statistics
        public int TotalSurveys { get; set; }
        public int CompletedSurveys { get; set; }
        public int InProgressSurveys { get; set; }
        public int PendingSurveys { get; set; }
        public int OnHoldSurveys { get; set; }
        public int MissedDeadlineSurveys { get; set; }
        public decimal CompletionRate { get; set; }
        
        // Location statistics
        public int TotalLocations { get; set; }
        public int CompletedLocations { get; set; }
        public int PendingLocations { get; set; }
        
        // Survey list
        public List<SurveyModel> Surveys { get; set; } = new List<SurveyModel>();
        
        // Breakdown by status
        public Dictionary<string, int> SurveysByStatus { get; set; } = new Dictionary<string, int>();
        
        // Breakdown by region
        public Dictionary<string, int> SurveysByRegion { get; set; } = new Dictionary<string, int>();
        
        // Breakdown by implementation type
        public Dictionary<string, int> SurveysByImplementationType { get; set; } = new Dictionary<string, int>();
        
        // Monthly trends
        public Dictionary<string, int> MonthlySurveyCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MonthlyCompletionCount { get; set; } = new Dictionary<string, int>();
    }
    
    public class DetailedSurveyReportModel
    {
        public string ReportTitle { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public string GeneratedBy { get; set; } = string.Empty;
        
        public SurveyModel Survey { get; set; } = new SurveyModel();
        public List<SurveyLocationModel> Locations { get; set; } = new List<SurveyLocationModel>();
        public List<SurveyAssignmentModel> Assignments { get; set; } = new List<SurveyAssignmentModel>();
        public List<SurveySubmissionModel> Submissions { get; set; } = new List<SurveySubmissionModel>();
        public Dictionary<int, string> LocationStatuses { get; set; } = new Dictionary<int, string>();
        public List<SurveyDetailsLocationModel> SurveyDetails { get; set; } = new List<SurveyDetailsLocationModel>();
        
        // Calculated fields
        public int TotalLocations { get; set; }
        public int CompletedLocations { get; set; }
        public decimal LocationCompletionRate { get; set; }
        public int TotalAssignments { get; set; }
        public TimeSpan? TimeToComplete { get; set; }
    }

    // Model for Requirement Summary - displays locations as rows with device types as columns
    public class RequirementSummaryModel
    {
        public long SurveyId { get; set; }
        public string SurveyName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        
        // List of device types (Camera categories) that will be shown as columns
        public List<string> DeviceTypes { get; set; } = new List<string>();
        
        // Dictionary mapping ItemType (category) to list of Items (e.g., "Camera" -> ["Dome", "Bullet", "PTZ"])
        public Dictionary<string, List<string>> DeviceCategories { get; set; } = new Dictionary<string, List<string>>();
        
        // Flag to indicate if any location has existing quantities > 0
        public bool HasAnyExisting { get; set; }

        // Granular column visibility per device type: [0]=Existing, [1]=Required, [2]=Images, [3]=Remarks
        public Dictionary<string, bool[]> DeviceColumnVisibility { get; set; } = new Dictionary<string, bool[]>();
        
        // List of locations with their device data - locations shown as rows
        public List<LocationRequirementData> Locations { get; set; } = new List<LocationRequirementData>();
    }

    // Data for a single location row in the requirement summary
    public class LocationRequirementData
    {
        public int SlNo { get; set; }
        public int LocId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Coordinates { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public string? GoogleMapsLink => !string.IsNullOrEmpty(Latitude) && !string.IsNullOrEmpty(Longitude) 
            ? $"https://www.google.com/maps?q={Latitude},{Longitude}" 
            : null;
        
        
        // Dictionary of device type to device data (each device type is a column)
        public Dictionary<string, DeviceRequirementData> DeviceData { get; set; } = new Dictionary<string, DeviceRequirementData>();
        
        
        // Check if this location has any devices assigned
        public bool HasAnyDevices => DeviceData.Any(d => d.Value.ExistingQty > 0 || d.Value.RequiredQty > 0);
    }

    
    // Data for a single device type cell in the requirement summary
    public class DeviceRequirementData
    {
        public string DeviceType { get; set; } = string.Empty;
        public int ExistingQty { get; set; }
        public int RequiredQty { get; set; }
        public bool IsDone { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string UOM { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
        
        // Check if this device has any quantity assigned
        public bool HasQuantity => ExistingQty > 0 || RequiredQty > 0;
    }
}
