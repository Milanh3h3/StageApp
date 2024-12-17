using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StageApp.Models
{
    public class Backup
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime? BackupDate { get; set; }
        [Required]
        public string? BackupName { get; set; }
        [Required]
        public string? Filepath { get; set; }
        [Required]
        public int DevicesAmount { get; set; }
    }

}
