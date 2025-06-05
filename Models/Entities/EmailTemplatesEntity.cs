using System.ComponentModel.DataAnnotations;

namespace AttendanceCRM.Models.Entities
{
	public class EmailTemplatesEntity
	{
		[Key]
		public int EMailTemplateID { get; set; }

		public string ModuleName { get; set; }

		public string Subject { get; set; }

		public string EmailTemplate { get; set; }

		public DateTime? CreatedOn { get; set; }

		public int? CreatedBy { get; set; }

		public DateTime? LastModifiedOn { get; set; }

		public int? LastModifiedBy { get; set; }

		public bool? IsDeleted { get; set; }
	}
}
