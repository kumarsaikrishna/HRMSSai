using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;

namespace AttendanceCRM.BAL.IServices
{
    public interface IUserService
    {

        int TotalEmplyee();
        int GetPresentEmployeesToday();
        int GetTodayAbsent();
        int GetCurrentProjects();


        int TotalEmplyeeActive();
        int TotalEmplyeeInActive();
        int TotalEmplyeeNewJoin();
        IEnumerable<UserMasterDTO> UserListActive(string searchTerm = null);
        IEnumerable<UserMasterDTO> UserListInActive(string searchTerm = null);
        IEnumerable<UserMasterDTO> UserListNewJoin(string searchTerm = null);
        IEnumerable<UserMasterDTO> UserList(string searchTerm = null);
        UserMasterEntitie GetUserById(int id);
        GenericResponse UpdateUser(UserMasterEntitie sme);
        GenericResponse CreateUser(UserMasterEntitie sme);


        LoginResponse LoginCheck(LoginRequest request);
        GenericResponse SaveUser(UserMasterEntitie usersEntity);
        GenericResponse UpdateOtpTransactions(OTPDetailsEntity ut);
        bool PushEmail(string emailtext, string to, string subject, string cc = "", byte[] attachement = null);


         Task<UserMasterEntitie> GetUserByEmailAsync(string email);
        GenericResponse ResetPassword(UserMasterEntitie request);
        IEnumerable<PerformanceDTO> GetPerformanceData();
        List<EmployeeDepartmentDto> GetEmployeesByDepartment();


    }
}
