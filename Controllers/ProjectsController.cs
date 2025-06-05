using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceCRM.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly MyDbContext _context;

        public ProjectsController(MyDbContext context)
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

            return View( _context.projectEntities.Where(p => !p.IsDeleted).ToList());
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectEntity project)
        {
            // Check if the user is logged in
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            if (ModelState.IsValid)
            {
                // Ensure only the date (time set to 00:00:00) is stored
                project.StartDate = DateOnly.FromDateTime(DateTime.Today); // Store only the date

                if (project.EndDate.HasValue)
                {
                    project.EndDate = DateOnly.FromDateTime(project.EndDate.Value.ToDateTime(TimeOnly.MinValue));
                }

                // Set created/updated timestamps
                project.CreatedOn = DateTime.Now;
                project.UpdatedOn = DateTime.Now;

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }






        public async Task<IActionResult> Edit(int? id)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            if (id == null) return NotFound();

            var project = await _context.projectEntities.FindAsync(id);
            if (project == null) return NotFound();

            return View(project);
        }

      

        [HttpPost]
        public async Task<IActionResult> Edit(int ProjectId, ProjectEntity project)
        {
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            if (ProjectId != project.ProjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Fetch the existing project from the database
                var existingProject = await _context.projectEntities.FindAsync(ProjectId);
                if (existingProject == null)
                {
                    return NotFound();
                }

                // ✅ Update only the necessary fields
                existingProject.ProjectName = project.ProjectName;
                existingProject.ClientName = project.ClientName;
                existingProject.StartDate = project.StartDate; // Ensure only date is stored
                existingProject.EndDate = project.EndDate;
                existingProject.Status = project.Status;
                existingProject.Budget = project.Budget;
                existingProject.Description = project.Description;
               
                existingProject.UpdatedOn = DateTime.Now;
                existingProject.UpdatedBy = lr.userId; // Assuming you store the logged-in user ID

                // ✅ Save changes
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(project);
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
            if (id == null) return NotFound();

            var project = await _context.projectEntities.FindAsync(id);
            if (project == null) return NotFound();

            return View(project);
        }
 
       

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = new GenericResponse();
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            var project = await _context.projectEntities.FindAsync(id);
            if (project != null)
            {
                project.IsDeleted = true;
                await _context.SaveChangesAsync();

                // Set response as success
                response.statuCode = 200;
                response.message = "Project deleted successfully.";
            }
            else
            {
                response.statuCode = 400; // Bad request
                response.message = "Project not found.";
            }

            return Json(response);
        }

    }
}
