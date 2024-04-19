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

        [JsonIgnore]
        public IEnumerable<SerialisedDrawableInfo> AllDrawables => DrawableInfo.Values.SelectMany(v => v);

        [JsonProperty]
        public Dictionary<string, SerialisedDrawableInfo[]> DrawableInfo { get; set; } = new Dictionary<string, SerialisedDrawableInfo[]>();

        public bool TryGetDrawableInfo(IRulesetInfo? ruleset, [NotNullWhen(true)] out SerialisedDrawableInfo[]? components) =>
            DrawableInfo.TryGetValue(ruleset?.ShortName ?? global_identifier, out components);

        public void Reset(IRulesetInfo? ruleset) =>
            DrawableInfo.Remove(ruleset?.ShortName ?? global_identifier);

        public void Update(IRulesetInfo? ruleset, SerialisedDrawableInfo[] components) =>
            DrawableInfo[ruleset?.ShortName ?? global_identifier] = components;
    }
}
