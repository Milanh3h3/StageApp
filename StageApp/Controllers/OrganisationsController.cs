﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StageApp;
using StageApp.Models;

namespace StageApp.Controllers
{
    public class OrganisationsController : Controller
    {
        private readonly MerakiDbDbContext _context;

        public OrganisationsController(MerakiDbDbContext context)
        {
            _context = context;
        }

        // GET: Organisations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Organisations.ToListAsync());
        }

        // GET: Organisations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Organisations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ApiKey")] Organisation organisation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(organisation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(organisation);
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
