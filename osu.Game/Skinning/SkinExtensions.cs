// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Skinning
{
    public static class SkinExtensions
    {
        /// <summary>
        /// Receives a skin and wraps it in a <see cref="SkinTransformer"/> provided by the given ruleset.
        /// </summary>
        /// <param name="skin">The original skin.</param>
        /// <param name="ruleset">The ruleset to use its transformer on.</param>
        /// <param name="beatmap">The beatmap to pass on to the transformer.</param>
        /// <returns></returns>
        public static ISkin? WithRulesetTransformer(this ISkin? skin, Ruleset ruleset, IBeatmap beatmap)
            => skin == null ? null : ruleset.CreateSkinTransformer(skin, beatmap) ?? skin;
    }
}
