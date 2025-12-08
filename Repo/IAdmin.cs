using AnalyticaDocs.Models;
using SurveyApp.Models;
namespace AnalyticaDocs.Repo
{
    public interface IAdmin
    {
        List<UserModel> GetAllDetails();
        UserLoginModel? GetLoginUser(UserLoginModel credentials);
        List<UserModel> GetRoles();
        bool AddUser(UserModel user);
        UserModel? GetUserById(int id);
        bool UpdateUser(UserModel user);

        bool UpdateProfilePicture(int userId, string profilePictureUrl, string profilePicturePublicId);

        bool ChangePassword(int userId, string currentPassword, string newPassword);
        
        /// <summary>
        /// Reset password with MustChangePassword flag for admin-set temporary passwords
        /// </summary>
        bool ResetPasswordWithFlag(int userId, string temporaryPassword);
        
        /// <summary>
        /// Clear the MustChangePassword flag after user changes their password
        /// </summary>
        bool ClearMustChangePasswordFlag(int userId);

        /// <summary>
        /// Sync employees from EmpMaster to LoginMaster with temporary passwords
        /// </summary>
        (int synced, int skipped, List<string> errors) SyncEmployeesToLoginMaster(string defaultPassword, int defaultRoleId, int createdBy);

        List<UsersRightsModel> GetUserRights(int RecordID);
        
        /// <summary>
        /// Get user rights by UserID for menu filtering
        /// </summary>
        List<UsersRightsModel> GetUserRightsByUserId(int userId);

        bool UpdateRights(UsersRightsFormModel model);
        List<EmpMasterModel> GetEmpMaster();
        List<EmpMasterModel> GetAvailableEmployeesForUserCreation(int? currentUserId = null);
        EmpMasterModel GetEmployeeById(int empId);
        int? GetEmpIdByUserId(int userId);
        List<RegionMasterModel> GetRegionMaster();

        // Device Modules (ItemTypeMaster) methods
        List<DeviceModuleViewModel> GetAllDeviceModules(bool activeOnly = false);
        DeviceModuleViewModel? GetDeviceModuleById(int id);
        bool CreateDeviceModule(DeviceModuleViewModel model, int userId);
        bool UpdateDeviceModule(DeviceModuleViewModel model, int userId);
        bool DeleteDeviceModule(int id, int userId);

        // Devices (ItemMaster) methods
        List<DeviceViewModel> GetAllDevices(int? moduleId = null, bool activeOnly = false);
        DeviceViewModel? GetDeviceById(int itemId);
        bool CreateDevice(DeviceViewModel model, int userId);
        bool UpdateDevice(DeviceViewModel model, int userId);
        bool DeleteDevice(int itemId, int userId);
    }
}
