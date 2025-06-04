using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace PdfSmith.BusinessLayer.Extensions;

public static class JsonDocumentExtensions
{
    public static object ToExpandoObject(this JsonDocument document)
        => ConvertElement(document.RootElement);

    private static object ConvertElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;
            foreach (var property in element.EnumerateObject())
            {
                expando[ToPascalCase(property.Name)] = ConvertValue(property.Value)!;
            }

            return (ExpandoObject)expando!;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray().Select(ConvertValue).ToList();
        }

        throw new InvalidOperationException($"Unsupported JSON ValueKind: {element.ValueKind}");
    }

    private static object? ConvertValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ConvertElement(element),
            JsonValueKind.Array => element.EnumerateArray().Select(ConvertValue).ToList(),
            JsonValueKind.String => ParseStringValue(element),
            JsonValueKind.Number => element.TryGetInt64(out var number) ? number : element.GetDouble(),
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            JsonValueKind.Null => null,

            _ => throw new UnreachableException($"Unsupported JSON ValueKind: {element.ValueKind}")
        };

        static object? ParseStringValue(JsonElement element)
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
                return dateTime;
            }

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timeSpan))
            {
                return timeSpan;
            }

            return value;
        }
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder();
        var capitalizeNext = true;

        foreach (var c in input)
        {
            if (c is '_' or '-' or '.')
            {
                capitalizeNext = true;
                continue;
            }

            if (capitalizeNext)
            {
                sb.Append(char.ToUpper(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}

