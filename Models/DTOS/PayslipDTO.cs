using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceCRM.Models.DTOS
{
    public class PayslipDTO
    {
        public int PayslipId { get; set; }

        public int? UserId { get; set; }

        public string? SalaryMonth { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal HRA { get; set; }

        public decimal Bonus { get; set; }

        public decimal Deductions { get; set; }

        public decimal GrossSalary { get; set; }

        public decimal NetSalary { get; set; }

        public decimal? TaxPaid { get; set; }

        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Designation { get; set; }
        public DateTime? DOJ { get; set; }
        public string? PAN { get; set; }
        public string? UAN { get; set; }
        public string? UserTypeName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? IFSC { get; set; }
        public int? LOP { get; set; }
        public int? PayDays { get; set; }


        public decimal SpecialAllowance { get; set; }
        public decimal Conveyance { get; set; }
        public decimal PF_Employee { get; set; }
        public decimal ProfessionalTax { get; set; }

        public string NetSalaryInWords { get; set; }


    }
}
