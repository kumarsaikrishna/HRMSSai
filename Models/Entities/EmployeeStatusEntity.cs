using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class EmployeeStatusEntity
    {
        [Key]
        public int StatusId { get; set; }

        public string StatusName { get; set; }

        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
