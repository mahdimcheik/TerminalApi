using iText.Signatures.Validation.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TerminalApi;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Utilities;
using Testcontainers.PostgreSql;

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
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(async services =>
        {
            // Remove the existing database context registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<ApiDefaultContext>)
            );
            if (descriptor != null)
                services.Remove(descriptor);

            // Add new database context with testcontainer connection string
            services.AddDbContext<ApiDefaultContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
                options.EnableSensitiveDataLogging(); // Helpful for debugging tests
            });

            // Build service provider to initialize the database
            var serviceProvider = services.BuildServiceProvider();

            // Create database and apply migrations
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApiDefaultContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserApp>>();

            try
            {
                // Ensure database is created and migrations are applied
                context.Database.EnsureCreated();

                await SeedDataAsync(userManager);
            }
            catch (Exception ex)
            {
                // Log any database initialization errors
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
        });

        // Set the environment to Testing to avoid production-specific configurations
        builder.UseEnvironment("Testing");
    }

    public async Task SeedDataAsync(UserManager<UserApp> userManager)
    {
        var student = new UserApp
        {
            Email = "student@skillhive.fr",
            FirstName = "mahdi",
            LastName = "mcheik",
            Gender = EnumGender.Homme,
            PhoneNumber = "123456789",
            DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = "Test user for integration test",
        };

        var result = await userManager.CreateAsync(student, "Olitec1>");

        Console.WriteLine("*********" + result);
        //await context.SaveChangesAsync();
    }
}
