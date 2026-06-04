using Tenisu.Domain.Entities;

namespace Tenisu.Application.Interfaces;

public interface IPlayerRepository
{
    Task<IReadOnlyList<Player>> GetAllAsync(CancellationToken ct = default);
    Task<Player?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Player player, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    bool DataFileIsReadable();
}
