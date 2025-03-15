namespace BackendForFrontendAspNetCore.Auth0Provider
{
    internal class Auth0RoleConfiguration
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string Auth0Domain { get; set; }
        public required string Audience { get; set; }
    }
}
