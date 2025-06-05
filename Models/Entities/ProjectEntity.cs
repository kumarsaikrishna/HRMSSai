using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceCRM.Models.Entities
{
     
    public class ProjectEntity
    {
        [Key]
        
        public int ProjectId { get; set; }

       
        public string ProjectName { get; set; }

      
        public string ClientName { get; set; }

        
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

      
        public string Status { get; set; } 

        public decimal? Budget { get; set; }

        public string Description { get; set; }
       

      
        public int CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
         
        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

     
        public bool IsDeleted { get; set; } 
    }
}
