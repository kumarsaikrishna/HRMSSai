using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class PayslipEntitie
	{
		[Key]
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
        public decimal? SpecialAllowance { get; set; }

        public decimal? Conveyance { get; set; }

        public decimal? PF { get; set; }

        public decimal? ProfessionalTax { get; set; }


        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
