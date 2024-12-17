using System.ComponentModel.DataAnnotations;

namespace StageApp.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        Organisation? Organisation { get; set; }
    }

}
