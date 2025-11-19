
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using AnalyticaDocs.Models;
using AnalyticaDocs.Util;

namespace AnalyticaDocs.Repository
{
    public class CommonUtil : ICommonUtil   
    {
        
        // Fix for CS0736: Convert static methods to instance methods to implement the interface correctly.
        public IActionResult CheckAuthorization(Controller controller, string requiredRole)
        {
            var sessionUserId = controller.HttpContext.Session.GetString("UserID");
            var sessionRoleId = controller.HttpContext.Session.GetString("RoleId");

            // ⏱️ Session Timeout Check
            if (string.IsNullOrEmpty(sessionUserId) || string.IsNullOrEmpty(sessionRoleId))
            {
                return controller.View("~/Views/Shared/SessionTimeout.cshtml");
            }

            // 🔐 Role Authorization Check
            if (sessionRoleId != requiredRole)
            {
                return controller.View("~/Views/Shared/Unauthorized.cshtml");
            }

            // Authorized
            return null;
        }

        public IActionResult CheckAuthorization(Controller controller, params string[] allowedRoles)
        {
            var sessionUserId = controller.HttpContext.Session.GetString("UserID");
            var sessionRoleId = controller.HttpContext.Session.GetString("RoleId");

            if (string.IsNullOrEmpty(sessionUserId) || string.IsNullOrEmpty(sessionRoleId))
                return controller.View("~/Views/Shared/SessionTimeout.cshtml");

            if (!allowedRoles.Contains(sessionRoleId))
                return controller.View("~/Views/Shared/Unauthorized.cshtml");

            return null;
        }

        public IActionResult CheckAuthorizationAll(Controller controller, int RightsId, int? RegionId, Int64? SurveyId, string Type)
        {
            var sessionUserId = controller.HttpContext.Session.GetString("UserID");
            var sessionRoleId = controller.HttpContext.Session.GetString("RoleId");

            // ⏱️ Session Timeout Check
            if (string.IsNullOrEmpty(sessionUserId) || string.IsNullOrEmpty(sessionRoleId))
            {
                return controller.View("~/Views/Shared/SessionTimeout.cshtml");
            }

            bool IsAuthorized = CheckUserRightsScalar(Convert.ToInt32(sessionUserId), RightsId, RegionId, SurveyId, Type);

            // 🔐 Role Authorization Check
            if (!IsAuthorized)
            {
                return controller.View("~/Views/Shared/Unauthorized.cshtml");
            }

            // Authorized
            return null;
        }

        public bool CheckUserRightsScalar(int sessionUserId, int rightsId, int? regionId, long? surveyId, string type)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);

                string query = "SELECT dbo.IsAuthorized(@SessionUserId,@RightsId,@RegionId,@SurveyId,@Type)";

                using var cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@SessionUserId", sessionUserId);
                cmd.Parameters.AddWithValue("@RightsId", rightsId);
                cmd.Parameters.AddWithValue("@RegionId", regionId.HasValue ? (object)regionId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@SurveyId", surveyId.HasValue ? (object)surveyId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Type", type);

                con.Open();
                var result = cmd.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int isAuthorized))
                {
                    return isAuthorized == 1;
                }
                else
                {
                    return false;
                }


            }
            catch (Exception)
            {
                throw;
            }
        }
        public List<DepartmentList> GetDepartment()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpCommonOptions", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 1);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<DepartmentList> records = SqlDbHelper.DataTableToList<DepartmentList>(dt);
                return records;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

       

        public List<CostPeriod> GetPeriodOptions()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpCommonOptions", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 3);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<CostPeriod> records = SqlDbHelper.DataTableToList<CostPeriod>(dt);
                return records;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public List<UsersList> GetUserOptions()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpCommonOptions", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<UsersList> records = SqlDbHelper.DataTableToList<UsersList>(dt);
                return records;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

       

        public CostPeriod GetPeriodDetailByID(int recordId)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("dbo.SpCommonOptions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SpType", 3);

            con.Open();

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            List<CostPeriod> records = SqlDbHelper.DataTableToList<CostPeriod>(dt);
            return records.FirstOrDefault();
        }

        public List<ItemMaster> GetItemOptions()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpCommonOptions", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 4);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<ItemMaster> records = SqlDbHelper.DataTableToList<ItemMaster>(dt);
                return records;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public ReportStatusModel GetReportStatus(DateOnly ProductionDate)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("dbo.SpCommonOptions", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SpType", 5);
            cmd.Parameters.AddWithValue("@ProductionDate", ProductionDate);

            con.Open();

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            List<ReportStatusModel> records = SqlDbHelper.DataTableToList<ReportStatusModel>(dt);
            return records.FirstOrDefault();
        }
    }
}
