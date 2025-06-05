using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;

namespace AttendanceCRM.BAL.IServices
{
    public interface IMasterMgmtService
    {
        #region UserType
        IEnumerable<UserTypeMasterEntitie> UserTypeList();
        UserTypeMasterEntitie GetUserTypeById(int id);
        GenericResponse CreateUserType(UserTypeMasterEntitie sme);
        GenericResponse UpdateUserType(UserTypeMasterEntitie sme);
        #endregion

        #region  Attendance
        IEnumerable<AttendanceDTO> AttendanceList();
        AttendanceEntitie GetAttendanceeById(int id);
        GenericResponse CreateAttendance(AttendanceEntitie sme);
        GenericResponse UpdateAttendance(AttendanceEntitie sme);

        #endregion

        #region  BankDetails
        IEnumerable<BankDetailsDTO> BankDetailsList(string searchTerm = null);
        BankDetailsEntitie GetBankDetailsById(int id);
        BankDetailsEntitie GetDetailsByuserid(int id);
        List<BankDetailsEntitie> GetBankDetailsByUserId(int userId);
        GenericResponse CreateBankDetails(BankDetailsEntitie sme);
        GenericResponse UpdateBankDetails(BankDetailsEntitie sme);
        #endregion

        #region Holidays
        IEnumerable<HolidaysDTO> HolidaysList(string searchTerm = null);
        HolidaysEntite GetHolidayById(int id);
        GenericResponse CreateHoliday(HolidaysEntite sme);
        GenericResponse UpdateHoliday(HolidaysEntite sme);
        #endregion

        #region PaySlip
        List<PayslipDTO> GetAllPayslipsForUser(int userId);
        PayslipDTO GetPayslipDetails(int payslipId);
        IEnumerable<PayslipDTO> PayslipList();
        PayslipEntitie GetPayslipById(int id);
        GenericResponse CreatePayslip(PayslipEntitie sme);
        GenericResponse UpdatePayslip(PayslipEntitie sme);
        #endregion

        #region Leave
        IEnumerable<LeavesDTO> LeaveList(string searchTerm = null);
        LeavesEntitie GetLeaveById(int id);
        GenericResponse CreateLeave(LeavesEntitie sme);
        GenericResponse UpdateLeave(LeavesEntitie sme);
		#endregion

		#region ModulePermissions
		 Task SavePermissionAsync(ModulePermissionsDTO permission);
        #endregion


        #region Salary Structure
        IEnumerable<SalaryStructureDTO> SalaryList(string searchTerm = null);
        SalaryStructureEntity GetSalaryStructureById(int id);
        GenericResponse CreateSalaryStructure(SalaryStructureEntity sme);
        GenericResponse UpdateSalaryStructure(SalaryStructureEntity sme);
        #endregion

        IEnumerable<UserMasterDTO> PresentList();
        GenericResponse UpdateLeaveStatus(int id,int uid);
        IEnumerable<LeavesDTO> LeaveListindividually(int id, string searchTerm = null);


        IEnumerable<ProjectEntity> projectlist();


    }
}
