using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageApp.Models;
using System.Diagnostics;

namespace StageApp.Controllers
{
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAPIkey([Bind("API_key")] string API_key)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("API_Key", API_key); //moet nog feedback of het gelukt is   
            }
            return RedirectToAction(nameof(Index));
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
