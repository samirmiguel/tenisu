using System.ComponentModel.DataAnnotations;
using Tenisu.Application.Validation;

namespace Tenisu.Application.DTOs;

public record CreatePlayerRequest
{
    [Required]
    public int Id { get; init; }

    [Required, MinLength(1)]
    public string Firstname { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Lastname { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Shortname { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^[MF]$", ErrorMessage = "Sex must be 'M' or 'F'.")]
    public string Sex { get; init; } = string.Empty;

    [Required]
    public CreateCountryRequest Country { get; init; } = new();

    public string Picture { get; init; } = string.Empty;

    [Required]
    public CreatePlayerDataRequest Data { get; init; } = new();
}

public record CreateCountryRequest
{
    [Required]
    [RegularExpression("^[A-Z]{2,3}$", ErrorMessage = "Country code must be 2–3 uppercase letters.")]
    public string Code { get; init; } = string.Empty;

    public string Picture { get; init; } = string.Empty;
}

public record CreatePlayerDataRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Rank must be a positive integer.")]
    public int Rank { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Points must be a positive integer.")]
    public int Points { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Weight must be a positive integer (grams).")]
    public int Weight { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Height must be a positive integer (cm).")]
    public int Height { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Age must be a positive integer.")]
    public int Age { get; init; }

    [LastArray]
    public List<int> Last { get; init; } = [];
}
