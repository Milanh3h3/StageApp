using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using NuGet.Common;
using StageApp.Excel;
using StageApp.Meraki_API;
using StageApp.Models;
using StageApp.Refactoring;
using System.Diagnostics;
using System.Security.Claims;

namespace StageApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MerakiApiHelper? _merakiApi;
        public static string _statusMessage = string.Empty;
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
                Expires = DateTimeOffset.UtcNow.AddMinutes(240), // Set an expiration time
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
        private bool InitializeMerakiApiForHomrRF()
        {
            if (Request.Cookies.TryGetValue("API_Key", out string? apiKey) && !string.IsNullOrEmpty(apiKey))
            {
                HomeRF._merakiApi = new MerakiApiHelper(apiKey);
                return true;
            }
            return false;
        }
        public IActionResult NetworkDeployer()
        {
            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NetworkDeployer(IFormFile excelFile)
        {

            if (!InitializeMerakiApi())
            {
                return RedirectToAction("Index", "Home");
            }

            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid Excel file.");
                return RedirectToAction("NetworkDeployer");
            }

            var tempFilePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(stream);
                }

                var excelData = ExcelReader.GetExcelTabs(tempFilePath);
                var organizations = await _merakiApi.GetOrganizations();
                _statusMessage = "Bestand aan het controleren";
                string Inputcontrolresult = UserInputControl.InputControlNetworkDeployer(excelData, organizations); 
                if ( Inputcontrolresult == string.Empty)
                {
                    _statusMessage = "Bestand succesvol ingelezen en gecontrolleerd";
                    if (excelData == null || excelData.Count == 0)
                    {
                        ModelState.AddModelError("", "The uploaded file is empty or invalid.");
                        return RedirectToAction("NetworkDeployer");
                    }

                    _statusMessage = "Bezig met het maken van netwerken";
                    var networksFromExcel = excelData["Networks"];
                    List<string> usedOrganizationIds = new List<string>(); // bijhouden voor netwerk ID's terug te vinden
                    foreach (var network in networksFromExcel)
                    {
                        string organizationId = organizations.FirstOrDefault(org => org.OrganizationName == network[0].Trim()).OrganizationId;
                        if (!string.IsNullOrEmpty(organizationId) && !usedOrganizationIds.Contains(organizationId))
                        {
                            usedOrganizationIds.Add(organizationId);
                        }
                        string[] productTypes = HomeRF.GetProductTypes(excelData, network[1].Trim());
                        _merakiApi.CreateNetworkAsync(organizationId, network[1].Trim(), productTypes, network[2].Trim());
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                    _statusMessage = "Bezig met het achterhalen van networkIDs";
                    List<(string NetworkId, string NetworkName)> networks = [];
                    foreach (var OrgId in usedOrganizationIds)
                    {
                        networks.AddRange(await _merakiApi.GetNetworks(OrgId));
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }

                    _statusMessage = "Bezig met het claimen van devices";
                    var devices = excelData["Devices"];
                    var devicesByNetwork = new Dictionary<string, List<string>>();
                    foreach (var device in devices)
                    {
                        string serialNumber = device[8].Trim();
                        string networkId = networks.FirstOrDefault(net => net.NetworkName == device[0].Trim()).NetworkId;
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
                    _statusMessage = "Aan het wachten totdat Meraki de devices heeft geclaimed.";
                    InitializeMerakiApiForHomrRF();
                    string LastDeviceSerial = devices.FindLast(device => device.Length > 8)?[8];
                    HomeRF.WaitForMeraki(LastDeviceSerial);
                    _statusMessage = "Bezig met devices hun netwerkinformatie geven";
                    //IP-Address	SubnetMask	Gateway	DNS1	DNS2	VLAN
                    foreach (var device in devices)
                    {
                        string[] DNSs = [device[5].Trim(), device[6].Trim()];
                        await _merakiApi.SetDeviceWAN1Async(device[8].Trim(), Int32.Parse(device[7].Trim()), device[4].Trim(), device[2].Trim(), device[3].Trim(), DNSs);
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                    _statusMessage = "Bezig met devices hun informatie geven";
                    //Device_name   Location	Notes
                    foreach (var device in devices)
                    {
                        await _merakiApi.SetDeviceDataAsync(device[8].Trim(), device[1].Trim(), device[9].Trim(), device[10].Trim());
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                }
                else 
                {  // Beste stukje code
                    _statusMessage = Inputcontrolresult;
                    _statusMessage = Inputcontrolresult + ". Redirecting in 10...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 9...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 8...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 7...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 6...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 5...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 4...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 3...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 2...";
                    await Task.Delay(1000);
                    _statusMessage = Inputcontrolresult + ". Redirecting in 1...";
                    await Task.Delay(1000);
                    return RedirectToAction("NetworkDeployer");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while processing the file: {ex.Message}");
                return RedirectToAction("NetworkDeployer");
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
        
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new { status = _statusMessage });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
