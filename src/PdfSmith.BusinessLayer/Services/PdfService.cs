﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
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
        var templateEngine = serviceProvider.GetKeyedService<ITemplateEngine>(request.TemplateEngine);

        if (templateEngine is null)
        {
            return Result.Fail(FailureReasons.ClientError, "Unable to render the template", $"The template engine {request.TemplateEngine} has not been registered");
        }

        string? content;
        try
        {
            var model = request.Model.ToExpandoObject();
            content = await templateEngine.RenderAsync(request.Template, model, CultureInfo.CurrentCulture, cancellationToken);
        }
        catch(TemplateEngineException ex)
        {
            return Result.Fail(FailureReasons.ClientError, "Unable to render the template", ex.Message);
        }

        var output = await pdfGenerator.CreateAsync(content, request.Options, cancellationToken);

        var fileName = request.FileName.HasValue() ? $"{Path.GetFileNameWithoutExtension(request.FileName)}.pdf" : $"{Guid.NewGuid():N}.pdf";
        var streamFileContent = new StreamFileContent(output, MediaTypeNames.Application.Pdf, fileName);

        return streamFileContent;
    }
}
