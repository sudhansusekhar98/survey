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
                System.Diagnostics.Debug.WriteLine($"AdminRepo.ChangePassword called - userId: {userId}");
                
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();
                System.Diagnostics.Debug.WriteLine($"Database connection opened successfully");
                
                // First verify the current password
                using var verifyCmd = new SqlCommand(@"
                    SELECT COUNT(1) FROM LoginMaster 
                    WHERE UserID = @UserID AND LoginPassword = @CurrentPassword", con);
                verifyCmd.Parameters.AddWithValue("@UserID", userId);
                verifyCmd.Parameters.AddWithValue("@CurrentPassword", currentPassword);
                
                int count = Convert.ToInt32(verifyCmd.ExecuteScalar());
                System.Diagnostics.Debug.WriteLine($"Password verification count: {count}");
                
                if (count == 0)
                {
                    // Current password doesn't match
                    System.Diagnostics.Debug.WriteLine($"Current password verification FAILED for userId: {userId}");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine($"Current password verified. Updating to new password...");
                
                // Update to new password and clear MustChangePassword flag
                using var updateCmd = new SqlCommand(@"
                    UPDATE LoginMaster 
                    SET LoginPassword = @NewPassword, 
                        MustChangePassword = 0 
                    WHERE UserID = @UserID", con);
                updateCmd.Parameters.AddWithValue("@UserID", userId);
                updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                
                int result = updateCmd.ExecuteNonQuery();
                System.Diagnostics.Debug.WriteLine($"Update query executed. Rows affected: {result}");
                
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in ChangePassword: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // log ex.ToString()
                throw;
            }
        }

        public bool ResetPasswordWithFlag(int userId, string temporaryPassword)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();
                
                // Update password and set MustChangePassword flag
                using var cmd = new SqlCommand(@"
                    UPDATE LoginMaster 
                    SET LoginPassword = @Password, 
                        MustChangePassword = 1 
                    WHERE UserID = @UserID", con);
                
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Password", temporaryPassword);

                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public bool ClearMustChangePasswordFlag(int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();
                
                using var cmd = new SqlCommand(@"
                    UPDATE LoginMaster 
                    SET MustChangePassword = 0 
                    WHERE UserID = @UserID", con);
                
                cmd.Parameters.AddWithValue("@UserID", userId);

                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                return false;
            }
        }

        /// <summary>
        /// Sync employees from EmpMaster to LoginMaster with temporary passwords
        /// Creates login accounts for employees who don't have one
        /// </summary>
        public (int synced, int skipped, List<string> errors) SyncEmployeesToLoginMaster(string defaultPassword, int defaultRoleId, int createdBy)
        {
            int syncedCount = 0;
            int skippedCount = 0;
            var errors = new List<string>();

            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();

                // Get active employees without login accounts
                var getEmployeesQuery = @"
                    SELECT e.EmpID, e.EmpCode, e.EmpName, e.MobileNo, e.Email
                    FROM EmpMaster e
                    WHERE e.IsActive = 1 
                    AND NOT EXISTS (
                        SELECT 1 FROM LoginMaster l WHERE l.EmpID = e.EmpID
                    )
                    ORDER BY e.EmpCode";

                using var getCmd = new SqlCommand(getEmployeesQuery, con);
                using var reader = getCmd.ExecuteReader();

                var employeesToSync = new List<(int empId, string empCode, string empName, string mobileNo, string email)>();
                
                while (reader.Read())
                {
                    employeesToSync.Add((
                        empId: Convert.ToInt32(reader["EmpID"]),
                        empCode: reader["EmpCode"]?.ToString() ?? "",
                        empName: reader["EmpName"]?.ToString() ?? "",
                        mobileNo: reader["MobileNo"]?.ToString() ?? "",
                        email: reader["Email"]?.ToString() ?? ""
                    ));
                }
                reader.Close();

                // Insert each employee into LoginMaster
                foreach (var emp in employeesToSync)
                {
                    try
                    {
                        // Generate LoginID from EmpCode (or EmpName if EmpCode is empty)
                        string loginId = !string.IsNullOrWhiteSpace(emp.empCode) 
                            ? emp.empCode 
                            : emp.empName.Replace(" ", "").ToLower();

                        // Check if LoginID already exists
                        var checkQuery = "SELECT COUNT(1) FROM LoginMaster WHERE LoginID = @LoginID";
                        using var checkCmd = new SqlCommand(checkQuery, con);
                        checkCmd.Parameters.AddWithValue("@LoginID", loginId);
                        int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (existingCount > 0)
                        {
                            // LoginID already exists, append EmpID to make it unique
                            loginId = $"{loginId}_{emp.empId}";
                        }

                        var insertQuery = @"
                            INSERT INTO LoginMaster 
                            (LoginID, LoginName, EmpID, EmailID, MobileNo, LoginPassword, 
                             RoleID, ISActive, CreateBy, CreateDate, MustChangePassword)
                            VALUES 
                            (@LoginID, @LoginName, @EmpID, @EmailID, @MobileNo, @LoginPassword, 
                             @RoleID, @IsActive, @CreateBy, GETDATE(), 1)";

                        using var insertCmd = new SqlCommand(insertQuery, con);
                        insertCmd.Parameters.AddWithValue("@LoginID", loginId);
                        insertCmd.Parameters.AddWithValue("@LoginName", emp.empName);
                        insertCmd.Parameters.AddWithValue("@EmpID", emp.empId);
                        insertCmd.Parameters.AddWithValue("@EmailID", string.IsNullOrWhiteSpace(emp.email) ? DBNull.Value : emp.email);
                        insertCmd.Parameters.AddWithValue("@MobileNo", string.IsNullOrWhiteSpace(emp.mobileNo) ? DBNull.Value : emp.mobileNo);
                        insertCmd.Parameters.AddWithValue("@LoginPassword", defaultPassword);
                        insertCmd.Parameters.AddWithValue("@RoleID", defaultRoleId);
                        insertCmd.Parameters.AddWithValue("@IsActive", "1");
                        insertCmd.Parameters.AddWithValue("@CreateBy", createdBy.ToString());

                        int result = insertCmd.ExecuteNonQuery();
                        
                        if (result > 0)
                        {
                            syncedCount++;
                            System.Diagnostics.Debug.WriteLine($"Synced employee: {emp.empName} (LoginID: {loginId})");
                        }
                        else
                        {
                            skippedCount++;
                            errors.Add($"Failed to create login for {emp.empName}");
                        }
                    }
                    catch (Exception empEx)
                    {
                        skippedCount++;
                        errors.Add($"Error syncing {emp.empName}: {empEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Error syncing {emp.empName}: {empEx.Message}");
                    }
                }

                return (syncedCount, skippedCount, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Database error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SyncEmployeesToLoginMaster error: {ex.ToString()}");
                return (syncedCount, skippedCount, errors);
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
                
                // Use direct SQL to ensure MustChangePassword is returned
                using var cmd = new SqlCommand(@"
                    SELECT 
                        UserID as UserId,
                        LoginID as LoginId,
                        LoginName,
                        LoginPassword,
                        RoleID as RoleId,
                        ISActive,
                        ProfilePictureUrl,
                        ProfilePicturePublicId,
                        ISNULL(MustChangePassword, 0) as MustChangePassword
                    FROM LoginMaster 
                    WHERE LoginID = @LoginId 
                        AND LoginPassword = @LoginPassword
                        AND ISActive = 'Y'", con);
                
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

        /// <summary>
        /// Get user rights by UserID for menu filtering - returns rights with IsView = 'Y'
        /// </summary>
        public List<UsersRightsModel> GetUserRightsByUserId(int userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetUserRightsByUserId called for userId: {userId}");
                
                using var con = new SqlConnection(DBConnection.ConnectionString);
                
                // Get user rights where IsView = 'Y' (user has view permission)
                var query = @"
                    SELECT 
                        ur.UserRightsID as Srno,
                        ur.RightsID,
                        rm.RightsName,
                        ur.RegionID,
                        CASE WHEN ur.IsView = 'Y' THEN 1 ELSE 0 END as IsView,
                        CASE WHEN ur.IsCreate = 'Y' THEN 1 ELSE 0 END as IsCreate,
                        CASE WHEN ur.IsUpdate = 'Y' THEN 1 ELSE 0 END as IsUpdate,
                        CASE WHEN ur.IsExecute = 'Y' THEN 1 ELSE 0 END as IsExecute,
                        CASE WHEN ur.IsDelete = 'Y' THEN 1 ELSE 0 END as IsDelete
                    FROM UserRightsMaster ur
                    INNER JOIN RightsMaster rm ON ur.RightsID = rm.RightsID
                    WHERE ur.UserID = @UserID 
                        AND ur.IsActive = 'Y' 
                        AND ur.IsView = 'Y'
                        AND rm.IsActive = 'Y'";
                
                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                
                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                
                var result = SqlDbHelper.DataTableToList<UsersRightsModel>(dt);
                System.Diagnostics.Debug.WriteLine($"GetUserRightsByUserId returned {result.Count} rights for userId: {userId}");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserRightsByUserId error for userId {userId}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<UsersRightsModel>();
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
                    cmd.Parameters.AddWithValue("@IsExecute", right.IsExecute ? 'Y' : 'N');
                    cmd.Parameters.AddWithValue("@IsDelete", right.IsDelete ? 'Y' : 'N');
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

        #region Item Specifications Management

        /// <summary>
        /// Get all specifications, optionally filtered by item ID
        /// </summary>
        public List<ItemSpecificationModel> GetAllSpecifications(int? itemId = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"
                    SELECT ism.ItemId, ism.SpecificationID, ism.SpecificationName, ism.InputType, 
                           ism.ConditionalDisplay, ism.AllowMultipleInstances,
                           im.ItemName
                    FROM ItemSpecificationMaster ism
                    LEFT JOIN ItemMaster im ON ism.ItemId = im.ItemId
                    WHERE (@ItemId IS NULL OR ism.ItemId = @ItemId)
                    ORDER BY ism.ItemId, ism.SpecificationID";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ItemId", (object?)itemId ?? DBNull.Value);

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                var specs = new List<ItemSpecificationModel>();
                foreach (DataRow row in dt.Rows)
                {
                    // Skip rows where SpecificationID is null
                    if (row["SpecificationID"] == DBNull.Value) continue;
                    
                    specs.Add(new ItemSpecificationModel
                    {
                        ItemId = row["ItemId"] != DBNull.Value ? Convert.ToInt32(row["ItemId"]) : 0,
                        SpecificationID = Convert.ToInt32(row["SpecificationID"]),
                        SpecificationName = row["SpecificationName"]?.ToString() ?? "",
                        InputType = row["InputType"]?.ToString(),
                        ConditionalDisplay = row["ConditionalDisplay"]?.ToString(),
                        AllowMultipleInstances = row["AllowMultipleInstances"] != DBNull.Value && Convert.ToBoolean(row["AllowMultipleInstances"]),
                        Options = row["ItemName"]?.ToString() // Store item name in Options temporarily for display
                    });
                }
                return specs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting specifications: {ex.Message}");
                return new List<ItemSpecificationModel>();
            }
        }

        /// <summary>
        /// Get a single specification by ID
        /// </summary>
        public ItemSpecificationModel? GetSpecificationById(int specificationId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"
                    SELECT ItemId, SpecificationID, SpecificationName, InputType, 
                           ConditionalDisplay, AllowMultipleInstances
                    FROM ItemSpecificationMaster
                    WHERE SpecificationID = @SpecificationID";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@SpecificationID", specificationId);

                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new ItemSpecificationModel
                    {
                        ItemId = Convert.ToInt32(reader["ItemId"]),
                        SpecificationID = Convert.ToInt32(reader["SpecificationID"]),
                        SpecificationName = reader["SpecificationName"]?.ToString() ?? "",
                        InputType = reader["InputType"]?.ToString(),
                        ConditionalDisplay = reader["ConditionalDisplay"]?.ToString(),
                        AllowMultipleInstances = reader["AllowMultipleInstances"] != DBNull.Value && Convert.ToBoolean(reader["AllowMultipleInstances"])
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting specification by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a new specification
        /// </summary>
        public bool CreateSpecification(ItemSpecificationModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                // SpecificationID is not an identity column, so we need to generate the next ID
                var sql = @"
                    DECLARE @NewSpecId INT = (SELECT ISNULL(MAX(SpecificationID), 0) + 1 FROM ItemSpecificationMaster);
                    INSERT INTO ItemSpecificationMaster (SpecificationID, ItemId, SpecificationName, InputType, ConditionalDisplay, AllowMultipleInstances)
                    VALUES (@NewSpecId, @ItemId, @SpecificationName, @InputType, @ConditionalDisplay, @AllowMultipleInstances)";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@ItemId", model.ItemId);
                cmd.Parameters.AddWithValue("@SpecificationName", model.SpecificationName);
                cmd.Parameters.AddWithValue("@InputType", (object?)model.InputType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ConditionalDisplay", (object?)model.ConditionalDisplay ?? "Always");
                cmd.Parameters.AddWithValue("@AllowMultipleInstances", model.AllowMultipleInstances);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating specification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update an existing specification
        /// </summary>
        public bool UpdateSpecification(ItemSpecificationModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"
                    UPDATE ItemSpecificationMaster
                    SET ItemId = @ItemId, 
                        SpecificationName = @SpecificationName, 
                        InputType = @InputType, 
                        ConditionalDisplay = @ConditionalDisplay, 
                        AllowMultipleInstances = @AllowMultipleInstances
                    WHERE SpecificationID = @SpecificationID";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@SpecificationID", model.SpecificationID);
                cmd.Parameters.AddWithValue("@ItemId", model.ItemId);
                cmd.Parameters.AddWithValue("@SpecificationName", model.SpecificationName);
                cmd.Parameters.AddWithValue("@InputType", (object?)model.InputType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ConditionalDisplay", (object?)model.ConditionalDisplay ?? "Always");
                cmd.Parameters.AddWithValue("@AllowMultipleInstances", model.AllowMultipleInstances);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating specification: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a specification and its options
        /// </summary>
        public bool DeleteSpecification(int specificationId, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();
                using var transaction = con.BeginTransaction();

                try
                {
                    // First delete all options for this specification
                    using var cmdOptions = new SqlCommand(
                        "DELETE FROM ItemSpecificationOptionsMaster WHERE SpecificationID = @SpecificationID", 
                        con, transaction);
                    cmdOptions.Parameters.AddWithValue("@SpecificationID", specificationId);
                    cmdOptions.ExecuteNonQuery();

                    // Then delete the specification
                    using var cmdSpec = new SqlCommand(
                        "DELETE FROM ItemSpecificationMaster WHERE SpecificationID = @SpecificationID", 
                        con, transaction);
                    cmdSpec.Parameters.AddWithValue("@SpecificationID", specificationId);
                    cmdSpec.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting specification: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Specification Options Management

        /// <summary>
        /// Get all options for a specification
        /// </summary>
        public List<SpecificationOptionModel> GetAllSpecificationOptions(int specificationId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"
                    SELECT OptionID, SpecificationID, OptionValue, OptionText, DisplayOrder, IsActive
                    FROM ItemSpecificationOptionsMaster
                    WHERE SpecificationID = @SpecificationID
                    ORDER BY DisplayOrder";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@SpecificationID", specificationId);

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                return SqlDbHelper.DataTableToList<SpecificationOptionModel>(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting specification options: {ex.Message}");
                return new List<SpecificationOptionModel>();
            }
        }

        /// <summary>
        /// Get a single option by ID
        /// </summary>
        public SpecificationOptionModel? GetSpecificationOptionById(int optionId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"
                    SELECT OptionID, SpecificationID, OptionValue, OptionText, DisplayOrder, IsActive
                    FROM ItemSpecificationOptionsMaster
                    WHERE OptionID = @OptionID";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@OptionID", optionId);

                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new SpecificationOptionModel
                    {
                        OptionID = Convert.ToInt32(reader["OptionID"]),
                        SpecificationID = Convert.ToInt32(reader["SpecificationID"]),
                        OptionValue = reader["OptionValue"]?.ToString() ?? "",
                        OptionText = reader["OptionText"]?.ToString() ?? "",
                        DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                        IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"])
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting option by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a new specification option
        /// </summary>
        public bool CreateSpecificationOption(SpecificationOptionModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"
                    INSERT INTO ItemSpecificationOptionsMaster (SpecificationID, OptionValue, OptionText, DisplayOrder, IsActive)
                    VALUES (@SpecificationID, @OptionValue, @OptionText, @DisplayOrder, @IsActive)";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@SpecificationID", model.SpecificationID);
                cmd.Parameters.AddWithValue("@OptionValue", model.OptionValue);
                cmd.Parameters.AddWithValue("@OptionText", model.OptionText);
                cmd.Parameters.AddWithValue("@DisplayOrder", model.DisplayOrder);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating option: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Update an existing specification option
        /// </summary>
        public bool UpdateSpecificationOption(SpecificationOptionModel model, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"
                    UPDATE ItemSpecificationOptionsMaster
                    SET OptionValue = @OptionValue, 
                        OptionText = @OptionText, 
                        DisplayOrder = @DisplayOrder, 
                        IsActive = @IsActive
                    WHERE OptionID = @OptionID";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@OptionID", model.OptionID);
                cmd.Parameters.AddWithValue("@OptionValue", model.OptionValue);
                cmd.Parameters.AddWithValue("@OptionText", model.OptionText);
                cmd.Parameters.AddWithValue("@DisplayOrder", model.DisplayOrder);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating option: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Delete a specification option
        /// </summary>
        public bool DeleteSpecificationOption(int optionId, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = "DELETE FROM ItemSpecificationOptionsMaster WHERE OptionID = @OptionID";

                using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@OptionID", optionId);

                con.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting option: {ex.Message}");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Get all Super Users (RoleId = 101) with active status for email notifications
        /// </summary>
        public List<UserModel> GetSuperUsers()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                var sql = @"SELECT UserID as UserId, LoginId, LoginName, LoginPassword, RoleID as RoleId, 
                            EmpID, MobileNo, EmailID, IsActive as ISActive
                            FROM LoginMaster 
                            WHERE RoleID = 101 AND IsActive = 'Y' AND EmailID IS NOT NULL AND EmailID != ''";

                using var cmd = new SqlCommand(sql, con);
                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                return SqlDbHelper.DataTableToList<UserModel>(dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting super users: {ex.Message}");
                return new List<UserModel>();
            }
        }
    }

}
