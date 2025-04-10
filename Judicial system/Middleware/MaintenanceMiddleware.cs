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

        // Разреши достъп до страницата за логин по време на поддръжка
        bool isLoginPath = path.StartsWith("/Identity/Account/Login", StringComparison.OrdinalIgnoreCase);

        if (AppState.MaintenanceMode && !context.User.IsInRole("Admin") && !isLoginPath)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "text/html; charset=utf-8";

            var maintenanceHtml = await System.IO.File.ReadAllTextAsync("Views/Home/Maintenance.cshtml");
            await context.Response.WriteAsync(maintenanceHtml);
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