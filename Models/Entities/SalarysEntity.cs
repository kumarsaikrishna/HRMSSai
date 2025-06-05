using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
    public class SalarysEntity
    {
        [Key]
    public int SalaryId { get; set; }

        public int? UserId { get; set; }

        public int? UserTypeId { get; set; }

        public decimal? BasicSalary { get; set; }

        public decimal? HRA { get; set; }

        public decimal? SpecialAllowance { get; set; }

        public decimal? Conveyance { get; set; }

        public decimal? PF { get; set; }

        public decimal? ProfessionalTax { get; set; }

        public decimal? Bonus { get; set; }

        public decimal? Deductions { get; set; }

        public DateTime? CreditedOn { get; set; }

    

}
}
