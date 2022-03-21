using System.Text.Json.Serialization;
using OsuLazerServer.Models.Response.Beatmaps;
using OsuLazerServer.Utils;

namespace OsuLazerServer.Services.Beatmaps;

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Hype
    {
        [JsonPropertyName("current")]
        public int? Current { get; set; }

        [JsonPropertyName("required")]
        public int? Required { get; set; }
    }

    public class Availability
    {
        [JsonPropertyName("download_disabled")]
        public bool DownloadDisabled { get; set; }

        [JsonPropertyName("more_information")]
        public object MoreInformation { get; set; }
    }

    public class NominationsSummary
    {
        [JsonPropertyName("current")]
        public int Current { get; set; }

        [JsonPropertyName("required")]
        public int Required { get; set; }
    }

    public class Beatmap
    {
        [JsonPropertyName("difficulty_rating")]
        public double DifficultyRating { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("total_length")]
        public int TotalLength { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("accuracy")]
        public double Accuracy { get; set; }

        [JsonPropertyName("ar")]
        public double Ar { get; set; }

        [JsonPropertyName("beatmapset_id")]
        public int BeatmapsetId { get; set; }

        [JsonPropertyName("bpm")]
        public string Bpm { get; set; }

        [JsonPropertyName("convert")]
        public bool Convert { get; set; }

        [JsonPropertyName("count_circles")]
        public int CountCircles { get; set; }

        [JsonPropertyName("count_sliders")]
        public int CountSliders { get; set; }

        [JsonPropertyName("count_spinners")]
        public int CountSpinners { get; set; }

        [JsonPropertyName("cs")]
        public double Cs { get; set; }

        [JsonPropertyName("deleted_at")]
        public object DeletedAt { get; set; }

        [JsonPropertyName("drain")]
        public double Drain { get; set; }

        [JsonPropertyName("hit_length")]
        public int HitLength { get; set; }

        [JsonPropertyName("is_scoreable")]
        public bool IsScoreable { get; set; }

        [JsonPropertyName("last_updated")]
        public string LastUpdated { get; set; }

        [JsonPropertyName("mode_int")]
        public int ModeInt { get; set; }

        [JsonPropertyName("passcount")]
        public int Passcount { get; set; }

        [JsonPropertyName("playcount")]
        public int Playcount { get; set; }

        [JsonPropertyName("ranked")]
        public int Ranked { get; set; }

        [JsonPropertyName("url")]
        public object Url { get; set; }

        [JsonPropertyName("checksum")]
        public string Checksum { get; set; }

        [JsonPropertyName("max_combo")]
        public int MaxCombo { get; set; }

        public async Task<APIBeatmap> ToOsu(IBeatmapSetResolver? resolver = null) => new APIBeatmap
        {
            Checksum = Checksum,
            Length = HitLength,
            Status = BeatmapUtils.Status(Status?.ToLower()??"graveyard"),
            DifficultyName = Version,
            StarRating = DifficultyRating,
            OnlineID = Id,
            RulesetID = ModeInt,
            OnlineBeatmapSetID = BeatmapsetId,
            BeatmapSet = resolver?.FetchSetAsync(BeatmapsetId).GetAwaiter().GetResult()?.ToBeatmapSet()
        };
    }

    public class Description
    {
        [JsonPropertyName("description")]
        public string Description_ { get; set; }
    }

    public class Genre
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Language
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Cache
    {
        [JsonPropertyName("video")]
        public bool Video { get; set; }

        [JsonPropertyName("noVideo")]
        public bool NoVideo { get; set; }
    }

    public class BeatmapSet
    {
        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("artist_unicode")]
        public string ArtistUnicode { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; }

        [JsonPropertyName("favourite_count")]
        public int FavouriteCount { get; set; }

        [JsonPropertyName("hype")]
        public Hype Hype { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nsfw")]
        public bool Nsfw { get; set; }

        [JsonPropertyName("play_count")]
        public int PlayCount { get; set; }

        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("title_unicode")]
        public string TitleUnicode { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("video")]
        public bool Video { get; set; }

        [JsonPropertyName("availability")]
        public Availability Availability { get; set; }

        [JsonPropertyName("bpm")]
        public string Bpm { get; set; }

        [JsonPropertyName("can_be_hyped")]
        public bool CanBeHyped { get; set; }

        [JsonPropertyName("discussion_enabled")]
        public bool DiscussionEnabled { get; set; }

        [JsonPropertyName("discussion_locked")]
        public bool DiscussionLocked { get; set; }

        [JsonPropertyName("is_scoreable")]
        public bool IsScoreable { get; set; }

        [JsonPropertyName("last_updated")]
        public string LastUpdated { get; set; }

        [JsonPropertyName("legacy_thread_url")]
        public string LegacyThreadUrl { get; set; }

        [JsonPropertyName("nominations_summary")]
        public NominationsSummary NominationsSummary { get; set; }

        [JsonPropertyName("ranked")]
        public int Ranked { get; set; }

        [JsonPropertyName("ranked_date")]
        public string RankedDate { get; set; }

        [JsonPropertyName("storyboard")]
        public bool Storyboard { get; set; }

        [JsonPropertyName("submitted_date")]
        public string SubmittedDate { get; set; }

        [JsonPropertyName("tags")]
        public string Tags { get; set; }

        [JsonPropertyName("has_favourited")]
        public bool HasFavourited { get; set; }

        [JsonPropertyName("beatmaps")]
        public List<Beatmap> Beatmaps { get; set; }

        [JsonPropertyName("description")]
        public Description Description { get; set; }

        [JsonPropertyName("genre")]
        public Genre Genre { get; set; }

        [JsonPropertyName("language")]
        public Language Language { get; set; }

        [JsonPropertyName("ratings_string")]
        public string RatingsString { get; set; }

        [JsonPropertyName("cache")]
        public Cache Cache { get; set; }

        public APIBeatmapSet ToBeatmapSet() => new APIBeatmapSet
        {
            OnlineID = Id,
            Artist = Artist,
            Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = false,
                ExternalLink = ""
            },
            Beatmaps = Beatmaps?.Select(beatmap => new APIBeatmap
            {
                Checksum = beatmap?.Checksum??"",
                Length = beatmap?.HitLength??0,
                Status = BeatmapUtils.Status(beatmap?.Status?.ToLower()??"graveyard"),
                DifficultyName = beatmap?.Version??"",
                StarRating = beatmap?.DifficultyRating??0,
                OnlineID = beatmap?.Id??0,
                RulesetID = beatmap?.ModeInt??0,
                OnlineBeatmapSetID = Id
            }).ToArray()??new APIBeatmap[] {},
            Covers = new BeatmapSetOnlineCovers
            {
                Card = $"https://assets.ppy.sh/beatmaps/{Id}/covers/card.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                Cover = $"https://assets.ppy.sh/beatmaps/{Id}/covers/cover.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                List = $"https://assets.ppy.sh/beatmaps/{Id}/covers/list.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                SlimCover = $"https://assets.ppy.sh/beatmaps/{Id}/covers/slim@2x.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                Card2x = $"https://assets.ppy.sh/beatmaps/{Id}/covers/card@2x.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                Cover2x = $"https://assets.ppy.sh/beatmaps/{Id}/covers/cover@2x.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                List2x = $"https://assets.ppy.sh/beatmaps/{Id}/covers/list@2x.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                SlimCover2x = $"https://assets.ppy.sh/beatmaps/{Id}/covers/slim@2x.jpg?{DateTimeOffset.Now.ToUnixTimeSeconds()}"
            },
            Genre = new BeatmapSetOnlineGenre
            {
                Id = Genre.Id ?? 1,
                Name = Genre.Name
            },
            Language = new BeatmapSetOnlineLanguage
            {
                Id = Language.Id ?? 1,
                Name = Language.Name
            },
            Preview = PreviewUrl,
            Ranked = RankedDate,
            Ratings = new int[] { },
            Source = Source,
            Status = BeatmapUtils.Status(Status),
            Submitted = SubmittedDate,
            Tags = Tags,
            Title = Title,
            ArtistUnicode = ArtistUnicode,
            AuthorString = Artist,
            FavouriteCount = 1,
            HasFavourited = HasFavourited,
            HasStoryboard = Storyboard,
            HasVideo = Video,
            HypeStatus = new BeatmapSetHypeStatus
            {
                Current = 1,
                Required = 1
            },
            LastUpdated = LastUpdated,
            NominationStatus = new BeatmapSetNominationStatus
            {
                Current = 1,
                Required = 1
            },
            PlayCount = PlayCount,
            TitleUnicode = TitleUnicode,
            TrackId = Id,
            AuthorID = 1,
            BPM = Convert.ToDouble(Bpm),
            HasExplicitContent = Nsfw
        };
    }

