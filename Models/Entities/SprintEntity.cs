using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class SprintEntity
    {
        [Key]
        public int SprintId { get; set; }

        public int? ProjectId { get; set; }

        [Required]
        public string? SprintName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }

        public int? AssignedTo { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Property (Join)
        public ProjectEntity? Project { get; set; }
    }


    public class SprintViewModel
    {
        public int SprintId { get; set; }
        public string? SprintName { get; set; }
        public string? ProjectName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public int? AssignedTo { get; set; }
    }


}
