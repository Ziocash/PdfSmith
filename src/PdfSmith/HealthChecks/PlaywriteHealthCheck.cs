using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Playwright;

namespace PdfSmith.HealthChecks;

public class PlaywriteHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = true
            });
        }
        catch (PlaywrightException)
        {
            return HealthCheckResult.Unhealthy("Playwright isn't ready yet.");
        }

        return HealthCheckResult.Healthy();
    }
}
