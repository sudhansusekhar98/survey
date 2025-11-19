using System;
using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class SurveyLocationModel
    {
        public int LocID { get; set; }
        public Int64 SurveyID { get; set; }

        [Required]
        [Display(Name = "Location Name")]
        public string LocName { get; set; } = string.Empty;

        [Display(Name = "Latitude")]
        public decimal? LocLat { get; set; } 

        [Display(Name = "Longitude")]
        public decimal? LocLog { get; set; }
        public DateTime? CreateOn { get; set; }
        public int? CreateBy { get; set; }
        public bool Isactive { get; set; }
    }
}
