namespace JWTAuthentication.Application.Abstractions.Interfaces;

public interface IMediator
{
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken);
}