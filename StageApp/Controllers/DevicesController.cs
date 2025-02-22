﻿using System;
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
        private MerakiApiHelper? _merakiApi;

        public DevicesController()
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

        // GET: Devices/Claim
        [HttpGet]
        public async Task<IActionResult> Claim()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            var organizations = await _merakiApi.GetOrganizations();
            var model = new ClaimDevicesViewModel
            {
                Organizations = organizations.Select(o => new SelectListItem
                {
                    Value = o.OrganizationId,
                    Text = o.OrganizationName
                }).ToList(),
                Networks = new List<SelectListItem>() // Initially empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNetworks(ClaimDevicesViewModel model)
        {
            if (!InitializeMerakiApi())
            {
                ModelState.AddModelError(string.Empty, "Failed to initialize Meraki API.");
                return RedirectToAction(nameof(Claim));
            }
            var organizations = await _merakiApi.GetOrganizations();
            var networks = await _merakiApi.GetNetworks(model.OrganizationId);

            model.Organizations = organizations.Select(o => new SelectListItem
            {
                Value = o.OrganizationId,
                Text = o.OrganizationName,
                Selected = o.OrganizationId == model.OrganizationId
            }).ToList();

            model.Networks = networks.Select(n => new SelectListItem
            {
                Value = n.NetworkId,
                Text = n.NetworkName
            }).ToList();

            return View("Claim", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(ClaimDevicesViewModel model)
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
                await _merakiApi.ClaimDevicesAsync(model.NetworkId, model.SerialNumbers);
                ViewBag.Message = "Devices claimed successfully";
                var organizations = await _merakiApi.GetOrganizations();
                model.Organizations = organizations.Select(o => new SelectListItem { Value = o.OrganizationId, Text = o.OrganizationName });
                return View(model);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                var organizations = await _merakiApi.GetOrganizations();
                model.Organizations = organizations.Select(o => new SelectListItem { Value = o.OrganizationId, Text = o.OrganizationName });

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClaimBulk(IFormFile excelFile)
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid Excel file.");
                return RedirectToAction("Claim");
            }

            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(stream);
                }

                var excelData = ExcelReader.GetExcel(tempFilePath);
                if (excelData == null || excelData.Count == 0)
                {
                    ModelState.AddModelError("", "The uploaded file is empty or invalid.");
                    return RedirectToAction("Claim");
                }

                var devicesByNetwork = new Dictionary<string, List<string>>();
                foreach (var row in excelData)
                {
                    if (row.Length < 2) continue; 
                    string serialNumber = row[0]?.Trim();
                    string networkId = row[1]?.Trim();
                    if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(networkId))
                    {
                        continue;
                    }
                    if (!devicesByNetwork.ContainsKey(networkId))
                    {
                        devicesByNetwork[networkId] = new List<string>();
                    }
                    devicesByNetwork[networkId].Add(serialNumber);
                }
                foreach (var kvp in devicesByNetwork)
                {
                    string networkId = kvp.Key;
                    List<string> serialNumbers = kvp.Value;
                    await _merakiApi.ClaimDevicesAsync(networkId, serialNumbers);
                    await Task.Delay(350); // ongeveer 3 calls per second
                }
                ViewBag.Message = "Devices successfully claimed from the uploaded file.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
                return RedirectToAction("Claim");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
            return RedirectToAction("Claim");
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
                        await _merakiApi.RenameDeviceAsync(serial, name);
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
                    NamesAndSerials.Add(Header);
                    foreach (var (serial, name) in model.SerialNumbers.Zip(model.NewNames))
                    {
                        string OldName = await _merakiApi.GetDeviceNameAsync(serial);
                        await Task.Delay(350); // ongeveer 3 calls per second
                        await _merakiApi.RenameDeviceAsync(serial, name);
                        await Task.Delay(350); // ongeveer 3 calls per second
                        string[] DataRow = [serial, OldName, name];
                        NamesAndSerials.Add(DataRow);
                    }
                    string backupDirectory = Path.Combine("RenameBackups");
                    string fileName = $"Backup_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
                    ExcelWriter.SetExcel(Path.Combine(backupDirectory, fileName), NamesAndSerials);
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
        [HttpPost]
        public async Task<IActionResult> RenameBulk(IFormFile excelFile, bool MakeBackupBulk)
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid Excel file.");
                return RedirectToAction("Rename");
            }
            if (!MakeBackupBulk)
            {
                var tempFilePath = Path.GetTempFileName();
                try
                {
                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await excelFile.CopyToAsync(stream);
                    }

                    var excelData = ExcelReader.GetExcel(tempFilePath);
                    if (excelData == null || excelData.Count == 0)
                    {
                        ModelState.AddModelError("", "The uploaded file is empty or invalid.");
                        return RedirectToAction("Rename");
                    }
                    foreach (var row in excelData)
                    {
                        if (row.Length < 2) continue;
                        string serial = row[0];
                        string Name = row[1];
                        if (string.IsNullOrEmpty(serial)) continue;
                        await _merakiApi.RenameDeviceAsync(serial, Name);
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                    ViewBag.Message = "Devices successfully renamed from the uploaded file.";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
                    return RedirectToAction("Rename");
                }
                finally
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }
            }
            else
            {
                var tempFilePath = Path.GetTempFileName();
                try
                {
                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await excelFile.CopyToAsync(stream);
                    }

                    var excelData = ExcelReader.GetExcel(tempFilePath);
                    if (excelData == null || excelData.Count == 0)
                    {
                        ModelState.AddModelError("", "The uploaded file is empty or invalid.");
                        return RedirectToAction("Rename");
                    }
                    List<string[]> NamesAndSerials = [];
                    string[] Header = ["Serial Numbers", "Old Names", "New Names"];
                    NamesAndSerials.Add(Header);
                    foreach (var row in excelData)
                    {
                        if (row.Length < 2) continue;
                        string serial = row[0];
                        string Name = row[1];
                        if (string.IsNullOrEmpty(serial)) continue;
                        string OldName = await _merakiApi.GetDeviceNameAsync(serial);
                        await Task.Delay(350); // ongeveer 3 calls per second
                        await _merakiApi.RenameDeviceAsync(serial, Name);
                        await Task.Delay(350); // ongeveer 3 calls per second
                        string[] DataRow = [serial, OldName, Name];
                        NamesAndSerials.Add(DataRow);
                    }
                    string backupDirectory = Path.Combine("RenameBackups");
                    string fileName = $"Backup_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv";
                    ExcelWriter.SetExcel(Path.Combine(backupDirectory, fileName), NamesAndSerials);
                    ViewBag.Message = "Devices successfully renamed from the uploaded file.";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
                    return RedirectToAction("Rename");
                }
                finally
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }

            }
            return RedirectToAction("Rename");
        }
        // GET: Devices/RenameBackups
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
                    await _merakiApi.SetDeviceAddressAsync(serial, model.Address.Trim(), model.Notes.Trim());
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
        [HttpPost]
        public async Task<IActionResult> SetLocationBulk(IFormFile excelFile)
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid Excel file.");
                return RedirectToAction("SetLocation");
            }

            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(stream);
                }

                var excelData = ExcelReader.GetExcel(tempFilePath);
                if (excelData == null || excelData.Count == 0)
                {
                    ModelState.AddModelError("", "The uploaded file is empty or invalid.");
                    return RedirectToAction("SetLocation");
                }
                foreach (var row in excelData)
                {
                    if (row.Length < 2) continue;
                    string serial = row[0].Trim();
                    string Address = row[1].Trim();
                    string Notes = row[2].Trim();
                    if (string.IsNullOrEmpty(serial)) continue;
                    await _merakiApi.SetDeviceAddressAsync(serial, Address, Notes);
                    await Task.Delay(350); // ongeveer 3 calls per second
                }
                ViewBag.Message = "Devices location successfully set from the uploaded file.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
                return RedirectToAction("SetLocation");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
            return RedirectToAction("SetLocation");
        }
    }
}
