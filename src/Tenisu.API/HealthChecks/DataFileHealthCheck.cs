using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tenisu.Application.Interfaces;

namespace Tenisu.API.HealthChecks;

public class DataFileHealthCheck : IHealthCheck
{
    private readonly IPlayerRepository _repo;

    public DataFileHealthCheck(IPlayerRepository repo) => _repo = repo;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        var result = _repo.DataFileIsReadable()
            ? HealthCheckResult.Healthy("Data file is accessible.")
            : HealthCheckResult.Unhealthy("Data file cannot be read.");
        return Task.FromResult(result);
    }
}
