namespace Tenisu.Application.DTOs;

public record PlayerDto(
    int Id,
    string Firstname,
    string Lastname,
    string Shortname,
    string Sex,
    CountryDto Country,
    string Picture,
    PlayerDataDto Data
);

public record CountryDto(string Picture, string Code);

public record PlayerDataDto(int Rank, int Points, int Weight, int Height, int Age, List<int> Last);
