using osu.Game.Online.API;
using OsuLazerServer.Models.Response.Beatmaps;
using System.Text.Json.Serialization;

namespace OsuLazerServer.Models.Multiplayer;

    public class PlaylistItem
    {
        
        [JsonPropertyName("id")]
        public int ID { get; set; }
        [JsonPropertyName("owner_id")]
        public int OwnerId { get; set; }

        [JsonPropertyName("ruleset_id")]
        public int RulesetId { get; set; }

        [JsonPropertyName("expired")]
        public bool Expired { get; set; }

        [JsonPropertyName("playlist_order")]
        public ushort? PlaylistOrder { get; set; }

        [JsonPropertyName("played_at")]
        public DateTime? PlayedAt { get; set; }

        [JsonPropertyName("allowed_mods")]
        public List<APIMod> AllowedMods { get; set; }

        [JsonPropertyName("required_mods")]
        public List<APIMod> RequiredMods { get; set; }

        [JsonPropertyName("beatmap_id")]
        public int BeatmapId { get; set; }

        [JsonPropertyName("beatmap")] public APIBeatmap? Beatmap { get; set; }
    }