using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class SurveyCamRemarksModel
    {
        public int TransID { get; set; }
        
        [Required]
        public Int64 SurveyID { get; set; }
        
        [Required]
        public int LocID { get; set; }
        
        [Required]
        public int ItemID { get; set; }
        
        [Required]
        public int RemarkNo { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;
        
        public int CreatedBy { get; set; }
        
        public DateTime CreatedOn { get; set; }
        
        // Helper property for camera number display
        public int CameraNumber { get; set; }
    }
}
