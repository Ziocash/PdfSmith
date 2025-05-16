using Microsoft.Playwright;
using PdfSmith.Shared.Enums;
using PdfSmith.Shared.Models;
using TinyHelpers.Extensions;

namespace PdfSmith.BusinessLayer.Generators;

public class ChromiumPdfGenerator : IPdfGenerator
{
    public async Task<Stream> CreateAsync(string content, PdfOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true            
        });

        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // Set the content of the page to the rendered HTML.
        await page.SetContentAsync(content);

        // Generate the PDF as returns it as a stream.
        var pdfOptions = options ?? new(Margin: new());

        var output = await page.PdfAsync(new()
        {
            Format=pdfOptions.PageSize.GetValueOrDefault("A4"),
            Landscape = pdfOptions.Orientation == PdfOrientation.Landscape,
            PrintBackground = true,           
            Margin = pdfOptions.Margin is not null? new()
            {
                Top = $"{pdfOptions.Margin.Top}px",
                Bottom = $"{pdfOptions.Margin.Bottom}px",
                Left = $"{pdfOptions.Margin.Left}px",
                Right = $"{pdfOptions.Margin.Right}px",
            } : null
        });

        await context.CloseAsync();
        await browser.CloseAsync();

        var result = new MemoryStream(output);
        return result;
    }
}
