// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Rulesets.Difficulty
{
    public interface IPerformanceAttributes
    {
        /// <summary>
        /// Calculated score performance points.
        /// </summary>
        [JsonProperty("pp")]
        public double Total { get; set; }

        /// <summary>
        /// Return a <see cref="PerformanceDisplayAttribute"/> for each attribute so that a performance breakdown can be displayed.
        /// Some attributes may be omitted if they are not meant for display.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay();
    }

    /// <summary>
    /// Represents a full, minimal implementation of <see cref="IPerformanceAttributes"/>.
    /// </summary>
    public class PerformanceAttributes : IPerformanceAttributes
    {
        public double Total { get; set; }

        public IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            yield return new PerformanceDisplayAttribute(nameof(Total), "Achieved PP", Total);
        }
    }
}
