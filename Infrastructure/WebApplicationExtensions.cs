using System.Reflection;

namespace JWTAuthentication.Infrastructure;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Finds every <see cref="EndpointGroupBase"/> in this assembly and maps its endpoints.
    /// </summary>
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupTypes = Assembly.GetExecutingAssembly()
            .GetExportedTypes()
            .Where(t => t.IsSubclassOf(typeof(EndpointGroupBase)) && !t.IsAbstract);

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase group)
            {
                group.Map(app);
            }
        }

        return app;
    }
}
