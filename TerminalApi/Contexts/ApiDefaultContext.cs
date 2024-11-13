using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Role;
using TerminalApi.Models.Slots;
using TerminalApi.Models.User;

namespace TerminalApi.Contexts
{
    public class ApiDefaultContext : IdentityDbContext<UserApp>
    {
        public ApiDefaultContext(DbContextOptions<ApiDefaultContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var roles = new List<Role>()
            {
                new Role()
                {
                    Id = "63a2a3ac-442e-4e4c-ad91-1443122b5a6a",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "63a2a3ac-442e-4e4c-ad91-1443122b5a6a",
                },
                new Role()
                {
                    Id = "12ccaa16-0d50-491e-8157-ec1b133cf120",
                    Name = "Client",
                    NormalizedName = "CLIENT",
                    ConcurrencyStamp = "12ccaa16-0d50-491e-8157-ec1b133cf120",
                },
                new Role()
                {
                    Id = "7f56db63-4e78-44a8-b681-ec1490a9b29s",
                    Name = "Student",
                    NormalizedName = "STUDENT",
                    ConcurrencyStamp = "7f56db63-4e78-44a8-b681-ec1490a9b29s",
                },
                new Role()
                {
                    Id = "7f56db63-4e78-44a8-b681-ec1490a9b29T",
                    Name = "Teacher",
                    NormalizedName = "TEACHER",
                    ConcurrencyStamp = "7f56db63-4e78-44a8-b681-ec1490a9b29d",
                },
            };
            builder.Entity<Role>().HasData(roles);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<UserApp> Users { get; set; }
        public DbSet<Slot> Slots { get; set; }
    }
}
