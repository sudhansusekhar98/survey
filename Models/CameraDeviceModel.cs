using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class CameraDeviceModel
    {
        public int Id { get; set; }
        
        [Required]
        public Int64 SurveyId { get; set; }
        
        [Required]
        public int LocID { get; set; }
        
        [Required]
        public int ItemId { get; set; }
        
        public string? ItemName { get; set; }
        
        public string? ItemCode { get; set; }
        
        public string? ItemDescription { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
        
        public string? Remarks { get; set; }
        
        public List<string>? ImagePaths { get; set; }
        
        public DateTime? CreatedOn { get; set; }
        
        public int? CreatedBy { get; set; }
    }
}
