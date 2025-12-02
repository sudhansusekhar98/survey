using SurveyApp.Models;
using System.Data;

namespace SurveyApp.Repo
{
    public interface ISurveySubmission
    {
        /// <summary>
        /// Submit or update a survey submission (SpType = 1)
        /// </summary>
        bool SubmitSurvey(Int64 surveyId, int submittedBy, string submissionStatus);

        /// <summary>
        /// Update review status - Approve or Reject (SpType = 2)
        /// </summary>
        bool UpdateReviewStatus(Int64 submissionId, string status, int reviewedBy, string? reviewComments);

        /// <summary>
        /// Delete a submission (SpType = 3)
        /// </summary>
        bool DeleteSubmission(Int64 submissionId);

        /// <summary>
        /// Get all submissions with optional filter by submitted user (SpType = 4)
        /// </summary>
        List<SurveySubmissionModel> GetAllSubmissions(int? submittedBy = null);

        /// <summary>
        /// Get submission details by survey ID (SpType = 5)
        /// </summary>
        SurveySubmissionModel? GetSubmissionBySurveyId(Int64 surveyId);

        /// <summary>
        /// Check if survey can be edited (SpType = 6)
        /// </summary>
        bool CanEditSurvey(Int64 surveyId);

        /// <summary>
        /// Get submissions pending review for supervisor
        /// </summary>
        List<SurveySubmissionModel> GetPendingSubmissionsForReview(int? reviewerId = null);

        /// <summary>
        /// Get survey creator/supervisor ID
        /// </summary>
        int? GetSurveyCreatorId(Int64 surveyId);
    }
}
