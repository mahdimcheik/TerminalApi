using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Models;
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
            // user
            builder.Entity<UserApp>(e =>
            {
                e.HasKey(u => u.Id);

                e.Property(u => u.Id).IsRequired().HasMaxLength(64);

                e.Property(u => u.LastName).IsRequired().HasMaxLength(64);

                e.Property(u => u.Gender);

                e.Property(u => u.ImgUrl).HasMaxLength(256);

                e.Property(u => u.Title).HasMaxLength(256);

                e.Property(u => u.Description).HasMaxLength(512).HasColumnType("text");

                e.Property(e => e.DateOfBirth)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");

                e.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");

                e.Property(e => e.LastModifiedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");

                e.Property(e => e.LastLogginAt).HasColumnType("timestamp with time zone");

                e.Property(e => e.IsBanned).IsRequired().HasDefaultValue(false);

                e.Property(e => e.BannedUntilDate).HasColumnType("timestamp with time zone");
               
            });

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

            builder
                .Entity<UserApp>()
                .HasMany<Slot>(u => u.Slots)
                .WithOne(f => f.Creator)
                .HasForeignKey(f => f.CreatedById);

            builder
                .Entity<UserApp>()
                .HasMany<Booking>(u => u.Bookings)
                .WithOne(b => b.Booker)
                .HasForeignKey(b => b.BookedById);

            builder
                .Entity<UserApp>()
                .HasMany<Booking>(u => u.Bookings)
                .WithOne(b => b.Booker)
                .HasForeignKey(b => b.BookedById);

            // TVA
            builder.Entity<TVARate>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity
                    .Property(e => e.StartAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Rate).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Slot

            builder.Entity<Slot>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(u => u.Id).ValueGeneratedOnAdd().IsRequired();

                entity
                    .Property(e => e.StartAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");

                entity
                    .Property(e => e.EndAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");

                entity
                    .Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.CreatedById)
                 .IsRequired();

                entity.HasOne(e => e.Creator)
                      .WithMany( u => u.Slots)
                      .HasForeignKey(e => e.CreatedById);

                entity.HasOne(x => x.Booking)
                        .WithOne(x => x.Slot);

                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();

                entity.Property(e => e.Price).HasDefaultValue(0m);

                entity.Property(e => e.Type);
            });


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
