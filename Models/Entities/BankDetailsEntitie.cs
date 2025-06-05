using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class BankDetailsEntitie
	{
		[Key]
		public int BankId { get; set; }

		public int? UserId { get; set; }

		public string? AccountNumber { get; set; }

		public string? IFSCNumber { get; set; }

		public string? BranchName { get; set; }


		public string? AccountType { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
