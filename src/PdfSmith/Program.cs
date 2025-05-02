using System.Dynamic;
using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PdfSmith.BusinessLayer.Extensions;
using PdfSmith.BusinessLayer.Generators;
using PdfSmith.BusinessLayer.Templating;
using PdfSmith.Shared.Models;
using SimpleAuthentication;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSimpleAuthentication(builder.Configuration);

builder.Services.AddKeyedSingleton<ITemplateEngine, ScribanTemplateEngine>("scriban");
//builder.Services.AddKeyedSingleton<ITemplateEngine, RazorTemplateEngine>("razor");

builder.Services.AddSingleton<IPdfGenerator, ChromiumPdfGenerator>();

builder.Services.AddRequestLocalization(options =>
{
    var supportedCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.DefaultRequestCulture = new RequestCulture("en-US");
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PdfGeneration", context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(context.User.Identity?.Name ?? "Default", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromSeconds(30),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var window))
        {
            var response = context.HttpContext.Response;
            response.Headers.RetryAfter = window.TotalSeconds.ToString();
        }

        return ValueTask.CompletedTask;
    };
});

builder.Services.AddRequestTimeouts();

builder.Services.AddOpenApi(options =>
{
    options.AddSimpleAuthentication(builder.Configuration);
    options.AddAcceptLanguageHeader();
});

builder.Services.AddDefaultProblemDetails();
builder.Services.AddDefaultExceptionHandler();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
});

app.UseRouting();
//app.UseCors();

app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();
app.UseRequestTimeouts();

app.MapPost("/api/pdf", async (PdfGenerationRequest request, IServiceProvider serviceProvider, IPdfGenerator pdfGenerator, HttpContext httpContext) =>
{
    var model = request.Model.ToExpandoObject();

    var templateEngine = serviceProvider.GetRequiredKeyedService<ITemplateEngine>(request.TemplateEngine);
    var result = await templateEngine.RenderAsync(request.Template, model, CultureInfo.CurrentCulture, httpContext.RequestAborted);

    var pdfStream = await pdfGenerator.CreateAsync(result, httpContext.RequestAborted);

    return TypedResults.Stream(pdfStream, contentType: MediaTypeNames.Application.Pdf, fileDownloadName: $"{Guid.CreateVersion7():N}.pdf");
})
.RequireAuthorization()
.RequireRateLimiting("PdfGeneration")
.WithRequestTimeout(new RequestTimeoutPolicy
{
    Timeout = TimeSpan.FromSeconds(30),
    TimeoutStatusCode = StatusCodes.Status408RequestTimeout
});

// On Windows, it is installed in %USERPROFILE%\AppData\Local\ms-playwright
Microsoft.Playwright.Program.Main(["install", "chromium"]);

app.Run();
