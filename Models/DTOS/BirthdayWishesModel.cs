namespace AttendanceCRM.Models.DTOS
{
    public class BirthdayWishesModel
    {

        public int Id { get; set; }

        public int? SenderId { get; set; }

        public int? ReceiverId { get; set; }

        public string? Message { get; set; }

        public DateTime? CreatedOn { get; set; }
    }


    public class BirthdayWishViewModel
    {
        public string Message { get; set; }
    }

}
