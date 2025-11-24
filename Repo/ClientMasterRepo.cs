using SurveyApp.Models;
using AnalyticaDocs.Util;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SurveyApp.Repo
{
    public class ClientMasterRepo : IClientMaster
    {
        // Get all active clients
        public List<ClientMasterModel> GetAllClients()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SpSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SpType", 24); // SELECT all clients
                        cmd.Parameters.AddWithValue("@ClientID", DBNull.Value);

                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        return SqlDbHelper.DataTableToList<ClientMasterModel>(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllClients: {ex.Message}");
                return new List<ClientMasterModel>();
            }
        }

        // Get client by ID
        public ClientMasterModel? GetClientById(int clientId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SpSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SpType", 24); // SELECT by ID
                        cmd.Parameters.AddWithValue("@ClientID", clientId);

                        conn.Open();
                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        var result = SqlDbHelper.DataTableToList<ClientMasterModel>(dt);
                        return result?.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetClientById: {ex.Message}");
                return null;
            }
        }

        // Insert new client
        public int InsertClient(ClientMasterModel model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SpSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SpType", 21); // INSERT
                        cmd.Parameters.AddWithValue("@ClientName", model.ClientName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ClientType", model.ClientType ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Address1", model.Address1 ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address3", model.Address3 ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@State", model.State ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@City", model.City ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactPerson", model.ContactPerson ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value);

                        conn.Open();
                        var result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertClient: {ex.Message}");
                return 0;
            }
        }

        // Update existing client
        public bool UpdateClient(ClientMasterModel model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SpSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SpType", 22); // UPDATE
                        cmd.Parameters.AddWithValue("@ClientID", model.ClientID);
                        cmd.Parameters.AddWithValue("@ClientName", model.ClientName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ClientType", model.ClientType ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Address1", model.Address1 ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address3", model.Address3 ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@State", model.State ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@City", model.City ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactPerson", model.ContactPerson ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNumber", model.ContactNumber ?? (object)DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateClient: {ex.Message}");
                return false;
            }
        }

        // Delete client (soft delete)
        public bool DeleteClient(int clientId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBConnection.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SpSurvey", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@SpType", 23); // DELETE
                        cmd.Parameters.AddWithValue("@ClientID", clientId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteClient: {ex.Message}");
                return false;
            }
        }
    }
}
