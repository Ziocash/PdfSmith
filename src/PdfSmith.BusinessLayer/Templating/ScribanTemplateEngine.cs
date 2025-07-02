using System.Globalization;
using System.Text.RegularExpressions;
using PdfSmith.BusinessLayer.Exceptions;
using Scriban;
using Scriban.Runtime;

namespace PdfSmith.BusinessLayer.Templating;

public partial class ScribanTemplateEngine(TimeProvider timeProvider) : ITemplateEngine
{
    private static readonly string DateTimeZonePlaceholder = "date_time_zone";

    public async Task<string> RenderAsync(string text, object model, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        var sanitizedText = DateNowRegex.Replace(text, DateTimeZonePlaceholder);

        var template = Template.Parse(sanitizedText);
        if (template.HasErrors)
        {
            throw new TemplateEngineException(template.Messages.ToString());
        }

        var context = new TemplateContext { MemberRenamer = member => member.Name };
        context.PushGlobal(new ScriptObject { { "Model", model } });
        context.PushCulture(culture);

        var dateWithTimeZoneScript = new ScriptObject();
        dateWithTimeZoneScript.Import(DateTimeZonePlaceholder, new Func<DateTime>(() => timeProvider.GetLocalNow().DateTime));
        context.PushGlobal(dateWithTimeZoneScript);

        var result = await template.RenderAsync(context);
        return result;
    }

    [GeneratedRegex("(?<![\\w$])date\\.now(?![\\w$])")]
    private static partial Regex DateNowRegex { get; }
}