using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.DTOS
{
    public class BankDetailsDTO
    {
        public int BankId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [RegularExpression(@"^\d{9,18}$", ErrorMessage = "Account number must be between 9 to 18 digits.")]
        public string AccountNumber { get; set; }

        [Required]
        [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Enter a valid IFSC code.")]
        public string IFSCNumber { get; set; }

        [Required]
        public string BranchName { get; set; }

        [Required]
        public string AccountType { get; set; }
        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public string? UserName { get; set; }
    }
}
