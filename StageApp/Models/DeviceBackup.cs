using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StageApp.Models
{
    public class DeviceBackup
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? SerialNumber { get; set; }
        [Required]
        public string? OldName { get; set; }
        [Required]
        public DateTime? BackupDate { get; set; }
    }

}
