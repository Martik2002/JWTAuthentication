using System.Net;
using System.Net.Http.Json;
using JWTAuthentication.Tests.Infrastructure;

namespace JWTAuthentication.Tests.Integration;

[Collection(ApiCollection.Name)]
public sealed class LoginTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsTokens()
    {
        var user = AuthClient.NewUser();
        await _client.RegisterAsync(user);

        var tokens = await _client.LoginSuccessfullyAsync(user.UserName, user.Password);

        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));
        Assert.True(tokens.AccessTokenExpiresAt > DateTime.UtcNow);
        Assert.True(tokens.RefreshTokenExpiresAt > tokens.AccessTokenExpiresAt);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var user = AuthClient.NewUser();
        await _client.RegisterAsync(user);

        var response = await _client.LoginAsync(user.UserName, "Wr0ng!Password");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithPasswordEqualToUserName_ReturnsUnauthorized()
    {
        // Regression test: the old code verified the username instead of the password
        var user = AuthClient.NewUser();
        await _client.RegisterAsync(user);

        var response = await _client.LoginAsync(user.UserName, user.UserName);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ReturnsSameErrorAsWrongPassword()
    {
        var unknown = await _client.LoginAsync("does_not_exist", "Wr0ng!Password");

        var user = AuthClient.NewUser();
        await _client.RegisterAsync(user);
        var wrongPassword = await _client.LoginAsync(user.UserName, "Wr0ng!Password");

        // Same status and message, so an attacker can't tell which usernames exist
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);

        var unknownProblem = await unknown.Content.ReadFromJsonAsync<ProblemResponse>();
        var wrongPasswordProblem = await wrongPassword.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal(wrongPasswordProblem!.Title, unknownProblem!.Title);
        Assert.Equal(wrongPasswordProblem.Detail, unknownProblem.Detail);
    }

    [Fact]
    public async Task Login_AfterTooManyFailedAttempts_LocksTheAccount()
    {
        var user = AuthClient.NewUser();
        await _client.RegisterAsync(user);

        for (var i = 0; i < 5; i++)
        {
            await _client.LoginAsync(user.UserName, "Wr0ng!Password");
        }

        // Even the correct password is rejected while the account is locked
        var response = await _client.LoginAsync(user.UserName, user.Password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyBody_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SeededAdmin_CanAccessAdminEndpoint()
    {
        var tokens = await _client.LoginSuccessfullyAsync(ApiFactory.AdminUserName, ApiFactory.AdminPassword);

        var response = await _client.SendAuthorizedAsync(HttpMethod.Get, "/api/auth/admin", tokens.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
