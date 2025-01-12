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
        private readonly MerakiDbDbContext _context;
        private MerakiApiHelper? _merakiApi;

        public NetworksController(MerakiDbDbContext context)
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
        // GET: networks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Networks.ToListAsync());
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

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var network = await _context.Networks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (network == null)
            {
                return NotFound();
            }

            return View(network);
        }

        // POST: networks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var network = await _context.Networks.FindAsync(id);
            if (network != null)
            {
                _context.Networks.Remove(network);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool networkExists(int id)
        {
            return _context.Networks.Any(e => e.Id == id);
        }
    }
}
