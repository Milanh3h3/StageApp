using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Meraki.Api.Data;
using Meraki.Api.Interfaces.General.Devices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StageApp;
using StageApp.Meraki_API;
using StageApp.Models;
using StageApp.ViewModels;

namespace StageApp.Controllers
{
    public class DevicesController : Controller
    {
        private readonly MerakiDbDbContext _context;
        private MerakiApiHelper? _merakiApi;

        public DevicesController(MerakiDbDbContext context)
        {
            _context = context;
        }
        private bool InitializeMerakiApi()
        {
            if (Request.Cookies.TryGetValue("API_Key", out string? apiKey) && !string.IsNullOrEmpty(apiKey))
            {
                _merakiApi = new MerakiApiHelper(apiKey);
                return true;
            }
            return false;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Devices.ToListAsync());
        }
        // GET: Networks/Create
        [HttpGet]
        public IActionResult Claim()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ClaimDevicesViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(ClaimDevicesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!InitializeMerakiApi())
            {
                ModelState.AddModelError(string.Empty, "Failed to initialize Meraki API.");
                return View(model);
            }

            try
            {
                await _merakiApi.ClaimDevicesAsync(model.NetworkId, model.SerialNumbers);
                ViewBag.Message = "Device claimed successfully";
                return View(model);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }
    
        public IActionResult SetLocation()
        {
            return View();
        }
        public IActionResult Rename()
        {
            return View();
        }
    }
}
