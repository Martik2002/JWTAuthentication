using System.Net;
using System.Net.Http.Json;
using JWTAuthentication.Tests.Infrastructure;

namespace JWTAuthentication.Tests.Integration;

[Collection(ApiCollection.Name)]
public sealed class TokenLifecycleTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        var user = AuthClient.NewUser();
        var tokens = await _client.RegisterAndLoginAsync(user);

        var response = await _client.SendAuthorizedAsync(HttpMethod.Get, "/api/auth/me", tokens.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>();
        Assert.Equal(user.UserName, me!.UserName);
        Assert.Equal(user.Email, me.Email);
        Assert.Equal("Test", me.FirstName);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithTamperedToken_ReturnsUnauthorized()
    {
        var tokens = await _client.RegisterAndLoginAsync(AuthClient.NewUser());
        var tampered = tokens.AccessToken[..^4] + "abcd";

        var response = await _client.SendAuthorizedAsync(HttpMethod.Get, "/api/auth/me", tampered);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var tokens = await _client.RegisterAndLoginAsync(AuthClient.NewUser());

        var response = await _client.RefreshAsync(tokens.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newTokens = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotEqual(tokens.RefreshToken, newTokens!.RefreshToken);

        var me = await _client.SendAuthorizedAsync(HttpMethod.Get, "/api/auth/me", newTokens.AccessToken);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReusingAnOldToken_ReturnsUnauthorized()
    {
        // Refresh tokens are rotated: each one can be used only once
        var tokens = await _client.RegisterAndLoginAsync(AuthClient.NewUser());
        var first = await _client.RefreshAsync(tokens.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var reused = await _client.RefreshAsync(tokens.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_ReturnsUnauthorized()
    {
        var response = await _client.RefreshAsync("this-token-was-never-issued");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_DoesNotRequireAccessToken()
    {
        // The client refreshes exactly when its access token has expired,
        // so the endpoint must work without an Authorization header.
        var tokens = await _client.RegisterAndLoginAsync(AuthClient.NewUser());
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.RefreshAsync(tokens.RefreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var tokens = await _client.RegisterAndLoginAsync(AuthClient.NewUser());

        var logout = await _client.SendAuthorizedAsync(HttpMethod.Post, "/api/auth/logout", tokens.AccessToken);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await _client.RefreshAsync(tokens.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
