using Tenisu.Application.DTOs;

namespace Tenisu.Application.Interfaces;

public interface IPlayerService
{
    Task<IReadOnlyList<PlayerDto>> GetAllSortedByRankAsync(string? sex, CancellationToken ct = default);
    Task<PlayerDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PlayerDto> AddAsync(CreatePlayerRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
