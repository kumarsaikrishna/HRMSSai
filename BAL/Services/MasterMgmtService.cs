using AttendanceCRM.BAL.IServices;
using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace AttendanceCRM.BAL.Services
{
    public class MasterMgmtService: IMasterMgmtService
    {
        private readonly MyDbContext _context;


        public MasterMgmtService(MyDbContext context)
        {
            _context = context;
        }


        #region UserType
        public IEnumerable<UserTypeMasterEntitie> UserTypeList()
        {


            var obj = (from sr in _context.userTypeMasterEntitie

                       where (sr.IsDeleted == false)


                       select new UserTypeMasterEntitie
                       {
                          UserTypeId=sr.UserTypeId,
                          UserTypeName=sr.UserTypeName,
                          Designation=sr.Designation,
                           IsDeleted = sr.IsDeleted,


                       }).ToList();

            return obj;
        }



        public UserTypeMasterEntitie GetUserTypeById(int id)
        {
            return _context.userTypeMasterEntitie.Where(a => a.UserTypeId == id).FirstOrDefault();
        }

        public GenericResponse CreateUserType(UserTypeMasterEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                _context.userTypeMasterEntitie.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.UserTypeId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdateUserType(UserTypeMasterEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var u = _context.userTypeMasterEntitie.Where(a => a.UserTypeId == sme.UserTypeId).FirstOrDefault();
                _context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.UserTypeId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        #endregion

        #region Attendance
        public IEnumerable<AttendanceDTO> AttendanceList()
        {


            var obj = (from sr in _context.attendanceEntitie

                       where (sr.IsDeleted == false)


                       select new AttendanceDTO
                       {
                          AttendanceId=sr.AttendanceId,
                          GracePeriodTime=sr.GracePeriodTime,   
                          UserId=sr.UserId,
                          PunchInTime=sr.PunchInTime,
                          PunchOutTime=sr.PunchOutTime,
                          ScreenShot=sr.ScreenShot,
                           IsDeleted = sr.IsDeleted,


                       }).ToList();

            return obj;
        }
        public List<AttendanceViewModel> GetFilteredAttendance(string filterType, DateTime? startDate, DateTime? endDate, int? userId)
        {
            IQueryable<AttendanceEntitie> query = _context.attendanceEntitie;

            // Filter by user
            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }

            DateTime today = DateTime.Today;

            switch (filterType)
            {
                case "Daily":
                    query = query.Where(a => a.CreatedOn == today);
                    break;

                case "Weekly":
                    DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
                    DateTime weekEnd = weekStart.AddDays(6);
                    query = query.Where(a => a.CreatedOn >= weekStart && a.CreatedOn <= weekEnd);
                    break;

                case "Monthly":
                    DateTime monthStart = new DateTime(today.Year, today.Month, 1);
                    DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    query = query.Where(a => a.CreatedOn >= monthStart && a.CreatedOn <= monthEnd);
                    break;

                case "Custom":
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        DateTime sDate = startDate.Value.Date;
                        DateTime eDate = endDate.Value.Date;
                        query = query.Where(a => a.CreatedOn >= sDate && a.CreatedOn <= eDate);
                    }
                    break;
            }

            var result = query
                .Select(a => new AttendanceViewModel
                {
                    UserId = a.UserId ?? 0,
                    UserName =_context.userMasterEntitie.Where(a=>a.UserId==a.UserId).Select(a=>a.UserName).FirstOrDefault(),
                    Designation = _context.userMasterEntitie.Where(a => a.UserId == a.UserId).Select(a => a.Designation).FirstOrDefault(),
                    CreatedOn = a.CreatedOn,
                    PunchInTime = a.PunchInTime,
                    PunchOutTime = a.PunchOutTime,
                    SelfiePath=a.SelfiePath,
                    PunchOutSelfiePath=a.PunchOutSelfiePath,
                    PunchOutLatitude = a.PunchOutLatitude,
                    PunchOutLongitude = a.PunchOutLongitude,
                    Latitude=a.Latitude,
                    Longitude=a.Longitude,
                    GracePeriodTime = a.GracePeriodTime
                })
                .OrderByDescending(a => a.CreatedOn)
                .ToList();

            return result;
        }

        public List<AttendanceViewModel> GetPunchDetails(int userId, string filterType, DateTime? startDate, DateTime? endDate)
        {
            return GetFilteredAttendance(filterType, startDate, endDate, userId)
                                  .OrderBy(a => a.CreatedOn)
                                  .ToList();
        }
        public AttendanceEntitie GetAttendanceeById(int id)
        {
            return _context.attendanceEntitie.Where(a => a.AttendanceId == id).FirstOrDefault();
        }

        public GenericResponse CreateAttendance(AttendanceEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                _context.attendanceEntitie.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.AttendanceId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdateAttendance(AttendanceEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var u = _context.attendanceEntitie.Where(a => a.AttendanceId == sme.AttendanceId).FirstOrDefault();
                _context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.AttendanceId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        #endregion


        #region BankDetails
        public IEnumerable<BankDetailsDTO> BankDetailsList(string searchTerm = null)
        {
            var query = (from sr in _context.bankDetailsEntitie
                       join usr in _context.userMasterEntitie
                           on sr.UserId equals usr.UserId
                       where sr.IsDeleted == false && usr.IsDeleted == false
                       select new BankDetailsDTO
                       {
                           BankId = sr.BankId,
                           UserId = sr.UserId,
                           AccountNumber = sr.AccountNumber,
                           IFSCNumber = sr.IFSCNumber,
                           BranchName = sr.BranchName,
                           AccountType = sr.AccountType,
                           IsDeleted = sr.IsDeleted,
                           UserName = usr.UserName // <-- Added field from userMasterEntitie
                       }).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) 
                ).ToList();
            }

            return query.OrderByDescending(x => x.CreatedOn).ToList();
        }


        public BankDetailsEntitie GetBankDetailsById(int id)
        {
            return _context.bankDetailsEntitie.Where(a => a.BankId == id).FirstOrDefault();
        }
        public BankDetailsEntitie GetDetailsByuserid(int id)
        {
            return _context.bankDetailsEntitie.Where(a => a.UserId == id&& a.IsDeleted==false && a.IsActive==true).FirstOrDefault();
        }

        public List<BankDetailsEntitie> GetBankDetailsByUserId(int userId)
        {
            return _context.bankDetailsEntitie
                           .Where(b => b.UserId == userId && b.IsDeleted == false)
                           .ToList();
        }


        public GenericResponse CreateBankDetails(BankDetailsEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                sme.CreatedOn = DateTime.UtcNow;
                _context.bankDetailsEntitie.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.BankId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdateBankDetails(BankDetailsEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var u = _context.bankDetailsEntitie.Where(a => a.BankId == sme.BankId).FirstOrDefault();
                _context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.BankId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        #endregion

        #region Holidays
        public IEnumerable<HolidaysDTO> HolidaysList(string searchTerm = null)
        {
            var query = (from sr in _context.holidaysEntite
                         join c in _context.userTypeMasterEntitie on sr.UserTypeId equals c.UserTypeId
                         where sr.IsDeleted == false
                         select new HolidaysDTO
                         {
                             HolidayId = sr.HolidayId,
                             UserTypeId = sr.UserTypeId,
                             HolidayDate = sr.HolidayDate,
                             HolidayName = sr.HolidayName,
                             HolidayDescription = sr.HolidayDescription,
                             IsDeleted = sr.IsDeleted,
                             UserTypeName = c.UserTypeName,
                             IsActive = sr.IsActive
                         });

            // Apply search in DB
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.HolidayName ?? "").ToLower().Contains(searchTerm)
                );
            }

            // Force materialization first
            var result = query.ToList();


            return result;
        }
      
        public HolidaysEntite GetHolidayById(int id)
        {
            return _context.holidaysEntite.Where(a => a.HolidayId == id).FirstOrDefault();
        }

        public GenericResponse CreateHoliday(HolidaysEntite sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                _context.holidaysEntite.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.HolidayId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdateHoliday(HolidaysEntite sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var u = _context.holidaysEntite.Where(a => a.HolidayId == sme.HolidayId).FirstOrDefault();
                _context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.HolidayId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        #endregion

        #region PaySlip

        public List<PayslipDTO> GetAllPayslipsForUser(int userId)
        {
            var payslipList = (from p in _context.payslipEntitie
                               join u in _context.userMasterEntitie on p.UserId equals u.UserId
                               where p.UserId == userId && p.IsDeleted == false
                               select new PayslipDTO
                               {
                                   PayslipId = p.PayslipId,
                                   SalaryMonth = p.SalaryMonth,
                                   NetSalary = p.NetSalary,
                                   Designation = u.Designation,
                                   EmployeeName = u.UserName
                               }).ToList();

            return payslipList;
        }



        public PayslipDTO GetPayslipDetails(int payslipId)
        {
            var payslip = (from sr in _context.payslipEntitie
                           join u in _context.userMasterEntitie on sr.UserId equals u.UserId
                           join b in _context.bankDetailsEntitie on sr.UserId equals b.UserId into bankJoin
                           from b in bankJoin.DefaultIfEmpty()
                           join ss in _context.salaryStructure on sr.UserId equals ss.UserId into salaryJoin
                           from ss in salaryJoin.DefaultIfEmpty()
                           where sr.PayslipId == payslipId && sr.IsDeleted == false
                           select new PayslipDTO
                           {
                               PayslipId = sr.PayslipId,
                               UserId = sr.UserId,
                               SalaryMonth = sr.SalaryMonth,

                               // SalaryStructure
                               BasicSalary = ss != null ? ss.BasicSalary : 0,
                               HRA = ss != null ? ss.HRA : 0,
                               SpecialAllowance = ss != null ? ss.SpecialAllowance : 0,
                               Conveyance = ss != null ? ss.Conveyance : 0,
                               PF_Employee = ss != null ? ss.PF_Employee : 0,
                               ProfessionalTax = ss != null ? ss.ProfessionalTax : 0,

                               // From Payslip table if needed
                               Bonus = sr.Bonus,
                               Deductions = sr.Deductions,

                               // From User
                               EmployeeName = u.UserName,
                               EmployeeCode = u.EmployeeId,
                               Designation = u.Designation,
                               DOJ = u.DateOfJoining,
                               PAN = u.PanNumber,

                               // Bank
                               AccountNumber = b.AccountNumber,
                               BankName = b.BranchName,
                               IFSC = b.IFSCNumber,
                           }).FirstOrDefault();

            if (payslip != null && DateTime.TryParse(payslip.SalaryMonth, out DateTime salaryDate))
            {
                int totalDays = DateTime.DaysInMonth(salaryDate.Year, salaryDate.Month);

                var presentDays = _context.attendanceEntitie
                    .Where(a => a.UserId == payslip.UserId &&
                                a.PunchInTime.HasValue &&
                                a.PunchInTime.Value.Month == salaryDate.Month &&
                                a.PunchInTime.Value.Year == salaryDate.Year &&
                                a.Status == "Present")
                    .Count();

                payslip.LOP = totalDays - presentDays;
                payslip.PayDays = presentDays;
            }

            // ✨ Calculating gross salary
            payslip.GrossSalary = payslip.BasicSalary + payslip.HRA + payslip.SpecialAllowance + payslip.Conveyance + payslip.Bonus;

            // ✨ Calculating total deductions
            payslip.Deductions = payslip.PF_Employee + payslip.ProfessionalTax; // + TDS if needed

            // ✨ Net salary
            payslip.NetSalary = payslip.GrossSalary - payslip.Deductions;

            payslip.NetSalaryInWords = ConvertAmountToWords((int)payslip.NetSalary);


            return payslip;
        }



        public static string ConvertAmountToWords(int number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "minus " + ConvertAmountToWords(Math.Abs(number));

            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                          "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += ConvertAmountToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += ConvertAmountToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += ConvertAmountToWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }

        
        public IEnumerable<PayslipDTO> PayslipList()
        {


            var obj = (from sr in _context.payslipEntitie

                       where (sr.IsDeleted == false)


                       select new PayslipDTO
                       {
                          PayslipId = sr.PayslipId,
                          UserId= sr.UserId,
                          SalaryMonth = sr.SalaryMonth,
                          BasicSalary=sr.BasicSalary,
                          HRA=sr.HRA,
                          Bonus=sr.Bonus,
                          Deductions=sr.Deductions,
                          GrossSalary=sr.GrossSalary,
                          NetSalary=sr.NetSalary,

                           IsDeleted = sr.IsDeleted,


                       }).ToList();

            return obj;
        }



        public PayslipEntitie GetPayslipById(int id)
        {
            return _context.payslipEntitie.Where(a => a.PayslipId == id).FirstOrDefault();
        }

        public GenericResponse CreatePayslip(PayslipEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                _context.payslipEntitie.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.PayslipId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdatePayslip(PayslipEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var u = _context.payslipEntitie.Where(a => a.PayslipId == sme.PayslipId).FirstOrDefault();
                _context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.PayslipId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        #endregion

        #region Salary Structure
        public IEnumerable<SalaryStructureDTO> SalaryList(string searchTerm = null)
        {
            var query = (from sr in _context.salaryStructure

                         where sr.IsDeleted == false
                         select new SalaryStructureDTO
                         {
                             SalaryStructureId = sr.SalaryStructureId,
                             UserId = sr.UserId,
                             UserType = (from u in _context.userMasterEntitie
                                                 join ut in _context.userTypeMasterEntitie on u.UserTypeId equals ut.UserTypeId
                                                 where u.UserId == sr.UserId
                                                 select ut.UserTypeName).FirstOrDefault(),
                             UserName =_context.userMasterEntitie.Where(a=>a.UserId==sr.UserId).Select(a=>a.UserName).FirstOrDefault(),
                             BasicSalary = sr.BasicSalary,
                             JoiningDate= _context.userMasterEntitie.Where(a => a.UserId == sr.UserId).Select(a => a.DateOfJoining).FirstOrDefault(),
                             HRA = sr.HRA,
                             SpecialAllowance = sr.SpecialAllowance,
                             Conveyance = sr.Conveyance,
                             PF_Employee = sr.PF_Employee,
                             ProfessionalTax = sr.ProfessionalTax,
                         }).ToList();


            return query;
        }


        public SalaryStructureEntity GetSalaryStructureById(int id)
        {
            return _context.salaryStructure.Where(a => a.SalaryStructureId == id).FirstOrDefault();
        }

        public GenericResponse CreateSalaryStructure(SalaryStructureEntity sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                sme.CreatedOn = DateTime.UtcNow;
                _context.salaryStructure.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.SalaryStructureId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdateSalaryStructure(SalaryStructureEntity sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var u = _context.salaryStructure.Where(a => a.SalaryStructureId == sme.SalaryStructureId).FirstOrDefault();
                _context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.SalaryStructureId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
        #endregion
        public IEnumerable<UserMasterDTO> PresentList()
        {


            var obj = (from sr in _context.userMasterEntitie
                       join c in _context.attendanceEntitie
                       on sr.UserId equals c.UserId
                       where sr.IsDeleted==false && EF.Functions.DateDiffDay(c.PunchInTime, DateTime.Today) == 0
                       select new UserMasterDTO
                       {
                           UserId = sr.UserId,
                           UserName = sr.UserName,
                           DateOfJoining = sr.DateOfJoining,
                           Email = sr.Email,
                           DateOfBirth = sr.DateOfBirth,
                           MobileNumber = sr.MobileNumber,
                           GuardianNumber = sr.GuardianNumber,
                           ProfilePicture = sr.ProfilePicture,
                           EmployeeId = sr.EmployeeId,
                           CollegeName = sr.CollegeName,
                           Address = sr.Address,
                           Designation = sr.Designation,
                           AdharNumber = sr.AdharNumber,
                           PanNumber = sr.PanNumber,
                           UserTypeId = sr.UserTypeId,
                           IsDeleted = sr.IsDeleted,
                           PunchInTime = c.PunchInTime,
                           PunchOutTime = c.PunchOutTime
                       }).ToList();


            return obj;
        }

        #region Leave
        public IEnumerable<LeavesDTO> LeaveList(string searchTerm = null)
        {
            var query = (from sr in _context.leavesEntitie
                         join lt in _context.leaveType
                             on sr.LeaveTypeId equals lt.LeaveTypeId
                         join um in _context.userMasterEntitie
                             on sr.UserId equals um.UserId into userJoin
                         from um in userJoin.DefaultIfEmpty()
                         where sr.IsDeleted == false && sr.IsActive == true
                         select new LeavesDTO
                         {
                             LeaveId = sr.LeaveId,
                             Description = sr.Description,
                             LeaveTypeId = sr.LeaveTypeId,
                             UserName = _context.userMasterEntitie
                                 .Where(a => a.UserId == sr.UserId)
                                 .Select(a => a.UserName)
                                 .FirstOrDefault(),
                             UserTypeId = sr.UserId,
                             Remarks = sr.Remarks,
                             FromDate = sr.FromDate,
                             ToDate = sr.ToDate,
                             LeaveType = lt.LeaveType,
                             Status = sr.Status,
                             ApprovedBy = sr.ApprovedBy,
                           
                             IsDeleted = sr.IsDeleted,
                             CreatedOn = sr.CreatedOn
                         });

            // Search implementation
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    x.UserName.ToLower().Contains(searchTerm) ||
                    x.LeaveType.ToLower().Contains(searchTerm) ||
                    x.Status.ToLower().Contains(searchTerm) ||
                    x.Description.ToLower().Contains(searchTerm)  // Adding search for ApprovedByName
                );
            }

            return query.OrderByDescending(x => x.CreatedOn).ToList();
        }


        public IEnumerable<LeavesDTO> LeaveListindividually(int id, string searchTerm = null)
        {
            var query = (from sr in _context.leavesEntitie
                         join lt in _context.leaveType on sr.LeaveTypeId equals lt.LeaveTypeId
                         join um in _context.userMasterEntitie on sr.ApprovedBy equals um.UserId into userJoin
                         from um in userJoin.DefaultIfEmpty()
                         where sr.UserId == id && sr.IsDeleted == false && sr.IsActive == true
                         select new LeavesDTO
                         {
                             LeaveId = sr.LeaveId,
                             Description = sr.Description,
                             LeaveTypeId = sr.LeaveTypeId,
                             UserId = sr.UserId,
                             UserTypeId = sr.UserId,
                             Approve = um != null ? um.UserName : "",
                             Remarks = sr.Remarks,
                             FromDate = sr.FromDate,
                             ToDate = sr.ToDate,
                             LeaveType = lt.LeaveType,
                             Status = sr.Status,
                             IsDeleted = sr.IsDeleted,
                             CreatedOn = sr.CreatedOn,
                             UserName = um.UserName // 🔸 assuming navigation property exists
                         }).AsEnumerable();

            // 🔍 Search filter (case-insensitive and null-safe)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();

                query = query.Where(x =>
                    (x.LeaveType ?? "").ToLower().Contains(searchTerm) ||
                    (x.Description ?? "").ToLower().Contains(searchTerm) ||
                    (x.Remarks ?? "").ToLower().Contains(searchTerm) ||
                    (x.Status ?? "").ToLower().Contains(searchTerm) ||
                    (x.Approve ?? "").ToLower().Contains(searchTerm) ||
                    (x.UserName ?? "").ToLower().Contains(searchTerm)
                );
            }

            return query.OrderByDescending(x => x.CreatedOn).ToList();
        }



        public LeavesEntitie GetLeaveById(int id)
        {
            return _context.leavesEntitie.Where(a => a.LeaveId == id).FirstOrDefault();
        }

        public GenericResponse CreateLeave(LeavesEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                _context.leavesEntitie.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.LeaveId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdateLeave(LeavesEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var u = _context.leavesEntitie.Where(a => a.LeaveId == sme.LeaveId).FirstOrDefault();
                _context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.LeaveId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
		#endregion


		#region ModulePermissions

		public async Task SavePermissionAsync(ModulePermissionsDTO permission)
		{
			var entity = await _context.modulePermissionsEntity.FirstOrDefaultAsync(p => p.ModuleName == permission.ModuleName);
			if (entity == null)
			{
				entity = new ModulePermissionsEntity
				{
					ModuleName = permission.ModuleName,
					IsEnabled = permission.IsEnabled,
					CanRead = permission.CanRead,
					CanWrite = permission.CanWrite,
					CanCreate = permission.CanCreate,
					CanDelete = permission.CanDelete,
					CanImport = permission.CanImport,
					CanExport = permission.CanExport
				};
				await _context.modulePermissionsEntity.AddAsync(entity);
			}
			else
			{
				entity.IsEnabled = permission.IsEnabled;
				entity.CanRead = permission.CanRead;
				entity.CanWrite = permission.CanWrite;
				entity.CanCreate = permission.CanCreate;
				entity.CanDelete = permission.CanDelete;
				entity.CanImport = permission.CanImport;
				entity.CanExport = permission.CanExport;
				_context.modulePermissionsEntity.Update(entity);
			}

			await _context.SaveChangesAsync();
		}
        #endregion

        public GenericResponse UpdateLeaveStatus(int id, int uid)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                var res = _context.leavesEntitie
                    .Where(a => a.LeaveId == id && a.Status == null)
                    .FirstOrDefault();
                var u = _context.notificationEntity
                    .Where(a => a.UserId == res.UserId && a.IsRead==false)
                    .FirstOrDefault();
                
                if (u == null)
                {
                    response.statuCode = 0;
                    response.message = "Notification not found.";
                    return response;
                }
                
                int userid = Convert.ToInt32(u.UserId);
                DateTime date = Convert.ToDateTime(u.CreatedOn);
                var re = _context.leavesEntitie
                    .Where(a => a.UserId == userid &&
                                a.CreatedOn != null && 
                                a.CreatedOn.Value.Date == date.Date &&
                                a.ApprovedBy == null)
                    .FirstOrDefault();
               

                if (re != null)
                {
                    re.ApprovedBy = uid;
                    re.Status = "Accepted";
                    _context.leavesEntitie.Update(re);
                    _context.SaveChanges();
                    u.IsRead = true;
                    _context.notificationEntity.Update(u);
                    _context.SaveChanges();
                    response.statuCode = 1;
                    response.message = "Success";
                }
                else
                {
                    response.statuCode = 0;
                    response.message = "No matching leave record found.";
                }
            }
            catch (Exception ex)
            {
                response.message = "Failed to update: " + ex.Message;
                response.currentId = 0;
            }
            return response;
        }



        public IEnumerable<ProjectEntity> projectlist()
        {


            var obj = _context.projectEntities.Where(t => t.IsDeleted == false).ToList();

                       

            return obj;
        }

    }
}
