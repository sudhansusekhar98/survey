namespace SurveyApp.Models
{
    public class EmpMasterModel
    {
        public int EmpID { get; set; }
        public string EmpName { get; set; } = string.Empty;
        public string EmpCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }
}
