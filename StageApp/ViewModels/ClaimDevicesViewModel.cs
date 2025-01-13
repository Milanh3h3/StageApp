using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StageApp.ViewModels;
public class ClaimDevicesViewModel
{
    public string? NetworkId { get; set; }

    public List<string> SerialNumbers { get; set; } = new List<string>();
}
