using Judicial_system.Data;
using Task = Judicial_system.Data.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Create(Data.Task task)
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
            // Изтриване на файла, ако съществува
            if (!string.IsNullOrEmpty(task.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", task.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Изтриване на файла от сървъра
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
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder); // Създава папката, ако не съществува

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            task.FilePath = "/uploads/" + file.FileName;
            _context.Update(task);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Details", new { id = taskId });
    }

    [HttpGet]
    public IActionResult DownloadFile(int taskId)
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null || string.IsNullOrEmpty(task.FilePath)) return NotFound();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", task.FilePath.TrimStart('/'));
        if (!System.IO.File.Exists(filePath)) return NotFound();

        var contentType = "application/octet-stream";
        return PhysicalFile(filePath, contentType, Path.GetFileName(filePath));
    }


    
}

