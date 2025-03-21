namespace JWTAuthentication.Interfaces;

public interface IPasswordHasher
{
    string Generate(String password);
    bool Verify(String password, string hashedPassword);
}