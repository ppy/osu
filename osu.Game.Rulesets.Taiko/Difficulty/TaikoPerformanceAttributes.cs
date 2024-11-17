// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public interface ITaikoPerformanceAttributes : IPerformanceAttributes
    {
        public double Difficulty { get; set; }

        public double Accuracy { get; set; }

        public double EffectiveMissCount { get; set; }

        public double? EstimatedUnstableRate { get; set; }
    }

    public class TaikoPerformanceAttributes : PerformanceAttributes, ITaikoPerformanceAttributes
    {
        /// <inheritdoc/>
        [JsonProperty("difficulty")]
        public double Difficulty { get; set; }

        /// <inheritdoc/>
        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        /// <inheritdoc/>
        [JsonProperty("effective_miss_count")]
        public double EffectiveMissCount { get; set; }

        /// <inheritdoc/>
        [JsonProperty("estimated_unstable_rate")]
        public double? EstimatedUnstableRate { get; set; }

        public override IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            foreach (var attribute in base.GetAttributesForDisplay())
                yield return attribute;

            yield return new PerformanceDisplayAttribute(nameof(Difficulty), "Difficulty", Difficulty);
            yield return new PerformanceDisplayAttribute(nameof(Accuracy), "Accuracy", Accuracy);
        }
    }
}
