namespace AttendanceCRM.Models.DTOS
{
    public class PerformanceDTO
    {
        public int PerformanceId { get; set; }

        public int? UserId { get; set; }

        public string? UserName { get; set; }

        public double? TotalProductionDuration { get; set; }

        public string? Designation { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
