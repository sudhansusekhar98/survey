using System;
using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class SurveyLocationStatusModel
    {
        public int StatusID { get; set; }
        
        [Required]
        public long SurveyID { get; set; }
        
        [Required]
        public int LocID { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
        
        [Display(Name = "Started Date")]
        public DateTime? StartedDate { get; set; }
        
        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }
        
        [Display(Name = "Completed By")]
        public int? CompletedBy { get; set; }
        
        [Display(Name = "Verified Date")]
        public DateTime? VerifiedDate { get; set; }
        
        [Display(Name = "Verified By")]
        public int? VerifiedBy { get; set; }
        
        [Display(Name = "Remarks")]
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation/Display properties
        public string? LocName { get; set; }
        public string? CompletedByName { get; set; }
        public string? VerifiedByName { get; set; }
    }
}
