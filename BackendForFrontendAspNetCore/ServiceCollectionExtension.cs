using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace BackendForFrontendAspNetCore
{
    public static class ServiceCollectionExtension
    {
        public static void EnableCorsPolicyForBackendForFrontend(this IServiceCollection services, BackendForFrontendConfiguration backendForFrontendConfiguration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOriginForFrontend",
                    builder => builder
                        .WithOrigins(backendForFrontendConfiguration.FrontendUrl)
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .AllowAnyMethod());
            });
        }

        public static void EnableBackendForFrontend(this IServiceCollection services, BackendForFrontendConfiguration backendForFrontendConfiguration)
        {
            services.AddDataProtection();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.IsEssential = true;
                options.LoginPath = "/api/auth/login";
                options.LogoutPath = "/api/auth/logout";
                options.DataProtectionProvider = services.BuildServiceProvider().GetService<IDataProtectionProvider>();
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = backendForFrontendConfiguration.Authority;
                options.ClientId = backendForFrontendConfiguration.ClientId;
                options.ClientSecret = backendForFrontendConfiguration.ClientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.UsePkce = true;
                foreach (var scope in backendForFrontendConfiguration.Scopes)
                    options.Scope.Add(scope);
                options.CallbackPath = new PathString(backendForFrontendConfiguration.CallbackPath);
                options.ClaimsIssuer = backendForFrontendConfiguration.ClaimsIssuer;
                if (!string.IsNullOrEmpty(backendForFrontendConfiguration.ValidIssuer)
                    && !string.IsNullOrEmpty(backendForFrontendConfiguration.ValidAudience))
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = backendForFrontendConfiguration.ValidIssuer,
                        ValidateAudience = true,
                        ValidAudience = backendForFrontendConfiguration.ValidAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                }

                if (backendForFrontendConfiguration.OpenIdConnectEvents != null)
                {
                    options.Events = backendForFrontendConfiguration.OpenIdConnectEvents;
                }
            });
        }
    }
}