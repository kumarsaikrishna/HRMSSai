using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AttendanceCRM.Controllers
{
    public class TeamMembersController : Controller
    {
        private readonly MyDbContext _context;

        public TeamMembersController(MyDbContext context)
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
            var teamMembers = from tm in _context.teamMemberEntities
                              join p in _context.projectEntities on tm.ProjectId equals p.ProjectId
                              join tl in _context.userMasterEntitie on tm.TeamLeadId equals tl.UserId
                              join m in _context.userMasterEntitie on tm.MemberId equals m.UserId
                              where !tm.IsDeleted
                              select new TeamMemberEntity
                              {
                                  TeamMemberId = tm.TeamMemberId,
                                  ProjectId = tm.ProjectId,
                                  TeamLeadId = tm.TeamLeadId,
                                  MemberId = tm.MemberId,
                                  Role = tm.Role,
                                  CreatedBy = tm.CreatedBy,
                                  UpdatedBy = tm.UpdatedBy,
                                  CreatedOn = tm.CreatedOn,
                                  UpdatedOn = tm.UpdatedOn,
                                  IsDeleted = tm.IsDeleted,
                                  AssignedOn = tm.AssignedOn,
                                  Project = p,
                                  TeamLead = tl,
                                  Member = m
                              };
            return View(await teamMembers.ToListAsync());
        }

        // GET: TeamMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            if (id == null) return NotFound();

            var teamMember = await (from tm in _context.teamMemberEntities
                                    join p in _context.projectEntities on tm.ProjectId equals p.ProjectId
                                    join tl in _context.userMasterEntitie on tm.TeamLeadId equals tl.UserId
                                    join m in _context.userMasterEntitie on tm.MemberId equals m.UserId
                                    where tm.TeamMemberId == id
                                    select new TeamMemberEntity
                                    {
                                        TeamMemberId = tm.TeamMemberId,
                                        ProjectId = tm.ProjectId,
                                        TeamLeadId = tm.TeamLeadId,
                                        MemberId = tm.MemberId,
                                        Role = tm.Role,
                                        CreatedBy = tm.CreatedBy,
                                        UpdatedBy = tm.UpdatedBy,
                                        CreatedOn = tm.CreatedOn,
                                        UpdatedOn = tm.UpdatedOn,
                                        IsDeleted = tm.IsDeleted,
                                        AssignedOn = tm.AssignedOn,
                                        Project = p,
                                        TeamLead = tl,
                                        Member = m
                                    }).FirstOrDefaultAsync();

            if (teamMember == null) return NotFound();
            return View(teamMember);
        }

        // GET: TeamMembers/Create
        public IActionResult Create()
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            // Fetching projects and users from database
            var projects = _context.projectEntities
                .Select(p => new { p.ProjectId, p.ProjectName })
                .ToList();

            var users = _context.userMasterEntitie
                .Select(u => new { u.UserId, u.UserName })
                .ToList();

            // Storing lists in ViewData
            ViewData["Projects"] = new SelectList(projects, "ProjectId", "ProjectName");
            ViewData["Users"] = new SelectList(users, "UserId", "UserName");

            return View();
        }


        
        [HttpPost]
        public async Task<IActionResult> Create([Bind("ProjectId,TeamLeadId,MemberId,Role")] TeamMemberEntity teamMember)
        {
            var lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            
                teamMember.CreatedOn = DateTime.Now;  
                teamMember.AssignedOn = DateTime.Now;  
                _context.Add(teamMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
           

            
            ViewData["Projects"] = new SelectList(_context.projectEntities, "ProjectId", "ProjectName", teamMember.ProjectId);
            ViewData["Users"] = new SelectList(_context.userMasterEntitie, "UserId", "UserName", teamMember.TeamLeadId);

            return View(teamMember);
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
            var teamMember = _context.teamMemberEntities.Find(id);
            if (teamMember == null)
            {
                return NotFound();
            }

            // Fetch Projects
            var projects = _context.projectEntities
                .Select(p => new { p.ProjectId, p.ProjectName })
                .ToList();

            // Fetch Users (for both Team Lead & Member)
            var users = _context.userMasterEntitie
                .Select(u => new { u.UserId, u.UserName })
                .ToList();

            // Pass data as SelectList
            ViewData["Projects"] = new SelectList(projects, "ProjectId", "ProjectName", teamMember.ProjectId);
            ViewData["Users"] = new SelectList(users, "UserId", "UserName", teamMember.TeamLeadId);

            return View(teamMember);
        }


        // POST: TeamMembers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamMemberId,ProjectId,TeamLeadId,MemberId,Role,UpdatedBy")] TeamMemberEntity teamMember)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;
            if (id != teamMember.TeamMemberId) return NotFound();

           
                try
                {
                    teamMember.UpdatedOn = DateTime.Now;
                    _context.Update(teamMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.teamMemberEntities.Any(e => e.TeamMemberId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            
            return View(teamMember);
        }

        // GET: TeamMembers/Delete/5
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

            var teamMember = await (from tm in _context.teamMemberEntities
                                    join p in _context.projectEntities on tm.ProjectId equals p.ProjectId
                                    join tl in _context.userMasterEntitie on tm.TeamLeadId equals tl.UserId
                                    join m in _context.userMasterEntitie on tm.MemberId equals m.UserId
                                    where tm.TeamMemberId == id
                                    select new TeamMemberEntity
                                    {
                                        TeamMemberId = tm.TeamMemberId,
                                        ProjectId = tm.ProjectId,
                                        TeamLeadId = tm.TeamLeadId,
                                        MemberId = tm.MemberId,
                                        Role = tm.Role,
                                        CreatedBy = tm.CreatedBy,
                                        UpdatedBy = tm.UpdatedBy,
                                        CreatedOn = tm.CreatedOn,
                                        UpdatedOn = tm.UpdatedOn,
                                        IsDeleted = tm.IsDeleted,
                                        AssignedOn = tm.AssignedOn,
                                        Project = p,
                                        TeamLead = tl,
                                        Member = m
                                    }).FirstOrDefaultAsync();

            if (teamMember == null) return NotFound();
            return View(teamMember);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teamMember = await _context.teamMemberEntities.FindAsync(id);
            if (teamMember != null)
            {
                teamMember.IsDeleted = true;
                _context.Update(teamMember);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
