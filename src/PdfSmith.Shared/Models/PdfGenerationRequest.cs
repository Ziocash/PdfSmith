using System.Text.Json;

namespace PdfSmith.Shared.Models;

public record class PdfGenerationRequest(string Template, JsonDocument Model, PdfOptions? Options = null, string TemplateEngine = "scriban", string? FileName = null);
