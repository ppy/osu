// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public struct OsuPerformanceAttributes : IPerformanceAttributes
    {
        /// <summary>
        /// Calculated score performance points.
        /// </summary>
        [JsonProperty("pp")]
        public double Total { get; set; }

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

        public IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            yield return new PerformanceDisplayAttribute(nameof(Total), "Achieved PP", Total);
            yield return new PerformanceDisplayAttribute(nameof(Aim), "Aim", Aim);
            yield return new PerformanceDisplayAttribute(nameof(Speed), "Speed", Speed);
            yield return new PerformanceDisplayAttribute(nameof(Accuracy), "Accuracy", Accuracy);
            yield return new PerformanceDisplayAttribute(nameof(Flashlight), "Flashlight Bonus", Flashlight);
        }
    }
}
