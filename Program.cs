using DACN.Data;
using DACN.Repositories;
using DACN.Service.Email;
using DACN.Service.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDB")));
// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn đến trang từ chối truy cập
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Thời gian hết hạn cookie
        options.SlidingExpiration = true; // Gia hạn thời gian hết hạn khi người dùng hoạt động
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<JobPostingRepository>();
builder.Services.AddScoped<PositionRepository>();
builder.Services.AddScoped<JobApplicationRepository>();
builder.Services.AddScoped<ContractRepository>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<UserAccountRepository>();
builder.Services.AddScoped<SalaryRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddSignalR();
builder.Services.AddScoped<ApplicationConfirmationEmail>();
builder.Services.AddScoped<HrConfirmEmail>();
builder.Services.AddScoped<SendWelcomeEmail>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.MapHub<Notifications>("/Notifications");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthorization();
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account", area = "" }); // 👈 không nằm trong area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}",
    defaults: new { area = "Employee" });


app.Run();
