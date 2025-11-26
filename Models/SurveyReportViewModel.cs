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
}
