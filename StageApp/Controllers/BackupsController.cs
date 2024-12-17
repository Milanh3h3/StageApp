using System;
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
    public class BackupsController : Controller
    {
        private readonly MerakiDbDbContext _context;

        public BackupsController(MerakiDbDbContext context)
        {
            _context = context;
        }

        // GET: Backups
        public async Task<IActionResult> Index()
        {
            return View(await _context.DeviceBackups.ToListAsync());
        }

        // GET: Backups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var backup = await _context.DeviceBackups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (backup == null)
            {
                return NotFound();
            }

            return View(backup);
        }

        // GET: Backups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var backup = await _context.DeviceBackups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (backup == null)
            {
                return NotFound();
            }

            return View(backup);
        }

        // POST: Backups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var backup = await _context.DeviceBackups.FindAsync(id);
            if (backup != null)
            {
                _context.DeviceBackups.Remove(backup);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BackupExists(int id)
        {
            return _context.DeviceBackups.Any(e => e.Id == id);
        }
    }
}
