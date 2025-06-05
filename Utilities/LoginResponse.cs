namespace AttendanceCRM.Utilities
{
    public class LoginResponse
    {
        public int userId { get; set; }
        public string? userName { get; set; }
        public string? Password { get; set; }
        public string? Emailid { get; set; }
        public string? userTypeName { get; set; }
        public string? profileUrl { get; set; }
        public DateTime? lastLoggedTime { get; set; }
        public string? Message { get; set; }
        public int? statusCode { get; set; }
    }
}
