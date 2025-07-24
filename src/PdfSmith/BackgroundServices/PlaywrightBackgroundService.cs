
using PdfSmith.HealthChecks;

namespace PdfSmith.BackgroundServices;

public class PlaywrightBackgroundService(PlaywrightHealthCheck playwrightHealthCheck) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // On Windows, it is installed in %USERPROFILE%\AppData\Local\ms-playwright by default
        // We can use PLAYWRIGHT_BROWSERS_PATH environment variable to change the default location
        await Task.Run(() => Microsoft.Playwright.Program.Main(["install", "chromium"]));

        playwrightHealthCheck.IsPlaywrightReady = true;
    }
}
