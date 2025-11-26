namespace SurveyApp.Models
{
    public class DashboardViewModel
    {
        // Overall Statistics
        public int TotalSurveys { get; set; }
        public int CompletedSurveys { get; set; }
        public int InProgressSurveys { get; set; }
        public int PendingSurveys { get; set; }
        public int OnHoldSurveys { get; set; }
        public int AssignedSurveys { get; set; }
        public int MissedDeadlineSurveys { get; set; }
        public decimal CompletionRate { get; set; }
        
        // User-specific data
        public string UserName { get; set; } = string.Empty;
        public int UserRoleId { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanView { get; set; }
        
        // Region-based statistics
        public Dictionary<string, int> SurveysByRegion { get; set; } = new();
        public Dictionary<string, int> SurveysByStatus { get; set; } = new();
        public Dictionary<string, int> SurveysByImplementationType { get; set; } = new();
        
        // Recent surveys
        public List<SurveyModel> RecentSurveys { get; set; } = new();
        
        // Monthly trends
        public Dictionary<string, int> MonthlySurveyCount { get; set; } = new();
        public Dictionary<string, int> MonthlyCompletionCount { get; set; } = new();
        
        // Performance metrics
        public int TotalLocations { get; set; }
        public int CompletedLocations { get; set; }
        public int PendingLocations { get; set; }
        
        // Alerts and notifications
        public int OverdueSurveys { get; set; }
        public int DueSoon { get; set; }
        
        // Team statistics
        public int ActiveTeamMembers { get; set; }
        public int TotalAssignments { get; set; }
    }
}
