using AnalyticaDocs.Util;
using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using System.Data;

namespace SurveyApp.Repo
{
    public class SurveyLocationStatusRepo : ISurveyLocationStatus
    {
        public SurveyLocationStatusModel? GetLocationStatus(long surveyId, int locId)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("SpSurveyDetails", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SpType", 8); // Select
            cmd.Parameters.AddWithValue("@SurveyID", surveyId);
            cmd.Parameters.AddWithValue("@LocID", locId);
            
            con.Open();
            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            var list = SqlDbHelper.DataTableToList<SurveyLocationStatusModel>(dt);
            return list.FirstOrDefault();
        }

        public List<SurveyLocationStatusModel> GetSurveyLocationStatuses(long surveyId)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("SpSurveyDetails", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SpType", 8); // Select
            cmd.Parameters.AddWithValue("@SurveyID", surveyId);
            
            con.Open();
            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return SqlDbHelper.DataTableToList<SurveyLocationStatusModel>(dt);
        }

        public bool UpsertLocationStatus(long surveyId, int locId, string status, string? remarks, int userId)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("SpSurveyDetails", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SpType", 5); // Insert/Update
            cmd.Parameters.AddWithValue("@SurveyID", surveyId);
            cmd.Parameters.AddWithValue("@LocID", locId);
            cmd.Parameters.AddWithValue("@Status", status ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserID", userId);
            
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["Result"] != DBNull.Value && Convert.ToInt32(reader["Result"]) == 1;
            }
            return false;
        }

        public bool MarkLocationAsCompleted(long surveyId, int locId, int userId, string? remarks = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 7); // Mark Complete
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocID", locId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Remarks", remarks ?? (object)DBNull.Value);
                
                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader["Result"] != DBNull.Value && Convert.ToInt32(reader["Result"]) == 1;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MarkLocationAsCompleted Error: {ex.Message}");
                throw;
            }
        }

        public bool MarkLocationAsInProgress(long surveyId, int locId, int userId, string? remarks = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 9); // Mark In Progress
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocID", locId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Remarks", remarks ?? (object)DBNull.Value);
                
                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader["Result"] != DBNull.Value && Convert.ToInt32(reader["Result"]) == 1;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MarkLocationAsInProgress Error: {ex.Message}");
                throw;
            }
        }

        public bool MarkLocationAsVerified(long surveyId, int locId, int userId, string? remarks = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpSurveyDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 10); // Mark Verified
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocID", locId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Remarks", remarks ?? (object)DBNull.Value);
                
                con.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader["Result"] != DBNull.Value && Convert.ToInt32(reader["Result"]) == 1;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MarkLocationAsVerified Error: {ex.Message}");
                throw;
            }
        }

        public bool DeleteLocationStatus(long surveyId, int locId)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("SpSurveyDetails", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SpType", 6); // Delete
            cmd.Parameters.AddWithValue("@SurveyID", surveyId);
            cmd.Parameters.AddWithValue("@LocID", locId);
            
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["Result"] != DBNull.Value && Convert.ToInt32(reader["Result"]) == 1;
            }
            return false;
        }
    }
}
