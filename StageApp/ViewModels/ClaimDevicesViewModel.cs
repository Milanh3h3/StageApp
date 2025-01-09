using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StageApp.ViewModels;
public class ClaimDevicesViewModel
{
    [Required(ErrorMessage = "Network ID is required.")]
    public string NetworkId { get; set; }

    [Required(ErrorMessage = "At least one serial number is required.")]
    public List<string> SerialNumbers { get; set; } = new List<string>();
}
