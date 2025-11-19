using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Models
{
    public class SurveyAssignmentModel
    {
        [Required]
        public Int64 SurveyID { get; set; }
        public int TransID { get; set; }
        public int EmpID { get; set; }
        public string? EmpName { get; set; }
        public int? CreateBy { get; set; }
        public DateTime DueDate { get; set; }
        [NotMapped]
        public List<int> SelectedEmpIDs { get; set; } // For multi-select binding, not mapped to DB
    }
    

}
