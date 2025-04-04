using Judicial_system.Data;
using Task = Judicial_system.Data.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Judicial_system.Controllers;

[Authorize]
public class TaskController : Controller
{
    private readonly ApplicationDbContext _context;

    public TaskController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Tasks.ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Task task)
    {
        if (ModelState.IsValid)
        {
            task.CreatedAt = DateTime.Now;
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(task);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Task task)
    {
        if (id != task.Id) return NotFound();

        if (ModelState.IsValid)
        {
            task.CreatedAt = DateTime.Now;
            _context.Update(task);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(task);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            if (!string.IsNullOrEmpty(task.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", task.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadFile(int taskId, IFormFile file)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return NotFound();

        if (file != null && file.Length > 0)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                task.FileContent = memoryStream.ToArray();
            }
            task.FileName = file.FileName;
            task.FilePath = null;

            _context.Update(task);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { id = taskId });
    }

    [HttpGet]
    public IActionResult DownloadFile(int taskId)
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null || task.FileContent == null || task.FileContent.Length == 0)
        {
            return NotFound();
        }

        return File(task.FileContent, "application/octet-stream", task.FileName);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateLink([FromBody] JsonElement body)
    {
        Console.WriteLine("▶ Получена заявка към /Task/GenerateLink");

        if (!body.TryGetProperty("taskId", out var taskIdProperty))
        {
            Console.WriteLine("❌ ГРЕШКА: Липсва 'taskId' в заявката!");
            return BadRequest(new { message = "Грешен формат на заявката." });
        }

        int taskId = taskIdProperty.GetInt32();
        Console.WriteLine($"✔ Получен taskId: {taskId}");

        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            Console.WriteLine("❌ ГРЕШКА: Няма такъв Task в базата!");
            return NotFound(new { message = "Задачата не е намерена." });
        }

        if (task.FileContent == null)
        {
            Console.WriteLine("❌ ГРЕШКА: Task съществува, но няма прикачен файл!");
            return NotFound(new { message = "Файлът не съществува или не е качен." });
        }

        task.ShareableLink = Guid.NewGuid().ToString();
        task.ExpirationDate = DateTime.UtcNow.AddDays(1);
        await _context.SaveChangesAsync();

        var fullUrl = $"{Request.Scheme}://{Request.Host}/Task/DownloadShared/{task.ShareableLink}";
        Console.WriteLine($"✅ Генериран линк: {fullUrl}");

        return Json(new { ShareableUrl = fullUrl });
    }
    


    [HttpGet("Task/DownloadShared/{shareableLink}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadShared(string shareableLink)
    {
        Console.WriteLine($"▶ Заявка за DownloadShared с линк: {shareableLink}");
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.ShareableLink == shareableLink && t.ExpirationDate > DateTime.UtcNow);

        if (task == null)
        {
            Console.WriteLine("❌ Задача не е намерена или линкът е изтекъл.");
            return NotFound("Файлът не съществува или линкът е изтекъл.");
        }

        if (task.FileContent == null)
        {
            Console.WriteLine("❌ Файлът липсва в задачата.");
            return NotFound("Файлът не съществува или линкът е изтекъл.");
        }

        Console.WriteLine("✅ Файлът се изпраща успешно.");
        return File(task.FileContent, "application/octet-stream", task.FileName);
    }
}
