namespace PdfSmith.BusinessLayer.Services.Interfaces;

public interface ITimeZoneService
{
    (string? TimeZone, TimeZoneInfo? TimeZoneInfo) GetTimeZone();
}