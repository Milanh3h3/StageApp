using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageApp.Excel;
using StageApp.Meraki_API;
using StageApp.Models;
using System.Diagnostics;

namespace StageApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MerakiApiHelper? _merakiApi;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        // Action to store the API key in a secure cookie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetAPIkey(string API_key)
        {
            if (string.IsNullOrEmpty(API_key))
            {
                ModelState.AddModelError("", "API key cannot be empty.");
                return View();
            }

            // Set the API key in a secure cookie
            CookieOptions options = new CookieOptions
            {
                HttpOnly = true, // Prevent client-side scripts from accessing the cookie
                Expires = DateTimeOffset.UtcNow.AddMinutes(30), // Set an expiration time
                SameSite = SameSiteMode.Strict,
                Secure = true,
            };

            Response.Cookies.Append("API_Key", API_key, options);

            return RedirectToAction(nameof(Index));
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
        public IActionResult MultiActions()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MultiActions(IFormFile excelFile)
        {

            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid Excel file.");
                return RedirectToAction("MultiActions");
            }

            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(stream);
                }

                var excelData = ExcelReader.GetExcelTabs(tempFilePath);
                UserInputControl.InputControlMultiActions(excelData); // doe hier nog iets mee
                if (excelData == null || excelData.Count == 0)
                {
                    ModelState.AddModelError("", "The uploaded file is empty or invalid.");
                    return RedirectToAction("MultiActions");
                }
                //maken van netwerken
                var networksFromExcel = excelData["Networks"];
                var organizations = await _merakiApi.GetOrganizations();
                List<string> usedOrganizationIds = new List<string>(); // bijhouden voor netwerk ID's terug te vinden
                foreach (var network in networksFromExcel)
                {
                    string organizationId = organizations.FirstOrDefault(org => org.OrganizationName == network[0]).OrganizationId;
                    if (!string.IsNullOrEmpty(organizationId) && !usedOrganizationIds.Contains(organizationId))
                    {
                        usedOrganizationIds.Add(organizationId);
                    }
                    string[] productTypes = [""]; // TODO Productypes uit namen van producten halen
                    _merakiApi.CreateNetworkAsync(organizationId, network[1], productTypes, network[2]);
                    await Task.Delay(350); // ongeveer 3 calls per second
                }
                //achterhalen van networkIDs
                List<(string NetworkId, string NetworkName)> networks = [];
                foreach (var OrgId in usedOrganizationIds)
                {
                    networks.AddRange(await _merakiApi.GetNetworks(OrgId));
                    await Task.Delay(350); // ongeveer 3 calls per second
                }

                //claimen van devices
                var devices = excelData["Devices"];
                var devicesByNetwork = new Dictionary<string, List<string>>();
                foreach (var device in devices)
                {
                    string serialNumber = device[8].Trim();
                    string networkId = networks.FirstOrDefault(net => net.NetworkName == device[0]).NetworkId;
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
                //TODO
                //Timer of iets van oplossing voor cooldown na claimen
                //IP-Address	SubnetMask	Gateway	DNS1	DNS2	VLAN

                //Device_name   Location	Notes
                foreach (var device in devices)
                {
                    await _merakiApi.SetDeviceDataAsync(device[8].Trim(), device[1].Trim(), device[9].Trim(), device[10].Trim());
                    await Task.Delay(350); // ongeveer 3 calls per second
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
                return RedirectToAction("MultiActions");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }

            return RedirectToAction(nameof(Index));
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
