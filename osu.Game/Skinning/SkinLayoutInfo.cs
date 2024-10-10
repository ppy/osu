// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A serialisable model describing layout of a <see cref="SkinnableContainer"/>.
    /// May contain multiple configurations for different rulesets, each of which should manifest their own <see cref="SkinnableContainer"/> as required.
    /// </summary>
    [Serializable]
    public class SkinLayoutInfo
    {
        private const string global_identifier = @"global";

        /// <summary>
        /// Latest version representing the schema of the skin layout.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>0: Initial version of all skin layouts.</description></item>
        /// <item><description>1: Moves existing combo counters from global to per-ruleset HUD targets.</description></item>
        /// </list>
        /// </remarks>
        public const int LATEST_VERSION = 1;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Version = LATEST_VERSION;

        [JsonProperty]
        public Dictionary<string, SerialisedDrawableInfo[]> DrawableInfo { get; set; } = new Dictionary<string, SerialisedDrawableInfo[]>();

        [JsonIgnore]
        public IEnumerable<SerialisedDrawableInfo> AllDrawables => DrawableInfo.Values.SelectMany(v => v);

        public bool TryGetDrawableInfo(RulesetInfo? ruleset, [NotNullWhen(true)] out SerialisedDrawableInfo[]? components) =>
            DrawableInfo.TryGetValue(ruleset?.ShortName ?? global_identifier, out components);

        public void Reset(RulesetInfo? ruleset) =>
            DrawableInfo.Remove(ruleset?.ShortName ?? global_identifier);

        public void Update(RulesetInfo? ruleset, SerialisedDrawableInfo[] components) =>
            DrawableInfo[ruleset?.ShortName ?? global_identifier] = components;
    }
}
