using AttendanceCRM.Models.Entities;

namespace AttendanceCRM.Models.DTOS
{
    public class HolidaysDTO
    {
        public int HolidayId { get; set; }

        public int? UserTypeId { get; set; }

        public string? HolidayDescription { get; set; }
        public string? HolidayName { get; set; }

        public DateTime? HolidayDate { get; set; } = DateTime.Today;
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public IEnumerable<UserMasterEntitie>? UserTypeList { get; set; }
        public string? UserTypeName { get; set; }
    }
}
