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
    }
}
