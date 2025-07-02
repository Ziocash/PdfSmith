using Microsoft.AspNetCore.Http;
using PdfSmith.BusinessLayer.Services.Interfaces;

namespace PdfSmith.BusinessLayer.Services;

public class TimeZoneService(IHttpContextAccessor httpContextAccessor) : ITimeZoneService
{
    public static readonly string X_TIME_ZONE = "x-time-zone";

    public TimeZoneInfo? GetTimeZone()
    {
        var timeZoneId = GetTimeZoneHeaderValue();

        if (timeZoneId is null)
        {
            return null;
        }

        return GetTimeZoneInfo(timeZoneId);
    }

    public string? GetTimeZoneHeaderValue()
    {
        if (httpContextAccessor.HttpContext?.Request?.Headers?.TryGetValue(X_TIME_ZONE, out var timeZone) ?? false)
        {
            return timeZone.ToString();
        }

        return null;
    }

    private static TimeZoneInfo? GetTimeZoneInfo(string timeZone)
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out var timeZoneInfo))
        {
            return timeZoneInfo;
        }

        return null;
    }
}
