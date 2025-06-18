using JWTAuthentication.Application.Abstractions.Interfaces;
using JWTAuthentication.Common.Models.AuthResponse;
using JWTAuthentication.Entities;

namespace JWTAuthentication.Application.ActiveUser;

public sealed record GetActiveUserCommand : IRequest<ActiveUserResponse>;