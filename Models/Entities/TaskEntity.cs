using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceCRM.Models.Entities
{
    public class TaskEntity
    {
        [Key]
        public int TaskId { get; set; }

        public int ProjectId { get; set; }
        public int AssignedTo { get; set; }
        public int AssignedBy { get; set; }

        [Required]
        [StringLength(255)]
        public string? TaskName { get; set; }

        public string? Description { get; set; }
        public int? TimeSpent { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? SprintId { get; set; }

        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties (Joins)
        [ForeignKey("ProjectId")]
        public ProjectEntity Project { get; set; }

        [ForeignKey("AssignedTo")]
        public UserMasterEntitie AssignedToUser { get; set; }

        [ForeignKey("AssignedBy")]
        public UserMasterEntitie AssignedByUser { get; set; }
    }

    public class TaskViewModel
    {
        public int TaskId { get; set; }
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public int? TimeSpent { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProjectName { get; set; }
        public string? AssignedToUser { get; set; }
        public string? AssignedByUser { get; set; }
    }



}
