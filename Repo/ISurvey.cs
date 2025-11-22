using SurveyApp.Models;
using System.Security.Cryptography.Xml;

namespace SurveyApp.Repo
{
    public interface ISurvey
    {
        List<SurveyModel> GetAllSurveys(int UserId);
        SurveyModel? GetSurveyById(Int64 surveyId);
        bool AddSurvey(SurveyModel survey);
        bool UpdateSurvey(SurveyModel survey);
        bool DeleteSurvey(Int64 surveyId);
        List<SurveyLocationModel> GetSurveyLocationById(Int64 surveyId);
        SurveyLocationModel? GetSurveyLocationByLocId(int locId);
        bool AddSurveyLocation(SurveyLocationModel location);
        bool UpdateSurveyLocation(SurveyLocationModel location);
        bool DeleteSurveyLocation(int locId);
        bool CreateLocationsBySurveyId(Int64 surveyId, List<SurveyLocationModel> locations, int createdBy);
        List<ItemTypeMasterModel> GetItemTypeMaster(int locId);
        //bool SaveItemTypesForLocation(Int64 surveyId, string surveyName, int locId, List<int> itemTypeIds);
        List<ItemTypeMasterModel> GetSelectedItemTypesForLocation(int locId);
        bool SaveItemTypesForLocation(Int64 surveyId, string surveyName, int locId, List<int> itemTypeIds, int createdBy);

        List<AssignedItemsListModel> GetItemTypebySurveyLoc(int LocId, Int64 SurveyID);

        bool UpdateAssignedItems(AssignedItemsModel model);

        List<SurveyAssignmentModel>? GetSurveyAssignments(Int64 surveyId);
        bool AddSurveyAssignment(SurveyAssignmentModel assignment);
        bool UpdateSurveyAssignment(SurveyAssignmentModel assignment);
        bool DeleteSurveyAssignment(int transId);


        //------------------- Survey Details -------------------//

        List<SurveyDetailsLocationModel> GetAssignedTypeList(Int64 SurveyID, int LocId);

        List<SurveyDetailsModel> GetAssignedItemList(Int64 SurveyID, int LocId, int ItemTypeID);

        List<SurveyDetailsUpdatelist> GetSurveyUpdateItemList(Int64 SurveyID, int LocId, int ItemTypeID);

        public bool UpdateSurveyDetails(SurveyDetailsUpdate model);

        bool MarkLocationAsCompleted(Int64 surveyId, int locId, int userId);
    }
}

