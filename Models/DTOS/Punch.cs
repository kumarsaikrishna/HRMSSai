namespace AttendanceCRM.Models.DTOS
{
    public class Punch
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime PunchDate { get; set; }
        public DateTime? PunchInTime { get; set; }
        public DateTime? PunchOutTime { get; set; }
        public string? PunchInSelfiePath { get; set; }
        public string? PunchOutSelfiePath { get; set; }
        public string? PunchInLocationUrl { get; set; }
        public string? PunchOutLocationUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? PunchOutLatitude { get; set; }
        public double? PunchOutLongitude { get; set; }
    }
}
