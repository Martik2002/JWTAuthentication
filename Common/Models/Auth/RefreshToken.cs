namespace JWTAuthentication.Common.Models.AuthResponse;

public class RefreshToken
{
    public string UserName {get; set;}
    public string TokenString {get; set;}
    public DateTime Expires {get; set;}
    
}