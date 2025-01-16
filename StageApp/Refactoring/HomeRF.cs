using Microsoft.Identity.Client;

namespace StageApp.Refactoring
{
    public class HomeRF
    {
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
    }
}
