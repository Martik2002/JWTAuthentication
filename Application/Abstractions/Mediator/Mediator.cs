using JWTAuthentication.Application.Abstractions.Interfaces;

namespace JWTAuthentication.Application.Abstractions.Mediator;

public class Mediator(IServiceProvider serviceProvider) : IMediator 
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = serviceProvider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)request, cancellationToken);
    }
}