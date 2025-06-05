using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceCRM.Controllers
{
    public class TasksController : Controller
    {
        private readonly MyDbContext _context;

        public TasksController(MyDbContext context)
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
            var tasks =  _context.taskEntities
                .Where(t => !t.IsDeleted)
                .Select(t => new TaskViewModel
                {
                    TaskId = t.TaskId,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    TimeSpent = t.TimeSpent,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    ProjectName = t.Project.ProjectName,
                    AssignedToUser = t.AssignedToUser.UserName,
                    AssignedByUser = t.AssignedByUser.UserName
                })
                .ToList();

            return View(tasks);
        }

        // 2️⃣ CREATE TASK (GET)
        //public IActionResult Create()
        //{

        //    ViewData["Projects"] = new SelectList(_context.projectEntities, "ProjectId", "ProjectName");
        //    ViewData["Users"] = new SelectList(_context.userMasterEntitie, "UserId", "UserName");
        //    return View();
        //}
        public IActionResult Create()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");

            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            ViewBag.UserDetails = lr;
            ViewBag.Projects = new SelectList(_context.projectEntities, "ProjectId", "ProjectName");
            ViewBag.Users = new SelectList(_context.userMasterEntitie, "UserId", "UserName");
            ViewBag.Sprints = new SelectList(_context.sprintEntities, "SprintId", "SprintName");
            ViewBag.CurrentUserId = lr.userId;  

            return View();
        }



        // 3️⃣ CREATE TASK (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskEntity task)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            
                task.AssignedBy = lr.userId;
                task.CreatedOn = DateTime.Now;
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
          
            return View(task);
        }

      
        public IActionResult Edit(int id)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;

			var task = _context.taskEntities.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.UserDetails = lr;
            ViewBag.Projects = new SelectList(_context.projectEntities, "ProjectId", "ProjectName", task.ProjectId);
            ViewBag.Users = new SelectList(_context.userMasterEntitie, "UserId", "UserName", task.AssignedTo);
            ViewBag.Sprints = new SelectList(_context.sprintEntities, "SprintId", "SprintName", task.SprintId);
            ViewBag.CurrentUserId = lr.userId; // Ensure this is a property, not a method

            return View(task);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TaskEntity task)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;

			if (id != task.TaskId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var existingTask = _context.taskEntities.Find(id);
                if (existingTask == null)
                {
                    return NotFound();
                }

                // Update task properties
                existingTask.TaskName = task.TaskName;
                existingTask.Description = task.Description;
                existingTask.ProjectId = task.ProjectId;
                existingTask.AssignedTo = task.AssignedTo;
                existingTask.Status = task.Status;
                existingTask.TimeSpent = task.TimeSpent;
                existingTask.UpdatedBy = lr.userId; // Assign current logged-in user
                existingTask.UpdatedOn = DateTime.Now;

                _context.Update(existingTask);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Projects = new SelectList(_context.projectEntities, "ProjectId", "ProjectName", task.ProjectId);
            ViewBag.Users = new SelectList(_context.userMasterEntitie, "UserId", "UserName", task.AssignedTo);
            ViewBag.Sprints = new SelectList(_context.sprintEntities, "SprintId", "SprintName", task.SprintId);
            ViewBag.CurrentUserId = lr.userId;

            return View(task);
        }


        // 6️⃣ DELETE TASK (GET)
        public async Task<IActionResult> Delete(int id)
        {
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			if (lr == null)
			{
				return RedirectToAction("Login", "Authenticate");
			}
			ViewBag.UserDetails = lr;
			var task =   _context.taskEntities
                .Select(t => new TaskViewModel
                {
                    TaskId = t.TaskId,
                    TaskName = t.TaskName,
                    ProjectName = t.Project.ProjectName,
                    AssignedToUser = t.AssignedToUser.UserName,
                    AssignedByUser = t.AssignedByUser.UserName
                })
                .FirstOrDefault(t => t.TaskId == id);

            if (task == null) return NotFound();
            return View(task);
        }

        // 7️⃣ DELETE TASK (POST)
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
			var task = await _context.taskEntities.FindAsync(id);
            if (task != null)
            {
                task.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
