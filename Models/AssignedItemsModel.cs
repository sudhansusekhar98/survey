using AnalyticaDocs.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyApp.Models
{
    public class AssignedItemsModel
    {
        public Int64 SurveyId { get; set; }

        public string SurveyName { get; set; } = string.Empty;
        public int LocID { get; set; }
        public int? CreatedBy { get; set; }

        public List<AssignedItemsListModel> AssignItemList { get; set; } = new List<AssignedItemsListModel>();
    }
    public class AssignedItemsListModel
    {
        public int ItemTypeID { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string TypeDesc { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;

        public bool IsAssigned { get; set; }
    }

}



