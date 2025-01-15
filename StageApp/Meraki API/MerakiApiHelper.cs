using Meraki.Api.Data;
using Newtonsoft.Json;
using StageApp.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StageApp.Meraki_API
{
    public class MerakiApiHelper
    {
        private static readonly string BaseUrl = "https://api.meraki.com/api/v1";
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public MerakiApiHelper(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task CreateOrganizationAsync(string name)
        {
            var url = $"{BaseUrl}/organizations";
            var payload = new { name };

            var response = await _httpClient.PostAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();
        }
        public async Task CreateNetworkAsync(string organizationId, string name, Array productTypes, string timezone = "America/Los_Angeles")
        {
            var url = $"{BaseUrl}/organizations/{organizationId}/networks";
            var payload = new{ name, productTypes, timezone };

            var response = await _httpClient.PostAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();

        }
        public async Task ClaimDevicesAsync(string networkId, List<string> serialNumbers)
        {
            var url = $"{BaseUrl}/networks/{networkId}/devices/claim";
            var payload = new { serials = serialNumbers };

            var response = await _httpClient.PostAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();
        }

        public async Task RenameDeviceAsync(string serialNumber, string newName)
        {
            var url = $"{BaseUrl}/devices/{serialNumber}";
            var payload = new { name = newName };

            var response = await _httpClient.PutAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();
        }
        public async Task<string> GetDeviceNameAsync(string serial)
        {
            var url = $"{BaseUrl}/devices/{serial}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var deviceDetails = System.Text.Json.JsonSerializer.Deserialize<Models.Device>(jsonResponse);
            return deviceDetails.name ?? "Name not available";
        }
        public async Task SetDeviceAddressAsync(string serialNumber, string address, string notes)
        {
            var url = $"{BaseUrl}/devices/{serialNumber}";
            var payload = new { address, notes };

            var response = await _httpClient.PutAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();
        }
        public async Task<List<(string NetworkId, string NetworkName)>> GetNetworks(string organizationId)
        {
            var url = $"{BaseUrl}/organizations/{organizationId}/networks";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var networks = System.Text.Json.JsonSerializer.Deserialize<List<Models.Network>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return networks.Select(network => (network.Id, network.Name)).ToList();
        }
        public async Task<List<(string OrganizationId, string OrganizationName)>> GetOrganizations()
        {
            var url = $"{BaseUrl}/organizations/";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var Organizations = System.Text.Json.JsonSerializer.Deserialize<List<Models.Organization>>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return Organizations.Select(Organization => (Organization.Id, Organization.Name)).ToList();
        }
        public async Task SetDeviceDataAsync(string serialNumber, string name, string address, string notes)
        {
            var url = $"{BaseUrl}/devices/{serialNumber}";
            var payload = new { name, address, notes };

            var response = await _httpClient.PutAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();
        }
        private StringContent GetJsonContent(object payload)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
