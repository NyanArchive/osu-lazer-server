using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Response.Comments;

public class CommentsResponse : WithCursor
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("top_level_count")]
    public int TopLevelCount { get; set; }
    
    [JsonPropertyName("users")]
    public List<APIUser> Users { get; set; }
    
    [JsonPropertyName("user_votes")]
    public List<long> UserVotes { get; set; }
    
    [JsonPropertyName("pinned_comments")]
    public List<Comment> PinnedComments { get; set; }
    
    [JsonPropertyName("included_comments")]
    public List<Comment> IncludedComments { get; set; }
    
    [JsonPropertyName("user_follow")]
    public bool UserFollow { get; set; }
    
    [JsonPropertyName("has_more_id")]
    public long HasMoreId { get; set; }
    
    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }

    [JsonPropertyName("comments")] public List<Comment> Comments;
    [JsonPropertyName("commentable_meta")] public APICommentableMeta[] Meta;
    [JsonPropertyName("cursor")]
    public APICursor Cursor { get; set; }
}

public interface WithCursor
{
    [JsonPropertyName("cursor")]
    public APICursor Cursor { get; set; }
}

public class APICursor
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class APICommentableMeta
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("owner_id")]
    public int OwnerId { get; set; }
    [JsonPropertyName("owner_title")]
    public string OwnerTitle { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class Comment
{
    [JsonPropertyName(@"id")]
    public long Id { get; set; }

    [JsonPropertyName(@"parent_id")]
    public long? ParentId { get; set; }

    [JsonPropertyName(@"user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName(@"message")]
    public string Message { get; set; }

    [JsonPropertyName(@"message_html")]
    public string MessageHtml { get; set; }

    [JsonPropertyName(@"replies_count")]
    public int RepliesCount { get; set; }

    [JsonPropertyName(@"votes_count")]
    public int VotesCount { get; set; }

    [JsonPropertyName(@"commenatble_type")]
    public string CommentableType { get; set; }

    [JsonPropertyName(@"commentable_id")]
    public int CommentableId { get; set; }

    [JsonPropertyName(@"legacy_name")]
    public string LegacyName { get; set; }

    [JsonPropertyName(@"created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName(@"updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName(@"deleted_at")]
    public DateTimeOffset? DeletedAt { get; set; }

    [JsonPropertyName(@"edited_at")]
    public DateTimeOffset? EditedAt { get; set; }

    [JsonPropertyName(@"edited_by_id")]
    public long? EditedById { get; set; }

    [JsonPropertyName(@"pinned")]
    public bool Pinned { get; set; }
}