using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class EmpMasterModel
    {
        public int EmpID { get; set; }

        [Required(ErrorMessage = "Employee Code is required")]
        [Display(Name = "Employee Code")]
        [StringLength(50)]
        public string EmpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee Name is required")]
        [Display(Name = "Employee Name")]
        [StringLength(150)]
        public string EmpName { get; set; } = string.Empty;

        [Display(Name = "Gender")]
        [StringLength(10)]
        public string? Gender { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Mobile No")]
        [StringLength(20)]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? MobileNo { get; set; }

        [Display(Name = "Email")]
        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(200)]
        public string? AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Display(Name = "City")]
        [StringLength(100)]
        public string? City { get; set; }

        [Display(Name = "State")]
        [StringLength(100)]
        public string? State { get; set; }

        [Display(Name = "Country")]
        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Pin Code")]
        [StringLength(20)]
        public string? PinCode { get; set; }

        [Display(Name = "Department")]
        public int? DeptID { get; set; }

        [Display(Name = "Designation")]
        [StringLength(100)]
        public string? Designation { get; set; }

        [Display(Name = "Date of Joining")]
        [DataType(DataType.Date)]
        public DateTime? DateOfJoining { get; set; }

        [Display(Name = "Date of Leaving")]
        [DataType(DataType.Date)]
        public DateTime? DateOfLeaving { get; set; }

        [Display(Name = "Employment Type")]
        [StringLength(50)]
        public string? EmploymentType { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
