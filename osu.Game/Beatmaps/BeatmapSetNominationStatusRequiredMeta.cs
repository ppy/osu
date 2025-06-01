// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Contains information about the number of nominations required for a beatmap set.
    /// </summary>
    public class BeatmapSetNominationRequiredMeta
    {
        /// <summary>
        /// The number of nominations required for difficulties of the main ruleset.
        /// </summary>
        [JsonProperty(@"main_ruleset")]
        public int MainRuleset { get; set; }

        /// <summary>
        /// The number of nominations required for difficulties of each non-main ruleset.
        /// </summary>
        [JsonProperty(@"non_main_ruleset")]
        public int NonMainRuleset { get; set; }
    }
}
