using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace OsuLazerServer.Models.Response.Beatmaps;


public enum BeatmapOnlineStatus
{
    None = -3,
    Graveyard = -2,
    WIP = -1,
    Pending = 0,
    Ranked = 1,
    Approved = 2,
    Qualified = 3,
    Loved = 4,
}
public class BeatmapSetNominationStatus
{
    /// <summary>
    /// The current number of nominations that the set has received.
    /// </summary>
    [JsonPropertyName(@"current")]
    public int Current { get; set; }

    /// <summary>
    /// The number of nominations required so that the map is eligible for qualification.
    /// </summary>
    [JsonPropertyName(@"required")]
    public int Required { get; set; }
}
/// <summary>
/// Contains information about the current hype status of a beatmap set.
/// </summary>
public class BeatmapSetHypeStatus
{
    /// <summary>
    /// The current number of hypes that the set has received.
    /// </summary>
    [JsonPropertyName(@"current")]
    public int Current { get; set; }

    /// <summary>
    /// The number of hypes required so that the set is eligible for nomination.
    /// </summary>
    [JsonPropertyName(@"required")]
    public int Required { get; set; }
}

public struct BeatmapSetOnlineCovers
{

    [JsonPropertyName(@"cover@2x")] public string Cover2x { get; set; }

    [JsonPropertyName(@"cover")]
    public string Cover { get; set; }
    
    [JsonPropertyName(@"card@2x")]
    public string Card2x { get; set; }
    
    [JsonPropertyName(@"card")]
    public string Card { get; set; }
    
    [JsonPropertyName(@"list@")]
    public string List { get; set; }
    
    [JsonPropertyName(@"list@2x")]
    public string List2x { get; set; }


    [JsonPropertyName("slimcover")] public string SlimCover { get; set; }
    [JsonPropertyName("slimcover@2x")] public string SlimCover2x { get; set; }

}
public struct BeatmapSetOnlineAvailability
{
    [JsonPropertyName(@"download_disabled")]
    public bool DownloadDisabled { get; set; }

    [JsonPropertyName(@"more_information")]
    public string ExternalLink { get; set; }
}

public struct BeatmapSetOnlineGenre
{
    public int Id { get; set; }
    public string Name { get; set; }
}
public struct BeatmapSetOnlineLanguage
{
    public int Id { get; set; }
    public string Name { get; set; }
}

    public class APIBeatmap
    {
        [JsonPropertyName(@"id")]
        public int OnlineID { get; set; }

        [JsonPropertyName(@"beatmapset_id")]
        public int OnlineBeatmapSetID { get; set; }

        [JsonPropertyName(@"status")]
        public BeatmapOnlineStatus Status { get; set; }

        [JsonPropertyName("checksum")]
        public string Checksum { get; set; } = string.Empty;

        [JsonPropertyName(@"playcount")]
        private int playCount { get; set; }

        [JsonPropertyName(@"passcount")]
        private int passCount { get; set; }

        [JsonPropertyName(@"mode_int")]
        public int RulesetID { get; set; }

        [JsonPropertyName(@"difficulty_rating")]
        public double StarRating { get; set; }

        [JsonPropertyName(@"drain")]
        private float drainRate { get; set; }

        [JsonPropertyName(@"cs")]
        private float circleSize { get; set; }

        [JsonPropertyName(@"ar")]
        private float approachRate { get; set; }

        [JsonPropertyName(@"accuracy")]
        private float overallDifficulty { get; set; }

        [JsonPropertyName(@"total_length")]
        public double Length { get; set; }

        [JsonPropertyName(@"count_circles")]
        private int circleCount { get; set; }

        [JsonPropertyName(@"count_sliders")]
        private int sliderCount { get; set; }

        [JsonPropertyName(@"version")]
        public string DifficultyName { get; set; } = string.Empty;

        [JsonPropertyName(@"failtimes")]
        private object? metrics { get; set; }

        [JsonPropertyName(@"max_combo")]
        private int? maxCombo { get; set; }
        
        
    }
 public class APIBeatmapSet
    {
        [JsonPropertyName(@"covers")]
        public BeatmapSetOnlineCovers Covers { get; set; }

        [JsonPropertyName(@"id")]
        public int OnlineID { get; set; }

        [JsonPropertyName(@"status")]
        public BeatmapOnlineStatus Status { get; set; }

        [JsonPropertyName(@"preview_url")]
        public string Preview { get; set; } = string.Empty;

        [JsonPropertyName(@"has_favourited")]
        public bool HasFavourited { get; set; }

        [JsonPropertyName(@"play_count")]
        public int PlayCount { get; set; }

        [JsonPropertyName(@"favourite_count")]
        public int FavouriteCount { get; set; }

        [JsonPropertyName(@"bpm")]
        public double BPM { get; set; }

        [JsonPropertyName(@"nsfw")]
        public bool HasExplicitContent { get; set; }

        [JsonPropertyName(@"video")]
        public bool HasVideo { get; set; }

        [JsonPropertyName(@"storyboard")]
        public bool HasStoryboard { get; set; }

        [JsonPropertyName(@"submitted_date")]
        public string Submitted { get; set; }

        [JsonPropertyName(@"ranked_date")]
        public string? Ranked { get; set; }

        [JsonPropertyName(@"last_updated")]
        public string? LastUpdated { get; set; }

        [JsonPropertyName("ratings")]
        public int[] Ratings { get; set; } = Array.Empty<int>();

        [JsonPropertyName(@"track_id")]
        public int? TrackId { get; set; }

        [JsonPropertyName(@"hype")]
        public BeatmapSetHypeStatus? HypeStatus { get; set; }

        [JsonPropertyName(@"nominations_summary")]
        public BeatmapSetNominationStatus? NominationStatus { get; set; }

        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("title_unicode")]
        public string TitleUnicode { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("artist_unicode")]
        public string ArtistUnicode { get; set; } = string.Empty;

        /// <summary>
        /// Helper property to deserialize a username to <see cref="APIUser"/>.
        /// </summary>
        [JsonPropertyName(@"user_id")]
        public int AuthorID { get; set; }

        /// <summary>
        /// Helper property to deserialize a username to <see cref="APIUser"/>.
        /// </summary>
        [JsonPropertyName(@"creator")]
        public string AuthorString { get; set; }

        [JsonPropertyName(@"availability")]
        public BeatmapSetOnlineAvailability Availability { get; set; }

        [JsonPropertyName(@"genre")]
        public BeatmapSetOnlineGenre Genre { get; set; }

        [JsonPropertyName(@"language")]
        public BeatmapSetOnlineLanguage Language { get; set; }

        public string Source { get; set; } = string.Empty;

        [JsonPropertyName(@"tags")]
        public string Tags { get; set; } = string.Empty;

        [JsonPropertyName(@"beatmaps")]
        public APIBeatmap[] Beatmaps { get; set; } = Array.Empty<APIBeatmap>();


    }