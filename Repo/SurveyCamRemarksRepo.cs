using AnalyticaDocs.Util;
using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using System.Data;
using System.Text;

namespace SurveyApp.Repo
{
    public class SurveyCamRemarksRepo : ISurveyCamRemarks
    {
        private static readonly string cs = 
            "Server=10.0.32.135;Database=VLDev;UID=adminrole;Password=@dminr0le;Connect Timeout=360000;TrustServerCertificate=True";

        public bool SaveCameraRemarks(SurveyCamRemarksModel model)
        {
            try
            {
                using var con = new SqlConnection(cs);
                using var cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;

                if (model.TransID > 0)
                {
                    // Update existing remark
                    cmd.CommandText = @"
                        UPDATE SurveyCamRemarks 
                        SET Remarks = @Remarks, 
                            CreatedBy = @CreatedBy, 
                            CreatedOn = @CreatedOn
                        WHERE TransID = @TransID";
                    cmd.Parameters.AddWithValue("@TransID", model.TransID);
                }
                else
                {
                    // Insert new remark
                    cmd.CommandText = @"
                        INSERT INTO SurveyCamRemarks (SurveyID, LocID, ItemID, RemarkNo, Remarks, CreatedBy, CreatedOn)
                        VALUES (@SurveyID, @LocID, @ItemID, @RemarkNo, @Remarks, @CreatedBy, @CreatedOn)";
                    cmd.Parameters.AddWithValue("@SurveyID", model.SurveyID);
                    cmd.Parameters.AddWithValue("@LocID", model.LocID);
                    cmd.Parameters.AddWithValue("@ItemID", model.ItemID);
                    cmd.Parameters.AddWithValue("@RemarkNo", model.RemarkNo);
                }

                cmd.Parameters.AddWithValue("@Remarks", model.Remarks);
                cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving camera remarks: {ex.Message}");
                throw;
            }
        }

        public List<SurveyCamRemarksModel> GetCameraRemarks(Int64 surveyId, int locId, int itemId)
        {
            try
            {
                using var con = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
                    SELECT TransID, SurveyID, LocID, ItemID, RemarkNo, Remarks, CreatedBy, CreatedOn
                    FROM SurveyCamRemarks
                    WHERE SurveyID = @SurveyID AND LocID = @LocID AND ItemID = @ItemID
                    ORDER BY RemarkNo", con);

                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocID", locId);
                cmd.Parameters.AddWithValue("@ItemID", itemId);

                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                var remarks = SqlDbHelper.DataTableToList<SurveyCamRemarksModel>(dt);
                
                // Assign camera numbers
                for (int i = 0; i < remarks.Count; i++)
                {
                    remarks[i].CameraNumber = i + 1;
                }

                return remarks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting camera remarks: {ex.Message}");
                return new List<SurveyCamRemarksModel>();
            }
        }

        public SurveyCamRemarksModel? GetCameraRemarkById(int transId)
        {
            try
            {
                using var con = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
                    SELECT TransID, SurveyID, LocID, ItemID, RemarkNo, Remarks, CreatedBy, CreatedOn
                    FROM SurveyCamRemarks
                    WHERE TransID = @TransID", con);

                cmd.Parameters.AddWithValue("@TransID", transId);

                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                var remarks = SqlDbHelper.DataTableToList<SurveyCamRemarksModel>(dt);
                return remarks.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting camera remark by ID: {ex.Message}");
                return null;
            }
        }

        public bool DeleteCameraRemark(int transId)
        {
            try
            {
                using var con = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
                    DELETE FROM SurveyCamRemarks WHERE TransID = @TransID", con);

                cmd.Parameters.AddWithValue("@TransID", transId);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting camera remark: {ex.Message}");
                return false;
            }
        }

        public bool DeleteAllCameraRemarks(Int64 surveyId, int locId, int itemId)
        {
            try
            {
                using var con = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
                    DELETE FROM SurveyCamRemarks 
                    WHERE SurveyID = @SurveyID AND LocID = @LocID AND ItemID = @ItemID", con);

                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocID", locId);
                cmd.Parameters.AddWithValue("@ItemID", itemId);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all camera remarks: {ex.Message}");
                return false;
            }
        }

        public string GetFormattedCameraRemarks(Int64 surveyId, int locId, int itemId)
        {
            var remarks = GetCameraRemarks(surveyId, locId, itemId);
            
            if (remarks == null || remarks.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < remarks.Count; i++)
            {
                sb.AppendLine($"#Cam{i + 1} Remarks: {remarks[i].Remarks}");
            }

            return sb.ToString().TrimEnd();
        }

        public List<SurveyCamRemarksModel> GetCameraRemarksByLocation(Int64 surveyId, int locId)
        {
            try
            {
                using var con = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
                    SELECT TransID, SurveyID, LocID, ItemID, RemarkNo, Remarks, CreatedBy, CreatedOn
                    FROM SurveyCamRemarks
                    WHERE SurveyID = @SurveyID AND LocID = @LocID
                    ORDER BY ItemID, RemarkNo", con);

                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocID", locId);

                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                var remarks = SqlDbHelper.DataTableToList<SurveyCamRemarksModel>(dt);
                
                // Assign camera numbers
                for (int i = 0; i < remarks.Count; i++)
                {
                    remarks[i].CameraNumber = i + 1;
                }

                return remarks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting camera remarks by location: {ex.Message}");
                return new List<SurveyCamRemarksModel>();
            }
        }

        public List<SurveyCamRemarksModel> GetAllCameraRemarksBySurvey(Int64 surveyId)
        {
            try
            {
                using var con = new SqlConnection(cs);
                using var cmd = new SqlCommand(@"
                    SELECT TransID, SurveyID, LocID, ItemID, RemarkNo, Remarks, CreatedBy, CreatedOn
                    FROM SurveyCamRemarks
                    WHERE SurveyID = @SurveyID
                    ORDER BY LocID, ItemID, RemarkNo", con);

                cmd.Parameters.AddWithValue("@SurveyID", surveyId);

                con.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                var remarks = SqlDbHelper.DataTableToList<SurveyCamRemarksModel>(dt);
                
                // Assign camera numbers per location
                var grouped = remarks.GroupBy(r => r.LocID).ToList();
                foreach (var group in grouped)
                {
                    int camNum = 1;
                    foreach (var remark in group)
                    {
                        remark.CameraNumber = camNum++;
                    }
                }

                return remarks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all camera remarks by survey: {ex.Message}");
                return new List<SurveyCamRemarksModel>();
            }
        }
    }
}
