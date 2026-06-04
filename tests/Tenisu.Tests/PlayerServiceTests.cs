using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Tenisu.Application.DTOs;
using Tenisu.Application.Interfaces;
using Tenisu.Application.Services;
using Tenisu.Domain.Entities;
using Tenisu.Domain.Exceptions;
using ValidationException = Tenisu.Domain.Exceptions.ValidationException;

namespace Tenisu.Tests;

public class PlayerServiceTests
{
    private static IMemoryCache BuildCache() =>
        new MemoryCache(Options.Create(new MemoryCacheOptions()));

    private static List<Player> SamplePlayers() =>
    [
        new() { Id = 1, Firstname = "Rafael", Lastname = "Nadal", Sex = "M",
            Country = new() { Code = "ESP" }, Picture = "",
            Data = new() { Rank = 1, Points = 100, Weight = 85000, Height = 185, Age = 33, Last = [1, 0, 0, 0, 1] } },
        new() { Id = 2, Firstname = "Serena", Lastname = "Williams", Sex = "F",
            Country = new() { Code = "USA" }, Picture = "",
            Data = new() { Rank = 10, Points = 200, Weight = 72000, Height = 175, Age = 37, Last = [0, 1, 1, 1, 0] } },
        new() { Id = 3, Firstname = "Novak", Lastname = "Djokovic", Sex = "M",
            Country = new() { Code = "SRB" }, Picture = "",
            Data = new() { Rank = 2, Points = 150, Weight = 80000, Height = 188, Age = 31, Last = [1, 1, 1, 1, 1] } },
    ];

    [Fact]
    public async Task GetAllPlayersSortedByRank_ReturnsSortedList()
    {
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(SamplePlayers());
        var svc = new PlayerService(repo.Object, BuildCache());

        var result = await svc.GetAllSortedByRankAsync(null);

        Assert.Equal([1, 2, 10], result.Select(p => p.Data.Rank).ToList());
    }

    [Fact]
    public async Task GetPlayerById_ExistingId_ReturnsPlayer()
    {
        var players = SamplePlayers();
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(players[0]);
        var svc = new PlayerService(repo.Object, BuildCache());

        var result = await svc.GetByIdAsync(1);

        Assert.Equal(1, result.Id);
        Assert.Equal("Nadal", result.Lastname);
    }

    [Fact]
    public async Task GetPlayerById_UnknownId_ThrowsNotFoundException()
    {
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Player?)null);
        var svc = new PlayerService(repo.Object, BuildCache());

        await Assert.ThrowsAsync<NotFoundException>(() => svc.GetByIdAsync(999));
    }

    [Fact]
    public async Task AddPlayer_ValidPlayer_AddsToStore()
    {
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Player?)null);
        repo.Setup(r => r.AddAsync(It.IsAny<Player>(), default)).Returns(Task.CompletedTask);
        var svc = new PlayerService(repo.Object, BuildCache());

        var request = new CreatePlayerRequest
        {
            Id = 99, Firstname = "New", Lastname = "Player", Shortname = "N.PLA", Sex = "M",
            Country = new() { Code = "FRA" },
            Data = new() { Rank = 5, Points = 500, Weight = 75000, Height = 180, Age = 25, Last = [1, 1] }
        };

        var result = await svc.AddAsync(request);

        Assert.Equal(99, result.Id);
        repo.Verify(r => r.AddAsync(It.Is<Player>(p => p.Id == 99), default), Times.Once);
    }

    [Fact]
    public async Task AddPlayer_DuplicateId_ThrowsConflictException()
    {
        var players = SamplePlayers();
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(players[0]);
        var svc = new PlayerService(repo.Object, BuildCache());

        var request = new CreatePlayerRequest
        {
            Id = 1, Firstname = "X", Lastname = "X", Shortname = "X.X", Sex = "M",
            Country = new() { Code = "USA" },
            Data = new() { Rank = 1, Points = 100, Weight = 70000, Height = 180, Age = 30, Last = [] }
        };

        await Assert.ThrowsAsync<ConflictException>(() => svc.AddAsync(request));
    }

    [Fact]
    public async Task DeletePlayer_ExistingId_RemovesPlayer()
    {
        var players = SamplePlayers();
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(players[0]);
        repo.Setup(r => r.DeleteAsync(1, default)).Returns(Task.CompletedTask);
        var svc = new PlayerService(repo.Object, BuildCache());

        await svc.DeleteAsync(1);

        repo.Verify(r => r.DeleteAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task DeletePlayer_UnknownId_ThrowsNotFoundException()
    {
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetByIdAsync(999, default)).ReturnsAsync((Player?)null);
        var svc = new PlayerService(repo.Object, BuildCache());

        await Assert.ThrowsAsync<NotFoundException>(() => svc.DeleteAsync(999));
    }

    [Fact]
    public async Task GetPlayersFiltered_BySex_ReturnsCorrectSubset()
    {
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(SamplePlayers());
        var svc = new PlayerService(repo.Object, BuildCache());

        var males = await svc.GetAllSortedByRankAsync("M");
        var females = await svc.GetAllSortedByRankAsync("F");

        Assert.All(males, p => Assert.Equal("M", p.Sex));
        Assert.All(females, p => Assert.Equal("F", p.Sex));
        Assert.Equal(2, males.Count);
        Assert.Single(females);
    }

    [Fact]
    public async Task GetPlayersFiltered_InvalidSex_ThrowsValidationException()
    {
        var repo = new Mock<IPlayerRepository>();
        repo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(SamplePlayers());
        var svc = new PlayerService(repo.Object, BuildCache());

        await Assert.ThrowsAsync<ValidationException>(() => svc.GetAllSortedByRankAsync("X"));
    }
}
