using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SyslogShared.Models;

namespace SyslogShared
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Alerts> alerts { get; set; }
        public DbSet<AppVars>appvars { get; set; }
        public DbSet<Device>Devices { get; set; }
        public DbSet<RemoteDevice>RemoteDevices { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
