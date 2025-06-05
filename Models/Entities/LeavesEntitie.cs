using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class LeavesEntitie
	{
		[Key]
		public int LeaveId { get; set; }

		public string? Description { get; set; }

		public int UserId { get; set; }

		public int? UserTypeId { get; set; }

		public int? ApprovedBy { get; set; }

		public string? Remarks { get; set; }
		public string? Status { get; set; }
        public int? LeaveTypeId { get; set; }
        public DateOnly FromDate { get; set; }

		public DateOnly ToDate { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
