using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AttendanceCRM.Controllers
{
    public class SprintsController : Controller
    {
        private readonly MyDbContext _context;

        public SprintsController(MyDbContext context)
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
            var sprints =  _context.sprintEntities
                .Where(s => !s.IsDeleted)
                .Select(s => new SprintViewModel
                {
                    SprintId = s.SprintId,
                    SprintName = s.SprintName,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Status = s.Status,
                    ProjectName = s.Project.ProjectName
                })
                .ToList();

            return View(sprints);
        }

        [Authorize] 
        [Authorize(Policy = "Hr")]
        public async Task<IActionResult> MySprints()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            
            var userSprints =  _context.sprintEntities
                .Where(s => s.AssignedTo == lr.userId) 
                .ToList();

            return View(userSprints);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            if (newStatus != "Active" && newStatus != "Completed")
            {
                return BadRequest("Invalid status update.");
            }

            var sprint = await _context.sprintEntities.FindAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }

             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (sprint.AssignedTo != lr.userId)
            {
                return Unauthorized("You are not allowed to update this sprint.");
            }

            sprint.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MySprints));  
        }



        // 2️⃣ CREATE (GET)
        public IActionResult Create()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            //ViewData["Projects"] = new SelectList(_context.projectEntities, "ProjectId", "ProjectName");
            ViewBag.Projects = new SelectList(_context.projectEntities, "ProjectId", "ProjectName");
            ViewBag.Users = new SelectList(_context.userMasterEntitie, "UserId", "UserName");
            return View();
        }

        // 3️⃣ CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SprintEntity sprint)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            sprint.CreatedBy = lr.userId;
                sprint.CreatedOn = DateTime.Now;
                _context.Add(sprint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            
            return View(sprint);
        }

        // 4️⃣ EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            var sprint = await _context.sprintEntities.FindAsync(id);
            if (sprint == null) return NotFound();

            ViewData["Projects"] = new SelectList(_context.projectEntities, "ProjectId", "ProjectName", sprint.ProjectId);
            return View(sprint);
        }

        // 5️⃣ EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SprintEntity sprint)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            if (id != sprint.SprintId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(sprint);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sprint);
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
            {
                return NotFound();
            }

            var sprint =  (from s in _context.sprintEntities
                                join p in _context.projectEntities on s.ProjectId equals p.ProjectId
                                where s.SprintId == id
                                select new SprintViewModel
                                {
                                    SprintId = s.SprintId,
                                    SprintName = s.SprintName,
                                    ProjectName = p.ProjectName,
                                    StartDate = s.StartDate,
                                    EndDate = s.EndDate,
                                    Status = s.Status
                                }).FirstOrDefault();

            if (sprint == null)
            {
                return NotFound();
            }

            return View(sprint);
        }


        // 6️⃣ DELETE (GET)
        public async Task<IActionResult> Delete(int id)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            var sprint =  _context.sprintEntities
                .Select(s => new SprintViewModel
                {
                    SprintId = s.SprintId,
                    SprintName = s.SprintName,
                    ProjectName = s.Project.ProjectName
                })
                .FirstOrDefault(s => s.SprintId == id);

            if (sprint == null) return NotFound();
            return View(sprint);
        }

        // 7️⃣ DELETE (POST)
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
            var sprint = await _context.sprintEntities.FindAsync(id);
            if (sprint != null)
            {
                sprint.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }

}
