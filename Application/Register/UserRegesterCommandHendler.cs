using JWTAuthentication.Application.Abstractions.Interfaces;

namespace JWTAuthentication.Application.Register;

public class UserRegesterCommandHandler : IRequestHandler<UserRegisterCommand, string>
{
    public async Task<string> HandleAsync(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        return  request.Email;
    }
}