using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class HolidaysEntite
	{
		[Key]
		public int HolidayId { get; set; }

		public int? UserTypeId { get; set; }

		public string? HolidayDescription { get; set; }
		public string? HolidayName { get; set; }


		public DateTime? HolidayDate { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }

    }
}
