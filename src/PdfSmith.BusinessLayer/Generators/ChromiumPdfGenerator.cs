using Microsoft.Playwright;

namespace PdfSmith.BusinessLayer.Generators;

public class ChromiumPdfGenerator : IPdfGenerator
{
    public async Task<Stream> CreateAsync(string content, CancellationToken cancellationToken = default)
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

        // Generate the PDF as returns it as a byte array.
        var output = await page.PdfAsync();

        await context.CloseAsync();
        await browser.CloseAsync();

        var result = new MemoryStream(output);
        return result;
    }
}
