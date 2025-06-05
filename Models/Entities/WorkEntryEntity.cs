using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class WorkEntryEntity
    {
        [Key]
        public int Id { get; set; }

        public int? ProjectId { get; set; }
        public string TaskName { get; set; }

        public string Description { get; set; }

       
        public int TimeSpent { get; set; } 

    
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; }

     
        public int UserId { get; set; } 

       // public DateTime CreatedDate { get; set; } 
    }

    public class WorkEntryViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } // Holds user name
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
        public string TaskName { get; set; }
        public string Status { get; set; }
        public int TimeSpent { get; set; }

        public int ProjectId { get; set; }


        public string? ProjectName { get; set; }


        public string? ClientName { get; set; }


        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }


        public string? PStatus { get; set; }

        public decimal? Budget { get; set; }

        public string? PDescription { get; set; }
    }


}
