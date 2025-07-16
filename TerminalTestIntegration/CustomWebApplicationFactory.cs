using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using TerminalApi;
using TerminalApi.Contexts;

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
        builder.ConfigureServices(services =>
        {
            // Remove the existing database context registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApiDefaultContext>));
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
            
            try
            {
                // Ensure database is created and migrations are applied
                context.Database.EnsureCreated();
                
                // Or use migrations if you prefer:
                // context.Database.Migrate();
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
}

