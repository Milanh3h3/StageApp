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

        public async Task<string> CreateOrganizationAsync(string name) //voorbeeldfunctie van chatgpt  geen idee of het kan werken.
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Cisco-Meraki-API-Key", _apiKey);

                var organizationData = new
                {
                    name
                };

                string jsonContent = System.Text.Json.JsonSerializer.Serialize(organizationData);
                StringContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"{BaseUrl}/organizations", content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception($"Error creating organization: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }
    }
}
