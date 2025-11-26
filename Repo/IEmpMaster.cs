using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public interface IEmpMaster
    {
        List<EmpMasterModel> GetAllEmployees();
        EmpMasterModel GetEmployeeById(int empId);
        bool AddEmployee(EmpMasterModel employee);
        bool UpdateEmployee(EmpMasterModel employee);
        bool DeleteEmployee(int empId);
    }
}
