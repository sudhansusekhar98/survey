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
                con.Open();
                
                using var transaction = con.BeginTransaction();
                
                try
                {
                    // Update submission status
                    using var cmd = new SqlCommand("dbo.SpSurveySubmission", con, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SpType", 2);
                    cmd.Parameters.AddWithValue("@SubmissionId", submissionId);
                    cmd.Parameters.AddWithValue("@SubmissionStatus", status);
                    cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
                    cmd.Parameters.AddWithValue("@ReviewDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ReviewComments", string.IsNullOrEmpty(reviewComments) ? DBNull.Value : reviewComments);
                    
                    int result = cmd.ExecuteNonQuery();
                    
                    if (result > 0)
                    {
                        // Get SurveyId from submission
                        using var getSurveyCmd = new SqlCommand("SELECT SurveyId FROM SurveySubmission WHERE SubmissionId = @SubmissionId", con, transaction);
                        getSurveyCmd.Parameters.AddWithValue("@SubmissionId", submissionId);
                        var surveyId = getSurveyCmd.ExecuteScalar();
                        
                        if (surveyId != null)
                        {
                            // Update Survey table status based on approval/rejection
                            string surveyStatus = status == "Approved" ? "Completed" : "In Progress";
                            using var updateSurveyCmd = new SqlCommand("UPDATE Survey SET SurveyStatus = @SurveyStatus WHERE SurveyId = @SurveyId", con, transaction);
                            updateSurveyCmd.Parameters.AddWithValue("@SurveyStatus", surveyStatus);
                            updateSurveyCmd.Parameters.AddWithValue("@SurveyId", surveyId);
                            updateSurveyCmd.ExecuteNonQuery();
                        }
                    }
                    
                    transaction.Commit();
                    return result > 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
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

        /// <summary>
        /// Get pending submissions visible to user based on role and assignments
        /// </summary>
        public List<SurveySubmissionModel> GetPendingSubmissionsForUser(int userId, int roleId, int? empId)
        {
            try
            {
                var allSubmissions = GetAllSubmissions();
                
                // Filter for submitted status (pending review)
                var pendingSubmissions = allSubmissions
                    .Where(s => s.SubmissionStatus == "Submitted")
                    .OrderByDescending(s => s.SubmissionDate)
                    .ToList();

                // Super User (RoleId 101) sees all pending submissions
                if (roleId == 101)
                {
                    return pendingSubmissions;
                }

                // For other users, filter by:
                // 1. Surveys created by this user (Team Leader)
                // 2. Surveys assigned to this user's employee (Team Member)
                var filteredSubmissions = new List<SurveySubmissionModel>();
                
                foreach (var submission in pendingSubmissions)
                {
                    // Check if user is the survey creator
                    var creatorId = GetSurveyCreatorId(submission.SurveyId);
                    if (creatorId.HasValue && creatorId.Value == userId)
                    {
                        filteredSubmissions.Add(submission);
                        continue;
                    }

                    // Check if user's employee is assigned to this survey
                    if (empId.HasValue && IsUserAssignedToSurvey(submission.SurveyId, empId.Value))
                    {
                        filteredSubmissions.Add(submission);
                    }
                }
                
                return filteredSubmissions;
            }
            catch (Exception ex)
            {
                // Log error: ex.ToString()
                throw;
            }
        }

        /// <summary>
        /// Check if user can approve/reject a submission (must be creator or super user)
        /// </summary>
        public bool CanUserReviewSubmission(Int64 surveyId, int userId, int roleId)
        {
            // Super User can review any submission
            if (roleId == 101)
            {
                return true;
            }

            // Check if user is the survey creator (Team Leader)
            var creatorId = GetSurveyCreatorId(surveyId);
            return creatorId.HasValue && creatorId.Value == userId;
        }

        /// <summary>
        /// Check if an employee is assigned to a survey
        /// </summary>
        private bool IsUserAssignedToSurvey(Int64 surveyId, int empId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM SurveyAssignment WHERE SurveyID = @SurveyId AND EmpID = @EmpId", con);
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                cmd.Parameters.AddWithValue("@EmpId", empId);

                con.Open();
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
