// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public abstract class BeatmapCheck : Check<IBeatmap>
    {
        /// <summary>
        /// Returns zero, one, or several issues detected by this
        /// check on the given beatmap.
        /// </summary>
        /// <param name="beatmap">The beatmap to run the check on.</param>
        /// <returns></returns>
        public abstract override IEnumerable<Issue> Run(IBeatmap beatmap);
    }
}
