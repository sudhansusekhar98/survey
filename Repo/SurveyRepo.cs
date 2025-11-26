using AnalyticaDocs.Models;
using AnalyticaDocs.Util;
using Humanizer;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using SurveyApp.Models;
using System.Data;

namespace SurveyApp.Repo
       
{
        public class SurveyRepo : ISurvey
    {
        public bool AddSurvey(SurveyModel survey)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // SpType = 1 -> Insert Survey 
                cmd.Parameters.AddWithValue("@SpType", 1);

                // The SP generates SurveyId internally. Passing SurveyId is optional
                // but we'll pass null to keep it explicit.
                cmd.Parameters.AddWithValue("@SurveyID", survey.SurveyId == 0 ? (object)DBNull.Value : survey.SurveyId);

                cmd.Parameters.AddWithValue("@SurveyName", survey.SurveyName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ImplementationType", survey.ImplementationType ?? (object)DBNull.Value);

                // Your SP expects SurveyDate as varchar(100) � keep same format or adjust SP to accept DATE.
                // If your model uses DateTime? convert to string (yyyy-MM-dd) or pass as DBNull if null.
                cmd.Parameters.AddWithValue("@SurveyDate", survey.SurveyDate ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue("@SurveyTeamName", survey.SurveyTeamName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SurveyTeamContact", survey.SurveyTeamContact ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AgencyName", survey.AgencyName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocationSiteName", survey.LocationSiteName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CityDistrict", survey.CityDistrict ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ScopeOfWork", survey.ScopeOfWork ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", survey.Latitude ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Longitude", survey.Longitude ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MapMarking", survey.MapMarking ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SurveyStatus", survey.SurveyStatus ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RegionID", survey.RegionID);

                if (survey.CreatedBy == 0)
                    cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@CreatedBy", survey.CreatedBy);

                con.Open();

                int rowsAffected = cmd.ExecuteNonQuery();

                // NOTE: If you later modify the stored procedure to output the generated SurveyId
                // you can add an output parameter and read it here:
                // var newId = cmd.Parameters["@NewSurveyId"].Value;

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // TODO: log ex.ToString()
                throw;
            }
        }

