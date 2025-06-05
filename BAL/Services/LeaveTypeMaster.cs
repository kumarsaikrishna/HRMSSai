using AttendanceCRM.BAL.IServices;
using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using System.Runtime.Intrinsics.X86;

namespace AttendanceCRM.BAL.Services
{
    public class LeaveTypeMaster:ILeaveTypeMaster
    {
        private readonly MyDbContext _context;


        public LeaveTypeMaster(MyDbContext context)
        {
            _context = context;
        }
        public IEnumerable<LeaveTypeDTO> GetAllLeaveTypes()
		{
			var obj = (from sr in _context.leaveType
					   where (sr.IsDeleted == false)


					   select new LeaveTypeDTO
					   {
						   LeaveTypeId = sr.LeaveTypeId,
						   LeaveType = sr.LeaveType,
					
					   }).ToList();
			return obj;
        }
        public LeavetypeEntity GetLeaveTypeById(int leaveTypeId) 
        {
            return _context.leaveType.Where(a => a.LeaveTypeId == leaveTypeId).FirstOrDefault();
        }
        
        public List<LeavesEntitie> GetLeaves() 
        {
            return _context.leavesEntitie.Where(a => a.IsDeleted == false).ToList();
        }

        public GenericResponse AddLeave(LeaveTypeDTO lEntity)

        {
            GenericResponse response = new GenericResponse();
            LeavetypeEntity l=new LeavetypeEntity();
            try
            {
                if (lEntity != null)
                {
                    l.LeaveType=lEntity.LeaveType;
                    l.IsActive=true;
                    l.IsDeleted = false;
                    l.CreatedBy = 1;
                    l.CreatedOn = DateTime.Now;
                    _context.leaveType.Add(l);
                    _context.SaveChanges();
                    response.statuCode = 1;
                    response.message = "Success";
                    //response.currentId = sme.UserTypeId;
                    return response;
                }
                else
                {
                    response.statuCode = 0;
                    response.message = "Not Added";
                    //response.currentId = sme.UserTypeId;
                    return response;
                }
            }
            catch (Exception ex)
            
           {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        public GenericResponse UpdateLeave(LeaveTypeDTO lEntity)

        {
            GenericResponse response = new GenericResponse();
            var result=_context.leaveType.Where(a=>a.LeaveTypeId==lEntity.LeaveTypeId).FirstOrDefault();
          
            try
            {
                if (lEntity != null)
                {   result.LeaveTypeId=lEntity.LeaveTypeId;
                    result.LeaveType = lEntity.LeaveType;
                    result.IsActive = true;
                    result.IsDeleted = false;
                    result.UpdatedBy = 1;
                    result.UpdatedOn = DateTime.Now;
                    _context.leaveType.Update(result);
                    _context.SaveChanges();
                    response.statuCode = 1;
                    response.message = "Success";
                    //response.currentId = sme.UserTypeId;
                    return response;
                }
                else
                {
                    response.statuCode = 0;
                    response.message = "Not Updated";
                    //response.currentId = sme.UserTypeId;
                    return response;
                }
            }
            catch (Exception ex)

            {
                response.message = "Failed to Updated : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        public GenericResponse DeleteLeave(int id)

        {
            GenericResponse response = new GenericResponse();
            var result = _context.leaveType.Where(a => a.LeaveTypeId == id).FirstOrDefault();

            try
            {
                if (result != null)
                {
                    result.IsActive = true;
                    result.IsDeleted = true;
                    result.UpdatedBy = 1;
                    result.UpdatedOn = DateTime.Now;
                    _context.leaveType.Update(result);
                    _context.SaveChanges();
                    response.statuCode = 1;
                    response.message = "Success";
                    //response.currentId = sme.UserTypeId;
                    return response;
                }
                else
                {
                    response.statuCode = 0;
                    response.message = "Not Updated";
                    //response.currentId = sme.UserTypeId;
                    return response;
                }
            }
            catch (Exception ex)

            {
                response.message = "Failed to Updated : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
    }
}
