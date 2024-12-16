using System.ComponentModel.DataAnnotations;

namespace StageApp.Models
{
    public class Organisation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ApiKey { get; set; }
    }

}
