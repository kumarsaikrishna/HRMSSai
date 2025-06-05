using AttendanceCRM.Models.Entities;

namespace AttendanceCRM.Models.DTOS
{
    public class LeavesDTO
    {
        public int LeaveId { get; set; }

        public string? Description { get; set; }

        public int? UserId { get; set; }
        public string? UserName { get; set; }

        public int? UserTypeId { get; set; }
        public int? LeaveTypeId { get; set; }
        public int? ApprovedBy { get; set; }
        public string? Status { get; set; }
      
        public string? Remarks { get; set; }

        public DateOnly FromDate { get; set; }

        public DateOnly ToDate { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }
        public string? Approve { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public string? LeaveType { get; set; }
      
      
    }


    public class PaginatedLeaveListViewModel
    {
        public List<LeavesDTO> Leaves { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

}
