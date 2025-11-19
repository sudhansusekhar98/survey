using Microsoft.AspNetCore.Mvc;
using AnalyticaDocs.Models;

namespace AnalyticaDocs.Repository
{
    public interface ICommonUtil
    {
        IActionResult CheckAuthorization(Controller controller, string requiredRole);
        IActionResult CheckAuthorization(Controller controller, params string[] allowedRoles);

        IActionResult CheckAuthorizationAll(Controller controller, int RightsId, int? RegionId, Int64? SurveyId, string Type);
        List<DepartmentList> GetDepartment();
        List<UsersList> GetUserOptions();

        List<CostPeriod> GetPeriodOptions();

        List<ItemMaster> GetItemOptions();

        CostPeriod GetPeriodDetailByID(int recordId);

        ReportStatusModel GetReportStatus(DateOnly ProductionDate);
    }
}
