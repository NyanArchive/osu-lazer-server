namespace OsuLazerServer.Services.Commands;

public interface ICommandManager
{
    public CommandItem? GetCommandByName(string command);
}