namespace AttendanceCRM.Models.DTOS
{
	public class ModulePermissionsDTO
	{
		public int ModuleId { get; set; }

		public string? ModuleName { get; set; }

		public bool? IsEnabled { get; set; }

		public bool? CanRead { get; set; }

		public bool? CanWrite { get; set; }

		public bool? CanCreate { get; set; }

		public bool? CanDelete { get; set; }

		public bool? CanImport { get; set; }

		public bool? CanExport { get; set; }

		public bool? IsActive { get; set; }

		public bool? IsDeleted { get; set; }

		public DateTime? CreatedOn { get; set; }

		public int? CreatedBy { get; set; }

		public DateTime? UpdatedOn { get; set; }

		public int? UpdatedBy { get; set; }
	}
}
