using Serilog.Core;
using Serilog.Events;

namespace PdfSmith.Logging;

public class HttpContextEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", httpContext.User?.Identity?.Name));

        if (httpContext.Request?.Headers?.TryGetValue("User-Agent", out var userAgent) == true)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("User-Agent", userAgent.ToString()));
        }
    }
}
