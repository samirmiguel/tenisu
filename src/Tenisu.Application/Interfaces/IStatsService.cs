using Tenisu.Application.DTOs;

namespace Tenisu.Application.Interfaces;

public interface IStatsService
{
    Task<StatsDto> ComputeStatsAsync(CancellationToken ct = default);
}
