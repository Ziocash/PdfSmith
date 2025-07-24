using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PdfSmith.HealthChecks;

public class PlaywrightHealthCheck : IHealthCheck
{
    public bool IsPlaywrightReady { get; set; } = false;
    public string? ErrorMessage { get; set; }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (IsPlaywrightReady)
        {
            return Task.FromResult(HealthCheckResult.Healthy());
        }

        var unhealthyMessage = ErrorMessage ?? "Playwright isn't ready yet.";
        return Task.FromResult(HealthCheckResult.Unhealthy(unhealthyMessage));
    }
}
