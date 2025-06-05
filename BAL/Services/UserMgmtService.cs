using AttendanceCRM.BAL.IServices;
using AttendanceCRM.Models.DTOS;
using AttendanceCRM.Models.Entities;
using AttendanceCRM.Utilities;

using System.Net.Mail;
using PagedList.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Configuration;


namespace AttendanceCRM.BAL.Services
{
    public class UserMgmtService: IUserService
    {
        private readonly MyDbContext _context;
		private readonly IConfiguration _config;

		public UserMgmtService(MyDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
       
        #region USER
        public int TotalEmplyee()
        {
            int count = _context.userMasterEntitie.Where(a => a.IsDeleted == false ).Count();
            return count;
        }

        public int GetPresentEmployeesToday()
        {
            DateTime today = DateTime.Today;

            // Fetching all attendance records where the PunchInTime is today
            int count = _context.attendanceEntitie
                .Where(a => a.PunchInTime.HasValue && // Ensure the PunchInTime is not null
                            a.PunchInTime.Value.Date == today && // Only consider today's punch in time
                            a.IsActive == true && // Only active employees
                            a.IsDeleted == false) // Only non-deleted employees
                .Count();

            return count;
        }


        public int TotalEmplyeeActive()
		{
			int count = _context.userMasterEntitie.Where(a => a.IsDeleted == false && a.IsActive == true).Count();
			return count;
		}
		public int TotalEmplyeeInActive()
		{
			int count = _context.userMasterEntitie.Where(a => a.IsDeleted == true && a.IsActive == false).Count();
			return count;
		}
        public int TotalEmplyeeNewJoin()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30).Date; // Get only the date part
            int count = _context.userMasterEntitie
                .Where(a => a.IsDeleted == false && a.IsActive == true && a.DateOfJoining.HasValue && a.DateOfJoining.Value.Date >= thirtyDaysAgo)
                .Count();
            return count;

        }

        public int GetTodayAbsent()
        {
            DateTime todayStart = DateTime.Today;
            DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);

            // Ensure we only fetch non-null UserIds
            var allEmployeeIds = _context.userMasterEntitie
                .Where(u => u.IsDeleted == false && u.IsActive == true && u.UserId != null)
                .Select(u => u.UserId) // Convert nullable int? to int
                .ToList();

            // Debugging: Log total employees
            Console.WriteLine("Total Active Employees: " + allEmployeeIds.Count);

            // Get employees who have punched in today, ensuring UserId is not null
            var punchedInEmployees = _context.attendanceEntitie
                .Where(a => a.PunchInTime >= todayStart && a.PunchInTime <= todayEnd && a.UserId != null)
                .Select(a => a.UserId.Value) // Convert nullable int? to int
                .Distinct()
                .ToList();

            // Debugging: Log total punched-in employees
            Console.WriteLine("Total Employees Punched In: " + punchedInEmployees.Count);

            // Count employees who have NOT punched in (Absent employees)
            int absentCount = allEmployeeIds.Except(punchedInEmployees).Count();

            // Debugging: Log absent count
            Console.WriteLine("Total Absent Employees: " + absentCount);

