using System.ComponentModel.DataAnnotations;
namespace AttendanceCRM.Models.Entities
{
    public class Incidents
    {
        [Key]
        public int IncidentId { get; set; }

        public int? Attendanceid { get; set; }

        public string Incident { get; set; }

        public DateTime? IncidentDate { get; set; }

        public string Swipes { get; set; }

        public string Reason { get; set; }

        public int? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

    }
}
