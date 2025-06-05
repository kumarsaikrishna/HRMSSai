namespace AttendanceCRM.Models.DTOS
{
    public class UserTypeDTO
    {
        public int UserTypeId { get; set; }

        public string? UserTypeName { get; set; }

        public string? Designation { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
