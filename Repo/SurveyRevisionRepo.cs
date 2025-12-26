using AnalyticaDocs.Util;
using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using System.Data;
using System.Text.Json;

namespace SurveyApp.Repo
{
    /// <summary>
    /// Repository for Survey Revision operations
    /// </summary>
    public class SurveyRevisionRepo : ISurveyRevision
    {
        private readonly ILogger<SurveyRevisionRepo> _logger;

        public SurveyRevisionRepo(ILogger<SurveyRevisionRepo> logger)
        {
            _logger = logger;
        }

        // Create a new revision of an approved survey
        public async Task<RevisionResultModel> CreateRevisionAsync(
            long surveyId,
            int assignedBy,
            string? reason,
            List<int> teamMembers,
            DateTime? dueDate)
        {
            try
            {
                // First check if survey can be revised
                var canReviseCheck = CanRevise(surveyId);
                if (!canReviseCheck.CanRevise)
                {
                    return new RevisionResultModel
                    {
                        Success = false,
                        Message = canReviseCheck.Reason ?? "This survey cannot be revised."
                    };
                }

                // Serialize team members to JSON
                string teamMembersJson = JsonSerializer.Serialize(teamMembers);

                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyRevision", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 1); // Create Revision
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                cmd.Parameters.AddWithValue("@RevisionReason", (object?)reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AssignedBy", assignedBy);
                cmd.Parameters.AddWithValue("@AssignedToTeam", teamMembersJson);
                cmd.Parameters.AddWithValue("@NewDueDate", (object?)dueDate ?? DBNull.Value);

                await con.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Use Convert.ToInt64 because SQL NUMERIC(11,0) returns Decimal, not Int64
                    long newSurveyId = Convert.ToInt64(reader["NewSurveyId"]);
                    int revisionNumber = Convert.ToInt32(reader["RevisionNumber"]);

                    // Team Leader assignment is now handled by the stored procedure
                    await reader.CloseAsync();

                    _logger.LogInformation(
                        "Created revision {RevisionNumber} for survey {OriginalSurveyId}. New survey ID: {NewSurveyId}",
                        revisionNumber, surveyId, newSurveyId);

                    return new RevisionResultModel
                    {
                        Success = true,
                        NewSurveyId = newSurveyId,
                        RevisionNumber = revisionNumber,
                        Message = $"Revision {revisionNumber} created successfully."
                    };
                }

                return new RevisionResultModel
                {
                    Success = false,
                    Message = "Failed to create revision. No data returned from procedure."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating revision for survey {SurveyId}", surveyId);
                return new RevisionResultModel
                {
                    Success = false,
                    Message = $"Error creating revision: {ex.Message}"
                };
            }
        }

        // Create survey assignments for team members on the revised survey
        private async Task CreateAssignmentsForRevision(SqlConnection con, long surveyId, List<int> teamMembers, DateTime? dueDate)
        {
            if (teamMembers == null || !teamMembers.Any()) return;

            foreach (var empId in teamMembers)
            {
                try
                {
                    using var cmd = new SqlCommand("SpSurvey", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SpType", 11); // Add Assignment
                    cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                    cmd.Parameters.AddWithValue("@EmpId", empId);
                    cmd.Parameters.AddWithValue("@DueDate", (object?)dueDate ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create assignment for EmpId {EmpId} on survey {SurveyId}", empId, surveyId);
                }
            }
        }

        // Get the revision history for a survey
        public List<SurveyRevisionModel> GetRevisionHistory(long surveyId)
        {
            var revisions = new List<SurveyRevisionModel>();

            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyRevision", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2); // Get Revision History
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    revisions.Add(MapToSurveyRevisionModel(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revision history for survey {SurveyId}", surveyId);
            }

            return revisions;
        }

        // Get all pending revisions
        public List<SurveyRevisionModel> GetAllPendingRevisions()
        {
            var revisions = new List<SurveyRevisionModel>();

            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyRevision", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 3); // Get All Pending

                con.Open();
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    revisions.Add(MapToSurveyRevisionModel(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending revisions");
            }

            return revisions;
        }

        // Update revision status
        public bool UpdateRevisionStatus(long revisionLogId, string status, string? notes)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyRevision", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 4); // Update Status
                cmd.Parameters.AddWithValue("@RevisionLogId", revisionLogId);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Notes", (object?)notes ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();

                _logger.LogInformation("Updated revision {RevisionLogId} status to {Status}", revisionLogId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating revision {RevisionLogId} status", revisionLogId);
                return false;
            }
        }

        // Check if a survey can be revised
        public CanReviseCheckModel CanRevise(long surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyRevision", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 6); // Check can revise
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    bool canRevise = reader.GetInt32(reader.GetOrdinal("CanRevise")) == 1;
                    string? submissionStatus = reader["SubmissionStatus"] as string;
                    
                    return new CanReviseCheckModel
                    {
                        CanRevise = canRevise,
                        SurveyName = reader["SurveyName"] as string,
                        SurveyStatus = reader["SurveyStatus"] as string,
                        SubmissionStatus = submissionStatus,
                        IsRevised = reader["IsRevised"] != DBNull.Value && Convert.ToBoolean(reader["IsRevised"]),
                        RevisionNumber = reader["RevisionNumber"] != DBNull.Value ? Convert.ToInt32(reader["RevisionNumber"]) : 0,
                        Reason = canRevise ? null : $"Survey cannot be revised. Current submission status: {submissionStatus ?? "Not Submitted"}. Only approved surveys can be revised."
                    };
                }

                return new CanReviseCheckModel
                {
                    CanRevise = false,
                    Reason = "Survey not found."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if survey {SurveyId} can be revised", surveyId);
                return new CanReviseCheckModel
                {
                    CanRevise = false,
                    Reason = $"Error checking revision eligibility: {ex.Message}"
                };
            }
        }

        // Get the original (root) survey of a revision chain
        public SurveyModel? GetOriginalSurvey(long surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyRevision", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5); // Get Survey Chain
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                // The last row is the root (original) survey
                if (dt.Rows.Count > 0)
                {
                    var rootRow = dt.Rows[dt.Rows.Count - 1];
                    return new SurveyModel
                    {
                        SurveyId = Convert.ToInt64(rootRow["SurveyId"]),
                        SurveyName = rootRow["SurveyName"]?.ToString() ?? "",
                        IsRevised = rootRow["IsRevised"] != DBNull.Value && Convert.ToBoolean(rootRow["IsRevised"]),
                        RevisionNumber = rootRow["RevisionNumber"] != DBNull.Value ? Convert.ToInt32(rootRow["RevisionNumber"]) : 0
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting original survey for {SurveyId}", surveyId);
                return null;
            }
        }

        // Map SqlDataReader to SurveyRevisionModel
        private SurveyRevisionModel MapToSurveyRevisionModel(SqlDataReader reader)
        {
            var model = new SurveyRevisionModel
            {
                // Use Convert because SQL NUMERIC returns Decimal, not Int64
                RevisionLogId = Convert.ToInt64(reader["RevisionLogId"]),
                OriginalSurveyId = Convert.ToInt64(reader["OriginalSurveyId"]),
                RevisedSurveyId = Convert.ToInt64(reader["RevisedSurveyId"]),
                RevisionNumber = Convert.ToInt32(reader["RevisionNumber"]),
                AssignedBy = Convert.ToInt32(reader["AssignedBy"]),
                AssignedDate = reader.GetDateTime(reader.GetOrdinal("AssignedDate")),
                Status = reader["Status"]?.ToString() ?? "Unknown"
            };

            // Optional fields
            if (HasColumn(reader, "OriginalSurveyName"))
                model.OriginalSurveyName = reader["OriginalSurveyName"] as string;
            
            if (HasColumn(reader, "RevisedSurveyName"))
                model.RevisedSurveyName = reader["RevisedSurveyName"] as string;
            
            if (HasColumn(reader, "RevisionReason"))
                model.RevisionReason = reader["RevisionReason"] as string;
            
            if (HasColumn(reader, "AssignedByName"))
                model.AssignedByName = reader["AssignedByName"] as string;
            
            if (HasColumn(reader, "AssignedToTeam"))
                model.AssignedToTeam = reader["AssignedToTeam"] as string;
            
            if (HasColumn(reader, "CompletedDate") && reader["CompletedDate"] != DBNull.Value)
                model.CompletedDate = reader.GetDateTime(reader.GetOrdinal("CompletedDate"));
            
            if (HasColumn(reader, "Notes"))
                model.Notes = reader["Notes"] as string;
            
            if (HasColumn(reader, "CurrentSurveyStatus"))
                model.CurrentSurveyStatus = reader["CurrentSurveyStatus"] as string;
            
            if (HasColumn(reader, "SubmissionStatus"))
                model.SubmissionStatus = reader["SubmissionStatus"] as string;
            
            if (HasColumn(reader, "ClientName"))
                model.ClientName = reader["ClientName"] as string;
            
            if (HasColumn(reader, "RegionName"))
                model.RegionName = reader["RegionName"] as string;
            
            if (HasColumn(reader, "DueDate") && reader["DueDate"] != DBNull.Value)
                model.DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate"));

            return model;
        }

        // Check if a column exists in the reader
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
