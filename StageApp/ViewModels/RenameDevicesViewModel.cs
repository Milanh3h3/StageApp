using System.ComponentModel.DataAnnotations;

namespace StageApp.ViewModels
{
    public class RenameDevicesViewModel
    {
        public List<string> SerialNumbers { get; set; } = new List<string>();

        public List<string> NewNames { get; set; } = new List<string>();
    }
}
