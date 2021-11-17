// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

        public double DrainRate { get; set; }

        public int HitCircleCount { get; set; }

        public int SliderCount { get; set; }

        public int SpinnerCount { get; set; }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (1, AimStrain);
            yield return (3, SpeedStrain);
            yield return (5, OverallDifficulty);
            yield return (7, ApproachRate);
            yield return (9, MaxCombo);
            yield return (11, StarRating);

            if (ShouldSerializeFlashlightRating())
                yield return (17, FlashlightRating);

            yield return (19, SliderFactor);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values)
        {
            base.FromDatabaseAttributes(values);

            AimStrain = values[1];
            SpeedStrain = values[3];
            OverallDifficulty = values[5];
            ApproachRate = values[7];
            MaxCombo = (int)values[9];
            StarRating = values[11];
            FlashlightRating = values.GetValueOrDefault(17);
            SliderFactor = values[19];
        }

        // Used implicitly by Newtonsoft.Json to not serialize flashlight property in some cases.
        [UsedImplicitly]
        public bool ShouldSerializeFlashlightRating() => Mods.Any(m => m is ModFlashlight);
    }
}
