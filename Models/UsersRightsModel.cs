using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnalyticaDocs.Models
{
    public class UsersRightsModel
    {
        public int Srno { get; set; }
        public int DeptID { get; set; }

        public int RightsID { get; set; }

        public int RegionID { get; set; }

        public string? RegionName { get; set; }

        public string? RightsName { get; set; }
        public bool IsView { get; set; } = false;
        public bool IsCreate { get; set; } = false;
        public bool IsUpdate { get; set; } = false;
        public string? DeptName { get; set; }

    }

    public class UsersRightsFormModel
    {
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public int? RoleID { get; set; }
        public char IsActive { get; set; } = 'Y';
        public int CreateBy { get; set; }
        public List<UsersRightsModel> RightsList { get; set; } = new List<UsersRightsModel>();
    }

}
