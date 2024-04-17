// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring.Legacy
{
    /// <summary>
    /// A minified version of <see cref="SoloScoreInfo"/> retrofit onto the end of legacy replay files (.osr),
    /// containing the minimum data required to support storage of non-legacy replays.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class LegacyReplaySoloScoreInfo
    {
        /// <remarks>
        /// The value of this property should correspond to <see cref="ScoreInfo.OnlineID"/>
        /// (i.e. come from the `solo_scores` ID scheme).
        /// </remarks>
        [JsonProperty("online_id")]
        public long OnlineID { get; set; } = -1;

        [JsonProperty("mods")]
        public APIMod[] Mods { get; set; } = Array.Empty<APIMod>();

        [JsonProperty("statistics")]
        public Dictionary<HitResult, int> Statistics { get; set; } = new Dictionary<HitResult, int>();

        [JsonProperty("maximum_statistics")]
        public Dictionary<HitResult, int> MaximumStatistics { get; set; } = new Dictionary<HitResult, int>();

        [JsonProperty("client_version")]
        public string ClientVersion = string.Empty;

        [JsonProperty("total_score_without_mods")]
        public long? TotalScoreWithoutMods { get; set; }

        public static LegacyReplaySoloScoreInfo FromScore(ScoreInfo score) => new LegacyReplaySoloScoreInfo
        {
            OnlineID = score.OnlineID,
            Mods = score.APIMods,
            Statistics = score.Statistics.Where(kvp => kvp.Value != 0).ToDictionary(),
            MaximumStatistics = score.MaximumStatistics.Where(kvp => kvp.Value != 0).ToDictionary(),
            ClientVersion = score.ClientVersion,
            TotalScoreWithoutMods = score.TotalScoreWithoutMods > 0 ? score.TotalScoreWithoutMods : null,
        };
    }
}
