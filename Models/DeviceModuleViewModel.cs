using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class DeviceModuleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Device Module Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Module Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Group Name cannot exceed 100 characters")]
        [Display(Name = "Group Name")]
        public string? GroupName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
    }
}
