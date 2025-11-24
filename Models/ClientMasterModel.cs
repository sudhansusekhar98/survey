using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class ClientMasterModel
    {
        public int ClientID { get; set; }

        [Required(ErrorMessage = "Client Name is required")]
        [Display(Name = "Client Name")]
        [StringLength(200, ErrorMessage = "Client Name cannot exceed 200 characters")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client Type is required")]
        [Display(Name = "Client Type")]
        [StringLength(100, ErrorMessage = "Client Type cannot exceed 100 characters")]
        public string ClientType { get; set; } = string.Empty;

        [Display(Name = "Address Line 1")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address3 { get; set; }

        [Display(Name = "State")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }

        [Display(Name = "City")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(200, ErrorMessage = "Contact Person name cannot exceed 200 characters")]
        public string? ContactPerson { get; set; }

        [Display(Name = "Contact Number")]
        [StringLength(20, ErrorMessage = "Contact Number cannot exceed 20 characters")]
        [RegularExpression(@"^[0-9+\-\s()]*$", ErrorMessage = "Invalid contact number format")]
        public string? ContactNumber { get; set; }

        public bool Isactive { get; set; } = true;

        [Display(Name = "Created On")]
        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }
    }
}
