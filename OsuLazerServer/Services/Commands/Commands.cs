using OsuLazerServer.Attributes;

namespace OsuLazerServer.Services.Commands;

public class Commands
{
    [Command("help", "there is not help", -1, true)]
    public static string GetHelp(List<string> args)
    {
        return "There is not help!";
    }
}