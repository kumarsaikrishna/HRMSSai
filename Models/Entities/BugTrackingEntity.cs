using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class BugTracking
    {
        [Key]
        public int BugId { get; set; }

        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        [Required]
        [ForeignKey("Task")]
        public int TaskId { get; set; }

        [Required]
        [ForeignKey("ReportedByUser")]
        public int ReportedBy { get; set; }

        [ForeignKey("AssignedToUser")]
        public int? AssignedTo { get; set; }

        [Required]
        public string BugDescription { get; set; }

        [Required]
        [EnumDataType(typeof(PriorityLevel))]
        public string Priority { get; set; }

        [Required]
        [EnumDataType(typeof(BugStatus))]
        public string Status { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        //public DateTime? UpdatedOn { get; set; }

        // Navigation Properties
        //public virtual ProjectEntity Project { get; set; }
        //public virtual Task Task { get; set; }
        //public virtual UserMasterEntitie ReportedByUser { get; set; }
        //public virtual UserMasterEntitie AssignedToUser { get; set; }
    }

     public enum PriorityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

     public enum BugStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed
    }


    public class BugViewModel
    {
        public int BugId { get; set; }
        public string ProjectName { get; set; }
        public string TaskName { get; set; }
        public string ReportedBy { get; set; }
        public string AssignedTo { get; set; }
        public string BugDescription { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
    }

}
