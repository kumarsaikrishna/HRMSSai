namespace AttendanceCRM.Utilities
{
    public class LoginResponseModel
    {
        public string userName { get; set; }
        public string emailId { get; set; }
        public string userTypeName { get; set; }
        public int userId { get; set; }
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
        public int userTypeCode { get; set; }
        public string profileUrl { get; set; }
        public string password { get; set; }
        public DateTime lastLoginTime { get; set; }
    }
}
