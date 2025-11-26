using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class SurveySubmissionModel
    {
        public Int64 SubmissionId { get; set; }
        
        [Required]
        public Int64 SurveyId { get; set; }
        
        [Display(Name = "Survey Name")]
        public string? SurveyName { get; set; }
        
        [Display(Name = "Submission Status")]
        public string SubmissionStatus { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected
        
        [Display(Name = "Submitted By")]
        public int? SubmittedBy { get; set; }
        
        [Display(Name = "Submitted By Name")]
        public string? SubmittedByName { get; set; }
        
        [Display(Name = "Submission Date")]
        public DateTime? SubmissionDate { get; set; }
        
        [Display(Name = "Reviewed By")]
        public int? ReviewedBy { get; set; }
        
        [Display(Name = "Reviewed By Name")]
        public string? ReviewedByName { get; set; }
        
        [Display(Name = "Review Date")]
        public DateTime? ReviewDate { get; set; }
        
        [Display(Name = "Review Comments")]
        public string? ReviewComments { get; set; }
        
        [Display(Name = "Can Edit")]
        public bool CanEdit { get; set; }
        
        [Display(Name = "Locked For Editing")]
        public bool IsLockedForEditing { get; set; }
        
        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }
        
        [Display(Name = "Modified On")]
        public DateTime? ModifiedOn { get; set; }
    }
    
    public class SurveyCompletionStatus
    {
        public bool IsComplete { get; set; }
        public int TotalLocations { get; set; }
        public int CompletedLocations { get; set; }
        public int PendingLocations { get; set; }
        public List<string> IncompleteLocationNames { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
    }
}
