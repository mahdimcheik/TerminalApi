using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Formations;
using TerminalApi.Models.Payments;
using TerminalApi.Models.Role;
using TerminalApi.Models.Slots;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

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

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
           configurationBuilder.Properties<DateTimeOffset>().HaveConversion<CustomDateTimeConversion>();
            base.ConfigureConventions(configurationBuilder);
        }
        // Override SaveChangesAsync
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Get entries that are being Added
            var orders = ChangeTracker
                .Entries<Order>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity);

            foreach (var order in orders)
            {
                // Generate and set the OrderNumber
                order.OrderNumber = await GenerateUniqueOrderNumberAsync();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        private async Task<long> GenerateUniqueOrderNumberAsync()
        {
            long orderNumber;
            bool exists;
            int i = 1;
            do
            {
                var max = Orders.Max(o => o.OrderNumber);
                orderNumber = max + i;

                exists = await Orders.AnyAsync(o => o.OrderNumber == orderNumber);
                i++;
            }
            while (exists);

            return orderNumber;
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<UserApp> Users { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Formation> Formations { get; set; }

    }

}
