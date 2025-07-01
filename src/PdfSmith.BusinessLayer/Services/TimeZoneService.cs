using Microsoft.AspNetCore.Http;
using PdfSmith.BusinessLayer.Services.Interfaces;

namespace PdfSmith.BusinessLayer.Services;
public class TimeZoneService(IHttpContextAccessor httpContextAccessor) : ITimeZoneService
{
    public static readonly string X_TIME_ZONE = "x-time-zone";
    private static readonly string UTC = "Utc";

    public (string? TimeZone, TimeZoneInfo? TimeZoneInfo) GetTimeZone()
    {
        if (httpContextAccessor.HttpContext?.Request?.Headers?.TryGetValue(X_TIME_ZONE, out var timeZone) ?? false)
        {
            return string.IsNullOrWhiteSpace(timeZone)
                ? (timeZone.ToString(), null)
                : (timeZone.ToString(), GetTimeZoneInfo(timeZone!));
        }

        return (UTC, TimeZoneInfo.Utc);
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
