using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceCRM.Models.DTOS
{
    public class SalaryStructureDTO
    {
        public int SalaryStructureId { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserType { get; set; }
        public DateTime? JoiningDate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HRA { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SpecialAllowance { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Conveyance { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PF_Employee { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ProfessionalTax { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }


        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
