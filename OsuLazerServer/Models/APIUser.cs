using System.Text.Json.Serialization;

namespace OsuLazerServer.Models;

public class Kudosu
{
    [JsonPropertyName("total")] public int Total { get; set; }

    [JsonPropertyName("available")] public int Available { get; set; }
}

public class Country
{
    [JsonPropertyName("code")] public string? Code { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }
}

public class Cover
{
    [JsonPropertyName("custom_url")] public string? CustomUrl { get; set; }

    [JsonPropertyName("url")] public string? Url { get; set; }

    [JsonPropertyName("id")] public string? Id { get; set; }
}

public class MonthlyPlaycount
{
    [JsonPropertyName("start_date")] public string? StartDate { get; set; }

    [JsonPropertyName("count")] public int? Count { get; set; }
}

public class Page
{
    [JsonPropertyName("html")] public string? Html { get; set; }

    [JsonPropertyName("raw")] public string? Raw { get; set; }
}

public class ReplaysWatchedCount
{
    [JsonPropertyName("start_date")] public string? StartDate { get; set; }

    [JsonPropertyName("count")] public int? Count { get; set; }
}

public class Level
{
    [JsonPropertyName("current")] public int Current { get; set; }

    [JsonPropertyName("progress")] public int Progress { get; set; }
}

public class GradeCounts
{
    [JsonPropertyName("ss")] public int Ss { get; set; }

    [JsonPropertyName("ssh")] public int Ssh { get; set; }

    [JsonPropertyName("s")] public int S { get; set; }

    [JsonPropertyName("sh")] public int Sh { get; set; }

    [JsonPropertyName("a")] public int A { get; set; }
}

public class Rank
{
    [JsonPropertyName("country")] public int Country { get; set; }
}

public class Statistics
{
    [JsonPropertyName("level")] public Level? Level { get; set; }

    [JsonPropertyName("global_rank")] public int GlobalRank { get; set; }

    [JsonPropertyName("pp")] public double PerfomancePoints { get; set; }

    [JsonPropertyName("ranked_score")] public long RankedScore { get; set; }

    [JsonPropertyName("hit_accuracy")] public double HitAccuracy { get; set; }

    [JsonPropertyName("play_count")] public int PlayCount { get; set; }

    [JsonPropertyName("play_time")] public int PlayTime { get; set; }

    [JsonPropertyName("total_score")] public long TotalScore { get; set; }

    [JsonPropertyName("total_hits")] public int TotalHits { get; set; }

    [JsonPropertyName("maximum_combo")] public int MaximumCombo { get; set; }

    [JsonPropertyName("replays_watched_by_others")]
    public int ReplaysWatchedByOthers { get; set; }

    [JsonPropertyName("is_ranked")] public bool IsRanked { get; set; }

    [JsonPropertyName("grade_counts")] public GradeCounts? GradeCounts { get; set; }

    [JsonPropertyName("country_rank")] public int CountryRank { get; set; }

    [JsonPropertyName("rank")] public Rank? Rank { get; set; }
}

public class UserAchievement
{
    [JsonPropertyName("achieved_at")] public DateTime AchievedAt { get; set; }

    [JsonPropertyName("achievement_id")] public int AchievementId { get; set; }
}

public class RankHistory
{
    [JsonPropertyName("mode")] public string? Mode { get; set; }

    [JsonPropertyName("data")] public List<int>? Data { get; set; }
}

public class RankHistory2
{
    [JsonPropertyName("mode")] public string? Mode { get; set; }

    [JsonPropertyName("data")] public List<int>? Data { get; set; }
}

public class APIUser
{
    [JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; }

    [JsonPropertyName("country_code")] public string? CountryCode { get; set; }

