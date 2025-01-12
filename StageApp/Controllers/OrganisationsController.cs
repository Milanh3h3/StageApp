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
        private readonly MerakiDbDbContext _context;
        private MerakiApiHelper? _merakiApi;

        public OrganisationsController(MerakiDbDbContext context)
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

        // GET: Organisations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Organisations.ToListAsync());
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


        // GET: Organisations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organisation = await _context.Organisations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organisation == null)
            {
                return NotFound();
            }

            return View(organisation);
        }

        // POST: Organisations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var organisation = await _context.Organisations.FindAsync(id);
            if (organisation != null)
            {
                _context.Organisations.Remove(organisation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganisationExists(int id)
        {
            return _context.Organisations.Any(e => e.Id == id);
        }
    }
}
