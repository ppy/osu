// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaDifficultyAttributes : DifficultyAttributes
    {
        [JsonProperty("cross_column_difficulty")]
        public double CrossColumnDifficulty { get; set; }

        [JsonProperty("jack_difficulty")]
        public double JackDifficulty { get; set; }

        [JsonProperty("pressing_intensity_difficulty")]
        public double PressingIntensityDifficulty { get; set; }

        [JsonProperty("release_difficulty")]
        public double ReleaseDifficulty { get; set; }

        [JsonProperty("unevenness_difficulty")]
        public double UnevennessDifficulty { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (ATTRIB_ID_DIFFICULTY, StarRating);
            yield return (10000, CrossColumnDifficulty);
            yield return (10001, JackDifficulty);
            yield return (10002, PressingIntensityDifficulty);
            yield return (10003, ReleaseDifficulty);
            yield return (10004, UnevennessDifficulty);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values, IBeatmapOnlineInfo onlineInfo)
        {
            base.FromDatabaseAttributes(values, onlineInfo);

            StarRating = values[ATTRIB_ID_DIFFICULTY];
            CrossColumnDifficulty = values[10001];
            JackDifficulty = values[10002];
            PressingIntensityDifficulty = values[10003];
            ReleaseDifficulty = values[10004];
            UnevennessDifficulty = values[10005];
        }
    }
}
