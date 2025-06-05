using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class TimelineRecordsEntity
	{
		[Key]
		public int TimelineId { get; set; }
		public int TaskId { get; set; }
		public string TaskName { get; set; } // From Tasks table (JOIN)
		public int UpdatedBy { get; set; }
		public string UpdatedByUser { get; set; } // From UserMaster table (JOIN)
		public string PreviousStatus { get; set; }
		public string NewStatus { get; set; }
		public DateTime UpdatedOn { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedOn { get; set; }
		public bool IsDeleted { get; set; }
	}
}
