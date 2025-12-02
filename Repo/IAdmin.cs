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

        List<UsersRightsModel> GetUserRights(int RecordID);

        bool UpdateRights(UsersRightsFormModel model);
        List<EmpMasterModel> GetEmpMaster();
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
