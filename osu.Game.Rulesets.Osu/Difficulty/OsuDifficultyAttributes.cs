// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;

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

        public override IEnumerable<(int attributeId, object value)> ToDatabase()
        {
            foreach (var v in base.ToDatabase())
                yield return v;

            yield return (1, AimStrain);
            yield return (3, SpeedStrain);
            yield return (5, OverallDifficulty);
            yield return (7, ApproachRate);
            yield return (9, MaxCombo);
            yield return (11, StarRating);

            if (Mods.Any(m => m is ModFlashlight))
                yield return (17, FlashlightRating);

            yield return (19, SliderFactor);
        }

        public override void FromDatabase(IReadOnlyDictionary<int, double> values, int hitCircleCount, int spinnerCount)
        {
            base.FromDatabase(values, hitCircleCount, spinnerCount);

            AimStrain = values[1];
            SpeedStrain = values[3];
            OverallDifficulty = values[5];
            ApproachRate = values[7];
            MaxCombo = (int)values[9];
            StarRating = values[11];
            FlashlightRating = values.GetValueOrDefault(17);
            SliderFactor = values[19];
            HitCircleCount = hitCircleCount;
            SpinnerCount = spinnerCount;
        }
    }
}
