using Microsoft.Extensions.Caching.Memory;
using Tenisu.Application.DTOs;
using Tenisu.Application.Interfaces;

namespace Tenisu.Application.Services;

public class StatsService : IStatsService
{
    private readonly IPlayerRepository _repo;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);
    private const string StatsCacheKey = "stats";

    public StatsService(IPlayerRepository repo, IMemoryCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<StatsDto> ComputeStatsAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(StatsCacheKey, out StatsDto? cached))
            return cached!;

        var players = await _repo.GetAllAsync(ct);

        var bestCountry = players
            .GroupBy(p => p.Country.Code)
            .Select(g => new
            {
                Code = g.Key,
                WinRatio = g.SelectMany(p => p.Data.Last).DefaultIfEmpty(0).Average()
            })
            .OrderByDescending(x => x.WinRatio)
            .First().Code;

        var avgBmi = Math.Round(
            players.Average(p =>
            {
                double weightKg = p.Data.Weight / 1000.0;
                double heightM = p.Data.Height / 100.0;
                return weightKg / (heightM * heightM);
            }), 2);

        var heights = players.Select(p => (double)p.Data.Height).OrderBy(h => h).ToList();
        var n = heights.Count;
        double medianHeight = n % 2 == 1
            ? heights[n / 2]
            : (heights[n / 2 - 1] + heights[n / 2]) / 2.0;

        var result = new StatsDto(bestCountry, avgBmi, medianHeight);
        _cache.Set(StatsCacheKey, result, CacheDuration);
        return result;
    }
}
