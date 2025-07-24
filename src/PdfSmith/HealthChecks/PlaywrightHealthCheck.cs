using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PdfSmith.HealthChecks;

public class PlaywrightHealthCheck : IHealthCheck
{
    public PlaywrightStatus Status { get; set; } = PlaywrightStatus.Unknown;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => Status switch
        {
            PlaywrightStatus.Unknown => Task.FromResult(HealthCheckResult.Unhealthy("Playwright isn't ready yet.")),
            PlaywrightStatus.Error => Task.FromResult(HealthCheckResult.Unhealthy("Error during Playwright installation.")),
            PlaywrightStatus.Installed => Task.FromResult(HealthCheckResult.Healthy()),
            _ => throw new NotImplementedException()
        };
}
