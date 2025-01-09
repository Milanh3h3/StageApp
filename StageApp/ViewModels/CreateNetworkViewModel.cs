using Microsoft.AspNetCore.Mvc.Rendering;

namespace StageApp.ViewModels
{
    public class CreateNetworkViewModel
    {
        public string OrganizationId { get; set; }
        public string Name { get; set; }
        public List<string> SelectedNetworkTypes { get; set; } = new List<string>();
        public string Timezone { get; set; }

        public IEnumerable<SelectListItem> NetworkTypes = new List<SelectListItem>
        {
            new SelectListItem("Appliance", "appliance"),
            new SelectListItem("Switch", "switch"),
            new SelectListItem("Sensor", "sensor"),
            new SelectListItem("Wireless", "wireless"),
            new SelectListItem("Camera", "camera"),
            new SelectListItem("EMM (Systems manager)", "systemsManager"),
            new SelectListItem("Cellular gateway", "cellularGateway"),
            new SelectListItem("SecureConnect", "secureConnect"),
            new SelectListItem("Wireless LAN controller", "wirelessController")
        };
        public IEnumerable<SelectListItem> Timezones = new List<SelectListItem>
        {
            new SelectListItem { Text = "US - Eastern (UTC -5.0)", Value = "US/Eastern" },
            new SelectListItem { Text = "US - Central (UTC -6.0)", Value = "US/Central" },
            new SelectListItem { Text = "US - Arizona (UTC -7.0)", Value = "US/Arizona" },
            new SelectListItem { Text = "US - Mountain (UTC -7.0)", Value = "US/Mountain" },
            new SelectListItem { Text = "US - Pacific (UTC -8.0)", Value = "US/Pacific" },
            new SelectListItem { Text = "US - Alaska (UTC -9.0)", Value = "US/Alaska" },
            new SelectListItem { Text = "US - Aleutian (UTC -10.0)", Value = "US/Aleutian" },
            new SelectListItem { Text = "US - Hawaii (UTC -10.0)", Value = "US/Hawaii" },
            new SelectListItem { Text = "US - Samoa (UTC -11.0)", Value = "US/Samoa" },
            new SelectListItem { Text = "Canada - Newfoundland (UTC -3.5)", Value = "Canada/Newfoundland" },
            new SelectListItem { Text = "Canada - Atlantic (UTC -4.0)", Value = "Canada/Atlantic)" },
            new SelectListItem { Text = "Canada - Eastern (UTC -5.0)", Value = "Canada/Eastern" },
            new SelectListItem { Text = "Canada - Central (UTC -6.0)", Value = "Canada/Central" },
            new SelectListItem { Text = "Canada - Saskatchewan (UTC -6.0)", Value = "Canada/Saskatchewan" },
            new SelectListItem { Text = "Canada - Mountain (UTC -7.0)", Value = "Canada/Mountain" },
            new SelectListItem { Text = "Canada - Yukon (UTC -7.0)", Value = "Canada/Yukon" },
            new SelectListItem { Text = "Canada - Pacific (UTC -8.0)", Value = "Canada/Pacific" },
            new SelectListItem { Text = "Europe - Moscow (UTC +3.0)", Value = "Europe/Moscow" },
            new SelectListItem { Text = "Europe - Istanbul (UTC +3.0)", Value = "Europe/Istanbul)" },
            new SelectListItem { Text = "Europe - Helsinki (UTC +2.0)", Value = "Europe/Helsinki" },
            new SelectListItem { Text = "Europe - Athens (UTC +2.0)", Value = "Europe/Athens" },
            new SelectListItem { Text = "Europe - Vienna (UTC +1.0)", Value = "Europe/Vienna" },
            // Add all remaining timezones extracted from the document
        };
    }
}
