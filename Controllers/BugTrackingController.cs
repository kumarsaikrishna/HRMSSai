using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceCRM.Controllers
{
    public class BugTrackingController : Controller
    {
        private readonly MyDbContext _context;

        public BugTrackingController(MyDbContext context)
        {
            _context = context;
        }

		[Authorize(Policy = "HRAccess")]
		public async Task<IActionResult> Index()
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			var bugs =  (from bug in _context.bugTrackings
                              join project in _context.projectEntities on bug.ProjectId equals project.ProjectId
                              join task in _context.taskEntities on bug.TaskId equals task.TaskId
                              join reporter in _context.userMasterEntitie on bug.ReportedBy equals reporter.UserId
                              join assignee in _context.userMasterEntitie on bug.AssignedTo equals assignee.UserId into assignees
                              from assignee in assignees.DefaultIfEmpty()
                              select new BugViewModel
                              {
                                  BugId = bug.BugId,
                                  ProjectName = project.ProjectName,
                                  TaskName = task.TaskName,
                                  ReportedBy = reporter.UserName,
                                  AssignedTo = assignee != null ? assignee.UserName : "Unassigned",
                                  BugDescription = bug.BugDescription,
                                  Priority = bug.Priority,
                                  Status = bug.Status,
                                  CreatedOn = bug.CreatedOn
                              }).ToList();

            return View(bugs);
        }

         public IActionResult Create()
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			ViewBag.Projects = _context.projectEntities.ToList();
            ViewBag.Tasks = _context.taskEntities.ToList();
            ViewBag.Users = _context.userMasterEntitie.ToList();
            return View();
        }

         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BugTracking bug)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			 
                _context.bugTrackings.Add(bug);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
             
            return View(bug);
        }

         public async Task<IActionResult> Edit(int id)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			var bug = await _context.bugTrackings.FindAsync(id);
            if (bug == null) return NotFound();

            ViewBag.Projects = _context.projectEntities.ToList();
            ViewBag.Tasks = _context.taskEntities.ToList();
            ViewBag.Users = _context.userMasterEntitie.ToList();
            return View(bug);
        }

         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BugTracking bug)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			if (id != bug.BugId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(bug);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bug);
        }

         public async Task<IActionResult> Delete(int id)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			var bug = await _context.bugTrackings.FindAsync(id);
            if (bug == null) return NotFound();

            return View(bug);
        }

         [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			var bug = await _context.bugTrackings.FindAsync(id);
            if (bug != null)
            {
                _context.bugTrackings.Remove(bug);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
