using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;

namespace AttendanceCRM.BAL.IServices
{
    public interface ILeaveTypeMaster
    {
        IEnumerable<LeaveTypeDTO> GetAllLeaveTypes();
        List<LeavesEntitie> GetLeaves();


        LeavetypeEntity GetLeaveTypeById(int leaveTypeId);
        GenericResponse AddLeave(LeaveTypeDTO lEntity);
        GenericResponse UpdateLeave(LeaveTypeDTO lEntity);
        GenericResponse DeleteLeave(int id);
    }
}
