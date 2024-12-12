using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StageApp;
using StageApp.Models;

namespace StageApp.Controllers
{
    public class DevicesController : Controller
    {
        private readonly MerakiDbDbContext _context;

        public DevicesController(MerakiDbDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Devices.ToListAsync());
        }
        public Task<IActionResult> ClaimDevices()
        {
            return Task.FromResult<IActionResult>(View());
        }
        public Task<IActionResult> AssignToLocation()
        {
            return Task.FromResult<IActionResult>(View());
        }
        public Task<IActionResult> RenameDevices()
        {
            return Task.FromResult<IActionResult>(View());
        }
        public Task<IActionResult> BackupNames()
        {
            return Task.FromResult<IActionResult>(View());
        }

    }
}
