using Meraki.Api.Data;
using StageApp.Models;
using System.Text;

namespace StageApp.Meraki_API
{
    public class MerakiApiHelper
    {
        private static readonly string BaseUrl = "https://api.meraki.com/api/v1";
        private readonly string _apiKey;

        public MerakiApiHelper(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<Organisation> CreateOrganizationAsync(string name, string apiKey)
        {
            Organisation organisation = null;
            return organisation;
            // Call Meraki API to create organization
        }

        public async Task ClaimDevicesAsync(string organizationId, List<string> serialNumbers)
        {
            // Call Meraki API to claim devices
        }

        public async Task AssignDeviceToLocationAsync(string serialNumber, string location)
        {
            // Call Meraki API to assign device to location
        }

        public async Task RenameDeviceAsync(string serialNumber, string newName)
        {
            // Call Meraki API to rename device
        }

        public async Task<List<Models.Device>> GetDevicesAsync(string organizationId)
        {
            List<Models.Device> devices = [];
            return devices;
            // Call Meraki API to get devices
        }
    }
}
