using System.Globalization;
using HandlebarsDotNet;
using PdfSmith.BusinessLayer.Exceptions;
using PdfSmith.BusinessLayer.Services;

namespace PdfSmith.BusinessLayer.Templating;

public class HandlebarsTemplateEngine(TimeZoneTimeProvider timeZoneTimeProvider) : ITemplateEngine
{
    static HandlebarsTemplateEngine()
    {
        RegisterGlobalHelpers();
    }

    public Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        try
        {
            var handlerbars = Handlebars.Compile(template);

            cancellationToken.ThrowIfCancellationRequested();

            var result = handlerbars(new { Model = model });
            return Task.FromResult(result);
        }
        catch (HandlebarsException ex)
        {
            throw new TemplateEngineException(ex.Message, ex);
        }
        catch (Exception ex)
        {
            throw new TemplateEngineException($"An error occurred while rendering the Handlebars template: {ex.Message}", ex);
        }
    }

    private static void RegisterGlobalHelpers()
    {
        var handlebarsInstance = Handlebars.Create();

        // Register helper for formatting currency
        Handlebars.RegisterHelper("formatCurrency", (context, arguments) =>
            arguments.Length switch
            {
                0 => string.Empty,
                >= 1 when decimal.TryParse(arguments[0].ToString(), CultureInfo.CurrentCulture, out var value)
                    => value.ToString("C", CultureInfo.CurrentCulture),
                _ => arguments.FirstOrDefault()?.ToString() ?? string.Empty
            });

        // Register helper for formatting dates
        Handlebars.RegisterHelper("formatDate", (context, arguments) =>
        {
            if (arguments.Length == 0)
            {
                return string.Empty;
            }

            var dateValue = arguments[0];
            var format = arguments.ElementAtOrDefault(1)?.ToString() ??
                $"{CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern} {CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern}";

            if (dateValue is DateTime dateTime)
            {
                return dateTime.ToString(format, CultureInfo.CurrentCulture);
            }

            if (dateValue is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.ToString(format, CultureInfo.CurrentCulture);
            }

            if (DateTime.TryParse(dateValue.ToString(), CultureInfo.CurrentCulture, out var parsedDate))
            {
                return parsedDate.ToString(format, CultureInfo.CurrentCulture);
            }

            return arguments.FirstOrDefault()?.ToString() ?? string.Empty;
        });

        // Register helper for mathematical operations.
        Handlebars.RegisterHelper("add", (context, arguments) => GetValues(arguments, out var value1, out var value2) ? value1 + value2 : 0m);
        Handlebars.RegisterHelper("multiply", (context, arguments) => GetValues(arguments, out var value1, out var value2) ? value1 * value2 : 0m);
        Handlebars.RegisterHelper("subtract", (context, arguments) => GetValues(arguments, out var value1, out var value2) ? value1 - value2 : 0m);
        Handlebars.RegisterHelper("divide", (context, arguments) => GetValues(arguments, out var value1, out var value2) ? value1 / value2 : 0m);
        Handlebars.RegisterHelper("round", (context, arguments) => GetValues(arguments, out var value1, out var value2) ? Math.Round(value1, (int)value2, MidpointRounding.AwayFromZero) : 0m);

        Handlebars.RegisterHelper("formatNumber", (context, arguments) =>
            arguments.Length switch
            {
                0 => string.Empty,
                >= 1 when decimal.TryParse(arguments[0].ToString(), CultureInfo.CurrentCulture, out var value)
                    => value.ToString(arguments[1].ToString(), CultureInfo.CurrentCulture),
                _ => arguments[0]
            });
    }

    private static bool GetValues(Arguments arguments, out decimal value1, out decimal value2)
    {
        value1 = value2 = 0;

        if (arguments.Length < 2)
        {
            return false;
        }

        if (decimal.TryParse(arguments[0].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out value1) &&
            decimal.TryParse(arguments[1].ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out value2))
        {
            return true;
        }

        return false;
    }
}