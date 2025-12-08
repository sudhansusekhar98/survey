using Microsoft.AspNetCore.Mvc;
using AnalyticaDocs.Models;

namespace AnalyticaDocs.Repository
{
    public interface ICommonUtil
    {
        IActionResult CheckAuthorization(Controller controller, string requiredRole);
        IActionResult CheckAuthorization(Controller controller, params string[] allowedRoles);

        IActionResult CheckAuthorizationAll(Controller controller, int RightsId, int? RegionId, Int64? SurveyId, string Type);
        
        /// <summary>
        /// Check if a user has authorization for a specific action without redirecting
        /// </summary>
        bool IsAuthorizedWithAction(int userId, int rightsId, string actionType);
        
        List<DepartmentList> GetDepartment();
        List<UsersList> GetUserOptions();

        List<CostPeriod> GetPeriodOptions();

        List<ItemMaster> GetItemOptions();

        CostPeriod GetPeriodDetailByID(int recordId);

        ReportStatusModel GetReportStatus(DateOnly ProductionDate);
    }
}
