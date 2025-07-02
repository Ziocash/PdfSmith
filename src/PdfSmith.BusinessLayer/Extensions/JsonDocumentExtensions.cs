using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Text.Json;

namespace PdfSmith.BusinessLayer.Extensions;

public static class JsonDocumentExtensions
{
    public static object ToExpandoObject(this JsonDocument document, TimeZoneInfo? timeZoneInfo)
        => ConvertElement(document.RootElement, timeZoneInfo);

    private static object ConvertElement(JsonElement element, TimeZoneInfo? timeZoneInfo)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;
            foreach (var property in element.EnumerateObject())
            {
                expando[property.Name.ToPascalCase()] = ConvertValue(property.Value, timeZoneInfo)!;
            }

            return (ExpandoObject)expando!;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray().Select(e => ConvertValue(e, timeZoneInfo)).ToList();
        }

        throw new InvalidOperationException($"Unsupported JSON ValueKind: {element.ValueKind}");
    }

    private static object? ConvertValue(JsonElement element, TimeZoneInfo? timeZoneInfo)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertElement(element, timeZoneInfo),
            JsonValueKind.Array => element.EnumerateArray().Select(e => ConvertValue(e, timeZoneInfo)).ToList(),
            JsonValueKind.String => ParseStringValue(element, timeZoneInfo),
            JsonValueKind.Number => element.TryGetInt64(out var number) ? number : element.GetDouble(),
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            JsonValueKind.Null => null,

            _ => throw new UnreachableException($"Unsupported JSON ValueKind: {element.ValueKind}")
        };

        static object? ParseStringValue(JsonElement element, TimeZoneInfo? timeZoneInfo)
        {
            var value = element.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (element.TryGetGuid(out var guid))
            {
                return guid;
            }

            if (element.TryGetDateTime(out var dateTime))
            {
                if (timeZoneInfo is null)
                {
                    return dateTime;
                }

                return dateTime.Kind == DateTimeKind.Unspecified
                    ? TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), timeZoneInfo)
                    : TimeZoneInfo.ConvertTime(dateTime, timeZoneInfo);
            }

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timeSpan))
            {
                return timeSpan;
            }

            return value;
        }
    }
}

