﻿using PdfSmith.HealthChecks;

namespace PdfSmith.BackgroundServices;

public class InstallPlaywrightBackgroundService(PlaywrightHealthCheck playwrightHealthCheck, ILogger<InstallPlaywrightBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // On Windows, it is installed in %USERPROFILE%\AppData\Local\ms-playwright by default.
        // We can use PLAYWRIGHT_BROWSERS_PATH environment variable to change the default location.
        var returnCode = await Task.Run(() =>
        {
            try
            {
                return Microsoft.Playwright.Program.Main(["install", "chromium"]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while installing Chromium");
                return -1;
            }
        }, stoppingToken);

        var playwrightStatus = returnCode switch
        {
            0 => PlaywrightStatus.Installed,
            _ => PlaywrightStatus.Error
        };

        playwrightHealthCheck.Status = playwrightStatus;
    }
}