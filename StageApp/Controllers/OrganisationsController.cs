using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StageApp;
using StageApp.ViewModels;
using StageApp.Meraki_API;

namespace StageApp.Controllers
{
    public class OrganisationsController : Controller
    {
        private MerakiApiHelper? _merakiApi;

        public OrganisationsController()
        {
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

        [HttpGet]
        public IActionResult Create()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrganizationViewModel model)
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
                await _merakiApi.CreateOrganizationAsync(model.Name);
                ViewBag.Message = $"Organization '{model.Name}' created successfully!";
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }
        public async Task<IActionResult> OrganizationID()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }
            try
            {
                var Organizations = await _merakiApi.GetOrganizations();
                return View(Organizations);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error fetching networks: {ex.Message}");
                return View(new List<(string, string)>());
            }
        }
    }
}
