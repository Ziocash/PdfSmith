using FluentValidation;
using PdfSmith.Shared.Models;

namespace PdfSmith.BusinessLayer.Validations;

public class TemplateGenerationRequestValidator : AbstractValidator<TemplateGenerationRequest>
{
    public TemplateGenerationRequestValidator()
    {
        RuleFor(r => r.Template).NotEmpty();
        RuleFor(r => r.TemplateEngine).NotEmpty().When(r => r.Model is not null);
    }
}
