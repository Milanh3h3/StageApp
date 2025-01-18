using System.Text.Json.Serialization;

namespace StageApp.Models
{
    public class WAN
    {
        [JsonPropertyName("vlan")]
        public int Vlan { get; set; }

        [JsonPropertyName("staticGatewayIp")]
        public string StaticGatewayIp { get; set; }

        [JsonPropertyName("staticIp")]
        public string StaticIp { get; set; }

        [JsonPropertyName("staticSubnetMask")]
        public string StaticSubnetMask { get; set; }

        [JsonPropertyName("staticDns")]
        public string[] StaticDns { get; set; }

        [JsonPropertyName("usingStaticIp")]
        public bool UsingStaticIp { get; set; }

        public WAN(int vlan, string staticGatewayIp, string staticIp, string staticSubnetMask, string[] staticDns, bool usingStaticIp)
        {
            Vlan = vlan;
            StaticGatewayIp = staticGatewayIp;
            StaticIp = staticIp;
            StaticSubnetMask = staticSubnetMask;
            StaticDns = staticDns;
            UsingStaticIp = usingStaticIp;
        }
    }

}
