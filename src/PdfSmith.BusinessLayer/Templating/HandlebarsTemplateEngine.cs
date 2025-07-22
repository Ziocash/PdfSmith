using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using PdfSmith.BusinessLayer.Exceptions;
using PdfSmith.BusinessLayer.Services;

namespace PdfSmith.BusinessLayer.Templating;

public partial class HandlebarsTemplateEngine(TimeZoneTimeProvider timeZoneTimeProvider) : ITemplateEngine
{
    private readonly IHandlebars handlebars = Handlebars.Create();
    private static readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> TemplateCache = new();
    private static TimeZoneTimeProvider? timeProvider;

    static HandlebarsTemplateEngine()
    {
        RegisterGlobalHelpers();
    }

    public async Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        try
        {
            // Set the time provider for helpers
            timeProvider = timeZoneTimeProvider;

            // Replace DateTime.Now with timezone-aware helper placeholders like other engines
            var sanitizedTemplate = DateTimeNowRegex.Replace(template, "{{dateTimeNow}}");
            sanitizedTemplate = DateTimeOffsetNowRegex.Replace(sanitizedTemplate, "{{dateTimeOffsetNow}}");

            // Get or compile template (with caching for performance)
            var compiledTemplate = TemplateCache.GetOrAdd(sanitizedTemplate, key =>
                handlebars.Compile(key)
            );

            cancellationToken.ThrowIfCancellationRequested();

            // Pass the model directly (Handlebars expects the root model, not wrapped)
            var result = await Task.Run(() => compiledTemplate(model), cancellationToken);
            return result;
        }
        catch (HandlebarsException ex)
        {
            throw new TemplateEngineException(ex.Message, ex);
        }
        catch (Exception ex) when (ex is not TemplateEngineException)
        {
            throw new TemplateEngineException($"An error occurred while rendering the Handlebars template: {ex.Message}", ex);
        }
    }

    private static void RegisterGlobalHelpers()
    {
        var handlebarsInstance = Handlebars.Create();

        // Register helper for formatting currency
        Handlebars.RegisterHelper("formatCurrency", (context, arguments) =>
        {
            if (arguments.Length > 0 && decimal.TryParse(arguments[0].ToString(), out var value))
            {
                // Use the current culture for formatting
                return value.ToString("C", CultureInfo.CurrentCulture);
            }
            return arguments.FirstOrDefault()?.ToString() ?? string.Empty;
        });

        // Register helper for formatting dates
        Handlebars.RegisterHelper("formatDate", (context, arguments) =>
        {
            if (arguments.Length > 0)
            {
                var dateValue = arguments[0];
                var format = arguments.Length > 1 ? arguments[1].ToString() : "yyyy-MM-dd";
                
                if (dateValue is DateTime dateTime)
                {
                    return dateTime.ToString(format, CultureInfo.CurrentCulture);
                }
                if (dateValue is DateTimeOffset dateTimeOffset)
                {
                    return dateTimeOffset.ToString(format, CultureInfo.CurrentCulture);
                }
                if (DateTime.TryParse(dateValue.ToString(), out var parsedDate))
                {
                    return parsedDate.ToString(format, CultureInfo.CurrentCulture);
                }
            }
            return arguments.FirstOrDefault()?.ToString() ?? string.Empty;
        });

        // Register helper for accessing DateTime.Now with timezone support
        Handlebars.RegisterHelper("dateTimeNow", (context, arguments) =>
        {
            return timeProvider?.GetLocalNow().DateTime ?? DateTime.Now;
        });

        // Register helper for accessing DateTimeOffset.Now with timezone support
        Handlebars.RegisterHelper("dateTimeOffsetNow", (context, arguments) =>
        {
            return timeProvider?.GetLocalNow() ?? DateTimeOffset.Now;
        });

        // Register helper for mathematical operations
        Handlebars.RegisterHelper("multiply", (context, arguments) =>
        {
            if (arguments.Length >= 2 && 
                decimal.TryParse(arguments[0].ToString(), out var value1) &&
                decimal.TryParse(arguments[1].ToString(), out var value2))
            {
                return value1 * value2;
            }
            return 0m;
        });
    }

    [GeneratedRegex(@"(?<![\\w$])(?:@)?(?:System\.)?DateTime\.Now(?![\\w$])")]
    private static partial Regex DateTimeNowRegex { get; }

    [GeneratedRegex(@"(?<![\\w$])(?:@)?(?:System\.)?DateTimeOffset\.Now(?![\\w$])")]
    private static partial Regex DateTimeOffsetNowRegex { get; }
}