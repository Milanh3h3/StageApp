using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StageApp;
using StageApp.Meraki_API;
using StageApp.Models;
using StageApp.ViewModels;

namespace StageApp.Controllers
{
    public class NetworksController : Controller
    {
        private MerakiApiHelper? _merakiApi;

        public NetworksController()
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

        // GET: Networks/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CreateNetworkViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNetworkViewModel model)
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
                string[] productTypes = model.SelectedNetworkTypes.ToArray();
                await _merakiApi.CreateNetworkAsync(model.OrganizationId, model.Name, productTypes, model.Timezone);
                ViewBag.Message = $"Network '{model.Name}' created successfully!";
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }
        public async Task<IActionResult> NetworkID(string organizationId)
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(organizationId))
            {
                ModelState.AddModelError(string.Empty, "Organization ID is required.");
                return View(new List<(string, string)>());
            }

            try
            {
                var networks = await _merakiApi.GetNetworks(organizationId);
                return View(networks);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error fetching networks: {ex.Message}");
                return View(new List<(string, string)>());
            }
        }
    }
}
