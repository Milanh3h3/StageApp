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

        public async Task<Organisation> CreateOrganizationAsync(string name)
        {
            var url = $"{BaseUrl}/organizations";
            var payload = new { name };

            var response = await _httpClient.PostAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<Organisation>(json);
        }
        public async Task<string> CreateNetworkAsync(string organizationId, string name, Array productTypes, string timezone = "America/Los_Angeles")
        {
            var url = $"{BaseUrl}/organizations/{organizationId}/networks";

            var payload = new
            {
                name = name,
                productTypes = productTypes,
                timezone = timezone
            };

            var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody; // Returns the response JSON with network details
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error: {response.StatusCode}, Details: {error}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while creating the network: {ex.Message}");
            }
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

        public async Task<List<Models.Device>> GetDevicesAsync(string organizationId)
        {
            var url = $"{BaseUrl}/organizations/{organizationId}/devices";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<List<Models.Device>>(json);
        }

        private StringContent GetJsonContent(object payload)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
