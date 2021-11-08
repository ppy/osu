// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Online.Rooms
{
    public class MultiplayerScore
    {
        [JsonProperty("id")]
        public long ID { get; set; }

        [JsonProperty("user")]
        public APIUser User { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ScoreRank Rank { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("mods")]
        public APIMod[] Mods { get; set; }

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics = new Dictionary<HitResult, int>();

        [JsonProperty("passed")]
        public bool Passed { get; set; }

        [JsonProperty("ended_at")]
        public DateTimeOffset EndedAt { get; set; }

        /// <summary>
        /// The position of this score, starting at 1.
        /// </summary>
        [JsonProperty("position")]
        public int? Position { get; set; }

        /// <summary>
        /// Any scores in the room around this score.
        /// </summary>
        [JsonProperty("scores_around")]
        [CanBeNull]
        public MultiplayerScoresAround ScoresAround { get; set; }

        public ScoreInfo CreateScoreInfo(PlaylistItem playlistItem, [NotNull] BeatmapInfo beatmap)
        {
            var rulesetInstance = playlistItem.Ruleset.Value.CreateInstance();

            var scoreInfo = new ScoreInfo
            {
                OnlineScoreID = ID,
                TotalScore = TotalScore,
                MaxCombo = MaxCombo,
                BeatmapInfo = beatmap,
                BeatmapInfoID = playlistItem.BeatmapID,
                Ruleset = playlistItem.Ruleset.Value,
                RulesetID = playlistItem.RulesetID,
                Statistics = Statistics,
                User = User,
                Accuracy = Accuracy,
                Date = EndedAt,
                Hash = string.Empty, // todo: temporary?
                Rank = Rank,
                Mods = Mods?.Select(m => m.ToMod(rulesetInstance)).ToArray() ?? Array.Empty<Mod>(),
                Position = Position,
            };

            return scoreInfo;
        }
    }
}
