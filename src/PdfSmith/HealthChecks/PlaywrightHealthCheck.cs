using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PdfSmith.HealthChecks;

public class PlaywrightHealthCheck : IHealthCheck
{
    public bool IsPlaywrightReady { get; set; } = false;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (IsPlaywrightReady)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Playwright isn't ready yet."));
    }
}
