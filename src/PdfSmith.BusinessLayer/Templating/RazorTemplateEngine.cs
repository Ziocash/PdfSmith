using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RazorLight;

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
        var result = await engine.CompileRenderStringAsync(template, template, model);
        return result;
    }
}

