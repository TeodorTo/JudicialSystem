using Judicial_system.Models;
using Microsoft.AspNetCore.Mvc;

namespace Judicial_system.Controllers;

public class BsaController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new BsaModel());
    }

    [HttpPost]
    public IActionResult Index(BsaModel model)
    {
        if (ModelState.IsValid)
        {
            // Формулата на Mosteller:
            // BSA = sqrt((ръст * тегло) / 3600)
            double bsa = Math.Sqrt((model.HeightCm * model.WeightKg) / 3600);
            model.BsaResult = Math.Round(bsa, 2); // Закръгляме до 2 знака
        }

        return View(model);
    }
}
