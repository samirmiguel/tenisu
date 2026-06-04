using System.Text.Json;
using System.Text.Json.Serialization;
using Tenisu.Application.Interfaces;
using Tenisu.Domain.Entities;

namespace Tenisu.Infrastructure.Repositories;

public sealed class JsonPlayerRepository : IPlayerRepository
{
    private readonly List<Player> _players;
    private readonly string _dataFilePath;
    private readonly Lock _lock = new();

    public JsonPlayerRepository(string dataFilePath)
    {
        _dataFilePath = dataFilePath;
        var json = File.ReadAllText(dataFilePath);
        var doc = JsonSerializer.Deserialize<PlayersDocument>(json, JsonOptions)
                  ?? throw new InvalidOperationException("Failed to deserialize players data.");
        _players = doc.Players;
    }

    public Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<Player>>([.. _players]);
        }
    }

    public Task<Player?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_players.FirstOrDefault(p => p.Id == id));
        }
    }

    public Task AddAsync(Player player, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _players.Add(player);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var player = _players.FirstOrDefault(p => p.Id == id);
            if (player is not null)
                _players.Remove(player);
        }
        return Task.CompletedTask;
    }

    public bool DataFileIsReadable()
    {
        try
        {
            _ = File.ReadAllText(_dataFilePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private sealed class PlayersDocument
    {
        public List<Player> Players { get; set; } = [];
    }
}
