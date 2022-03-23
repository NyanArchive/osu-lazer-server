using System.Reflection;
using OsuLazerServer.Attributes;

namespace OsuLazerServer.Services.Commands;

public class CommandManagerService : ICommandManager
{

    private Dictionary<string, CommandItem> CommandCache { get; set; } = new();
    public CommandItem? GetCommandByName(string command)
    {
        var spaceIndex = command.IndexOf(' ');
        var cmdString = command[spaceIndex == -1 ? 1.. : 1..spaceIndex];
        
        if (!CommandCache.TryGetValue(cmdString, out var cmd))
        {
            var method = Assembly.GetEntryAssembly().GetTypes()
                .SelectMany(type => type.GetMethods())
                .FirstOrDefault(m => m.GetCustomAttribute<CommandAttribute>()?.Command == cmdString);

            if (method is null)
                return null;
            var attr = method.GetCustomAttribute<CommandAttribute>();

            cmd = new()
            {
                AdminRequired = attr.AdminRequired,
                Action = new Func<List<string>, string>((list) =>
                {
                    return method.Invoke(null, new[] {list}).ToString();
                }),
                Description = attr.Description,
                Name = attr.Command
            };
            
            CommandCache.Add(cmdString, cmd);
        }

        return cmd;

    }
}