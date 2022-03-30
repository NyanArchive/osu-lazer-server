namespace OsuLazerServer.Services.Commands;

public class CommandItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool AdminRequired { get; set; }
    public Func<List<object>, string> Action { get; set; }
    public int RequiredArguments { get; set; }
}