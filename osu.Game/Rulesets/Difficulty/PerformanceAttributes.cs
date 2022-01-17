// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Rulesets.Difficulty
{
    public class PerformanceAttributes
    {
        /// <summary>
        /// Calculated score performance points.
        /// </summary>
        [JsonProperty("pp")]
        public double Total { get; set; }

        /// <summary>
        /// Return a <see cref="PerformanceDisplayAttribute"/> for each attribute so that a performance breakdown can be displayed.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<PerformanceDisplayAttribute> GetAttributesForDisplay()
        {
            yield return new PerformanceDisplayAttribute("Total", Total);
        }
    }
}
