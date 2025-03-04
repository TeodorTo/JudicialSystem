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
            .Include(s => s.Task)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (submission == null) return NotFound();
        return View(submission);
    }

    
  
    private decimal EvaluateSolution(string userCode, int taskId)
    {
        var task = _context.Tasks.Find(taskId);
        if (task == null) return 0;

        string finalCode;

        switch (task.Type)
        {
            case TaskType.Method:
                finalCode = $@"
using System;
public class Solution
{{
    {userCode}
}}
{task.UnitTestCode}
TestRunner testRunner = new TestRunner();
testRunner.RunTests();";
                break;

            case TaskType.Class:
                finalCode = $@"
using System;
{userCode}
{task.UnitTestCode}
TestRunner testRunner = new TestRunner();
testRunner.RunTests();";
                break;

            case TaskType.ConsoleIO:
                finalCode = $@"
using System;
using System.IO;

public class Program
{{
    public static void Main()
    {{
        {userCode}
    }}
}}";

                break;

            default:
                return 0;
        }

        var testResults = RunTests(finalCode).Result;

        if (testResults.Length == 0)
            return 0;

        // Броим успешните тестове и смятаме резултата в проценти
        int passedTests = testResults.Count(r => r);
        decimal score = (decimal)passedTests / testResults.Length * 100;

        return score;
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
                .WithImports("System", "System.Linq", "System.Console", "System.IO");

            if (code.Contains("Console.ReadLine()"))
            {
                // Емулираме входните данни
                string inputData = "5\n10\n"; // Примерен вход
                using (StringReader sr = new StringReader(inputData))
                {
                    Console.SetIn(sr);
                    var result = await CSharpScript.EvaluateAsync<bool[]>(code, scriptOptions);
                    return result ?? [];
                }
            }
            else
            {
                var result = await CSharpScript.EvaluateAsync<bool[]>(code, scriptOptions);
                return result ?? [];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error executing script: " + ex.Message);
            return [];
        }
    }






    
}