        public bool UpdateSurvey(SurveyModel survey)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 8);
                cmd.Parameters.AddWithValue("@SurveyId", survey.SurveyId);
                cmd.Parameters.AddWithValue("@SurveyName", survey.SurveyName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ImplementationType", survey.ImplementationType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SurveyDate", survey.SurveyDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SurveyTeamName", survey.SurveyTeamName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SurveyTeamContact", survey.SurveyTeamContact ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AgencyName", survey.AgencyName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocationSiteName", survey.LocationSiteName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CityDistrict", survey.CityDistrict ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ScopeOfWork", survey.ScopeOfWork ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Latitude", survey.Latitude ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Longitude", survey.Longitude ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MapMarking", survey.MapMarking ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SurveyStatus", survey.SurveyStatus ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RegionID", survey.RegionID);
                cmd.Parameters.AddWithValue("@CreatedBy", survey.CreatedBy);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public List<SurveyModel> GetAllSurveys(int UserId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand(@"
                    SELECT s.*, r.RegionName,
                           (SELECT MIN(DueDate) FROM SurveyAssignment WHERE SurveyID = s.SurveyId) AS DueDate
                    FROM Survey s
                    LEFT JOIN RegionMaster r ON s.RegionID = r.RegionID
                    WHERE (@CreatedBy IS NULL OR s.CreatedBy = @CreatedBy)
                    ORDER BY s.SurveyId DESC", con);
                
                if (UserId == 0)
                    cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@CreatedBy", UserId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<SurveyModel> records = SqlDbHelper.DataTableToList<SurveyModel>(dt);
                return records;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public SurveyModel? GetSurveyById(Int64 surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 7);
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<SurveyModel> surveys = SqlDbHelper.DataTableToList<SurveyModel>(dt);
                return surveys.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public bool DeleteSurvey(Int64 surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand(@"
                    DELETE FROM dbo.Survey 
                    WHERE SurveyId = @SurveyId", con);

                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                // log ex.ToString()
                throw;
            }
        }

        public List<SurveyLocationModel> GetSurveyLocationById(long surveyId)
        {
            var locations = new List<SurveyLocationModel>();
            using (var conn = new SqlConnection(DBConnection.ConnectionString))
            using (var cmd = new SqlCommand("dbo.SpSurvey", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 9); // 9 = Get locations for survey
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    locations = SqlDbHelper.DataTableToList<SurveyLocationModel>(dt);
                }
            }
            return locations;
        }

        public SurveyLocationModel? GetSurveyLocationByLocId(int locId)
        {
            try
            {
                using var conn = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 10);
                cmd.Parameters.AddWithValue("@LocID", locId);
                
                conn.Open();
                using var reader = cmd.ExecuteReader();
                var dt = new DataTable();
                dt.Load(reader);
                var locations = SqlDbHelper.DataTableToList<SurveyLocationModel>(dt);
                return locations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool AddSurveyLocation(SurveyLocationModel location)
        {
            try
            {
                using var conn = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5);
                cmd.Parameters.AddWithValue("@SurveyID", location.SurveyID);
                cmd.Parameters.AddWithValue("@LocName", location.LocName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocLat", location.LocLat ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocLog", location.LocLog ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", location.CreateBy ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocationType", location.LocationType ?? (object)DBNull.Value);
                
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //public bool UpdateSurveyLocation(SurveyLocationModel location)
        //{
        //    try
        //    {
        //        using var conn = new SqlConnection(DBConnection.ConnectionString);
        //        using var cmd = new SqlCommand("dbo.SpSurvey", conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@SpType", 12);
        //        cmd.Parameters.AddWithValue("@LocID", location.LocID);
        //        cmd.Parameters.AddWithValue("@SurveyID", location.SurveyID);
        //        cmd.Parameters.AddWithValue("@LocName", location.LocName ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@LocLat", location.LocLat ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@LocLog", location.LocLog ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@Isactive", location.Isactive ? 'Y' : 'N');

        //        conn.Open();
        //        int result = cmd.ExecuteNonQuery();
        //        if (result >= 0)
        //            return true;
        //        else
        //            return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}

        //public bool DeleteSurveyLocation(int locId)
        //{
        //    try
        //    {
        //        using var conn = new SqlConnection(DBConnection.ConnectionString);
        //        using var cmd = new SqlCommand("dbo.SpSurvey", conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@SpType", 11);
        //        cmd.Parameters.AddWithValue("@LocID", locId);

        //        conn.Open();
        //        int rowsAffected = cmd.ExecuteNonQuery();
        //        return rowsAffected > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //}


        public bool UpdateSurveyLocation(SurveyLocationModel location)
        {
            try
            {
                using var conn = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SpType", 12);
                cmd.Parameters.AddWithValue("@LocID", location.LocID);
                cmd.Parameters.AddWithValue("@SurveyID", location.SurveyID);
                cmd.Parameters.AddWithValue("@LocName", location.LocName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocLat", location.LocLat ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocLog", location.LocLog ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Isactive", location.Isactive ? "Y" : "N");
                cmd.Parameters.AddWithValue("@LocationType", location.LocationType ?? (object)DBNull.Value);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0; // Check if at least 1 row was affected
            }
            catch
            {
                throw;
            }
        }

        public bool DeleteSurveyLocation(int locId)
        {
            try
            {
                using var conn = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SpType", 11);
                cmd.Parameters.AddWithValue("@LocID", locId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0; // Check if at least 1 row was affected
            }
            catch
            {
                throw;
            }
        }

        public bool CreateLocationsBySurveyId(Int64 surveyId, List<SurveyLocationModel> locations, int createdBy)
        {
            using var conn = new SqlConnection(DBConnection.ConnectionString);
            conn.Open();
            foreach (var location in locations)
            {
                using var cmd = new SqlCommand("dbo.SpSurvey", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5);
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocName", location.LocName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocLat", location.LocLat ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LocLog", location.LocLog ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                cmd.ExecuteNonQuery();
            }
            return true;
        }

        public List<ItemTypeMasterModel> GetItemTypeMaster(int locId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 13);
                cmd.Parameters.AddWithValue("@LocID", locId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<ItemTypeMasterModel> itemTypes = SqlDbHelper.DataTableToList<ItemTypeMasterModel>(dt);
                return itemTypes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<ItemTypeMasterModel> GetSelectedItemTypesForLocation(int locId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 15); // New SpType for getting selected item types
                cmd.Parameters.AddWithValue("@LocID", locId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<ItemTypeMasterModel> itemTypes = SqlDbHelper.DataTableToList<ItemTypeMasterModel>(dt);
                return itemTypes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

       
        public bool SaveItemTypesForLocation(Int64 surveyId, string surveyName, int locId, List<int> itemTypeIds, int createdBy)
        {
            using var conn = new SqlConnection(DBConnection.ConnectionString);
            conn.Open();

            using var tran = conn.BeginTransaction();
            try
            {
                //// Reset any existing assignments for this survey/location to IsAssigned = 0
                //// (so final state exactly matches the provided itemTypeIds).
                //using (var resetCmd = new SqlCommand(
                //    "UPDATE AssignedItems SET IsAssigned = 0, ModifiedOn = SYSDATETIME() WHERE SurveyId = @SurveyID AND LocID = @LocID",
                //    conn, tran))
                //{
                //    resetCmd.Parameters.AddWithValue("@SurveyID", surveyId);
                //    resetCmd.Parameters.AddWithValue("@LocID", locId);
                //    resetCmd.Parameters.AddWithValue("@ModifiedBy", createdBy);
                //    resetCmd.ExecuteNonQuery();
                //}

                //// If no item types selected, we're done (all were reset).
                //if (itemTypeIds == null || itemTypeIds.Count == 0)
                //{
                //    tran.Commit();
                //    return true;
                //}

                // For each selected item type, call the SP with SpType = 10 which:
                // - updates IsAssigned if record exists
                // - inserts a new record otherwise
                foreach (var itemTypeId in itemTypeIds)
                {
                    using var cmd = new SqlCommand("dbo.SpSurvey", conn, tran);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SpType", 10);
                    cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                    cmd.Parameters.AddWithValue("@LocID", locId);
                    cmd.Parameters.AddWithValue("@ItemTypeID", itemTypeId);
                    cmd.Parameters.AddWithValue("@IsAssigned", 1);      // mark selected
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch
            {
                try { tran.Rollback(); } catch { /* log if needed */ }
                throw; // bubble up so caller can handle/log
            }
        }

        public List<AssignedItemsListModel> GetItemTypebySurveyLoc(int LocId, Int64 SurveyID)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 15);
                cmd.Parameters.AddWithValue("@SurveyID", SurveyID);
                cmd.Parameters.AddWithValue("@LocID", LocId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<AssignedItemsListModel> itemTypes = SqlDbHelper.DataTableToList<AssignedItemsListModel>(dt);
                return itemTypes;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool UpdateAssignedItems(AssignedItemsModel model)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            con.Open();
            using var transaction = con.BeginTransaction();
            try
            {
                foreach (var right in model.AssignItemList)
                {
                    using var cmd = new SqlCommand("dbo.SpSurvey", con, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SpType", 10);
                    cmd.Parameters.AddWithValue("@SurveyID", model.SurveyId);
                    cmd.Parameters.AddWithValue("@LocID", model.LocID);
                    cmd.Parameters.AddWithValue("@ItemTypeID", right.ItemTypeID);
                    cmd.Parameters.AddWithValue("@IsAssigned", right.IsAssigned ? 1 : 0);
                    cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value);

                    int result = cmd.ExecuteNonQuery();
                    if (result <= 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }

        public List<SurveyAssignmentModel> GetSurveyAssignments(Int64 SurveyID)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 16);
                cmd.Parameters.AddWithValue("@SurveyID", SurveyID);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<SurveyAssignmentModel> SurveyAssignmentList = SqlDbHelper.DataTableToList<SurveyAssignmentModel>(dt);
                return SurveyAssignmentList;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public  bool AddSurveyAssignment(SurveyAssignmentModel assignment)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 17);
                cmd.Parameters.AddWithValue("@SurveyID", assignment.SurveyID);
                cmd.Parameters.AddWithValue("@EmpID", assignment.EmpID);
                cmd.Parameters.AddWithValue("@DueDate", assignment.DueDate);
                cmd.Parameters.AddWithValue("@CreatedBy", assignment.CreateBy ?? (object)DBNull.Value);
                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool UpdateSurveyAssignment(SurveyAssignmentModel assignment)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 18); // Update assignment
                cmd.Parameters.AddWithValue("@TransID", assignment.TransID);
                cmd.Parameters.AddWithValue("@SurveyID", assignment.SurveyID);
                cmd.Parameters.AddWithValue("@EmpID", assignment.EmpID);
                cmd.Parameters.AddWithValue("@DueDate", assignment.DueDate);
                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool DeleteSurveyAssignment(int transId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 19); // Delete assignment
                cmd.Parameters.AddWithValue("@TransID", transId);
                con.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<SurveyAssignmentModel> GetAllSurveyAssignments(int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurvey", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 16);
                // If userId is 0, pass DBNull to get all assignments
                if (userId == 0)
                    cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@CreatedBy", userId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<SurveyAssignmentModel> assignments = SqlDbHelper.DataTableToList<SurveyAssignmentModel>(dt);
                return assignments;
            }
            catch (Exception)
            {
                // Return empty list if there's an error (e.g., no assignments table)
                return new List<SurveyAssignmentModel>();
            }
        }

        //public bool AssignSurvey(SurveyAssignmentModel model)
        //{
        //    try
        //    {
        //        using var con = new SqlConnection(DBConnection.ConnectionString);
        //        using var cmd = new SqlCommand("dbo.SpSurvey", con);
        //        cmd.CommandType = CommandType.StoredProcedure;

        //        // SpType = 1 -> Insert Survey 
        //        cmd.Parameters.AddWithValue("@SpType", 17);


        //        cmd.Parameters.AddWithValue("@SurveyID", survey.SurveyId == 0 ? (object)DBNull.Value : survey.SurveyId);



        //        if (survey.CreatedBy == 0)
        //            cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
        //        else
        //            cmd.Parameters.AddWithValue("@CreatedBy", survey.CreatedBy);

        //        con.Open();

        //        int rowsAffected = cmd.ExecuteNonQuery();

        //        // NOTE: If you later modify the stored procedure to output the generated SurveyId
        //        // you can add an output parameter and read it here:
        //        // var newId = cmd.Parameters["@NewSurveyId"].Value;

        //        return rowsAffected > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        // TODO: log ex.ToString()
        //        throw;
        //    }
        //}
        //------------------- Survey Details -------------------//

        public List<SurveyDetailsLocationModel> GetAssignedTypeList(long SurveyID, int LocId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveyDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2);
                cmd.Parameters.AddWithValue("@SurveyID", SurveyID);
                cmd.Parameters.AddWithValue("@LocID", LocId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<SurveyDetailsLocationModel> result = SqlDbHelper.DataTableToList<SurveyDetailsLocationModel>(dt);
                return result;
            }
            catch
            {
                throw;
            }
        }

        public List<SurveyDetailsModel> GetAssignedItemList(long SurveyID, int LocId, int ItemTypeID)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveyDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 3);
                cmd.Parameters.AddWithValue("@SurveyID", SurveyID);
                cmd.Parameters.AddWithValue("@LocID", LocId);
                cmd.Parameters.AddWithValue("@ItemTypeID", ItemTypeID);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<SurveyDetailsModel> result = SqlDbHelper.DataTableToList<SurveyDetailsModel>(dt);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<SurveyDetailsUpdatelist> GetSurveyUpdateItemList(long SurveyID, int LocId, int ItemTypeID)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveyDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 4);
                cmd.Parameters.AddWithValue("@SurveyID", SurveyID);
                cmd.Parameters.AddWithValue("@LocID", LocId);
                cmd.Parameters.AddWithValue("@ItemTypeID", ItemTypeID);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                List<SurveyDetailsUpdatelist> result = SqlDbHelper.DataTableToList<SurveyDetailsUpdatelist>(dt);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool UpdateSurveyDetails(SurveyDetailsUpdate model)
        {
            using var con = new SqlConnection(DBConnection.ConnectionString);
            con.Open();

            using var transaction = con.BeginTransaction();
            try
            {
                // Validate that all items have quantities selected (at least one qty must be > 0)
                foreach (var items in model.ItemLists)
                {
                    if (items.ItemQtyExist == 0 && items.ItemQtyReq == 0)
                    {
                        transaction.Rollback();
                        throw new InvalidOperationException($"Please enter quantity for item: {items.ItemName}. Both Existing and Required quantities cannot be zero.");
                    }
                }

                foreach (var items in model.ItemLists)
                {
                    using var cmd = new SqlCommand("dbo.SpSurveyDetails", con, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Convert Cloudinary lists to comma-separated strings
                    string imgPath = items.ImgPath ?? "";
                    string imgID = items.ImgID ?? "";
                    
                    if (items.CloudinaryUrls != null && items.CloudinaryUrls.Count > 0)
                    {
                        imgPath = string.Join(",", items.CloudinaryUrls);
                    }
                    
                    if (items.CloudinaryPublicIds != null && items.CloudinaryPublicIds.Count > 0)
                    {
                        imgID = string.Join(",", items.CloudinaryPublicIds);
                    }

                    cmd.Parameters.AddWithValue("@SpType", 1);
                    cmd.Parameters.AddWithValue("@SurveyID", model.SurveyID);
                    cmd.Parameters.AddWithValue("@LocID", model.LocID);
                    cmd.Parameters.AddWithValue("@ItemTypeID", model.ItemTypeID);
                    cmd.Parameters.AddWithValue("@ItemID", items.ItemID);
                    cmd.Parameters.AddWithValue("@ItemQtyExist", items.ItemQtyExist);
                    cmd.Parameters.AddWithValue("@ItemQtyReq", items.ItemQtyReq);
                    cmd.Parameters.AddWithValue("@ImgPath", imgPath);
                    cmd.Parameters.AddWithValue("@ImgID", imgID);
                    cmd.Parameters.AddWithValue("@Remarks", items.Remarks);
                    cmd.Parameters.AddWithValue("@CreateBy", model.CreateBy);

                    int result = cmd.ExecuteNonQuery();
                    if (result <= 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool MarkLocationAsCompleted(long surveyId, int locId, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveyDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;

                // Use SpType to mark location as completed
                // You may need to add this SpType to your stored procedure
                cmd.Parameters.AddWithValue("@SpType", 3); // 3 for marking location complete
                cmd.Parameters.AddWithValue("@SurveyID", surveyId);
                cmd.Parameters.AddWithValue("@LocID", locId);
                cmd.Parameters.AddWithValue("@CreateBy", userId);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception)
            {
                // Log exception
                throw;
            }
        }

        // Survey Submission Methods
        public bool SubmitSurvey(long surveyId, int userId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SpType", 1); // Insert/Update
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                cmd.Parameters.AddWithValue("@SubmissionStatus", "Submitted");
                cmd.Parameters.AddWithValue("@SubmittedBy", userId);
                cmd.Parameters.AddWithValue("@SubmissionDate", DateTime.Now);

                con.Open();
                cmd.ExecuteNonQuery();
                
                // Update survey status to Completed
                using var updateCmd = new SqlCommand("UPDATE Survey SET SurveyStatus = 'Completed' WHERE SurveyId = @SurveyId", con);
                updateCmd.Parameters.AddWithValue("@SurveyId", surveyId);
                updateCmd.ExecuteNonQuery();
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool WithdrawSubmission(long surveyId)
        {
            try
            {
                // Get current survey status BEFORE opening transaction
                var survey = GetSurveyById(surveyId);
                
                if (survey == null)
                {
                    Console.WriteLine($"WithdrawSubmission: Survey {surveyId} not found");
                    return false;
                }

                using var con = new SqlConnection(DBConnection.ConnectionString);
                con.Open();
                using var transaction = con.BeginTransaction();

                try
                {
                    // Update submission status to Draft
                    using var cmd = new SqlCommand("dbo.SpSurveySubmission", con, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SpType", 1); // Insert/Update
                    cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                    cmd.Parameters.AddWithValue("@SubmissionStatus", "Draft");
                    cmd.Parameters.AddWithValue("@SubmittedBy", DBNull.Value);
                    cmd.Parameters.AddWithValue("@SubmissionDate", DBNull.Value);

                    int submissionResult = cmd.ExecuteNonQuery();
                    Console.WriteLine($"WithdrawSubmission: Submission update affected {submissionResult} rows");

                    // Always change survey status to In Progress when unlocking
                    using var cmdUpdate = new SqlCommand("dbo.SpSurvey", con, transaction);
                    cmdUpdate.CommandType = CommandType.StoredProcedure;
                    cmdUpdate.Parameters.AddWithValue("@SpType", 8); // Update Survey (same as UpdateSurvey)
                    cmdUpdate.Parameters.AddWithValue("@SurveyId", surveyId);
                    cmdUpdate.Parameters.AddWithValue("@SurveyName", survey.SurveyName ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@ImplementationType", survey.ImplementationType ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@SurveyDate", survey.SurveyDate ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@SurveyTeamName", survey.SurveyTeamName ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@SurveyTeamContact", survey.SurveyTeamContact ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@AgencyName", survey.AgencyName ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@LocationSiteName", survey.LocationSiteName ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@CityDistrict", survey.CityDistrict ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@ScopeOfWork", survey.ScopeOfWork ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@Latitude", survey.Latitude ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@Longitude", survey.Longitude ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@MapMarking", survey.MapMarking ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("@SurveyStatus", "In Progress"); // Always change to In Progress
                    cmdUpdate.Parameters.AddWithValue("@RegionID", survey.RegionID);
                    cmdUpdate.Parameters.AddWithValue("@CreatedBy", survey.CreatedBy);

                    int surveyResult = cmdUpdate.ExecuteNonQuery();
                    Console.WriteLine($"WithdrawSubmission: Survey status update affected {surveyResult} rows, new status: In Progress");

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WithdrawSubmission transaction error: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WithdrawSubmission outer error: {ex.Message}");
                return false;
            }
        }

        public SurveySubmissionModel? GetSubmissionBySurveyId(long surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SpType", 5); // Select by SurveyId
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                var dt = new DataTable();
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                if (dt.Rows.Count == 0)
                    return null;

                var list = SqlDbHelper.DataTableToList<SurveySubmissionModel>(dt);
                return list.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<SurveySubmissionModel> GetAllSubmissions(int? userId = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SpType", 4); // Select All
                cmd.Parameters.AddWithValue("@SubmittedBy", userId.HasValue ? userId.Value : (object)DBNull.Value);

                con.Open();
                var dt = new DataTable();
                using var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                return SqlDbHelper.DataTableToList<SurveySubmissionModel>(dt);
            }
            catch (Exception)
            {
                return new List<SurveySubmissionModel>();
            }
        }

        public bool UpdateSubmissionStatus(long submissionId, string status, int reviewedBy, string? comments = null)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SpType", 2); // Update Review
                cmd.Parameters.AddWithValue("@SubmissionId", submissionId);
                cmd.Parameters.AddWithValue("@SubmissionStatus", status);
                cmd.Parameters.AddWithValue("@ReviewedBy", reviewedBy);
                cmd.Parameters.AddWithValue("@ReviewDate", DateTime.Now);
                cmd.Parameters.AddWithValue("@ReviewComments", comments ?? (object)DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanEditSurvey(long surveyId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("dbo.SpSurveySubmission", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SpType", 6); // Check if can edit
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);

                con.Open();
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) == 1;
            }
            catch (Exception)
            {
                return true; // Allow editing if check fails
            }
        }

        public SurveyCompletionStatus CheckSurveyCompletionStatus(long surveyId)
        {
            var status = new SurveyCompletionStatus();
            
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                
                // Get all locations for the survey
                using var cmd = new SqlCommand(@"
                    SELECT 
                        sl.LocID,
                        sl.LocName,
                        ISNULL(sls.Status, 'Pending') AS LocationStatus
                    FROM SurveyLocation sl
                    LEFT JOIN SurveyLocationStatus sls ON sl.LocID = sls.LocID AND sl.SurveyID = sls.SurveyID
                    WHERE sl.SurveyID = @SurveyId 
                    AND CAST(sl.Isactive AS VARCHAR(10)) IN ('Y', '1', 'True')", con);
                
                cmd.Parameters.AddWithValue("@SurveyId", surveyId);
                
                con.Open();
                using var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    status.TotalLocations++;
                    var locationStatus = reader["LocationStatus"]?.ToString()?.Trim() ?? "Pending";
                    var locationName = reader["LocName"]?.ToString() ?? "Unknown";
                    
                    if (locationStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) || 
                        locationStatus.Equals("Verified", StringComparison.OrdinalIgnoreCase))
                    {
                        status.CompletedLocations++;
                    }
                    else
                    {
                        status.PendingLocations++;
                        status.IncompleteLocationNames.Add($"{locationName} ({locationStatus})");
                    }
                }
                
                status.IsComplete = status.TotalLocations > 0 && status.PendingLocations == 0;
                
                if (status.TotalLocations == 0)
                {
                    status.Message = "No locations found for this survey. Please add at least one location.";
                }
                else if (!status.IsComplete)
                {
                    status.Message = $"{status.PendingLocations} out of {status.TotalLocations} location(s) are not completed.";
                }
                else
                {
                    status.Message = "All locations are completed. Survey is ready for submission.";
                }
                
                return status;
            }
            catch (Exception ex)
            {
                status.IsComplete = false;
                status.Message = $"Error checking completion status: {ex.Message}";
                return status;
            }
        }

    }
}


