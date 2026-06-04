using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Tenisu.Application.Interfaces;
using Tenisu.Application.Services;
using Tenisu.Domain.Entities;

namespace Tenisu.Tests;

public class StatsServiceTests
{
    private static IMemoryCache BuildCache() =>
        new MemoryCache(Options.Create(new MemoryCacheOptions()));

    private static List<Player> FullDataset() =>
    [
        new() { Id = 52, Firstname = "Novak", Lastname = "Djokovic", Sex = "M",
            Country = new() { Code = "SRB" }, Picture = "",
            Data = new() { Rank = 2, Points = 2542, Weight = 80000, Height = 188, Age = 31, Last = [1,1,1,1,1] } },
        new() { Id = 95, Firstname = "Venus", Lastname = "Williams", Sex = "F",
            Country = new() { Code = "USA" }, Picture = "",
            Data = new() { Rank = 52, Points = 1105, Weight = 74000, Height = 185, Age = 38, Last = [0,1,0,0,1] } },
        new() { Id = 65, Firstname = "Stan", Lastname = "Wawrinka", Sex = "M",
            Country = new() { Code = "SUI" }, Picture = "",
            Data = new() { Rank = 21, Points = 1784, Weight = 81000, Height = 183, Age = 33, Last = [1,1,1,0,1] } },
        new() { Id = 102, Firstname = "Serena", Lastname = "Williams", Sex = "F",
            Country = new() { Code = "USA" }, Picture = "",
            Data = new() { Rank = 10, Points = 3521, Weight = 72000, Height = 175, Age = 37, Last = [0,1,1,1,0] } },
        new() { Id = 17, Firstname = "Rafael", Lastname = "Nadal", Sex = "M",
            Country = new() { Code = "ESP" }, Picture = "",
            Data = new() { Rank = 1, Points = 1982, Weight = 85000, Height = 185, Age = 33, Last = [1,0,0,0,1] } },
    ];

    [Fact]
    public async Task ComputeStats_ReturnsBestCountrySRB()
    {
        // SRB (Djokovic): 5/5 = 100% win ratio — highest in the dataset
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(FullDataset());
        var svc = new StatsService(repo.Object, BuildCache());

        var stats = await svc.ComputeStatsAsync();

        Assert.Equal("SRB", stats.BestCountry);
    }

    [Fact]
    public async Task ComputeStats_ReturnsCorrectAverageBmi()
    {
        // Pre-computed:
        // Djokovic:  80 / 1.88² = 22.64
        // Venus:     74 / 1.85² = 21.62
        // Wawrinka:  81 / 1.83² = 24.19
        // Serena:    72 / 1.75² = 23.51
        // Nadal:     85 / 1.85² = 24.84
        // Average ≈ 23.36
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(FullDataset());
        var svc = new StatsService(repo.Object, BuildCache());

        var stats = await svc.ComputeStatsAsync();

        Assert.Equal(23.36, stats.AverageBmi);
    }

    [Fact]
    public async Task ComputeStats_ReturnsCorrectMedianHeight()
    {
        // Heights sorted: [175, 183, 185, 185, 188] — n=5, median = heights[2] = 185
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(FullDataset());
        var svc = new StatsService(repo.Object, BuildCache());

        var stats = await svc.ComputeStatsAsync();

        Assert.Equal(185.0, stats.MedianHeight);
    }

    [Fact]
    public async Task MedianHeight_EvenNumberOfPlayers_ReturnsAverageOfMiddleTwo()
    {
        // 4 players with heights [175, 183, 185, 188]
        // sorted: [175, 183, 185, 188], n=4, median = (183 + 185) / 2 = 184
        var players = FullDataset().Take(4).ToList();
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(players);
        var svc = new StatsService(repo.Object, BuildCache());

        var stats = await svc.ComputeStatsAsync();

        Assert.Equal(184.0, stats.MedianHeight);
    }
}
