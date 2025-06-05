using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class TeamMemberEntity
    {
        [Key]
        public int TeamMemberId { get; set; }

        [Required(ErrorMessage = "Project is required")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Team Lead is required")]
        public int TeamLeadId { get; set; }

        [Required(ErrorMessage = "Member is required")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50, ErrorMessage = "Role cannot be more than 50 characters")]
        public string? Role { get; set; }

        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime AssignedOn { get; set; }

        public ProjectEntity Project { get; set; }
        public UserMasterEntitie TeamLead { get; set; }
        public UserMasterEntitie Member { get; set; }
    }
      public class TeamMemberEntit
    {
        
        public int TeamMemberId { get; set; }

        
        public int ProjectId { get; set; }

        
        public int TeamLeadId { get; set; }

       
        public int MemberId { get; set; }

       
        public string? Role { get; set; }

        
        public DateTime CreatedOn { get; set; }
      
        public DateTime AssignedOn { get; set; }

        
    }

}
