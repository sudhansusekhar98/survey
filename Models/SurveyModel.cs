using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class SurveyModel
    {
        public Int64 SurveyId { get; set; }

        [Required]
        [Display(Name = "Survey Name")]
        public string SurveyName { get; set; } = string.Empty;

        [Display(Name = "Implementation Type")]
        public string? ImplementationType { get; set; }

        [Display(Name = "Survey Date")]
        public DateTime? SurveyDate { get; set; }

        [Display(Name = "Survey Team Name")]
        public string? SurveyTeamName { get; set; }

        [Display(Name = "Survey Team Contact")]
        public string? SurveyTeamContact { get; set; }

        [Display(Name = "Agency Name")]
        public string? AgencyName { get; set; }

        [Display(Name = "Location Site Name")]
        public string? LocationSiteName { get; set; }

        [Display(Name = "State")]
        public int? StateId { get; set; }
        
        [Display(Name = "State")]
        public string? MapMarking { get; set; }

        [Display(Name = "City/District")]
        public int? CityId { get; set; }
        
        [Display(Name = "City/District")]
        public string? CityDistrict { get; set; }

        [Display(Name = "Scope of Work")]
        public string? ScopeOfWork { get; set; }

        [Display(Name = "Latitude")]
        public decimal? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public decimal? Longitude { get; set; }

        [Display(Name = "Survey Status")]
        public string? SurveyStatus { get; set; }
        public int CreatedBy { get; set; }

        [Display(Name = "Region")]
        public int? RegionID { get; set; }
        public string? RegionName { get; set; }

        [Display(Name = "Client")]
        public int? ClientID { get; set; }
        public string? ClientName { get; set; }

        // Options for dropdowns - populated from API
        public List<SelectListItem> StateOptions { get; set; } = new();
        public List<SelectListItem> CityOptions { get; set; } = new();

        // Options for dropdowns
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "Pending", Value = "Pending" },
            new SelectListItem { Text = "In Progress", Value = "In Progress" },
            new SelectListItem { Text = "Completed", Value = "Completed" },
            new SelectListItem { Text = "On Hold", Value = "On Hold" }
        };

        public List<SelectListItem> ImplementationTypeOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "New Project", Value = "New Project" },
            new SelectListItem { Text = "Upgradation", Value = "Upgradation" },
            new SelectListItem { Text = "Extension", Value = "Extension" },
            new SelectListItem { Text = "Restoration", Value = "Restoration" },
            new SelectListItem { Text = "AMC", Value = "AMC"}
        };
    }
}

