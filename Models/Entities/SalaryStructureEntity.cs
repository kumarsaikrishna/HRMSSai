using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class SalaryStructureEntity
    {
        [Key]
        public int SalaryStructureId { get; set; }

        public int UserId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal HRA { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal SpecialAllowance { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Conveyance { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }

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


    public class BankDetailssDTO
    {
        public int BankId { get; set; }
        public int UserId { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCNumber { get; set; }
        public string BranchName { get; set; }
        public string AccountType { get; set; }
    }

    public class PayslippDTO
    {
        public int PayslipId { get; set; }
        public int UserId { get; set; }
        public string SalaryMonth { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TaxPaid { get; set; }
    }


}
