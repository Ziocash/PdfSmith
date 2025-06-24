using System.Globalization;
using System.Reflection;
using PdfSmith.BusinessLayer.Exceptions;
using RazorLight;
using RazorLight.Compilation;

namespace PdfSmith.BusinessLayer.Templating;

public class RazorTemplateEngine : ITemplateEngine
{
    private readonly RazorLightEngine engine;

    public RazorTemplateEngine()
    {
        var assembly = Assembly.GetExecutingAssembly();

        engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(assembly)
            .SetOperatingAssembly(assembly)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = $"""
                @using System
                @using System.Collections.Generic
                @using System.Linq
                {template}
                """;

            var result = await engine.CompileRenderStringAsync(template, content, model);
            return result;
        }
        catch (TemplateCompilationException ex)
        {
            throw new TemplateEngineException(ex.Message, ex);
        }
    }
}

