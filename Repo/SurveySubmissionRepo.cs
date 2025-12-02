using AnalyticaDocs.Util;
using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using System.Data;

namespace SurveyApp.Repo
{
    public class SurveySubmissionRepo : ISurveySubmission
    {
        public bool SubmitSurvey(Int64 surveyId, int submittedBy, string submissionStatus)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 1);
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                cmd.Parameters.AddWithValue("@SubmissionStatus", submissionStatus);
                cmd.Parameters.AddWithValue("@SubmittedBy", submittedBy);
                cmd.Parameters.AddWithValue("@SubmissionDate", DateTime.Now);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        public bool UpdateReviewStatus(Int64 submissionId, string status, int reviewedBy, string? reviewComments)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2);
                cmd.Parameters.AddWithValue("@SubmissionId", submissionId);
                cmd.Parameters.AddWithValue("@SubmissionStatus", status);
                cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
                cmd.Parameters.AddWithValue("@ReviewDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ReviewComments", string.IsNullOrEmpty(reviewComments) ? DBNull.Value : reviewComments);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        public bool DeleteSubmission(Int64 submissionId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 3);
                cmd.Parameters.AddWithValue("@SubmissionId", submissionId);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        public List<SurveySubmissionModel> GetAllSubmissions(int? submittedBy = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 4);
                cmd.Parameters.AddWithValue("@SubmittedBy", submittedBy.HasValue ? submittedBy.Value : DBNull.Value);

                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                return SqlDbHelper.DataTableToList<SurveySubmissionModel>(dt);
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        public SurveySubmissionModel? GetSubmissionBySurveyId(Int64 surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5);
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                var submissions = SqlDbHelper.DataTableToList<SurveySubmissionModel>(dt);
                return submissions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        public bool CanEditSurvey(Int64 surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 6);
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        public List<SurveySubmissionModel> GetPendingSubmissionsForReview(int? reviewerId = null)
        {
            try
            {
                var allSubmissions = GetAllSubmissions();
                
                // Filter for submitted status (pending review)
                var pendingSubmissions = allSubmissions
                    .Where(s => s.SubmissionStatus == "Submitted")
                    .OrderByDescending(s => s.SubmissionDate)
                    .ToList();

                // If reviewerId provided, filter by surveys created by that reviewer
                if (reviewerId.HasValue)
                {
                    var filteredSubmissions = new List<SurveySubmissionModel>();
                    foreach (var submission in pendingSubmissions)
                    {
                        var creatorId = GetSurveyCreatorId(submission.SurveyId);
                        if (creatorId.HasValue && creatorId.Value == reviewerId.Value)
                        {
                            filteredSubmissions.Add(submission);
                        }
                    }
                    return filteredSubmissions;
                }

                return pendingSubmissions;
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        public int? GetSurveyCreatorId(Int64 surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SELECT CreatedBy FROM Survey WHERE SurveyId = @SurveyId", con);
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                var result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : null;
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }
    }
}
