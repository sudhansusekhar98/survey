using SurveyApp.Models;
using System.Security.Cryptography.Xml;
using System.Data;

namespace SurveyApp.Repo
{
    public interface ISurvey
    {
        List<SurveyModel> GetAllSurveys(int UserId);
        SurveyModel? GetSurveyById(Int64 surveyId);
        bool AddSurvey(SurveyModel survey);
        bool UpdateSurvey(SurveyModel survey);
        bool UpdateSurveyStatus(Int64 surveyId, string status);
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
        bool UpdateAllSurveyAssignmentsDueDate(Int64 SurveyID, DateTime? DueDate);
        bool DeleteSurveyAssignment(int transId);
        List<SurveyAssignmentModel> GetAllSurveyAssignments(int userId);


        //------------------- Survey Details -------------------//

        List<SurveyDetailsLocationModel> GetAssignedTypeList(Int64 SurveyID, int LocId);

        List<SurveyDetailsModel> GetAssignedItemList(Int64 SurveyID, int LocId, int ItemTypeID);

        List<SurveyDetailsUpdatelist> GetSurveyUpdateItemList(Int64 SurveyID, int LocId, int ItemTypeID);

        public bool UpdateSurveyDetails(SurveyDetailsUpdate model);

        bool MarkLocationAsCompleted(Int64 surveyId, int locId, int userId);

        // Global cable count for location
        void SaveGlobalCableCount(Int64 surveyId, int locId, string cableCount, string remarks, int userId);

        // Survey Submission Methods
        bool SubmitSurvey(Int64 surveyId, int userId);
        bool WithdrawSubmission(Int64 surveyId);
        SurveySubmissionModel? GetSubmissionBySurveyId(Int64 surveyId);
        List<SurveySubmissionModel> GetAllSubmissions(int? userId = null);
        bool UpdateSubmissionStatus(Int64 submissionId, string status, int reviewedBy, string? comments = null);
        bool CanEditSurvey(Int64 surveyId);
        SurveyCompletionStatus CheckSurveyCompletionStatus(Int64 surveyId);

        DataTable GetSurveyDetails(long surveyId, int spType);

        // Item Specification Methods
        /// <summary>
        /// Get all specifications defined for a specific item
        /// </summary>
        List<ItemSpecificationModel> GetItemSpecifications(int itemId);

        /// <summary>
        /// Get saved specification details for a specific survey/location/item combination
        /// </summary>
        List<SpecificationDetailsModel> GetSpecificationDetails(long surveyId, int locId, int itemId);

        /// <summary>
        /// Save or update specification details for a survey item
        /// </summary>
        bool SaveSpecificationDetails(SpecificationDetailsSubmitModel model, int userId);

        /// <summary>
        /// Get dropdown options for a specification from database
        /// </summary>
        List<SpecificationOptionModel> GetSpecificationOptions(int specificationId);

        // Survey-Level Cable Count Methods
        /// <summary>
        /// Get all cable items from ItemMaster (TypeId = Cable category)
        /// </summary>
        List<CableItemModel> GetCableItems();

        /// <summary>
        /// Get saved cable counts for a survey (stored in SurveyDetails with LocID = 0)
        /// </summary>
        List<SurveyCableCountModel> GetSurveyCableCounts(long surveyId);

        /// <summary>
        /// Save cable counts for a survey (stored in SurveyDetails with LocID = 0)
        /// </summary>
        bool SaveSurveyCableCounts(long surveyId, List<SurveyCableCountModel> cableCounts, int userId);
    }
}
