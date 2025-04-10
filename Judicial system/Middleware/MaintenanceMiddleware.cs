namespace Judicial_system.Middleware;

public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public MaintenanceMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        Console.WriteLine($"MaintenanceMode: {AppState.MaintenanceMode}, IsAdmin: {context.User.IsInRole("Admin")}, Path: {path}");

        // Разреши достъп до至于: Тук добавям малко повече логика за логин страницата
        bool isLoginPath = path.StartsWith("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase);

        if (AppState.MaintenanceMode && !context.User.IsInRole("Admin") && !isLoginPath)
        {
            context.Response.Redirect("/Home/Maintenance");
            return;
        }

        await _next(context);
    }
}

public static class MaintenanceMiddlewareExtensions
{
    public static IApplicationBuilder UseMaintenanceMode(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MaintenanceMiddleware>();
    }
}