using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnCourse.Turnstile.Mvc.Models;

namespace OnCourse.Turnstile.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateTurnstile]
    public IActionResult Index(ContactViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        TempData["Message"] = $"Email {model.Email} submitted successfully!";
        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
