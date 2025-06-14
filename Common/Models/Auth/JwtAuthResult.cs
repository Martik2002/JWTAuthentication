namespace JWTAuthentication.Common.Models.AuthResponse;

public class JwtAuthResult
{
    public string AccessToken { get; set; }
    public RefreshToken RefreshToken { get; set; }
}