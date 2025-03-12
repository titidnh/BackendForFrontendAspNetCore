namespace BackendForFrontendAspNetCore
{
    public class UserInfo
    {
        public required bool IsAuthenticated { get; set; }
        public UserClaim[] UserClaims { get; set; } = Array.Empty<UserClaim>();
    }
}
