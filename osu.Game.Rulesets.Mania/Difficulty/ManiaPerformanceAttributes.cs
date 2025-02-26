// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public struct ManiaPerformanceAttributes : IPerformanceAttributes
    {
        /// <summary>
        /// Calculated score performance points.
        /// </summary>
        [JsonProperty("pp")]
        public double Total { get; set; }

        [JsonProperty("difficulty")]
        public double Difficulty { get; set; }

        public IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            yield return new PerformanceDisplayAttribute(nameof(Total), "Achieved PP", Total);
            yield return new PerformanceDisplayAttribute(nameof(Difficulty), "Difficulty", Difficulty);
        }
    }
}
