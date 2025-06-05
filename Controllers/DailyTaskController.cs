using AttendanceCRM.BAL.IServices;
using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AttendanceCRM.Controllers
{
    public class DailyTaskController : Controller
    {

        private readonly IUserService _Service;
        private readonly IMasterMgmtService _mService;
        private readonly ILeaveTypeMaster _lService;
        private readonly MyDbContext _context;

        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;


        public DailyTaskController(IUserService Service, MyDbContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IMasterMgmtService mService, ILeaveTypeMaster lService)
        {
            _Service = Service;

            _hostingEnvironment = hostingEnvironment;
            _mService = mService;
            _context = context;
            _lService = lService;
        }

		[Authorize(Policy = "EmployeeAccess")]
        public async Task<IActionResult> Index()
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            DateTime date = DateTime.Today;
            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;
            var workEntries = _context.WorkEntries
                .Where(t => t.UserId == lr.userId && t.CreatedAt.Date == date.Date)
                .Join(_context.userMasterEntitie, w => w.UserId, u => u.UserId, (w, u) => new { w, u })
                .Join(_context.projectEntities, wu => wu.w.ProjectId, p => p.ProjectId, (wu, p) => new WorkEntryViewModel
                {
                    Id = wu.w.Id,
                    UserId = wu.w.UserId,
                    UserName = wu.u.UserName,
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    CreatedAt = wu.w.CreatedAt,
                    Description = wu.w.Description,
                    TaskName = wu.w.TaskName,
                    Status = wu.w.Status,
                    TimeSpent = wu.w.TimeSpent
                })
                .ToList();



            var projects = _mService.projectlist().ToList();
            ViewBag.pp = projects;

            return View(workEntries);
        }

        public IActionResult FilterTasksByDate(DateTime date)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;
            var workEntries = _context.WorkEntries
     .Where(t => t.UserId == lr.userId && t.CreatedAt.Date == date.Date)
     .Join(_context.userMasterEntitie, w => w.UserId, u => u.UserId, (w, u) => new { w, u })
     .Join(_context.projectEntities, wu => wu.w.ProjectId, p => p.ProjectId, (wu, p) => new WorkEntryViewModel
     {
         Id = wu.w.Id,
         UserId = wu.w.UserId,
         UserName = wu.u.UserName,
         ProjectId = p.ProjectId,
         ProjectName = p.ProjectName,
         CreatedAt = wu.w.CreatedAt,
         Description = wu.w.Description,
         TaskName = wu.w.TaskName,
         Status = wu.w.Status,
         TimeSpent = wu.w.TimeSpent
     })
     .ToList();

            var tasksHtml = RenderPartialViewToString("_EmpTaskListPartial", workEntries);

            int totalMinutes = workEntries.Sum(t => t.TimeSpent);
            TimeSpan timeSpent = TimeSpan.FromMinutes(totalMinutes);
            string formattedTimeSpent = $"{(int)timeSpent.TotalHours}h {timeSpent.Minutes}m";

           // var totalTimeSpent = workEntries.Sum(t => t.TimeSpent);
            var completedTasks = workEntries.Count(t => t.Status == "Completed");
            var inProgressTasks = workEntries.Count(t => t.Status == "In Progress");
            var plannedTasks = workEntries.Count(t => t.Status == "Planned");

            return Json(new
            {
                tasksHtml,
                totalTime = formattedTimeSpent,
                completedTasks,
                inProgressTasks,
                plannedTasks
            });
        }


        // private string RenderPartialViewToString(string viewName, object model)
        //{
        //    ViewData.Model = model;
        //    using (var sw = new StringWriter())
        //    {
        //        var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
        //        var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());
        //        viewResult.View.RenderAsync(viewContext);
        //        return sw.ToString();
        //    }
        //}




        [HttpPost]
        public IActionResult DeleteTasks(int id)
        {
             var workEntry = _context.WorkEntries.FirstOrDefault(e => e.Id == id);

            if (workEntry == null)
            {
                return NotFound();  
            }

            _context.WorkEntries.Remove(workEntry);  
            _context.SaveChanges();  

            return Json(new { success = true, message = "Record deleted successfully!" });
        }


        //public async Task<IActionResult> FetchTasks()
        //{
        //    var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

        //    if (lr == null)
        //    {
        //        return RedirectToAction("Login", "Authenticate");
        //    }

        //    var filteredTasks = _context.WorkEntries
        //       .Where(w =>
        //            w.UserId == lr.userId)
        //       .Join(_context.userMasterEntitie, w => w.UserId, u => u.UserId, (w, u) => new WorkEntryViewModel
        //       {
        //           Id = w.Id,
        //           UserId = w.UserId,
        //           UserName = u.UserName,
        //           CreatedAt = w.CreatedAt,
        //           Description = w.Description,
        //           TaskName = w.TaskName,
        //           Status = w.Status,
        //           TimeSpent = w.TimeSpent
        //       })
        //       .ToList();


        //    //var workEntries = await _context.WorkEntries
        //    //    .Where(w => w.UserId == lr.userId)
        //    //    .Join(_context.userMasterEntitie, w => w.UserId, u => u.UserId, (w, u) => new WorkEntryViewModel
        //    //    {
        //    //        Id = w.Id,
        //    //        UserId = w.UserId,
        //    //        UserName = u.UserName, 
        //    //        CreatedAt = w.CreatedAt,
        //    //        Description = w.Description,
        //    //        TaskName = w.TaskName,
        //    //        Status = w.Status,
        //    //        TimeSpent = w.TimeSpent
        //    //    })
        //    //    .ToListAsync(); 

        //    return PartialView("_timestamp", filteredTasks);
        //}


        public async Task<IActionResult> Index1(string selectedDate)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;

            DateTime filterDate = string.IsNullOrEmpty(selectedDate) ? DateTime.Today : DateTime.Parse(selectedDate);

            var workEntries = _context.WorkEntries
                .Where(t => t.UserId == lr.userId && t.CreatedAt.Date == filterDate.Date)
                .ToList();

            ViewBag.SelectedDate = filterDate.ToString("yyyy-MM-dd");

            return View(workEntries);
        }

        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> GetAlltasksByUser(int? userId, DateTime? startDate, DateTime? endDate)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;

            var users = _context.userMasterEntitie
                .Where(u => u.UserTypeId == 3&& u.IsDeleted==false)
                .Select(u => new { u.UserId, u.UserName })
                .ToList();
            ViewBag.Users = new SelectList(users, "UserId", "UserName");

            var workEntries = _context.WorkEntries
     .Join(_context.userMasterEntitie,
           w => w.UserId,
           u => u.UserId,
           (w, u) => new { w, u })  
     .Join(_context.projectEntities,
           wu => wu.w.ProjectId,
           p => p.ProjectId,
           (wu, p) => new WorkEntryViewModel
           {
               Id = wu.w.Id,
               UserId = wu.w.UserId,
               UserName = wu.u.UserName,
               CreatedAt = wu.w.CreatedAt,
               Description = wu.w.Description,
               TaskName = wu.w.TaskName,
               Status = wu.w.Status,
               TimeSpent = wu.w.TimeSpent,
               ProjectId = (int)wu.w.ProjectId,  
               ProjectName = p.ProjectName   
           })
     .AsQueryable();

            if (!userId.HasValue && !startDate.HasValue && !endDate.HasValue)
            {
                return View(new List<WorkEntryViewModel>());
            }

            if (userId.HasValue)
            {
                workEntries = workEntries.Where(t => t.UserId == userId.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                workEntries = workEntries.Where(t => t.CreatedAt >= start && t.CreatedAt <= end);
            }
            else if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = start.AddDays(1).AddTicks(-1);
                workEntries = workEntries.Where(t => t.CreatedAt >= start && t.CreatedAt <= end);
            }

            return View(await workEntries.ToListAsync());
        }


        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? userId, DateTime? startDate, DateTime? endDate)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var workEntries = _context.WorkEntries
                .Join(_context.userMasterEntitie,
                    w => w.UserId,
                    u => u.UserId,
                    (w, u) => new { w, u })
                .Join(_context.projectEntities,
                    wu => wu.w.ProjectId,
                    p => p.ProjectId,
                    (wu, p) => new WorkEntryViewModel
                    {
                        UserId = wu.w.UserId,
                        UserName = wu.u.UserName,
                        CreatedAt = wu.w.CreatedAt,
                        Description = wu.w.Description,
                        TaskName = wu.w.TaskName,
                        Status = wu.w.Status,
                        TimeSpent = wu.w.TimeSpent,
                        ProjectId = (int)wu.w.ProjectId,
                        ProjectName = p.ProjectName
                    })
                .AsQueryable();

            if (userId.HasValue)
            {
                workEntries = workEntries.Where(t => t.UserId == userId.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                workEntries = workEntries.Where(t => t.CreatedAt >= start && t.CreatedAt <= end);
            }
            else if (startDate.HasValue)
            {
                DateTime start = startDate.Value.Date;
                DateTime end = start.AddDays(1).AddTicks(-1);
                workEntries = workEntries.Where(t => t.CreatedAt >= start && t.CreatedAt <= end);
            }

            var data = await workEntries.ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tasks");

                // Header Row
                worksheet.Cells[1, 1].Value = "No.";
                worksheet.Cells[1, 2].Value = "User Name";
                worksheet.Cells[1, 3].Value = "Project Name";
                worksheet.Cells[1, 4].Value = "Task Name";
                worksheet.Cells[1, 5].Value = "Description";
                worksheet.Cells[1, 6].Value = "Status";
                worksheet.Cells[1, 7].Value = "Time Spent (hrs)";
                worksheet.Cells[1, 8].Value = "Created At";

                // Data Rows
                int row = 2;
                int counter = 1;  
                double totalHours = 0;

                foreach (var task in data)
                {
                    double hoursSpent = Math.Round(task.TimeSpent / 60.0, 2);
                    totalHours += hoursSpent;

                    worksheet.Cells[row, 1].Value = counter++; 
                    worksheet.Cells[row, 2].Value = task.UserName;
                    worksheet.Cells[row, 3].Value = task.ProjectName;
                    worksheet.Cells[row, 4].Value = task.TaskName;
                    worksheet.Cells[row, 5].Value = task.Description;
                    worksheet.Cells[row, 6].Value = task.Status;
                    worksheet.Cells[row, 7].Value = hoursSpent + " hrs";
                    worksheet.Cells[row, 8].Value = task.CreatedAt.ToString("yyyy-MM-dd HH:mm");

                    row++;
                }

                // Add total hours row
                worksheet.Cells[row, 6].Value = "Total Hours:";
                worksheet.Cells[row, 6].Style.Font.Bold = true;
                worksheet.Cells[row, 7].Value = totalHours + " hrs";
                worksheet.Cells[row, 7].Style.Font.Bold = true;
                worksheet.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Get username for file name
                string username = _context.userMasterEntitie
                    .Where(user => user.UserId == userId)
                    .Select(user => user.UserName)
                    .FirstOrDefault() ?? "Unknown_User";

                string safeUsername = username.Replace(" ", "_");
                string fileName = $"{safeUsername}_Tasks.xlsx";

                var stream = new MemoryStream(package.GetAsByteArray());

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }


        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> GetAlltasksByUsers()
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;

            var users = _context.userMasterEntitie
                .Where(u => u.UserTypeId == 3)
                .Select(u => new { u.UserId, u.UserName })
                .ToList();

            ViewBag.Users = new SelectList(users, "UserId", "UserName");

            var workEntries = await _context.WorkEntries
                .Join(_context.userMasterEntitie,
                      w => w.UserId,
                      u => u.UserId,
                      (w, u) => new { w, u }) 
                .Join(_context.projectEntities,
                      wu => wu.w.ProjectId,
                      p => p.ProjectId,
                      (wu, p) => new WorkEntryViewModel
                      {
                          Id = wu.w.Id,
                          UserId = wu.w.UserId,
                          UserName = wu.u.UserName,
                          CreatedAt = wu.w.CreatedAt,
                          Description = wu.w.Description,
                          TaskName = wu.w.TaskName,
                          Status = wu.w.Status,
                          TimeSpent = wu.w.TimeSpent,
                          ProjectId = (int)wu.w.ProjectId,   
                          ProjectName = p.ProjectName  
                      })
                .ToListAsync();

            return View(workEntries);
        }


        [HttpPost]
        public IActionResult FilterTaskss(int? userId, DateTime? startDate, DateTime? endDate)
        {
            var filteredTasks = _context.WorkEntries
                .Where(w =>
                    (!userId.HasValue || w.UserId == userId.Value) &&
                    (!startDate.HasValue || w.CreatedAt.Date >= startDate.Value.Date) &&
                    (!endDate.HasValue || w.CreatedAt.Date <= endDate.Value.Date))
                .Join(_context.userMasterEntitie, w => w.UserId, u => u.UserId, (w, u) => new WorkEntryViewModel
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    UserName = u.UserName,
                    CreatedAt = w.CreatedAt,
                    Description = w.Description,
                    TaskName = w.TaskName,
                    Status = w.Status,
                    TimeSpent = w.TimeSpent
                })
                .ToList();

            return PartialView("_TaskListPartial", filteredTasks);
        }

        public IActionResult FilterTasks(int? userId, DateTime? startDate, DateTime? endDate)
        {
            var filteredTasks = _context.WorkEntries
                .Where(w =>
                    (!userId.HasValue || w.UserId == userId.Value) &&
                    (!startDate.HasValue || w.CreatedAt.Date >= startDate.Value.Date) &&
                    (!endDate.HasValue || w.CreatedAt.Date <= endDate.Value.Date))
                .Join(_context.userMasterEntitie,
                      w => w.UserId,
                      u => u.UserId,
                      (w, u) => new { w, u })  
                .Join(_context.projectEntities,
                      wu => wu.w.ProjectId,
                      p => p.ProjectId,
                      (wu, p) => new WorkEntryViewModel
                      {
                          Id = wu.w.Id,
                          UserId = wu.w.UserId,
                          UserName = wu.u.UserName,
                          CreatedAt = wu.w.CreatedAt,
                          Description = wu.w.Description,
                          TaskName = wu.w.TaskName,
                          Status = wu.w.Status,
                          TimeSpent = wu.w.TimeSpent,
                          ProjectId = (int)wu.w.ProjectId,  
                          ProjectName = p.ProjectName 
                      })
                .ToList();

            var taskSummaryHtml = RenderPartialViewToString("_timestamp", filteredTasks);
            var taskListHtml = RenderPartialViewToString("_TaskListPartial", filteredTasks);

            return Json(new { taskSummary = taskSummaryHtml, taskList = taskListHtml });
        }



        private string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.Success)
                {
                    var viewContext = new ViewContext(
                        ControllerContext,
                        viewResult.View,
                        ViewData,
                        TempData,
                        sw,
                        new HtmlHelperOptions()
                    );

                    viewResult.View.RenderAsync(viewContext).Wait();
                    return sw.ToString();
                }
                else
                {
                    throw new Exception($"View '{viewName}' not found.");
                }
            }
        }


        [HttpPost]
        public IActionResult SaveTask([FromBody] WorkEntryEntity task)
        {
             LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            if (ModelState.IsValid)
            {
                task.UserId = lr.userId;

                // DateTimeOffset utcNow = DateTimeOffset.UtcNow;

                // TimeZoneInfo indianZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                //DateTime indianTime = TimeZoneInfo.ConvertTime(utcNow, indianZone).DateTime;

                 task.CreatedAt = DateTime.Now;

                 _context.WorkEntries.Add(task);
                _context.SaveChanges();

                return Json(new { success = true, message = "Task saved successfully!" });
            }

            return Json(new { success = false, message = "Invalid task data!" });
        }



        [HttpPost]
        public IActionResult DeleteTask(int id)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.UserId = lr.userId;
            var task = _context.WorkEntries.FirstOrDefault(t => t.Id == id && t.UserId == lr.userId);
            if (task != null)
            {
                _context.WorkEntries.Remove(task);
                _context.SaveChanges();
                return Json(new { success = true, message = "Record deleted successfully!" });
            }
            return Json(new { success = false, message = "Fail to deleted!" });
            
        }









    }
}
