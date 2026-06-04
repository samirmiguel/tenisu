using Microsoft.Extensions.Caching.Memory;
using Tenisu.Application.DTOs;
using Tenisu.Application.Interfaces;
using Tenisu.Domain.Entities;
using Tenisu.Domain.Exceptions;
using ValidationException = Tenisu.Domain.Exceptions.ValidationException;

namespace Tenisu.Application.Services;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _repo;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);
    private const string AllCacheKeyPrefix = "players_";
    private const string StatsCacheKey = "stats";

    public PlayerService(IPlayerRepository repo, IMemoryCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<IReadOnlyList<PlayerDto>> GetAllSortedByRankAsync(string? sex, CancellationToken ct = default)
    {
        if (sex is not null && sex != "M" && sex != "F")
            throw new ValidationException("The 'sex' parameter must be 'M' or 'F'.");

        var cacheKey = $"{AllCacheKeyPrefix}{sex ?? "all"}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<PlayerDto>? cached))
            return cached!;

        var players = await _repo.GetAllAsync(ct);
        var query = sex is not null ? players.Where(p => p.Sex == sex) : players;
        var result = query.OrderBy(p => p.Data.Rank).Select(MapToDto).ToList();

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<PlayerDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var player = await _repo.GetByIdAsync(id, ct);
        if (player is null)
            throw new NotFoundException($"Player with id {id} was not found.");
        return MapToDto(player);
    }

    public async Task<PlayerDto> AddAsync(CreatePlayerRequest request, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(request.Id, ct);
        if (existing is not null)
            throw new ConflictException($"A player with id {request.Id} already exists.");

        var player = new Player
        {
            Id = request.Id,
            Firstname = request.Firstname,
            Lastname = request.Lastname,
            Shortname = request.Shortname,
            Sex = request.Sex,
            Country = new Country { Code = request.Country.Code, Picture = request.Country.Picture },
            Picture = request.Picture,
            Data = new PlayerData
            {
                Rank = request.Data.Rank,
                Points = request.Data.Points,
                Weight = request.Data.Weight,
                Height = request.Data.Height,
                Age = request.Data.Age,
                Last = request.Data.Last
            }
        };

        await _repo.AddAsync(player, ct);
        InvalidateListCaches();
        return MapToDto(player);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null)
            throw new NotFoundException($"Player with id {id} was not found.");

        await _repo.DeleteAsync(id, ct);
        InvalidateListCaches();
    }

    private void InvalidateListCaches()
    {
        _cache.Remove($"{AllCacheKeyPrefix}all");
        _cache.Remove($"{AllCacheKeyPrefix}M");
        _cache.Remove($"{AllCacheKeyPrefix}F");
        _cache.Remove(StatsCacheKey);
    }

    internal static PlayerDto MapToDto(Player p) => new(
        p.Id,
        p.Firstname,
        p.Lastname,
        p.Shortname,
        p.Sex,
        new CountryDto(p.Country.Picture, p.Country.Code),
        p.Picture,
        new PlayerDataDto(p.Data.Rank, p.Data.Points, p.Data.Weight, p.Data.Height, p.Data.Age, p.Data.Last)
    );
}
