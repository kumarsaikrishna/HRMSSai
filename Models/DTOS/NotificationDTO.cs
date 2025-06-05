namespace AttendanceCRM.Models.DTOS
{
	public class NotificationDTO
	{
		
			public int Id { get; set; } 
			public int UserId { get; set; } 
			public string? Message { get; set; } 
			public DateTime? CreatedOn { get; set; } 
			public bool IsRead { get; set; }
        public int? leaveid { get; set; }



    }
}
