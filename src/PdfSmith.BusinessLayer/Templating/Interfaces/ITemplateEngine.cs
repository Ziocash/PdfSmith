using System.Globalization;

namespace PdfSmith.BusinessLayer.Templating.Interfaces;

public interface ITemplateEngine
{
    Task<string> RenderAsync(string template, object model, CultureInfo culture, CancellationToken cancellationToken = default);
}
