using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public interface IClientMaster
    {
        List<ClientMasterModel> GetAllClients();
        ClientMasterModel? GetClientById(int clientId);
        int InsertClient(ClientMasterModel model);
        bool UpdateClient(ClientMasterModel model);
        bool DeleteClient(int clientId);
    }
}
