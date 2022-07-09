using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using osu.Framework.Extensions;
using osu.Game.Rulesets;

namespace OsuLazerServer.Services.Rulesets;

public class RulesetManager : IRulesetManager
{
    public Dictionary<int, Ruleset> Rulesets { get; set; } = new();

    private List<Ruleset> _rulesets = new();
    public async Task<Ruleset> LoadRuleset(string path)
    {
        var assembly = Assembly.LoadFrom(path);
        
        var types = assembly.GetTypes();
        
        var rulesetType = (Ruleset) assembly.CreateInstance(types.First(t => t.IsSubclassOf(typeof(Ruleset))).FullName);


        return rulesetType;
    }

    public async Task<List<Ruleset>> LoadRulesets(string root)
    {
        
        var rulesets = Directory.GetFiles(root, "*.dll");

        foreach (var rulesetPath in rulesets)
        {
            var ruleset = await LoadRuleset(rulesetPath);
            _rulesets.Add(ruleset);
#if DEBUG            
            Console.WriteLine($"[Rulesets] Loaded ruleset {ruleset.RulesetInfo.Name}");
#endif
        }
        
        //We need to calculate IDs for the rulesets

        await CalculateIDs();
        
        return Rulesets.Values.ToList();
    }

    public async Task CalculateIDs()
    {
        //We need calculate legacy rulesets first.
        foreach (var ruleset in _rulesets.Where(c => c.RulesetInfo.OnlineID > -1))
        {
            Rulesets.Add(ruleset.RulesetInfo.OnlineID, ruleset);
        }
        
        foreach (var ruleset in _rulesets.Where(c => c.RulesetInfo.OnlineID < 0))
        {
            ruleset.RulesetInfo.OnlineID = Rulesets.Max(c => c.Value.RulesetInfo.OnlineID) + 1;
            Rulesets.Add(ruleset.RulesetInfo.OnlineID, ruleset);
        }
    }

    public Ruleset GetRuleset(int id) => Rulesets[id];

    public async Task<List<Ruleset>> GetRulesets() => Rulesets.Values.ToList();

    public async Task<Ruleset> GetRulesetByName(string name) => Rulesets.Values.FirstOrDefault(c => c.RulesetInfo.ShortName == name);
    
    public async Task UpdateLegacy()
    {
        
        if (!Directory.Exists("rulesets")) 
            Directory.CreateDirectory("rulesets");
        var zipFile = await (new HttpClient().GetAsync("https://github.com/DHCPCD9/osu-rulesets-dlls/releases/download/latest/rulesets.zip"));

        var filePath = Path.Join(Path.GetTempPath(), "rulesets.zip");
        var file = File.OpenWrite(filePath);
        (await zipFile.Content.ReadAsStreamAsync()).CopyTo(file);
        
        file.Close();   
        var hash = new Dictionary<string, string>();
        using (var zip = ZipFile.OpenRead(filePath))
        {

            var fileHashes = zip.Entries.FirstOrDefault(c => c.Name.EndsWith("hash.txt"));

            var stream = fileHashes.Open();
            
            var content = new StreamReader(stream).ReadToEnd();
            
            foreach(var line in content.Split('\n'))
            {
                var split = line.Split(':');
                if (split.Length == 2)
                {
                    hash.Add(split[0], split[1]);
                }
            }

            var rulesetFiles = zip.Entries.Where(c => c.Name.EndsWith(".dll"));
            
            foreach (var fileEntry in rulesetFiles)
            {
                var hashValue = hash[fileEntry.Name];
                var fileStream = fileEntry.Open();
                var fileHash = BitConverter.ToString(MD5.Create().ComputeHash(fileStream)).Replace("-", "").ToLower();

                if (fileHash != hashValue)
                {
                    Console.WriteLine($"{fileEntry.Name} is corrupted (Local: {fileHash}, Remote: {hashValue})");
                    continue;
                }
                
                var fs = File.OpenWrite(Path.Join("rulesets", fileEntry.Name));
                await fileEntry.Open().CopyToAsync(fs);
                fs.Close();
            }
        }
    }
}