    [JsonPropertyName("default_group")] public string? DefaultGroup { get; set; }

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("is_active")] public bool IsActive { get; set; }

    [JsonPropertyName("is_bot")] public bool IsBot { get; set; }

    [JsonPropertyName("is_deleted")] public bool? IsDeleted { get; set; }

    [JsonPropertyName("is_online")] public bool? IsOnline { get; set; }

    [JsonPropertyName("is_supporter")] public bool? IsSupporter { get; set; }

    [JsonPropertyName("last_visit")] public DateTime? LastVisit { get; set; }

    [JsonPropertyName("pm_friends_only")] public bool PmFriendsOnly { get; set; }

    [JsonPropertyName("profile_colour")] public string? ProfileColour { get; set; }

    [JsonPropertyName("username")] public string? Username { get; set; }

    [JsonPropertyName("cover_url")] public string? CoverUrl { get; set; }

    [JsonPropertyName("discord")] public string? Discord { get; set; }

    [JsonPropertyName("has_supported")] public bool HasSupported { get; set; }

    [JsonPropertyName("interests")] public string? Interests { get; set; }

    [JsonPropertyName("join_date")] public DateTime? JoinDate { get; set; }

    [JsonPropertyName("kudosu")] public Kudosu? Kudosu { get; set; }

    [JsonPropertyName("location")] public string? Location { get; set; }

    [JsonPropertyName("max_blocks")] public int? MaxBlocks { get; set; }

    [JsonPropertyName("max_friends")] public int? MaxFriends { get; set; }

    [JsonPropertyName("occupation")] public string? Occupation { get; set; }

    [JsonPropertyName("playmode")] public string? Playmode { get; set; }

    [JsonPropertyName("playstyle")] public List<string>? Playstyle { get; set; }

    [JsonPropertyName("post_count")] public int PostCount { get; set; }

    [JsonPropertyName("profile_order")] public List<string> ProfileOrder { get; set; }

    [JsonPropertyName("title")] public string? Title { get; set; }

    [JsonPropertyName("title_url")] public string? TitleUrl { get; set; }

    [JsonPropertyName("twitter")] public string? Twitter { get; set; }

    [JsonPropertyName("website")] public string Website { get; set; }

    [JsonPropertyName("country")] public Country Country { get; set; }

    [JsonPropertyName("cover")] public Cover Cover { get; set; }

    [JsonPropertyName("is_restricted")] public bool IsRestricted { get; set; }

    [JsonPropertyName("account_history")] public List<string>? AccountHistory { get; set; }

    [JsonPropertyName("active_tournament_banner")]
    public string? ActiveTournamentBanner { get; set; }

    [JsonPropertyName("badges")] public List<string>? Badges { get; set; }

    [JsonPropertyName("beatmap_playcounts_count")]
    public int BeatmapPlaycountsCount { get; set; }

    [JsonPropertyName("comments_count")] public int CommentsCount { get; set; }

    [JsonPropertyName("favourite_beatmapset_count")]
    public int FavouriteBeatmapsetCount { get; set; }

    [JsonPropertyName("follower_count")] public int FollowerCount { get; set; }

    [JsonPropertyName("graveyard_beatmapset_count")]
    public int GraveyardBeatmapsetCount { get; set; }

    [JsonPropertyName("groups")] public List<string>? Groups { get; set; }

    [JsonPropertyName("loved_beatmapset_count")]
    public int LovedBeatmapsetCount { get; set; }

    [JsonPropertyName("mapping_follower_count")]
    public int MappingFollowerCount { get; set; }

    [JsonPropertyName("monthly_playcounts")]
    public List<MonthlyPlaycount>? MonthlyPlaycounts { get; set; }

    [JsonPropertyName("page")] public Page? Page { get; set; }

    [JsonPropertyName("pending_beatmapset_count")]
    public int PendingBeatmapsetCount { get; set; }

    [JsonPropertyName("previous_usernames")]
    public List<string>? PreviousUsernames { get; set; }

    [JsonPropertyName("ranked_beatmapset_count")]
    public int RankedBeatmapsetCount { get; set; }

    [JsonPropertyName("replays_watched_counts")]
    public List<ReplaysWatchedCount>? ReplaysWatchedCounts { get; set; }

    [JsonPropertyName("scores_best_count")]
    public int ScoresBestCount { get; set; }

    [JsonPropertyName("scores_first_count")]
    public int ScoresFirstCount { get; set; }

    [JsonPropertyName("scores_pinned_count")]
    public int ScoresPinnedCount { get; set; }

    [JsonPropertyName("scores_recent_count")]
    public int ScoresRecentCount { get; set; }

    [JsonPropertyName("statistics")] public Statistics Statistics { get; set; }

    [JsonPropertyName("support_level")] public int SupportLevel { get; set; }

    [JsonPropertyName("user_achievements")]
    public List<UserAchievement>? UserAchievements { get; set; }

    [JsonPropertyName("rank_history")] public RankHistory? RankHistory { get; set; }

    [JsonPropertyName("ranked_and_approved_beatmapset_count")]
    public int RankedAndApprovedBeatmapsetCount { get; set; }

    [JsonPropertyName("unranked_beatmapset_count")]
    public int UnrankedBeatmapsetCount { get; set; }
}