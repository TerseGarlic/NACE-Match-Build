using NACE_Match_Builder.Models;

namespace NACE_Match_Builder.Builders;

public class CallOfDutyMatchBuilder
{
    private Guid _id = Guid.NewGuid();
    private DateTimeOffset _time = DateTimeOffset.Now.AddDays(1);
    private int _bestOf = 5;
    private readonly List<Team> _teams = new();
    private readonly List<MapMode> _rotation = new();
    private CdlRuleSet _rules = CdlRuleSet.Default;

    public CallOfDutyMatchBuilder WithScheduled(DateTimeOffset dto) { _time = dto; return this; }
    public CallOfDutyMatchBuilder WithBestOf(int bestOf) { _bestOf = bestOf; return this; }
    public CallOfDutyMatchBuilder AddTeam(Team t) { if (_teams.Count < 2) _teams.Add(t); return this; }
    public CallOfDutyMatchBuilder WithRotation(IEnumerable<MapMode> mm) { _rotation.Clear(); _rotation.AddRange(mm); return this; }
    public CallOfDutyMatchBuilder WithRules(CdlRuleSet rules) { _rules = rules; return this; }

    public CallOfDutyMatch Build()
    {
        if (_teams.Count != 2) throw new InvalidOperationException("Exactly 2 teams required");
        if (_bestOf <= 0 || _bestOf % 2 == 0) throw new InvalidOperationException("BestOf must be an odd positive number");
        if (_rotation.Count < _bestOf) throw new InvalidOperationException("Rotation smaller than BestOf");
        return new CallOfDutyMatch(_id, _time, _bestOf, _teams.ToList(), _rotation.Take(_bestOf).ToList(), _rules);
    }
}
