using StageApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            return JsonSerializer.Deserialize<Organisation>(json);
        }

        public async Task ClaimDevicesAsync(string organizationId, List<string> serialNumbers)
        {
            var url = $"{BaseUrl}/organizations/{organizationId}/inventory/devices/claim";
            var payload = new { serials = serialNumbers };

            var response = await _httpClient.PostAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();
        }

        public async Task<Location> CreateLocationAsync(string serialNumber, string? address, string? city = null, string? zip = null, string? country = null, double? latitude = null, double? longitude = null)
        {
            var url = $"{BaseUrl}/devices/{serialNumber}";

            var payload = new
            {
                address,
                city,
                zip,
                country,
                latitude,
                longitude
            };

            var response = await _httpClient.PutAsync(url, GetJsonContent(payload));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Location>(json);
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
            return JsonSerializer.Deserialize<List<Models.Device>>(json);
        }

        private StringContent GetJsonContent(object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
