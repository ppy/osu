// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    public interface IOsuPerformanceAttributes : IPerformanceAttributes
    {
        public double Aim { get; set; }

        public double Speed { get; set; }

        public double Accuracy { get; set; }

        public double Flashlight { get; set; }

        public double EffectiveMissCount { get; set; }
    }

    public class OsuPerformanceAttributes : PerformanceAttributes, IOsuPerformanceAttributes
    {
        /// <inheritdoc/>
        [JsonProperty("aim")]
        public double Aim { get; set; }

        /// <inheritdoc/>
        [JsonProperty("speed")]
        public double Speed { get; set; }

        /// <inheritdoc/>
        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        /// <inheritdoc/>
        [JsonProperty("flashlight")]
        public double Flashlight { get; set; }

        /// <inheritdoc/>
        [JsonProperty("effective_miss_count")]
        public double EffectiveMissCount { get; set; }

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
