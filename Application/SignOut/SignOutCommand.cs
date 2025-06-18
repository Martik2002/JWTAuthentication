using JWTAuthentication.Application.Abstractions.Interfaces;

namespace JWTAuthentication.Application.SignOut;

public  sealed record SignOutCommand : IRequest<string>;
    
