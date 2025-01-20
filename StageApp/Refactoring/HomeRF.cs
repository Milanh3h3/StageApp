using Azure.Core;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using StageApp.Controllers;
using StageApp.Meraki_API;
using Microsoft.AspNetCore.Mvc;

namespace StageApp.Refactoring
{
    public class HomeRF
    {

        public static MerakiApiHelper? _merakiApi;
        public static string[] GetProductTypes(Dictionary<string, List<string[]>> excelData, string network)
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
                if ((productTypeLetters == "SW" || productTypeLetters == "MS") && !productTypes.Contains("switch"))
                {
                    productTypes.Add("switch");
                }
                if (productTypeLetters == "RT" && !productTypes.Contains("wireless"))
                {
                    productTypes.Add("wireless");
                }
            }
            return productTypes.ToArray();
        }
        public static async void WaitForMeraki(string serial)
        {
            while (true)
            {
                try
                {
                    await _merakiApi.GetDeviceNameAsync(serial);
                    HomeController._statusMessage = "Device geclaimed!";
                    return;
                }
                catch
                {
                    for (int i = 60; i > 0; i--)
                    {
                        await Task.Delay(1000);
                        HomeController._statusMessage = $"Aan het wachten totdat Meraki de devices heeft geclaimed. Retrying in {i}s";
                    }

                }
            }
        }
    }
}
