using Meraki.Api.Data;
using Microsoft.EntityFrameworkCore;
using StageApp.Models;

namespace StageApp
{
    public class MerakiDbDbContext : DbContext
    {
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<Models.Device> Devices { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<DeviceBackup> DeviceBackups { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connection = @"Data Source=.;Initial Catalog=MerakiDbDb; Integrated Security=SSPI; TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connection);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organisation>().HasKey(o => o.Id);
            modelBuilder.Entity<Models.Device>().HasKey(d => d.SerialNumber);
            modelBuilder.Entity<Location>().HasKey(l => l.Id);
            modelBuilder.Entity<DeviceBackup>().HasKey(b => b.Id);

            modelBuilder.Entity<Models.Device>().HasIndex(d => d.SerialNumber).IsUnique();
        }
    }
}
