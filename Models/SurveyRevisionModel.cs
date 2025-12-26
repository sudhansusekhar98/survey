using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    /// <summary>
    /// Model for Survey Revision Log entries
    /// </summary>
    public class SurveyRevisionModel
    {
        public long RevisionLogId { get; set; }
        
        public long OriginalSurveyId { get; set; }
        
        [Display(Name = "Original Survey")]
        public string? OriginalSurveyName { get; set; }
        
        public long RevisedSurveyId { get; set; }
        
        [Display(Name = "Revised Survey")]
        public string? RevisedSurveyName { get; set; }
        
        [Display(Name = "Revision #")]
        public int RevisionNumber { get; set; }
        
        [Display(Name = "Reason for Revision")]
        public string? RevisionReason { get; set; }
        
        public int AssignedBy { get; set; }
        
        [Display(Name = "Assigned By")]
        public string? AssignedByName { get; set; }
        
        [Display(Name = "Assigned Team")]
        public string? AssignedToTeam { get; set; }  // JSON array of EmpIDs
        
        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; }
        
        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }
        
        [Display(Name = "Status")]
        public string Status { get; set; } = "Assigned";
        
        public string? Notes { get; set; }
        
        // Additional display fields
        [Display(Name = "Survey Status")]
        public string? CurrentSurveyStatus { get; set; }
        
        [Display(Name = "Submission Status")]
        public string? SubmissionStatus { get; set; }
        
        [Display(Name = "Client")]
        public string? ClientName { get; set; }
        
        [Display(Name = "Region")]
        public string? RegionName { get; set; }
        
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// ViewModel for creating a new survey revision
    /// </summary>
    public class CreateRevisionModel
    {
        [Required]
        public long SurveyId { get; set; }
        
        [Display(Name = "Survey Name")]
        public string? SurveyName { get; set; }
        
        [Display(Name = "Client")]
        public string? ClientName { get; set; }
        
        [Required(ErrorMessage = "Please provide a reason for the revision")]
        [Display(Name = "Reason for Revision")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string RevisionReason { get; set; } = string.Empty;
        
        [Display(Name = "Assign Team Members")]
        public List<int> AssignedTeamMembers { get; set; } = new();
        
        [Display(Name = "New Due Date")]
        [DataType(DataType.Date)]
        public DateTime? NewDueDate { get; set; }
        
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }
        
        // For dropdown population
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> AvailableTeamMembers { get; set; } = new();
    }

    /// <summary>
    /// Response model for revision operations
    /// </summary>
    public class RevisionResultModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public long? NewSurveyId { get; set; }
        public int? RevisionNumber { get; set; }
    }

    /// <summary>
    /// Model for checking if a survey can be revised
    /// </summary>
    public class CanReviseCheckModel
    {
        public bool CanRevise { get; set; }
        public string? SurveyName { get; set; }
        public string? SurveyStatus { get; set; }
        public string? SubmissionStatus { get; set; }
        public bool IsRevised { get; set; }
        public int RevisionNumber { get; set; }
        public string? Reason { get; set; }  // Reason if can't revise
    }
}
