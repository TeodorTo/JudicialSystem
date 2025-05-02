using Judicial_system.Data;
using Judicial_system.Middleware;
using Judicial_system.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections;
using Judicial_system.Hubs;
using Judicial_system.Models;
using Task = System.Threading.Tasks.Task;

// Create builder
var builder = WebApplication.CreateBuilder(args);

// Configure EmailSender
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSingleton<EmailBackgroundService>(); // Добавяме като Singleton
builder.Services.AddSignalR();
builder.Services.AddHostedService(provider => provider.GetRequiredService<EmailBackgroundService>());

var envConnectionString = Environment.GetEnvironmentVariable("DB_SERVER") != null
    ? $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
      $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
      $"User Id={Environment.GetEnvironmentVariable("DB_USER")};" +
      $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};" +
      $"TrustServerCertificate=True;"
    : builder.Configuration.GetConnectionString("DefaultConnection") 
      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(envConnectionString));

// Add Identity
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});

// Load EmailSettings from environment variables
builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "";
    options.Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var port) ? port : 587;
    options.SenderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL") ?? "";
    options.SenderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD") ?? "";
});

// MVC
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

// Dev / Prod behavior
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

// Add session middleware before authentication and routing
app.UseSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMaintenanceMode();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.MapRazorPages();

// Role init
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await InitializeRoles(services);
}

app.Run();

// Create initial roles and admin user
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
