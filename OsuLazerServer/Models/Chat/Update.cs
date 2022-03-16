using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Chat;

public class Update
{

    [JsonIgnore]
    public DateTimeOffset UpdateRecievedAt;
    
    [JsonPropertyName("presence")]
    public List<Channel> Channels { get; set; }

    [JsonPropertyName("messages")] public List<Message> Messages { get; set; }
}

public class Message
{
    
    [JsonPropertyName("message_id")] public long MessageId { get; set; }
    [JsonPropertyName("sender_id")] public int SenderId { get; set; }
    [JsonPropertyName("channel_id")] public long ChannelId { get; set; }
    [JsonPropertyName("timestamp")] public DateTime Timetamp { get; set; }
    [JsonPropertyName("content")] public string Content { get; set; }
    [JsonPropertyName("sender")] public Sender Sender { get; set; }
}

public class Channel
{

    public List<Message> Messages;
    [JsonPropertyName("channel_id")] public int ChannelId { get; set; }
    [JsonPropertyName("description")] public string Description;
    [JsonPropertyName("icon")] public string? Icon { get; set; }
    [JsonPropertyName("last_message_id")] public long? LastMessageId { get; set; }
    [JsonPropertyName("last_read_id")] public long? LastReadId { get; set; }
    [JsonPropertyName("moderated")] public bool Moderated { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("users")] public List<int> Users { get; set; }

}

public class Sender
{
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }

    [JsonPropertyName("default_group")]
    public string DefaultGroup { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("is_deleted")]
    public bool IsDeleted { get; set; }

    [JsonPropertyName("is_online")]
    public bool IsOnline { get; set; }

    [JsonPropertyName("is_supporter")]
    public bool IsSupporter { get; set; }

    [JsonPropertyName("last_visit")]
    public DateTime? LastVisit { get; set; }

    [JsonPropertyName("pm_friends_only")]
    public bool PmFriendsOnly { get; set; }

    [JsonPropertyName("profile_colour")]
    public object ProfileColour { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}