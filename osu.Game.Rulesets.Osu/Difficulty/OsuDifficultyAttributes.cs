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
        [JsonProperty("aim_difficulty")]
        public double AimDifficulty { get; set; }

        [JsonProperty("speed_difficulty")]
        public double SpeedDifficulty { get; set; }

        [JsonProperty("flashlight_difficulty")]
        public double FlashlightDifficulty { get; set; }

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

            yield return (ATTRIB_ID_AIM, AimDifficulty);
            yield return (ATTRIB_ID_SPEED, SpeedDifficulty);
            yield return (ATTRIB_ID_OVERALL_DIFFICULTY, OverallDifficulty);
            yield return (ATTRIB_ID_APPROACH_RATE, ApproachRate);
            yield return (ATTRIB_ID_MAX_COMBO, MaxCombo);
            yield return (ATTRIB_ID_DIFFICULTY, StarRating);

            if (ShouldSerializeFlashlightRating())
                yield return (ATTRIB_ID_FLASHLIGHT, FlashlightDifficulty);

            yield return (ATTRIB_ID_SLIDER_FACTOR, SliderFactor);
        }

        public override void FromDatabaseAttributes(IReadOnlyDictionary<int, double> values)
        {
            base.FromDatabaseAttributes(values);

            AimDifficulty = values[ATTRIB_ID_AIM];
            SpeedDifficulty = values[ATTRIB_ID_SPEED];
            OverallDifficulty = values[ATTRIB_ID_OVERALL_DIFFICULTY];
            ApproachRate = values[ATTRIB_ID_APPROACH_RATE];
            MaxCombo = (int)values[ATTRIB_ID_MAX_COMBO];
            StarRating = values[ATTRIB_ID_DIFFICULTY];
            FlashlightDifficulty = values.GetValueOrDefault(ATTRIB_ID_FLASHLIGHT);
            SliderFactor = values[ATTRIB_ID_SLIDER_FACTOR];
        }

        #region Newtonsoft.Json implicit ShouldSerialize() methods

        // The properties in this region are used implicitly by Newtonsoft.Json to not serialise certain fields in some cases.
        // They rely on being named exactly the same as the corresponding fields (casing included) and as such should NOT be renamed
        // unless the fields are also renamed.

        [UsedImplicitly]
        public bool ShouldSerializeFlashlightRating() => Mods.Any(m => m is ModFlashlight);

        #endregion
    }
}
