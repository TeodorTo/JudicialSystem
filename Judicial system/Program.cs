using Judicial_system;
using Judicial_system.Data;
using Judicial_system.Middleware;
using Judicial_system.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Judicial_system.Hubs;
using Judicial_system.Models;
using Task = System.Threading.Tasks.Task;

var builder = WebApplication.CreateBuilder(args);

// =================== EMAIL SERVICES ===================
// Твоят собствен IEmailSender за вътрешна употреба
builder.Services.AddTransient<Judicial_system.Services.IEmailSender, EmailSender>();

// Системният IEmailSender (на Identity) чрез адаптера
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, IdentityEmailSenderAdapter>();

// BackgroundService за изпращане на мейли
builder.Services.AddSingleton<EmailBackgroundService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<EmailBackgroundService>());

// SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

// =================== DATABASE CONNECTION ===================
var envConnectionString = Environment.GetEnvironmentVariable("DB_SERVER") != null
    ? $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
      $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
      $"User Id={Environment.GetEnvironmentVariable("DB_USER")};" +
      $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
      $"TrustServerCertificate=True;"
    : builder.Configuration.GetConnectionString("DefaultConnection")
      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(envConnectionString));

// =================== IDENTITY ===================
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// =================== LOGGING ===================
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});

// =================== EMAIL SETTINGS ===================

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
/*
builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "";
    options.Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 587;
    options.SenderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL") ?? "";
    options.SenderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD") ?? "";
});
*/

// =================== MVC & SESSION ===================
builder.Services.AddControllersWithViews();

// Maintenance mode flag
Judicial_system.AppState.MaintenanceMode = builder.Configuration.GetValue<bool>("MaintenanceMode");

// Sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// =================== PIPELINE ===================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMaintenanceMode();

// =================== ROUTES ===================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");
app.MapHub<CallHub>("/callHub");
app.MapRazorPages();

// =================== ROLES INIT ===================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await InitializeRoles(services);
}

app.Run();

// =================== ROLE SEED ===================
static async Task InitializeRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string adminEmail = "admin@example.com";
    string adminPassword = "Admin@123";

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(adminUser, adminPassword);
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
