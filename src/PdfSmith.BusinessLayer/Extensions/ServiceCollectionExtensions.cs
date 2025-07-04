using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.Extensions;

namespace PdfSmith.BusinessLayer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRazorLightEngine(this IServiceCollection services)
    {
        services.AddRazorLight(() =>
        {
            var assembly = Assembly.GetExecutingAssembly();

            return new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(assembly)
                .SetOperatingAssembly(assembly)
                .UseMemoryCachingProvider()
                .Build();
        });

        return services;
    }
}
