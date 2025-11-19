using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnalyticaDocs.Models
{
    public class UserModel
    {
        [DisplayName("User ID")]
        public int? UserId { get; set; }


        [DisplayName("Login ID")]
        [Required]
        public string LoginId { get; set; } = null!;

        [DisplayName("User Name")]
        [Required]
        public string LoginName { get; set; } = null!;

        [DisplayName("Password")]
        [Required]
        public string LoginPassword { get; set; } = null!;

        [DisplayName("Role")]
        [Required]
        public int RoleId { get; set; }

        [DisplayName("Employee ID")]
        [Required]
        public int EmpID { get; set; }


        [DisplayName("Status")]
        [Required] public string ISActive { get; set; } = null!;

        public int? CreateBy { get; set; }

        [DisplayName("Role")]
        [NotMapped]
        public string? RoleName { get; set; }

        [NotMapped]
        public string? ISActivedesc { get; set; }

        [NotMapped]
        public List<SelectListItem> EmployeeOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "-- Select Employee --", Value = "" }
        };

        public List<SelectListItem> StatusOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "", Selected = true},
            new SelectListItem { Text = "Active", Value = "Y" /*, Selected = true*/ },
            new SelectListItem { Text = "Deactive", Value = "N" }
        };

        public List<SelectListItem> RoleOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "", Value = "" ,Selected = true },
            new SelectListItem { Text = "Super User", Value = "101" /*, Selected = true*/ },
            new SelectListItem { Text = "Limited User", Value = "102" }
        };

        [DisplayName("Mobile No")]
        [Required]
        public string MobileNo { get; set; } = null!;

        [DisplayName("Email ID")]
        [Required]
        public string EmailID { get; set; } = null!;
        public int Srno { get; set; }
    }
}
