using NACE_Match_Builder.Models;

namespace NACE_Match_Builder.Builders;

public class ValorantMatchBuilder
{
    private Guid _id = Guid.NewGuid();
    private DateTimeOffset _time = DateTimeOffset.Now.AddDays(1);
    private int _bestOf = 3;
    private readonly List<Team> _teams = new();
    private readonly List<string> _maps = new();
    private ValorantRuleSet _rules = ValorantRuleSet.Default;

    public ValorantMatchBuilder WithScheduled(DateTimeOffset dto) { _time = dto; return this; }
    public ValorantMatchBuilder WithBestOf(int bestOf) { _bestOf = bestOf; return this; }
    public ValorantMatchBuilder AddTeam(Team t) { if (_teams.Count < 2) _teams.Add(t); return this; }
    public ValorantMatchBuilder WithMaps(IEnumerable<string> maps) { _maps.Clear(); _maps.AddRange(maps); return this; }
    public ValorantMatchBuilder WithRules(ValorantRuleSet rules) { _rules = rules; return this; }

    public ValorantMatch Build()
    {
        if (_teams.Count != 2) throw new InvalidOperationException("Exactly 2 teams required");
        if (_bestOf <= 0 || _bestOf % 2 == 0) throw new InvalidOperationException("BestOf must be an odd positive number");
        if (_maps.Count < _bestOf) throw new InvalidOperationException("Not enough maps selected");
        return new ValorantMatch(_id, _time, _bestOf, _teams.ToList(), _maps.Take(_bestOf).ToList(), _rules);
    }
}
