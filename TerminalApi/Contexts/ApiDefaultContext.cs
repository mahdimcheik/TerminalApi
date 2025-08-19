using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Xml;
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

            // Seed data for Levels
            var levels = new List<Level>()
            {
                new Level()
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Débutant",
                    Icon = "🟢",
                    Color = "#22c55e"
                },
                new Level()
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Intermédiaire",
                    Icon = "🟡",
                    Color = "#eab308"
                },
                new Level()
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Avancé",
                    Icon = "🟠",
                    Color = "#f97316"
                },
                new Level()
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Expert",
                    Icon = "🔴",
                    Color = "#ef4444"
                },
                new Level()
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Name = "Tous niveaux",
                    Icon = "⭐",
                    Color = "#8b5cf6"
                }
            };
            builder.Entity<Level>().HasData(levels);

            // Seed data for Categories
            var categories = new List<Category>()
            {
                new Category()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Name = "Programmation",
                    Icon = "💻",
                    Color = "#3b82f6"
                },
                new Category()
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Name = "Design",
                    Icon = "🎨",
                    Color = "#ec4899"
                },
                new Category()
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Name = "Business",
                    Icon = "📊",
                    Color = "#059669"
                },
                new Category()
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Name = "Marketing",
                    Icon = "📢",
                    Color = "#dc2626"
                },
                new Category()
                {
                    Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Name = "Data Science",
                    Icon = "📈",
                    Color = "#7c3aed"
                },
                new Category()
                {
                    Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                    Name = "Cybersécurité",
                    Icon = "🔒",
                    Color = "#1f2937"
                },
                new Category()
                {
                    Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
                    Name = "Intelligence Artificielle",
                    Icon = "🤖",
                    Color = "#6366f1"
                },
                new Category()
                {
                    Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                    Name = "DevOps",
                    Icon = "⚙️",
                    Color = "#f59e0b"
                },
                new Category()
                {
                    Id = Guid.Parse("30303030-3030-3030-3030-303030303030"),
                    Name = "Mobile",
                    Icon = "📱",
                    Color = "#10b981"
                },
                new Category()
                {
                    Id = Guid.Parse("40404040-4040-4040-4040-404040404040"),
                    Name = "Gestion de projet",
                    Icon = "📋",
                    Color = "#64748b"
                }
            };
            builder.Entity<Category>().HasData(categories);

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

            // Address entity configuration
            builder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Id).ValueGeneratedOnAdd();

                entity.Property(a => a.Street)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(a => a.PostalCode)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(a => a.Country)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(a => a.AddressType)
                    .IsRequired();

                entity.Property(a => a.UserId)
                    .IsRequired();

                entity.HasOne(a => a.user)
                    .WithMany(u => u.Adresses)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Formation entity configuration
            builder.Entity<Formation>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.Property(f => f.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(f => f.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(f => f.StartAt)
                    .HasColumnType("timestamp with time zone");

                entity.Property(f => f.EndAt)
                    .HasColumnType("timestamp with time zone");

                entity.Property(f => f.UserId)
                    .IsRequired();

                entity.Property(f => f.City)
                    .HasMaxLength(255);

                entity.Property(f => f.Country)
                    .HasMaxLength(255);

                entity.HasOne(f => f.User)
                    .WithMany(u => u.Formations)
                    .HasForeignKey(f => f.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

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

                entity.Property(e => e.CreatedById).IsRequired();

                entity
                    .HasOne(e => e.Creator)
                    .WithMany(u => u.Slots)
                    .HasForeignKey(e => e.CreatedById);

                entity.HasOne(x => x.Booking).WithOne(x => x.Slot);

                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();

                entity.Property(e => e.Price).HasDefaultValue(0m);

                entity.Property(e => e.Type);
            });

            // Booking
            builder.Entity<Booking>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Subject).HasMaxLength(128);
                entity.Property(x => x.Description).HasColumnType("text");
                entity
                    .Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");
                entity.Property(x => x.TypeHelp);

                entity
                    .HasOne(x => x.Slot)
                    .WithOne(x => x.Booking)
                    .HasForeignKey<Booking>(x => x.SlotId)
                    .IsRequired();

                entity.Property(e => e.BookedById).IsRequired();

                entity
                    .HasOne(e => e.Booker)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.BookedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.Bookings)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.Communications)
                    .HasColumnType("jsonb");
            });

            // Order entity configuration
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Id).ValueGeneratedOnAdd();

                entity.Property(o => o.OrderNumber)
                    .IsRequired();

                entity.Property(o => o.PaymentDate)
                    .HasColumnType("timestamp with time zone");

                entity.Property(o => o.CreatedAt)
                    .HasColumnType("timestamp with time zone");

                entity.Property(o => o.UpdatedAt)
                    .HasColumnType("timestamp with time zone");

                entity.Property(o => o.TVARate)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(o => o.CheckoutExpiredAt)
                    .HasColumnType("timestamp with time zone");

                entity.Property(o => o.BookerId)
                    .IsRequired();

                entity.HasOne(o => o.Booker)
                    .WithMany(b => b.Orders)
                    .HasForeignKey(o => o.BookerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification entity configuration
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Id).ValueGeneratedOnAdd();

                entity.Property(n => n.Description)
                    .HasMaxLength(255);

                entity.Property(n => n.Type);

                entity.Property(n => n.CreatedAt)
                    .HasColumnType("timestamp with time zone")
                    .IsRequired();

                entity.Property(n => n.SenderId);

                entity.Property(n => n.RecipientId);

                entity.Property(n => n.BookingId);

                entity.Property(n => n.OrderId);

                entity.HasOne(n => n.Sender)
                    .WithMany(u => u.NotificationsCreated)
                    .HasForeignKey(n => n.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Recipient)
                    .WithMany(u => u.NotificationsRecieved)
                    .HasForeignKey(n => n.RecipientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Booking)
                    .WithMany()
                    .HasForeignKey(n => n.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(n => n.Order)
                    .WithMany()
                    .HasForeignKey(n => n.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Cursus configurations
            builder.Entity<Cursus>(entity =>
            {
                entity.HasKey(c => c.Id);
                
                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(c => c.Description)
                    .IsRequired()
                    .HasMaxLength(1000);
                
                entity.Property(c => c.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone");
                
                entity.Property(c => c.UpdatedAt)
                    .HasColumnType("timestamp with time zone");
                
                entity.HasOne(c => c.Level)
                    .WithMany(l => l.Cursus)
                    .HasForeignKey(c => c.LevelId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(c => c.Category)
                    .WithMany(cat => cat.Cursus)
                    .HasForeignKey(c => c.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Level configurations
            builder.Entity<Level>(entity =>
            {
                entity.HasKey(l => l.Id);
                
                entity.Property(l => l.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(l => l.Icon)
                    .HasMaxLength(50);
                
                entity.Property(l => l.Color)
                    .HasMaxLength(20);
            });

            // Category configurations
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                
                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(c => c.Icon)
                    .HasMaxLength(50);
                
                entity.Property(c => c.Color)
                    .HasMaxLength(20);
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
        public DbSet<Cursus> Cursus { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
