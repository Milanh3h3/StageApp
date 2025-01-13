using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Elfie.Serialization;
using Humanizer;
using Meraki.Api.Data;
using Meraki.Api.Interfaces.General.Devices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StageApp;
using StageApp.Excel;
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
        // GET: Devices/Create
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
                ViewBag.Message = "Devices claimed successfully";
                return View(model);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }

        // GET: Devices/Rename
        [HttpGet]
        public IActionResult Rename()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new RenameDevicesViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(RenameDevicesViewModel model, bool MakeBackup)
        {
            model.SerialNumbers = model.SerialNumbers.SelectMany(s => s.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            model.NewNames = model.NewNames.SelectMany(n => n.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();

            if (model.SerialNumbers.Count != model.NewNames.Count)
            {
                ModelState.AddModelError(string.Empty, "The number of serial numbers must match the number of new names.");
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!InitializeMerakiApi())
            {
                ModelState.AddModelError(string.Empty, "Failed to initialize Meraki API.");
                return View(model);
            }
            if (!MakeBackup)
            {
                try
                {
                    foreach (var (serial, name) in model.SerialNumbers.Zip(model.NewNames))
                    {
                        await _merakiApi.RenameDeviceAsync(name, serial);
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                    ViewBag.Message = "Devices renamed successfully";
                    return View(model);
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                    return View(model);
                }
            }
            else //if make backup checkbox is on
            {
                try
                {
                    List<string[]> NamesAndSerials = [];
                    string[] Header = ["Serial Numbers", "Old Names", "New Names"];
                    NamesAndSerials.Append(Header);
                    foreach (var (serial, name) in model.SerialNumbers.Zip(model.NewNames))
                    {
                        string OldName = await _merakiApi.GetDeviceNameAsync(serial);
                        await Task.Delay(350); // ongeveer 3 calls per second
                        await _merakiApi.RenameDeviceAsync(name, serial);
                        await Task.Delay(350); // ongeveer 3 calls per second
                        string[] DataRow = [serial, OldName, name];
                        NamesAndSerials.Append(DataRow);
                    }
                    string backupDirectory = Path.Combine("RenameBackups");
                    string fileName = $"Backup_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
                    ExcelWriter.SetExcel(Path.Combine(backupDirectory, fileName), NamesAndSerials); // moet nog iets doen voor de filepath
                    ViewBag.Message = "Devices renamed successfully";
                    return View(model);
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                    return View(model);
                }
            }
        }
        // GET: Devices/Rename
        [HttpGet]
        public IActionResult RenameBackups()
        {
            string backupsFolder = Path.Combine("RenameBackups");
            var files = Directory.GetFiles(backupsFolder);
            var model = files.Select(file =>
            {
                var fileInfo = new FileInfo(file);
                return new RenameBackupsViewModel
                {
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    LastModified = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length
                };
            }).ToList();

            return View(model);
        }
        [HttpGet]
        public IActionResult Download(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var fileName = Path.GetFileName(filePath); // Extract only the file name
                return File(fileBytes, "application/octet-stream", fileName);
            }
            return NotFound("The requested file does not exist.");
        }
        // GET: Devices/Rename
        [HttpGet]
        public IActionResult SetLocation()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new SetLocationViewModel();
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetLocation(SetLocationViewModel model)
        {
            model.SerialNumbers = model.SerialNumbers.SelectMany(s => s.Split(new[] { '\r', '\n', ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();

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
                foreach (string serial in model.SerialNumbers)
                {
                    await _merakiApi.SetDeviceAddressAsync(serial, model.Address, model.Notes);
                    await Task.Delay(350); // ongeveer 3 calls per second
                }
                ViewBag.Message = "Devices location set successfully";
                return View(model);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }

        }
    }
}
