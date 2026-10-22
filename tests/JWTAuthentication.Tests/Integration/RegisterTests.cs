using System.Net;
using System.Net.Http.Json;
using JWTAuthentication.Tests.Infrastructure;

namespace JWTAuthentication.Tests.Integration;

[Collection(ApiCollection.Name)]
public sealed class RegisterTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var response = await _client.RegisterAsync(AuthClient.NewUser());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("userId", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var user = AuthClient.NewUser();
        await _client.RegisterAsync(user);

        var sameEmail = AuthClient.NewUser() with { Email = user.Email };
        var response = await _client.RegisterAsync(sameEmail);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUserName_ReturnsConflict()
    {
        var user = AuthClient.NewUser();
        await _client.RegisterAsync(user);

        var sameName = AuthClient.NewUser() with { UserName = user.UserName };
        var response = await _client.RegisterAsync(sameName);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("short1!")]        // too short
    [InlineData("alllowercase1!")] // no upper case
    [InlineData("NoDigitsHere!")]  // no digit
    [InlineData("NoSymbols123")]   // no symbol
    public async Task Register_WithWeakPassword_ReturnsBadRequest(string password)
    {
        var user = AuthClient.NewUser() with { Password = password };

        var response = await _client.RegisterAsync(user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsValidationProblem()
    {
        var user = AuthClient.NewUser() with { Email = "not-an-email" };

        var response = await _client.RegisterAsync(user);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email", body);
    }

    [Fact]
    public async Task Register_CannotChooseAdminRole()
    {
        // An attacker adds a "role" field to the request. It must be ignored.
        var user = AuthClient.NewUser();
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            userName = user.UserName,
            email = user.Email,
            password = user.Password,
            firstName = "Evil",
            lastName = "User",
            dateOfBirth = "1990-01-01",
            role = "Admin"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var tokens = await _client.LoginSuccessfullyAsync(user.UserName, user.Password);
        var me = await _client.SendAuthorizedAsync(HttpMethod.Get, "/api/auth/me", tokens.AccessToken);
        var profile = await me.Content.ReadFromJsonAsync<MeResponse>();

        Assert.Equal(new[] { "User" }, profile!.Roles);

        var admin = await _client.SendAuthorizedAsync(HttpMethod.Get, "/api/auth/admin", tokens.AccessToken);
        Assert.Equal(HttpStatusCode.Forbidden, admin.StatusCode);
    }
}
