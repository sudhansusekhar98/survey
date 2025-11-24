using AnalyticaDocs.Util;
using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using System.Data;

namespace SurveyApp.Repo
{
    public class SurveyLocationRepo : ISurveyLocation
    {
        public List<SurveyLocationModel> GetAllLocations()
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("SELECT LocID, SurveyID, LocName, LocLat, LocLog, CreateOn, CreateBy, Isactive, LocationType FROM dbo.SurveyLocation", con);
            con.Open();
            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return SqlDbHelper.DataTableToList<SurveyLocationModel>(dt);
        }

        public SurveyLocationModel? GetLocationById(int locId)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("SELECT LocID, SurveyID, LocName, LocLat, LocLog, CreateOn, CreateBy, Isactive, LocationType FROM dbo.SurveyLocation WHERE LocID = @LocID", con);
            cmd.Parameters.AddWithValue("@LocID", locId);
            con.Open();
            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            var list = SqlDbHelper.DataTableToList<SurveyLocationModel>(dt);
            return list.FirstOrDefault();
        }

        public bool AddLocation(SurveyLocationModel location)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand(@"INSERT INTO dbo.SurveyLocation (SurveyID, LocName, LocLat, LocLog, CreateOn, CreateBy, Isactive, LocationType)
                VALUES (@SurveyID, @LocName, @LocLat, @LocLog, @CreateOn, @CreateBy, @Isactive, @LocationType)", con);
            cmd.Parameters.AddWithValue("@SurveyID", location.SurveyID);
            cmd.Parameters.AddWithValue("@LocName", location.LocName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LocLat", location.LocLat ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LocLog", location.LocLog ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CreateOn", location.CreateOn ?? DateTime.Now);
            cmd.Parameters.AddWithValue("@CreateBy", location.CreateBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Isactive", location.Isactive);
            cmd.Parameters.AddWithValue("@LocationType", location.LocationType ?? (object)DBNull.Value);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateLocation(SurveyLocationModel location)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand(@"UPDATE dbo.SurveyLocation SET SurveyID=@SurveyID, LocName=@LocName, LocLat=@LocLat, LocLog=@LocLog, Isactive=@Isactive, LocationType=@LocationType WHERE LocID=@LocID", con);
            cmd.Parameters.AddWithValue("@LocID", location.LocID);
            cmd.Parameters.AddWithValue("@SurveyID", location.SurveyID);
            cmd.Parameters.AddWithValue("@LocName", location.LocName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LocLat", location.LocLat ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LocLog", location.LocLog ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Isactive", location.Isactive);
            cmd.Parameters.AddWithValue("@LocationType", location.LocationType ?? (object)DBNull.Value);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteLocation(int locId)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            using var cmd = new SqlCommand("DELETE FROM dbo.SurveyLocation WHERE LocID=@LocID", con);
            cmd.Parameters.AddWithValue("@LocID", locId);
            con.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}