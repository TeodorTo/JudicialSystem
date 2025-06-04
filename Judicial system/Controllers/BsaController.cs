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
            double fullBsa = Math.Sqrt((model.HeightCm * model.WeightKg) / 3600);
            model.BsaFullResult = fullBsa;
            model.BsaResult = Math.Round(fullBsa, 2);
        }

        return View(model);
    }

}
