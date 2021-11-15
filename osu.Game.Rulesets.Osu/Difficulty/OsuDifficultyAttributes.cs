// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuDifficultyAttributes : DifficultyAttributes
    {
        [JsonProperty("aim_strain")]
        public double AimStrain { get; set; }

        [JsonProperty("speed_strain")]
        public double SpeedStrain { get; set; }

        [JsonProperty("flashlight_rating")]
        public double FlashlightRating { get; set; }

        [JsonProperty("slider_factor")]
        public double SliderFactor { get; set; }

        [JsonProperty("approach_rate")]
        public double ApproachRate { get; set; }

        [JsonProperty("overall_difficulty")]
        public double OverallDifficulty { get; set; }

        [JsonProperty("drain_rate")]
        public double DrainRate { get; set; }

        [JsonProperty("hit_circle_count")]
        public int HitCircleCount { get; set; }

        [JsonProperty("slider_count")]
        public int SliderCount { get; set; }

        [JsonProperty("spinner_count")]
        public int SpinnerCount { get; set; }
    }
}
