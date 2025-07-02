using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using PdfSmith.BusinessLayer.Exceptions;
using RazorLight;
using RazorLight.Compilation;

namespace PdfSmith.BusinessLayer.Templating;

public partial class RazorTemplateEngine : ITemplateEngine
{
    private readonly RazorLightEngine engine;
    private readonly TimeProvider timeProvider;

    public RazorTemplateEngine(TimeProvider timeProvider)
    {
        var assembly = Assembly.GetExecutingAssembly();

        engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(assembly)
            .SetOperatingAssembly(assembly)
            .UseMemoryCachingProvider()
            .Build();

        this.timeProvider = timeProvider;
    }

    public async Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        try
        {
            var sanitizedTemplate = DateTimeNowRegex.Replace(template, "@ViewBag.TimeProvider.GetLocalNow()");

            var content = $"""
                @using System
                @using System.Collections.Generic
                @using System.Linq

                {sanitizedTemplate}
                """;

            dynamic modelWithServices = new ExpandoObject();
            modelWithServices.TimeProvider = timeProvider;

            var result = await engine.CompileRenderStringAsync(sanitizedTemplate, content, model, modelWithServices);
            return result;
        }
        catch (TemplateCompilationException ex)
        {
            throw new TemplateEngineException(ex.Message, ex);
        }
    }

    [GeneratedRegex("(?<![\\w$])@DateTime\\.Now(?![\\w$])")]
    private static partial Regex DateTimeNowRegex { get; }
}

