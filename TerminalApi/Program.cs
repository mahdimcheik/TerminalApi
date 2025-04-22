using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
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
using TerminalApi.Contexts;
using TerminalApi.Models.Role;
using TerminalApi.Models.User;
using TerminalApi.Services;
using TerminalApi.Utilities;
using TerminalApi.Utilities.Policies.NotBanned;

namespace TerminalApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;

            ConfigureServices(services);

            var app = builder.Build();
            ConfigureMiddlewarePipeline(app);

            app.Run();
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {
            // Configuration Swagger
            services.AddSwaggerGen(c =>
            {
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
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
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                            },
                            new List<string>()
                        }
                    }
                );
            });

            services.AddHttpClient();
        }

        private static void ConfigureCors(IServiceCollection services)
        {
            // Configurez CORS ici
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
            // Configuration des contr�leurs
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
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                    " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Login settings.
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            });
        }

        private static void ConfigureAuthentication(IServiceCollection services)
        {
            // authentication
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
                })
                .AddGoogle(options =>
                {
                    options.ClientId = EnvironmentVariables.ID_CLIENT_GOOGLE;
                    options.ClientSecret = EnvironmentVariables.SECRET_CLIENT_GOOGLE;
                    options.CallbackPath = new PathString("/google-callback");
                });
            // authorization
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
            services.AddSingleton<SendMailService>();
            services.AddScoped<AddressService>();
            services.AddScoped<FormationService>();
            services.AddScoped<SlotService>();
            services.AddScoped<BookingService>();
            services.AddScoped<OrderService>();
            services.AddScoped<PaymentsService>();
            services.AddScoped<FakerService>();
            services.AddSingleton<SseConnectionManager>();
            services.AddSingleton<SseService>();
            services.AddScoped<PdfService>();
            services.AddScoped<JobChron>();
            services.AddScoped<NotificationService>();
            services.AddScoped<UsersService>();
            services.AddScoped<AuthService>();
            services.AddScoped<IAuthorizationHandler, NotBannedHandler>();

            // logger
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
            services.AddTransient<PdfService>();
            services.AddRouting(opt => opt.LowercaseUrls = true);

            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            string? provider = configuration?.GetValue(
                "Provider",
                EnvironmentVariables.DB_PROVIDER
            );

            services.AddDbContext<ApiDefaultContext>(options =>
            {
                options.UseNpgsql("Host=localhost;Port=5432;Database=leprojet;Username=postgres;Password=beecoming;");               
            });

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });

            // hangfire
            services.AddHangfire(configuration =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(
                        (options) =>
                        {
                            options.UseNpgsqlConnection(
                                "Host=localhost;Port=5432;Database=leprojet;Username=postgres;Password=beecoming;"
                            );
                        }
                    )
            );

            services.AddHangfireServer();
            ConfigureCors(services);
            ConfigureControllers(services);
            ConfigureSwagger(services);
            ConfigureIdentity(services);
            ConfigureAuthentication(services);
        }

        private static void ConfigureMiddlewarePipeline(WebApplication app)
        {
            // Configure localization for supported cultures.
            var supportedCultures = new string[] { "fr-FR" };
            app.UseRequestLocalization(options =>
                options
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures)
                    .SetDefaultCulture("fr-FR")
            );

            app.UseStaticFiles();

            // Enable authentication.
            app.UseAuthentication();

            // Enable developer exception page if in development environment.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable Swagger and Swagger UI.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "data_lib v1");
                c.RoutePrefix = "swagger";
            });
            // hangfire
            app.UseHangfireDashboard("/hangfire");

            // Enable routing.
            app.UseRouting();

            // Enable Cross-Origin Resource Sharing (CORS).
            app.UseCors();

            // Enable HTTPS redirection (if needed).
            // app.UseHttpsRedirection();

            // Enable authorization.
            app.UseAuthorization();

            // Map controllers.
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
    }
}


/*********** usefull command ***************/
// dotnet ef database update --context ApiDefaultContext -- --launch-profile "Base"
// stripe listen --forward-to https://localhost:7113/payments/webhook
