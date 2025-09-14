using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TerminalApi;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Utilities;
using TerminalApi.Interfaces;
using Testcontainers.PostgreSql;
using System.Collections.Generic;
using Hangfire;

namespace TerminalTestIntegration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;

    public CustomWebApplicationFactory()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("beecoming")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        
        Environment.SetEnvironmentVariable("API_BACK_URL", "https://localhost:7113");
        Environment.SetEnvironmentVariable("API_FRONT_URL", "https://localhost:4200");
        Environment.SetEnvironmentVariable("SMTP_BREVO_PORT", "587");
        Environment.SetEnvironmentVariable("SMTP_BREVO_SERVER", "smtp-relay.brevo.com");
        Environment.SetEnvironmentVariable("SMTP_BREVO_LOGIN", "test@example.com");
        Environment.SetEnvironmentVariable("SMTP_BREVO_KEY", "test-key");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        
        Environment.SetEnvironmentVariable("JWT_KEY", "i7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPs");
        
        Environment.SetEnvironmentVariable("TEACHER_GUID", "44ea5267-31c5-44a6-94a3-bac6efd009c7");
        Environment.SetEnvironmentVariable("TEACHER_EMAIL", "teacher@skillhive.fr");
        Environment.SetEnvironmentVariable("TOKEN_AUDIENCE", "https://localhost:7113");
        Environment.SetEnvironmentVariable("TOKEN_ISSUER", "https://localhost:7113");
        Environment.SetEnvironmentVariable("TOKEN_VALIDITY_MINUTES", "60");
        Environment.SetEnvironmentVariable("DO_NO_REPLY_MAIL", "test@skillhive.com");
        Environment.SetEnvironmentVariable("COOKIES_VALIDITY_DAYS", "7");
        Environment.SetEnvironmentVariable("DOCKER_ENVIRONMENT", "false");
        
        await Task.Delay(1000);
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configure the application configuration first
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override connection string and other settings for testing
            /*
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", _postgresContainer.GetConnectionString()},
                {"AppSettings:Database:ConnectionString", _postgresContainer.GetConnectionString()},
                {"AppSettings:Database:Host", _postgresContainer.Hostname},
                {"AppSettings:Database:Port", _postgresContainer.GetMappedPublicPort(5432).ToString()},
                {"AppSettings:Database:Name", "testdb"},
                {"AppSettings:Database:User", "postgres"},
                {"AppSettings:Database:Password", "beecoming"},
                {"AppSettings:Database:Provider", "PostgreSql"},
                {"AppSettings:Token:JwtKey", "i7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPsi7RdBacZPs"},
                {"AppSettings:Api:BackUrl", "https://localhost:7113"},
                {"AppSettings:Api:FrontUrl", "https://localhost:4200"},
                {"AppSettings:Token:Audience", "https://localhost:7113"},
                {"AppSettings:Token:Issuer", "https://localhost:7113"},
                {"AppSettings:Token:ValidityMinutes", "60"},
                {"AppSettings:Smtp:BrevoKey", "test-key"},
                {"AppSettings:Smtp:BrevoServer", "smtp-relay.brevo.com"},
                {"AppSettings:Smtp:BrevoPort", "587"},
                {"AppSettings:Smtp:BrevoLogin", "test@example.com"},
                {"AppSettings:Mail:DoNotReplyMail", "test@skillhive.com"},
                {"AppSettings:Google:ClientId", "test-client-id"},
                {"AppSettings:Google:ClientSecret", "test-client-secret"},
                {"AppSettings:Google:RedirectUrl", "/google-callback"},
                {"AppSettings:Google:ApiKey", "test-api-key"},
                {"AppSettings:Stripe:SecretKey", "sk_test_test"},
                {"AppSettings:Stripe:PublishableKey", "pk_test_test"},
                {"AppSettings:Stripe:SecretEndpointTest", "whsec_test"},
                {"AppSettings:Hangfire:OrderCleaningDelayMinutes", "15"},
                {"AppSettings:Checkout:ExpiryDelayMinutes", "15"},
                {"AppSettings:Cookies:ValidityDays", "7"},
                {"AppSettings:Environment:DockerEnvironment", "false"},
                {"AppSettings:Teacher:Guid", "44ea5267-31c5-44a6-94a3-bac6efd009c7"},
                {"AppSettings:Teacher:Email", "teacher@skillhive.fr"}
            });*/
        });

        // Configure services
        builder.ConfigureServices(services =>
        {
            // Ajouter le service provider pour injjecter les services necessaires
            var tempServiceProvider = services.BuildServiceProvider();
            var configuration = tempServiceProvider.GetService<IConfiguration>();

            // hangfire me causer des problemes dans les tests, on le vire
            var hangfireServices = services.Where(s => 
                s.ServiceType.Name.Contains("Hangfire") || 
                s.ServiceType.Name.Contains("BackgroundJob") ||
                s.ServiceType.Name.Contains("JobStorage") ||
                s.ServiceType.Name.Contains("IBackgroundJobServer") ||
                s.ServiceType.Name.Contains("IGlobalConfiguration")
            ).ToList();
            
            foreach (var service in hangfireServices)
            {
                services.Remove(service);
            }

            // le service d'envoi de mail ne fonctionne pas en test, parce que ca demande un vrai serveur smtp
            // on le vire et on met un mock a la place
            var emailServiceDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(ISendMailService)
            );
            if (emailServiceDescriptor != null)
            {
                services.Remove(emailServiceDescriptor);
            }
            services.AddScoped<ISendMailService, MockEmailService>();

            // chercher le service DbContextOptions<ApiDefaultContext> et le supprimer
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApiDefaultContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // le remplacer par un qui pointe sur la base de test (overrider)
            services.AddDbContext<ApiDefaultContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
                options.EnableSensitiveDataLogging();
            });

            //// Build service provider to initialize the database
            var serviceProvider = services.BuildServiceProvider();

            // creer la base de donnees et appliquer les migrations de base
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDefaultContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserApp>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            try
            {
                // creer la base de donnees
                context.Database.EnsureCreated();

                // ajouter les données de test
                SeedDataAsync(userManager, roleManager).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                // pour pouvoir debogger plus facilement
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        });

        // cette varaible d'environnement est lue dans Program.cs pour activer certains comportements (hangfire)
        builder.UseEnvironment("Testing");
    }
    private async Task SeedDataAsync(UserManager<UserApp> userManager, RoleManager<Role> roleManager)
    {
        // Admin
        var adminUser = new UserApp
        {
            Email = "admin@skillhive.fr",
            UserName = "admin@skillhive.fr",
            FirstName = "Admin",
            LastName = "User",
            Gender = EnumGender.Homme,
            PhoneNumber = "0600000001",
            DateOfBirth = new DateTimeOffset(1985, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "Administrator user for testing",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        var adminResult = await userManager.CreateAsync(adminUser, "Admin123!");
        if (adminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Seed Etudiant
        var studentUser = new UserApp
        {
            Email = "student@skillhive.fr",
            UserName = "student@skillhive.fr",
            FirstName = "Student",
            LastName = "User",
            Gender = EnumGender.Femme,
            PhoneNumber = "0600000002",
            DateOfBirth = new DateTimeOffset(1995, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Description = "Student user for testing",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        var studentResult = await userManager.CreateAsync(studentUser, "Student123!");
        if (studentResult.Succeeded)
        {
            await userManager.AddToRoleAsync(studentUser, "Student");
        }

        // Seed Etuduiant non confirme' (Email Not Confirmed)
        var unconfirmedUser = new UserApp
        {
            Email = "unconfirmed@skillhive.fr",
            UserName = "unconfirmed@skillhive.fr",
            FirstName = "Unconfirmed",
            LastName = "User",
            Gender = EnumGender.Autre,
            PhoneNumber = "0600000003",
            DateOfBirth = new DateTimeOffset(1990, 3, 10, 0, 0, 0, TimeSpan.Zero),
            Description = "Unconfirmed user for testing",
            EmailConfirmed = false,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        var unconfirmedResult = await userManager.CreateAsync(unconfirmedUser, "Unconfirmed123!");
        if (unconfirmedResult.Succeeded)
        {
            await userManager.AddToRoleAsync(unconfirmedUser, "Student");
        }

        // Etudiant banni (Banned User)
        var bannedUser = new UserApp
        {
            Email = "banned@skillhive.fr",
            UserName = "banned@skillhive.fr",
            FirstName = "Banned",
            LastName = "User",
            Gender = EnumGender.Homme,
            PhoneNumber = "0600000004",
            DateOfBirth = new DateTimeOffset(1988, 12, 25, 0, 0, 0, TimeSpan.Zero),
            Description = "Banned user for testing",
            EmailConfirmed = true,
            IsBanned = true,
            BannedUntilDate = DateTimeOffset.UtcNow.AddDays(30),
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        var bannedResult = await userManager.CreateAsync(bannedUser, "Banned123!");
        if (bannedResult.Succeeded)
        {
            await userManager.AddToRoleAsync(bannedUser, "Student");
        }

        // un autre etudiant
        var clientUser = new UserApp
        {
            Email = "client@skillhive.fr",
            UserName = "client@skillhive.fr",
            FirstName = "Client",
            LastName = "User",
            Gender = EnumGender.Femme,
            PhoneNumber = "0600000005",
            DateOfBirth = new DateTimeOffset(1992, 8, 20, 0, 0, 0, TimeSpan.Zero),
            Description = "Client user for testing",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        var clientResult = await userManager.CreateAsync(clientUser, "Client123!");
        if (clientResult.Succeeded)
        {
            await userManager.AddToRoleAsync(clientUser, "Client");
        }

        // un autre etudiant

        var updateUser = new UserApp
        {
            Email = "update@skillhive.fr",
            UserName = "update@skillhive.fr",
            FirstName = "Update",
            LastName = "User",
            Gender = EnumGender.Homme,
            PhoneNumber = "0600000006",
            DateOfBirth = new DateTimeOffset(1993, 4, 5, 0, 0, 0, TimeSpan.Zero),
            Description = "User for update testing",
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModifiedAt = DateTimeOffset.UtcNow
        };

        var updateResult = await userManager.CreateAsync(updateUser, "Update123!");
        if (updateResult.Succeeded)
        {
            await userManager.AddToRoleAsync(updateUser, "Student");
        }
    }
}
