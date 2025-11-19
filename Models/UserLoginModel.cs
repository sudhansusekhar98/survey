using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnalyticaDocs.Models
{
    public class UserLoginModel
    {

        public int UserId { get; set; }

        [Required]
        public string LoginId { get; set; } = null!;

        [Required]
        public string LoginName { get; set; } = null!;

        [Required]
        public string LoginPassword { get; set; } = null!;

        public int RoleId { get; set; }

        public string ISActive { get; set; } = null!;

        
    }
}
