using AttendanceCRM.BAL.IServices;
using AttendanceCRM.BAL.Services;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .AddRazorRuntimeCompilation();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnection")), ServiceLifetime.Transient);

builder.Services.AddRazorPages();
builder.Services.AddCors();
 

double sessionTimeout = Convert.ToDouble(builder.Configuration["sessionTimeOut"]);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeout);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddScoped<ILeaveTypeMaster, LeaveTypeMaster>();
builder.Services.AddScoped<IMasterMgmtService, MasterMgmtService>();
builder.Services.AddScoped<IUserService, UserMgmtService>();
builder.Services.AddScoped<ILeaveTypeMaster, LeaveTypeMaster>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Authenticate/Login";
		options.AccessDeniedPath = "/Authenticate/Login";
	});



builder.Services.AddAuthorization(options =>
{
	// Policy for AdminDashboard (SuperAdmin and Admin only)
	options.AddPolicy("AdminAccess", policy =>
		policy.RequireClaim("UserType", "SuperAdmin", "Admin", "HR"));

	// Policy for EmployeeDashboard (SuperAdmin, Admin, Employee)
	options.AddPolicy("EmployeeAccess", policy =>
		policy.RequireClaim("UserType", "SuperAdmin", "Admin", "Employee", "HR", "OnBoard Trainee"));
	options.AddPolicy("Employee", policy =>
		policy.RequireClaim("UserType","Employee", "OnBoard Trainee"));
	options.AddPolicy("HRAccess", policy =>
		policy.RequireClaim("UserType", "HR"));
	options.AddPolicy("EmployeeOrHRAccess", policy =>
	   policy.RequireRole("EmployeeAccess", "HR", "OnBoard Trainee"));
});





builder.Services.AddControllersWithViews()
	.AddRazorOptions(options =>
	{
		options.ViewLocationFormats.Add("/Views/Shared/_Layout.cshtml");
	});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
 

app.UseStaticFiles();
app.UseRouting();
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authenticate}/{action=Login}/{id?}");



app.Run();
