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

        [JsonProperty("tap")]
        public double Tap { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("flashlight")]
        public double Flashlight { get; set; }

        [JsonProperty("visual")]
        public double Visual { get; set; }

        [JsonProperty("effective_miss_count")]
        public double EffectiveMissCount { get; set; }

        public override IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            foreach (var attribute in base.GetAttributesForDisplay())
                yield return attribute;

            yield return new PerformanceDisplayAttribute(nameof(Aim), "Aim", Aim);
            yield return new PerformanceDisplayAttribute(nameof(Tap), "Tap", Tap);
            yield return new PerformanceDisplayAttribute(nameof(Accuracy), "Accuracy", Accuracy);
            yield return new PerformanceDisplayAttribute(nameof(Flashlight), "Flashlight Bonus", Flashlight);
            yield return new PerformanceDisplayAttribute(nameof(Visual), "Visual", Visual);
        }
    }
}
