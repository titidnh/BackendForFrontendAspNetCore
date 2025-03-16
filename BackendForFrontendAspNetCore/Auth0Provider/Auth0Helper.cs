using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;

namespace BackendForFrontendAspNetCore.Auth0Provider
{
    public class Auth0Helper
    {
        private static readonly HttpClient client = new HttpClient();

        public static BackendForFrontendConfiguration CreateConfiguration(string auth0Domain, string clientId, string clientSecret, string frontendUrl, bool activateRoles = false, string? customCallbackPath = null)
        {
            var configurationForBackendFrontent = new BackendForFrontendConfiguration()
            {
                Authority = $"https://{auth0Domain}",
                ClientId = clientId,
                ClientSecret = clientSecret,
                FrontendUrl = frontendUrl,
                ClaimsIssuer = "Auth0",
                ValidIssuer = $"https://{auth0Domain}",
                ValidAudience = clientId,
                OpenIdConnectEvents = new OpenIdConnectEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (activateRoles)
                        {
                            var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;

                            if (claimsIdentity != null)
                            {
                                var userId = context.SecurityToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
                                var roles = await GetUserRolesAsync(new Auth0RoleConfiguration
                                {
                                    ClientId = clientId,
                                    ClientSecret = clientSecret,
                                    Auth0Domain = $"https://{auth0Domain}",
                                    Audience = $"https://{auth0Domain}" + "/api/v2/"
                                }, userId!);

                                foreach (var role in roles)
                                {
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                                }
                            }
                        }
                    },
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"https://{auth0Domain}/v2/logout?client_id={clientId}";
                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }

                }
            };

            if (customCallbackPath != null)
            {
                configurationForBackendFrontent.CallbackPath = customCallbackPath;
            }

            return configurationForBackendFrontent;
        }

        private static async Task<string[]> GetUserRolesAsync(Auth0RoleConfiguration auth0RoleConfiguration, string userId)
        {
            var accessToken = await GetAccessTokenAsync(auth0RoleConfiguration);
            var rolesUrl = $"{auth0RoleConfiguration.Auth0Domain}/api/v2/users/{userId}/roles";

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(rolesUrl);
            List<Role> roles = new List<Role>();
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Role[]>(responseBody);
                if (result != null)
                {
                    roles.AddRange(result);
                }
            }
            return roles.Select(x => x.Name).ToArray();
        }

        private class Role
        {
            public string? Id { get; set; }
            public required string Name { get; set; }
            public string? Description { get; set; }
        }

        private static async Task<string> GetAccessTokenAsync(Auth0RoleConfiguration auth0Configuration)
        {
            var tokenUrl = $"{auth0Configuration.Auth0Domain}/oauth/token";
            var requestBody = new
            {
                client_id = auth0Configuration.ClientId,
                client_secret = auth0Configuration.ClientSecret,
                audience = auth0Configuration.Audience,
                grant_type = "client_credentials"
            };
            var json = JObject.FromObject(requestBody).ToString();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(tokenUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var accessToken = JObject.Parse(responseBody)["access_token"]!.ToString();
                return accessToken;
            }
            else
            {
                throw new Exception($"Failed to obtain access token: {response.StatusCode}");
            }
        }
    }
}
