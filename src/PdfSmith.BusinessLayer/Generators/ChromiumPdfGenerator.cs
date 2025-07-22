using Microsoft.Playwright;
using PdfSmith.BusinessLayer.Generators.Interfaces;
using PdfSmith.Shared.Enums;
using PdfSmith.Shared.Models;
using TinyHelpers.Extensions;

namespace PdfSmith.BusinessLayer.Generators;

public class ChromiumPdfGenerator : IPdfGenerator
{
    public async Task<Stream> CreateAsync(string content, PdfOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true            
        });

        cancellationToken.ThrowIfCancellationRequested();

        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // Set the content of the page to the rendered HTML.
        await page.SetContentAsync(content);

        // Generate the PDF as returns it as a stream.
        var pdfOptions = options ?? new(Margin: new());

        cancellationToken.ThrowIfCancellationRequested();

        var output = await page.PdfAsync(new()
        {
            Format = pdfOptions.PageSize.GetValueOrDefault("A4"),
            Landscape = pdfOptions.Orientation == PdfOrientation.Landscape,
            PrintBackground = true,
            Margin = pdfOptions.Margin is not null ? new()
            {
                Top = pdfOptions.Margin.Top.GetValueOrDefault("2.5cm").Replace(",", "."),
                Bottom = pdfOptions.Margin.Bottom.GetValueOrDefault("2cm").Replace(",", "."),
                Left = pdfOptions.Margin.Left.GetValueOrDefault("2cm").Replace(",", "."),
                Right = pdfOptions.Margin.Right.GetValueOrDefault("2cm").Replace(",", "."),
            } : null
        });

        cancellationToken.ThrowIfCancellationRequested();

        await context.CloseAsync();
        await browser.CloseAsync();

        var result = new MemoryStream(output);
        return result;
    }
}
