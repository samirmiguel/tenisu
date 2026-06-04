namespace Tenisu.Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Shortname { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public Country Country { get; set; } = new();
    public string Picture { get; set; } = string.Empty;
    public PlayerData Data { get; set; } = new();
}

public class Country
{
    public string Picture { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class PlayerData
{
    public int Rank { get; set; }
    public int Points { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public int Age { get; set; }
    public List<int> Last { get; set; } = [];
}
