// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Checks;
using osu.Game.Screens.Edit.Verify.Components;

namespace osu.Game.Rulesets.Edit
{
    public abstract class Checker
    {
        // These are all mode-invariant, hence here instead of in e.g. `OsuChecker`.
        private readonly List<BeatmapCheck> beatmapChecks = new List<BeatmapCheck>
        {
            new CheckMetadataVowels()
        };

        public virtual IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            return beatmapChecks.SelectMany(check => check.Run(beatmap));
        }
    }
}
