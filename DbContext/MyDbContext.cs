using AttendanceCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;


 
    public class MyDbContext : DbContext
    {

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserMasterEntitie>().ToTable("UserMaster");
        modelBuilder.Entity<UserTypeMasterEntitie>().ToTable("UserTypeMaster");
        modelBuilder.Entity<AttendanceEntitie>().ToTable("Attendance");
        modelBuilder.Entity<BankDetailsEntitie>().ToTable("BankDetails");
        modelBuilder.Entity<HolidaysEntite>().ToTable("Holidays");
        modelBuilder.Entity<PayslipEntitie>().ToTable("Payslip");
        modelBuilder.Entity<LeavesEntitie>().ToTable("Leaves");
        modelBuilder.Entity<ModulePermissionsEntity>().ToTable("ModulePermissions");
        modelBuilder.Entity<OTPDetailsEntity>().ToTable("OTPDetails");
        modelBuilder.Entity<EmailTemplatesEntity>().ToTable("EmailTemplates");
        modelBuilder.Entity<LeavetypeEntity>().ToTable("LeaveType");
        modelBuilder.Entity<NotificationEntity>().ToTable("Notifications");
        modelBuilder.Entity<EmployeeStatusEntity>().ToTable("EmployeeStatus");
        modelBuilder.Entity<WorkEntryEntity>().ToTable("WorkEntry");
        modelBuilder.Entity<ProjectEntity>().ToTable("Projects");
        modelBuilder.Entity<ProjectAssignment>().ToTable("ProjectAssignments");
        modelBuilder.Entity<TeamMemberEntity>().ToTable("TeamMembers");
        modelBuilder.Entity<TaskEntity>().ToTable("Tasks");
        modelBuilder.Entity<SprintEntity>().ToTable("Sprints");
        modelBuilder.Entity<BugTracking>().ToTable("BugTracking");
        modelBuilder.Entity<PerformanceDataEntity>().ToTable("PerformanceData");
        modelBuilder.Entity<BirthdayWishesEntity>().ToTable("BirthdayWishes");
        modelBuilder.Entity<DepartmentEntity>().ToTable("Department");
        modelBuilder.Entity<SalaryStructureEntity>().ToTable("SalaryStructure");
        modelBuilder.Entity<SalarysEntity>().ToTable("Salarys");
        modelBuilder.Entity<Incidents>().ToTable("Incidents");

    }


    public DbSet<BugTracking> bugTrackings { get; set; }
    public DbSet<SprintEntity> sprintEntities { get; set; }
    public DbSet<TaskEntity> taskEntities { get; set; }
    public DbSet<TeamMemberEntity> teamMemberEntities { get; set; }
    public DbSet<ProjectEntity> projectEntities { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
    public DbSet<UserMasterEntitie> userMasterEntitie { get; set; }
    public DbSet<WorkEntryEntity> WorkEntries { get; set; }
    public DbSet<UserTypeMasterEntitie> userTypeMasterEntitie { get; set; }
    public DbSet<AttendanceEntitie> attendanceEntitie { get; set; }
    public DbSet<BankDetailsEntitie> bankDetailsEntitie { get; set; }
    public DbSet<HolidaysEntite> holidaysEntite { get; set; }
    public DbSet<PayslipEntitie> payslipEntitie { get; set; }
    public DbSet<LeavesEntitie> leavesEntitie { get; set; }
    public DbSet<ModulePermissionsEntity> modulePermissionsEntity { get; set; }
    public DbSet<OTPDetailsEntity> otpDetailsEntity { get; set; }
    public DbSet<EmailTemplatesEntity> emailTemplatesEntity { get; set; }
    public DbSet<LeavetypeEntity> leaveType { get; set; }
    public DbSet<NotificationEntity> notificationEntity { get; set; }
    public DbSet<EmployeeStatusEntity> employeeStatusEntity { get; set; }
    public DbSet<PerformanceDataEntity> PerformanceData { get; set; }
    public DbSet<BirthdayWishesEntity> BirthdayWishes { get; set; }
    public DbSet<DepartmentEntity> department { get; set; }
    public DbSet<SalaryStructureEntity> salaryStructure { get; set; }
    public DbSet<SalarysEntity> salary{ get; set; }
    public DbSet<Incidents> incidents { get; set; }
}
