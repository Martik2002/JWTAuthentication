using JWTAuthentication.Application.Auth.Register;
using JWTAuthentication.Common.Security;
using JWTAuthentication.Entities;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.Tests.Unit;

public sealed class RefreshTokenHasherTests
{
    [Fact]
    public void Generate_ReturnsUniqueUrlSafeTokens()
    {
        var tokens = Enumerable.Range(0, 100).Select(_ => RefreshTokenHasher.Generate()).ToList();

        Assert.Equal(tokens.Count, tokens.Distinct().Count());
        Assert.All(tokens, t => Assert.DoesNotContain(t, c => c is '+' or '/' or '='));
    }

    [Fact]
    public void Hash_IsDeterministic_AndDifferentFromToken()
    {
        var token = RefreshTokenHasher.Generate();

        var hash1 = RefreshTokenHasher.Hash(token);
        var hash2 = RefreshTokenHasher.Hash(token);

        Assert.Equal(hash1, hash2);
        Assert.NotEqual(token, hash1);
        Assert.Equal(64, hash1.Length); // SHA-256 as hex
    }
}

public sealed class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();
    private readonly User _user = new();

    [Fact]
    public void Verify_WithCorrectPassword_Succeeds()
    {
        var hash = _hasher.HashPassword(_user, "Str0ng!Password");

        Assert.Equal(PasswordVerificationResult.Success,
            _hasher.VerifyHashedPassword(_user, hash, "Str0ng!Password"));
    }

    [Fact]
    public void Verify_WithWrongPassword_Fails()
    {
        var hash = _hasher.HashPassword(_user, "Str0ng!Password");

        Assert.Equal(PasswordVerificationResult.Failed,
            _hasher.VerifyHashedPassword(_user, hash, "Other!Password1"));
    }

    [Fact]
    public void Verify_WithInvalidHash_FailsInsteadOfThrowing()
    {
        Assert.Equal(PasswordVerificationResult.Failed,
            _hasher.VerifyHashedPassword(_user, "not-a-bcrypt-hash", "Str0ng!Password"));
    }
}

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new(TimeProvider.System);

    private static RegisterCommand ValidCommand() => new(
        UserName: "martik",
        Email: "martik@example.com",
        Password: "Str0ng!Password",
        FirstName: "Martik",
        LastName: "Avagyan",
        MiddleName: null,
        PhoneNumber: null,
        DateOfBirth: new DateTime(1995, 5, 20));

    [Fact]
    public void ValidCommand_Passes()
    {
        Assert.True(_validator.Validate(ValidCommand()).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("has spaces")]
    [InlineData("<script>")]
    public void InvalidUserName_Fails(string userName)
    {
        var result = _validator.Validate(ValidCommand() with { UserName = userName });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.UserName));
    }

    [Fact]
    public void DateOfBirthInFuture_Fails()
    {
        var result = _validator.Validate(ValidCommand() with { DateOfBirth = DateTime.UtcNow.AddDays(1) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.DateOfBirth));
    }

    [Fact]
    public void MissingDateOfBirth_Fails()
    {
        var result = _validator.Validate(ValidCommand() with { DateOfBirth = null });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterCommand.DateOfBirth));
    }
}
