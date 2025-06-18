namespace AttendanceCRM.Models.DTOS
{
    public class AttendanceDTO
    {
        public int AttendanceId { get; set; }

        public int? GracePeriodTime { get; set; }

        public int? UserId { get; set; }

        public DateTime? PunchInTime { get; set; }

        public DateTime? PunchOutTime { get; set; }
        public int ProductionDuration { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? SelfiePath { get; set; }
        public double? PunchOutLatitude { get; set; }
        public double? PunchOutLongitude { get; set; }
        public string? PunchOutSelfiePath { get; set; }
        public string? Reason { get; set; }
        public string? WorkType { get; set; }
        public string? Status { get; set; }
        public string? ScreenShot { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public string? UserName { get; set; }
    }

    public class AttendanceViewDTO
    {
        public DateTime Date { get; set; }
        public string WorkType { get; set; } // "WFO" or "WFH"
        public string TimeSpent { get; set; } // e.g., "08:45:00"
        public bool IsAbsent { get; set; }
        public bool IsMissedPunchOut { get; set; }
    }

    public class AttendanceViewModel
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsPunchedIn { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int TotalHours { get; set; }
        public DateTime? PunchInTime { get; set; }
        public DateTime? PunchOutTime { get; set; }
        public string TotalWorkHours { get; set; } = "00:00:00";
        public DateTime? DateOfJoining { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? SelfiePath { get; set; }
        public double? PunchOutLatitude { get; set; }
        public double? PunchOutLongitude { get; set; }
        public string? WorkType { get; set; }
        public string? Reason { get; set; }
        public string? PunchOutSelfiePath { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Designation { get; set; }

        public string? EmployeeId { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? GracePeriodTime { get; set; }
        public int TotalLeaves { get; set; }
        public int Taken { get; set; }
        public int Absent { get; set; }
        public int Requests { get; set; }
        public int WorkedDays { get; set; }
        public int LossOfPay { get; set; }

    }

    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int EmployeesLeavs { get; set; }
        public int PresentEmployees { get; set; }
        public int Todayabsent { get; set; }
        public int CurrentProjects { get; set; }
        public double PercentageChange { get; set; }

        public EmployeeStatusViewModel EmployeeStatus { get; set; }

        public TopPerformer? Top { get; set; }
        public List<EmployeeByDepartmentViewModel> EmployeesByDepartment { get; set; }

    }

    public class EmployeeStatusViewModel
    {
        public int TotalEmployees { get; set; }
        public int FullTimeCount { get; set; }
        public int ContractCount { get; set; }
        public int ProbationCount { get; set; }
        public int WFHCount { get; set; }
        public int FullTimePercentage { get; set; }
        public int ContractPercentage { get; set; }
        public int ProbationPercentage { get; set; }
        public int WFHPercentage { get; set; }
        public int EmployeesByDepartment { get; set; }
        public TopPerformer? TopPerformer { get; set; }
    }

    public class TopPerformer
    {
        public int userid { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public int Performance { get; set; }
        public string ProfilePicture { get; set; }
    }



    public class EmployeeDetailsViewModel
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Status { get; set; }
        public DateTime? JoinDate { get; set; }
        public string? ContactNumber { get; set; }
        public string? department { get; set; }
        public double? TotalHoursWorked { get; set; }
        public int? Performance { get; set; }
    }

    public class EmployeeByDepartmentViewModel
    {
        public string Department { get; set; }
        public int Count { get; set; }
    }


    public class EmployeeBirthdayViewModel
    {
        public int UserId { get; set; } // Unique identifier for the user
        public string UserName { get; set; } // Name of the employee
        public string Designation { get; set; } // Job title or designation of the employee
        public string ProfilePicture { get; set; } // Path to the profile picture
        public DateTime BirthdayDate { get; set; } // The date of the employee's birthday
    }




    public class AttendanceReportViewModel
    {
        public int UserId { get; set; }
        public string? EmployeeId { get; set; }
        public string UserName { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int? DepartmentId { get; set; }

        public string DOJ { get; set; }
        public int PresentDays { get; set; }
        public int CasualLeave { get; set; }
        public int WeekOff { get; set; }
        public int Absent { get; set; }
        public int Holidays { get; set; }
        public double HalfDay { get; set; }
        public int SickLeave { get; set; }
        public int EarnedLeaves { get; set; }
        public double FinalPaidDays { get; set; }

        // Stores daily status for each date
        public Dictionary<int, string> DailyStatus { get; set; } = new Dictionary<int, string>();

        // Convert DailyStatus dictionary to a comma-separated string for easy use
        public string DailyAttendance
        {
            get
            {
                return string.Join(", ", DailyStatus.OrderBy(d => d.Key)
                                                     .Select(d => $"{d.Key}: {d.Value}"));
            }
        }
    }







}
