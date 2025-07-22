using System.Collections.Concurrent;
using System.Globalization;
using HandlebarsDotNet;
using PdfSmith.BusinessLayer.Exceptions;
using PdfSmith.BusinessLayer.Services;

namespace PdfSmith.BusinessLayer.Templating;

public class HandlebarsTemplateEngine(TimeZoneTimeProvider timeZoneTimeProvider) : ITemplateEngine
{
    private readonly IHandlebars handlebars = Handlebars.Create();
    private static readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> TemplateCache = new();

    static HandlebarsTemplateEngine()
    {
        RegisterGlobalHelpers();
    }

    public async Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get or compile template (with caching for performance)
            var compiledTemplate = TemplateCache.GetOrAdd(template, key =>
                handlebars.Compile(key)
            );

            cancellationToken.ThrowIfCancellationRequested();

            // Pass the model directly (Handlebars expects the root model, not wrapped)
            var result = compiledTemplate(model);
            return await Task.FromResult(result);
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
            if (arguments.Length > 0 && decimal.TryParse(arguments[0].ToString(), CultureInfo.CurrentCulture, out var value))
            {
                // Use the current culture for formatting
                return value.ToString("C", CultureInfo.CurrentCulture);
            }
            return arguments.FirstOrDefault()?.ToString() ?? string.Empty;
        });

        // Register helper for formatting dates
        Handlebars.RegisterHelper("formatDate", (context, arguments) =>
        {
            if (arguments.Length == 0)
            {
                return arguments.FirstOrDefault()?.ToString() ?? string.Empty;
            }

            var dateValue = arguments[0];
            var format = arguments.Length > 1 ? arguments[1].ToString() : 
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
    }
}