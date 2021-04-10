// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Osu.Edit.Checks;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuChecker : Checker
    {
        public readonly List<Check> beatmapChecks = new List<Check>
        {
            new CheckOffscreenObjects()
        };

        public override IEnumerable<Issue> Run(IBeatmap beatmap)
        {
            // Also run mode-invariant checks.
            foreach (var issue in base.Run(beatmap))
                yield return issue;

            foreach (var issue in beatmapChecks.SelectMany(check => check.Run(beatmap)))
                yield return issue;
        }
    }
}
