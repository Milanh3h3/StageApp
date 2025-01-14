using System.ComponentModel.DataAnnotations;

namespace StageApp.Models
{
    public class Device
    {
        [Key]
        public string? SerialNumber { get; set; }
        [Required]
        public string? name { get; set; } // moet een kleine letter n hebben
        public string? Location { get; set; }
        /*
            "name": "My AP",
            "lat": 37.4180951010362,
            "lng": -122.098531723022,
            "address": "1600 Pennsylvania Ave",
            "notes": "My AP's note",
            "tags": [ " recently-added " ],
            "networkId": "N_24329156",
            "serial": "Q234-ABCD-5678",
            "model": "MR34",
            "mac": "00:11:22:33:44:55",
            "lanIp": "1.2.3.4",
            "firmware": "wireless-25-14",
            "floorPlanId": "g_2176982374",
            "details": [
                {
                    "name": "Catalyst serial",
                    "value": "123ABC"
                }
            ],
            "beaconIdParams": {
                "uuid": "00000000-0000-0000-0000-000000000000",
                "major": 5,
                "minor": 3
            }
        */
    }
}
