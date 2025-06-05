using AttendanceCRM.BAL.IServices;

namespace AttendanceCRM.BAL.Services
{
    public class PayrollManagementService : IPayrollManagementService
    {
        private readonly MyDbContext _context;


        public PayrollManagementService(MyDbContext context)
        {
            _context = context;
        }


    }
}
