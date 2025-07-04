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

public class PdfService(IServiceProvider serviceProvider, IPdfGenerator pdfGenerator, ITimeZoneService timeZoneService) : IPdfService
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

            var timeZoneInfo = timeZoneService.GetTimeZone();

            if (timeZoneInfo is null)
            {
                var timeZoneId = timeZoneService.GetTimeZoneHeaderValue();
                if (timeZoneId is not null)
                {
                    // If timeZoneInfo is null, but timeZoneId has a value, it means that the time zone specified in the header is invalid.
                    return Result.Fail(FailureReasons.ClientError, "Unable to find the time zone", $"The time zone '{timeZoneId}' is invalid or is not available on the system");
                }
            }

            try
            {
                var model = request.Model.ToExpandoObject(timeZoneInfo);

                cancellationToken.ThrowIfCancellationRequested();

                content = await templateEngine.RenderAsync(request.Template, model, CultureInfo.CurrentCulture, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
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

        cancellationToken.ThrowIfCancellationRequested();

        var fileName = request.FileName.HasValue() ? $"{Path.GetFileNameWithoutExtension(request.FileName)}.pdf" : $"{Guid.NewGuid():N}.pdf";
        var streamFileContent = new StreamFileContent(output, MediaTypeNames.Application.Pdf, fileName);

        return streamFileContent;
    }
}
