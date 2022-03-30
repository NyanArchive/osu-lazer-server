namespace OsuLazerServer.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public string Command { get; set; }
    public string Description { get; set; }
    public int RequiredArgs { get; set; }
    public bool AdminRequired { get; set; }
    public bool IsPublic { get; set; }

    public CommandAttribute(string command, string description, int requiredArgs = -1, bool adminRequired = false,
        bool isPublic = false)
    {
        Command = command;
        Description = description;
        RequiredArgs = requiredArgs;
        AdminRequired = adminRequired;
        IsPublic = isPublic;
    }
}