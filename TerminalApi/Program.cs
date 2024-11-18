using System.Data;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TerminalApi.Contexts;
using TerminalApi.Models.Role;
using TerminalApi.Models.User;
using TerminalApi.Services;
using TerminalApi.Utilities;

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
                        //.AllowAnyOrigin()
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
            services
                .AddAuthentication(options =>
                {
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
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
                        ValidIssuer = EnvironmentVariables.API_BASE_URL,
                        ValidAudience = EnvironmentVariables.USER_BASE_URL,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
                        ),
                    };
                });
            /*.AddCookie(options =>
            {
                options.Cookie.Name = CookieName;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.MaxAge = TimeSpan.FromDays(365);
                options.Cookie.IsEssential = true;

                options.Events.OnRedirectToAccessDenied = (ctx) =>
                {
                    if (ctx.Response.StatusCode == 200)
                    {
                        ctx.Response.StatusCode = 403;
                    }

                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogin = (ctx) =>
                {
                    if (ctx.Response.StatusCode == 200)
                    {
                        ctx.Response.StatusCode = 401;
                    }

                    return Task.CompletedTask;
                };
            });*/
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SendMailService>();
            services.AddScoped<AddressService>();
            services.AddScoped<FormationService>();
            services.AddScoped<SlotService>();


            //Lowercase routing
            services.AddRouting(opt => opt.LowercaseUrls = true);

            // Set the active provider via configuration
            var configuration = services.BuildServiceProvider().GetService<IConfiguration>();
            string? provider = configuration?.GetValue(
                "Provider",
                EnvironmentVariables.DB_PROVIDER
            );

            services.AddDbContext<ApiDefaultContext>(options =>
            {
                options.UseSqlite("Data Source = d:\\terminal.db;");
                
            });
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

            // Enable routing.
            app.UseRouting();

            // Enable Cross-Origin Resource Sharing (CORS).
            app.UseCors();

            // Enable HTTPS redirection (if needed).
            // _webApplication.UseHttpsRedirection();

            // Enable authorization.
            app.UseAuthorization();

            // Map controllers.
            app.MapControllers();

            // Check and limit the payload size.
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
