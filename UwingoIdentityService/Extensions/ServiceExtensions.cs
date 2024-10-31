using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RapositoryAppClient;
using Repositories;
using Repositories.Contracts;
using Repositories.EFCore;
using RepositoryAppClient.Contracts;
using RepositoryAppClient.EFCore;
using Services.Contracts;
using Services.EFCore;
using System.Text;

namespace UwingoIdentityService.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            //Migration atarken burayı açmak aşağıdakini kapatmak gerekiyor.
            //services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>(); 
        }

        public static void ConfigureDbContextAppClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContextAppClient>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionAppClient")));
            //Migration atarken burayı açmak aşağıdakini kapatmak gerekiyor.
            //services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>(); 
        }

        public static void ConfigureEmailSenderOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        }

        //public static void ConfigureIdentity(this IServiceCollection services)
        //{
        //    services.AddIdentity<User, Role>
        //        (
        //            opts =>
        //            {
        //                opts.Password.RequireDigit = true;
        //                opts.Password.RequireLowercase = true;
        //                opts.Password.RequireUppercase = true;
        //                opts.Password.RequireNonAlphanumeric = true;
        //                opts.Password.RequiredLength = 8;

        //                opts.User.RequireUniqueEmail = true;
        //                opts.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;

        //            }
        //        )
        //        .AddDefaultTokenProviders()
        //        .AddEntityFrameworkStores<RepositoryContext>();
        //}

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            // RepositoryContext için Identity yapılandırması
            services.AddIdentity<User, Role>(opts =>
            {
                opts.Password.RequireDigit = true;
                opts.Password.RequireLowercase = true;
                opts.Password.RequireUppercase = true;
                opts.Password.RequireNonAlphanumeric = true;
                opts.Password.RequiredLength = 8;
                opts.User.RequireUniqueEmail = true;
                opts.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider)
            .AddEntityFrameworkStores<RepositoryContext>()
            .AddDefaultTokenProviders();

            // RepositoryContextAppClient için bağımsız Identity yapılandırması
            services.AddIdentityCore<UwingoUser>(opts =>
            {
                opts.Password.RequireDigit = true;
                opts.Password.RequireLowercase = true;
                opts.Password.RequireUppercase = true;
                opts.Password.RequireNonAlphanumeric = true;
                opts.Password.RequiredLength = 8;
                opts.User.RequireUniqueEmail = true;
                opts.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddTokenProvider<DataProtectorTokenProvider<UwingoUser>>(TokenOptions.DefaultProvider)
            .AddEntityFrameworkStores<RepositoryContextAppClient>()
            .AddDefaultTokenProviders();
        }



        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped<IRepositoryApplication, RepositoryApplication>();
            services.AddScoped<IRepositoryCompany, RepositoryCompany>();
            services.AddScoped<IRepositoryCompanyApplication, RepositoryCompanyApplication>();
            services.AddScoped<IRepositoryRole, RepositoryRole>();
            services.AddScoped<IRepositoryTenant, RepositoryTenant>();
            //services.AddScoped<IRepositoryUserRole, RepositoryUserRole>();
            services.AddScoped<IRepositoryBase<User>, RepositoryBase<User>>();

            services.AddScoped<IRepositoryAppClientManager, RepositoryAppClientManager>();
            services.AddScoped<IRepositoryAppClientRole, RepositoryAppClientRole>();
            services.AddScoped<IRepositoryAppClientBase<User>, RepositoryAppClientBase<User>>();
        }

        public static void ConfigureServiceManager(this IServiceCollection services)
        {
            services.AddScoped<ICustomAuthorizationService, CustomAuthorizationService>();
            services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, CustomAuthorizationHandler>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanyApplicationService, CompanyApplicationService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ITenantService, TenantService>();
            //services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IEmailService, EmailService>();

        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings"); //appsettingsteki istenilen tagı okumaya yarar
            var secretKey = jwtSettings["SecretKey"];
            //Tokenlarda hassas veriler hiçbir şekilde yer almamalıdır //userName, password, eMail, phoneNumber etc
            TokenValidationParameters validateParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["ValidateIssue"],
                ValidAudience = jwtSettings["ValidateAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(jwtSettings.GetSection("Expire").Value)),
            };


            var data = services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["ValidateIssue"],
                    ValidAudience = jwtSettings["ValidateAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(jwtSettings.GetSection("Expire").Value))
                }

            );
            var asd = data;

        }

        public static void ConfigureAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    var policyProviderService = serviceProvider.GetRequiredService<ICustomAuthorizationService>();
                    var requirements = policyProviderService.GetCustomRequirementsAsync().Result;

                    foreach (var requirement in requirements)
                    {
                        options.AddPolicy(requirement.ClaimValue, policy =>
                            policy.RequireClaim(requirement.ClaimType, requirement.ClaimValue));
                    }
                }
            });
        }

        public static void ConfigureMyJsonSerializer(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                });
        }

    }
}
