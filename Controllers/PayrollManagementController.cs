using Microsoft.AspNetCore.Mvc;

namespace AttendanceCRM.Controllers
{
    public class PayrollManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
