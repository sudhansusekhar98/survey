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
                cmd.Parameters.AddWithValue("@EmpID", obj.EmpID ?? (object)DBNull.Value);

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
                cmd.Parameters.AddWithValue("@EmpID", obj.EmpID ?? (object)DBNull.Value);

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
                string query = "SELECT EmpID, EmpName, EmpCode, Email, MobileNo, IsActive, CreatedOn, CreatedBy FROM EmpMaster";
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

        public List<EmpMasterModel> GetAvailableEmployeesForUserCreation(int? currentUserId = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                string query = @"SELECT e.EmpID, e.EmpName, e.EmpCode, e.Email, e.MobileNo, e.IsActive, e.CreatedOn, e.CreatedBy 
                                FROM EmpMaster e
                                WHERE e.EmpID NOT IN (
                                    SELECT EmpID FROM LoginMaster WHERE EmpID IS NOT NULL" +
                                    (currentUserId.HasValue ? " AND UserID != @CurrentUserId" : "") + @"
                                )";
                using var cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                
                if (currentUserId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId.Value);
                }

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<EmpMasterModel> Employees = SqlDbHelper.DataTableToList<EmpMasterModel>(dt);
                return Employees;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public EmpMasterModel GetEmployeeById(int empId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                string query = "SELECT EmpID, EmpName, EmpCode, Email, MobileNo, IsActive FROM EmpMaster WHERE EmpID = @EmpID";
                using var cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@EmpID", empId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    List<EmpMasterModel> employees = SqlDbHelper.DataTableToList<EmpMasterModel>(dt);
                    return employees.FirstOrDefault();
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public int? GetEmpIdByUserId(int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                string query = "SELECT EmpID FROM LoginMaster WHERE UserID = @UserID AND EmpID IS NOT NULL";
                using var cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@UserID", userId);

                con.Open();
                var result = cmd.ExecuteScalar();
                
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                return null;
            }
            catch (Exception ex)
            {
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

        #region Device Modules (ItemTypeMaster) Methods

        // SpType = 5: Get all Device Modules
        public List<DeviceModuleViewModel> GetAllDeviceModules(bool activeOnly = false)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5);

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                var modules = new List<DeviceModuleViewModel>();
                foreach (DataRow row in dt.Rows)
                {
                    var module = new DeviceModuleViewModel
                    {
                        Id = dt.Columns.Contains("Id") && row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                        Name = dt.Columns.Contains("TypeName") && row["TypeName"] != DBNull.Value ? Convert.ToString(row["TypeName"]) ?? string.Empty : string.Empty,
                        Description = dt.Columns.Contains("TypeDesc") && row["TypeDesc"] != DBNull.Value ? Convert.ToString(row["TypeDesc"]) : null,
                        GroupName = dt.Columns.Contains("GroupName") && row["GroupName"] != DBNull.Value ? Convert.ToString(row["GroupName"]) : null,
                        IsActive = dt.Columns.Contains("IsActive") && row["IsActive"] != DBNull.Value && Convert.ToString(row["IsActive"]) == "1"
                    };
                    
                    if (!activeOnly || module.IsActive)
                    {
                        modules.Add(module);
                    }
                }
                return modules;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 4: Get single Device Module by Id
        public DeviceModuleViewModel? GetDeviceModuleById(int id)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 4);
                cmd.Parameters.AddWithValue("@Id", id);

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0) return null;

                var row = dt.Rows[0];
                return new DeviceModuleViewModel
                {
                    Id = dt.Columns.Contains("Id") && row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                    Name = dt.Columns.Contains("TypeName") && row["TypeName"] != DBNull.Value ? Convert.ToString(row["TypeName"]) ?? string.Empty : string.Empty,
                    Description = dt.Columns.Contains("TypeDesc") && row["TypeDesc"] != DBNull.Value ? Convert.ToString(row["TypeDesc"]) : null,
                    GroupName = dt.Columns.Contains("GroupName") && row["GroupName"] != DBNull.Value ? Convert.ToString(row["GroupName"]) : null,
                    IsActive = dt.Columns.Contains("IsActive") && row["IsActive"] != DBNull.Value && Convert.ToString(row["IsActive"]) == "1"
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 1: Insert Device Module
        public bool CreateDeviceModule(DeviceModuleViewModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 1);
                cmd.Parameters.AddWithValue("@TypeName", model.Name);
                cmd.Parameters.AddWithValue("@TypeDesc", (object?)model.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GroupName", (object?)model.GroupName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive ? "1" : "0");
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using var reader = cmd.ExecuteReader();
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 2: Update Device Module
        public bool UpdateDeviceModule(DeviceModuleViewModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2);
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@TypeName", model.Name);
                cmd.Parameters.AddWithValue("@TypeDesc", (object?)model.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GroupName", (object?)model.GroupName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive ? "1" : "0");
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using var reader = cmd.ExecuteReader();
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 3: Soft Delete Device Module
        public bool DeleteDeviceModule(int id, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 3);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using var reader = cmd.ExecuteReader();
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region Devices (ItemMaster) Methods

        // SpType = 10: Get all Devices
        public List<DeviceViewModel> GetAllDevices(int? moduleId = null, bool activeOnly = false)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 10);
                if (moduleId.HasValue)
                {
                    cmd.Parameters.AddWithValue("@TypeId", moduleId.Value);
                }

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                var devices = new List<DeviceViewModel>();
                foreach (DataRow row in dt.Rows)
                {
                    var device = new DeviceViewModel
                    {
                        ItemId = dt.Columns.Contains("ItemId") && row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                        ModuleId = dt.Columns.Contains("TypeId") && row["TypeId"] != DBNull.Value ? Convert.ToInt32(row["TypeId"]) : 0,
                        Name = dt.Columns.Contains("ItemName") && row["ItemName"] != DBNull.Value ? Convert.ToString(row["ItemName"]) ?? string.Empty : string.Empty,
                        Code = dt.Columns.Contains("ItemCode") && row["ItemCode"] != DBNull.Value ? Convert.ToString(row["ItemCode"]) ?? string.Empty : string.Empty,
                        UOM = dt.Columns.Contains("ItemUOM") && row["ItemUOM"] != DBNull.Value ? Convert.ToString(row["ItemUOM"]) ?? string.Empty : string.Empty,
                        Description = dt.Columns.Contains("ItemDesc") && row["ItemDesc"] != DBNull.Value ? Convert.ToString(row["ItemDesc"]) : null,
                        SequenceNo = dt.Columns.Contains("SqNo") && row["SqNo"] != DBNull.Value ? Convert.ToInt32(row["SqNo"]) : (int?)null,
                        IsActive = dt.Columns.Contains("IsActive") && row["IsActive"] != DBNull.Value && Convert.ToString(row["IsActive"]) == "1",
                        ModuleName = dt.Columns.Contains("TypeName") && row["TypeName"] != DBNull.Value ? Convert.ToString(row["TypeName"]) : null
                    };
                    
                    if (!activeOnly || device.IsActive)
                    {
                        devices.Add(device);
                    }
                }
                return devices;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 9: Get single Device by ItemId
        public DeviceViewModel? GetDeviceById(int itemId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 9);
                cmd.Parameters.AddWithValue("@ItemId", itemId);

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0) return null;

                var row = dt.Rows[0];
                return new DeviceViewModel
                {
                    ItemId = dt.Columns.Contains("ItemId") && row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                    ModuleId = dt.Columns.Contains("TypeId") && row["TypeId"] != DBNull.Value ? Convert.ToInt32(row["TypeId"]) : 0,
                    Name = dt.Columns.Contains("ItemName") && row["ItemName"] != DBNull.Value ? Convert.ToString(row["ItemName"]) ?? string.Empty : string.Empty,
                    Code = dt.Columns.Contains("ItemCode") && row["ItemCode"] != DBNull.Value ? Convert.ToString(row["ItemCode"]) ?? string.Empty : string.Empty,
                    UOM = dt.Columns.Contains("ItemUOM") && row["ItemUOM"] != DBNull.Value ? Convert.ToString(row["ItemUOM"]) ?? string.Empty : string.Empty,
                    Description = dt.Columns.Contains("ItemDesc") && row["ItemDesc"] != DBNull.Value ? Convert.ToString(row["ItemDesc"]) : null,
                    SequenceNo = dt.Columns.Contains("SqNo") && row["SqNo"] != DBNull.Value ? Convert.ToInt32(row["SqNo"]) : (int?)null,
                    IsActive = dt.Columns.Contains("IsActive") && row["IsActive"] != DBNull.Value && Convert.ToString(row["IsActive"]) == "1",
                    ModuleName = dt.Columns.Contains("TypeName") && row["TypeName"] != DBNull.Value ? Convert.ToString(row["TypeName"]) : null
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 6: Insert Device
        public bool CreateDevice(DeviceViewModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 6);
                cmd.Parameters.AddWithValue("@TypeId", model.ModuleId);
                cmd.Parameters.AddWithValue("@ItemName", model.Name);
                cmd.Parameters.AddWithValue("@ItemCode", model.Code);
                cmd.Parameters.AddWithValue("@ItemUOM", model.UOM);
                cmd.Parameters.AddWithValue("@ItemDesc", (object?)model.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SqNo", (object?)model.SequenceNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive ? "1" : "0");
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                 using var reader = cmd.ExecuteReader();
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 7: Update Device
        public bool UpdateDevice(DeviceViewModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 7);
                cmd.Parameters.AddWithValue("@ItemId", model.ItemId);
                cmd.Parameters.AddWithValue("@TypeId", model.ModuleId);
                cmd.Parameters.AddWithValue("@ItemName", model.Name);
                cmd.Parameters.AddWithValue("@ItemCode", model.Code);
                cmd.Parameters.AddWithValue("@ItemUOM", model.UOM);
                cmd.Parameters.AddWithValue("@ItemDesc", (object?)model.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SqNo", (object?)model.SequenceNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive ? "1" : "0");
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                using var reader = cmd.ExecuteReader();
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // SpType = 8: Soft Delete Device
        public bool DeleteDevice(int itemId, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpAdminItemMasterType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 8);
                cmd.Parameters.AddWithValue("@ItemId", itemId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                con.Open();
                 using var reader = cmd.ExecuteReader();
                return reader.HasRows;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion
    }

}
