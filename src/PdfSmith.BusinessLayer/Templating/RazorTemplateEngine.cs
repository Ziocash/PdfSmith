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
        engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly())
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await engine.CompileRenderStringAsync(template, template, model);
            return result;
        }
        catch (TemplateCompilationException ex)
        {
            throw new TemplateEngineException(ex.Message);
        }
    }
}

