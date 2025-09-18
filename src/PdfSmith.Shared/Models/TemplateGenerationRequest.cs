using System.Text.Json;
using System.Text.Json.Serialization;

namespace PdfSmith.Shared.Models;

[method: JsonConstructor]
public record class TemplateGenerationRequest(string Template, JsonDocument? Model, string? TemplateEngine = null)
{
    public TemplateGenerationRequest(string template, object? model, string? templateEngine = null)
        : this(template, ToJsonDocument(model), templateEngine)
    {
    }

    protected static JsonDocument? ToJsonDocument(object? model)
    {
        if (model is null)
        {
            return null;
        }

        var jsonString = JsonSerializer.Serialize(model, JsonSerializerOptions.Default);
        return JsonDocument.Parse(jsonString);
    }
}
