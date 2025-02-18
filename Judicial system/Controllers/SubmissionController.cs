using Judicial_system.Data;
//using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Judicial_system.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize] 
public class SubmissionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public SubmissionController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<IActionResult> Create(int taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return NotFound();

        return View(new Submission { TaskId = taskId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Submission submission)
    {
        if (ModelState.IsValid)
        {
            submission.UserId = _userManager.GetUserId(User);
            submission.SubmissionDate = DateTime.UtcNow;


       //     submission.Score = EvaluateSolution(submission.SourceCode, submission.TaskId);

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = submission.Id });
        }
        return View(submission);
    }


    public async Task<IActionResult> Details(int id)
    {
        var submission = await _context.Submissions
            .Include(s => s.Task)
            .Include(s => s.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (submission == null) return NotFound();
        return View(submission);
    }

    /*
     // TODO FIX Csharp.Scripting nuget
    private decimal EvaluateSolution(string userCode, int taskId)
    {

        var task = _context.Tasks.Find(taskId);
        if (task == null) return 0;

        string unitTests = task.UnitTestCode;


        string finalCode = $"{userCode}\n{unitTests}";

        try
        {

            var result = RunTests(finalCode).Result;


            var passedTests = result.Count(r => r);
            var totalTests = result.Length;
            return totalTests > 0 ? (decimal)passedTests / totalTests * 100 : 0;
        }
        catch
        {
            return 0;
        }
    }


    private async Task<bool[]> RunTests(string code)
    {
        try
        {
            // Изпълняваме динамично подадения C# код
            var scriptOptions = ScriptOptions.Default
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies()) // Зареждаме нужните библиотеки
                .WithImports("System", "System.Linq", "System.Collections.Generic");

            var script = await CSharpScript.EvaluateAsync<object>(code, scriptOptions);

            // Ако в кода има метод "RunTests", извикваме го
            var testMethod = script?.GetType().GetMethod("RunTests");
            if (testMethod != null)
            {
                var testResults = (bool[])testMethod.Invoke(script, null);
                return testResults;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing script: " + ex.Message);
        }

        return new bool[0]; // Ако не можем да изпълним тестовете, връщаме празен масив
    }
    */
}
