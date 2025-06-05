//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;

//namespace AttendanceCRM.Controllers
//{
//    public class DashboardController : Controller
//    {
//        public IActionResult Index()
//        {
//            var role = User.FindFirst(ClaimTypes.Role)?.Value;

//            if (role == "Admin")
//                return RedirectToAction("AdminDashboard");
//            if (role == "Project Manager")
//                return RedirectToAction("ProjectManagerDashboard");
//            if (role == "Developer")
//                return RedirectToAction("DeveloperDashboard");
//            if (role == "QA")
//                return RedirectToAction("QADashboard");

//            return RedirectToAction("AccessDenied");
//        }

//        [Authorize(Roles = "Admin")]
//        public IActionResult AdminDashboard() => View();

//        [Authorize(Roles = "Project Manager")]
//        public IActionResult ProjectManagerDashboard() => View();

//        [Authorize(Roles = "Developer")]
//        public IActionResult DeveloperDashboard() => View();

//        [Authorize(Roles = "QA")]
//        public IActionResult QADashboard() => View();

//        public IActionResult AccessDenied() => View();
//    }
//}
