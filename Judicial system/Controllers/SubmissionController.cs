using Judicial_system.Data;
using Microsoft.CodeAnalysis.CSharp.Scripting;
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
    public async Task<IActionResult> Create(int TaskId, string SourceCode)
    {
        var task = await _context.Tasks.FindAsync(TaskId);
        if (task == null) return NotFound();

        var submission = new Submission
        {
            TaskId = TaskId,
            SourceCode = SourceCode,
            UserId = _userManager.GetUserId(User),
            SubmissionDate = DateTime.UtcNow,
            Score = EvaluateSolution(SourceCode, TaskId)
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Submission", new { id = submission.Id });
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

    
     //TODO FIX Csharp.Scripting nuget
     private decimal EvaluateSolution(string userCode, int taskId)
     {
         var task = _context.Tasks.Find(taskId);
         if (task == null) return 0;

         string unitTests = task.UnitTestCode;

         // Комбинираме кода на потребителя и тестовете
         string finalCode = $@"
        using System;
        public class Program 
        {{
            public static void Main() 
            {{
                var results = TestRunner.RunTests();
                Console.WriteLine(string.Join("","", results));
            }}
        }}
        {userCode}
        {unitTests}";

         try
         {
             var result = RunTests(finalCode).Result;

             int passedTests = result.Count(r => r);
             int totalTests = result.Length;

             return totalTests > 0 ? (decimal)passedTests / totalTests * 100 : 0;
         }
         catch (Exception ex)
         {
             Console.WriteLine("Грешка при изпълнението: " + ex.Message);
             return 0;
         }
     }



     private async Task<bool[]> RunTests(string code)
     {
         try
         {
             var scriptOptions = ScriptOptions.Default
                 .WithReferences(AppDomain.CurrentDomain.GetAssemblies())
                 .WithImports("System", "System.Linq", "System.Collections.Generic");

             var result = await CSharpScript.EvaluateAsync<string>(code, scriptOptions);

             // Логваме резултата за дебъг
             Console.WriteLine("Output: " + result);

             // Парсваме резултатите от Console.WriteLine
             return result.Split(',').Select(bool.Parse).ToArray();
         }
         catch (Exception ex)
         {
             Console.WriteLine("Error executing script: " + ex.Message);
         }

         return new bool[0];
     }

    
}
