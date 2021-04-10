// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit
{
    public abstract class Checker
    {
        // These are all ruleset-invariant, hence here instead of in e.g. `OsuChecker`.
        private readonly IReadOnlyList<Check> checks = new List<Check>
        {
            new CheckBackground()
        };

        public virtual IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            return checks.SelectMany(check => check.Run(beatmap));
        }
    }
}
