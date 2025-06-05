using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class UserMasterEntitie
	{
		[Key]
		public int UserId { get; set; }

		public string? UserName { get; set; }

		public DateTime? DateOfJoining { get; set; }

		public string? Email { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public string? MobileNumber { get; set; }

		public string? GuardianNumber { get; set; }

		public string? ProfilePicture { get; set; }

		public string? EmployeeId { get; set; }

		public string? CollegeName { get; set; }

		public string? Address { get; set; }

		public string? Designation { get; set; }

		public string? AdharNumber { get; set; }

		public string? PanNumber { get; set; }

		public int? UserTypeId { get; set; }
        public int? StatusId { get; set; }
        public int? DepartmentId { get; set; }

        public string? Password { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
