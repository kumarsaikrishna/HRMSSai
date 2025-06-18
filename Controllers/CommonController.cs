using AttendanceCRM.BAL.IServices;
using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Bibliography;

namespace AttendanceCRM.Controllers
{
    public class CommonController : Controller
    {
        private readonly IUserService _Service;
        private readonly IMasterMgmtService _mService;
        private readonly ILeaveTypeMaster _lService;
        private readonly MyDbContext _context;

        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;


        public CommonController(IUserService Service, MyDbContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IMasterMgmtService mService, ILeaveTypeMaster lService)
        {
            _Service = Service;

            _hostingEnvironment = hostingEnvironment;
            _mService = mService;
            _context = context;
            _lService = lService;
        }
        #region USERTYPE
        public async Task<IActionResult> UserTypeList()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            var steps = _mService.UserTypeList().ToList();
            return View(steps);
        }

        [HttpPost]
        public GenericResponse UserType(UserTypeDTO request)
        {
            UserTypeMasterEntitie pp = new UserTypeMasterEntitie();
            GenericResponse res = new GenericResponse();
            try
            {
                if (request.UserTypeId > 0)
                {
                    var obj = _mService.GetUserTypeById(request.UserTypeId);
                    LoginResponseModel lr = new LoginResponseModel();

                    if (obj.UserTypeName == request.UserTypeName)
                    {
                        obj.UserTypeName = request.UserTypeName;

                        obj.IsDeleted = false;
                        res = _mService.UpdateUserType(obj);
                    }
                    else
                    {

                        var check = _mService.UserTypeList().Where(a => a.IsDeleted == false &&
                        a.UserTypeName.Trim().ToLower().Equals(request.UserTypeName.Trim().ToLower())).FirstOrDefault();
                        if (check != null)
                        {

                            res.statuCode = 0;
                            res.message = "User Type Name already exists";
                            return res;
                        }
                        obj.UserTypeName = request.UserTypeName;

                        obj.IsDeleted = false;
                        res = _mService.CreateUserType(obj);
                    }
                }
                else
                {
                    var check = _mService.UserTypeList().Where(a => a.IsDeleted == false &&
                        a.UserTypeName.Trim().ToLower().Equals(request.UserTypeName.Trim().ToLower())).FirstOrDefault();
                    if (check != null)
                    {

                        res.statuCode = 0;
                        res.message = "User Type Name already exists";
                        return res;
                    }

                    pp.UserTypeName = request.UserTypeName;

                    pp.IsDeleted = false;
                    res = _mService.CreateUserType(pp);
                }


            }
            catch (Exception ex)
            {
                res.message = "Failed to update : " + ex.Message;
                res.currentId = 0;
                return res;
            }

            return res;

        }

        public async Task<UserTypeMasterEntitie> GetUserTypeById(int id)
        {
            UserTypeMasterEntitie obj = new UserTypeMasterEntitie();
            obj = _mService.GetUserTypeById(id);
            return obj;
        }


        [HttpPost]
        public GenericResponse DeleteUserType(int idd)
        {
            GenericResponse res = new GenericResponse();
            if (idd > 0)
            {
                var check = _mService.GetUserTypeById(idd);
                if (check != null)
                {
                    check.IsDeleted = true;
                    res = _mService.UpdateUserType(check);

                }


            }
            return res;
        }
        #endregion

