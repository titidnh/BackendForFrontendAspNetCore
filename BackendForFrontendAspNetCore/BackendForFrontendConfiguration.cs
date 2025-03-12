using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace BackendForFrontendAspNetCore
{
    public class BackendForFrontendConfiguration
    {
        public required string FrontendUrl { get; set; }

        public required string Authority { get; set; }

        public required string ClientId { get; set; }

        public required string ClientSecret { get; set; }

        public string ClaimsIssuer { get; set; } = "Auth0";

        public string? ValidIssuer { get; set; }

        public string? ValidAudience { get; set; }

        public string CallbackPath { get; set; } = "/callback";

        public OpenIdConnectEvents? OpenIdConnectEvents { get; set; }
    }
}
