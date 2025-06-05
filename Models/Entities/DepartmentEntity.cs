using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class DepartmentEntity
    {
        [Key]
        public int DepartmentID { get; set; }

        public string? DepartmentName { get; set; }

        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string? UpdatedOn { get; set; }

        public int? CreatedBY { get; set; }

        public int? UpdatedBy { get; set; }

    }
}
