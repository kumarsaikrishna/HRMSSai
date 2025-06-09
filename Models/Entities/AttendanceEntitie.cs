using System.ComponentModel.DataAnnotations;
namespace AttendanceCRM.Models.Entities
{
	public class AttendanceEntitie
	{
		[Key]
		public int AttendanceId { get; set; }

		public int? GracePeriodTime { get; set; }

		public int? UserId { get; set; }

		public DateTime? PunchInTime { get; set; }
        public string? Status { get; set; }
        public DateTime? PunchOutTime { get; set; }
		public int ProductionDuration { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? SelfiePath { get; set; }
        public string? WorkType { get; set; }
        public string? Reason { get; set; }
        public double? PunchOutLatitude { get; set; }
        public double? PunchOutLongitude { get; set; }
        public string? PunchOutSelfiePath { get; set; }

        public string? ScreenShot { get; set; }
        public string? IPAddress { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

    }


	

}
