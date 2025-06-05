using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class OTPDetailsEntity
	{
		[Key]
		public int OtpId { get; set; }

		public string? OTPString { get; set; }

		public DateTime? GeneratedOn { get; set; }

		public DateTime? UsedOn { get; set; }

		public string? CurrentStatus { get; set; }

		public int? UserId { get; set; }

		public bool? IsDeleted { get; set; }
	}
}
