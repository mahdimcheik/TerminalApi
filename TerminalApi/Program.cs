using System.Data;
using System.Reflection;
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PuppeteerSharp;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;
using TerminalApi.Utilities.Policies.NotBanned;

namespace TerminalApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            EnvironmentVariables.Initialize(builder.Configuration);

            var services = builder.Services;

            ConfigureServices(services);
            var toto = new BrowserFetcher().DownloadAsync().Result;

            var launchOptions = new LaunchOptions
            {
                Headless = true, // = false for testing
            };

            var app = builder.Build();
            ConfigureMiddlewarePipeline(app);
            using (var scope = app.Services.CreateScope())
            {
                var service = scope.ServiceProvider;
                try
                {
                    await SeedAdminUserAsync(service);
                }
                catch (Exception ex)
                {
                    // Log l'erreur si nécessaire
                    Console.WriteLine($"Erreur lors du seed de l'utilisateur admin : {ex.Message}");
                }
            }

            app.Run();
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                c.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                    }
                );

                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer",
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        },
                    }
                );
            });

            services.AddHttpClient();
        }

        private static void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .SetIsOriginAllowed(CorsHelper.IsOriginAllowed)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("filename");
                });
            });
        }

        private static void ConfigureControllers(IServiceCollection services)
        {
            services.AddControllers();
        }

        private static void ConfigureIdentity(IServiceCollection services)
        {
            services
                .AddIdentity<UserApp, Role>()
                .AddEntityFrameworkStores<ApiDefaultContext>()
                .AddRoleManager<RoleManager<Role>>()
                .AddUserManager<UserManager<UserApp>>()
                .AddSignInManager<SignInManager<UserApp>>()
                .AddErrorDescriber<FrenchIdentityErrorDescriber>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters =
                    " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });
        }

        private static void ConfigureAuthentication(IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = EnvironmentVariables.API_BACK_URL,
                        ValidAudience = EnvironmentVariables.API_BACK_URL,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
                        ),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            var path = context.HttpContext.Request.Path;
                            if (
                                !string.IsNullOrEmpty(accessToken)
                                && (path.StartsWithSegments("/notificationHub"))
                            )
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "NotBanned",
                    policy => policy.Requirements.Add(new NotBannedRequirement())
                );
            });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var config = services.BuildServiceProvider().GetService<IConfiguration>();
            services.Configure<AppSettings>(config!.GetSection("AppSettings"));

            services.AddSingleton<ISendMailService, SendMailService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IFormationService, FormationService>();
            services.AddScoped<ISlotService, SlotService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentsService, PaymentsService>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IJobChron, JobChron>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICursusService, CursusService>();
            services.AddScoped<IAuthorizationHandler, NotBannedHandler>();
            services.AddScoped<FakerService>();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationFormats.Add(
                    "/TemplatesInvoice/{0}" + RazorViewEngine.ViewExtension
                );
            });
            services.AddTransient<IPdfService, PdfService>();
            services.AddRouting(opt => opt.LowercaseUrls = true);

            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            string? provider = configuration?.GetValue(
                "Provider",
                EnvironmentVariables.DB_PROVIDER
            );


            services.AddDbContext<ApiDefaultContext>(options =>
            {
                options.UseNpgsql(
                    $"Host={EnvironmentVariables.DB_HOST};Port={EnvironmentVariables.DB_PORT};Database={EnvironmentVariables.DB_NAME};Username={EnvironmentVariables.DB_USER};Password={EnvironmentVariables.DB_PASSWORD};"
                );
            });

            var environment = services.BuildServiceProvider().GetService<IWebHostEnvironment>();
            if (environment?.EnvironmentName != "Testing")
            {
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApiDefaultContext>();
                    context.Database.Migrate();
                }
            }

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });

            // Skip Hangfire in test environments to prevent integration test failures
            if (environment?.EnvironmentName != "Testing")
            {
                services.AddHangfire(configuration =>
                    configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UsePostgreSqlStorage(
                            (options) =>
                            {
                                options.UseNpgsqlConnection(
                                    $"Host={EnvironmentVariables.DB_HOST};Port={EnvironmentVariables.DB_PORT};Database={EnvironmentVariables.DB_NAME};Username={EnvironmentVariables.DB_USER};Password={EnvironmentVariables.DB_PASSWORD};"
                                );
                            }
                        )
                );

                services.AddHangfireServer();
            }

            ConfigureCors(services);
            ConfigureControllers(services);
            ConfigureSwagger(services);
            ConfigureIdentity(services);
            ConfigureAuthentication(services);
        }

        private static void ConfigureMiddlewarePipeline(WebApplication app)
        {
            var supportedCultures = new string[] { "fr-FR" };
            app.UseRequestLocalization(options =>
                options
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures)
                    .SetDefaultCulture("fr-FR")
            );

            app.UseStaticFiles();

            app.UseAuthentication();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "data_lib v1");
                c.RoutePrefix = "swagger";
            });
            
            // Skip Hangfire in test environments to prevent integration test failures
            if (!app.Environment.IsEnvironment("Testing"))
            {
                app.UseHangfireDashboard("/hangfire");
            }

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();
            app.Use(
                async (context, next) =>
                {
                    if (context.Request.ContentLength > 200_000_000)
                    {
                        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                        await context.Response.WriteAsync("Payload Too Large");
                        return;
                    }

                    await next.Invoke();
                }
            );
        }

        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<UserApp>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var context = serviceProvider.GetRequiredService<ApiDefaultContext>();

            string adminEmail = "teacher@skillhive.fr";
            string adminPassword = "Admin123!";

            string adminRole = "Admin";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(
                    new Role
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                    }
                );
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new UserApp
                {
                    Id = EnvironmentVariables.TEACHER_ID,
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "Admin",
                    PhoneNumber = "0606060606",
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
                else
                {
                    throw new Exception(
                        $"Erreur lors de la création de l'admin: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );
                }
            }

            var countTVA = context.TVARates.Count();
            if (countTVA == 0)
            {
                TVARate defaultRate = new TVARate { Rate = 0.2m, StartAt = DateTimeOffset.UtcNow };
                context.TVARates.Add(defaultRate);
                context.SaveChanges();
            }
        }
    }
}


/*********** usefull command ***************/
// dotnet ef database update --context ApiDefaultContext -- --launch-profile "Base"
// stripe listen --forward-to https://localhost:7113/payments/webhook
