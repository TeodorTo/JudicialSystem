using System.Reflection;
using Judicial_system.Data;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Judicial_system.Controllers;



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

    
  
     private decimal EvaluateSolution(string userCode, int taskId)
     {
         var task = _context.Tasks.Find(taskId);
         if (task == null) return 0;

         string unitTests = task.UnitTestCode;

         // Лог: Проверяваме дали userCode съдържа Solution
         Console.WriteLine("User Code: " + userCode);
         Console.WriteLine("Unit Tests: " + unitTests);

         string finalCode = $@"
using System;
public class Solution
{{
    {userCode}
}}

{unitTests}

TestRunner testRunner = new TestRunner();
testRunner.RunTests()";


         // Лог: Проверяваме финалния код
         Console.WriteLine("Final Code: " + finalCode);

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
                 .WithReferences(
                     typeof(object).Assembly,
                     typeof(Console).Assembly,
                     typeof(Enumerable).Assembly,
                     Assembly.GetExecutingAssembly() 
                 )
                 .WithImports("System", "System.Linq", "System.Console");

             Console.WriteLine("DEBUG: code: => " + code);
        
             var result = await CSharpScript.EvaluateAsync<bool[]>(code, scriptOptions);

             Console.WriteLine("Output: " + (result != null ? string.Join(",", result) : "NULL"));
        
             return result ?? [];
         }
         catch (Exception ex)
         {
             Console.WriteLine("Error executing script: " + ex.Message);
             return [];
         }
     }





    
}



