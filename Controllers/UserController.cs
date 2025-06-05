using AttendanceCRM.BAL.IServices;
using AttendanceCRM.Utilities;
using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml; // For EPPlus library
using ClosedXML.Excel; // For Excel export
using iTextSharp.text; // For PDF export
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace AttendanceCRM.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _Service;
        private readonly IMasterMgmtService _mService;
        private readonly MyDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;


        public UserController(IUserService Service, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, IMasterMgmtService mService, MyDbContext context)
        {
            _Service = Service;
			_mService = mService;
           _context = context;
            _hostingEnvironment = hostingEnvironment;
          
        }
        public async Task<IActionResult> counts()
        {
            int TotalEmplyeecount = _Service.TotalEmplyee();
            int TotalEmplyeeActiveCount = _Service.TotalEmplyeeActive();
            int TotalEmplyeeInActiveCount = _Service.TotalEmplyeeInActive();
            int TotalEmplyeeNewJoinCount = _Service.TotalEmplyeeNewJoin();
            ViewBag.TotalEmplyeecount = TotalEmplyeecount;
            ViewBag.TotalEmplyeeActiveCount = TotalEmplyeeActiveCount;
            ViewBag.TotalEmplyeeInActiveCount = TotalEmplyeeInActiveCount;
            ViewBag.TotalEmplyeeNewJoinCount = TotalEmplyeeNewJoinCount;
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

     
            var utypes = _mService.UserTypeList().ToList();

            ViewBag.utypes = utypes;
            return View();
        }

        [Authorize(Policy = "AdminAccess")]

        public IActionResult UserList(string searchTerm, int page = 1, int pageSize = 10)
        {
            // Check user authentication
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set view bag details
            ViewBag.UserDetails = lr;
            ViewBag.Username = lr.userName;
            counts();
            // Retrieve all leaves
            var allLeaves = _Service.UserList();

            // Apply comprehensive search across multiple fields
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                allLeaves = allLeaves.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = allLeaves
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = allLeaves.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)allLeaves.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserListTable", paginatedLeaves);
            }

            return View(paginatedLeaves);
        }




        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> UserListadmin(string searchTerm, int page = 1, int pageSize = 10)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }
            ViewBag.UserDetails = lr;

            counts();
            var allLeaves = _Service.UserList().ToList();
            // Apply comprehensive search across multiple fields
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                allLeaves = allLeaves.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = allLeaves
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = allLeaves.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)allLeaves.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserListAdmin", paginatedLeaves);
            }

            return View(paginatedLeaves);
        }

        [Authorize(Policy = "AdminAccess")]

        public IActionResult UserListActive(string searchTerm, int page = 1, int pageSize = 10)
        {
            // Check user authentication
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set view bag details
            ViewBag.UserDetails = lr;
            ViewBag.Username = lr.userName;
            counts();
            // Retrieve all leaves
            var allLeaves = _Service.UserListActive();

            // Apply comprehensive search across multiple fields
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                allLeaves = allLeaves.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = allLeaves
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = allLeaves.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)allLeaves.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserListTable", paginatedLeaves);
            }

            return View(paginatedLeaves);
        }



        [Authorize(Policy = "AdminAccess")]

        public IActionResult UserListInActive(string searchTerm, int page = 1, int pageSize = 10)
        {
            // Check user authentication
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set view bag details
            ViewBag.UserDetails = lr;
            ViewBag.Username = lr.userName;
            counts();
            // Retrieve all leaves
            var allLeaves = _Service.UserListInActive();

            // Apply comprehensive search across multiple fields
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                allLeaves = allLeaves.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = allLeaves
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = allLeaves.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)allLeaves.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserListTable", paginatedLeaves);
            }

            return View(paginatedLeaves);
        }

        [Authorize(Policy = "AdminAccess")]

        public IActionResult UserListNewJoin(string searchTerm, int page = 1, int pageSize = 10)
        {
            // Check user authentication
            LoginResponseModel lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            if (lr == null)
            {
                return RedirectToAction("Login", "Authenticate");
            }

            // Set view bag details
            ViewBag.UserDetails = lr;
            ViewBag.Username = lr.userName;
            counts();
            // Retrieve all leaves
            var allLeaves = _Service.UserListNewJoin();

            // Apply comprehensive search across multiple fields
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower().Trim();
                allLeaves = allLeaves.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            // Sort and paginate
            var paginatedLeaves = allLeaves
                .OrderByDescending(x => x.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = allLeaves.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)allLeaves.Count() / pageSize);
            ViewBag.SearchTerm = searchTerm;

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_UserListTable", paginatedLeaves);
            }

            return View(paginatedLeaves);
        }

      
        public IActionResult AddUser()
		{
			UserMasterDTO um = new UserMasterDTO();
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
		
			var em=_Service.UserList().ToList();
			//var utypes = _mService.UserTypeList().ToList();
			//ViewBag.utypes = utypes;

			return View(um);


		}

        [HttpPost]
        public IActionResult AddUser(UserMasterDTO model)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            if (lr == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                if (ModelState.IsValid)
                {
                   
                    var existingUser = _context.userMasterEntitie
                        .FirstOrDefault(u => u.EmployeeId == model.EmployeeId ||
                                             u.Email == model.Email ||
                                             u.MobileNumber == model.MobileNumber);

                    if (existingUser != null)
                    {
                        
                        string duplicateField = "";
                        if (existingUser.EmployeeId == model.EmployeeId) duplicateField = "EmployeeId";
                        if (existingUser.Email == model.Email) duplicateField = "Email";
                        if (existingUser.MobileNumber == model.MobileNumber) duplicateField = "MobileNumber";

                        ViewBag.ErrorMessage = $"{duplicateField} already exists. Please use a unique value.";
                        return View(model);
                    }

                   
                    string generatedPassword = GenerateCustomPassword(model.EmployeeId);

                   
                    UserMasterEntitie u = new UserMasterEntitie();
                    CloneObjects.CopyPropertiesTo(model, u);
                    u.IsDeleted = false;
                    u.CreatedBy = lr.userId;
                    u.EmployeeId = model.EmployeeId;
                    u.UserName = model.UserName;
                    u.Email = model.Email;
                    u.MobileNumber = model.MobileNumber;
                    u.Designation = model.Designation;
                    u.DateOfBirth = model.DateOfBirth;
                    u.DateOfJoining = model.DateOfJoining;
                    u.Password = EncryptModel.Encrypt(generatedPassword);
                    u.CreatedOn = DateTime.Now;
					u.IsDeleted = false;
					u.IsActive = true;
					u.UserTypeId = _context.userTypeMasterEntitie.Where(a => a.UserTypeName == "Employee" && a.IsDeleted == false).Select(a => a.UserTypeId).FirstOrDefault();
					u.StatusId = _context.employeeStatusEntity
	.Where(a => a.StatusName == "Probation").Select(a => a.StatusId).FirstOrDefault();
					u.CreatedBy = lr.userId;


					SendEmail(model.Email, generatedPassword);

                
                    var res = _Service.CreateUser(u);
                    if (res.statuCode == 1)
                    {
                        return RedirectToAction("UserList");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = res.message;
                    }
                }
            }
            catch (Exception e)
            {
                // Log the exception (optional)
                ViewBag.ErrorMessage = "An error occurred while adding the user.";
            }

            return View(model);
        }




        public IActionResult EditUser(int userid)
		{
			UserMasterDTO um = new UserMasterDTO();
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			ViewBag.UserDetails = lr;

			if (lr == null)
			{
				return RedirectToAction("Login");
			}

			var UserDeatils=_Service.GetUserById(userid);
            CloneObjects.CopyPropertiesTo(UserDeatils, um);
            ViewBag.deptlist = new SelectList(_context.department, "DepartmentID", "DepartmentName");
          

			return View(um);
		}
		public IActionResult DeatilUser(int userid)
		{
			UserMasterDTO um = new UserMasterDTO();
			LoginResponseModel lr = new LoginResponseModel();
			lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
			ViewBag.UserDetails = lr;

			if (lr == null)
			{
				return RedirectToAction("Login");
			}

			var UserDeatils = _Service.GetUserById(userid);

			// Decrypt the password before setting it in DTO
			UserDeatils.Password = EncryptModel.Decrypt(UserDeatils.Password);

			CloneObjects.CopyPropertiesTo(UserDeatils, um);

			return View(um);
		}


        [HttpPost]
        public IActionResult EditUser(UserMasterDTO model, IFormFile ProfileImgUploaded)
        {
            LoginResponseModel lr = new LoginResponseModel();
            lr = SessionHelper.GetObjectFromJson<LoginResponseModel>(HttpContext.Session, "loggedUser");
            ViewBag.UserDetails = lr;

            bool isProfilePicUploaded = false;
            string ProfilePic = "";

            // Check for email existence
            var checkEmail = _Service.UserList().Where(a => a.EmployeeId == model.EmployeeId && a.UserId != model.UserId).FirstOrDefault();
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

            UserMasterEntitie u = new UserMasterEntitie();
            u = _Service.GetUserById(model.UserId);

            // If AdharNumber is not provided, leave it as is (null allowed)
            if (model.AdharNumber != null)
            {
                u.AdharNumber = model.AdharNumber;
            }

            // If PanNumber is not provided, leave it as is (null allowed)
            if (model.PanNumber != null)
            {
                u.PanNumber = model.PanNumber;
            }

            // Now update other user fields regardless of the above AdharNumber/PanNumber check
            u.ProfilePicture = ProfilePic;
            u.EmployeeId = model.EmployeeId;
            u.UserName = model.UserName;
            u.DateOfJoining = model.DateOfJoining;
            u.DateOfBirth = model.DateOfBirth;
            u.UserTypeId = u.UserTypeId; // Seems like you might want to update UserTypeId as well if needed
            u.Address = model.Address;
            u.MobileNumber = model.MobileNumber;
            u.GuardianNumber = model.GuardianNumber;
            u.CollegeName = model.CollegeName;
            u.Designation = model.Designation;
            u.DepartmentId = model.DepartmentId;

            // Check for duplicates based on EmployeeId, AdharNumber, and PanNumber, excluding null values
            int count = _context.userMasterEntitie
                                .Where(a => a.EmployeeId == u.EmployeeId && a.IsDeleted == false)
                                .Count();

            // Check if AdharNumber is provided (not null)
            int count1 = 0;
            if (!string.IsNullOrEmpty(u.AdharNumber))
            {
                count1 = _context.userMasterEntitie
                                .Where(a => a.AdharNumber == u.AdharNumber && a.IsDeleted == false)
                                .Count();
            }

            // Check if PanNumber is provided (not null)
            int count2 = 0;
            if (!string.IsNullOrEmpty(u.PanNumber))
            {
                count2 = _context.userMasterEntitie
                                .Where(a => a.PanNumber == u.PanNumber && a.IsDeleted == false)
                                .Count();
            }

            // Ensure that there are no duplicates for EmployeeId, AdharNumber, or PanNumber
            if (count < 2 && count1 < 2 && count2 < 2)
            {
                var res = _Service.UpdateUser(u);
                if (res.statuCode == 1)
                {
                    return RedirectToAction("UserList");
                }
                else
                {
                    ViewBag.ErrorMessage = res.message;
                }
            }
            else
            {
                // Handle the case when duplicates are found.
                ViewBag.ErrorMessage = "Duplicate records found for the given EmployeeId, AdharNumber, or PanNumber.";
            }

            return View(model); // Return the model to the view in case of error or invalid data
        }



      
      

        public async Task<UserMasterEntitie> GetUserById(int id)
        {
            UserMasterEntitie obj = new UserMasterEntitie();
            obj = _Service.GetUserById(id);
            return obj;
        }




        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.userMasterEntitie.FirstOrDefault(x => x.UserId == id);
            if (user != null)
            {
                user.IsDeleted = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }



        [HttpPost]
		public ActionResult AddUserBulkUpload(IFormFile file)
		{
			DateTime? nullableDateTime = null;
			List<UserMasterEntitie> users = new List<UserMasterEntitie>();
			GenericResponse res = new GenericResponse();

			try
			{
				if (file != null && file.Length > 0)
				{
					string fileName = Path.GetFileName(file.FileName);
					string ext = Path.GetExtension(fileName);
					if (ext.ToLower() != ".xls" && ext.ToLower() != ".xlsx")
						throw new Exception("Invalid file format. Please upload an Excel file (.xls or .xlsx).");

					using (var stream = file.OpenReadStream())
					{
						ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

						using (var package = new ExcelPackage(stream))
						{
							ExcelWorksheet currentSheet = package.Workbook.Worksheets[0];
							bool hasHeader = true;
							int noOfRows = currentSheet.Dimension.End.Row;

							int startRow = hasHeader ? 2 : 1;
							for (int rowIterator = startRow; rowIterator <= noOfRows; rowIterator++)
							{
								var email = currentSheet.Cells[rowIterator, 3].Value?.ToString()?.Trim();
								if (string.IsNullOrEmpty(email))
								{
									TempData["ErrorMessage"] = $"Row {rowIterator}: Email cannot be empty.";
									return RedirectToAction("UserList");
								}

								var employeeId = currentSheet.Cells[rowIterator, 1].Value?.ToString()?.Trim();
								var mobileNumber = currentSheet.Cells[rowIterator, 6].Value?.ToString()?.Trim();

                                //// Check for duplicate entries
                                //var existingUser = _context.userMasterEntitie
                                //	.FirstOrDefault(u => u.EmployeeId == employeeId || u.MobileNumber == mobileNumber || u.Email == email);

                                //if (existingUser != null)
                                //{
                                //	string duplicateField = "";
                                //	if (existingUser.EmployeeId == employeeId) duplicateField = "EmployeeId";
                                //	if (existingUser.MobileNumber == mobileNumber) duplicateField = "MobileNumber";
                                //	if (existingUser.Email == email) duplicateField = "Email";

                                //	TempData["ErrorMessage"] = $"Row {rowIterator}: Duplicate entry found for {duplicateField}.";
                                //	return RedirectToAction("UserList");
                                //}

                                 
                                var existingUser = _context.userMasterEntitie
                                    .FirstOrDefault(u => u.IsDeleted == false &&
                                                         (u.EmployeeId == employeeId ||
                                                          u.MobileNumber == mobileNumber ||
                                                          u.Email == email));

                                if (existingUser != null)
                                {
                                    string duplicateField = "";
                                    if (existingUser.EmployeeId == employeeId) duplicateField = "EmployeeId";
                                    if (existingUser.MobileNumber == mobileNumber) duplicateField = "MobileNumber";
                                    if (existingUser.Email == email) duplicateField = "Email";

                                    TempData["ErrorMessage"] = $"Row {rowIterator}: Duplicate entry found for {duplicateField}.";
                                    return RedirectToAction("UserList");
                                }


                                // Generate a custom password using the EmployeeId
                                string generatedPassword = GenerateCustomPassword(employeeId);

								// Create a new user entity
								var user = new UserMasterEntitie
								{
									EmployeeId = employeeId,
									UserName = currentSheet.Cells[rowIterator, 2].Value?.ToString()?.Trim() ?? "",
									Email = email,
                                    DateOfJoining = currentSheet.Cells[rowIterator, 4].Value == null
                                        ? nullableDateTime
                                        : Convert.ToDateTime(currentSheet.Cells[rowIterator, 4].Value),
                                    DateOfBirth = currentSheet.Cells[rowIterator, 5].Value == null
                                        ? nullableDateTime
                                        : Convert.ToDateTime(currentSheet.Cells[rowIterator, 5].Value),
                                    MobileNumber = mobileNumber,
                                    GuardianNumber = currentSheet.Cells[rowIterator, 7].Value?.ToString()?.Trim() ?? "",
									Designation = currentSheet.Cells[rowIterator, 9].Value?.ToString()?.Trim() ?? "",
                                    // Department = currentSheet.Cells[rowIterator, 10].Value?.ToString()?.Trim() ?? "",
                                    // ReportingManager = currentSheet.Cells[rowIterator, 10].Value?.ToString()?.Trim() ?? "",

                                    Password = EncryptModel.Encrypt(generatedPassword),
									IsDeleted = false,
									IsActive = true,
                                    UserTypeId=_context.userTypeMasterEntitie.Where(a=>a.UserTypeName=="Employee" && a.IsDeleted==false).Select(a=>a.UserTypeId).FirstOrDefault(),
									StatusId = _context.employeeStatusEntity
	.Where(a => a.StatusName == "Probation").Select(a => a.StatusId).FirstOrDefault(),
                                    CreatedOn = DateTime.Now,
									CreatedBy = 1
								};

								// Send an email with the generated password
								SendEmail(email, generatedPassword);

								users.Add(user);
							}
						}
					}
				}
				else
				{
					TempData["ErrorMessage"] = "Please upload a valid Excel file.";
					return RedirectToAction("UserList");
				}

				// Save all users to the database
				foreach (var user in users)
				{
					_context.userMasterEntitie.Add(user);
				}

				_context.SaveChanges();

				TempData["SuccessMessage"] = "File uploaded and processed successfully.";
				return RedirectToAction("UserList");
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				return RedirectToAction("UserList");
			}
		}


		private string GenerateCustomPassword(string employeeId)
		{
			if (string.IsNullOrWhiteSpace(employeeId))
			{
				throw new ArgumentException("EmployeeId cannot be null or empty.");
			}
			return $"{employeeId}@123";
		}



        //private void SendEmail(string toEmail, string password)
        //{
        //    try
        //    {
        //        var message = new MimeKit.MimeMessage();
        //        message.From.Add(new MimeKit.MailboxAddress("GuardiansInfoTech", "guardianit6@gmail.com")); // Ensure this matches the authenticated email
        //        message.To.Add(new MimeKit.MailboxAddress("", toEmail));
        //        message.Subject = "Welcome to Our Platform";

        //        message.Body = new MimeKit.TextPart("plain")
        //        {
        //            Text = $"Dear user,\n\nYour account has been created successfully. Here is your login password: {password}\n\nThank you."
        //        };

        //        using (var client = new MailKit.Net.Smtp.SmtpClient())
        //        {
        //            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls); // Ensure the host and port are correct
        //            client.Authenticate("guardianit6@gmail.com", "keyzuntvhlrbehok"); // Use the app password here
        //            client.Send(message);
        //            client.Disconnect(true);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Failed to send email to {toEmail}: {ex.Message}");
        //    }
        //}


        private void SendEmail(string toEmail, string password)
        {
            try
            {
                string websiteUrl = "https://guardianshrms.com/";
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress("Guardians InfoTech", "guardianit6@gmail.com"));
                message.To.Add(new MimeKit.MailboxAddress("", toEmail));
                message.Subject = "Welcome to Guardians InfoTech Platform";

                // Email body with inline CSS styling
                string htmlBody = $@"
    <div style='font-family: Arial, sans-serif; background-color: #f5f7fa; padding: 20px;'>
        <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); overflow: hidden;'>
            <div style='background-color: #4a90e2; color: white; padding: 15px 20px;'>
                <h2 style='margin: 0;'>Welcome to Guardians InfoTech</h2>
            </div>
            <div style='padding: 20px; color: #333;'>
                <p>Hello <strong>{toEmail}</strong>,</p>
                <p>Site <strong>{websiteUrl}</strong>,</p>
                <p>Your account has been created successfully. Below are your login details:</p>
                <ul style='list-style-type: none; padding-left: 0;'>
                    <li><strong>Login Email:</strong> {toEmail}</li>
                    <li><strong>Password:</strong> {password}</li>
                </ul>
                <p>You can log in using the button below:</p>
                <p style='text-align: center;'>
                    <a href='{websiteUrl}' style='display: inline-block; background-color: #4a90e2; color: white; padding: 10px 20px; border-radius: 5px; text-decoration: none; font-weight: bold;'>Visit Website</a>
                </p>
                <p>If you did not request this account, please ignore this email.</p>
                <p style='margin-top: 30px;'>Best regards,<br/>The Guardians InfoTech Team</p>
            </div>
        </div>
    </div>";


                message.Body = new MimeKit.BodyBuilder
                {
                    HtmlBody = htmlBody
                }.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate("guardianit6@gmail.com", "keyzuntvhlrbehok"); // App-specific password
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email to {toEmail}: {ex.Message}");
            }
        }



        public IActionResult ExportEmployees(DateTime StartDate, DateTime EndDate, string ExportType, string EmployeeId = null)
        {
            var employees = GetEmployeesByDateRange(StartDate, EndDate, EmployeeId); // Modified method to include EmployeeId

            if (ExportType == "Excel")
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Employees");
                    worksheet.Cell(1, 1).Value = "Employee ID";
                    worksheet.Cell(1, 2).Value = "Name";
                    worksheet.Cell(1, 3).Value = "Email";
                    worksheet.Cell(1, 4).Value = "Punch-In";
                    worksheet.Cell(1, 5).Value = "Punch-Out";

                    int row = 2;
                    foreach (var emp in employees)
                    {
                        worksheet.Cell(row, 1).Value = emp.EmployeeId;
                        worksheet.Cell(row, 2).Value = emp.UserName;
                        worksheet.Cell(row, 3).Value = emp.Email;
                        worksheet.Cell(row, 4).Value = emp.PunchInTime;
                        worksheet.Cell(row, 5).Value = emp.PunchOutTime;
                        row++;
                    }

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employees.xlsx");
                    }
                }
            }
            else if (ExportType == "PDF")
            {
                using (var stream = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 10, 10, 10, 10);
                    PdfWriter.GetInstance(document, stream);
                    document.Open();

                    var table = new PdfPTable(5)
                    {
                        WidthPercentage = 100
                    };
                    table.AddCell("Employee ID");
                    table.AddCell("Name");
                    table.AddCell("Email");
                    table.AddCell("Punch-In");
                    table.AddCell("Punch-Out");

                    foreach (var emp in employees)
                    {
                        table.AddCell(emp.EmployeeId);
                        table.AddCell(emp.UserName);
                        table.AddCell(emp.Email);
                        table.AddCell(emp.PunchInTime.ToString());
                        table.AddCell(emp.PunchOutTime.ToString());
                    }

                    document.Add(table);
                    document.Close();

                    var content = stream.ToArray();
                    return File(content, "application/pdf", "Employees.pdf");
                }
            }

            return RedirectToAction("EmployeeList"); // Redirect back in case of error
        }

        // Modified method to fetch employees by date range and optional EmployeeId
        private List<UserMasterDTO> GetEmployeesByDateRange(DateTime startDate, DateTime endDate, string employeeId = null)
        {
            var query = from user in _context.userMasterEntitie
                        join attendance in _context.attendanceEntitie
                        on user.UserId equals attendance.UserId
                        where attendance.PunchInTime >= startDate && attendance.PunchOutTime <= endDate
                        select new UserMasterDTO
                        {
                            EmployeeId = user.EmployeeId,
                            UserName = user.UserName,
                            Email = user.Email,
                            PunchInTime = attendance.PunchInTime,
                            PunchOutTime = attendance.PunchOutTime
                        };

            if (!string.IsNullOrEmpty(employeeId))
            {
                query = query.Where(e => e.EmployeeId == employeeId); // Filter by string EmployeeId
            }

            return query.ToList();
        }


    }
}
