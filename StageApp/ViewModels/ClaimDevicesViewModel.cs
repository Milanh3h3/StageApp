using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StageApp.ViewModels;
public class ClaimDevicesViewModel
{
    public string? OrganizationId { get; set; }
    public string? NetworkId { get; set; }
    public List<string> SerialNumbers { get; set; } = new List<string>();

    public IEnumerable<SelectListItem> Organizations { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Networks { get; set; } = new List<SelectListItem>();
}

