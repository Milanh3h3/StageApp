using System.ComponentModel.DataAnnotations;

namespace StageApp.Models
{
    public class Device
    {
        [Key]
        public string? SerialNumber { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Location { get; set; }
    }

}
