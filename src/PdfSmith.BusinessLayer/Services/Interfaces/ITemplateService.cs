using OperationResults;
using PdfSmith.Shared.Models;

namespace PdfSmith.BusinessLayer.Services.Interfaces;

public interface ITemplateService
{
    Task<Result<TemplateResponse>> CreateAsync(TemplateGenerationRequest request, CancellationToken cancellationToken);
}