            return absentCount;
        }




        public int GetCurrentProjects()
        {
            // Logic to get the number of active/current projects
            var activeProjectsCount = _context.projectEntities
                .Where(p => p.IsDeleted == false)
                .Count();

            return activeProjectsCount;
        }

        public IEnumerable<UserMasterDTO> UserList(string searchTerm = null)
        {
            var query = (from sr in _context.userMasterEntitie
                         where sr.IsDeleted == false && sr.IsActive == true
                         select new UserMasterDTO
                         {
                             UserId = sr.UserId,
                             UserName = sr.UserName,
                             DateOfJoining = sr.DateOfJoining,
                             Email = sr.Email,
                             DateOfBirth = sr.DateOfBirth,
                             MobileNumber = sr.MobileNumber,
                             GuardianNumber = sr.GuardianNumber,
                             ProfilePicture = sr.ProfilePicture,
                             EmployeeId = sr.EmployeeId,
                             CollegeName = sr.CollegeName,
                             Address = sr.Address,
                             Designation = sr.Designation,
                             AdharNumber = sr.AdharNumber,
                             PanNumber = sr.PanNumber,
                             UserTypeId = sr.UserTypeId,
                             IsDeleted = sr.IsDeleted,
                             StatusId = sr.StatusId,
                             CreatedOn = sr.CreatedOn // ✅ Ensure CreatedOn is included
                         }).ToList();

            // Search implementation
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            return query.OrderByDescending(x => x.CreatedOn).ToList();
        }




        public IEnumerable<UserMasterDTO> UserListActive(string searchTerm = null)
        {


            var query = (from sr in _context.userMasterEntitie
                      
                       where (sr.IsDeleted == false && sr.IsActive==true)


                       select new UserMasterDTO
                       {
                           UserId = sr.UserId,
                           UserName = sr.UserName,
                           DateOfJoining = sr.DateOfJoining,
                           Email = sr.Email,
                           DateOfBirth = sr.DateOfBirth,
                           MobileNumber = sr.MobileNumber,
                           GuardianNumber = sr.GuardianNumber,
                           ProfilePicture = sr.ProfilePicture,
                           EmployeeId = sr.EmployeeId,
                           CollegeName = sr.CollegeName,
                           Address = sr.Address,
                           Designation = sr.Designation,
                           AdharNumber = sr.AdharNumber,
                           PanNumber = sr.PanNumber,
                           UserTypeId = sr.UserTypeId,
                           IsDeleted = sr.IsDeleted,
                           StatusId=sr.StatusId,
                           CreatedOn = sr.CreatedOn

                       }).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            return query.OrderByDescending(x => x.CreatedOn).ToList();
        }
        public IEnumerable<UserMasterDTO> UserListInActive(string searchTerm = null)
        {

            var query = (from sr in _context.userMasterEntitie

                       where (sr.IsDeleted == true && sr.IsActive == false)


                       select new UserMasterDTO
                       {
                           UserId = sr.UserId,
                           UserName = sr.UserName,
                           DateOfJoining = sr.DateOfJoining,
                           Email = sr.Email,
                           DateOfBirth = sr.DateOfBirth,
                           MobileNumber = sr.MobileNumber,
                           GuardianNumber = sr.GuardianNumber,
                           ProfilePicture = sr.ProfilePicture,
                           EmployeeId = sr.EmployeeId,
                           CollegeName = sr.CollegeName,
                           Address = sr.Address,
                           Designation = sr.Designation,
                           AdharNumber = sr.AdharNumber,
                           PanNumber = sr.PanNumber,
                           UserTypeId = sr.UserTypeId,
                           IsDeleted = sr.IsDeleted,
                           StatusId = sr.StatusId,
                           CreatedOn = sr.CreatedOn

                       }).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            return query.OrderByDescending(x => x.CreatedOn).ToList();



        }
        public IEnumerable<UserMasterDTO> UserListNewJoin(string searchTerm = null)
        {


            var query = (from sr in _context.userMasterEntitie
                      
                       where sr.IsDeleted==false && sr.IsActive==true
                       select new
                       {
                           sr
                         
                       })
            .AsEnumerable()
           .Where(x => x.sr.DateOfJoining.HasValue &&
            x.sr.DateOfJoining.Value.Date >= DateTime.Now.AddDays(-30).Date)

            .Select(x => new UserMasterDTO
            {
                UserId = x.sr.UserId,
                UserName = x.sr.UserName,
                DateOfJoining = x.sr.DateOfJoining,
                Email = x.sr.Email,
                DateOfBirth = x.sr.DateOfBirth,
                MobileNumber = x.sr.MobileNumber,
                GuardianNumber = x.sr.GuardianNumber,
                ProfilePicture = x.sr.ProfilePicture,
                EmployeeId = x.sr.EmployeeId,
                CollegeName = x.sr.CollegeName,
                Address = x.sr.Address,
                Designation = x.sr.Designation,
                AdharNumber = x.sr.AdharNumber,
                PanNumber = x.sr.PanNumber,
                UserTypeId = x.sr.UserTypeId,
                IsDeleted = x.sr.IsDeleted,
                StatusId=x.sr.StatusId,
                CreatedOn = x.sr.CreatedOn
            })
            .ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(x =>
                    (x.UserName ?? "").ToLower().Contains(searchTerm) ||
                    (x.Email ?? "").ToLower().Contains(searchTerm) ||
                    (x.MobileNumber ?? "").ToLower().Contains(searchTerm) ||
                    (x.CollegeName ?? "").ToLower().Contains(searchTerm)
                ).ToList();
            }

            return query.OrderByDescending(x => x.CreatedOn).ToList();
        }
        public UserMasterEntitie GetUserById(int id)
        {
            return _context.userMasterEntitie.Where(a => a.UserId == id).FirstOrDefault();
        }

        public GenericResponse CreateUser(UserMasterEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.IsDeleted = false;
                _context.userMasterEntitie.Add(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.UserId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to save : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }

        public GenericResponse UpdateUser(UserMasterEntitie sme)
        {
            GenericResponse response = new GenericResponse();
            try
            {
                sme.UpdatedOn = DateTime.UtcNow;
                var u = _context.userMasterEntitie.Where(a => a.UserId == sme.UserId).FirstOrDefault();
				
				_context.Entry(u).CurrentValues.SetValues(sme);
                _context.SaveChanges();
                response.statuCode = 1;
                response.message = "Success";
                response.currentId = sme.UserId;
                return response;
            }
            catch (Exception ex)
            {
                response.message = "Failed to update : " + ex.Message;
                response.currentId = 0;
                return response;
            }
        }
#endregion

        public LoginResponse LoginCheck(LoginRequest request)
        {
            LoginResponse lr = new LoginResponse();

            try
            {

                var u = _context.userMasterEntitie.Where(a => a.IsDeleted == false && a.IsActive == true && a.Email == request.emailId).FirstOrDefault();

                if (u == null)
                {
                    lr.statusCode = 0;
                    lr.Message = "Please enter valid email";
                    return lr;
                }
                else
                {
                    var p = EncryptModel.Decrypt(u.Password);
                    if (request.password == EncryptModel.Decrypt(u.Password))
                    {
                        lr.statusCode = 1;
                        lr.Message = "success";
                        lr.userTypeName = _context.userTypeMasterEntitie.Where(a => a.UserTypeId == u.UserTypeId).Select(b => b.UserTypeName).FirstOrDefault();
                        lr.userName = u.UserName;
                        lr.userId = u.UserId;
                        lr.profileUrl = u.ProfilePicture == null ? "dummy.png" : u.ProfilePicture;

                        // entry in lastlogin detials

                        lr.statusCode = 1;
                        lr.Message = "Login success";
                        return lr;
                    }
                    else
                    {
                        lr.statusCode = 0;
                        lr.Message = "Password incorrect";
                        return lr;
                    }
                }

            }
            catch (Exception ex)
            {


            }

            return lr;
        }


        public GenericResponse SaveUser(UserMasterEntitie usersEntity)

        {
            GenericResponse response = new GenericResponse();


            try
            {

                var emailCheck = _context.userMasterEntitie.Where(a => a.Email == usersEntity.Email && a.IsDeleted == false && a.IsActive == true).FirstOrDefault();
                if (emailCheck != null)
                {
                    response.statuCode = 0;
                    response.message = "emailid not available";
                    return response;
                }

                usersEntity.IsDeleted = false;
                //usersEntity.IsActive = true;
                usersEntity.UpdatedOn = DateTime.Now;
                usersEntity.CreatedOn = DateTime.Now;
                usersEntity.Password = usersEntity.Password;
                _context.userMasterEntitie.Add(usersEntity);
                _context.SaveChanges();

                response.statuCode = 1;
                response.message = "Success";


            }
            catch (Exception ex)
            {
                response.statuCode = 0;
                response.message = "Save failed. " + ex.Message;
            }

            return response;
        }

		public GenericResponse UpdateOtpTransactions(OTPDetailsEntity ut)
		{
			GenericResponse response = new GenericResponse();
			try
			{
				if (ut.OtpId > 0)

				{
					var u = _context.otpDetailsEntity.Where(a => a.OtpId == ut.OtpId).FirstOrDefault();
					_context.Entry(u).CurrentValues.SetValues(ut);
					_context.SaveChanges();
					response.statuCode = 1;
					response.message = "Success";
					response.currentId = u.OtpId;
					return response;
				}
				else
				{
					ut.IsDeleted = false;
					_context.otpDetailsEntity.Add(ut);
					_context.SaveChanges();
					response.statuCode = 1;
					response.message = "Success";
					response.currentId = ut.OtpId;
					return response;
				}

			}
			catch (Exception ex)
			{
				response.message = "Failed to save : " + ex.Message;
				response.currentId = 0;
				return response;
			}
		}


		public bool PushEmail(string emailtext, string to, string subject, string cc = "", byte[] attachement = null)
		{
			bool res = false;
			try
			{

				// string emailimagesurl = _config.GetValue<string>("EmailConfig:EMAILIMAGEURL");
				string smtpserver = _config.GetValue<string>("EmailConfig:smtpServer");
				string smtpUsername = _config.GetValue<string>("EmailConfig:smtpEmail");
				string smtpPassword = _config.GetValue<string>("EmailConfig:smtppassword");
				int smtpPort = _config.GetValue<int>("EmailConfig:portNumber");

				// emailtext = emailtext.Replace("##EMAILIMAGES##", emailimagesurl);
				MailMessage msg = new MailMessage(smtpUsername, to, subject, emailtext);

				MailMessage mail = new MailMessage();
				mail.To.Add(to);
				if (!string.IsNullOrEmpty(cc))
				{
					List<string> ccMails = cc.Split(",").ToList();
					foreach (string e in ccMails)
					{
						mail.CC.Add(e);
					}
				}

				mail.From = new MailAddress(smtpUsername);
				mail.Subject = subject;
				mail.Body = emailtext;
				mail.IsBodyHtml = true;
				if (attachement != null)
				{
					using var attachment = new Attachment(new MemoryStream(attachement), "test.pdf");
					mail.Attachments.Add(attachment);
				}
				SmtpClient smtp = new SmtpClient();
				smtp.Host = smtpserver;
				smtp.Port = smtpPort;

				smtp.UseDefaultCredentials = false;
				smtp.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);
				smtp.EnableSsl = true;

				try
				{
					smtp.Send(mail);

					res = true;
				}
				catch (Exception ex)
				{
					res = false;
				}



			}
			catch (Exception ex)
			{

				res = false;

			}
			return res;
		}


        public async Task<UserMasterEntitie> GetUserByEmailAsync(string email)
        {
            // Query the database to find a user with the provided email and ensure that it's not deleted
            return await _context.userMasterEntitie
                                 .Where(u => u.Email == email && u.IsDeleted == false)
                                 .FirstOrDefaultAsync();
        }

        public GenericResponse ResetPassword(UserMasterEntitie request)
		{
			GenericResponse response = new GenericResponse();
			try
			{

				UserMasterEntitie ue = new UserMasterEntitie();
				ue = _context.userMasterEntitie.Where(a => a.UserId == request.UserId).FirstOrDefault();
				
				ue.Password = request.Password;
				response = CreateUser(ue);
				if (response.statuCode == 1)
				{
					// send email with otp to activate

				}
			}
			catch (Exception ex)
			{
				ex.Source = "Register User";

				response.statuCode = 0;
				response.message = "Failed to register";
            }
            return response;
        }

       
            // Define the method to fetch performance data
            public IEnumerable<PerformanceDTO> GetPerformanceData()
            {
                // Assuming you have a table called PerformanceData in your database
                var performanceData = _context.PerformanceData
                    .Where(p => p.TotalProductionDuration > 0)  // Example filter, adjust as needed
                    .Select(p => new PerformanceDTO
                    {
                        UserId = p.UserId,
                        UserName = p.UserName,
                        TotalProductionDuration = p.TotalProductionDuration,
                        Designation = p.Designation
                    })
                    .ToList();

                return performanceData;
            }

        public List<EmployeeDepartmentDto> GetEmployeesByDepartment()
        {
            return _context.userMasterEntitie
                .Where(e => e.Designation != null)  // Filter out null designations
                .GroupBy(e => e.Designation)
                .Select(group => new EmployeeDepartmentDto
                {
                    Department = group.Key,
                    EmployeeCount = group.Count()
                })
                .ToList();
        }
    }



}

