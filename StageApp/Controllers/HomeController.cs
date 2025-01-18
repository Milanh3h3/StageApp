using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using StageApp.Excel;
using StageApp.Meraki_API;
using StageApp.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace StageApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MerakiApiHelper? _merakiApi; 
        private static readonly ConcurrentDictionary<string, string> StatusMessages = new();
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
        public IActionResult NetworkDeployer(IFormFile excelFile)
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

            // Generate a unique ID for this deployment process
            string processId = Guid.NewGuid().ToString();
            StatusMessages[processId] = "Processing started...";

            // Run the deployment logic in a background task
            Task.Run(() => ProcessNetworkDeployment(excelFile, processId));

            // Pass the process ID back to the frontend for status updates
            ViewBag.ProcessId = processId;
            return View();
        }

        private async Task ProcessNetworkDeployment(IFormFile excelFile, string processId)
        {
            string tempFilePath = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(stream);
                }

                var excelData = ExcelReader.GetExcelTabs(tempFilePath);
                var organizations = await _merakiApi.GetOrganizations();
                UpdateStatus(processId, "Bestand aan het controleren");
                string Inputcontrolresult = UserInputControl.InputControlNetworkDeployer(excelData, organizations);
                if (Inputcontrolresult == string.Empty)
                {
                    UpdateStatus(processId, "Bestand succesvol ingelezen en gecontrolleerd");
                    UpdateStatus(processId, "Bezig met het maken van netwerken");
                    var networksFromExcel = excelData["Networks"];
                    List<string> usedOrganizationIds = new List<string>(); // bijhouden voor netwerk ID's terug te vinden
                    foreach (var network in networksFromExcel)
                    {
                        string organizationId = organizations.FirstOrDefault(org => org.OrganizationName == network[0].Trim()).OrganizationId;
                        if (!string.IsNullOrEmpty(organizationId) && !usedOrganizationIds.Contains(organizationId))
                        {
                            usedOrganizationIds.Add(organizationId);
                        }
                        string[] productTypes = GetProductTypes(excelData, network[1].Trim());
                        await _merakiApi.CreateNetworkAsync(organizationId, network[1].Trim(), productTypes, network[2].Trim());
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                    UpdateStatus(processId, "Bezig met het achterhalen van networkIDs");
                    List<(string NetworkId, string NetworkName)> networks = [];
                    foreach (var OrgId in usedOrganizationIds)
                    {
                        networks.AddRange(await _merakiApi.GetNetworks(OrgId));
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }

                    UpdateStatus(processId, "Bezig met het claimen van devices");
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
                    UpdateStatus(processId, "Aan het wachten totdat Meraki de devices heeft geclaimed.");
                    string LastDeviceSerial = devices.FindLast(device => device.Length > 8)?[8];
                    WaitForMeraki(processId, LastDeviceSerial);
                    UpdateStatus(processId, "Bezig met devices hun netwerkinformatie geven");
                    //IP-Address	SubnetMask	Gateway	DNS1	DNS2	VLAN
                    foreach (var device in devices)
                    {
                        string[] DNSs = [device[5].Trim(), device[6].Trim()];
                        await _merakiApi.SetDeviceWAN1Async(device[8].Trim(), Int32.Parse(device[7].Trim()), device[4].Trim(), device[2].Trim(), device[3].Trim(), DNSs);
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                    UpdateStatus(processId,"Bezig met devices hun informatie geven");
                    //Device_name   Location	Notes
                    foreach (var device in devices)
                    {
                        await _merakiApi.SetDeviceDataAsync(device[8].Trim(), device[1].Trim(), device[9].Trim(), device[10].Trim());
                        await Task.Delay(350); // ongeveer 3 calls per second
                    }
                }
                else
                {
                    UpdateStatus(processId, Inputcontrolresult);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus(processId, $"Error: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
        }
        public string[] GetProductTypes(Dictionary<string, List<string[]>> excelData, string network)
        {
            var devices = excelData["Devices"];
            var devicesByNetwork = new List<string>();
            foreach (var device in devices)
            {
                if (device[0] == network)
                {
                    devicesByNetwork.Add(device[1]);
                }
            }
            var productTypes = new List<string>();
            foreach (string device in devicesByNetwork)
            {
                string lastFiveLetters = device.Substring(device.Length - 5);
                string productTypeLetters = lastFiveLetters.Substring(0, 2);

                if ((productTypeLetters == "RT" || productTypeLetters == "FW") && !productTypes.Contains("appliance"))
                {
                    productTypes.Add("appliance");
                }
                else if ((productTypeLetters == "SW" || productTypeLetters == "MS") && !productTypes.Contains("switch"))
                {
                    productTypes.Add("switch");
                }
                else if (productTypeLetters == "RT" && !productTypes.Contains("wireless"))
                {
                    productTypes.Add("wireless");
                }
            }
            return productTypes.ToArray();
        }
        public async void WaitForMeraki(string serial, string processId)
        {
            while (true)
            {
                try
                {
                    await _merakiApi.GetDeviceNameAsync(serial);
                    UpdateStatus(processId, "Device geclaimed!");
                    return;
                }
                catch
                {
                    for (int i = 60; i > 0; i--)
                    {
                        await Task.Delay(1000);
                        UpdateStatus(processId, $"Aan het wachten totdat Meraki de devices heeft geclaimed. Retrying in {i}s");
                    }

                }
            }
        }

        public void UpdateStatus(string processId, string message)
        {
            StatusMessages[processId] = message;
        }

        [HttpGet]
        public IActionResult GetStatus(string processId)
        {
            if (StatusMessages.TryGetValue(processId, out string? status))
            {
                return Ok(new { status });
            }
            return NotFound(new { status = "Invalid process ID" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
