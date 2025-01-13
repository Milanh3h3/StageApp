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
        // Action to store the API key in a secure cookie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetAPIkey(string API_key)
        {
            if (string.IsNullOrEmpty(API_key))
            {
                ModelState.AddModelError("", "API key cannot be empty.");
                return View();
            }

            // Set the API key in a secure cookie
            CookieOptions options = new CookieOptions
            {
                HttpOnly = true, // Prevent client-side scripts from accessing the cookie
                Expires = DateTimeOffset.UtcNow.AddMinutes(30), // Set an expiration time
                SameSite = SameSiteMode.Strict,
                Secure = true,
            };

            Response.Cookies.Append("API_Key", API_key, options);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
