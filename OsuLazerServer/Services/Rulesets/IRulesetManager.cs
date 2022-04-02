using osu.Game.Rulesets;

namespace OsuLazerServer.Services.Rulesets;

public interface IRulesetManager
{
    public Dictionary<int, Ruleset> Rulesets { get; set; }

    public Task<Ruleset> LoadRuleset(string path);
    public Task<List<Ruleset>> LoadRulesets(string root);
    
    public Task CalculateIDs();
    
    public Ruleset GetRuleset(int id);
    
    public Task<List<Ruleset>> GetRulesets();
    
    public Task<Ruleset> GetRulesetByName(string name);

    public Task UpdateLegacy();
}