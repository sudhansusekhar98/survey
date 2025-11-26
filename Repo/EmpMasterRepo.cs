using SurveyApp.Models;
using AnalyticaDocs.Util;
using Microsoft.Data.SqlClient;
using SurveyApp.Repo;
using System.Data;

namespace AnalyticaDocs.Repo
{
    public class EmpMasterRepo : IEmpMaster
    {
        public List<EmpMasterModel> GetAllEmployees()
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpEmpMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 4); // GET ALL

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                return SqlDbHelper.DataTableToList<EmpMasterModel>(dt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employees: {ex.Message}", ex);
            }
        }

        public EmpMasterModel GetEmployeeById(int empId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpEmpMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 5); // GET BY ID
                cmd.Parameters.AddWithValue("@EmpID", empId);

                con.Open();

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                var employees = SqlDbHelper.DataTableToList<EmpMasterModel>(dt);
                return employees.FirstOrDefault() ?? new EmpMasterModel();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving employee: {ex.Message}", ex);
            }
        }

        public bool AddEmployee(EmpMasterModel employee)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpEmpMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 1); // INSERT
                cmd.Parameters.AddWithValue("@EmpCode", employee.EmpCode);
                cmd.Parameters.AddWithValue("@EmpName", employee.EmpName);
                cmd.Parameters.AddWithValue("@Gender", (object?)employee.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth", (object?)employee.DateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MobileNo", (object?)employee.MobileNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)employee.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine1", (object?)employee.AddressLine1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine2", (object?)employee.AddressLine2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@City", (object?)employee.City ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@State", (object?)employee.State ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Country", (object?)employee.Country ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PinCode", (object?)employee.PinCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DeptID", (object?)employee.DeptID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Designation", (object?)employee.Designation ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfJoining", (object?)employee.DateOfJoining ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfLeaving", (object?)employee.DateOfLeaving ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmploymentType", (object?)employee.EmploymentType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", (object?)employee.CreatedBy ?? DBNull.Value);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding employee: {ex.Message}", ex);
            }
        }

        public bool UpdateEmployee(EmpMasterModel employee)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpEmpMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 2); // UPDATE
                cmd.Parameters.AddWithValue("@EmpID", employee.EmpID);
                cmd.Parameters.AddWithValue("@EmpCode", employee.EmpCode);
                cmd.Parameters.AddWithValue("@EmpName", employee.EmpName);
                cmd.Parameters.AddWithValue("@Gender", (object?)employee.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth", (object?)employee.DateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MobileNo", (object?)employee.MobileNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object?)employee.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine1", (object?)employee.AddressLine1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AddressLine2", (object?)employee.AddressLine2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@City", (object?)employee.City ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@State", (object?)employee.State ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Country", (object?)employee.Country ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PinCode", (object?)employee.PinCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DeptID", (object?)employee.DeptID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Designation", (object?)employee.Designation ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfJoining", (object?)employee.DateOfJoining ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfLeaving", (object?)employee.DateOfLeaving ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmploymentType", (object?)employee.EmploymentType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
                cmd.Parameters.AddWithValue("@ModifiedBy", (object?)employee.ModifiedBy ?? DBNull.Value);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating employee: {ex.Message}", ex);
            }
        }

        public bool DeleteEmployee(int empId)
        {
            try
            {
                using var con = new SqlConnection(DBConnection.ConnectionString);
                using var cmd = new SqlCommand("SpEmpMaster", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SpType", 3); // DELETE
                cmd.Parameters.AddWithValue("@EmpID", empId);

                con.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting employee: {ex.Message}", ex);
            }
        }
    }
}
