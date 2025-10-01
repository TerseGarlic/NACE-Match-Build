namespace NACE_Match_Builder.Models;

public enum GameTitle { CallOfDuty, Valorant }
public enum GameMode { Hardpoint, SearchAndDestroy, Control }

public abstract record MatchBase(
    Guid Id,
    GameTitle GameTitle,
    DateTimeOffset ScheduledTime,
    int BestOf,
    IReadOnlyList<Team> Teams);

public record Team(string Name, List<Player> Roster)
{
    public override string ToString() => Name;
}

public record Player(Guid Id, string Handle, string? InGameId, string? Role);

public record MapMode(string Map, string Mode)
{
    public override string ToString() => $"{Map} - {Mode}";
}

public sealed record CallOfDutyMatch(
    Guid Id,
    DateTimeOffset ScheduledTime,
    int BestOf,
    IReadOnlyList<Team> Teams,
    IReadOnlyList<MapMode> Rotation,
    CdlRuleSet Rules)
    : MatchBase(Id, GameTitle.CallOfDuty, ScheduledTime, BestOf, Teams);

public sealed record ValorantMatch(
    Guid Id,
    DateTimeOffset ScheduledTime,
    int BestOf,
    IReadOnlyList<Team> Teams,
    IReadOnlyList<string> Maps,
    ValorantRuleSet Rules)
    : MatchBase(Id, GameTitle.Valorant, ScheduledTime, BestOf, Teams);

public record CdlRuleSet(bool EnableGentleman, bool AllowSnipers)
{
    public static CdlRuleSet Default => new(true, true);
}

public record ValorantRuleSet(bool EnableTimeOuts, int MaxPauseMinutes)
{
    public static ValorantRuleSet Default => new(true, 10);
}
