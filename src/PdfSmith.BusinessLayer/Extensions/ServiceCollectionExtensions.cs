using Microsoft.Extensions.DependencyInjection;
using PdfSmith.BusinessLayer.Services;
using PdfSmith.BusinessLayer.Services.Interfaces;

namespace PdfSmith.BusinessLayer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTimeZoneService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ITimeZoneService, TimeZoneService>();

        return services;
    }
}
