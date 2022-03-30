
using OsuLazerServer.Database.Tables;
using OsuLazerServer.Models.Chat;
using OsuLazerServer.Services.Users;
using ChannelModel = OsuLazerServer.Models.Chat.Channel;

namespace OsuLazerServer.Services.Commands;

public class CommandContext
{
    public string Command { get; set; }
    public string[] Args { get; set; }
    public User User { get; set; }
    private IUserStorage  _userStorage;
    private int _userId;
    public ChannelModel Channel { get; set; }
    
    public CommandContext(string command, string[] args, int userId, int channelId, IUserStorage storage)
    {
        _userId = userId;
        _userStorage = storage;
        Command = command;
        Args = args;
        Channel = _userStorage.GetChannel(channelId);
        User = storage.Users.Values.FirstOrDefault(x => x.Id == userId)!;
    }
    
    public async Task Reply(string message)
    {
        await _userStorage.SendMessageToChannel(UserStorage.SystemSender, Channel, message, false);
    }
    
    public async Task ReplyInDm(string message)
    {
        await _userStorage.SendMessageToUser(User, message, false);
    }
}