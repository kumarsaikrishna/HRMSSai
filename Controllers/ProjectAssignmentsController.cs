using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceCRM.Controllers
{
    public class ProjectAssignmentsController : Controller
    {
        private readonly MyDbContext _context;

        public ProjectAssignmentsController(MyDbContext context)
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
            var assignments = from pa in _context.ProjectAssignments
                              join p in _context.projectEntities on pa.ProjectId equals p.ProjectId
                              join u in _context.userMasterEntitie on pa.TeamLeadId equals u.UserId
                              select new ProjectAssignment
                              {
                                  AssignmentId = pa.AssignmentId,
                                  ProjectId = pa.ProjectId,
                                  TeamLeadId = pa.TeamLeadId,
                                  AssignedOn = pa.AssignedOn,
                                  Project = p,
                                  TeamLead = u
                              };
            return View(await assignments.ToListAsync());
        }

         public async Task<IActionResult> Details(int? id)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            if (id == null)
                return NotFound();

            var assignment = await (from pa in _context.ProjectAssignments
                                    join p in _context.projectEntities on pa.ProjectId equals p.ProjectId
                                    join u in _context.userMasterEntitie on pa.TeamLeadId equals u.UserId
                                    where pa.AssignmentId == id
                                    select new ProjectAssignment
                                    {
                                        AssignmentId = pa.AssignmentId,
                                        ProjectId = pa.ProjectId,
                                        TeamLeadId = pa.TeamLeadId,
                                        AssignedOn = pa.AssignedOn,
                                        Project = p,
                                        TeamLead = u
                                    }).FirstOrDefaultAsync();
            if (assignment == null)
                return NotFound();

            return View(assignment);
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
 
            var projects = _context.projectEntities.ToList();
			var users = _context.userMasterEntitie.ToList();

			// Debugging: Check if data exists
			if (projects == null || !projects.Any())
			{
				ViewData["Projects"] = new List<ProjectEntity>(); // Prevent null reference error
			}
			else
			{
				ViewData["Projects"] = projects;
			}

			if (users == null || !users.Any())
			{
				ViewData["Users"] = new List<UserMasterEntitie>(); // Prevent null reference error
			}
			else
			{
				ViewData["Users"] = users;
			}

			return View();
		}




		// POST: ProjectAssignments/Create
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,TeamLeadId")] ProjectAssignment assignment)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            assignment.CreatedBy = lr.userId;
                assignment.AssignedOn = DateTime.Now;
                _context.Add(assignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           
            return View(assignment);
        }

        // GET: ProjectAssignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var assignment = await _context.ProjectAssignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            ViewData["Projects"] = _context.projectEntities.ToList();
            ViewData["Users"] = _context.userMasterEntitie.ToList();
            return View(assignment);
        }

         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AssignmentId,ProjectId,TeamLeadId")] ProjectAssignment assignment)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            if (id != assignment.AssignmentId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ProjectAssignments.Any(e => e.AssignmentId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(assignment);
        }

         
        public async Task<IActionResult> Delete(int? id)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            if (id == null)
                return NotFound();

            var assignment = await (from pa in _context.ProjectAssignments
                                    join p in _context.projectEntities on pa.ProjectId equals p.ProjectId
                                    join u in _context.userMasterEntitie on pa.TeamLeadId equals u.UserId
                                    where pa.AssignmentId == id
                                    select new ProjectAssignment
                                    {
                                        AssignmentId = pa.AssignmentId,
                                        ProjectId = pa.ProjectId,
                                        TeamLeadId = pa.TeamLeadId,
                                        AssignedOn = pa.AssignedOn,
                                        Project = p,
                                        TeamLead = u
                                    }).FirstOrDefaultAsync();
            if (assignment == null)
                return NotFound();

            return View(assignment);
        }

         [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.ProjectAssignments.FindAsync(id);
            if (assignment != null)
            {
                _context.ProjectAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
