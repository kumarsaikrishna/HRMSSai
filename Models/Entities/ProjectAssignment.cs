using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class ProjectAssignment
    {
        [Key]
        public int AssignmentId { get; set; }
        public int ProjectId { get; set; }
        public int TeamLeadId { get; set; }
        public DateTime AssignedOn { get; set; }

        // Navigation Properties
        public ProjectEntity Project { get; set; }
        public UserMasterEntitie TeamLead { get; set; }
        public bool? IsDeleted { get; set; }

     

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }


}
