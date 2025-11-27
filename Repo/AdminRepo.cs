using AnalyticaDocs.Models;
using AnalyticaDocs.Util;
using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using System.Data;
namespace AnalyticaDocs.Repo
{
    public class AdminRepo : IAdmin
    {
        public bool AddUser(UserModel obj)
        {
             try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 4);
                cmd.Parameters.AddWithValue("@LoginId", obj.LoginId);
                cmd.Parameters.AddWithValue("@LoginName", obj.LoginName);
                cmd.Parameters.AddWithValue("@MobileNo", obj.MobileNo);
                cmd.Parameters.AddWithValue("@EmailID", obj.EmailID);
                cmd.Parameters.AddWithValue("@LoginPassword", obj.LoginPassword);
                cmd.Parameters.AddWithValue("@IsActive", obj.ISActive);
                cmd.Parameters.AddWithValue("@RoleID", obj.RoleId);
                cmd.Parameters.AddWithValue("@CreateBy", obj.CreateBy);
                //cmd.Parameters.AddWithValue("@EmpID", obj.EmpID);

                con.Open();
                int Resut = cmd.ExecuteNonQuery();
                if (Resut >= 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public bool UpdateUser(UserModel obj)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5);
                cmd.Parameters.AddWithValue("@UserID", obj.UserId);
                cmd.Parameters.AddWithValue("@LoginId", obj.LoginId);
                cmd.Parameters.AddWithValue("@LoginName", obj.LoginName);
                cmd.Parameters.AddWithValue("@MobileNo", obj.MobileNo);
                cmd.Parameters.AddWithValue("@EmailID", obj.EmailID);
                cmd.Parameters.AddWithValue("@LoginPassword", obj.LoginPassword);
                cmd.Parameters.AddWithValue("@IsActive", obj.ISActive);
                cmd.Parameters.AddWithValue("@RoleID", obj.RoleId);
                cmd.Parameters.AddWithValue("@CreateBy", obj.CreateBy);
                //cmd.Parameters.AddWithValue("@EmpID", obj.EmpID);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                if (result >= 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public bool UpdateProfilePicture(int userId, string profilePictureUrl, string profilePicturePublicId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 7); // SpType 7 for profile picture update only
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@ProfilePictureUrl", (object)profilePictureUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProfilePicturePublicId", (object)profilePicturePublicId ?? DBNull.Value);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 8);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@LoginPassword", currentPassword);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                if (result >= 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public List<UserModel> GetAllDetails()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<UserModel> records = SqlDbHelper.DataTableToList<UserModel>(dt);
                return records; 
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public UserLoginModel? GetLoginUser(UserLoginModel obj)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SpType", 1);
                cmd.Parameters.AddWithValue("@LoginId", obj.LoginId);
                cmd.Parameters.AddWithValue("@LoginPassword", obj.LoginPassword);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<UserLoginModel> record = SqlDbHelper.DataTableToList<UserLoginModel>(dt);
                return record.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // TODO: log ex.ToString()
                throw;
            }

        }

        public List<UserModel> GetRoles()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();

                adapter.Fill(dt);
                List<UserModel> records = SqlDbHelper.DataTableToList<UserModel>(dt);
                return records;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public UserModel? GetUserById(int id)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 3);
                cmd.Parameters.AddWithValue("@UserID", id);
                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                List<UserModel> users = SqlDbHelper.DataTableToList<UserModel>(dt);
                return users.FirstOrDefault(); // No await needed here
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public List<UsersRightsModel> GetUserRights(int RecordID)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpUserRights", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2);
                cmd.Parameters.AddWithValue("@UserID", RecordID);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();

                adapter.Fill(dt);
                List<UsersRightsModel> records = SqlDbHelper.DataTableToList<UsersRightsModel>(dt);
                return records;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public bool UpdateRights(UsersRightsFormModel model)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            con.Open();

            using var transaction = con.BeginTransaction();
            try
            {
                foreach (var right in model.RightsList)
                {
                    using var cmd = new SqlCommand("dbo.SpUserRights", con, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SpType", 1);
                    cmd.Parameters.AddWithValue("@UserID", model.UserID);
                    cmd.Parameters.AddWithValue("@RightsID", right.RightsID);
                    cmd.Parameters.AddWithValue("@RegionID", right.RegionID);
                    cmd.Parameters.AddWithValue("@IsView", right.IsView ? 'Y' : 'N' );
                    cmd.Parameters.AddWithValue("@IsCreate", right.IsCreate ? 'Y' : 'N');
                    cmd.Parameters.AddWithValue("@IsUpdate", right.IsUpdate ? 'Y' : 'N');
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive); 
                    cmd.Parameters.AddWithValue("@CreateBy", model.CreateBy); 

                    int result = cmd.ExecuteNonQuery();
                    if (result <= 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }

        //Empolee Master
        public List<EmpMasterModel> GetEmpMaster()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                string query = "SELECT EmpID, EmpName, EmpCode, IsActive, CreatedOn, CreatedBy FROM EmpMaster";
                using var cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<EmpMasterModel> Employees = SqlDbHelper.DataTableToList<EmpMasterModel>(dt);
                return Employees;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public List<RegionMasterModel> GetRegionMaster()
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

                List<RegionMasterModel> Regions = SqlDbHelper.DataTableToList<RegionMasterModel>(dt);
                return Regions;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }
    }

}
