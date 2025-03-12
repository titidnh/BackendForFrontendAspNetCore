using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BackendForFrontendAspNetCore
{
    public static class WebApplicationExtension
    {
        public static void UseCorsPolicyForBackendForFrontend(this WebApplication app)
        {
            app.UseCors("AllowSpecificOriginForFrontend");
        }

        public static void UseBackendForFrontend(this WebApplication app, BackendForFrontendConfiguration backendForFrontendConfiguration)
        {
            app.MapGet("/api/auth/login", async (HttpContext context) =>
            {
                await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
                {
                    RedirectUri = backendForFrontendConfiguration.FrontendUrl
                });
            });

            app.MapGet("/api/auth/profile", (HttpContext context) =>
            {
                var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
                var userInfo = new UserInfo { IsAuthenticated = isAuthenticated };
                if (isAuthenticated)
                {
                    userInfo.UserClaims = context.User.Claims
                    .Select(x => new UserClaim { Type = x.Type, Value = x.Value, ValueType = x.ValueType })
                    .ToArray();
                }

                return Results.Ok(userInfo);
            });

            app.MapGet("/api/auth/logout", [Authorize] async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                return Results.Ok();
            });
        }
    }
}
