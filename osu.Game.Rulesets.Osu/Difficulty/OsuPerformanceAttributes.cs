// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public class OsuPerformanceAttributes : PerformanceAttributes
    {
        [JsonProperty("aim")]
        public double Aim { get; set; }

        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("flashlight")]
        public double Flashlight { get; set; }

        [JsonProperty("effective_miss_count")]
        public double EffectiveMissCount { get; set; }

        [JsonProperty("speed_deviation")]
        public double? SpeedDeviation { get; set; }

        [JsonProperty("combo_based_estimated_miss_count")]
        public double ComboBasedEstimatedMissCount { get; set; }

        [JsonProperty("score_based_estimated_miss_count")]
        public double? ScoreBasedEstimatedMissCount { get; set; }

        [JsonProperty("aim_estimated_slider_breaks")]
        public double AimEstimatedSliderBreaks { get; set; }

        [JsonProperty("speed_estimated_slider_breaks")]
        public double SpeedEstimatedSliderBreaks { get; set; }

        public override IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            foreach (var attribute in base.GetAttributesForDisplay())
                yield return attribute;

            yield return new PerformanceDisplayAttribute(nameof(Aim), "Aim", Aim);
            yield return new PerformanceDisplayAttribute(nameof(Speed), "Speed", Speed);
            yield return new PerformanceDisplayAttribute(nameof(Accuracy), "Accuracy", Accuracy);
            yield return new PerformanceDisplayAttribute(nameof(Flashlight), "Flashlight Bonus", Flashlight);
        }
    }
}
