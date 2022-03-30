using System.Reflection;
using OsuLazerServer.Attributes;

namespace OsuLazerServer.Services.Commands;

public class CommandManagerService : ICommandManager, IServiceScope
{
    private Dictionary<string, CommandItem> CommandCache { get; set; } = new();
    
    public IServiceProvider ServiceProvider { get; }
    private IServiceScope Scope { get; set; }
    private  Commands Commands { get; set; }
    public CommandManagerService(IServiceProvider provider)
    {
        ServiceProvider = provider;
        Scope = provider.CreateScope();
        Commands = new Commands(ServiceProvider);

    }
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
                Action = (list) =>
                {
                    return method.Invoke(Commands, list.ToArray()).ToString();
                },
                Description = attr.Description,
                Name = attr.Command,
                RequiredArguments = attr.RequiredArgs
            };
            
            CommandCache.Add(cmdString, cmd);
        }

        return cmd;

    }

    public void Dispose()
    {
        Scope.Dispose();
    }

}