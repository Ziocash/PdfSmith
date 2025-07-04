using System.Globalization;
using System.Text.RegularExpressions;
using PdfSmith.BusinessLayer.Exceptions;
using RazorLight;
using RazorLight.Compilation;

namespace PdfSmith.BusinessLayer.Templating;

public partial class RazorTemplateEngine(IRazorLightEngine engine) : ITemplateEngine
{
    public async Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        try
        {
            var sanitizedTemplate = DateTimeNowRegex.Replace(template, "@timeZoneTimeProvider.GetLocalNow().DateTime");
            sanitizedTemplate = DateTimeOffsetNowRegex.Replace(sanitizedTemplate, "@timeZoneTimeProvider.GetLocalNow()");

            var content = $"""
                @using System
                @using System.Collections.Generic
                @using System.Linq

                @inject PdfSmith.BusinessLayer.Services.TimeZoneTimeProvider timeZoneTimeProvider

                {sanitizedTemplate}
                """;

            var result = await engine.CompileRenderStringAsync(sanitizedTemplate, content, model);
            return result;
        }
        catch (TemplateCompilationException ex)
        {
            throw new TemplateEngineException(ex.Message, ex);
        }
    }

    [GeneratedRegex("(?<![\\w$])(?:@)?(?:System\\.)?DateTime\\.Now(?![\\w$])")]
    private static partial Regex DateTimeNowRegex { get; }

    [GeneratedRegex("(?<![\\w$])(?:@)?(?:System\\.)?DateTimeOffset\\.Now(?![\\w$])")]
    private static partial Regex DateTimeOffsetNowRegex { get; }
}

