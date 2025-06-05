using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class BirthdayWishesEntity
    {
        [Key]
        public int Id { get; set; }

        public int? SenderId { get; set; }

        public int? ReceiverId { get; set; }

        public string? Message { get; set; }

        public DateTime? CreatedOn { get; set; }

    }
}
