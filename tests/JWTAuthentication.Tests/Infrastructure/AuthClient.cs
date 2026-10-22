using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace JWTAuthentication.Tests.Infrastructure;

public sealed record TokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

public sealed record MeResponse(
    string Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    List<string> Roles);

public sealed record ProblemResponse(int Status, string Title, string Detail);

public sealed record TestUser(string UserName, string Email, string Password);

/// <summary>
/// Small helpers so the tests read like the scenario they check.
/// </summary>
public static class AuthClient
{
    public const string ValidPassword = "Str0ng!Password";

    public static TestUser NewUser()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        return new TestUser($"user_{id}", $"user_{id}@example.com", ValidPassword);
    }

    public static Task<HttpResponseMessage> RegisterAsync(this HttpClient client, TestUser user) =>
        client.PostAsJsonAsync("/api/auth/register", new
        {
            userName = user.UserName,
            email = user.Email,
            password = user.Password,
            firstName = "Test",
            lastName = "User",
            middleName = "",
            phoneNumber = "+37400000000",
            dateOfBirth = "1995-05-20"
        });

    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string userName, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new { userName, password });

    public static Task<HttpResponseMessage> RefreshAsync(this HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

    public static async Task<TokenResponse> RegisterAndLoginAsync(this HttpClient client, TestUser user)
    {
        var register = await client.RegisterAsync(user);
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        return await client.LoginSuccessfullyAsync(user.UserName, user.Password);
    }

    public static async Task<TokenResponse> LoginSuccessfullyAsync(this HttpClient client, string userName, string password)
    {
        var login = await client.LoginAsync(userName, password);
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var tokens = await login.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokens);
        return tokens;
    }

    public static Task<HttpResponseMessage> SendAuthorizedAsync(
        this HttpClient client, HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client.SendAsync(request);
    }
}
