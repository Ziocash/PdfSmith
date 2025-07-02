namespace PdfSmith.BusinessLayer.Services.Interfaces;

public interface ITimeZoneService
{
    TimeZoneInfo? GetTimeZone();
    string? GetTimeZoneHeaderValue();
}