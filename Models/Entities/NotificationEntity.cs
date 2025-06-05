using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class NotificationEntity
	{
		[Key]
		public int Id { get; set; }
		public int UserId { get; set; }
		public string? Message { get; set; }
		public DateTime? CreatedOn { get; set; }
		public bool IsRead { get; set; }
		public int? leaveid {  get; set; }

    }
}
