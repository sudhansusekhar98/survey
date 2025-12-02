using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class DeviceViewModel
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Device Module is required")]
        [Display(Name = "Device Module")]
        public int ModuleId { get; set; }

        [Required(ErrorMessage = "Device Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Device Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Device Code is required")]
        [StringLength(50, ErrorMessage = "Code cannot exceed 50 characters")]
        [Display(Name = "Device Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit of Measurement is required")]
        [StringLength(50, ErrorMessage = "UOM cannot exceed 50 characters")]
        [Display(Name = "Unit of Measurement")]
        public string UOM { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Sequence Number")]
        public int? SequenceNo { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }

        // Navigation property for display
        public string? ModuleName { get; set; }
    }
}
