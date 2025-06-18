using AttendanceCRM.BAL.IServices;
using AttendanceCRM.Models;
using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;







namespace AttendanceCRM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly MyDbContext _context;
		private readonly IUserService _service;
        private readonly IMasterMgmtService _mService;
        private readonly ILeaveTypeMaster _TService;
        
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;


        public HomeController(ILogger<HomeController> logger, MyDbContext context, IUserService service,IMasterMgmtService mService, ILeaveTypeMaster TService, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
			_context = context;
			_service = service;
			_mService = mService;
            _TService = TService;
            _hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Policy = "EmployeeAccess")]
        public async Task<IActionResult> Index(int? year)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;
            var today = DateTime.UtcNow.Date;

            var punch = _context.attendanceEntitie
                         .FirstOrDefault(p => p.UserId == lr.userId && p.CreatedOn == today);

            ViewBag.PunchInTime = punch?.PunchInTime;
            ViewBag.PunchOutTime = punch?.PunchOutTime;
            // Fetch user details
            var user = await _context.userMasterEntitie
                .Where(u => u.UserId == lr.userId)
                .FirstOrDefaultAsync();

            // Fetch attendance
            var attendance = await _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId)
                .OrderByDescending(a => a.AttendanceId)
                .FirstOrDefaultAsync();

            var totalHours = TimeSpan.Zero;
            if (attendance?.PunchInTime != null && attendance?.PunchOutTime != null)
            {
                totalHours = attendance.PunchOutTime.Value - attendance.PunchInTime.Value;
            }

            int selectedYear = year ?? DateTime.Now.Year;

            // Fetch Leave Details
            var totalLeaves = await _context.leavesEntitie
        .Where(l => l.UserId == lr.userId && l.FromDate.Year == selectedYear)
        .CountAsync();

            var takenLeaves = await _context.leavesEntitie
                .Where(l => l.UserId == lr.userId && l.Status == "Accepted" && l.FromDate.Year == selectedYear)
                .CountAsync();

            var absentLeaves = await _context.leavesEntitie
                .Where(l => l.UserId == lr.userId && l.Status == "Rejected" && l.FromDate.Year == selectedYear)
                .CountAsync();

            var requestedLeaves = await _context.leavesEntitie
                .Where(l => l.UserId == lr.userId && l.Status == null && l.FromDate.Year == selectedYear)
                .CountAsync();

            var lossOfPay = await _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId
                    && a.PunchInTime == null
                    && !_context.leavesEntitie.Any(l => l.UserId == lr.userId
                        && l.FromDate <= DateOnly.FromDateTime(a.CreatedOn.Value)
                        && l.ToDate >= DateOnly.FromDateTime(a.CreatedOn.Value)
                        && l.Status == "Accepted"
                        && l.FromDate.Year == selectedYear))
                .CountAsync();

            var totalWorkedDays = await _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId && a.PunchInTime.HasValue && a.CreatedOn.Value.Year == selectedYear)
                .CountAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    TotalLeaves = totalLeaves,
                    Taken = takenLeaves,
                    Absent = absentLeaves,
                    Requests = requestedLeaves,
                    WorkedDays = totalWorkedDays,
                    LossOfPay = lossOfPay
                });
            }

            var model = new AttendanceViewModel
            {
                UserId = lr.userId,
                UserName = user?.UserName,
                IsPunchedIn = attendance != null && attendance.PunchOutTime == null,
                PunchInTime = attendance?.PunchInTime,
                PunchOutTime = attendance?.PunchOutTime,
                TotalWorkHours = $"{(int)totalHours.TotalHours:D2}:{totalHours.Minutes:D2}:{totalHours.Seconds:D2}",
                Designation = user?.Designation,
                MobileNumber = user?.MobileNumber,
                Email = user?.Email,
                DateOfJoining = user?.DateOfJoining,
                EmployeeId = user?.EmployeeId,
                ProfilePicture = user?.ProfilePicture,
                TotalLeaves=totalLeaves,
                Taken = takenLeaves,
                Absent = absentLeaves,
                Requests = requestedLeaves,
                WorkedDays = totalWorkedDays,
                LossOfPay = lossOfPay
            };
            ViewBag.SelectedYear = selectedYear;

            // Calculate today's attendance hours
         
            var todayAttendance = _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId && a.PunchInTime.Value.Date == today)
                .ToList();

            var totalWorkHoursToday = todayAttendance
                .Where(a => a.PunchOutTime.HasValue)
                .Select(a => (a.PunchOutTime.Value - a.PunchInTime.Value).TotalHours)
                .Sum();

            // Calculate weekly attendance hours
            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var weekAttendance = _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId && a.PunchInTime.Value.Date >= weekStart)
                .ToList();

            var totalWorkHoursWeek = weekAttendance
                .Where(a => a.PunchOutTime.HasValue)
                .Select(a => (a.PunchOutTime.Value - a.PunchInTime.Value).TotalHours)
                .Sum();

            ViewBag.TotalWorkHoursToday = totalWorkHoursToday;
            ViewBag.TotalWorkHoursWeek = totalWorkHoursWeek;

            var nextHoliday = await _context.holidaysEntite
                .Where(h => h.HolidayDate >= DateTime.UtcNow.Date && h.IsDeleted==false)
                .OrderBy(h => h.HolidayDate)
                .FirstOrDefaultAsync();

            ViewBag.NextHolidayName = nextHoliday?.HolidayName ?? "No upcoming holidays";
            ViewBag.NextHolidayDate = nextHoliday?.HolidayDate?.ToString("dd MMM yyyy") ?? string.Empty;

            // Fetch all birthdays for the current month
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var monthlyBirthdays = await _context.userMasterEntitie
                .Where(u => u.DateOfBirth.HasValue
                            && u.DateOfBirth.Value.Month == today.Month
                            && u.DateOfBirth.Value.Day == today.Day
                            && u.IsDeleted == false)
                .ToListAsync(); // Fetch only today's birthdays

            // Transform birthday data
            var monthlyBirthdayViewModels = monthlyBirthdays
                .Select(u => new EmployeeBirthdayViewModel
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Designation = u.Designation,
                    ProfilePicture = u.ProfilePicture,
                    BirthdayDate = new DateTime(currentYear, u.DateOfBirth.Value.Month, u.DateOfBirth.Value.Day)
                })
                .OrderBy(u => u.BirthdayDate)
                .ToList();

            ViewBag.MonthlyBirthdays = monthlyBirthdayViewModels;

            var wishes = _context.BirthdayWishes
        .Where(w => w.ReceiverId == lr.userId)
        .OrderByDescending(w => w.CreatedOn)
        .ToList();

            ViewBag.BirthdayWishes = wishes;


            return View(model);
        }


        [HttpPost]
        public IActionResult SendWishes(int userId)
        {
            // Retrieve logged-in user session
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate"); // Redirect to login if user is not logged in
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;

            // Ensure userId is valid
            var receiver = _context.userMasterEntitie.FirstOrDefault(u => u.UserId == userId);
            if (receiver == null)
            {
                TempData["BirthdayWishMessage"] = "Invalid user.";
                return RedirectToAction("Index");
            }

            // Create the wish message
            string message = $"Happy Birthday, {receiver.UserName}! Best wishes from {lr.userName}.";

            // Save the wish in the database
            BirthdayWishesEntity wish = new BirthdayWishesEntity
            {
                SenderId = lr.userId,
                ReceiverId = userId,
                Message = message,
                CreatedOn = DateTime.Now
            };

            _context.BirthdayWishes.Add(wish);
            _context.SaveChanges();

            TempData["BirthdayWishMessage"] = "Wish sent successfully!"; // Feedback message

            return RedirectToAction("Index"); // Redirect back to Birthday Wishes section
        }


        public IActionResult GetNotifications()
        {
            // Get logged-in user session
            var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            var today = DateTime.Today;

            // Fetch today's birthday wishes for the logged-in user
            var wishes = _context.BirthdayWishes
                        .Where(w => w.ReceiverId == lr.userId && w.CreatedOn.HasValue &&
                                    w.CreatedOn.Value.Date == today) // Filter only today's wishes
                        .OrderByDescending(w => w.CreatedOn)
                        .Select(w => new
                        {
                            message = w.Message,
                            createdOn = w.CreatedOn.HasValue
                                ? w.CreatedOn.Value.ToString("ddd, MMM dd yyyy hh:mm tt")
                                : "N/A" // Handle null case
                        })
                        .ToList();

            return Json(new { success = true, wishes });
        }



        [HttpPost]
        public IActionResult AutoMarkAbsent()
        {
            var today = DateTime.Today;
            var userId = GetCurrentUserId(); 

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { message = "User not authenticated." });
            }

            int currentUserId = int.Parse(GetCurrentUserId());  

            var attendance = _context.attendanceEntitie
                .FirstOrDefault(a => a.UserId == currentUserId && a.CreatedOn == today);


            if (attendance == null)
            {
                _context.attendanceEntitie.Add(new AttendanceEntitie
                {
                    UserId = currentUserId,
                    CreatedOn = today,
                    Status = "Absent",
                    //UpdatedOn = DateTime.Now
                });

                _context.SaveChanges();
            }

            return Json(new { message = "Absent status updated." });
        }


        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);

        }

       
         
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


		
		public IActionResult UserList()
		{
			return View();
		}


        [Authorize(Policy = "AdminAccess")]
        public IActionResult GetAllEmployeesAttendance(string searchTerm, int page = 1, int pageSize = 10)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.userid = lr.userId;

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                HttpContext.Session.SetInt32("UserId", lr.userId);
            }

            var users = _context.userMasterEntitie
                .Where(u => u.UserTypeId == 3)
                .Select(u => new { u.UserId, u.UserName })
                .ToList();
            ViewBag.Users = new SelectList(users, "UserId", "UserName");

            // Fetch attendance records for all employees
            var allAttendanceRecords = GetAllEmployeesAttendanceRecords();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                allAttendanceRecords = allAttendanceRecords.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = allAttendanceRecords
       .OrderByDescending(x => x.CreatedOn)
       .Skip((page - 1) * pageSize)
       .Take(pageSize)
       .ToList();


            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = allAttendanceRecords.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)allAttendanceRecords.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_AttendanceList", paginatedLeaves);
            }
            if (paginatedLeaves == null || !paginatedLeaves.Any())
            {
                ViewBag.Message = "No attendance records found.";
            }

            return View(paginatedLeaves);
        }

        [HttpGet]
       
        public IActionResult GetCalendarAttendance()
        {
            int userId = int.Parse(User.FindFirstValue("UserID"));
            var attendanceList = GetAttendanceByUserId(userId);
            var events = new List<object>();

            foreach (var record in attendanceList)
            {
                string title;
                string color;

                DateTime date = record.PunchInTime?.Date ?? DateTime.Today;

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    title = "Weekly Off";
                    color = "#808080"; // Gray
                }
                else if (record.PunchInTime == null)
                {
                    title = "Absent";
                    color = "#ff4d4d"; // Red
                }
                else if (string.IsNullOrEmpty(record.PunchOutSelfiePath))
                {
                    title = "Missed PunchOut";
                    color = "#a0a0a0"; // Grey
                }
                else if (record.WorkType == "WFO")
                {
                    double totalHours = record.TotalHours ?? 0;
                    if (totalHours < 9)
                    {
                        title = $"TimeSpent: {totalHours:0.00} hrs - WFO (Under Hours)";
                        color = "#ffcc00"; // Yellow
                    }
                    else
                    {
                        title = $"TimeSpent: {totalHours:0.00} hrs - WFO";
                        color = "#28a745"; // Green
                    }
                }
                else
                {
                    title = $"TimeSpent: {record.TotalHours ?? 0:0.00} hrs - {record.WorkType}";
                    color = "#007bff"; // Blue
                }

                events.Add(new
                {
                    title = title,
                    start = date.ToString("yyyy-MM-dd"),
                    color = color
                });
            }

            return Json(events);
        }

        public List<AttendanceViewModel> GetAllEmployeesAttendanceRecords(string searchTerm = null)
        {
           var attendanceData = (
    from a in _context.attendanceEntitie
    join u in _context.userMasterEntitie on a.UserId equals u.UserId
    where a.UserId != null
    group a by new { a.UserId, u.UserName, u.Designation,u.Email } into g
    select new AttendanceViewModel
    {
        UserId = g.Key.UserId.Value,
        UserName = g.Key.UserName,
        Designation = g.Key.Designation,
        Email=g.Key.Email,
       
        
    }).Distinct().ToList();

            return attendanceData.OrderByDescending(x => x.PunchInTime).ToList();


            //var query = (from a in _context.attendanceEntitie
            //             join u in _context.userMasterEntitie on a.UserId equals u.UserId
            //             orderby a.CreatedOn descending
            //             select new AttendanceViewModel
            //             {
            //                 UserId = (int)a.UserId,
            //                 UserName = u.UserName,
            //                 CreatedOn = a.CreatedOn,
            //                 PunchInTime = a.PunchInTime,
            //                 PunchOutTime = a.PunchOutTime,
            //                 GracePeriodTime = a.GracePeriodTime,
            //                 Designation = u.Designation
            //             }).ToList();


            //if (!string.IsNullOrEmpty(searchTerm))
            //{
            //    searchTerm = searchTerm.ToLower();
            //    query = query.Where(x =>
            //        (x.UserName ?? "").ToLower().Contains(searchTerm)
            //    ).ToList();
            //}

            //return query.OrderByDescending(x => x.CreatedOn).ToList();
        }




        public ActionResult FilterAttendance(string filterType, string startDate, string endDate, int? userId)
            {
            //DateTime today = DateTime.Today;
            //DateTime start = DateTime.MinValue;
            //DateTime end = DateTime.MaxValue;

            //// Determine the date range based on the selected filter
            //switch (filterType)
            //{
            //    case "Daily":
            //        start = today;
            //        end = today.AddDays(1).AddSeconds(-1);
            //        break;
            //    case "Weekly":
            //        start = today.AddDays(-(int)today.DayOfWeek); // Start of the week (Sunday)
            //        end = start.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59); // End of the week (Saturday)
            //        break;
            //    case "Monthly":
            //        start = new DateTime(today.Year, today.Month, 1); // First day of the month
            //        end = start.AddMonths(1).AddSeconds(-1); // Last day of the month
            //        break;
            //    case "Custom":
            //        if (DateTime.TryParse(startDate, out DateTime parsedStart) &&
            //            DateTime.TryParse(endDate, out DateTime parsedEnd))
            //        {
            //            start = parsedStart;
            //            end = parsedEnd.AddHours(23).AddMinutes(59).AddSeconds(59); // Include full day
            //        }
            //        break;
            //}

            //// Query attendance records based on the selected filter
            //var query = _context.attendanceEntitie
            // .Join(_context.userMasterEntitie, a => a.UserId, u => u.UserId, (a, u) => new { a, u })
            // .Where(x => x.a.PunchInTime >= start && x.a.PunchInTime <= end);

            //// Apply filter before materializing the list
            //if (userId.HasValue && userId > 0)
            //{
            //    query = query.Where(x => x.a.UserId == userId.Value);
            //}

            //var records = query
            //    .OrderBy(x => x.a.CreatedOn)
            //    .Select(x => new AttendanceViewModel
            //    {
            //        UserId = (int)x.a.UserId,
            //        UserName = x.u.UserName,
            //        CreatedOn = x.a.CreatedOn,
            //        PunchInTime = x.a.PunchInTime,
            //        PunchOutTime = x.a.PunchOutTime,
            //        GracePeriodTime = x.a.GracePeriodTime,
            //        Designation = x.u.Designation,
            //    }).
            //    Distinct().ToList();
            var attendanceData = (
  from a in _context.attendanceEntitie
  join u in _context.userMasterEntitie on a.UserId equals u.UserId
  where a.UserId != null
  group a by new { a.UserId, u.UserName, u.Designation, u.Email } into g
  select new AttendanceViewModel
  {
      UserId = g.Key.UserId.Value,
      UserName = g.Key.UserName,
      Designation = g.Key.Designation,
      Email = g.Key.Email,


  }).Distinct().ToList();


            return PartialView("_AttendanceTable", attendanceData.ToList()); // Return partial view with updated data
        }

        public IActionResult GetPunchDetails(string date)
        {
            DateTime targetDate = DateTime.Parse(date);
            var attendance = _context.attendanceEntitie
                .FirstOrDefault(a => a.CreatedOn == targetDate.Date);

            if (attendance == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                punchIn = attendance.PunchInTime.HasValue ? new
                {
                    time = attendance.PunchInTime.Value.ToString("hh:mm tt"),
                    selfieUrl = Url.Content($"~/UploadedImages/{attendance.SelfiePath}"),
                    lat = attendance.Latitude,
                    lng = attendance.Longitude
                } : null,
                punchOut = attendance.PunchOutTime.HasValue ? new
                {
                    time = attendance.PunchOutTime.Value.ToString("hh:mm tt"),
                    selfieUrl = Url.Content($"~/UploadedImages/{attendance.PunchOutSelfiePath}"),
                    lat = attendance.PunchOutLatitude,
                    lng = attendance.PunchOutLongitude
                } : null
            });
        }

        [Authorize(Policy = "EmployeeAccess")]
        [HttpGet]


        public IActionResult GetAttendanceById(int? month, int? year, bool isCalendar = true)
        {
            var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
                return RedirectToAction("Login", "Authenticate");

            ViewBag.UserDetails = lr;

            var data = _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId)
                .ToList();

            if (isCalendar)
            {
                var events = data.Select(a => new
                {
                    title = a.PunchOutTime == null ? "Forgot to Punch Out"
                        : a.TotalHours < 4 ? $"Time spent: {a.TotalHours:F2} [LOP]"
                        : a.TotalHours < 8.5 ? $"Time spent: {a.TotalHours:F2} [Half Day]"
                        : $"Time spent: {a.TotalHours:F2} [Full Day]",

                    start = a.PunchInTime?.ToString("yyyy-MM-dd"),
                    color = a.PunchOutTime == null ? "#6c757d"
                        : a.TotalHours < 4 ? "#dc3545"
                        : a.TotalHours < 8.5 ? "#ffc107"
                        : a.WorkType == "WFO" ? "#28a745" : "#007bff"
                });

                ViewBag.CalendarEvents = JsonConvert.SerializeObject(events);
            }

            return View(data);
        }



        private string GetAttendanceTitle(AttendanceEntitie a)
        {
            if (a.Status == "Absent")
                return "Absent";
            if (a.WorkType == "WFO" && a.TotalHours < 9)
                return $"Present ({a.TotalHours} hrs)";
            if (a.WorkType == "WFO" && string.IsNullOrEmpty(a.PunchOutSelfiePath))
                return "Missed PunchOut";
            return $"Present ({a.TotalHours} hrs)";
        }

        private string GetAttendanceColor(AttendanceEntitie a)
        {
            if (a.Status == "Absent")
                return "red";
            if (a.WorkType == "WFO" && string.IsNullOrEmpty(a.PunchOutSelfiePath))
                return "gray";
            if (a.WorkType == "WFO" && a.TotalHours < 9)
                return "yellow";
            return "green";
        }




        public List<AttendanceEntitie> GetAttendanceByUserId(
      int userId,
      int? month = null,
      int? year = null
  )
        {
            DateTime today = DateTime.Today;

            // Default: last 7 days
            DateTime defaultStartDate = today.AddDays(-7);
            DateTime defaultEndDate = today;


            _context.ChangeTracker.Clear();

            var attendanceRecords = _context.attendanceEntitie
                .Where(a => a.UserId == userId &&
                            a.CreatedOn.HasValue &&
                            a.CreatedOn.Value.Date >= defaultStartDate.Date &&
                            a.CreatedOn.Value.Date <= defaultEndDate.Date)
                .OrderBy(a => a.CreatedOn)
                .ToList();

            List<AttendanceEntitie> result = new List<AttendanceEntitie>();

            bool isFridayPresent = false, isMondayPresent = false;

            for (DateTime date = defaultStartDate; date <= defaultEndDate; date = date.AddDays(1))
            {
                var record = attendanceRecords.FirstOrDefault(a => a.CreatedOn.HasValue && a.CreatedOn.Value.Date == date.Date);
                string status = "Absent";

                if (record != null && record.PunchInTime != null &&
                    date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday)
                {
                    status = "Present";
                    if (date.DayOfWeek == DayOfWeek.Friday) isFridayPresent = true;
                    if (date.DayOfWeek == DayOfWeek.Monday) isMondayPresent = true;
                }

                result.Add(new AttendanceEntitie
                {
                    UserId = userId,
                    CreatedOn = date,
                    PunchInTime = record?.PunchInTime,
                    PunchOutTime = record?.PunchOutTime,
                    GracePeriodTime = record?.GracePeriodTime,
                    Status = status
                });
            }

            for (int i = 0; i < result.Count; i++)
            {
                DateTime date = result[i].CreatedOn.Value;

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    result[i].Status = (!isFridayPresent && !isMondayPresent) ? "Absent" : "Present";
                }
            }

        

            return result;
        }

        public int GetLeavesCount()
        {
            return _context.leavesEntitie
                .Count(l => l.CreatedOn.HasValue && l.CreatedOn.Value.Date == DateTime.Today);
        }

        [HttpGet]
        public IActionResult GetSummaryCounts(int month, int year)
        {
            // Replace this with your actual logic
            var result = new
            {
                TotalDays = DateTime.DaysInMonth(year, month),
                PresentDays = 11,
                WeeklyOffs = 8,
                Leaves = 2,
                Holidays = 1,
                Absents = 8,
                PaidDays = 22
            };

            return Json(result);
        }



        [Authorize(Policy = "AdminAccess")]
        public IActionResult AdminDash()
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set user details to ViewBag
            ViewBag.UserDetails = lr;
            ViewBag.username = lr.userName;
            ViewBag.profileUrl = lr.profileUrl;
            ViewBag.emailId = lr.emailId;
            ViewBag.userTypeName = lr.userTypeName;

            int totalEmployees = _service.TotalEmplyee();
            int presentEmployees = _service.GetPresentEmployeesToday();
            var todayabsent = _service.GetTodayAbsent();
            var currentProjects = _service.GetCurrentProjects();
            var percentageChange = CalculatePercentageChange(totalEmployees, presentEmployees);

            // Fetch notifications
            var notifications = (from n in _context.notificationEntity
                                 join l in _context.leavesEntitie on n.leaveid equals l.LeaveId
                                 where l.Status == null || (l.Status != "Accepted" && l.Status != "Rejected")
                                 orderby n.CreatedOn descending
                                 select n).ToList();

            ViewBag.Notifications = notifications;

            // Fetch employees by status
            var statusCounts = _context.userMasterEntitie
                .Where(e => e.IsActive == true && e.IsDeleted == false)
                .GroupBy(e => e.StatusId)
                .Select(group => new
                {
                    StatusId = group.Key,
                    Count = group.Count()
                })
                .ToDictionary(sc => sc.StatusId, sc => sc.Count);

            int fullTimeCount = statusCounts.ContainsKey(1) ? statusCounts[1] : 0;
            int contractCount = statusCounts.ContainsKey(2) ? statusCounts[2] : 0;
            int probationCount = statusCounts.ContainsKey(3) ? statusCounts[3] : 0;
            int wfhCount = statusCounts.ContainsKey(4) ? statusCounts[4] : 0;
            // Fetch Performance Data (Total Production Duration)
            var performanceData = _context.attendanceEntitie
                .Where(a => a.PunchInTime != null && a.PunchOutTime != null)
                .AsEnumerable() // Fetch data into memory before GroupBy
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalProductionDuration = g.Sum(e => (e.PunchOutTime.Value - e.PunchInTime.Value).TotalHours)
                })
                .ToList();

            // Fetch Top Performer (Attendance + Work Entry)
            var topPerformerData = _context.attendanceEntitie
                .Where(a => a.PunchInTime != null && a.PunchOutTime != null)
                .AsEnumerable() // Convert to in-memory for DateTime calculations
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalHoursStayed = g.Sum(a => (a.PunchOutTime.Value - a.PunchInTime.Value).TotalHours),
                    TotalTaskTime = _context.WorkEntries
                        .Where(w => w.UserId == g.Key)
                        .Sum(w => w.TimeSpent) / 60.0 // Convert minutes to hours (assuming TimeSpent is in minutes)
                })
                .OrderByDescending(x => x.TotalHoursStayed) // Rank by total hours stayed
                .FirstOrDefault();

            // Fetch user details from UserMasterEntity
            // Fetch max hours stayed for normalization
            var maxHoursStayed = performanceData.Max(x => x.TotalProductionDuration);

            // Fetch Top Performer (Normalized to 100%)
            var topPerformer = topPerformerData != null ? new TopPerformer
            {
                userid = topPerformerData.UserId ?? 0,
                Name = _context.userMasterEntitie
                    .Where(u => u.UserId == topPerformerData.UserId)
                    .Select(u => u.UserName)
                    .FirstOrDefault() ?? "Unknown",
                Role = _context.userMasterEntitie
                    .Where(u => u.UserId == topPerformerData.UserId)
                    .Select(u => u.Designation)
                    .FirstOrDefault() ?? "Unknown",

                // Normalize Performance: (TotalHoursStayed / MaxHoursStayed) * 100
                Performance = maxHoursStayed > 0 ? (int)Math.Round((topPerformerData.TotalHoursStayed / maxHoursStayed) * 100) : 0,

                ProfilePicture = _context.userMasterEntitie
                    .Where(u => u.UserId == topPerformerData.UserId)
                    .Select(u => u.ProfilePicture)
                    .FirstOrDefault() ?? "Unknown",
            } : null;



            var model = new AdminDashboardViewModel
            {
                TotalEmployees = totalEmployees,
                PresentEmployees = presentEmployees,
                Todayabsent = todayabsent,
                CurrentProjects = currentProjects,
                PercentageChange = percentageChange,
                EmployeeStatus = new EmployeeStatusViewModel
                {
                    TotalEmployees = totalEmployees,
                    FullTimeCount = fullTimeCount,
                    ContractCount = contractCount,
                    ProbationCount = probationCount,
                    WFHCount = wfhCount,
                    TopPerformer = topPerformer
                }
            };

            return View(model);
        }

      

        [HttpPost]
        public IActionResult MarkNotificationAsRead(int id)
        {
            var notification = _context.notificationEntity.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        public IActionResult TopEmployeeDetails(int userId)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set user details to ViewBag
            ViewBag.UserDetails = lr;
            // Fetch employee details
            var employeeData = _context.userMasterEntitie
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Designation,
                    u.Email,
                    u.MobileNumber,
                    u.DateOfJoining,
                    u.ProfilePicture
                })
                .FirstOrDefault();

            if (employeeData == null)
            {
                return NotFound(); // Return 404 if employee not found
            }

            // Fetch attendance records for total hours worked
            var attendanceRecords = _context.attendanceEntitie
                .Where(a => a.UserId == userId && a.PunchInTime.HasValue && a.PunchOutTime.HasValue)
                .AsEnumerable() // Move data to memory to allow DateTime operations
                .ToList();

            double totalHoursWorked = attendanceRecords.Sum(a => (a.PunchOutTime.Value - a.PunchInTime.Value).TotalHours);

            // Create ViewModel and populate data
            var employee = new EmployeeDetailsViewModel
            {
                UserId = employeeData.UserId,
                Name = employeeData.UserName,
                Role = employeeData.Designation,
                Email = employeeData.Email,
                ContactNumber = employeeData.MobileNumber,
                department = employeeData.Designation,
                JoinDate = employeeData.DateOfJoining,
                ProfilePicture = string.IsNullOrEmpty(employeeData.ProfilePicture) ? "default-profile.png" : employeeData.ProfilePicture,
                TotalHoursWorked = totalHoursWorked,
                Performance = CalculatePerformance(userId) // Call fixed method
            };

            return View(employee);
        }


        private int CalculatePerformance(int userId)
        {
            // Fetch attendance records (avoiding direct calculation in LINQ-to-SQL)
            var attendanceRecords = _context.attendanceEntitie
                .Where(a => a.UserId == userId && a.PunchInTime.HasValue && a.PunchOutTime.HasValue)
                .AsEnumerable() // Forces execution in C# memory
                .ToList();

            double totalHours = attendanceRecords.Sum(a => (a.PunchOutTime.Value - a.PunchInTime.Value).TotalHours);

            // Fetch total task time directly from database (no need for AsEnumerable here)
            double taskHours = _context.WorkEntries
                .Where(w => w.UserId == userId)
                .Sum(w => (double?)w.TimeSpent) ?? 0; // Handles null values safely

            if (totalHours == 0) return 0; // Avoid division by zero

            // Calculate performance percentage
            int performance = (int)Math.Round((taskHours / totalHours) * 100);

            return Math.Min(performance, 100); // Cap at 100%
        }


        public IActionResult LeaveList()
        {
            // Get logged-in user session
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set user details to ViewBag
            ViewBag.UserDetails = lr;

            // Get today's date range (start and end of today)
            DateTime todayStart = DateTime.Today;
            DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);

            // Get all active employees
            var allEmployees = _context.userMasterEntitie
                .Where(u => u.IsDeleted == false && u.IsActive == true)
                .Select(u => new UserMasterDTO
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Designation = u.Designation // Ensure Designation exists in the database
                })
                .ToList();

            // Get employees who have punched in today based on PunchInTime
            var punchedInEmployees = _context.attendanceEntitie
                .Where(a => a.PunchInTime >= todayStart && a.PunchInTime <= todayEnd)
                .Select(a => a.UserId)
                .Distinct()
                .ToList();

            // Get employees who have NOT punched in
            var leaveList = allEmployees
                .Where(emp => !punchedInEmployees.Contains(emp.UserId))
                .ToList();

            return View(leaveList);
        }

        [HttpGet]
        public IActionResult GetPunchDetails( DateTime date)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            var nextDate = date.Date.AddDays(1);

            var punches = _context.attendanceEntitie
                .Where(x => x.UserId == lr.userId && x.CreatedOn >= date.Date && x.CreatedOn < nextDate)
                .FirstOrDefault();

            if (punches == null)
                return Json(new { punchIn = (object)null, punchOut = (object)null });

            var result = new
            {
                punchIn = punches.PunchInTime != null ? new
                {
                    time = punches.PunchInTime.Value.ToString("hh:mm tt"),
                    selfieUrl = punches.SelfiePath,
                    lat = punches.Latitude,
                    lng = punches.Longitude
                } : null,

                punchOut = punches.PunchOutTime != null ? new
                {
                    time = punches.PunchOutTime.Value.ToString("hh:mm tt"),
                    selfieUrl = punches.PunchOutSelfiePath,
                    lat = punches.PunchOutLatitude,
                    lng = punches.PunchOutLongitude
                } : null
            };

            return Json(result);
        }


        public IActionResult PresentToday()
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set user details to ViewBag
            ViewBag.UserDetails = lr;

            // Get today's date
            DateTime today = DateTime.Today;

            // Fetch attendance records where PunchInTime is today, including UserName
            var presentUsers = _context.attendanceEntitie
                .Where(a => a.PunchInTime.HasValue && a.PunchInTime.Value.Date == today)
                .Join(_context.userMasterEntitie,
                      a => a.UserId,
                      u => u.UserId,
                      (a, u) => new AttendanceDTO
                      {
                          AttendanceId = a.AttendanceId,
                          UserId = a.UserId,
                          PunchInTime = a.PunchInTime,
                          Status = "Present",
                          CreatedOn = a.CreatedOn,
                          ScreenShot = a.ScreenShot, // Keeping other fields intact
                          IsActive = a.IsActive,
                          IsDeleted = a.IsDeleted,
                          GracePeriodTime = a.GracePeriodTime,
                          ProductionDuration = a.ProductionDuration,
                          UserName = u.UserName // Fetching username
                      })
                .ToList();

            ViewBag.TotalPresent = presentUsers.Count(); // Count of present users

            return View(presentUsers);
        }

        public IActionResult ProjectList()
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set user details to ViewBag
            ViewBag.UserDetails = lr;

            // Fetch all active projects (not deleted)
            var projects = _context.projectEntities
                .Where(p => !p.IsDeleted)
                .Select(p => new ProjectEntity
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    ClientName = p.ClientName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    Budget = p.Budget,
                    Description = p.Description,
                    CreatedOn = p.CreatedOn
                })
                .ToList();

            ViewBag.TotalProjects = projects.Count(); // Total number of projects

            return View(projects);
        }






        public IActionResult AcceptLeaveRequest(int notificationId)
        {
            var notification = _context.notificationEntity
                .Where(n => n.Id == notificationId)
                .FirstOrDefault();

            if (notification != null)
            {
                // Mark as read
                notification.IsRead = true;

                // Explicitly mark the entity as modified
                _context.Entry(notification).State = EntityState.Modified;
                _context.SaveChanges(); // Commit changes
            }

            return RedirectToAction("AdminDash");
        }




        [HttpPost]
		public async Task<IActionResult> AutoPunchOut()
		{
			LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

			if (lr == null)
			{
				return Unauthorized(new { message = "User not logged in" });
			}

			var attendance = await _context.attendanceEntitie
				.Where(a => a.UserId == lr.userId && a.PunchOutTime == null)
				.OrderByDescending(a => a.AttendanceId)
				.FirstOrDefaultAsync();

			if (attendance == null)
			{
				return NotFound(new { message = "No active attendance record found" });
			}

			// Auto punch-out at 11:59 PM
			DateTime punchOutTime = DateTime.Today.AddHours(23).AddMinutes(59).AddSeconds(59);
			attendance.PunchOutTime = punchOutTime;
			attendance.ProductionDuration = (int)(attendance.PunchOutTime - attendance.PunchInTime)?.TotalMinutes;
			attendance.UpdatedOn = DateTime.Now;
			attendance.UpdatedBy = lr.userId;

			_context.attendanceEntitie.Update(attendance);
			await _context.SaveChangesAsync();

			return Ok(new { message = "Auto Punch-Out Successful", punchOutTime });
		}


		// Helper method to calculate percentage change
		private double CalculatePercentageChange(int total, int present)
		{
			if (total == 0) return 0;
			return ((double)present / total) * 100;
		}


		public IActionResult PresentList()
		{
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set user details to ViewBag
            ViewBag.UserDetails = lr;
            ViewBag.username = lr.userName;
            var res = _mService.PresentList();
			return View(res);

        }


        public async Task<IActionResult> AttendanceStatus()
        {
             
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null) return Unauthorized(new { message = "User not logged in" });

            
            var lastAttendance = await _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId)
                .OrderByDescending(a => a.PunchInTime)
                .FirstOrDefaultAsync();

            bool isPunchedIn = lastAttendance != null && lastAttendance.PunchInTime.HasValue && !lastAttendance.PunchOutTime.HasValue;
            bool canPunchIn = lastAttendance == null || (lastAttendance.PunchOutTime.HasValue && lastAttendance.PunchOutTime.Value.Date < DateTime.Today);

            return Ok(new
            {
                isPunchedIn,
                punchInTime = lastAttendance?.PunchInTime?.ToString("HH:mm:ss"),
                canPunchIn
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetTodayPunchInfo()
        {
            var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var punchInfo = await _context.attendanceEntitie
                .Where(p => p.UserId == lr.userId && p.PunchInTime >= today && p.PunchInTime < tomorrow)
                .FirstOrDefaultAsync();

            if (punchInfo == null)
            {
                return Json(new { success = false });
            }

            // Generate map centered exactly on the GPS coordinates
            string GenerateMapUrl(double? lat, double? lng)
            {
                return (lat.HasValue && lng.HasValue)
                    ? $"https://maps.google.com/maps?q={lat.Value.ToString(CultureInfo.InvariantCulture)},{lng.Value.ToString(CultureInfo.InvariantCulture)}&hl=es&z=18&output=embed"
                    : "";
            }

            var punchInMapUrl = GenerateMapUrl(punchInfo.Latitude, punchInfo.Longitude);
            var punchOutMapUrl = GenerateMapUrl(punchInfo.PunchOutLatitude, punchInfo.PunchOutLongitude);

            return Json(new
            {
                success = true,
                punchInTime = punchInfo.PunchInTime?.ToString("hh:mm tt"),
                punchOutTime = punchInfo.PunchOutTime?.ToString("hh:mm tt"),
                punchInSelfie = punchInfo.SelfiePath,
                punchOutSelfie = punchInfo.PunchOutSelfiePath,
                punchInLocation = punchInMapUrl,
                punchOutLocation = punchOutMapUrl
            });
        }




        [HttpPost]
        public async Task<IActionResult> PunchIn(IFormFile Selfie, [FromForm] double latitude, [FromForm] double longitude, [FromForm] string WorkType, string WFHReason)
        {
            // ✅ Check session
            var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserId = lr.userId;
            ViewBag.userName = lr.userName;
            ViewBag.emailId = lr.emailId;
            ViewBag.userTypeName = lr.userTypeName;

            //// 📍 Static Office GPS Coordinates
            //double officeLatitude = 17.447021636336842;
            //double officeLongitude = 78.35464083733754;
            //double allowedRadius = 800; // meters

            //// ✅ Distance Check
            //double distance = GetDistance(latitude, longitude, officeLatitude, officeLongitude);
            //if (distance > allowedRadius)
            //{
            //    return BadRequest(new
            //    {
            //        message = "⚠️ Punch-in allowed only in office premises.",
            //        status = "warning"
            //    });
            //}

            // 📸 Save Selfie
            string selfiePath = "";
            if (Selfie != null && Selfie.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "selfies");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Selfie.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Selfie.CopyToAsync(fileStream);
                }

                selfiePath = "/selfies/" + uniqueFileName;
            }

            // 🕒 Grace Period Logic
            var gracePeriod = TimeSpan.FromMinutes(20);
            var expectedPunchInTime = DateTime.UtcNow.Date.AddHours(9).AddMinutes(30);
            var punchInTime = DateTime.Now;
            int minutesLate = (int)(punchInTime - expectedPunchInTime).TotalMinutes;
            if (minutesLate < 0) minutesLate = 0;

            string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // 💾 Save to DB
            var attendance = new AttendanceEntitie
            {
                UserId = lr.userId,
                PunchInTime = punchInTime,
                IPAddress = userIpAddress,
                Latitude = latitude,
                Longitude = longitude,
                SelfiePath = selfiePath,
                WorkType=WorkType,
                Reason= WFHReason,
                GracePeriodTime = minutesLate,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now,
                Status = "Present",
                CreatedBy = lr.userId
            };

            _context.attendanceEntitie.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "✅ Punched in successfully!",
                status = "success",
                punchInTime,
                ipAddress = userIpAddress,
                selfieUrl = selfiePath
            });
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000; 
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;  
        }



        [HttpPost]
        public async Task<IActionResult> PunchOut(IFormFile Selfie, [FromForm] double latitude, [FromForm] double longitude,string TotalHours)
        {
            // Get logged in user from session
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return Unauthorized(new { message = "Session expired. Please login again." });
            }

            var attendance = await _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId && a.PunchOutTime == null)
                .FirstOrDefaultAsync();
            string selfiePath = "";
            if (Selfie != null && Selfie.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "selfies");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Selfie.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Selfie.CopyToAsync(fileStream);
                }

                selfiePath = "/selfies/" + uniqueFileName;
            }

            if (attendance == null)
            {
                return BadRequest(new { message = "You have not punched in yet." });
            }
            attendance.PunchOutLatitude = latitude;
            attendance.PunchOutLongitude = longitude;
            attendance.PunchOutSelfiePath = selfiePath;
            attendance.PunchOutTime = DateTime.Now;
            attendance.TotalHours =Convert.ToDouble(TotalHours);
            attendance.ProductionDuration = (int)(attendance.PunchOutTime - attendance.PunchInTime)?.TotalMinutes;
            attendance.UpdatedOn = DateTime.Now;
            attendance.UpdatedBy = lr.userId;

            _context.attendanceEntitie.Update(attendance);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Punched out successfully",
                punchOutTime = attendance.PunchOutTime?.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }


        [HttpGet]
        public async Task<IActionResult> GetAttendanceStatus()
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            DateTime today = DateTime.Today;
            DateTime now = DateTime.Now;

            var attendance = await _context.attendanceEntitie
                .Where(a => a.UserId == lr.userId)
                .OrderByDescending(a => a.AttendanceId)
                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return Ok(new { isPunchedIn = false, canPunchIn = true, totalHours = "00:00:00" });
            }

            bool isToday = attendance.PunchInTime?.Date == today;
            bool isPunchedIn = attendance.PunchOutTime == null && isToday;
            bool canPunchIn = false;
            TimeSpan totalHours = TimeSpan.Zero;

            // Auto punch-out if exceeded time or day ends
            if (isPunchedIn)
            {
                TimeSpan elapsed = now - attendance.PunchInTime.Value;
                DateTime endOfDay = today.AddHours(23).AddMinutes(59).AddSeconds(59);

                if (elapsed.TotalHours >= 10)
                {
                    attendance.PunchOutTime = attendance.PunchInTime.Value.AddHours(10);
                }
                else if (now >= endOfDay)
                {
                    attendance.PunchOutTime = endOfDay;
                }

                if (attendance.PunchOutTime != null)
                {
                    attendance.ProductionDuration = (int)(attendance.PunchOutTime - attendance.PunchInTime)?.TotalMinutes;
                    attendance.UpdatedOn = now;
                    attendance.UpdatedBy = lr.userId;

                    _context.attendanceEntitie.Update(attendance);
                    await _context.SaveChangesAsync();

                    isPunchedIn = false;
                }
            }

            // Can punch in only if not punched in today
            canPunchIn = !isToday || (attendance.PunchOutTime != null && !isPunchedIn);

            if (isToday)
            {
                totalHours = (attendance.PunchOutTime ?? now) - attendance.PunchInTime.Value;
            }

            return Ok(new
            {
                isPunchedIn,
                canPunchIn,
                punchInTime = attendance.PunchInTime,
                totalHours = $"{(int)totalHours.TotalHours:D2}:{totalHours.Minutes:D2}:{totalHours.Seconds:D2}"
            });
        }



        private int? GetLoggedInUserId()
		{
			// Replace with your actual authentication logic
			return 1; // Example hardcoded User ID
		}




		public void CalculateWorkHours()
		{
			// Example collection of weekAttendance
			List<AttendanceEntitie> weekAttendance = new List<AttendanceEntitie>
	{
		new AttendanceEntitie { PunchInTime = DateTime.Parse("2025-01-01 09:00:00"), PunchOutTime = DateTime.Parse("2025-01-01 17:00:00") },
		new AttendanceEntitie { PunchInTime = DateTime.Parse("2025-01-02 09:00:00"), PunchOutTime = DateTime.Parse("2025-01-02 17:30:00") }
	};

			var totalWorkHoursWeek = weekAttendance
				.Where(a => a.PunchInTime != null && a.PunchOutTime.HasValue) // Ensure PunchInTime and PunchOutTime are not null
				.Select(a => (a.PunchOutTime.Value - a.PunchInTime.Value).TotalHours)  // Calculate the difference in hours
				.Sum();

			Console.WriteLine("Total Work Hours This Week: " + totalWorkHoursWeek);
		}








        [Authorize(Policy = "AdminAccess")]
        public IActionResult EmployeeDetails(int userId)
        {
            // Fetch the employee details from the database
            var user = _context.userMasterEntitie
                .FirstOrDefault(u => u.UserId == userId && u.IsDeleted==false && u.IsActive==true);

            if (user == null)
            {
                return NotFound("Employee not found");
            }

            // Prepare the ViewModel
            var model = new EmployeeDetailsViewModel
            {
                UserId = user.UserId,
                Name = user.UserName,
                Email = user.Email,
                Role = user.Designation,
                ProfilePicture = user.ProfilePicture,
                Status = user.StatusId == 1 ? "Full-Time"
                       : user.StatusId == 2 ? "Contract"
                       : user.StatusId == 3 ? "Probation"
                       : "Work From Home",
                JoinDate = user.DateOfJoining,
                ContactNumber = user.MobileNumber
            };

            return View(model);
        }

        public IActionResult EditUserempdash(int userid)
        {
            UserMasterDTO um = new UserMasterDTO();
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (lr == null)
            {
                return RedirectToAction("Login");
            }

            var UserDetails = _service.GetUserById(userid);
            CloneObjects.CopyPropertiesTo(UserDetails, um);

            // ✅ Correctly assign BankDetails to 'um'
            um.BankDetails = _mService.GetDetailsByuserid(userid); // or UserDetails.UserId if different

            // ✅ Pass the correct model
            return View(um);
        }


        [HttpPost]
        public IActionResult EditUserEmpdash(UserMasterDTO model, IFormFile ProfileImgUploaded)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            bool isProfilePicUploaded = false;
            string ProfilePic = "";

            // Check for email existence
            var checkEmail = _service.UserList().Where(a => a.EmployeeId == model.EmployeeId && a.UserId != model.UserId).FirstOrDefault();
            if (checkEmail != null)
            {
                ModelState.AddModelError("EmailId", "Email Id not available");
            }

            // Profile picture upload
            if (model.ProfileImgUploaded != null)
            {
                var fileNameUploaded = Path.GetFileName(model.ProfileImgUploaded.FileName);
                if (fileNameUploaded != null)
                {
                    var contentType = model.ProfileImgUploaded.ContentType;
                    string filename = DateTime.UtcNow.ToString();
                    filename = Regex.Replace(filename, @"[\[\]\\\^\$\.\|\?\*\+\(\)\{\}%,;: ><!@#&\-\+\/]", "");
                    filename = Regex.Replace(filename, "[A-Za-z ]", "");
                    filename = filename + RandomGenerator.RandomString(4, false);
                    string extension = Path.GetExtension(fileNameUploaded);
                    filename += extension;
                    var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "UploadedImages");
                    var filePath = Path.Combine(uploads, filename);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfileImgUploaded.CopyToAsync(fileStream);
                    }
                    ProfilePic = filename;
                    isProfilePicUploaded = true;
                }
                else
                {
                    ProfilePic = "dummy.png";
                }
            }
            else
            {
                ProfilePic = "dummy.png";
            }

            UserMasterEntitie u = _service.GetUserById(model.UserId);

            if (model.AdharNumber != null)
                u.AdharNumber = model.AdharNumber;

            if (model.PanNumber != null)
                u.PanNumber = model.PanNumber;

            u.ProfilePicture = ProfilePic;
            u.EmployeeId = model.EmployeeId;
            u.UserName = model.UserName;
            u.DateOfJoining = model.DateOfJoining;
            u.DateOfBirth = model.DateOfBirth;
            u.Address = model.Address;
            u.MobileNumber = model.MobileNumber;
            u.GuardianNumber = model.GuardianNumber;
            u.CollegeName = model.CollegeName;
            u.Designation = model.Designation;

            int count = _context.userMasterEntitie.Where(a => a.EmployeeId == u.EmployeeId && a.IsDeleted == false).Count();
            int count1 = !string.IsNullOrEmpty(u.AdharNumber) ? _context.userMasterEntitie.Where(a => a.AdharNumber == u.AdharNumber && a.IsDeleted == false).Count() : 0;
            int count2 = !string.IsNullOrEmpty(u.PanNumber) ? _context.userMasterEntitie.Where(a => a.PanNumber == u.PanNumber && a.IsDeleted == false).Count() : 0;

            if (count < 2 && count1 < 2 && count2 < 2)
            {
                var res = _service.UpdateUser(u);

                if (res.statuCode == 1)
                {
                    // 🏦 Bank Details Save/Update
                    BankDetailsEntitie bankDetails = new BankDetailsEntitie
                    {
                        BankId = Convert.ToInt32(Request.Form["BankId"]),
                        UserId = model.UserId,
                        AccountNumber = Request.Form["AccountNumber"],
                        IFSCNumber = Request.Form["IFSCNumber"],
                        BranchName = Request.Form["BranchName"],
                        AccountType = Request.Form["AccountType"],
                        UpdatedOn = DateTime.Now,
                        UpdatedBy = lr.userId
                    };

                    if (bankDetails.BankId > 0)
                    {
                        _mService.UpdateBankDetails(bankDetails);
                    }
                    else
                    {
                        bankDetails.AccountNumber = model.BankDetails.AccountNumber;
                        bankDetails.IFSCNumber = model.BankDetails.IFSCNumber;
                        bankDetails.BranchName = model.BankDetails.BranchName;
                        bankDetails.AccountType = model.BankDetails.AccountType;
                        bankDetails.CreatedBy = lr.userId;
                        bankDetails.CreatedOn = DateTime.Now;
                        bankDetails.IsActive = true;
                        bankDetails.IsDeleted = false;
                        _mService.CreateBankDetails(bankDetails);
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Duplicate records found for the given EmployeeId, AdharNumber, or PanNumber.";
            }

            return View(model);
        }





        [Authorize(Policy = "EmployeeAccess")]
        public async Task<IActionResult> HolidaysListempdash()
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserTypeList = new SelectList(_context.userTypeMasterEntitie, "UserTypeId", "UserTypeName");

            // Filter only upcoming holidays
            var upcomingHolidays = _mService.HolidaysList()
                                            .Where(h => h.HolidayDate >= DateTime.Now.Date)
                                            .ToList();

            return View(upcomingHolidays);
        }


        public IActionResult GetEmployeesByDepartment(string duration)
        {
            var employees = _context.userMasterEntitie
                .Where(u => !u.IsDeleted)
                .GroupBy(u => u.Designation)
                .Select(g => new
                {
                    Department = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            return Json(employees);
        }

        public IActionResult DownloadAttendanceReport(DateTime startDate, DateTime endDate, int? userId)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<AttendanceReportViewModel> report = GenerateAttendanceReport(startDate, endDate,userId);

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Attendance Report");

                // **Headers**
                string[] headers = { "S.No", "Emp ID", "Name", "Designation", "Department", "DOJ",
                             "Present Days", "Casual Leave", "Sick Leave", "Earned Leaves",
                             "Half Days", "Holidays", "Week Off", "Absent", "Final Paid Days" };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i]; // Normal headers (No background color)
                }

                int colIndex = headers.Length + 1;
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    worksheet.Cells[1, colIndex].Value = date.ToString("dd/MM");

                    // ✅ Apply light yellow background **only** to DailyStatus headers
                    worksheet.Cells[1, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, colIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

                    colIndex++;
                }

                // **Data Rows**
                int rowIndex = 2;
                int serialNumber = 1;
                foreach (var user in report)
                {
                    worksheet.Cells[rowIndex, 1].Value = serialNumber++;
                    worksheet.Cells[rowIndex, 2].Value = user.EmployeeId;
                    worksheet.Cells[rowIndex, 3].Value = user.UserName;
                    worksheet.Cells[rowIndex, 4].Value = user.Designation;
                    worksheet.Cells[rowIndex, 5].Value = user.Department;
                    worksheet.Cells[rowIndex, 6].Value = user.DOJ;
                    worksheet.Cells[rowIndex, 7].Value = user.PresentDays;
                    worksheet.Cells[rowIndex, 8].Value = user.CasualLeave;
                    worksheet.Cells[rowIndex, 9].Value = user.SickLeave;
                    worksheet.Cells[rowIndex, 10].Value = user.EarnedLeaves;
                    worksheet.Cells[rowIndex, 11].Value = user.HalfDay;
                    worksheet.Cells[rowIndex, 12].Value = user.Holidays;
                    worksheet.Cells[rowIndex, 13].Value = user.WeekOff;
                    worksheet.Cells[rowIndex, 14].Value = user.Absent;
                    worksheet.Cells[rowIndex, 15].Value = user.FinalPaidDays;

                    colIndex = 16;
                    foreach (var status in user.DailyStatus.Values)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = status; // No background color for data cells
                        colIndex++;
                    }
                    rowIndex++;
                }

                worksheet.Cells.AutoFitColumns();
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendanceReport.xlsx");
            }
        }

        public List<AttendanceReportViewModel> GenerateAttendanceReport(DateTime startDate, DateTime endDate, int? userId = null)
        {
            var usersQuery = from user in _context.userMasterEntitie
                             join dept in _context.department on user.DepartmentId equals dept.DepartmentID
                             where user.IsDeleted == false
                             select new
                             {
                                 user.UserId,
                                 user.UserName,
                                 user.Email,
                                 user.IsDeleted,
                                 user.EmployeeId,
                                 user.Designation,
                                 user.DateOfJoining,
                                 user.IsActive,
                                 Department = dept.DepartmentName
                             };

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

            var attendanceReport = new List<AttendanceReportViewModel>();

            foreach (var user in users)
            {
                int presentDays = 0, casualLeave = 0, weekOff = 0, absent = 0, holidaysCount = 0, lateArrivals = 0;
                double halfDays = 0, sickLeave = 0, earnedLeaves = 0;
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

                double finalPaidDays = presentDays + (halfDays * 0.5) + earnedLeaves + holidaysCount+casualLeave;

                attendanceReport.Add(new AttendanceReportViewModel
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    EmployeeId = user.EmployeeId,
                    Designation = user.Designation,
                    Department = user.Department,
                    DOJ = user.DateOfJoining.HasValue ? user.DateOfJoining.Value.ToString("dd-MM-yyyy") : "N/A",
                    PresentDays = presentDays,
                    CasualLeave = casualLeave,
                    WeekOff = weekOff,
                    Absent = absent,
                    Holidays = holidaysCount,
                    HalfDay = halfDays,
                    SickLeave = (int)sickLeave,
                    EarnedLeaves = (int)earnedLeaves,
                    FinalPaidDays = finalPaidDays,
                    DailyStatus = dailyStatus
                });
            }

            return attendanceReport;
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day <= thru.Date; day = day.AddDays(1))
                yield return day;
        }






    }




}