        #region Attendance
        public async Task<IActionResult> AttendanceList()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            var steps = _mService.AttendanceList().ToList();
            return View(steps);
        }

        [HttpPost]
        public GenericResponse Attendance(AttendanceDTO request)
        {
            AttendanceEntitie pp = new AttendanceEntitie();
            GenericResponse res = new GenericResponse();
            try
            {
                if (request.AttendanceId > 0)
                {

                    LoginResponseModel lr = new LoginResponseModel();


                    pp.GracePeriodTime = request.GracePeriodTime;
                    pp.UserId = request.UserId;
                    pp.PunchInTime = request.PunchInTime;
                    pp.PunchOutTime = request.PunchOutTime;
                    pp.ScreenShot = request.ScreenShot;
                    pp.IsDeleted = false;
                    res = _mService.CreateAttendance(pp);

                }
                else
                {


                    pp.GracePeriodTime = request.GracePeriodTime;
                    pp.UserId = request.UserId;
                    pp.PunchInTime = request.PunchInTime;
                    pp.PunchOutTime = request.PunchOutTime;
                    pp.ScreenShot = request.ScreenShot;
                    pp.IsDeleted = false;
                    res = _mService.UpdateAttendance(pp);
                }


            }
            catch (Exception ex)
            {
                res.message = "Failed to update : " + ex.Message;
                res.currentId = 0;
                return res;
            }

            return res;

        }

        public async Task<AttendanceEntitie> GetAttendanceeById(int id)
        {
            AttendanceEntitie obj = new AttendanceEntitie();
            obj = _mService.GetAttendanceeById(id);
            return obj;
        }


        [HttpPost]
        public GenericResponse DeleteAttendance(int idd)
        {
            GenericResponse res = new GenericResponse();
            if (idd > 0)
            {
                var check = _mService.GetAttendanceeById(idd);
                if (check != null)
                {
                    check.IsDeleted = true;
                    res = _mService.UpdateAttendance(check);

                }


            }
            return res;
        }

        public IActionResult FilterAttendance(string filterType, DateTime? startDate, DateTime? endDate, int? userId)
        {
            var data = _mService.GetFilteredAttendance(filterType, startDate, endDate, userId);

            var grouped = data
                .GroupBy(a => a.UserId)
                .Select(g => g.First()) 
                .ToList();

            return PartialView("_AttendanceTable", grouped);
        }

        public IActionResult GetPunchDetails(int userId, string filterType, DateTime? startDate, DateTime? endDate)
        {
            var punchData = _mService.GetPunchDetails(userId, filterType, startDate, endDate);
            return PartialView("_PunchDetailsModal", punchData);
        }


        #endregion

        #region BankDetails

        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> BankDetailsList(string searchTerm, int page = 1, int pageSize = 10)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            ViewBag.userlist = new SelectList(_context.userMasterEntitie, "UserId", "UserName");
            var steps = _mService.BankDetailsList().ToList();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                steps = steps.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = steps
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = steps.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)steps.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_bankdetails", paginatedLeaves);
            }



            return View(paginatedLeaves);
        }

        [HttpPost]
        public IActionResult AttenBankDetails(BankDetailsDTO request)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            BankDetailsEntitie pp = new BankDetailsEntitie();
            GenericResponse res = new GenericResponse();

            try
            {
                pp.BankId = request.BankId;
                pp.AccountNumber = request.AccountNumber;
                pp.IFSCNumber = request.IFSCNumber;
                pp.BranchName = request.BranchName;
                pp.AccountType = request.AccountType;
                pp.UserId = request.UserId;
                pp.IsDeleted = false;
                pp.IsActive = true;


                if (request.BankId == 0)
                {
                    res = _mService.CreateBankDetails(pp);
                }
                else
                {
                    res = _mService.UpdateBankDetails(pp);
                }

                if (res.statuCode == 1)
                {

                    return RedirectToAction("BankDetailsList"); // Replace "Employee" with actual controller name
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                    return View("BankDetailsList", request); // Show the same form with error
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed to update: " + ex.Message;
                return View("BankDetailsList", request);
            }
        }


        public async Task<BankDetailsEntitie> GetBankDetailsById(int id)
        {
            BankDetailsEntitie obj = new BankDetailsEntitie();
            obj = _mService.GetBankDetailsById(id);
            return obj;
        }




        [HttpPost]
        public IActionResult DeleteBankDetails(int id)
        {
            var user = _context.bankDetailsEntitie.FirstOrDefault(x => x.BankId == id);
            if (user != null)
            {
                user.IsDeleted = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }



        #endregion

        #region Holidays

        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> HolidaysList(string searchTerm, int page = 1, int pageSize = 10)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            ViewBag.UserTypeList = new SelectList(_context.userTypeMasterEntitie, "UserTypeId", "UserTypeName");
            var today = DateTime.Today;
            var steps = _mService.HolidaysList()
                .OrderByDescending(x => x.HolidayDate < today) // Completed first
                .ThenBy(x => x.HolidayDate)                    // Then sort by date
                .ToList();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                steps = steps.Where(x =>
                    (x.HolidayName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = steps
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = steps.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)steps.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_HolidayListTable", paginatedLeaves);
            }



            return View(paginatedLeaves);
        }

        [Authorize(Policy = "EmployeeAccess")]
        public async Task<IActionResult> HolidaysLists()
        {
            var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
                return RedirectToAction("Login", "Authenticate");

            ViewBag.UserDetails = lr;
            ViewBag.UserTypeList = new SelectList(_context.userTypeMasterEntitie, "UserTypeId", "UserTypeName");

            var today = DateTime.Today;

            var steps = _mService.HolidaysList()
                .OrderByDescending(x => x.HolidayDate < today) // Completed first
                .ThenBy(x => x.HolidayDate)                    // Then sort by date
                .ToList();


            return View(steps);
        }


        [HttpPost]
        public IActionResult Holidays(HolidaysDTO request)
        {
            HolidaysEntite pp = new HolidaysEntite();
            GenericResponse res = new GenericResponse();
            try
            {
                if (request.HolidayId == 0)
                {

                    LoginResponseModel lr = new LoginResponseModel();

                    pp.HolidayDate = request.HolidayDate;
                    pp.HolidayName = request.HolidayName;
                    pp.HolidayDescription = request.HolidayDescription;
                    pp.UserTypeId = request.UserTypeId;
                    pp.CreatedOn = request.CreatedOn;
                    pp.CreatedBy = lr.userId;
                    pp.IsDeleted = false;
                    pp.IsActive = true;
                    res = _mService.CreateHoliday(pp);

                }

                if (res.statuCode == 1)
                {
                    return RedirectToAction("HolidaysList");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }


            }
            catch (Exception e)
            {
                // Log the exception (optional)
                ViewBag.ErrorMessage = "An error occurred while adding the user.";

            }

            return View();


        }



        public IActionResult EditHoliday(int holidayid)
        {
            HolidaysDTO um = new HolidaysDTO();
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (lr == null)
            {
                return RedirectToAction("Login");
            }

            var UserDeatils = _mService.GetHolidayById(holidayid);

            var usertypelist = _mService.UserTypeList().OrderBy(u => u.UserTypeName).ToList();
            var type = _context.userTypeMasterEntitie
                .Select(u => new { UserTypeId = u.UserTypeId, UserTypeName = u.UserTypeName })
                .ToList();
            ViewBag.Type = type;

            CloneObjects.CopyPropertiesTo(UserDeatils, um);


            return View(um);
        }



        [HttpPost]
        public IActionResult EditHoliday(HolidaysDTO model)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (ModelState.IsValid)
            {
                HolidaysEntite u = new HolidaysEntite();
                u = _mService.GetHolidayById(model.HolidayId);
                u.HolidayDate = model.HolidayDate;
                u.HolidayName = model.HolidayName;
                u.HolidayDescription = model.HolidayDescription;
                u.UserTypeId = model.UserTypeId;
                u.UpdatedOn = model.UpdatedOn;
                u.UpdatedBy = lr.userId;
                u.IsActive = true;
                u.IsDeleted = false;

                var res = _mService.UpdateHoliday(u);
                if (res.statuCode == 1)
                {
                    return RedirectToAction("HolidaysList");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }

            }

            return View(model);
        }



        public async Task<HolidaysEntite> GetHolidayById(int id)
        {
            HolidaysEntite obj = new HolidaysEntite();
            obj = _mService.GetHolidayById(id);
            return obj;
        }


        [HttpPost]
        public IActionResult DeleteHoliday(int id)
        {
            var holiday = _context.holidaysEntite.FirstOrDefault(x => x.HolidayId == id);
            if (holiday != null)
            {
                holiday.IsDeleted = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> HolidaysListEmployee()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            ViewBag.UserTypeList = new SelectList(_context.userTypeMasterEntitie, "UserTypeId", "UserTypeName");
            var steps = _mService.HolidaysList().ToList();
            return View(steps);
        }


        #endregion

        #region Payslip

        public async Task<IActionResult> IndexPayslip(string fromMonth, string toMonth)
        {
            var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            AutoGeneratePayslip();
            ViewBag.UserDetails = lr;

            var payslips = _mService.GetAllPayslipsForUser(lr.userId);

            if (!string.IsNullOrEmpty(fromMonth) && !string.IsNullOrEmpty(toMonth))
            {
                DateTime from = DateTime.Parse(fromMonth + "-21");
                DateTime to = DateTime.Parse(toMonth + "-20");

                payslips = payslips
                    .Where(p =>
                    {
                        DateTime parsed;
                        return DateTime.TryParse(p.SalaryMonth, out parsed) &&
                               parsed >= from && parsed <= to;
                    })
                    .ToList();
            }


            return View(payslips);
        }

        public void AutoGeneratePayslip()
        {
            var today = DateTime.Today;
            var users = _context.userMasterEntitie.Where(u => u.IsDeleted == false).ToList();

            foreach (var user in users)
            {
                if (!user.DateOfJoining.HasValue)
                    continue;

                var doj = user.DateOfJoining.Value;

                var firstSalaryMonthEnd = new DateTime(doj.Year, doj.Month, 20);
                if (doj.Day > 20)
                    firstSalaryMonthEnd = firstSalaryMonthEnd.AddMonths(1);

                var salaryEndPointer = new DateTime(today.Year, today.Month, 20);
                if (today.Day < 21)
                    salaryEndPointer = salaryEndPointer.AddMonths(-1);

                while (firstSalaryMonthEnd <= salaryEndPointer)
                {
                    string label = firstSalaryMonthEnd.ToString("MMMM-yyyy");

                    bool exists = _context.payslipEntitie.Any(p =>
                        p.UserId == user.UserId &&
                        p.IsDeleted == false &&
                        p.SalaryMonth == label);

                    if (!exists)
                    {
                        var fromDate = firstSalaryMonthEnd.AddMonths(-1).AddDays(1); // From 21st of the previous month
                        var toDate = firstSalaryMonthEnd; // To 20th of the current month

                        var salaryRecord = _context.salary.FirstOrDefault(s =>
                            s.UserId == user.UserId &&
                            s.CreditedOn >= fromDate &&
                            s.CreditedOn <= toDate);

                        if (salaryRecord != null)
                        {
                           
                            GeneratePayslip(user, firstSalaryMonthEnd, salaryRecord);
                        }
                    }

                    firstSalaryMonthEnd = firstSalaryMonthEnd.AddMonths(1); // Move to the next month
                }

                if (today.Day == 22)
                {
                    var currentSalaryMonthEnd = new DateTime(today.Year, today.Month, 20);
                    string currentLabel = currentSalaryMonthEnd.ToString("MMMM-yyyy");

                    // Check if a payslip exists for the current month
                    bool exists = _context.payslipEntitie.Any(p =>
                        p.UserId == user.UserId &&
                        p.IsDeleted == false &&
                        p.SalaryMonth == currentLabel);

                    if (!exists)
                    {
                        var fromDate = currentSalaryMonthEnd.AddMonths(-1).AddDays(1); // From 21st of the previous month
                        var toDate = currentSalaryMonthEnd; // To 20th of the current month

                        // Fetch salary credited in the specific month range
                        var salaryRecord = _context.salary.FirstOrDefault(s =>
                            s.UserId == user.UserId &&
                            s.CreditedOn >= fromDate &&
                            s.CreditedOn <= toDate);

                        if (salaryRecord != null)
                        {
                            // Generate payslip for the user if salary record exists
                            GeneratePayslip(user, currentSalaryMonthEnd, salaryRecord);
                        }
                    }
                }
            }
        }

        private void GeneratePayslip(UserMasterEntitie user, DateTime salaryPeriodEnd, SalarysEntity salary)
        {
            var payslip = new PayslipEntitie
            {
                UserId = user.UserId,
                SalaryMonth = salaryPeriodEnd.ToString("MMMM-yyyy"),
                BasicSalary = salary.BasicSalary ?? 0,
                HRA = salary.HRA ?? 0,
                SpecialAllowance = salary.SpecialAllowance,
                Conveyance = salary.Conveyance,
                PF = salary.PF,
                ProfessionalTax = salary.ProfessionalTax,
                Bonus = salary.Bonus ?? 0,
                Deductions = salary.Deductions ?? 0,
                NetSalary = (salary.BasicSalary ?? 0)
                            + (salary.HRA ?? 0)
                            + (salary.SpecialAllowance ?? 0)
                            + (salary.Conveyance ?? 0)
                            + (salary.Bonus ?? 0)
                            - (salary.PF ?? 0)
                            - (salary.ProfessionalTax ?? 0)
                            - (salary.Deductions ?? 0),
                CreatedOn = DateTime.Now,
                IsDeleted = false
            };

            _context.payslipEntitie.Add(payslip);
            _context.SaveChanges();
        }




        public async Task<IActionResult> PayslipView(int payslipId)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            ViewBag.UserType = lr.userTypeName;
            var steps = _mService.GetPayslipDetails(payslipId);
            return View(steps);
        }


        public async Task<IActionResult> PayslipViews()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            var steps = _mService.PayslipList().ToList();
            return View(steps);
        }

        [HttpPost]
        public GenericResponse Payslip(PayslipDTO request)
        {
            PayslipEntitie pp = new PayslipEntitie();
            GenericResponse res = new GenericResponse();
            try
            {
                if (request.PayslipId > 0)
                {

                    LoginResponseModel lr = new LoginResponseModel();

                    pp.UserId = request.PayslipId;
                    pp.SalaryMonth = request.SalaryMonth;
                    pp.BasicSalary = request.BasicSalary;
                    pp.HRA = request.HRA;
                    pp.TaxPaid = request.TaxPaid;
                    pp.NetSalary = request.NetSalary;
                    pp.GrossSalary = request.GrossSalary;
                    pp.Deductions = request.Deductions;
                    pp.Bonus = request.Bonus;

                    pp.IsDeleted = false;
                    res = _mService.CreatePayslip(pp);

                }
                else
                {



                    pp.UserId = request.PayslipId;
                    pp.SalaryMonth = request.SalaryMonth;
                    pp.BasicSalary = request.BasicSalary;
                    pp.HRA = request.HRA;
                    pp.TaxPaid = request.TaxPaid;
                    pp.NetSalary = request.NetSalary;
                    pp.GrossSalary = request.GrossSalary;
                    pp.Deductions = request.Deductions;
                    pp.Bonus = request.Bonus;
                    pp.IsDeleted = false;
                    res = _mService.UpdatePayslip(pp);
                }


            }
            catch (Exception ex)
            {
                res.message = "Failed to update : " + ex.Message;
                res.currentId = 0;
                return res;
            }

            return res;

        }

        public async Task<PayslipEntitie> GetPayslipById(int id)
        {
            PayslipEntitie obj = new PayslipEntitie();
            obj = _mService.GetPayslipById(id);
            return obj;
        }


        [HttpPost]
        public GenericResponse Deletepayslip(int idd)
        {
            GenericResponse res = new GenericResponse();
            if (idd > 0)
            {
                var check = _mService.GetPayslipById(idd);
                if (check != null)
                {
                    check.IsDeleted = true;
                    res = _mService.UpdatePayslip(check);

                }


            }
            return res;
        }
        #endregion


        #region Leave
        [Authorize(Policy = "AdminAccess")]

        public IActionResult LeaveList(string searchTerm, int page = 1, int pageSize = 10)
        {
            // Check user authentication
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set view bag details
            ViewBag.UserDetails = lr;
            ViewBag.Username = lr.userName;

            // Retrieve all leaves
            var allLeaves = _mService.LeaveList().ToList(); // Ensure it's a list to allow modifications

            foreach (var leave in allLeaves)
            {
                var leaveEntity = _context.leavesEntitie.FirstOrDefault(l => l.LeaveId == leave.LeaveId);
                if (leaveEntity != null)
                {
                    leaveEntity.ApprovedBy = lr.userId;
                }
            }
            _context.SaveChanges(); // Persist changes to the database

            // Apply comprehensive search across multiple fields
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                allLeaves = allLeaves.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.LeaveType ?? "").ToLower().Contains(searchTerm) ||
                    (x.Status ?? "").ToLower().Contains(searchTerm) ||
                    (x.Description ?? "").ToLower().Contains(searchTerm) ||
                    (x.FromDate.ToString("yyyy-MM-dd") ?? "").Contains(searchTerm) ||
                    (x.ToDate.ToString("yyyy-MM-dd") ?? "").Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = allLeaves
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = allLeaves.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)allLeaves.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_LeaveListTable", paginatedLeaves);
            }

            return View(paginatedLeaves);
        }

        [HttpPost]


        [HttpPost]
        public IActionResult UpdateLeaveStatus(int id, string status, string remarks)
        {
            var leave = _context.leavesEntitie.FirstOrDefault(l => l.LeaveId == id);
            if (leave == null)
            {
                return Json(new { success = false, message = "Leave request not found" });
            }

            leave.Status = status;
            leave.Remarks = (status == "Rejected") ? remarks : null; // Store remarks only if rejected
            _context.SaveChanges();

            return Json(new { success = true, message = $"Leave {status} successfully" });
        }

        public IActionResult ApplyLeave()
        {

            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            ViewBag.username = lr.userName;
            IEnumerable<LeaveTypeDTO> leaveTypes = _lService.GetAllLeaveTypes();


            ViewBag.leavetypelist = new SelectList(leaveTypes, "LeaveTypeId", "LeaveType");

            LeavesDTO um = new LeavesDTO();
            return View(um);


        }

        [HttpPost]
        public JsonResult Leave(LeavesDTO request)
        {
            LeavesEntitie pp = new LeavesEntitie();
            GenericResponse res = new GenericResponse();
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            try
            {
                if (request.LeaveId == 0)
                {
                    pp.Description = request.Description;
                    pp.UserId = lr.userId;
                    pp.UserTypeId = _context.userTypeMasterEntitie
                        .Where(u => u.UserTypeName == lr.userTypeName)
                        .Select(u => u.UserTypeId)
                        .FirstOrDefault();
                    pp.FromDate = request.FromDate;
                    pp.LeaveTypeId = request.LeaveTypeId;
                    pp.ToDate = request.ToDate;
                    pp.ApprovedBy = request.ApprovedBy;
                    pp.IsDeleted = false;
                    pp.IsActive = true;
                    pp.CreatedOn = DateTime.Now;
                    pp.CreatedBy = lr.userId;

                    // Create leave
                    res = _mService.CreateLeave(pp);
                    int leaveid = _context.leavesEntitie
                        .Where(l => l.UserId == lr.userId && l.FromDate == request.FromDate && l.ToDate == request.ToDate && l.LeaveTypeId == request.LeaveTypeId && l.Status == null)
                        .Select(l => l.LeaveId)
                        .FirstOrDefault();
                    SendLeaveNotificationToAdmin(pp, leaveid);

                    if (res.currentId != 0)
                    {
                        return Json(new { success = true, message = "Leave request sent to reporting manager!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create leave request." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }

            return Json(new { success = false, message = "Unexpected error occurred." });
        }



        public void SendLeaveNotificationToAdmin(LeavesEntitie leave, int leaveid)
        {
            var username = _context.userMasterEntitie.Where(u => u.UserId == leave.UserId).Select(u => u.UserName).FirstOrDefault();
            var notification = new NotificationEntity
            {
                UserId = leave.UserId,
                Message = $"Leave request submitted by {username} from {leave.FromDate.ToString()} to {leave.ToDate.ToString()}",
                CreatedOn = DateTime.Now,
                IsRead = false,
                leaveid = leaveid
            };

            _context.notificationEntity.Add(notification);
            _context.SaveChanges();
        }


        [HttpPost]
        public IActionResult RejectLeave(int id, string remarks)
        {
            try
            {
                LoginResponseModel lr = new LoginResponseModel();
                lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
                // Find the leave request by ID
                var leave = _context.leavesEntitie.FirstOrDefault(l => l.LeaveId == id);
                if (leave == null)
                {
                    return Json(new { success = false, message = "Leave not found." });
                }
                var notification = _context.notificationEntity.Where(a => a.UserId == leave.UserId && a.IsRead == false).FirstOrDefault();

                // Update the leave status to "Rejected" and add remarks
                leave.Status = "Rejected";  // Assuming you have a "Status" field
                leave.Remarks = remarks;
                leave.ApprovedBy = lr.userId;

                // Save changes to the database
                _context.SaveChanges();
                notification.IsRead = true;
                _context.notificationEntity.Update(notification);
                _context.SaveChanges();

                return Json(new { success = true, message = "Leave rejected successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        public IActionResult EditLeave(int leaveid)
        {
            LeavesDTO um = new LeavesDTO();
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (lr == null)
            {
                return RedirectToAction("Login");
            }

            var leaves = _mService.GetLeaveById(leaveid);



            CloneObjects.CopyPropertiesTo(leaves, um);


            return View(um);
        }



        [HttpPost]
        public IActionResult EditLeave(LeavesDTO model)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (ModelState.IsValid)
            {
                LeavesEntitie u = new LeavesEntitie();
                u = _mService.GetLeaveById(model.LeaveId);
                u.Description = model.Description;
                u.FromDate = model.FromDate;
                u.ToDate = model.ToDate;
                u.UserTypeId = model.UserTypeId;

                u.IsActive = true;
                u.IsDeleted = false;

                var res = _mService.UpdateLeave(u);
                if (res.statuCode == 1)
                {
                    return RedirectToAction("LeaveList");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }

            }

            return View(model);
        }

        public async Task<LeavesEntitie> GetLeaveById(int id)
        {
            LeavesEntitie obj = new LeavesEntitie();
            obj = _mService.GetLeaveById(id);
            return obj;
        }


        [HttpPost]
        public GenericResponse DeleteLeave(int id)
        {
            GenericResponse res = new GenericResponse();
            if (id > 0)
            {
                var check = _mService.GetLeaveById(id);
                if (check != null)
                {
                    check.IsDeleted = true;
                    res = _mService.UpdateLeave(check);

                }


            }
            return res;
        }
        #endregion

        #region ModulePermissions

        [HttpPost]
        public async Task<IActionResult> SetUserPermissions([FromBody] List<ModulePermissionsDTO> permissions)
        {
            if (permissions == null || !permissions.Any())
                return BadRequest(new { Message = "No permissions data received." });

            try
            {
                foreach (var permission in permissions)
                {
                    await _mService.SavePermissionAsync(permission);
                }

                return Ok(new { Message = "Permissions updated successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine(ex.Message);
                return StatusCode(500, new { Message = "An error occurred while updating permissions." });
            }
        }

        #endregion


        #region LeaveType
        [HttpGet]
        public async Task<IActionResult> GetAllLeaveTypes()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            ViewBag.UserTypeList = new SelectList(_context.userTypeMasterEntitie, "UserTypeId", "UserTypeName");
            var steps = _lService.GetAllLeaveTypes().ToList();
            return View(steps);
        }
        public async Task<LeavetypeEntity> GetLeaveTypeById(int leaveTypeId)
        {
            LeavetypeEntity obj = new LeavetypeEntity();
            obj = _lService.GetLeaveTypeById(leaveTypeId);
            return obj;
        }

        [HttpPost]
        public IActionResult AddLeave(LeaveTypeDTO request)
        {
            GenericResponse res = new GenericResponse();
            LoginResponseModel lr = new LoginResponseModel();
            try
            {
                if (request.LeaveTypeId == 0 || request.LeaveTypeId == null)
                {

                    res = _lService.AddLeave(request);

                }

                if (res.statuCode == 1)
                {
                    return RedirectToAction("GetAllLeaveTypes");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }


            }
            catch (Exception e)
            {
                // Log the exception (optional)
                ViewBag.ErrorMessage = "An error occurred while adding the user.";

            }

            return View();


        }



        public IActionResult EditLeaveType(int leaveTypeId)
        {
            LeaveTypeDTO um = new LeaveTypeDTO();
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (lr == null)
            {
                return RedirectToAction("Login");
            }

            var LeaveDeatils = _lService.GetLeaveTypeById(leaveTypeId);

            var usertypelist = _mService.UserTypeList().OrderBy(u => u.UserTypeName).ToList();
            var type = _context.userTypeMasterEntitie
                .Select(u => new { UserTypeId = u.UserTypeId, UserTypeName = u.UserTypeName })
                .ToList();
            ViewBag.Type = type;

            CloneObjects.CopyPropertiesTo(LeaveDeatils, um);


            return View(um);
        }



        [HttpPost]
        public IActionResult EditLeaveType(LeaveTypeDTO model)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (ModelState.IsValid)
            {
                var res = _lService.UpdateLeave(model);
                if (res.statuCode == 1)
                {
                    return RedirectToAction("GetAllLeaveTypes");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }

            }

            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteLeaveType(int leaveTypeId)
        {
            if (leaveTypeId == 0)
            {
                return BadRequest("Invalid Leave Type ID.");
            }

            var res = _lService.DeleteLeave(leaveTypeId);

            if (res.statuCode == 1)
            {
                return RedirectToAction("GetAllLeaveTypes");
            }
            else
            {
                ViewBag.ErrorMessage = res.message;
                return View();
            }
        }

        #endregion



        #region SalaryStructure
        public async Task<IActionResult> SalaryList()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            var Usertype = _context.userTypeMasterEntitie.Where(a => a.IsDeleted == false).Select(a => a.UserTypeName).ToList();
            ViewBag.UserType = Usertype;
            ViewBag.UserDetails = lr;
            var steps = _mService.SalaryList().ToList();
            return View(steps);
        }

        [HttpPost]
        public IActionResult SalaryStructure(SalaryStructureDTO request)
        {
            SalaryStructureEntity pp = new SalaryStructureEntity();
            GenericResponse res = new GenericResponse();
            try
            {
                if (request.SalaryStructureId == 0)
                {

                    LoginResponseModel lr = new LoginResponseModel();

                    pp.SalaryStructureId = request.SalaryStructureId;
                    pp.UserId = request.UserId;
                    pp.BasicSalary = request.BasicSalary;
                    pp.HRA = request.HRA;
                    pp.SpecialAllowance = request.SpecialAllowance;
                    pp.Conveyance = request.Conveyance;
                    pp.PF_Employee = request.PF_Employee;
                    pp.ProfessionalTax = request.ProfessionalTax;
                    pp.IsDeleted = false;
                    pp.IsActive = true;
                    res = _mService.CreateSalaryStructure(pp);

                }

                if (res.statuCode == 1)
                {
                    return RedirectToAction("SalaryList");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }


            }
            catch (Exception e)
            {
                // Log the exception (optional)
                ViewBag.ErrorMessage = "An error occurred while adding the user.";

            }

            return View();


        }

        public IActionResult EditSalaryStructure(int id)
        {
            SalaryStructureDTO um = new SalaryStructureDTO();
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (lr == null)
            {
                return RedirectToAction("Login");
            }

            return View(um);
        }



        [HttpPost]
        public IActionResult EditSalaryStructure(SalaryStructureDTO request)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;
            int id = _context.userMasterEntitie.Where(a => a.UserName == request.UserName).Select(a => a.UserId).FirstOrDefault();
            if (ModelState.IsValid)
            {
                SalaryStructureEntity pp = new SalaryStructureEntity();
                pp = _mService.GetSalaryStructureById(request.SalaryStructureId);
                pp.SalaryStructureId = request.SalaryStructureId;
                pp.UserId = id;
                pp.BasicSalary = request.BasicSalary;
                pp.HRA = request.HRA;
                pp.SpecialAllowance = request.SpecialAllowance;
                pp.Conveyance = request.Conveyance;
                pp.PF_Employee = request.PF_Employee;
                pp.ProfessionalTax = request.ProfessionalTax;
                pp.IsActive = true;
                pp.IsDeleted = false;

                var res = _mService.UpdateSalaryStructure(pp);
                if (res.statuCode == 1)
                {
                    return RedirectToAction("HolidaysList");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }

            }

            return View(request);
        }

        public async Task<SalaryStructureEntity> GetSalaryStructureById(int id)
        {
            SalaryStructureEntity obj = new SalaryStructureEntity();
            obj = _mService.GetSalaryStructureById(id);
            return obj;
        }


        [HttpPost]
        public GenericResponse DeleteSalaryStructure(int idd)
        {
            GenericResponse res = new GenericResponse();
            if (idd > 0)
            {
                var check = _mService.GetSalaryStructureById(idd);
                if (check != null)
                {
                    check.IsDeleted = true;
                    res = _mService.UpdateSalaryStructure(check);

                }


            }
            return res;
        }
        [HttpGet]
        public JsonResult GetUsersByType(string userType)
        {
            int id = _context.userTypeMasterEntitie
                             .Where(a => a.UserTypeName == userType && a.IsDeleted == false)
                             .Select(a => a.UserTypeId)
                             .FirstOrDefault();

            var users = _context.userMasterEntitie
                                .Where(a => a.UserTypeId == id)
                                .Select(u => new { UserId = u.UserId, UserName = u.UserName })
                                .ToList();

            return Json(users);
        }
        private double GetPresentDays(int? userId = null)
        {
            DateTime currentDate = DateTime.Now;

            DateTime startDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-1).AddDays(20); // 21st of the previous month

            DateTime endDate = new DateTime(currentDate.Year, currentDate.Month, 20); // 20th of the current month

            var usersQuery = from user in _context.userMasterEntitie
                             join dept in _context.department on user.DepartmentId equals dept.DepartmentID
                             where user.IsDeleted == false
                             select user;

            if (userId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.UserId == userId.Value);
            }

            var users = usersQuery.ToList();

            var attendanceRecords = _context.attendanceEntitie
                .Where(a => a.CreatedOn.HasValue &&
                            a.CreatedOn.Value.Date >= startDate.Date &&
                            a.CreatedOn.Value.Date <= endDate.Date &&
                            a.IsDeleted == false)
                .ToList();

            var leaves = _context.leavesEntitie?
                .AsEnumerable()
                .Where(l => l.FromDate.ToDateTime(TimeOnly.MinValue) <= endDate.Date &&
                            l.ToDate.ToDateTime(TimeOnly.MinValue) >= startDate.Date &&
                            l.IsDeleted == false)
                .ToList() ?? new List<LeavesEntitie>();

            var leaveTypes = _context.leaveType?.ToList() ?? new List<LeavetypeEntity>();

            var holidays = _context.holidaysEntite?
                .Where(h => h.HolidayDate >= startDate && h.HolidayDate <= endDate && h.IsDeleted == false)
                .Select(h => h.HolidayDate)
                .ToList() ?? new List<DateTime?>();

            double totalPresentDays = 0;

            foreach (var user in users)
            {
                int presentDays = 0;
                double halfDays = 0;
                double sickLeave = 0;
                double earnedLeaves = 0;
                int holidaysCount = 0;
                int casualLeave = 0;
                int weekOff = 0;
                int absent = 0;
                int lateArrivals = 0;
                Dictionary<int, string> dailyStatus = new();

                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var attendance = attendanceRecords.FirstOrDefault(a => a.UserId == user.UserId && a.CreatedOn.Value.Date == date.Date && a.Status == "Present");
                    var leave = leaves.FirstOrDefault(l => l.UserId == user.UserId &&
                                                           l.FromDate.ToDateTime(TimeOnly.MinValue) <= date.Date &&
                                                           l.ToDate.ToDateTime(TimeOnly.MinValue) >= date.Date);

                    string leaveType = leave != null ? leaveTypes.FirstOrDefault(lt => lt.LeaveTypeId == leave.LeaveTypeId)?.LeaveType ?? "On Leave" : "";
                    bool isHoliday = holidays.Any(h => h.GetValueOrDefault().Date == date.Date);
                    bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

                    string status = "Absent";

                    if (attendance != null)
                    {
                        status = "P";
                        presentDays++;

                        if (attendance.GracePeriodTime > 20)
                        {
                            lateArrivals++;
                            if (lateArrivals == 2)
                            {
                                halfDays += 0.5; 
                                presentDays--;
                            }
                        }
                    }
                    else if (leave != null)
                    {
                        status = leaveType switch
                        {
                            "Casual Leave" => "CL",
                            "Sick Leave" => "SL",
                            "Annual Leave" => "AL",
                            "Paid Leave" => "PL",
                            _ => "L"
                        };

                        if (leaveType == "Casual Leave") casualLeave++;
                        else if (leaveType == "Sick Leave") sickLeave++;
                        else if (leaveType == "Annual Leave") earnedLeaves++;
                    }
                    else if (isHoliday)
                    {
                        status = "H";
                        holidaysCount++;
                    }
                    else if (isWeekend)
                    {
                        status = "WO";
                        weekOff++;
                    }
                    // Default to absent
                    else
                    {
                        absent++;
                    }

                    dailyStatus[date.Day] = status;
                }

                foreach (DateTime date in EachDay(startDate, endDate))
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        DateTime friday = date.AddDays(-1);
                        DateTime monday = date.AddDays(1);
                        if ((dailyStatus.ContainsKey(friday.Day) && dailyStatus[friday.Day] == "P") ||
                            (dailyStatus.ContainsKey(monday.Day) && dailyStatus[monday.Day] == "P"))
                        {
                            presentDays++;
                        }
                    }
                }

                // Final paid days calculation
                double finalPaidDays = presentDays + (halfDays * 0.5) + earnedLeaves + holidaysCount+casualLeave;

                // Add the result for this user to the total
                totalPresentDays += finalPaidDays;
            }

            // Return the total paid days (finalPaidDays)
            return totalPresentDays;
        }
        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day <= thru.Date; day = day.AddDays(1))
                yield return day;
        }


        public IActionResult DownloadTemplate(string userType)
        {
            int id = _context.userTypeMasterEntitie
                .Where(a => a.IsDeleted == false && a.UserTypeName == userType)
                .Select(a => a.UserTypeId)
                .FirstOrDefault();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SalaryData");

                worksheet.Cell(1, 1).Value = "User ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Basic Salary";
                worksheet.Cell(1, 4).Value = "HRA";
                worksheet.Cell(1, 5).Value = "Special Allowance";
                worksheet.Cell(1, 6).Value = "Conveyance";
                worksheet.Cell(1, 7).Value = "PF (Employee)";
                worksheet.Cell(1, 8).Value = "Professional Tax";
                worksheet.Cell(1, 9).Value = "UserType";
                worksheet.Cell(1, 10).Value = "Present Days";
                worksheet.Cell(1, 11).Value = "Bonus";
                worksheet.Cell(1, 12).Value = "Deductions";

                var employees = _context.userMasterEntitie
                    .Where(u => u.UserTypeId == id)
                    .Select(u => new
                    {
                        u.UserId,
                        u.UserName,
                        Salary = _context.salaryStructure.Where(a => a.UserId == u.UserId).Select(a => a.BasicSalary).FirstOrDefault(),
                    })
                    .ToList();

                int currentRow = 2;
                foreach (var emp in employees)
                {
                    worksheet.Cell(currentRow, 1).Value = emp.UserId;
                    worksheet.Cell(currentRow, 2).Value = emp.UserName;
                    worksheet.Cell(currentRow, 9).Value = id;
                    double presentDays = GetPresentDays(emp.UserId);
                    worksheet.Cell(currentRow, 10).Value = presentDays; // Default number of days worked


                    currentRow++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"SalaryTemplate_{userType}.xlsx"
                    );
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile excelFile)
        {
            if (excelFile != null && excelFile.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

                        foreach (var row in rows)
                        {
                            try
                            {
                                // Validate UserTypeId (Column B)
                                if (row.Cell(9).IsEmpty() || !row.Cell(9).TryGetValue(out int userTypeId))
                                {
                                    // Handle invalid UserTypeId
                                    Console.WriteLine($"Invalid UserTypeId in row {row.RowNumber()}");
                                    continue; // Skip this row
                                }

                                // Validate UserId (Column A)
                                if (row.Cell(1).IsEmpty() || !row.Cell(1).TryGetValue(out int userId))
                                {
                                    Console.WriteLine($"Invalid UserId in row {row.RowNumber()}");
                                    continue;
                                }

                                // Process decimal values safely
                                decimal? basicSalary = row.Cell(3).GetValue<decimal?>();
                                decimal? hra = row.Cell(4).GetValue<decimal?>();
                                decimal? specialAllowance = row.Cell(5).GetValue<decimal?>();
                                decimal? conveyance = row.Cell(6).GetValue<decimal?>();
                                decimal? pf = row.Cell(7).GetValue<decimal?>();
                                decimal? professionalTax = row.Cell(8).GetValue<decimal?>();
                                decimal? bonus = row.Cell(12).GetValue<decimal?>();
                                decimal? deductions = row.Cell(13).GetValue<decimal?>();

                                // Validate DateTime
                                if (!row.Cell(11).TryGetValue(out DateTime creditedOn))
                                {
                                    Console.WriteLine($"Invalid date in row {row.RowNumber()}");
                                    continue;
                                }



                                var entry = new SalarysEntity
                                {
                                    UserId = userId,
                                    UserTypeId = userTypeId,
                                    BasicSalary = basicSalary,
                                    HRA = hra,
                                    SpecialAllowance = specialAllowance,
                                    Conveyance = conveyance,
                                    PF = pf,
                                    ProfessionalTax = professionalTax,
                                    Bonus = bonus,
                                    Deductions = deductions,
                                    CreditedOn = DateTime.Now
                                };

                                _context.salary.Add(entry);

                                await _context.SaveChangesAsync();
                            }

                        
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing row {row.RowNumber()}: {ex.Message}");
                                continue;
                            }
                        }
                    }
                }

                TempData["SuccessMessage"] = "Excel file uploaded and data saved successfully!";
            }

            return RedirectToAction("SalaryList");
        }


        #endregion
        [Authorize(Policy = "Employee")]
        public async Task<IActionResult> LeaveListindividually(string searchTerm, int page = 1, int pageSize = 10)
        {

            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            ViewBag.username = lr.userName;

            var steps = _mService.LeaveListindividually(lr.userId).ToList();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                steps = steps.Where(x =>
                  (x.LeaveType ?? "").ToLower().Contains(searchTerm) ||
                    (x.Description ?? "").ToLower().Contains(searchTerm) ||
                    (x.Remarks ?? "").ToLower().Contains(searchTerm) ||
                    (x.Status ?? "").ToLower().Contains(searchTerm) ||
                    (x.Approve ?? "").ToLower().Contains(searchTerm) ||
                    (x.UserName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = steps
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = steps.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)steps.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Leave", paginatedLeaves);
            }

            return View(paginatedLeaves);

        }
    }
}
