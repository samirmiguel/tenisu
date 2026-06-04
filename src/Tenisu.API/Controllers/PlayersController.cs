using Microsoft.AspNetCore.Mvc;
using Tenisu.Application.DTOs;
using Tenisu.Application.Interfaces;

namespace Tenisu.API.Controllers;

/// <summary>Tennis players API.</summary>
[ApiController]
[Route("api/players")]
[Produces("application/json")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IStatsService _statsService;

    public PlayersController(IPlayerService playerService, IStatsService statsService)
    {
        _playerService = playerService;
        _statsService = statsService;
    }

    /// <summary>Returns all players sorted by rank (best first).</summary>
    /// <param name="sex">Optional gender filter: M or F.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Sorted list of players.</response>
    /// <response code="400">Invalid value for the sex parameter.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] string? sex, CancellationToken ct)
    {
        var players = await _playerService.GetAllSortedByRankAsync(sex, ct);
        return Ok(players);
    }

    /// <summary>Returns aggregate statistics across all players.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Computed statistics.</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(StatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _statsService.ComputeStatsAsync(ct);
        return Ok(stats);
    }

    /// <summary>Returns a single player by ID.</summary>
    /// <param name="id">Player identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">The requested player.</response>
    /// <response code="404">Player not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var player = await _playerService.GetByIdAsync(id, ct);
        return Ok(player);
    }

    /// <summary>Adds a new player to the in-memory store.</summary>
    /// <param name="request">Player data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Player created.</response>
    /// <response code="400">Validation errors.</response>
    /// <response code="409">A player with the same ID already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePlayerRequest request, CancellationToken ct)
    {
        var created = await _playerService.AddAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Removes a player from the in-memory store.</summary>
    /// <param name="id">Player identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Player deleted.</response>
    /// <response code="404">Player not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _playerService.DeleteAsync(id, ct);
        return NoContent();
    }
}
