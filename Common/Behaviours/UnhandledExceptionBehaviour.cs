namespace JWTAuthentication.Common.Behaviours;

public class UnhandledExceptionBehaviour(RequestDelegate next, ILogger<UnhandledExceptionBehaviour> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context); // Continue pipeline
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "JWTAuth Request: ");
            throw;
        }
    }
}