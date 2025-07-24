using Microsoft.Playwright;
using PdfSmith.HealthChecks;

namespace PdfSmith.BackgroundServices;

public class PlaywrightBackgroundService(ILogger<PlaywrightBackgroundService> logger, PlaywrightHealthCheck playwrightHealthCheck) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // On Windows, it is installed in %USERPROFILE%\AppData\Local\ms-playwright by default
        // We can use PLAYWRIGHT_BROWSERS_PATH environment variable to change the default location
        var (returnCode, errorMessage) = await Task.Run(() =>
        {
            try
            {
                return (Microsoft.Playwright.Program.Main(["install", "chromium"]), (string?)null);
            }
            catch (PlaywrightException playwrightException)
            {
                logger.LogError(playwrightException, "Error while installing chromium");
                return (1, "Error while initializing Playwright");
            }
        });

        if (returnCode != 0)
        {
            playwrightHealthCheck.ErrorMessage = errorMessage;
            return;
        }

        playwrightHealthCheck.IsPlaywrightReady = true;
    }
}
