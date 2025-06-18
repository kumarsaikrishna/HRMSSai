using AttendanceCRM.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.DTOS
{
    public class UserMasterDTO
    {
        public int UserId { get; set; }

        public string? UserName { get; set; }

        public DateTime? DateOfJoining { get; set; }

        public string? Email { get; set; }
        public string? MobileNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public string? Designation { get; set; }

        public string? EmployeeId { get; set; }
        public string? GuardianNumber { get; set; }

        public string? ProfilePicture { get; set; }

        public bool Status { get; set; }

        public string? CollegeName { get; set; }

        public string? Address { get; set; }

        
        public string? AdharNumber { get; set; }

        public string? PanNumber { get; set; }

        public int? UserTypeId { get; set; }
        public string? Password { get; set; }
        public int? StatusId { get; set; }
        public int? DepartmentId { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
		public IFormFile? ProfileImgUploaded { get; set; }
		public List<UserTypeMasterEntitie>? UserTypeList { get; set; }
        public string? UserTypeName { get; set; }

        public DateTime? PunchInTime { get; set; }

        public DateTime? PunchOutTime { get; set; }

        public BankDetailsEntitie BankDetails { get; set; }
    }




	public class RegisterStepFinal
	{
		public int UserId { get; set; }
		public string mobileOtp { get; set; }
		public string emailOtp { get; set; }
		public string UserName { get; set; }
		public string emailId { get; set; }
		public string mobileNumber { get; set; }

		public int OTPTrid { get; set; }

	}

	public class ResetPasswordFinal
	{
		public int UserId { get; set; }
		public string mobileOtp { get; set; }
		[Required(ErrorMessage = "OTP required")]
		public string emailOtp { get; set; }
		public int OTPTrid { get; set; }

		[StringLength(100, ErrorMessage = "Please enter at least 6 characters.", MinimumLength = 6)]

		[Required(ErrorMessage = "Password required")]
		public string pword { get; set; }
		[Required(ErrorMessage = "Confirm Password required")]
		[Compare("pword", ErrorMessage = "Passwords mismatch")]
		public string cnfpword { get; set; }
		public string UserName { get; set; }
	}


    public class EmployeeDepartmentDto
    {
        public string Department { get; set; }
        public int EmployeeCount { get; set; }
    }

}
