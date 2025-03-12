# BackendForFrontendAspNetCore


using BackendForFrontend.AspNetCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Configuration Auth0
var auth0Domain = "xxxxx.eu.auth0.com";
var clientId = "";
var clientSecret = "";

var configurationForBackendFrontent = new BackendForFrontendConfiguration()
{
    Authority = $"https://{auth0Domain}",
    ClientId = clientId,
    ClientSecret = clientSecret,
    FrontendUrl = "http://localhost:4200",
    ClaimsIssuer = "Auth0",
    ValidIssuer = $"https://{auth0Domain}",
    ValidAudience = clientId,
    OpenIdConnectEvents = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProviderForSignOut = (context) =>
        {
            var logoutUri = $"https://{auth0Domain}/v2/logout?client_id={clientId}";
            context.Response.Redirect(logoutUri);
            context.HandleResponse();
            return Task.CompletedTask;
        }

    }
};

builder.Services.EnableCorsPolicyForBackendForFrontend(configurationForBackendFrontent);
builder.Services.EnableBackendForFrontend(configurationForBackendFrontent);

builder.Services.AddControllers();

var app = builder.Build();
app.UseRouting();
app.UseCorsPolicyForBackendForFrontend();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.UseBackendForFrontend(configurationForBackendFrontent);
app.Run();
