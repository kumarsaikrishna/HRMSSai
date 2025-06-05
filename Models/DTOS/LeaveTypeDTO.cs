namespace AttendanceCRM.Models.DTOS
{
    public class LeaveTypeDTO
    {
        public int LeaveTypeId { get; set; }
        public string? LeaveType { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
