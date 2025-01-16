using Meraki.Api.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using StageApp.ViewModels;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace StageApp.Excel
{
    public class UserInputControl
    {
        public static string InputControlNetworkDeployer(Dictionary<string, List<string[]>> data, List<(string OrganizationId, string OrganizationName)> organizations)
        {

            var networksFromExcel = data["Networks"];
            string[] FirstRowTab1 = networksFromExcel[0];
            if (FirstRowTab1[0].Trim() == "Organization_name" && FirstRowTab1[1].Trim() == "Network_name" && FirstRowTab1[2].Trim() == "Timezone")
            {
                networksFromExcel.Remove(FirstRowTab1);

                var devicesFromExcel = data["Devices"];
                string[] FirstRowTab2 = devicesFromExcel[0];
                if (FirstRowTab2[0].Trim() == "Network_name" && FirstRowTab2[1].Trim() == "Device_name" && FirstRowTab2[2].Trim() == "IP-Address" && FirstRowTab2[3].Trim() == "SubnetMask" && FirstRowTab2[4].Trim() == "Gateway" && FirstRowTab2[5].Trim() == "DNS1" && FirstRowTab2[6].Trim() == "DNS2" && FirstRowTab2[7].Trim() == "VLAN" && FirstRowTab2[8].Trim() == "Serial_Number" && FirstRowTab2[9].Trim() == "Location" && FirstRowTab2[10].Trim() == "Notes")
                {
                    devicesFromExcel.Remove(FirstRowTab2);
                    List<string> devicenames = new List<string>();
                    // Checken of de waarden van de devices geldig zijn
                    foreach (var device in devicesFromExcel)
                    {
                        if (devicenames.Contains(device[1].Trim())) { return $"name van {device[1]} hetzelfde als device Y"; } else { devicenames.Add(device[1].Trim()); }
                        if (!IsValidIpAddress(device[2].Trim())) { return $"IP address van {device[1]} is niet valid"; }
                        if (!IsValidIpAddress(device[3].Trim())) { return $"SubNetMask van {device[1]} is niet valid"; }
                        if (!IsValidIpAddress(device[4].Trim())) { return $"Gateway van {device[1]} is niet valid";  }
                        if (!IsValidIpAddress(device[5].Trim())) { return $"DNS1 van {device[1]} is niet valid"; }
                        if (!string.IsNullOrEmpty(device[6].Trim()) && !IsValidIpAddress(device[6].Trim())) { return $"DNS2 van {device[1]} is niet valid";  }
                        if (!IsValidVlan(device[7].Trim())) { return $"Vlan van {device[1]} is niet valid";  }
                        if (!IsValidSerialNumber(device[8].Trim())) { return $"SerialNumber van {device[1]} is niet valid";  }
                    }
                    // Checken of de waarden van de networks geldig zijn
                    foreach (var network in networksFromExcel)
                    {
                        if (!organizations.Any(o => o.OrganizationName == network[0].Trim())) { return "Organization name van network X is niet valid";  }
                        CreateNetworkViewModel timezones = new CreateNetworkViewModel(); // ophalen van timezones uit viewmodel want daar staan ze al
                        if (!timezones.Timezones.Any(t => t.Value ==  network[2].Trim())) { return "Timezone van network X is niet valid";  }
                    }
                }
                else { return "Headers van Devices table niet correct. Gebruik het voorbeeld!"; }
            }
            else { return "Headers van Networks table niet correct. Gebruik het voorbeeld!"; }
            return string.Empty;
        }
        private static bool IsValidIpAddress(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out _))
                return true;
            return false;
        }

        private static bool IsValidVlan(string vlan)
        {
            if (int.TryParse(vlan, out int vlanNumber))
            {
                return vlanNumber >= 1 && vlanNumber <= 4094;
            }
            return false;
        }

        private static bool IsValidSerialNumber(string serialNumber)
        {
            string pattern = @"^[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}$";
            return Regex.IsMatch(serialNumber, pattern);
        }
    }
}
