using System.Globalization;
using System.Net.Mime;
using Microsoft.Extensions.DependencyInjection;
using OperationResults;
using PdfSmith.BusinessLayer.Exceptions;
using PdfSmith.BusinessLayer.Extensions;
using PdfSmith.BusinessLayer.Generators;
using PdfSmith.BusinessLayer.Services.Interfaces;
using PdfSmith.BusinessLayer.Templating;
using PdfSmith.Shared.Models;
using TinyHelpers.Extensions;

namespace PdfSmith.BusinessLayer.Services;

public class PdfService(IServiceProvider serviceProvider, IPdfGenerator pdfGenerator) : IPdfService
{
    public async Task<Result<StreamFileContent>> GeneratePdfAsync(PdfGenerationRequest request, CancellationToken cancellationToken)
    {
        string? content;
        if (request.Model is not null)
        {
            var templateEngine = serviceProvider.GetKeyedService<ITemplateEngine>(request.TemplateEngine!.ToLowerInvariant().Trim());

            if (templateEngine is null)
            {
                return Result.Fail(FailureReasons.ClientError, "Unable to render the template", $"The template engine '{request.TemplateEngine}' has not been registered");
            }

            try
            {
                var model = request.Model.ToExpandoObject();
                content = await templateEngine.RenderAsync(request.Template, model, CultureInfo.CurrentCulture, cancellationToken);
            }
            catch (TemplateEngineException ex)
            {
                return Result.Fail(FailureReasons.ClientError, "Unable to render the template", ex.Message);
            }
        }
        else
        {
            content = request.Template;
        }

        var output = await pdfGenerator.CreateAsync(content, request.Options, cancellationToken);

        var fileName = request.FileName.HasValue() ? $"{Path.GetFileNameWithoutExtension(request.FileName)}.pdf" : $"{Guid.NewGuid():N}.pdf";
        var streamFileContent = new StreamFileContent(output, MediaTypeNames.Application.Pdf, fileName);

        return streamFileContent;
    }
}
