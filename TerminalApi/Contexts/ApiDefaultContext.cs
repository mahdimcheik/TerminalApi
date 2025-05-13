using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Formations;
using TerminalApi.Models.Layout;
using TerminalApi.Models.Notification;
using TerminalApi.Models.Payments;
using TerminalApi.Models.Role;
using TerminalApi.Models.Slots;
using TerminalApi.Models.TVA;
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

            // Valeurs initiales
            var roles = new List<Role>()
            {
                HardCode.Admin,
                HardCode.Client,
                HardCode.Student,
                HardCode.Teacher,
            };
            builder.Entity<Role>().HasData(roles);
            TVARate tVARate = new TVARate()
            {
                Id = Guid.NewGuid(),
                Rate = 0.2m,
                StartAt = DateTimeOffset.Now,
            };
            builder.Entity<TVARate>().HasData(tVARate);

            // relations
            builder
                .Entity<UserApp>()
                .HasMany<Formation>(u => u.Formations)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId);
            builder
                .Entity<UserApp>()
                .HasMany<Address>(u => u.Adresses)
                .WithOne(f => f.user)
                .HasForeignKey(f => f.UserId);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<DateTimeOffset>()
                .HaveConversion<CustomDateTimeConversion>();
            base.ConfigureConventions(configurationBuilder);
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<UserApp> Users { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Formation> Formations { get; set; }
        public DbSet<Layout> Layouts { get; set; }
        public DbSet<TVARate> TVARates { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshTokens> RefreshTokens { get; set; }
    }
}
