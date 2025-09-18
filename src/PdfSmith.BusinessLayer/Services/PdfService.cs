using System.Net.Mime;
using OperationResults;
using PdfSmith.BusinessLayer.Generators.Interfaces;
using PdfSmith.BusinessLayer.Services.Interfaces;
using PdfSmith.Shared.Models;
using TinyHelpers.Extensions;

namespace PdfSmith.BusinessLayer.Services;

public class PdfService(ITemplateService templateService, IPdfGenerator pdfGenerator) : IPdfService
{
    public async Task<Result<StreamFileContent>> GeneratePdfAsync(PdfGenerationRequest request, CancellationToken cancellationToken)
    {
        var templateResponse = await templateService.CreateAsync(request, cancellationToken);

        if (!templateResponse.Success)
        {
            return Result.Fail(templateResponse.FailureReason, templateResponse.ErrorMessage!, templateResponse.ErrorDetail!, templateResponse.ValidationErrors);
        }

        var output = await pdfGenerator.CreateAsync(templateResponse.Content.Result, request.Options, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var fileName = request.FileName.HasValue() ? $"{Path.GetFileNameWithoutExtension(request.FileName)}.pdf" : $"{Guid.NewGuid():N}.pdf";
        var streamFileContent = new StreamFileContent(output, MediaTypeNames.Application.Pdf, fileName);

        return streamFileContent;
    }
}
